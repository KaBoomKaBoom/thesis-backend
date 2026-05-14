namespace ThesisBackend.DTOs.UserDTOs
{
    public class UserToLoginDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
