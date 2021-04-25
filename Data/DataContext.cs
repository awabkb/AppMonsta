using IMK_web.Models;
using Microsoft.EntityFrameworkCore;

namespace IMK_web.Data
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base (options){}

        public DbSet<User> Users { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<SiteVisit> SiteVisits { get; set; }
        public DbSet<IMK_Functions> IMK_Functions { get; set; }

        public DbSet<Country> Countries {get;set;}
        public DbSet<Operator> Operators {get;set;}
        public DbSet<AspCompany> AspCompanies {get;set;}
        public DbSet<ImkVersion> ImkVersions { get; set; }
        public DbSet<Log> Logs { get; set; }


    }
}