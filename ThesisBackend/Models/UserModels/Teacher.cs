namespace ThesisBackend.Models.UserModels
{
    public class Teacher : User
    {
        public required string Institution { get; set; }
        public required string Course { get; set; }
    }
}
