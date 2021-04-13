using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IGetVisitByVisitIdUseCase
    {
        Visit Execute(long id);
    }
}
