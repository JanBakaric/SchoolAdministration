using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using AdministracijaSkole.Web.Models;
using Microsoft.EntityFrameworkCore;
using AdministracijaSkole.DAL;

namespace AdministracijaSkole.Web.Controllers;

public class HomeController(
    ILogger<HomeController> _logger, SchoolManagerDbContext _context) : Controller
{
    public IActionResult Index()
    {
        var systemUserId = _context.Users
            .Where(u => u.UserName == SystemUsers.UserName)
            .Select(u => u.Id)
        .First();

        var news = _context.Messages
            .Where(m => m.ReceiverID == systemUserId)
            .OrderByDescending(m => m.SentAt)
            .ToList();

        return View(news);
    }


    public IActionResult Privacy(string lang)
    {
        ViewData["Message"] = "Your application description page. Language = " + lang;
        return View();
    }

    public IActionResult FAQ(int? selected = null)
    {
        ViewData["selected"] = selected;

        return View();
    }

    public IActionResult Contact()
    {
        ViewBag.Message = "Jednostavan način proslijeđivanja poruke iz Controller -> View.";

        return View();
    }

    [HttpPost]
    public IActionResult SubmitQuery(IFormCollection formData)
    {
        var ime = formData["ime"];
        var prezime = formData["prezime"];
        var email = formData["email"];
        var poruka = formData["poruka"];
        var tip = formData["tip"];
        var newsletter = bool.Parse(formData["newsletter"].FirstOrDefault());

        var msg = "Poštovani {0} {1} ({2}), zaprimili smo Vašu poruku te će vam se netko ubrzo javiti. Sadržaj vaše poruke je: [{3}] {4}." +
            " Također, {5} o daljnjim promjenama preko newslettera.";

        ViewBag.Message = string.Format(msg,
            ime, prezime,
            email,
            tip, poruka,
            newsletter ? "obavijestit ćemo Vas" : "nećemo Vas obavijestiti");

        return View("ContactSuccess");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
