using Microsoft.EntityFrameworkCore;
using API_Usage.Models;

namespace API_Usage.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<ChartElement> ChartElements { get; set; }
        public DbSet<Financial> Financials { get; set; }
        public DbSet<KeyStat> KeyStats { get; set; }
        public DbSet<Dividend> Dividends { get; set; }
    }
}