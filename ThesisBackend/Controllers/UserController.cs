using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThesisBackend.DTOs.TestSessionDTOs;
using ThesisBackend.DTOs.UserDTOs;
using ThesisBackend.Models.UserModels;
using ThesisBackend.Services.AuthServices;
using ThesisBackend.Services.TestSessionServices;

namespace ThesisBackend.Controllers
{
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserController(UserService userService, TestSessionService testSessionService, ILogger<UserController> logger) : ControllerBase
    {
        private readonly UserService _userService = userService;
        private readonly TestSessionService _testSessionService = testSessionService;
        private readonly ILogger<UserController> _logger = logger;

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                // Extract user ID from JWT token
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("Failed to extract user ID from token");
                    return Unauthorized(new { Message = "Invalid token" });
                }

                var user = await _userService.GetUserById(userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID: {UserId} not found", userId);
                    return NotFound(new { Message = "User not found" });
                }

                var profile = new UserProfileDTO
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    // Temporary mock data for fields not yet in database
                    Phone = user.PhoneNumber,
                    Location = user.Location,
                    DateOfBirth = "2008-05-15",
                    Bio = user.Biography
                };

                // Add role-specific data
                if (user is Student student)
                {
                    profile.GradeLevel = student.Grade;
                    profile.School = student.Institution;
                }
                else if (user is Teacher teacher)
                {
                    profile.School = teacher.Institution;
                }

                _logger.LogInformation("Profile retrieved for user: {UserId}", userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, new { Message = "An error occurred while retrieving your profile" });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserToUpdateDTO updateDTO)
        {
            try
            {
                // Extract user ID from JWT token
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("Failed to extract user ID from token");
                    return Unauthorized(new { Message = "Invalid token" });
                }

                var updatedUser = await _userService.UpdateUser(userId, updateDTO);

                var profile = new UserProfileDTO
                {
                    FirstName = updatedUser.FirstName,
                    LastName = updatedUser.LastName,
                    Email = updatedUser.Email,
                    Role = updatedUser.Role,
                    Phone = updatedUser.PhoneNumber,
                    Location = updatedUser.Location,
                    DateOfBirth = "2008-05-15",
                    Bio = updatedUser.Biography
                };

                // Add role-specific data
                if (updatedUser is Student student)
                {
                    profile.GradeLevel = student.Grade;
                    profile.School = student.Institution;
                }
                else if (updatedUser is Teacher teacher)
                {
                    profile.School = teacher.Institution;
                }

                _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
                return Ok(new 
                { 
                    Message = "Profile updated successfully",
                    Profile = profile 
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Update failed for user");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { Message = "An error occurred while updating your profile" });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetUserDashboard()
        {
            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("Failed to extract user ID from token");
                    return Unauthorized(new { Message = "Invalid token" });
                }

                UserDashboardDTO dashboard = await _testSessionService.GetUserDashboard(userId);
                _logger.LogInformation("Dashboard retrieved for user: {UserId}", userId);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user dashboard");
                return StatusCode(500, new { Message = "An error occurred while retrieving your dashboard" });
            }
        }
    }
}   