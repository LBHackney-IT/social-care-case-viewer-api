using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<CfsAllocationSet> CfsAllocations { get; set; }
        public DbSet<AscAllocationSet> AscAllocations { get; set; }
        public DbSet<PersonIdLookup> PersonLookups { get; set; }
    }
}
