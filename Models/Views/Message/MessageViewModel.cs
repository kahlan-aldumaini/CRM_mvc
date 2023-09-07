namespace CRM_mvc.Models.Views.Message
{
    public class MessageViewModel
    {
        public List<MessageResponse>? Responses { get; set; }
        public SendMessageViewModel SendMessageViewModel { get; set; }
    }

    public class MessageResponse
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Recipient { get; set; }
        public string Date { get; set; }
    }
}
