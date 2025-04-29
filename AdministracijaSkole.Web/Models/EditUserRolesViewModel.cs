namespace AdministracijaSkole.Web.Models;

public class EditUserRolesViewModel
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public List<RoleSelection> Roles { get; set; }
    public string SelectedRole { get; set; }
}

public class RoleSelection
{
    public string RoleName { get; set; }
    public bool IsSelected { get; set; }
}