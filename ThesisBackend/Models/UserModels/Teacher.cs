namespace ThesisBackend.Models.UserModels
{
    public class Teacher : User
    {
        public string Institution { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
    }
}
