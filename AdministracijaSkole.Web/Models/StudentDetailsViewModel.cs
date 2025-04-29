using AdministracijaSkole.Model;

namespace AdministracijaSkole.Web.Models
{
    public class StudentDetailsViewModel
    {
        public Student Student { get; set; }
        public List<GradeDisplay> Grades { get; set; }

        public class GradeDisplay
        {
            public string SubjectName { get; set; }
            public int Value { get; set; }
            public DateTime DateAwarded { get; set; }
        }
    }
}
