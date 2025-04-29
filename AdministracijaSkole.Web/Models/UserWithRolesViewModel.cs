namespace AdministracijaSkole.Web.Models;

public class UserWithRolesViewModel
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
}