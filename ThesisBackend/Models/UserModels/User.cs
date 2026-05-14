namespace ThesisBackend.Models.UserModels
{
    public class User
    {
        public int UserId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }    
        public required string Email { get; set; }
        public required string Password { get; set; }

        // student/teacher/parent/admin
        public required string Role { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
    }
}
