using Microsoft.EntityFrameworkCore;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    //TODO: add tests
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<AllocationSet> Allocations { get; set; }
        public DbSet<PersonIdLookup> PersonLookups { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<PersonOtherName> PersonOtherNames { get; set; }
        public DbSet<PhoneNumber> PhoneNumbers { get; set; }
    }
}
