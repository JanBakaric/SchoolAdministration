using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdministracijaSkole.Web.Controllers;

[Route("Api/Class"), ApiController]
public class ClassController(
    SchoolManagerDbContext _dbContext
) : Controller
{
	//
	//INDEX CLASS
	//

	[HttpGet, Authorize(Roles = "Administrator,Professor")]
	public async Task<IActionResult> Index()
	{
		var classes = await _dbContext.Classes
			.Include(c => c.Professor)
            .Include(c => c.Students)
			.ToListAsync();

		return View(classes);
	}

	[HttpPost("Search"), Authorize(Roles = "Administrator,Professor")]
	public async Task<IActionResult> Search([FromForm] ClassFilterModel filter)
    {
        var classQuery = _dbContext.Classes
            .Include(c => c.Professor)
			.Include(c => c.Students)
			.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.ClassName))
            classQuery = classQuery
				.Where(c => (c.SchoolYear + " " + c.ClassName)
				.Contains(filter.ClassName.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.ProfessorName))
            classQuery = classQuery
				.Where(c => (c.Professor.FirstName + " " + c.Professor.LastName)
				.Contains(filter.ProfessorName.ToLower()));

		if (filter.NumberOfStudents.HasValue)
			classQuery = classQuery
				.Where(c => c.Students.Count == filter.NumberOfStudents.Value);

		var model = await classQuery.ToListAsync();
        return View(model);
	}

	[HttpGet("Get")]
	public IActionResult Get()
	{
		var classes = _dbContext.Classes.ToList();
		return Ok(classes);
	}

	//
	//DETAILS CLASS
	//

	[ActionName(nameof(Details)), HttpGet("{id}"), Authorize(Roles = "Administrator,Professor")]
	public async Task<IActionResult> Details(int? id = null)
    {
        var cl = await _dbContext.Classes
            .Include(c => c.Professor)
			.Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.ClassID == id);

		if (cl == null)
		{
			return NotFound();
		}

		return View(cl);
	}

	[ActionName(nameof(Details)), HttpPost("{id}")]
	public async Task<IActionResult> DetailsRequest(int? id = null)
	{
		var cl = await _dbContext.Classes
			.Include(c => c.Professor)
			.Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.ClassID == id);

		if (cl == null)
		{
			return NotFound();
		}

		return Ok(cl);
	}

	//
	//EDIT CLASS
	//

	[ActionName(nameof(Edit)), HttpGet("Edit/{id}"), Authorize(Roles = "Administrator")]
	public async Task<IActionResult> Edit(int id)
    {
        var cl = await _dbContext.Classes
			.Include(c => c.Professor)
			.Include(c => c.Students)
			.FirstOrDefaultAsync(c => c.ClassID == id);

        FillDropdownValues();
        return View(cl);
	}

	[ActionName(nameof(Edit)), HttpPost("Edit/{id}")]
	public async Task<IActionResult> Edit(int id, [FromForm] Class cl)
	{
		var result = await UpdateClassAsync(cl, id);
		if(result != null)
		{
			return result;
		}

		return RedirectToAction(nameof(Index));
	}

	[ActionName(nameof(Edit)), HttpPut("{id}")]
	public async Task<IActionResult> EditRequest(int id, [FromBody] Class cl)
	{
		var result = await UpdateClassAsync(cl, id);
		if (result != null)
		{
			return result;
		}

		return Ok(cl);
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
			.Where(p => p.Class == null)
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

	private async Task<IActionResult> UpdateClassAsync(Class cl, int id)
	{
		if (id != cl.ClassID)
		{
			return BadRequest("Class ID mismatch");
		}

		try
		{
			_dbContext.Update(cl);
			await _dbContext.SaveChangesAsync();
			return null;
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!await ClassExists(id))
			{
				return NotFound();
			}

			else
			{
				throw;
			}
		}
	}

	private async Task<bool> ClassExists(int id)
	{
		return await _dbContext.Classes.AnyAsync(e => e.ClassID != id);
	}
}