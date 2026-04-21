using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThesisBackend.DTOs.TestSessionDTOs;
using ThesisBackend.Services.TestSessionServices;

namespace ThesisBackend.Controllers
{
    [ApiController]
    [Route("api/testSession")]
    public class TestSessionController(TestSessionService testSession) : ControllerBase
    {

        public readonly TestSessionService _testSessionService = testSession;

        [Authorize(Roles = "student,admin")]
        [HttpPost("testSession")]
        public async Task<IActionResult> CreateTestSession([FromBody] TestSessionToSave testSessionToSave)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                {
                    return Unauthorized(new { Message = "Invalid token" });
                }
                var testResult = await _testSessionService.CreateTestSession(userId, testSessionToSave);

                //This goes to the frontend to be used for fetching the test result after the test is completed
                return Ok(testResult.SessionId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the test session", Details = ex.Message });
            }

        }

        [Authorize(Roles = "student,admin")]
        [HttpPost("verifyTest/{sessionId}")]
        public async Task<IActionResult> VerifyTestSession([FromBody] TestSessionToSave testSessionToSave)
        {
            try
            {
                var testResult = await _testSessionService.VerifyTestSession(testSessionToSave.TestId, testSessionToSave);
                return Ok(testResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while verifying the test session", Details = ex.Message });

            }
        }

        [Authorize]
        [HttpGet("activity/sessions")]
        public async Task<IActionResult> GetUserTakenSessions()
        {
            try
            {
                if (!TryGetUserId(out var userId))
                {
                    return Unauthorized(new { Message = "Invalid token" });
                }

                var sessions = await _testSessionService.GetUserSessionSummaries(userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching your sessions", Details = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("activity/sessions/{sessionId:int}")]
        public async Task<IActionResult> GetUserTakenSessionDetails(int sessionId)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                {
                    return Unauthorized(new { Message = "Invalid token" });
                }

                var sessionDetails = await _testSessionService.GetUserSessionDetails(userId, sessionId);

                if (sessionDetails == null)
                {
                    return NotFound(new { Message = "Session not found" });
                }

                return Ok(sessionDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching session details", Details = ex.Message });
            }
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                            ?? User.FindFirst("sub");

            return userIdClaim != null && int.TryParse(userIdClaim.Value, out userId);
        }
    }
}
