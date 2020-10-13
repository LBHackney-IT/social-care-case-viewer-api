using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IExampleGateway
    {
        Entity GetEntityById(long id);

        List<Entity> GetAll();
    }
}
