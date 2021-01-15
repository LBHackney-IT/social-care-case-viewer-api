using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_allocations", Schema = "dbo")]
    public class AllocationSet
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("mosaic_id")]
        [MaxLength(16)]
        public string MosaicId { get; set; }

        [Column("first_name")]
        [MaxLength(30)]
        public string FirstName { get; set; }

        [Column("last_name")]
        [MaxLength(30)]
        public string LastName { get; set; }

        [Column("full_name")]
        [MaxLength(62)]
        public string FullName { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("gender")]
        [MaxLength(1)]
        public string Gender { get; set; }

        [Column("group_id")]
        [MaxLength(16)]
        public long? GroupId { get; set; }

        [Column("ethnicity")]
        [MaxLength(33)]
        public string Ethnicity { get; set; }

        [Column("sub_ethnicity")]
        [MaxLength(33)]
        public string SubEthnicity { get; set; }

        [Column("religion")]
        [MaxLength(30)]
        public string Religion { get; set; }

        [Column("service_user_group")]
        [MaxLength(30)]
        public string ServiceUserGroup { get; set; }

        [Column("school_name")]
        [MaxLength(255)]
        public string SchoolName { get; set; }

        [Column("school_address")]
        [MaxLength(255)]
        public string SchoolAddress { get; set; }

        [Column("gp_name")]
        [MaxLength(62)]
        public string GpName { get; set; }

        [Column("gp_address")]
        [MaxLength(150)]
        public string GpAddress { get; set; }

        [Column("gp_surgery")]
        [MaxLength(100)]
        public string GpSurgery { get; set; }

        [Column("allocated_worker")]
        [MaxLength(90)]
        public string AllocatedWorker { get; set; }

        [Column("worker_type")]
        [MaxLength(100)]
        public string WorkerType { get; set; }

        [Column("allocated_worker_team")]
        [MaxLength(50)]
        public string AllocatedWorkerTeam { get; set; }

        [Column("team_name")]
        [MaxLength(50)]
        public string TeamName { get; set; }

        [Column("allocation_start_date")]
        public DateTime? AllocationStartDate { get; set; }

        [Column("allocation_end_date")]
        public DateTime? AllocationEndDate { get; set; }

        [Column("legal_status")]
        [MaxLength(255)]
        public string LegalStatus { get; set; }

        [Column("placement")]
        [MaxLength(255)]
        public string Placement { get; set; }

        [Column("on_cp_register")]
        [MaxLength(3)]
        public string OnCpRegister { get; set; }

        [Column("contact_address")]
        [MaxLength(255)]
        public string ContactAddress { get; set; }

        [Column("case_status_open_closed")]
        [MaxLength(7)]
        public string CaseStatus { get; set; }

        [Column("closure_date_if_closed")]
        public DateTime? CaseClosureDate { get; set; }

        [Column("worker_email")]
        [MaxLength(62)]
        public string WorkerEmail { get; set; }

        [Column("lac")]
        [MaxLength(10)]
        public string LAC { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
