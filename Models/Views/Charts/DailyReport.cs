namespace CRM_mvc.Models.Views.Charts;

public class DailyReport
{
    public int IncomingCalls { get; set; }
    public int OutgoingCalls { get; set; }
    public int Sessions { get; set; }
    public int Emails { get; set; }
    public int Sms { get; set; }

    public int CurrentCalls { get; set; }
    
    public int CurrentIncomingCalls { get; set; }
    public int CurrentOutgoingCalls { get; set; }

    public List<int> IncomingHourlyCalls { get; set; }
    public List<int> OutgoingHourlyCalls { get; set; }

    public int TotalHourlyCalls => IncomingHourlyCalls.Sum() + OutgoingHourlyCalls.Sum();

    public int TotalCalls => IncomingCalls + OutgoingCalls;
    public int TotalInteractions => Sessions + Emails + Sms;
}