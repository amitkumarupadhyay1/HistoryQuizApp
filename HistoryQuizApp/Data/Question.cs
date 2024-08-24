namespace HistoryQuizApp.Data
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public List<string> Options { get; set; }
        public string DifficultyLevel { get; set; } // Easy, Medium, Hard
    }

}