using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers.MashReferral
{
    [TestFixture]
    public class CreateMashReferralTests
    {
        private Mock<IMashReferralUseCase> _mashReferralUseCase;
        private MashReferralController _mashReferralController;

        [SetUp]
        public void Setup()
        {
            _mashReferralUseCase = new Mock<IMashReferralUseCase>();
            _mashReferralController = new MashReferralController(_mashReferralUseCase.Object);
        }

        [Test]
        public void CallsMashReferralUseCaseToInsertNewReferral()
        {
            _mashReferralUseCase.Setup(x => x.InsertMashReferral(It.IsAny<CreateReferralRequest>()));
            var request = TestHelpers.GenerateCreateReferralRequest();

            _mashReferralController.CreateNewContact(request);

            _mashReferralUseCase.Verify(x => x.InsertMashReferral(request));
        }
    }
}
