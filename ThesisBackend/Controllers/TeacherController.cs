using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ThesisBackend.DTOs.UserDTOs;
using ThesisBackend.Models.UserModels;
using ThesisBackend.Services.AuthServices;

namespace ThesisBackend.Controllers
{
    [Route("api/teacher")]
    [ApiController]
    [Authorize]
    public class TeacherController(UserService userService, ILogger<TeacherController> logger) : ControllerBase
    {
        private readonly UserService _userService = userService;
        private readonly ILogger<TeacherController> _logger = logger;

        [Authorize(Roles = "teacher")]
        [HttpGet("students")]
        public async Task<IActionResult> SearchStudents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] string? grade = null,
            [FromQuery] string? school = null,
            [FromQuery] string? location = null)
        {
            try
            {
                if (pageNumber <= 0)
                {
                    return BadRequest(new { Message = "Page number must be greater than 0" });
                }

                var allowedPageSizes = new[] { 5, 10, 20, 50 };
                if (!allowedPageSizes.Contains(pageSize))
                {
                    return BadRequest(new { Message = "Page size must be one of: 5, 10, 20, 50" });
                }

                var students = await _userService.SearchStudents(pageNumber, pageSize, name, grade, school, location);
                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search students");
                return StatusCode(500, new { Message = "An error occurred while searching students" });
            }
        }

        [Authorize(Roles = "teacher")]
        [HttpPut("students/{studentId:int}/assign-me")]
        public async Task<IActionResult> AssignMyselfToStudent(int studentId)
        {
            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int teacherId))
                {
                    return Unauthorized(new { Message = "Invalid token" });
                }

                var student = await _userService.AssignTeacherToStudent(teacherId, studentId);

                if (student == null)
                {
                    return NotFound(new { Message = "Student not found" });
                }

                return Ok(new
                {
                    Message = "Teacher assigned successfully",
                    Student = new StudentSearchResultDTO
                    {
                        StudentId = student.UserId,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        Email = student.Email,
                        Grade = student.Grade,
                        School = student.Institution,
                        Location = student.Location,
                        TeacherId = student.TeacherId
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Teacher assignment failed");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign teacher to student");
                return StatusCode(500, new { Message = "An error occurred while assigning teacher to student" });
            }
        }
    }
}
