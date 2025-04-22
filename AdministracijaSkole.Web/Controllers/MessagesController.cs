using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdministracijaSkole.Web.Controllers;

//[Route("Api/Messages"), ApiController]
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


[Authorize]
public class MessagesController : Controller
{
    private readonly SchoolManagerDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public MessagesController(SchoolManagerDbContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Send()
    {
        ViewBag.Users = new SelectList(_context.Users.ToList(), "Id", "UserName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string receiverId, string subject, string body)
    {
        if (string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
        {
            ViewBag.ErrorMessage = "All fields are required.";
            ViewBag.Users = new SelectList(_context.Users.ToList(), "Id", "UserName");
            return View();
        }

        var sender = await _userManager.GetUserAsync(User);


        var message = new Message
        {
            SenderID = sender.Id,
            ReceiverID = receiverId,
            Subject = subject,
            Body = body,
            SentAt = DateTime.Now
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction("Inbox");
    }

    public async Task<IActionResult> Inbox()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var messages = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ReceiverID == user.Id)
            .OrderByDescending(m => m.SentAt)
            .ToList();

        var messageViewModels = new List<MessageViewModel>();

        foreach (var message in messages)
        {
            var senderRole = await GetUserRole(message.SenderID);
            var receiverRole = await GetUserRole(message.ReceiverID);

            messageViewModels.Add(new MessageViewModel
            {
                Id = message.MessageID,
                SenderId = message.SenderID,
                SenderUserName = message.Sender.UserName,
                SenderRole = senderRole,
                ReceiverId = message.ReceiverID,
                ReceiverUserName = message.Receiver.UserName,
                ReceiverRole = receiverRole,
                Subject = message.Subject,
                Body = message.Body,
                SentAt = message.SentAt
            }); ;
        }

        return View(messageViewModels); 
    }

    public async Task<IActionResult> Sent()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var sentMessages = _context.Messages
            .Include(m => m.Receiver)
            .Where(m => m.SenderID == user.Id)
            .OrderByDescending(m => m.SentAt)
            .ToList();

        var sentMessageViewModels = new List<MessageViewModel>();

        foreach (var message in sentMessages)
        {
            var senderRole = await GetUserRole(message.SenderID);
            var receiverRole = await GetUserRole(message.ReceiverID);

            sentMessageViewModels.Add(new MessageViewModel
            {
                Id = message.MessageID,
                SenderId = message.SenderID,
                SenderUserName = message.Sender.UserName,
                SenderRole = senderRole,
                ReceiverId = message.ReceiverID,
                ReceiverUserName = message.Receiver.UserName,
                ReceiverRole = receiverRole,
                Subject = message.Subject,
                Body = message.Body,
                SentAt = message.SentAt
            });
        }

        return View(sentMessageViewModels); 
    }

    public async Task<IActionResult> View(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account"); 
        }

        var message = await _context.Messages
            .Include(m => m.Sender)  
            .Include(m => m.Receiver) 
            .FirstOrDefaultAsync(m => m.MessageID == id); 

        if (message == null)
        {
            return NotFound(); 
        }

        bool isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
        if (message.SenderID != user.Id && message.ReceiverID != user.Id && !isAdmin)
        {
            return RedirectToAction("AccessDenied", "Messages");
        }

        var senderRole = await GetUserRole(message.SenderID);
        var receiverRole = await GetUserRole(message.ReceiverID);

        var messageViewModel = new MessageViewModel
        {
            Id = message.MessageID,
            SenderId = message.SenderID,
            SenderUserName = message.Sender.UserName,
            SenderRole = senderRole,
            ReceiverId = message.ReceiverID,
            ReceiverUserName = message.Receiver.UserName,
            ReceiverRole = receiverRole,
            Subject = message.Subject,
            Body = message.Body,
            SentAt = message.SentAt
        };

        return View(messageViewModel);
    }



    private async Task<string> GetUserRole(string userID)
    {
        var user = await _userManager.FindByIdAsync(userID);
        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? "No Role Assigned"; 
    }

    public IActionResult AccessDenied()
    {
        return View(); 
    }
}
