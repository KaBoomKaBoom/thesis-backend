namespace ThesisBackend.DTOs.TestSessionDTOs
{
    public class UserDashboardDTO
    {
        public DashboardStatsDTO Stats { get; set; } = new DashboardStatsDTO();
        public List<DashboardRecentSessionDTO> RecentSessions { get; set; } = new List<DashboardRecentSessionDTO>();
        public List<DashboardTrendPointDTO> ScoreTrend { get; set; } = new List<DashboardTrendPointDTO>();
        public DashboardTopicAnalyticsDTO TopicAnalytics { get; set; } = new DashboardTopicAnalyticsDTO();
    }

    public class DashboardStatsDTO
    {
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public float AverageScorePercentage { get; set; }
        public float BestScorePercentage { get; set; }
        public string? LastSessionResultLabel { get; set; }
        public DateTime? LastSessionTakenAt { get; set; }
    }

    public class DashboardRecentSessionDTO
    {
        public int SessionId { get; set; }
        public int TestId { get; set; }
        public DateTime TestTakenTime { get; set; }
        public int? CorrectAnswers { get; set; }
        public int? TotalQuestions { get; set; }
        public float? ScorePercentage { get; set; }
        public string? ResultLabel { get; set; }
    }

    public class DashboardTrendPointDTO
    {
        public DateTime Date { get; set; }
        public float AverageScorePercentage { get; set; }
        public int SessionsCount { get; set; }
    }

    public class DashboardTopicAnalyticsDTO
    {
        public List<DashboardQuestionAccuracyDTO> StrongestQuestions { get; set; } = new List<DashboardQuestionAccuracyDTO>();
        public List<DashboardQuestionAccuracyDTO> WeakestQuestions { get; set; } = new List<DashboardQuestionAccuracyDTO>();
    }

    public class DashboardQuestionAccuracyDTO
    {
        public int QuestionId { get; set; }
        public int Attempts { get; set; }
        public int CorrectAnswers { get; set; }
        public float AccuracyPercentage { get; set; }
    }
}
