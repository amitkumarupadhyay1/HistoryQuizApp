namespace HistoryQuizApp.Data
{
    public class UserProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public int Score { get; set; }
    }
}
