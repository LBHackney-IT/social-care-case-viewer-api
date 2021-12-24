using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("organisations", Schema = "dbo")]
    public class HistoricalOrganisation
    {
        [Column("id")]
        [MaxLength(9)]
        [Required]
        [Key]
        public long Id { get; set; }

        [Column("name")]
        [StringLength(240)]
        [Required]
        public string Name { get; set; } = null!;

        [Column("referrable")]
        [StringLength(1)]
        public string? Referrable { get; set; }

        [Column("sector")]
        [StringLength(16)]
        public string? Sector { get; set; }

        [Column("sub_sector")]
        [StringLength(16)]
        public string? SubSector { get; set; }

        [Column("type")]
        [StringLength(16)]
        public string? Type { get; set; }

        [Column("approved_supplier")]
        [StringLength(1)]
        public string? ApprovedSupplier { get; set; }

        [Column("email_address")]
        [StringLength(240)]
        public string? EmailAddress { get; set; }

        [Column("available")]
        [StringLength(1)]
        public string? Available { get; set; }

        [Column("registering_authority")]
        [StringLength(240)]
        public string? RegisteringAuthority { get; set; }

        [Column("description")]
        [StringLength(2000)]
        public string? Description { get; set; }

        [Column("registration_status")]
        [StringLength(16)]
        public string? RegistrationStatus { get; set; }

        [Column("web_address")]
        [StringLength(240)]
        public string? WebAddress { get; set; }

        [Column("ac_flag")]
        [StringLength(1)]
        public string? AcFlag { get; set; }

        [Column("placement_code")]
        [StringLength(16)]
        public string? PlacementCode { get; set; }

        [Column("team_org_id")]
        [MaxLength(9)]
        public long? TeamOrgId { get; set; }

        [Column("created_on")]
        public DateTime? CreatedOn { get; set; }

        [Column("created_by")]
        [StringLength(30)]
        public string? CreatedBy { get; set; }

        [Column("created_acting_for")]
        [MaxLength(30)]
        public string? CreatedActingFor { get; set; }

        [Column("updated_by")]
        [StringLength(30)]
        public string? UpdatedBy { get; set; }

        [Column("updated_acting_for")]
        [StringLength(30)]
        public string? UpdatedActingFor { get; set; }

        [Column("updated_on")]
        public DateTime? UpdatedOn { get; set; }

        [Column("purchaser_flag")]
        [StringLength(1)]
        public string? PurchaserFlag { get; set; }

        [Column("organisation_notes")]
        public string? OrganisationNotes { get; set; }

        [Column("department")]
        [StringLength(240)]
        public string? Department { get; set; }

        [Column("ward_specific")]
        [StringLength(1)]
        public string? WardSpecific { get; set; }

        [Column("responsible_authority")]
        [StringLength(1)]
        [Required]
        public string ResponsibleAuthority { get; set; } = null!;
    }
}
