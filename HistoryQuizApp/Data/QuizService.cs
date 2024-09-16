using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HistoryQuizApp.Data
{
    public class QuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuizService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public QuizService(ApplicationDbContext context, ILogger<QuizService> logger, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authenticationStateProvider)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _authenticationStateProvider = authenticationStateProvider;
        }

        private async Task<string> GetCurrentUserIdAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId; // Return User ID as string
                }
            }

            throw new InvalidOperationException("User ID is not in the correct format.");
        }

        public async Task<List<Quiz>> GetAvailableQuizzesAsync()
        {
            string userId = await GetCurrentUserIdAsync();

            return await _context.Quizzes
           .Include(q => q.Questions) // Include questions if needed for counting
           .Where(q => q.CreatedByUserId == userId)
           .ToListAsync();
        }

        public async Task<Quiz> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task SubmitAnswerAsync(string userId, int quizId, int questionId, string answer)
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

        public async Task<List<Question>> GetAdaptiveQuizAsync(int quizId, string userId)
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

        public async Task AwardBadgeAsync(string userId, string badgeName)
        {
            var user = await _context.Users.Include(u => u.Badges).FirstOrDefaultAsync(u => u.Id == userId);
            var badge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == badgeName);

            if (badge != null && !user.Badges.Contains(badge))
            {
                user.Badges.Add(badge);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUserScoreAsync(string userId, int quizId)
        {
            var userProgress = await _context.UserProgresses
                .FirstOrDefaultAsync(up => up.UserId == userId && up.QuizId == quizId);

            return userProgress?.Score ?? 0;
        }

        public async Task<Dictionary<int, string>> GetUserAnswersAsync(string userId, int quizId)
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
            string userId = await GetCurrentUserIdAsync(); // Get current logged-in user ID
            quiz.CreatedByUserId = userId; // Set creator as the current user
            quiz.IsPublished = false; // Initially set as not published
            quiz.PlayCount = 0; // Initialize play count to 0

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            // Add creator as the quiz owner
            await AddQuizCollaboratorAsync(quiz.Id, userId, "Owner");
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

        public async Task AddQuizCollaboratorAsync(int quizId, string userId, string role = "Owner")
        {
            var collaborator = new QuizCollaborator
            {
                QuizId = quizId,
                CollaboratorUserId = userId,
                Role = role
            };

            _context.QuizCollaborators.Add(collaborator);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanEditOrDeleteQuizAsync(int quizId)
        {
            string userId = await GetCurrentUserIdAsync(); // Retrieve the user ID

            var collaborator = await _context.QuizCollaborators
                .FirstOrDefaultAsync(qc => qc.QuizId == quizId && qc.CollaboratorUserId == userId);

            return collaborator != null && (collaborator.Role == "Owner" || collaborator.Role == "Collaborator");
        }

        public async Task PublishQuizAsync(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz != null)
            {
                quiz.IsPublished = true; // Mark quiz as published
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Quiz>> GetPublishedQuizzesAsync()
        {
            return await _context.Quizzes
                .Where(q => q.IsPublished)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetQuizzesByUserAsync(string userId)
        {
            return await _context.Quizzes
                .Where(q => q.CreatedByUserId == userId)
                .ToListAsync();
        }

        public async Task IncrementPlayCountAsync(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz != null)
            {
                quiz.PlayCount += 1; // Increment the play count
                await _context.SaveChangesAsync();
            }
        }
        public async Task AddQuestionToQuizAsync(int quizId, Question question)
        {
            var quiz = await _context.Quizzes
        .Include(q => q.Questions)
        .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz != null)
            {
                question.Id = 0; // Ensure this is a new question
                quiz.Questions.Add(question);
                await _context.SaveChangesAsync();
            }
        }


    }
}
