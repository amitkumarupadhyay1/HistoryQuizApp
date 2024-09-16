using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HistoryQuizApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<UserProgress> UserProgresses { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Badge> Badges{ get; set; }
        public DbSet<QuizCollaborator> QuizCollaborators { get; set; }
    }

}
