using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace AdministracijaSkole.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class UserManagementController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SchoolManagerDbContext _context;  
    private readonly ILogger<UserManagementController> _logger;


    public UserManagementController(UserManager<AppUser> userManager, SchoolManagerDbContext context, ILogger<UserManagementController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var userViewModels = new List<UserWithRolesViewModel>();

        _logger.LogInformation("User page loaded at {Time}", DateTime.UtcNow);


        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userViewModels.Add(new UserWithRolesViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.UserName,
                Roles = roles.ToList()
            });
        }

        return View(userViewModels); 
    }

    public async Task<IActionResult> EditRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        string currentUserId = _userManager.GetUserId(User);

        if (currentUserId == userId)
        {
  
            TempData["ErrorMessage"] = "You cannot change your own role.";
            return RedirectToAction("Index"); 
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var allRoles = new List<string> { "Administrator", "Professor", "Student" }; 

        var model = new EditUserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = allRoles.Select(role => new RoleSelection
            {
                RoleName = role,
                IsSelected = userRoles.Contains(role) 
            }).ToList()
        };

            return View(model);
 
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoles(EditUserRolesViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();




        if (string.IsNullOrEmpty(model.SelectedRole))
        {
            ModelState.AddModelError("", "Please select a role.");
            return View(model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Error removing roles.");
            return View(model);
        }

        result = await _userManager.AddToRoleAsync(user, model.SelectedRole);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Error adding role.");
            return View(model);
        }

        TempData["Success"] = "Role updated successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        string currentUserId = _userManager.GetUserId(User);

        if (currentUserId == userId)
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToAction("Index"); 
        }


        if (user == null) return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "User deleted successfully!";
        }
        else
        {
            TempData["Error"] = "Error deleting user.";
        }

        return RedirectToAction("Index");
    }
}