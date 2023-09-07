namespace CRM_mvc.Models.Views.CustomerViewModel;

public class CustomerResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int CallsNumber { get; set; }
    public int ChatsNumber { get; set; }
}