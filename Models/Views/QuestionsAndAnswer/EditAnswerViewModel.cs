using System.ComponentModel.DataAnnotations;

namespace CRM_mvc.Models.Views.QuestionsAndAnswer
{
    public class EditAnswerViewModel
    {
        [Required(ErrorMessage = "Answer is required")]
        public int? Id { get; set; }
        public string Answer { get; set; }
        public int QuestionId { get; set; }
    }
}