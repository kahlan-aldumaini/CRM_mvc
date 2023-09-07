namespace CRM_mvc.Models.Views.Scheduling;

public class SchedulingResponse
{
    public int Id { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
    public bool AllDay { get; set; }
    public ExtendedProps ExtendedProps { get; set; }
}