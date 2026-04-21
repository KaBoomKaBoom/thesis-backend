namespace ThesisBackend.DTOs.TestSessionDTOs
{
    public class UserSessionSummaryDTO
    {
        public int SessionId { get; set; }
        public int TestId { get; set; }
        public DateTime TestTakenTime { get; set; }
        public int? CorrectAnswers { get; set; }
        public int? TotalQuestions { get; set; }
        public float? ScorePercentage { get; set; }
        public string? ResultLabel { get; set; }
    }

    public class UserSessionDetailsDTO
    {
        public int SessionId { get; set; }
        public int TestId { get; set; }
        public DateTime TestTakenTime { get; set; }
        public float Score { get; set; }
        public int? TotalQuestions { get; set; }
        public int? CorrectAnswers { get; set; }
        public int? Skipped { get; set; }
        public float? ScorePercentage { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public List<TestComponentDTO> SubmittedAnswers { get; set; } = new List<TestComponentDTO>();
        public List<QuestionResultDTO> Results { get; set; } = new List<QuestionResultDTO>();
    }

    public class TestComponentDTO
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
    }
}
