using FinalPPE.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalPPE.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Trofeo> Trofeos => Set<Trofeo>();
}