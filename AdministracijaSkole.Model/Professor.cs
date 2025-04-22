using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AdministracijaSkole.Model;

public class Professor
{
    [Key]
    public int ProfessorID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
	public char Gender { get; set; }
	public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime HireDate { get; set; }

	[JsonIgnore]
	public string FullName => $"{FirstName} {LastName}";

    //Navigacijska svojstva
    [JsonIgnore]
	public virtual Class? Class { get; set; }
	[JsonIgnore]
	public virtual Subject? Subject { get; set; }
}