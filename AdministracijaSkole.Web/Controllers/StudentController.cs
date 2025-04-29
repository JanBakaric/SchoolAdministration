using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdministracijaSkole.Web.Controllers;

[Route("Api/Student"), ApiController]
public class StudentController
(
    SchoolManagerDbContext _dbContext
) : Controller
{
	//
	//INDEX STUDENT
	//

	[HttpGet, AllowAnonymous]
	public async Task<IActionResult> Index()
	{
		var students = await _dbContext.Students
            .Include(s => s.Class)
            .ToListAsync();

		return View(students);
	}

	[HttpPost("Search"), AllowAnonymous]
	public async Task<IActionResult> Search([FromForm] StudentFilterModel filter)
    {
        var studentQuery = _dbContext.Students
            .Include(s => s.Class)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.FullName))
            studentQuery = studentQuery
				.Where(s => (s.FirstName + " " + s.LastName)
				.Contains(filter.FullName.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Class))
            studentQuery = studentQuery
				.Where(s => (s.Class.SchoolYear + " " + s.Class.ClassName)
                .Contains(filter.Class.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Address))
            studentQuery = studentQuery
				.Where(s => s.Address.Contains(filter.Address.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Email))
            studentQuery = studentQuery
				.Where(s => s.Email.Contains(filter.Email.ToLower()));

        var model = await studentQuery.ToListAsync();
		return PartialView("_IndexTable", model);
    }

    [HttpGet("Get")]
    public IActionResult Get()
    {
        var students = _dbContext.Students.ToList();
        return Ok(students);
	}

	[HttpGet("Available-class")]
	public async Task<IActionResult> GetUnassignedStudentsClass()
	{
		var students = await _dbContext.Students
            .Where(s => s.ClassID == 1)
            .ToListAsync();

		return Ok(students);
	}

	[HttpPost("available-subject/{subjectID}")]
	public async Task<IActionResult> GetUnassignedStudentsSubject(int subjectID)
	{
		var students = await _dbContext.Students
			.Where(s => !s.Subjects.Any(sub => sub.SubjectID == subjectID))
			.ToListAsync();

		return Ok(students);
	}

	//
	//DETAILS STUDENT
	//

	[ActionName(nameof(Details)), HttpGet("{id}"), AllowAnonymous]
    public async Task<IActionResult> Details(int? id = null)
	{
		var student = await _dbContext.Students
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.StudentID == id);

        if (student == null)
        {
            return NotFound();
        }

        return View(student);
    }

    [ActionName(nameof(Details)), HttpPost("{id}")]
    public async Task<IActionResult> DetailsRequest(int? id = null)
    {
        var student = await _dbContext.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        return Ok(student);
    }

    //
    //CREATE STUDENT
    //

    [ActionName(nameof(Create)), HttpGet("Create"), Authorize(Roles = "Administrator,Professor")]
    public IActionResult Create()
	{
		FillDropdownValues();
		return View();
    }

    [ActionName(nameof(Create)), HttpPost("Create")]
    public async Task<IActionResult> Create([FromForm] Student student)
    {
        if (ModelState.IsValid)
        {
            await _dbContext.Students.AddAsync(student);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return BadRequest(ModelState);
    }

    [ActionName(nameof(Create)), HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] Student student)
    {
        await _dbContext.Students.AddAsync(student);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(Details), new { id = student.StudentID }, student);
    }

    //
    //EDIT STUDENT
    //

    [ActionName(nameof(Edit)), HttpGet("Edit/{id}"), Authorize(Roles = "Administrator,Professor")]
    public async Task<IActionResult> Edit(int id)
    {
        var student = await _dbContext.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        FillDropdownValues();
		return View(student);
    }

    [ActionName(nameof(Edit)), HttpPost("Edit/{id}")]
    public async Task<IActionResult> Edit(int id, [FromForm] Student student)
	{
		var result = await UpdateStudentAsync(student, id);
		if (result != null)
		{
			return result;
		}

		return RedirectToAction(nameof(Index));
    }

    [ActionName(nameof(Edit)), HttpPut("{id}")]
    public async Task<IActionResult> EditRequest(int id, [FromBody] Student student)
	{
		var result = await UpdateStudentAsync(student, id);
		if (result != null)
		{
			return result;
		}

		return Ok(student);
    }

    //
    //DELETE STUDENT
    //

    [ActionName(nameof(Delete)), HttpDelete("{id}"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await _dbContext.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        _dbContext.Students.Remove(student);
        await _dbContext.SaveChangesAsync();

        return NoContent();
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

		foreach (var cl in _dbContext.Classes)
		{
			listItem = new SelectListItem
			(
				cl.SchoolYear.ToString() + ". " + cl.ClassName,
				cl.ClassID.ToString()
			);
			selectItems.Add(listItem);
		}

		ViewBag.PossibleClasses = selectItems;
	}

	private async Task<IActionResult> UpdateStudentAsync(Student student, int id)
	{
		if (id != student.StudentID)
		{
			return BadRequest("Student ID mismatch");
		}

		try
		{
			_dbContext.Update(student);
			await _dbContext.SaveChangesAsync();
			return null;
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!await StudentExists(id))
			{
				return NotFound();
			}

			else
			{
				throw;
			}
		}
	}

	private async Task<bool> StudentExists(int id)
    {
        return await _dbContext.Students.AnyAsync(e => e.StudentID != id);
    }
}