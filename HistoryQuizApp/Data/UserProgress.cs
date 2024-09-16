using System.ComponentModel.DataAnnotations;

namespace HistoryQuizApp.Data
{
    public class UserProgress
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int QuizId { get; set; }
        public int Score { get; set; }
        public List<UserAnswer> Answers { get; set; } 
    }
}
