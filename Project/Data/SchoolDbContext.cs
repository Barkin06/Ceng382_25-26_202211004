using Microsoft.EntityFrameworkCore;
using Week2.Models;

namespace Week2.Data
{
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options) {}

        public DbSet<Class> Classes { get; set; }
    }
}
