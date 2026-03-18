namespace ThesisBackend.DTOs.TestSessionDTOs
{
    public class TestResultDTO
    {
        public int SessionId { get; set; }
        public int TestId { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int Skipped { get; set; }
        public float ScorePercentage { get; set; }
        public List<QuestionResultDTO> Results { get; set; } = new List<QuestionResultDTO>();
        public DateTime VerifiedAt { get; set; }
    }

    public class QuestionResultDTO
    {
        public int Position { get; set; }
        public int QuestionId { get; set; }
        public int? SubmittedAnswerId { get; set; }
        public int? CorrectAnswerId { get; set; }
        public bool IsCorrect { get; set; }
    }
}
