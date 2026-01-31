using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThesisBackend.DTOs.UserDTOs;
using ThesisBackend.Services;

namespace ThesisBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthentificationController(UserService userService) : ControllerBase
    {
        private readonly UserService _userService = userService;

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserToRegisterDTO userToRegister)
        {
            try
            {
                var (user, token) = await _userService.CreateUser(userToRegister);
                return Ok(new { User = user, Token = token });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserToLoginDTO userToLogin)
        {
            try
            {
                var token = await _userService.LogUser(userToLogin);
                return Ok(token);
            }
            catch (InvalidOperationException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh/{token}")]
        public async Task<IActionResult> RefreshToken(string token)
        {
            try
            {
                var newToken = await _userService.RefreshToken(token);
                return Ok(new { Token = newToken });
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }
    }
}
