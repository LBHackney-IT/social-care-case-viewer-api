using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListAllocationsRequest
    {
        [FromQuery(Name = "mosaic_id")]
        public long MosaicId { get; set; }

        [FromQuery(Name = "worker_id")]
        public long WorkerId { get; set; }

        [FromQuery(Name = "worker_email")]
        public string WorkerEmail { get; set; }

        [FromQuery(Name = "team_id")]
        public long TeamId { get; set; }

        [FromQuery(Name = "status")]
        public string? Status { get; set; }

        [FromQuery(Name = "team_allocation_status")]
        public string? TeamAllocationStatus { get; set; }
    }

    public class ListAllocationsRequestValidator : AbstractValidator<ListAllocationsRequest>
    {
        public ListAllocationsRequestValidator()
        {
            RuleFor(x => x)
                .Must(OneIsSet)
                .WithMessage("Please provide either mosaic_id, worker_id, worker_email or team_id");

            RuleFor(x => x)
                .Must(OnlyOneIsSet)
                .WithMessage("Please provide only one of mosaic_id, worker_id, worker_email or team_id");

            When(x => !String.IsNullOrEmpty(x.WorkerEmail), () =>
            {
                RuleFor(x => x.WorkerEmail)
                    .EmailAddress()
                    .WithMessage("Please provide a valid email address for worker_email");
            });
        }

        private bool OnlyOneIsSet(ListAllocationsRequest request)
        {
            return new List<int>
            {
                request.MosaicId == 0 ? 0 : 1,
                request.WorkerId == 0 ? 0 : 1,
                request.TeamId == 0 ? 0 : 1,
                String.IsNullOrEmpty(request.WorkerEmail) ? 0 : 1,
            }.Sum() <= 1;
        }

        private bool OneIsSet(ListAllocationsRequest request)
        {
            return !(request.MosaicId == 0 && request.WorkerId == 0 && String.IsNullOrEmpty(request.WorkerEmail) && request.TeamId == 0);
        }
    }
}
