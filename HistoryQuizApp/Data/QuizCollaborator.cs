using System.ComponentModel.DataAnnotations;

namespace HistoryQuizApp.Data
{
    public class QuizCollaborator
    {
        [Key]
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string? CollaboratorUserId { get; set; } // Collaborator's User ID
        public string? Role { get; set; } // "Owner" or "Collaborator"
        public Quiz? Quiz { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
