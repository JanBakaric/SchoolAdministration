using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AdministracijaSkole.Model;

public class Class
{
    [Key]
    public int ClassID { get; set; }
    public string ClassName { get; set; } = "";
    public int SchoolYear { get; set; }

    //Strani ključevi
    [ForeignKey(nameof(Professor))]
    public int? ProfessorID { get; set; }

	//Navigacijska svojstva
	[JsonIgnore]
	public virtual Professor? Professor { get; set; }
	[JsonIgnore]
	public virtual ICollection<Student>? Students { get; set; }
}