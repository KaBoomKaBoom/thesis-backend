namespace ThesisBackend.Models.TestSessionModels
{
    public class TestSession
    {
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public int TestId { get; set; }
        public List<TestComponent> TestComponents { get; set; } = new List<TestComponent>();
        public DateTime TestTakenTime { get; set; }
        public float Score { get; set; }
    }

    public class TestComponent
    {
        public int Id { get; set; }
        public int answer_id { get; set; }
        public int question_id { get; set; }
    }
}
