using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AdministracijaSkole.Web.Controllers;


[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
	private readonly SchoolManagerDbContext _context;
	//private static bool _isLoggingDisabled = false; 
	//private static DateTime _loggingDisabledUntil = DateTime.MinValue;

	public AdminController(SchoolManagerDbContext context)
	{
		_context = context;
	}

	public IActionResult Index(int page = 1, int pageSize = 10)
	{
		var logs = _context.Logs
			.OrderByDescending(log => log.Timestamp)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		var totalLogs = _context.Logs.Count();
		ViewBag.TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize);
		ViewBag.CurrentPage = page;

		return View(logs);
	}

	[HttpPost]
	public IActionResult FilterLogs(DateTime? startDate, DateTime? endDate, string logLevel, int page = 1, int pageSize = 10)
	{
		var query = _context.Logs.AsQueryable();

		if (!string.IsNullOrEmpty(logLevel))
		{
			query = query.Where(log => log.LogLevel.ToString() == logLevel);
		}

		if (startDate.HasValue)
		{
			query = query.Where(log => log.Timestamp >= startDate.Value);
		}

		if (endDate.HasValue)
		{
			query = query.Where(log => log.Timestamp <= endDate.Value);
		}

		var filteredLogs = query
			.OrderByDescending(log => log.Timestamp)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		var totalLogs = query.Count();
		ViewBag.TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize);
		ViewBag.CurrentPage = page;

		return View("Index", filteredLogs);
	}

	[HttpPost]
	public IActionResult ClearLogs()
	{
		try
		{
			var logs = _context.Logs.ToList();
			_context.Logs.RemoveRange(logs);
			_context.SaveChanges();

			TempData["Sucesss"] = "All logs have been cleared successfully.";

		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = "An error occurred while clearing logs: " + ex.Message;
		}

		return RedirectToAction("Index");
	}


	[HttpPost]
	public IActionResult ExportLogsToText(DateTime? startDate, DateTime? endDate, string logLevel)
	{
		var query = _context.Logs.AsQueryable();

		if (!string.IsNullOrEmpty(logLevel))
		{
			query = query.Where(log => log.LogLevel.ToString() == logLevel);
		}

		if (startDate.HasValue)
		{
			query = query.Where(log => log.Timestamp >= startDate.Value);
		}

		if (endDate.HasValue)
		{
			query = query.Where(log => log.Timestamp <= endDate.Value);
		}

		var filteredLogs = query.OrderByDescending(log => log.Timestamp).ToList();

		var sb = new StringBuilder();
		sb.AppendLine("Log Number\tTimestamp\tLog Level\tMessage");
		sb.AppendLine("-------------------------------------------------------------");

		int logIndex = 1;
		foreach (var log in filteredLogs)
		{
			sb.AppendLine($"{logIndex}. {log.Timestamp}\t{log.LogLevel}\t{log.Message}");
			sb.AppendLine();
			logIndex++;
		}

		var fileContent = Encoding.UTF8.GetBytes(sb.ToString());

		return File(fileContent, "text/plain", "logs.txt");
	}
}
