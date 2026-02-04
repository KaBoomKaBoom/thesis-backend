namespace ThesisBackend.DTOs.UserDTOs
{
    public class StudentToUpdateDTO : UserToUpdateDTO
    {
        public string InstitutionId { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
    }
}
