using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThesisBackend.DTOs.UserDTOs;
using ThesisBackend.Models.UserModels;
using ThesisBackend.Services.AuthServices;

namespace ThesisBackend.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    public class AdminController(UserService userService, ILogger<AdminController> logger) : ControllerBase
    {
        private readonly UserService _userService = userService;
        private readonly ILogger<AdminController> _logger = logger;

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                return Ok(users.Select(MapUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch all users");
                return StatusCode(500, new { Message = "An error occurred while fetching users" });
            }
        }

        [HttpGet("users/{userId:int}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var user = await _userService.GetUserById(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                return Ok(MapUser(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user with ID: {UserId}", userId);
                return StatusCode(500, new { Message = "An error occurred while fetching the user" });
            }
        }

        [HttpGet("users/role/{role}")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            try
            {
                var users = await _userService.GetUsersByRole(role);
                return Ok(users.Select(MapUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch users by role: {Role}", role);
                return StatusCode(500, new { Message = "An error occurred while fetching users by role" });
            }
        }

        [HttpPost("users")]
        public async Task<IActionResult> AddUser([FromBody] UserToRegisterDTO userToRegister)
        {
            try
            {
                var (user, _) = await _userService.CreateUser(userToRegister);
                return Ok(new
                {
                    Message = "User created successfully",
                    User = MapUser(user)
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user");
                return StatusCode(500, new { Message = "An error occurred while creating the user" });
            }
        }

        [HttpPut("users/{userId:int}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserToUpdateDTO updateDTO)
        {
            try
            {
                var updatedUser = await _userService.UpdateUser(userId, updateDTO);
                return Ok(new
                {
                    Message = "User updated successfully",
                    User = MapUser(updatedUser)
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user with ID: {UserId}", userId);
                return StatusCode(500, new { Message = "An error occurred while updating the user" });
            }
        }

        [HttpDelete("users/{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var deleted = await _userService.DeleteUser(userId);
                if (!deleted)
                {
                    return NotFound(new { Message = "User not found" });
                }

                return Ok(new { Message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user with ID: {UserId}", userId);
                return StatusCode(500, new { Message = "An error occurred while deleting the user" });
            }
        }

        private static object MapUser(User user)
        {
            return new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.PhoneNumber,
                user.Location,
                Biography = user.Biography,
                Grade = user is Student student ? student.Grade : null,
                School = user is Student s ? s.Institution : user is Teacher t ? t.Institution : null,
                Course = user is Teacher teacher ? teacher.Course : null,
                TeacherId = user is Student st ? st.TeacherId : null,
                ChildrenIds = user is Parent parent ? parent.ChildrenIds : null
            };
        }

    }
}
