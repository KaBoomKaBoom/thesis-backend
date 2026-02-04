namespace ThesisBackend.DTOs.UserDTOs
{
    public class UserToUpdateDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;  
        public string School { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
    }
}
