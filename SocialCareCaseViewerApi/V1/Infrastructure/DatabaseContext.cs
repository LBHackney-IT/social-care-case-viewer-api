using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates;

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
        public DbSet<WorkerTeam> WorkerTeams { get; set; }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<WarningNote> WarningNotes { get; set; }
        public DbSet<WarningNoteReview> WarningNoteReview { get; set; }
        public DbSet<PersonalRelationshipType> PersonalRelationshipTypes { get; set; }
        public DbSet<PersonalRelationship> PersonalRelationships { get; set; }
        public DbSet<PersonalRelationshipDetail> PersonalRelationshipDetails { get; set; }
        public DbSet<PersonRecordToBeDeleted> PersonRecordsToBeDeleted { get; set; }
        public DbSet<DeletedPersonRecord> DeletedPersonRecords { get; set; }
        public DbSet<RequestAudit> RequestAudits { get; set; }
        public DbSet<PersonImport> PersonImport { get; set; }
        public DbSet<CaseStatus> CaseStatuses { get; set; }
        public DbSet<CaseStatusFieldOption> CaseStatusFieldOptions { get; set; }
        public DbSet<CaseStatusType> CaseStatusTypes { get; set; }
        public DbSet<CaseStatusTypeField> CaseStatusTypeFields { get; set; }
        public DbSet<CaseStatusTypeFieldOption> CaseStatusTypeFieldOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkerTeam>().HasKey(wt => new { wt.WorkerId, wt.TeamId });
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var auditEntries = OnBeforeSaving();

            var result = base.SaveChanges(acceptAllChangesOnSuccess);

            OnAfterSaveChanges(auditEntries);

            return result;
        }

        //TODO: add overrides for async methods
        private List<AuditEntry> OnBeforeSaving()
        {
            ChangeTracker.DetectChanges();

            var auditEntries = new List<AuditEntry>();
            var dateTimeNow = DateTime.UtcNow; //TODO: check format

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is IAuditEntity auditEntity)
                {
                    //ignore entities we don't want to add audit records for
                    if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                        continue;

                    var auditEntry = new AuditEntry
                    {
                        TableName = entry.Metadata.GetTableName(),
                        DateTime = dateTimeNow
                    };

                    foreach (var property in entry.Properties)
                    {
                        //for PK generated after save
                        if (property.IsTemporary)
                        {
                            auditEntry.TemporaryProperties.Add(property);
                        }

                        string propertyName = property.Metadata.Name;

                        if (property.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[propertyName] = property.CurrentValue;
                            continue;
                        }

                        var createdBy = auditEntity.CreatedBy;
                        var updatedBy = auditEntity.LastModifiedBy;

                        auditEntry.EntityState = entry.State.ToString();

                        switch (entry.State)
                        {
                            case EntityState.Modified:
                                auditEntity.LastModifiedAt = dateTimeNow;
                                auditEntity.LastModifiedBy = updatedBy;

                                if (property.IsModified)
                                {
                                    auditEntry.OldValues[propertyName] = property.OriginalValue;
                                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                                }
                                break;

                            case EntityState.Added:
                                auditEntity.CreatedAt = dateTimeNow;
                                auditEntity.CreatedBy = createdBy;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                                break;

                            case EntityState.Deleted:
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                break;
                        }
                    }
                    auditEntries.Add(auditEntry);
                }
            }

            foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
            {
                Audits.Add(auditEntry.ToAudit());
            }

            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private int OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return 0;

            foreach (var auditEntry in auditEntries)
            {
                // Get the final value of the temporary properties
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                // Add the Audit entry
                Audits.Add(auditEntry.ToAudit());
            }

            return SaveChanges();
        }
    }
}
