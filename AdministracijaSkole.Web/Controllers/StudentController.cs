﻿using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdministracijaSkole.Web.Controllers;

[Route("Api/Student"), ApiController]
public class StudentController
(
    UserManager<AppUser> _userManager,
    SchoolManagerDbContext _dbContext
) : Controller
{
    //
    //INDEX STUDENT
    //

    [HttpGet, Authorize(Roles = "Administrator,Professor")]
    public async Task<IActionResult> Index()
	{
		var students = await _dbContext.Students
            .Include(s => s.Class)
            .ToListAsync();

		return View(students);
	}

	[HttpPost("Search"), Authorize(Roles = "Administrator,Professor")]
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
            var user = new AppUser
            {
                UserName = student.Email,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, "ABcd1234.");

            if(!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return BadRequest(ModelState);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Student");

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return BadRequest(ModelState);
            }

            student.UserID = user.Id;
            await _dbContext.Students.AddAsync(student);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return BadRequest(ModelState);
    }

    [ActionName(nameof(Create)), HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] Student student)
    {
        var user = new AppUser
        {
            UserName = student.Email,
            Email = student.Email,
            PhoneNumber = student.PhoneNumber
        };
        var result = await _userManager.CreateAsync(user, "ABcd1234.");

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return BadRequest(ModelState);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Student");

        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return BadRequest(ModelState);
        }

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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
        var student = await _dbContext.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentID == id);

        if (student == null)
        {
            return NotFound("Student not found.");
        }

        if (student.User != null)
        {
            var user = await _userManager.FindByIdAsync(student.UserID);

            if (user != null)
            {
                var deleteResult = await _userManager.DeleteAsync(user);

                if (!deleteResult.Succeeded)
                {
                    var errors = string.Join(" ", deleteResult.Errors.Select(e => e.Description));
                    return StatusCode(500, $"Failed to delete user account. {errors}");
                }
            }
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

        var existingStudent = await _dbContext.Students.FindAsync(id);
        if (existingStudent == null)
        {
            return NotFound("Student not found.");
        }

        if (!string.IsNullOrEmpty(existingStudent.UserID))
        {
            var user = await _userManager.FindByIdAsync(existingStudent.UserID);

            if (user != null)
            {
                if (user.Email != student.Email)
                {
                    var emailResult = await _userManager.SetEmailAsync(user, student.Email);
                    var usernameResult = await _userManager.SetUserNameAsync(user, student.Email);

                    if (!emailResult.Succeeded || !usernameResult.Succeeded)
                    {
                        var errors = emailResult.Errors.Concat(usernameResult.Errors)
                            .Select(e => e.Description).ToList();
                        ModelState.AddModelError("", string.Join(" ", errors));
                        return BadRequest(ModelState);
                    }
                }

                if (!await _userManager.HasPasswordAsync(user))
                {
                    var passwordResult = await _userManager.AddPasswordAsync(user, "ABcd1234.");
                    if (!passwordResult.Succeeded)
                    {
                        var errors = passwordResult.Errors.Select(e => e.Description).ToList();
                        ModelState.AddModelError("", string.Join(" ", errors));
                        return BadRequest(ModelState);
                    }
                }
            }
        }

        existingStudent.FirstName = student.FirstName;
        existingStudent.LastName = student.LastName;
        existingStudent.DateOfBirth = student.DateOfBirth;
        existingStudent.Gender = student.Gender;
        existingStudent.Address = student.Address;
        existingStudent.Email = student.Email;
        existingStudent.PhoneNumber = student.PhoneNumber;
        existingStudent.ClassID = student.ClassID;

        try
        {
            _dbContext.Update(existingStudent);
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
        return await _dbContext.Students.AnyAsync(e => e.StudentID == id);
    }
}