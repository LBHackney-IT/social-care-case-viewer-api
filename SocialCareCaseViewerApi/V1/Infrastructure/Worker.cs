using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_worker", Schema = "dbo")]
    public class Worker : IAuditEntity
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public int Id { get; set; }

        [Column("email")]
        [MaxLength(62)]
        [Required]
        public string Email { get; set; } = null!;

        [Column("first_name")]
        [MaxLength(100)]
        [Required]
        public string FirstName { get; set; } = null!;

        [Column("last_name")]
        [MaxLength(100)]
        [Required]
        public string LastName { get; set; } = null!;

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("role")]
        [MaxLength(200)]
        public string? Role { get; set; }

        [Column("context_flag")]
        [MaxLength(1)]
        public string? ContextFlag { get; set; }

        [Column("created_by")]
        [MaxLength(62)]
        public string? CreatedBy { get; set; }

        [Column("date_start")] public DateTime? DateStart { get; set; }

        [Column("date_end")] public DateTime? DateEnd { get; set; }


        [Column("last_modified_by")]
        public string? LastModifiedBy { get; set; }

        // save changes override populates created at and lost modified at

        [Column("created_at")] public DateTime? CreatedAt { get; set; }

        [Column("last_modified_at")] public DateTime? LastModifiedAt { get; set; }

        //nav props
        public ICollection<WorkerTeam>? WorkerTeams { get; set; }

        public ICollection<AllocationSet>? Allocations { get; set; }

        public Worker ShallowCopy()
        {
            return (Worker) this.MemberwiseClone();
        }
    }
}
