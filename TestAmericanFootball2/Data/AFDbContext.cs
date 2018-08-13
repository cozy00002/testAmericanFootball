using Microsoft.EntityFrameworkCore;

namespace TestAmericanFootball2.Models
{
    public class AFDbContext : DbContext
    {
        public AFDbContext(DbContextOptions<AFDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movie { get; set; }
        public DbSet<Game> Game { get; set; }
    }
}