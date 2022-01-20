using Microsoft.EntityFrameworkCore;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class HistoricalDataContext : DbContext
    {
        public HistoricalDataContext(DbContextOptions<HistoricalDataContext> options) : base(options)
        {
        }

        public DbSet<HistoricalCaseNote> HistoricalCaseNotes { get; set; }

        public DbSet<HistoricalNoteType> HistoricalNoteTypes { get; set; }

        public DbSet<HistoricalWorker> HistoricalWorkers { get; set; }

        public DbSet<HistoricalVisit> HistoricalVisits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkerTeam>().HasKey(wt => new { wt.WorkerId, wt.TeamId });

            modelBuilder.Entity<HistoricalCaseNote>()
              .HasOne(caseNote => caseNote.CreatedByWorker)
              .WithMany(worker => worker.CaseNotes)
              .HasForeignKey(caseNote => caseNote.CreatedBy)
              .HasPrincipalKey(worker => worker.SystemUserId);

            modelBuilder.Entity<HistoricalCaseNote>()
                .HasOne(caseNote => caseNote.HistoricalNoteType)
                .WithMany(noteType => noteType.CaseNotes)
                .HasForeignKey(caseNote => caseNote.NoteType)
                .HasPrincipalKey(noteType => noteType.Type);
        }
    }
}
