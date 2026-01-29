namespace ThesisBackend.DTOs.UserDTOs
{
    public class ParentToUpdateDTO : UserToUpdateDTO
    {
        public List<string> ChildrenIds { get; set; } = new List<string>();
    }
}
