using AppMonsta.Models;
using Microsoft.EntityFrameworkCore;

namespace AppMonsta.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

    }
}
