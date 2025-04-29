using AdministracijaSkole.Model;

namespace AdministracijaSkole.Web.Models
{
    public class StudentDetailsViewModel
    {
        public Student Student { get; set; }
        public List<GradesBySubject> GroupedGrades { get; set; }
		public List<PresenceDisplay> Presences { get; set; }

		public class GradesBySubject
		{
			public string SubjectName { get; set; }
			public List<GradeDisplay> Grades { get; set; }
		}

		public class GradeDisplay
		{
			public int Value { get; set; }
			public DateTime DateAwarded { get; set; }
		}

		public class PresenceDisplay
		{
			public DateTime PresenceDate { get; set; }
			public bool IsPresent { get; set; }
			public bool IsExcused { get; set; }
		}
	}
}
