using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;



namespace AdministracijaSkole.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class SystemUsers
{
    public const string UserName = "system@gmail.com";
}





[Authorize(Roles = "Administrator")]
public class NewsController (SchoolManagerDbContext _context, UserManager<AppUser> userManager) : Controller
{
    private string GetSystemUserId()
    {
        var systemUserId = _context.Users
            .Where(u => u.UserName == SystemUsers.UserName)
            .Select(u => u.Id)
            .FirstOrDefault();

        if (systemUserId == null)
            throw new Exception("System user does not exist.");

        return systemUserId;
    }

    public IActionResult Index()
    {
        var systemUserId = GetSystemUserId();

        var news = _context.Messages
            .Where(m => m.ReceiverID == systemUserId)
            .OrderByDescending(m => m.SentAt)
            .ToList();

        return View(news);
    }


    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Message message)
    {
        if (!ModelState.IsValid)
            return View(message);

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
            return Unauthorized();

        message.SenderID = currentUser.Id;        
        message.ReceiverID = GetSystemUserId();   
        message.SentAt = DateTime.Now;
        message.IsRead = true;

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }





    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var systemUserId = GetSystemUserId();

        var news = _context.Messages
            .FirstOrDefault(m => m.MessageID == id &&
                                 m.ReceiverID == systemUserId);

        if (news == null)
            return NotFound();

        _context.Messages.Remove(news);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

}




