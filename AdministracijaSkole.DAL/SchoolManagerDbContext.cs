﻿using AdministracijaSkole.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdministracijaSkole.DAL;

// ps naredbe:
// dotnet ef migrations add Logs --startup-project ../AdministracijaSkole.Web --context SchoolManagerDbContext
// dotnet ef database update --startup-project ../AdministracijaSkole.Web --context SchoolManagerDbContext

public class SchoolManagerDbContext(
    DbContextOptions<SchoolManagerDbContext> options
) : IdentityDbContext<AppUser, AppRole, string>(options)
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Subject> Subjects { get; set; }
	public DbSet<Class> Classes { get; set; }
	public DbSet<Professor> Professors { get; set; }
	public DbSet<Grade> Grades { get; set; }
	public DbSet<Presence> Presences { get; set; }
	public DbSet<Message> Messages { get; set; }
    public DbSet<Log> Logs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define relationship between Message and AppUser
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderID)
            .OnDelete(DeleteBehavior.Cascade); // promijeni kao .Restrict kod migriranja baze, ali nakon migracija stavi na .Cascade kako bi se useri koji imaju poruke mogli obrisati

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverID)
            .OnDelete(DeleteBehavior.Cascade); // promijeni kao .Restrict kod migriranja baze, ali nakon migracija stavi na .Cascade kako bi se useri koji imaju poruke mogli obrisati
    }
}