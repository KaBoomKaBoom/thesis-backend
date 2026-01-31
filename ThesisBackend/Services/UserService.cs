using BCrypt.Net;
using ThesisBackend.Data;
using ThesisBackend.DTOs.UserDTOs;
using ThesisBackend.Models.UserModels;
using ThesisBackend.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ThesisBackend.Services
{
    public class UserService(UserContext userContext, ILogger<UserService> logger, JwtHelper jwtGenerator)
    {
        public readonly UserContext _userContext = userContext;
        public readonly JwtHelper _jwtGenerator = jwtGenerator;
        public readonly ILogger<UserService> _logger = logger;

        public async Task<(User user, string token)> CreateUser(UserToRegisterDTO user)
        {
            _logger.LogInformation("Creating user with email: {Email} and role: {Role}", user.Email, user.Role);
            if (_userContext?.Users == null)
            {
                _logger.LogError("UserContext or Users DbSet is null");
                throw new InvalidOperationException("UserContext or Users DbSet is null");
            }

            var userExists = _userContext.Users.Any(u => u.Email == user.Email);
            if (userExists)
            {
                _logger.LogWarning("User with email: {Email} already exists", user.Email);
                throw new InvalidOperationException("User with this email already exists");
            }

            var newUser = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Role = user.Role
            };

            _userContext.Users.Add(newUser);
            try
            {
                await _userContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving new user with email: {Email}", user.Email);
                throw;
            }

            _logger.LogInformation("User with email: {Email} created successfully. ID: {ID}", newUser.Email, newUser.UserId);

            // Generate JWT token
            var token = _jwtGenerator.GenerateToken(newUser.UserId, newUser.Email, newUser.Role);

            _logger.LogInformation("JWT token generated for user: {Email}", newUser.Email);

            return (newUser, token);
        }


        public async Task<string> LogUser(UserToLoginDTO user)
        {
            _logger.LogInformation("User with email {Email} requested to log iin into the system", user.Email);

            if (_userContext?.Users == null)
            {
                _logger.LogError("UserContext or Users DbSet is null");
                throw new InvalidOperationException("UserContext or Users DbSet is null");
            }

            var dbUser = await _userContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (dbUser == null)
            {
                _logger.LogWarning("User with email: {Email} does not exist", user.Email);
                throw new InvalidOperationException("User with this email does not exist");
            }

            var passwordMatches = BCrypt.Net.BCrypt.Verify(user.Password, dbUser.Password);
            if (!passwordMatches)
            {
                _logger.LogWarning("Invalid password for user: {Email}", user.Email);
                throw new InvalidOperationException("Invalid password");
            }

            var token = _jwtGenerator.GenerateToken(dbUser.UserId, dbUser.Email, dbUser.Role);

            _logger.LogInformation("JWT token generated for user: {Email}", dbUser.Email);

            return token;
        }

        public async Task<string> RefreshToken(string expiredToken)
        {
            _logger.LogInformation("Token refresh requested");

            // Validate token without checking expiration
            var principal = _jwtGenerator.ValidateToken(expiredToken, validateLifetime: false);
            if (principal == null)
            {
                _logger.LogWarning("Invalid token provided for refresh");
                throw new InvalidOperationException("Invalid token");
            }

            // Extract user ID from token claims
            var userIdClaim = principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Invalid user ID in token");
                throw new InvalidOperationException("Invalid token claims");
            }

            // Verify user still exists in database
            if (_userContext?.Users == null)
            {
                _logger.LogError("UserContext or Users DbSet is null");
                throw new InvalidOperationException("UserContext or Users DbSet is null");
            }

            var dbUser = await _userContext.Users.FindAsync(userId);
            if (dbUser == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found", userId);
                throw new InvalidOperationException("User not found");
            }

            // Generate new token
            var newToken = _jwtGenerator.GenerateToken(dbUser.UserId, dbUser.Email, dbUser.Role);

            _logger.LogInformation("New JWT token generated for user: {Email}", dbUser.Email);

            return newToken;
        }
    }
}
