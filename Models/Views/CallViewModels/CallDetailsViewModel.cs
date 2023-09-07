using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views.CustomerViewModel;

namespace CRM_mvc.Models.Views.CallViewModels;

public class CallerDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public List<Question> AllQuestions { get; set; }
    public List<LastCall> Calls { get; set; }
    public List<LastQuestion> Questions { get; set; }
    public List<ProjectResponse>? Answers { get; set; }
}