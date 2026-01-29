namespace ThesisBackend.Models
{
    public class Student : User
    {
        public required string Institution { get; set; }

        //
        public required string Grade { get; set; }
    }
}
