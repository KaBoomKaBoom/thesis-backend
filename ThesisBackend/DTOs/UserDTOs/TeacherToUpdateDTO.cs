namespace ThesisBackend.DTOs.UserDTOs
{
    public class TeacherToUpdateDTO : UserToUpdateDTO
    {
        public string Institution { get; set; } = string.Empty;
        public string Course {  get; set; } = string.Empty;
    }
}
