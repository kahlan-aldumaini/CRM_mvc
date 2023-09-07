using CRM_mvc.Models.Entities;

namespace CRM_mvc.Models.Views.QuestionsAndAnswer;

public class AnswersViewModel
{
    public int Id { get; set; }
    public string? Question { get; set; }
    public List<AnswerResponse> Answers { get; set; }
    public List<ReturnAction> ReturnActions { get; set; }
    
    public NewAnswer NewAnswer { get; set; }
    public bool CanReturn { get; set; }
    public int? CallId { get; set; }
}