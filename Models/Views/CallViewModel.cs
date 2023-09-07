using CRM_mvc.Models.Entities;

namespace CRM_mvc.Models.Views
{
    public class CallViewModel
    {
        public List<CallResponse>? Calls { get; set; }
        public NewCallViewModel? NewCall { get; set; }
    }

    public class NewCallViewModel
    {
    }

    public class IncomingCall
    {
        string CallName { get; set; }
        string CallNumber { get; set; }
    }

    public class CallDetailViewModel
    {
        public Call? Call { get; set; }
    }

    public class CallResponse
    {
        public int Id { get; set; }
        public string? CustomerPhoneNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? UserName { get; set; }
        public string? TypeName { get; set; }
        public int QuestionCount { get; set; }
        public string Duration { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}