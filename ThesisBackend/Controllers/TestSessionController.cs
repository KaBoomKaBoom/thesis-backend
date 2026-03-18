using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThesisBackend.DTOs.TestSessionDTOs;
using ThesisBackend.Models.TestSessionModels;
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
                // Extract user ID from JWT token
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
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
    }
}
