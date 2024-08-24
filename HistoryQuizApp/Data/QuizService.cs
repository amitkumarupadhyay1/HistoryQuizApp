using Microsoft.EntityFrameworkCore;

namespace HistoryQuizApp.Data
{
    public class QuizService
    {
        private readonly ApplicationDbContext _context;

        public QuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Quiz> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task SubmitAnswerAsync(int userId, int quizId, int questionId, string answer)
        {
            var userProgress = await _context.UserProgresses
                .FirstOrDefaultAsync(up => up.UserId == userId && up.QuizId == quizId);

            if (userProgress == null)
            {
                userProgress = new UserProgress
                {
                    UserId = userId,
                    QuizId = quizId,
                    Score = 0
                };
                _context.UserProgresses.Add(userProgress);
            }

            var question = await _context.Questions.FindAsync(questionId);
            if (question.Answer == answer)
            {
                userProgress.Score += 1; // or any scoring logic
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUserScoreAsync(int userId, int quizId)
        {
            var userProgress = await _context.UserProgresses
                .FirstOrDefaultAsync(up => up.UserId == userId && up.QuizId == quizId);

            return userProgress?.Score ?? 0;
        }
    }

}
