using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HistoryQuizApp.Data
{
    public class QuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuizService> _logger;

        public QuizService(ApplicationDbContext context, ILogger<QuizService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Quiz>> GetAvailableQuizzesAsync()
        {
            return await _context.Quizzes.ToListAsync();
        }

        public async Task<Quiz> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes
       .Include(q => q.Questions)
       .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task SubmitAnswerAsync(int userId, int quizId, int questionId, string answer)
        {
            // Check if the user already submitted an answer for this question
            var userAnswer = await _context.UserAnswers
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.QuizId == quizId && ua.QuestionId == questionId);

            if (userAnswer == null)
            {
                // If no previous answer exists, create a new one
                userAnswer = new UserAnswer
                {
                    UserId = userId,
                    QuizId = quizId,
                    QuestionId = questionId,
                    SelectedAnswer = answer
                };
                _context.UserAnswers.Add(userAnswer);
            }
            else
            {
                // If an answer already exists, update it
                userAnswer.SelectedAnswer = answer;
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
        }


        public async Task<List<Question>> GetQuestionsByDifficultyAsync(int quizId, string difficultyLevel)
        {
            try
            {
                return await _context.Questions
                    .Where(q => q.Id == quizId && q.DifficultyLevel == difficultyLevel)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting questions by difficulty.");
                throw;
            }
        }

        public async Task<List<Question>> GetAdaptiveQuizAsync(int quizId, int userId)
        {
            var userProgress = await _context.UserProgresses
       .FirstOrDefaultAsync(up => up.UserId == userId && up.QuizId == quizId);

            string difficultyLevel = "Easy";
            if (userProgress != null)
            {
                if (userProgress.Score > 5)
                {
                    difficultyLevel = "Medium";
                }
                if (userProgress.Score > 10)
                {
                    difficultyLevel = "Hard";
                }
            }

            return await GetQuestionsByDifficultyAsync(quizId, difficultyLevel);
        }

        public async Task AwardBadgeAsync(int userId, string badgeName)
        {
            var user = await _context.Users.Include(u => u.Badges).FirstOrDefaultAsync(u => u.Id == userId);
            var badge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == badgeName);

            if (badge != null && !user.Badges.Contains(badge))
            {
                user.Badges.Add(badge);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUserScoreAsync(int userId, int quizId)
        {
            var userProgress = await _context.UserProgresses
                .FirstOrDefaultAsync(up => up.UserId == userId && up.QuizId == quizId);

            return userProgress?.Score ?? 0;
        }

        public async Task<Dictionary<int, string>> GetUserAnswersAsync(int userId, int quizId)
        {
            // Fetch all user answers for the specific quiz
            var userAnswers = await _context.UserAnswers
                .Where(ua => ua.UserId == userId && ua.QuizId == quizId)
                .ToListAsync();

            // Return a dictionary with questionId as key and selected answer as value
            return userAnswers.ToDictionary(ua => ua.QuestionId, ua => ua.SelectedAnswer);
        }

        public async Task CreateQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuizAsync(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }
    }
}
