namespace CRM_mvc.Models.Views.Charts;

public class WeeklyReport
{
    public PerDay CallDay { get; set; }
    public int Calls { get; set; }
        
    public PerDay SessionDay { get; set; }
    public int Sessions { get; set; }
        
    public PerDay EmailDay { get; set; }
    public int Emails { get; set; }
        
    public PerDay SmsDay { get; set; }
    public int Sms { get; set; }
}