using System.ComponentModel.DataAnnotations;

namespace HistoryQuizApp.Data
{
    public class Question
    {
        [Key]
        public int Id { get; set; }
        public string? Text { get; set; }
        public required List<string?> Options { get; set; }
        public required List<bool> CorrectAnswers { get; set; } // Track correct answers (true or false)
        public string? DifficultyLevel { get; set; } // Easy, Medium, Hard
    }
}