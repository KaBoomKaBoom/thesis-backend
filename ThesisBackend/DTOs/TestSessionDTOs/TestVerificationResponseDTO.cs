namespace ThesisBackend.DTOs.TestSessionDTOs
{
    public class TestVerificationResponseDTO
    {
        public int test_id { get; set; }
        public int total_questions { get; set; }
        public int correct_answers { get; set; }
        public int skipped { get; set; }
        public float score_percentage { get; set; }
        public List<QuestionResult> results { get; set; } = new List<QuestionResult>();
    }

    public class QuestionResult
    {
        public int position { get; set; }
        public int question_id { get; set; }
        public int? submitted_answer_id { get; set; }
        public int? correct_answer_id { get; set; }
        public bool is_correct { get; set; }
    }
}
