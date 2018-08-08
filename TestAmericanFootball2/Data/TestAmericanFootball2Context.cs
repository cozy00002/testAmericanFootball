using Microsoft.EntityFrameworkCore;

namespace TestAmericanFootball2.Models
{
    public class TestAmericanFootball2Context : DbContext
    {
        public TestAmericanFootball2Context(DbContextOptions<TestAmericanFootball2Context> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movie { get; set; }
        public DbSet<Game> Game { get; set; }
    }
}