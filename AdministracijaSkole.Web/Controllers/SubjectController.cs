using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdministracijaSkole.Web.Controllers
{
	[Route("Api/Subject"), ApiController]
    public class SubjectController (
    SchoolManagerDbContext _dbContext
) : Controller
    {
		//
		//INDEX SUBJECT
		//

		[HttpGet, Authorize(Roles = "Administrator,Professor")]
		public async Task<IActionResult> Index()
        {
            var subjects = await _dbContext.Subjects
                .Include(s => s.Professor)
                .Include(s => s.Students)
                .ToListAsync();

            return View(subjects);
        }

        [HttpPost("Search"), Authorize(Roles = "Administrator,Professor")]
        public async Task<IActionResult> Search([FromForm] SubjectFilterModel filter)
        {
            var subjectQuery = _dbContext.Subjects
                .Include(c => c.Professor)
                .Include(c => c.Students)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SubjectName))
                subjectQuery = subjectQuery
					.Where(s => s.SubjectName.Contains(filter.SubjectName.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.ProfessorName))
                subjectQuery = subjectQuery
					.Where(c => (c.Professor.FirstName + " " + c.Professor.LastName)
					.Contains(filter.ProfessorName.ToLower()));

            if (filter.NumberOfStudents.HasValue)
                subjectQuery = subjectQuery
                    .Where(c => c.Students.Count == filter.NumberOfStudents.Value);

            var model = await subjectQuery.ToListAsync();
            return View(model);
        }

        [HttpGet("Get")]
        public IActionResult Get()
        {
            var subjects = _dbContext.Subjects.ToList();
            return Ok(subjects);
		}

		//
		//DETAILS SUBJECT
		//

		[ActionName(nameof(Details)), HttpGet("{id}"), Authorize(Roles = "Administrator,Professor")]
		public async Task<IActionResult> Details(int? id = null)
		{
			var subject = await _dbContext.Subjects
				.Include(c => c.Professor)
				.Include(c => c.Students)
				.FirstOrDefaultAsync(c => c.SubjectID == id);

			if (subject == null)
			{
				return NotFound();
			}

			return View(subject);
		}

		[ActionName(nameof(Details)), HttpPost("{id}")]
		public async Task<IActionResult> DetailsRequest(int? id = null)
		{
			var subject = await _dbContext.Subjects
				.Include(c => c.Professor)
				.Include(c => c.Students)
				.FirstOrDefaultAsync(c => c.SubjectID == id);

			if (subject == null)
			{
				return NotFound();
			}

			return Ok(subject);
		}

		//
		//EDIT SUBJECT
		//

		[ActionName(nameof(Edit)), HttpGet("Edit/{id}"), Authorize(Roles = "Administrator")]
		public async Task<IActionResult> Edit(int id)
		{
			var subject = await _dbContext.Subjects
				.Include(c => c.Professor)
				.Include(c => c.Students)
				.FirstOrDefaultAsync(c => c.SubjectID == id);

			FillDropdownValues();
			return View(subject);
		}

		[ActionName(nameof(Edit)), HttpPost("Edit/{id}")]
		public async Task<IActionResult> Edit(int id, [FromForm] Subject subject)
		{
			var result = await UpdateSubjectAsync(subject, id);
			if (result != null)
			{
				return result;
			}

			return RedirectToAction(nameof(Index));
		}

		[ActionName(nameof(Edit)), HttpPut("{id}")]
		public async Task<IActionResult> EditRequest(int id, [FromBody] Subject subject)
		{
			var result = await UpdateSubjectAsync(subject, id);
			if (result != null)
			{
				return result;
			}

			return Ok(subject);
		}

		[HttpPost("Enroll-Students/{subjectId}")]
		public async Task<IActionResult> EnrollStudents(int subjectId, [FromBody] int studentId)
		{
			var subject = await _dbContext.Subjects
				.Include(s => s.Students)
				.FirstOrDefaultAsync(s => s.SubjectID == subjectId);

			if (subject == null)
			{
				return NotFound("Subject not found.");
			}

			var student = await _dbContext.Students.FindAsync(studentId);

			if (!subject.Students.Contains(student))
			{
				subject.Students.Add(student);
			}
			student.Subjects.Add(subject);

			await _dbContext.SaveChangesAsync();

			return Ok();
		}

		[HttpPost("Delist-Student/{subjectId}")]
		public async Task<IActionResult> DelistStudent(int subjectId, [FromBody] int studentId)
		{
			var subject = await _dbContext.Subjects
				.Include(s => s.Students)
				.FirstOrDefaultAsync(s => s.SubjectID == subjectId);

			if (subject == null)
			{
				return NotFound("Subject not found.");
			}

			var student = await _dbContext.Students.FindAsync(studentId);

			if (subject.Students.Contains(student))
			{
				subject.Students.Remove(student);
			}
			student.Subjects.Remove(subject);

			await _dbContext.SaveChangesAsync();

			return Ok();
		}

		//
		//UTILITES
		//

		private void FillDropdownValues()
		{
			var selectItems = new List<SelectListItem>();

			var listItem = new SelectListItem
			{
				Text = "- Choose -",
				Value = ""
			};
			selectItems.Add(listItem);

			var professors = _dbContext.Professors
				.Where(p => p.Subject == null)
				.ToList();

			foreach (var professor in professors)
			{
				listItem = new SelectListItem
				(
					professor.FullName,
					professor.ProfessorID.ToString()
				);
				selectItems.Add(listItem);
			}

			ViewBag.PossibleProfessors = selectItems;
		}

		private async Task<IActionResult> UpdateSubjectAsync(Subject subject, int id)
		{
			if (id != subject.SubjectID)
			{
				return BadRequest("Subject ID mismatch");
			}

			try
			{
				_dbContext.Update(subject);
				await _dbContext.SaveChangesAsync();
				return null;
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!await SubjectExists(id))
				{
					return NotFound();
				}

				else
				{
					throw;
				}
			}
		}

		private async Task<bool> SubjectExists(int id)
		{
			return await _dbContext.Subjects.AnyAsync(e => e.SubjectID != id);
		}
	}
}
