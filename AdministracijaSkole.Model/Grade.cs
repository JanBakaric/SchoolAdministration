using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AdministracijaSkole.Model
{
	public class Grade
	{
		[Key]
		public int GradeID { get; set; }
		public int GradeValue { get; set; }
		public string GradeTopic { get; set; } = "";
		public DateTime GradeDateTime { get; set; }

		//Strani ključevi
		[ForeignKey(nameof(Student))]
		public int? StudentID { get; set; }
		[ForeignKey(nameof(Subject))]
		public int? SubjectID { get; set; }

		//Navigacijska svojstva
		[JsonIgnore]
		public virtual Student? Student { get; set; }
		[JsonIgnore]
		public virtual Subject? Subject { get; set; }
	}
}