using MedicalAtention.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalAtention.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.IdentificationNumber)
            .IsUnique();
    }
}
