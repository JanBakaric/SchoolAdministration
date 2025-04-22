using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AdministracijaSkole.Model;

public class Subject
{
    [Key]
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public string? Description { get; set; }

    //Strani ključevi
    [ForeignKey(nameof(Professor))]
    public int? ProfessorID { get; set; }
    [JsonIgnore]
    public virtual Professor? Professor { get; set; }

    //Navigacijska svojstva
    public virtual ICollection<Student>? Students { get; set; }
}