namespace CRM_mvc.Models.Views.Sessions;

public class ConversationsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Closed { get; set; }
    
    public List<MessageResponse>? Messages { get; set; }

    public List<CurrentSessions>? CurrentSessions { get; set; }
}



public class CurrentSessions
{
    public int Id { get; set; }
    public string? Phone { get; set; }
    public string? Message { get; set; }
    public int Extention { get; set; }
    public string Time { get; set; }
    public bool Closed { get; set; } = false;
}