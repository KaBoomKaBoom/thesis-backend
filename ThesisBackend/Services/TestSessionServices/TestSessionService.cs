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

        public async Task<List<UserSessionSummaryDTO>> GetUserSessionSummaries(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching session summaries for user: {UserId}", userId);

                var sessions = await _testSessionContext.TestSessions
                    .AsNoTrackingWithIdentityResolution()
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.TestTakenTime)
                    .ToListAsync();

                var sessionIds = sessions.Select(s => s.SessionId).ToList();

                var latestResultsBySessionId = sessionIds.Count == 0
                    ? new Dictionary<int, TestResult>()
                    : (await _testSessionContext.TestResults
                        .AsNoTrackingWithIdentityResolution()
                        .Where(r => sessionIds.Contains(r.SessionId))
                        .ToListAsync())
                        .GroupBy(r => r.SessionId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.OrderByDescending(r => r.VerifiedAt).ThenByDescending(r => r.ResultId).First());

                var sessionSummaries = sessions.Select(session =>
                {
                    latestResultsBySessionId.TryGetValue(session.SessionId, out var result);

                    return new UserSessionSummaryDTO
                    {
                        SessionId = session.SessionId,
                        TestId = session.TestId,
                        TestTakenTime = session.TestTakenTime,
                        CorrectAnswers = result?.CorrectAnswers,
                        TotalQuestions = result?.TotalQuestions,
                        ScorePercentage = result?.ScorePercentage,
                        ResultLabel = result == null ? null : $"{result.CorrectAnswers}/{result.TotalQuestions}"
                    };
                }).ToList();

                return sessionSummaries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch session summaries for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<UserSessionDetailsDTO?> GetUserSessionDetails(int userId, int sessionId)
        {
            try
            {
                _logger.LogInformation("Fetching session details for user: {UserId}, session: {SessionId}", userId, sessionId);

                var session = await _testSessionContext.TestSessions
                    .AsNoTrackingWithIdentityResolution()
                    .FirstOrDefaultAsync(ts => ts.SessionId == sessionId && ts.UserId == userId);

                if (session == null)
                {
                    _logger.LogWarning("Session not found or does not belong to user. User: {UserId}, Session: {SessionId}", userId, sessionId);
                    return null;
                }

                var result = await _testSessionContext.TestResults
                    .AsNoTrackingWithIdentityResolution()
                    .Where(tr => tr.SessionId == sessionId)
                    .OrderByDescending(tr => tr.VerifiedAt)
                    .ThenByDescending(tr => tr.ResultId)
                    .FirstOrDefaultAsync();

                return new UserSessionDetailsDTO
                {
                    SessionId = session.SessionId,
                    TestId = session.TestId,
                    TestTakenTime = session.TestTakenTime,
                    Score = session.Score,
                    TotalQuestions = result?.TotalQuestions,
                    CorrectAnswers = result?.CorrectAnswers,
                    Skipped = result?.Skipped,
                    ScorePercentage = result?.ScorePercentage,
                    VerifiedAt = result?.VerifiedAt,
                    SubmittedAnswers = session.TestComponents
                        .Select(tc => new TestComponentDTO
                        {
                            QuestionId = tc.question_id,
                            AnswerId = tc.answer_id
                        })
                        .ToList(),
                    Results = result?.DetailedResults
                        .OrderBy(r => r.Position)
                        .Select(r => new QuestionResultDTO
                        {
                            Position = r.Position,
                            QuestionId = r.QuestionId,
                            SubmittedAnswerId = r.SubmittedAnswerId,
                            CorrectAnswerId = r.CorrectAnswerId,
                            IsCorrect = r.IsCorrect
                        })
                        .ToList() ?? new List<QuestionResultDTO>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch session details for user: {UserId}, session: {SessionId}", userId, sessionId);
                throw;
            }
        }

        public async Task<UserDashboardDTO> GetUserDashboard(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching dashboard data for user: {UserId}", userId);

                var sessions = await _testSessionContext.TestSessions
                    .AsNoTrackingWithIdentityResolution()
                    .Where(ts => ts.UserId == userId)
                    .OrderByDescending(ts => ts.TestTakenTime)
                    .ToListAsync();

                var sessionIds = sessions.Select(s => s.SessionId).ToList();

                var results = sessionIds.Count == 0
                    ? new List<TestResult>()
                    : await _testSessionContext.TestResults
                        .AsNoTrackingWithIdentityResolution()
                        .Where(tr => sessionIds.Contains(tr.SessionId))
                        .ToListAsync();

                var latestResults = results
                    .GroupBy(r => r.SessionId)
                    .Select(g => g.OrderByDescending(r => r.VerifiedAt).ThenByDescending(r => r.ResultId).First())
                    .ToList();

                var resultsBySessionId = latestResults.ToDictionary(r => r.SessionId, r => r);

                var lastSessionWithResult = sessions
                    .Select(s => resultsBySessionId.TryGetValue(s.SessionId, out var result) ? result : null)
                    .FirstOrDefault(r => r != null);

                var stats = new DashboardStatsDTO
                {
                    TotalSessions = sessions.Count,
                    CompletedSessions = latestResults.Count,
                    AverageScorePercentage = latestResults.Count == 0
                        ? 0
                        : (float)Math.Round(latestResults.Average(r => r.ScorePercentage), 2),
                    BestScorePercentage = latestResults.Count == 0
                        ? 0
                        : latestResults.Max(r => r.ScorePercentage),
                    LastSessionResultLabel = lastSessionWithResult == null
                        ? null
                        : $"{lastSessionWithResult.CorrectAnswers}/{lastSessionWithResult.TotalQuestions}",
                    LastSessionTakenAt = sessions.FirstOrDefault()?.TestTakenTime
                };

                var recentSessions = sessions
                    .Take(10)
                    .Select(s =>
                    {
                        resultsBySessionId.TryGetValue(s.SessionId, out var result);
                        return new DashboardRecentSessionDTO
                        {
                            SessionId = s.SessionId,
                            TestId = s.TestId,
                            TestTakenTime = s.TestTakenTime,
                            CorrectAnswers = result?.CorrectAnswers,
                            TotalQuestions = result?.TotalQuestions,
                            ScorePercentage = result?.ScorePercentage,
                            ResultLabel = result == null ? null : $"{result.CorrectAnswers}/{result.TotalQuestions}"
                        };
                    })
                    .ToList();

                var scoreTrend = sessions
                    .Where(s => resultsBySessionId.ContainsKey(s.SessionId))
                    .GroupBy(s => s.TestTakenTime.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new DashboardTrendPointDTO
                    {
                        Date = g.Key,
                        SessionsCount = g.Count(),
                        AverageScorePercentage = (float)Math.Round(g.Average(s => resultsBySessionId[s.SessionId].ScorePercentage), 2)
                    })
                    .ToList();

                var questionAnalytics = latestResults
                    .SelectMany(r => r.DetailedResults)
                    .GroupBy(dr => dr.QuestionId)
                    .Select(g => new DashboardQuestionAccuracyDTO
                    {
                        QuestionId = g.Key,
                        Attempts = g.Count(),
                        CorrectAnswers = g.Count(x => x.IsCorrect),
                        AccuracyPercentage = (float)Math.Round((double)g.Count(x => x.IsCorrect) * 100 / g.Count(), 2)
                    })
                    .ToList();

                var topicAnalytics = new DashboardTopicAnalyticsDTO
                {
                    StrongestQuestions = questionAnalytics
                        .OrderByDescending(q => q.AccuracyPercentage)
                        .ThenByDescending(q => q.Attempts)
                        .Take(5)
                        .ToList(),
                    WeakestQuestions = questionAnalytics
                        .OrderBy(q => q.AccuracyPercentage)
                        .ThenByDescending(q => q.Attempts)
                        .Take(5)
                        .ToList()
                };

                return new UserDashboardDTO
                {
                    Stats = stats,
                    RecentSessions = recentSessions,
                    ScoreTrend = scoreTrend,
                    TopicAnalytics = topicAnalytics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch dashboard data for user: {UserId}", userId);
                throw;
            }
        }
    }
}
