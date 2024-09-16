using System.ComponentModel.DataAnnotations;

namespace HistoryQuizApp.Data
{
    public class User
    {
        [Key]
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public required List<UserProgress> Progress { get; set; }
        public required List<Badge> Badges { get; set; }
    }

}
