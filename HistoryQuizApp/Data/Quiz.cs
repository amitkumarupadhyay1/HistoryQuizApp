using System.ComponentModel.DataAnnotations;

namespace HistoryQuizApp.Data
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public List<Question> Questions { get; set; }
        // New properties
        public bool IsPublished { get; set; } // Indicates if the quiz is published and visible to everyone
        public string? CreatedByUserId { get; set; } // Tracks who created the quiz
        public int PlayCount { get; set; } // Tracks how many times the quiz has been played
        public DateTime CreatedDate { get; set; }
    }

}
