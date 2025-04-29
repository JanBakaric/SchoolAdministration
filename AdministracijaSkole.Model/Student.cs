using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AdministracijaSkole.Model;

public class Student
{
    [Key]
    public int StudentID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public char Gender { get; set; }
    public string? Address { get; set; }
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }

	[JsonIgnore]
	public string FullName => $"{FirstName} {LastName}";

	//Strani ključevi
	[ForeignKey(nameof(Class))]
    public int ClassID { get; set; }
    [ForeignKey(nameof(AppUser))]
    public string? UserID { get; set; }

    //Navigacijska svojstva
    [JsonIgnore]
	public virtual Class? Class { get; set; }
    [JsonIgnore]
    public virtual ICollection<Subject> Subjects { get; set; } = [];
	[JsonIgnore]
	public virtual ICollection<Grade> Grades { get; set; } = [];
	[JsonIgnore]
    public virtual AppUser? User { get; set; }
}