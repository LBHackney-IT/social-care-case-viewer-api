using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    public class VisitsUseCaseTests
    {
        private Mock<IHistoricalSocialCareGateway> _mockHistoricalSocialCareGateway;
        private VisitsUseCase _visitsUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockHistoricalSocialCareGateway = new Mock<IHistoricalSocialCareGateway>();
            _visitsUseCase = new VisitsUseCase(_mockHistoricalSocialCareGateway.Object);
        }

        [Test]
        public void GetVisitsCallsHistoricalSocialCareGatewayWhenPersonIdIsUsed()
        {
            var request = new ListVisitsRequest() { Id = 1L };

            _mockHistoricalSocialCareGateway.Setup(x => x.GetVisitInformationByPersonId(It.IsAny<long>())).Returns(new List<Visit>());

            _visitsUseCase.ExecuteGetByPersonId(request.Id);

            _mockHistoricalSocialCareGateway.Verify(x => x.GetVisitInformationByPersonId(It.IsAny<long>()));
        }

        [Test]
        public void GetVisitsCallsHistoricalSocialCareGatewayWithParameterWhenPersonIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = 1L };

            _mockHistoricalSocialCareGateway.Setup(x => x.GetVisitInformationByPersonId(Convert.ToInt64(request.Id))).Returns(new List<Visit>());

            _visitsUseCase.ExecuteGetByPersonId(request.Id);

            _mockHistoricalSocialCareGateway.Verify(x => x.GetVisitInformationByPersonId(Convert.ToInt64(request.Id)));
        }
    }
}
