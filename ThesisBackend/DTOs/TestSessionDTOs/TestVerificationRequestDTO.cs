namespace ThesisBackend.DTOs.TestSessionDTOs
{
    public class TestVerificationRequestDTO
    {
        public int test_id { get; set; }
        public List<AnswerSubmission> answers { get; set; } = new List<AnswerSubmission>();
    }

    public class AnswerSubmission
    {
        public int? answer_id { get; set; }
        public int question_id { get; set; }
    }
}
