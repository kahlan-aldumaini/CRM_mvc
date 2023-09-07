using CRM_mvc.Models.Entities;

namespace CRM_mvc.Models.Views.CustomerViewModel;

public class PotentialCustomerViewModel
{
    public int Id { get; set; }
    public string Phone { get; set; }
    public NewCustomer NewCustomer { get; set; }
    
    public List<City>? Cities { get; set; }
    public List<ProjectType>? ProjectTypes { get; set; }
}