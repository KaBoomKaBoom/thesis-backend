using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using ThesisBackend.DTOs.UserDTOs;
using ThesisBackend.Helpers;

namespace ThesisBackend.Services
{
    public class OTPService(OTPGenerator otpGenerator, EmailService emailService, IDistributedCache cache, ILogger<OTPService> logger)
    {
        private readonly OTPGenerator _otpGenerator = otpGenerator;
        private readonly EmailService _emailService = emailService;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<OTPService> _logger = logger;

        public async Task<bool> SendRegistrationOTP(string recipient, UserToRegisterDTO registrationData)
        {
            var otp = _otpGenerator.GenerateOTP();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            try
            {
                // Save OTP
                var otpCacheKey = $"otp:{recipient}";
                await _cache.SetStringAsync(otpCacheKey, otp, cacheOptions);

                // Save registration data
                var registrationCacheKey = $"registration:{recipient}";
                var serializedData = JsonSerializer.Serialize(registrationData);
                await _cache.SetStringAsync(registrationCacheKey, serializedData, cacheOptions);

                _logger.LogInformation("OTP and registration data saved to cache for email: {Email}", recipient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save OTP and registration data to cache for email: {Email}", recipient);
                return false;
            }

            try
            {
                var emailBody = $@"
                    <h2>Welcome to Our Service!</h2>
                    <p>Thank you for registering. Please use the following verification code to complete your registration:</p>
                    <h1 style='color: #4CAF50; letter-spacing: 5px;'>{otp}</h1>
                    <p>This code will expire in 5 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                ";

                await _emailService.SendEmailAsync(recipient, "Verify Your Email - Registration", emailBody);
                _logger.LogInformation("Registration OTP email sent to: {Email}", recipient);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to: {Email}", recipient);
                return false;
            }
        }

        public async Task<bool> SendOTP(string recipient, string subject = "One Time Password")
        {
            var otp = _otpGenerator.GenerateOTP();

            var cacheKey = $"otp:{recipient}";
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            try
            {
                await _cache.SetStringAsync(cacheKey, otp, cacheOptions);
                _logger.LogInformation("OTP saved to cache for email: {Email}", recipient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save OTP to cache for email: {Email}", recipient);
                return false;
            }

            try
            {
                var emailBody = $@"
                    <h2>Verification Code</h2>
                    <p>Your verification code is:</p>
                    <h1 style='color: #4CAF50; letter-spacing: 5px;'>{otp}</h1>
                    <p>This code will expire in 5 minutes.</p>
                ";

                await _emailService.SendEmailAsync(recipient, subject, emailBody);
                _logger.LogInformation("OTP email sent to: {Email}", recipient);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to: {Email}", recipient);
                return false;
            }
        }

        public async Task<UserToRegisterDTO?> ValidateRegistrationOTP(string email, string otp)
        {
            var otpCacheKey = $"otp:{email}";
            var registrationCacheKey = $"registration:{email}";

            try
            {
                var cachedOTP = await _cache.GetStringAsync(otpCacheKey);

                if (string.IsNullOrEmpty(cachedOTP))
                {
                    _logger.LogWarning("OTP not found or expired for email: {Email}", email);
                    return null;
                }

                if (cachedOTP != otp)
                {
                    _logger.LogWarning("Invalid OTP provided for email: {Email}", email);
                    return null;
                }

                // Retrieve registration data
                var cachedRegistrationData = await _cache.GetStringAsync(registrationCacheKey);
                if (string.IsNullOrEmpty(cachedRegistrationData))
                {
                    _logger.LogWarning("Registration data not found for email: {Email}", email);
                    return null;
                }

                var registrationData = JsonSerializer.Deserialize<UserToRegisterDTO>(cachedRegistrationData);

                // Remove OTP and registration data after successful validation
                await _cache.RemoveAsync(otpCacheKey);
                await _cache.RemoveAsync(registrationCacheKey);

                _logger.LogInformation("OTP validated successfully for email: {Email}", email);
                return registrationData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OTP for email: {Email}", email);
                return null;
            }
        }

        public async Task<bool> ValidateOTP(string email, string otp)
        {
            var cacheKey = $"otp:{email}";

            try
            {
                var cachedOTP = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cachedOTP))
                {
                    _logger.LogWarning("OTP not found or expired for email: {Email}", email);
                    return false;
                }

                if (cachedOTP == otp)
                {
                    await _cache.RemoveAsync(cacheKey);
                    _logger.LogInformation("OTP validated successfully for email: {Email}", email);
                    return true;
                }

                _logger.LogWarning("Invalid OTP provided for email: {Email}", email);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OTP for email: {Email}", email);
                return false;
            }
        }
    }
}
