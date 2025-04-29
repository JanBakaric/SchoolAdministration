using AdministracijaSkole.DAL;
using AdministracijaSkole.Model;
using AdministracijaSkole.Web.Models;
using AdministracijaSkole.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<SchoolManagerDbContext>(options =>
    options.UseSqlServer(
        builder
            .Configuration
            .GetConnectionString("SchoolManagerDbContext"),
        opt => opt.MigrationsAssembly("AdministracijaSkole.DAL")
    )
);

builder.Services.AddLogging(logging =>
{
    logging.AddConsole(); // Optional, add default loggers
    logging.AddProvider(new DatabaseLoggerProvider(builder.Services.BuildServiceProvider()));
});

builder.Services.AddIdentity<AppUser, AppRole>()
    .AddRoleManager<RoleManager<AppRole>>()
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<SchoolManagerDbContext>();

builder.Services.AddControllersWithViews()
	.AddRazorRuntimeCompilation();



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var supportedCultures = new[]
{
    new CultureInfo("hr"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.MapControllerRoute(
    name: "stvaranje",
    pattern: "stvaranje",
    defaults: new { controller = "Student", action = "Create" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
