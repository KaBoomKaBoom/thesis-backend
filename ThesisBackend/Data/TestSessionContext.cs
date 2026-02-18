using Microsoft.EntityFrameworkCore;
using ThesisBackend.Models.TestSessionModels;

namespace ThesisBackend.Data
{
    public class TestSessionContext(DbContextOptions<TestSessionContext> options) : DbContext(options) 
    {
        public virtual required DbSet<TestSession> TestSessions { get; set; }
        public virtual required DbSet<TestResult> TestResults { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestSession>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.TestId).IsRequired();
                entity.Property(e => e.TestTakenTime).IsRequired();
                entity.OwnsMany(e => e.TestComponents, tc =>
                {
                    tc.WithOwner().HasForeignKey("SessionId");
                    tc.HasKey(e => e.Id);
                });
            });

            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.HasKey(e => e.ResultId);
                entity.Property(e => e.SessionId).IsRequired();
                entity.Property(e => e.TotalQuestions).IsRequired();
                entity.Property(e => e.CorrectAnswers).IsRequired();
                entity.Property(e => e.Skipped).IsRequired();
                entity.Property(e => e.ScorePercentage).IsRequired();
                entity.Property(e => e.VerifiedAt).IsRequired();
                entity.OwnsMany(e => e.DetailedResults, dr =>
                {
                    dr.WithOwner().HasForeignKey("ResultId");
                    dr.HasKey(e => e.Id);
                });
            });
        }
    }
}
