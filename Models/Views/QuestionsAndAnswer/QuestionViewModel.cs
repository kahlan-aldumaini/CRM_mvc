namespace CRM_mvc.Models.Views.QuestionsAndAnswer;

public class QuestionViewModel
{
    public int? CallId { get; set; }
    public int? CustomerId { get; set; }
    public List<QuestionResponse> Questions { get; set; }
}