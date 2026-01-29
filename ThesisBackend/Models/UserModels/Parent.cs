namespace ThesisBackend.Models.UserModels
{
    public class Parent : User
    {
        public List<string> ChildrenIds { get; set; } = new List<string>();
    }
}
