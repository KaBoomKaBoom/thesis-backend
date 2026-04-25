using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThesisBackend.DTOs.UserDTOs;
using ThesisBackend.Helpers.AuthHelpers;
using ThesisBackend.Services.AuthServices;

namespace ThesisBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthentificationController(UserService userService, OTPService otpService, JwtHelper jwtHelper) : ControllerBase
    {
        private readonly UserService _userService = userService;
        private readonly OTPService _otpService = otpService;
        private readonly JwtHelper _jwtHelper = jwtHelper;

        [AllowAnonymous]
        [HttpGet("api-health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { Status = "API is running" });
        }

        [AllowAnonymous]
        [HttpGet("db-health")]
        public IActionResult DbHealthCheck()
        {
            var dbHealth = _userService.DbHealth().Result;
            if (dbHealth)
            {
                return Ok(new { Status = "Database connection is healthy" });
            }
            else
            {
                return StatusCode(500, new { Status = "Database connection is unhealthy" });
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserToRegisterDTO userToRegister)
        {
            try
            {
                // Check if user already exists
                var userExists = await _userService.CheckUserExists(userToRegister.Email);
                if (userExists)
                {
                    return BadRequest(new { Message = "User with this email already exists" });
                }

                // Send OTP and save registration data temporarily
                var otpSent = await _otpService.SendRegistrationOTP(userToRegister.Email, userToRegister);
                
                if (!otpSent)
                {
                    return StatusCode(500, new { Message = "Failed to send verification email" });
                }

                return Ok(new 
                { 
                    Message = "Verification code sent to your email. Please verify to complete registration.",
                    Email = userToRegister.Email 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPDTO verifyOTP)
        {
            try
            {
                // Validate OTP and retrieve registration data
                var registrationData = await _otpService.ValidateRegistrationOTP(verifyOTP.Email, verifyOTP.OTP);
                
                if (registrationData == null)
                {
                    return BadRequest(new { Message = "Invalid or expired verification code" });
                }

                // Create user after successful OTP verification
                var (user, token) = await _userService.CreateUser(registrationData);
                
                return Ok(new 
                { 
                    Message = "Registration completed successfully",
                    User = user, 
                    Token = token 
                });
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
                return Ok(new { Token = token });
            }
            catch (InvalidOperationException)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var newToken = await _userService.RefreshToken(request.Token);
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

        [AllowAnonymous]
        [HttpPost("generate-role-token")]
        public IActionResult GenerateRoleToken([FromBody] GenerateRoleTokenRequest request)
        {
            try
            {
                var allowedRoles = new[] { "student", "teacher", "parent", "admin" };
                var normalizedRole = request.Role.Trim().ToLowerInvariant();

                if (!allowedRoles.Contains(normalizedRole))
                {
                    return BadRequest(new { Message = "Invalid role. Allowed roles: student, teacher, parent, admin" });
                }

                if (request.UserId <= 0)
                {
                    return BadRequest(new { Message = "UserId must be greater than 0" });
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { Message = "Email is required" });
                }

                var token = _jwtHelper.GenerateToken(request.UserId, request.Email, normalizedRole);
                return Ok(new { Token = token, Role = normalizedRole });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while generating token.", Details = ex.Message });
            }
        }
    }

    public class RefreshTokenRequest
    {
        public required string Token { get; set; }
    }

    public class GenerateRoleTokenRequest
    {
        public int UserId { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}
