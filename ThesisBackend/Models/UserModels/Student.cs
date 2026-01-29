namespace ThesisBackend.Models.UserModels
{
    public class Student : User
    {
        public string Institution { get; set; } = string.Empty;

        public string Grade { get; set; } = string.Empty;
    }
}
