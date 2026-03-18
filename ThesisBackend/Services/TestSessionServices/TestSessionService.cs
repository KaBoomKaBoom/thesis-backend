using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ThesisBackend.Data;
using ThesisBackend.DTOs.TestSessionDTOs;
using ThesisBackend.Models.TestSessionModels;

namespace ThesisBackend.Services.TestSessionServices
{
    public class TestSessionService(TestSessionContext testSessionContext, ILogger<TestSessionService> logger, IHttpClientFactory httpClientFactory)
    {

        public readonly TestSessionContext _testSessionContext = testSessionContext;
        public readonly ILogger<TestSessionService> _logger = logger;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<TestResult> CreateTestSession(int userId, TestSessionToSave testSessionToSave)
        {
            try
            {
                var testSession = new Models.TestSessionModels.TestSession
                {
                    UserId = userId,
                    TestId = testSessionToSave.TestId,
                    TestComponents = testSessionToSave.TestComponents
                        .Select(tc => new TestComponent
                        {
                            answer_id = tc.answer_id,
                            question_id = tc.question_id
                        })
                        .ToList(),
                    TestTakenTime = DateTime.UtcNow
                };
                _testSessionContext.TestSessions.Add(testSession);
                await _testSessionContext.SaveChangesAsync();
                _logger.LogInformation("Test session created for User ID: {UserId}, Test ID: {TestId}, Session ID: {SessionId}", 
                    userId, testSessionToSave.TestId, testSession.SessionId);

                // Verify the test and get results
                var testResult = await VerifyTestSession(testSession.SessionId, testSessionToSave);
                
                // Update the score in the test session
                testSession.Score = testResult.ScorePercentage;
                await _testSessionContext.SaveChangesAsync();

                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create test session for User ID: {UserId}, Test ID: {TestId}", userId, testSessionToSave.TestId);
                throw;
            }
        }

        public async Task<TestResult> VerifyTestSession(int sessionId, TestSessionToSave testSessionToSave)
        {
            try
            {
                _logger.LogInformation("Verifying test session: {SessionId}", sessionId);

                // Prepare verification request
                var verificationRequest = new TestVerificationRequestDTO
                {
                    test_id = testSessionToSave.TestId,
                    answers = testSessionToSave.TestComponents.Select(tc => new AnswerSubmission
                    {
                        answer_id = tc.answer_id == 0 ? null : tc.answer_id,
                        question_id = tc.question_id
                    }).ToList()
                };

                // Get the verification service URL from environment variables
                var verificationServiceUrl = Environment.GetEnvironmentVariable("TEST_VERIFICATION_SERVICE_URL") 
                    ?? "http://localhost:8070";

                // Create HTTP client
                var httpClient = _httpClientFactory.CreateClient();
                var requestJson = JsonSerializer.Serialize(verificationRequest);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                // Send request to verification microservice
                var response = await httpClient.PostAsync($"{verificationServiceUrl}/test/verify", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Test verification failed with status code: {StatusCode}, Content: {Content}", 
                        response.StatusCode, errorContent);
                    throw new InvalidOperationException($"Test verification service returned error: {response.StatusCode}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var verificationResponse = JsonSerializer.Deserialize<TestVerificationResponseDTO>(responseJson);

                if (verificationResponse == null)
                {
                    throw new InvalidOperationException("Failed to deserialize verification response");
                }

                // Create and save test result
                var testResult = new TestResult
                {
                    SessionId = sessionId,
                    TotalQuestions = verificationResponse.total_questions,
                    CorrectAnswers = verificationResponse.correct_answers,
                    Skipped = verificationResponse.skipped,
                    ScorePercentage = verificationResponse.score_percentage,
                    DetailedResults = verificationResponse.results.Select(r => new DetailedQuestionResult
                    {
                        Position = r.position,
                        QuestionId = r.question_id,
                        SubmittedAnswerId = r.submitted_answer_id,
                        CorrectAnswerId = r.correct_answer_id,
                        IsCorrect = r.is_correct
                    }).ToList(),
                    VerifiedAt = DateTime.UtcNow
                };

                _testSessionContext.TestResults.Add(testResult);
                await _testSessionContext.SaveChangesAsync();

                _logger.LogInformation("Test session {SessionId} verified successfully. Score: {Score}%", 
                    sessionId, verificationResponse.score_percentage);

                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify test session: {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<TestResult?> GetTestResultBySessionId(int sessionId)
        {
            try
            {
                _logger.LogInformation("Fetching test result for session: {SessionId}", sessionId);
                var testResult = await _testSessionContext.TestResults
                    .FirstOrDefaultAsync(tr => tr.SessionId == sessionId);
                
                if (testResult == null)
                {
                    _logger.LogWarning("Test result not found for session: {SessionId}", sessionId);
                }

                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch test result for session: {SessionId}", sessionId);
                throw;
            }
        }
    }
}
