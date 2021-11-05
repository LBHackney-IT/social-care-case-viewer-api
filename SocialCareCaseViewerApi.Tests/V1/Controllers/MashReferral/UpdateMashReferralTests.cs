using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Controllers.MashReferral
{
    [TestFixture]
    public class UpdateMashReferralTests
    {
        private readonly Faker _faker = new Faker();
        private MashReferralController _mashReferralController = null!;
        private Mock<IMashReferralUseCase> _mashReferralUseCase = null!;
        private string _fakeReferralId = null!;

        [SetUp]
        public void Setup()
        {
            _mashReferralUseCase = new Mock<IMashReferralUseCase>();
            _mashReferralController = new MashReferralController(_mashReferralUseCase.Object);
            _fakeReferralId = _faker.Random.String2(20, "0123456789abcdef");
        }

        [Test]
        public void OnSuccessReturnUpdatedReferralFromMashReferralUseCaseUpdateMashReferral()
        {
            var request = TestHelpers.CreateUpdateMashReferral();
            var updatedReferral = TestHelpers.CreateMashReferral().ToDomain().ToResponse();
            _mashReferralUseCase.Setup(x => x.UpdateMashReferral(request, _fakeReferralId)).Returns(updatedReferral);

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(updatedReferral);
        }

        [Test]
        public void WhenMashReferralUseCaseUpdateMashReferralThrowsMashReferralNotFoundExceptionReturnBadRequest()
        {
            var request = TestHelpers.CreateUpdateMashReferral();
            const string errorMessage = "test-error-message";
            _mashReferralUseCase
                .Setup(x => x.UpdateMashReferral(request, _fakeReferralId))
                .Throws(new MashReferralNotFoundException(errorMessage));

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void WhenMashReferralUseCaseUpdateMashReferralThrowsWorkerNotFoundExceptionReturnBadRequest()
        {
            var request = TestHelpers.CreateUpdateMashReferral();
            const string errorMessage = "test-error-message";
            _mashReferralUseCase
                .Setup(x => x.UpdateMashReferral(request, _fakeReferralId))
                .Throws(new WorkerNotFoundException(errorMessage));

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void WhenMashReferralUseCaseUpdateMashReferralThrowsMashReferralStageMismatchExceptionReturnBadRequest()
        {
            var request = TestHelpers.CreateUpdateMashReferral();
            const string errorMessage = "test-error-message";
            _mashReferralUseCase
                .Setup(x => x.UpdateMashReferral(request, _fakeReferralId))
                .Throws(new MashReferralStageMismatchException(errorMessage));

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void ValidationReturnsBadRequestIfUpdateTypeIsScreeningDecisionAndNullDecisionProvided()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");
            request.Decision = null;

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Must provide a decision");
        }

        [Test]
        public void ValidationReturnsBadRequestIfUpdateTypeIsScreeningDecisionAndEmptyDecisionProvided()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");
            request.Decision = "";

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Must provide a decision");
        }

        [Test]
        public void ValidationReturnsBadRequestIfUpdateTypeIsScreeningDecisionAndNoUrgentContactOptionSelected()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");
            request.RequiresUrgentContact = null;

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Must provide if urgent contact is required");
        }

        [Test]
        public void ValidationReturnsBadRequestIfUpdateTypeIsInitialDecisionAndNullDecisionProvided()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            request.Decision = null;

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Must provide a decision");
        }

        [Test]
        public void ValidationReturnsBadRequestIfUpdateTypeIsInitialDecisionAndEmptyDecisionProvided()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            request.Decision = "";

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Must provide a decision");
        }

        [Test]
        public void ValidationReturnsBadRequestIfUpdateTypeIsInitialDecisionAndNoUrgentContactOptionSelected()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            request.RequiresUrgentContact = null;

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Must provide if urgent contact is required");
        }

        [Test]
        public void ValidationReturnsBadRequestIfUpdateTypeIsInitialDecisionAndNoReferralCategorySelected()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            request.ReferralCategory = null;

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Must provide a referral category");
        }

        [Test]
        public void ValidationReturnsBadRequestIfUpdateTypeIsInitialDecisionAndReferralCategoryIsEmpty()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            request.ReferralCategory = "";

            var response = _mashReferralController.UpdateMashReferral(request, _fakeReferralId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Must provide a referral category");
        }
    }
}
