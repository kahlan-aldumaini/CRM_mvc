using CRM_mvc.Models.Views.User;

namespace CRM_mvc.Models.Views.CustomerViewModel;

public class CustomerDetailsResponse
{
    public dynamic Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public List<LastCall> Calls { get; set; }
    public List<LastSession> Sessions { get; set; }
    public NewUser? User { get; set; }
}