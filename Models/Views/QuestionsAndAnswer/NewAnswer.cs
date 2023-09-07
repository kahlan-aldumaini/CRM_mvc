
using System.ComponentModel.DataAnnotations;

namespace CRM_mvc.Models.Views.QuestionsAndAnswer;

public class NewAnswer
{
    
    [Required(ErrorMessage = "من فضلك ادخل الاجابة")]
    public string Answer { get; set; }
    public string? ReturnAction { get; set; }
    public DateTime? DateTime { get; set; }
}