using ThesisBackend.Models.TestSessionModels;

namespace ThesisBackend.DTOs.TestSessionDTOs
{
    public class TestSessionToSave
    {
        public int TestId { get; set; }
        public List<TestComponentToSave> TestComponents { get; set; } = new List<TestComponentToSave>();
    }

    public class TestComponentToSave
    {
        public int answer_id { get; set; }
        public int question_id { get; set; }
    }
}

