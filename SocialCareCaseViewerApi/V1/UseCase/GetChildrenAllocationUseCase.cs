using SocialCareCaseViewerApi.V1.Boundary.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetChildrenAllocationUseCase : IGetChildrenAllocationUseCase
    {
        public CfsAllocationList Execute(string officerEmail, long mosaicId)
        {
            return new CfsAllocationList();
        }
    }
}
