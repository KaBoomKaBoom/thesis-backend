namespace ThesisBackend.Models.TestSessionModels
{
    public class TestResult
    {
        public int ResultId { get; set; }
        public int SessionId { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int Skipped { get; set; }
        public float ScorePercentage { get; set; }
        public List<DetailedQuestionResult> DetailedResults { get; set; } = new List<DetailedQuestionResult>();
        public DateTime VerifiedAt { get; set; }
    }

    public class DetailedQuestionResult
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public int QuestionId { get; set; }
        public int? SubmittedAnswerId { get; set; }
        public int? CorrectAnswerId { get; set; }
        public bool IsCorrect { get; set; }
    }
}
