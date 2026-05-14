namespace ThesisBackend.DTOs.UserDTOs
{
    public class VerifyOTPDTO
    {
        public required string Email { get; set; }
        public required string OTP { get; set; }
    }
}