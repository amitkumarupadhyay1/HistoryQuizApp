﻿namespace HistoryQuizApp.Data
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int QuizId { get; set; }
        public int QuestionId { get; set; }
        public string? SelectedAnswer { get; set; }
    }
}
