using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListAllocationsRequest
    {
        [ListAllocationsRequestValidator]
        [FromQuery(Name = "mosaic_id")]
        public long MosaicId { get; set; }

        [FromQuery(Name = "worker_id")]
        public long WorkerId { get; set; }
    }

    public class ListAllocationsRequestValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var request = (ListAllocationsRequest) validationContext.ObjectInstance;
            var mosaicId = request.MosaicId;
            var workerId = request.WorkerId;

            if (mosaicId == 0 && workerId == 0) //missing parameter will result in value 0
            {
                return new ValidationResult($"Please provide either mosaic_id or worker_id");
            }
            else if(mosaicId != 0 && workerId != 0)
            {
                return new ValidationResult($"Please provide only mosaic_id or worker_id, but not both");
            }

            return ValidationResult.Success;
        }
    }
}
