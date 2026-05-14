namespace ThesisBackend.DTOs.UserDTOs
{
    public class UserProfileDTO
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? DateOfBirth { get; set; }
        public required string Role { get; set; }
        public string? GradeLevel { get; set; }
        public string? School { get; set; }
        public string? Bio { get; set; }
    }
}