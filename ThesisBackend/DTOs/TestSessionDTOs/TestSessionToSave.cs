using ThesisBackend.Models.TestSessionModels;

namespace ThesisBackend.DTOs.TestSessionDTOs
{
    public class TestSessionToSave
    {
        public int TestId { get; set; }
        public List<TestComponent> TestComponents { get; set; } = new List<TestComponent>();
    }
}
