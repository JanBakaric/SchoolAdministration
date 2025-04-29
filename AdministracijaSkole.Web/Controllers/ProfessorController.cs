using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdministracijaSkole.Web.Controllers;

[Route("Api/Professor"), ApiController]
public class ProfessorController
(
	SchoolManagerDbContext _dbContext
) : Controller
{
	//
	//INDEX PROFESSOR
	//

	[HttpGet, Authorize(Roles = "Administrator,Professor")]
	public IActionResult Index()
	{
		var professors = _dbContext.Professors.ToList();
		return View(professors);
	}

	[HttpPost("Search"), Authorize(Roles = "Administrator,Professor")]
	public async Task<IActionResult> Search([FromForm] ProfessorFilterModel filter)
	{
		var professorQuery = _dbContext.Professors.AsQueryable();

		if (!string.IsNullOrWhiteSpace(filter.FullName))
			professorQuery = professorQuery
				.Where(p => (p.FirstName + " " + p.LastName)
				.Contains(filter.FullName.ToLower()));

		if (!string.IsNullOrWhiteSpace(filter.Email))
			professorQuery = professorQuery
				.Where(p => p.Email.Contains(filter.Email.ToLower()));

		var model = await professorQuery.ToListAsync();
		return PartialView("_IndexTable", model);
	}

	[HttpGet("Get")]
    public IActionResult Get()
	{
		var professors = _dbContext.Professors.ToList();
		return Ok(professors);
	}

	//
	//DETAILS PROFESSOR
	//

	[ActionName(nameof(Details)), HttpGet("{id}"), Authorize(Roles = "Administrator,Professor")]
	public async Task<IActionResult> Details(int? id = null)
	{
		var professor = await _dbContext.Professors
			.Include(p => p.Class)
			.FirstOrDefaultAsync(p => p.ProfessorID == id);

		if (professor == null)
		{
			return NotFound();
		}

		return View(professor);
	}

	[ActionName(nameof(Details)), HttpPost("{id}")]
	public async Task<IActionResult> DetailsRequest(int? id = null)
	{
		var professor = await _dbContext.Professors
			.Include(p => p.Class)
			.FirstOrDefaultAsync(p => p.ProfessorID == id);

		if (professor == null)
        {
            return NotFound();
        }

        return Ok(professor);
	}

	//
	//CREATE PROFESSOR
	//

	[ActionName(nameof(Create)), HttpGet("Create"), Authorize(Roles = "Administrator")]
	public IActionResult Create()
	{
		return View();
	}

    [ActionName(nameof(Create)), HttpPost("Create")]
	public async Task<IActionResult> Create([FromForm] Professor professor)
	{
		if (ModelState.IsValid)
		{
			await _dbContext.Professors.AddAsync(professor);
			await _dbContext.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		return BadRequest(ModelState);
	}

	[ActionName(nameof(Create)), HttpPost]
	public async Task<IActionResult> CreateRequest([FromBody] Professor professor)
	{
		await _dbContext.Professors.AddAsync(professor);
		await _dbContext.SaveChangesAsync();

		return CreatedAtAction(nameof(Details), new { id = professor.ProfessorID }, professor);
	}

	//
	//EDIT PROFESSOR
	//

	[ActionName(nameof(Edit)), HttpGet("Edit/{id}"), Authorize(Roles = "Administrator")]
	public async Task<IActionResult> Edit(int id)
	{
		var professor = await _dbContext.Professors.FindAsync(id);
		if (professor == null)
		{
			return NotFound();
		}

		return View(professor);
	}

	[ActionName(nameof(Edit)), HttpPost("Edit/{id}")]
	public async Task<IActionResult> Edit(int id, [FromForm] Professor professor)
	{
		var result = await UpdateProfessorAsync(professor, id);
		if (result != null)
		{
			return result;
		}

		return RedirectToAction(nameof(Index));
	}

	[ActionName(nameof(Edit)), HttpPut("{id}")]
	public async Task<IActionResult> EditRequest(int id, [FromBody] Professor professor)
	{
		var result = await UpdateProfessorAsync(professor, id);
		if (result != null)
		{
			return result;
		}

		return Ok(professor);
	}

	//
	//DELETE PROFESSOR
	//

	[ActionName(nameof(Delete)), HttpDelete("{id}"), Authorize(Roles = "Administrator")]
	public async Task<IActionResult> Delete(int id)
	{
		var professor = await _dbContext.Professors.FindAsync(id);
		if (professor == null)
		{
			return NotFound();
		}

		var classes = _dbContext.Classes.Where(c => c.ProfessorID == professor.ProfessorID).ToList();

		foreach (var cl in classes)
		{
			cl.ProfessorID = null;
		}

		_dbContext.Classes.UpdateRange(classes);
		await _dbContext.SaveChangesAsync();

		_dbContext.Professors.Remove(professor);
		await _dbContext.SaveChangesAsync();

		return NoContent();
	}

	//
	//UTILITES
	//

	private async Task<IActionResult> UpdateProfessorAsync(Professor professor, int id)
	{
		if (id != professor.ProfessorID)
		{
			return BadRequest("Professor ID mismatch");
		}

		try
		{
			_dbContext.Update(professor);
			await _dbContext.SaveChangesAsync();
			return null;
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!await ProfessorExists(id))
			{
				return NotFound();
			}

			else
			{
				throw;
			}
		}
	}

	private async Task<bool> ProfessorExists(int id)
	{
		return await _dbContext.Professors.AnyAsync(e => e.ProfessorID != id);
	}
}