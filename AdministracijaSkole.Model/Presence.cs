using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AdministracijaSkole.Model
{
	public class Presence
	{
		[Key]
		public int PresenceID { get; set; }
		public bool IsPresent { get; set; }
		public bool IsExcused { get; set; }
		public DateTime PresenceDate { get; set; }

		//Strani ključevi
		[ForeignKey(nameof(Student))]
		public int? StudentID { get; set; }

		//Navigacijska svojstva
		[JsonIgnore]
		public virtual Student? Student { get; set; }
	}
}