using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class SaveRequestAudit : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private readonly Mock<IProcessDataGateway> _mockProcessDataGateway = new Mock<IProcessDataGateway>();
        private readonly Mock<ISystemTime> _mockSystemTime = new Mock<ISystemTime>();
        private Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _databaseGateway = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object, _mockSystemTime.Object);
        }

        [Test]
        public void CreatesRequestAudit()
        {
            var metadata = new Dictionary<string, object>() { { "residentId", 333 }, { "caseNoteId", 555 } };

            CreateRequestAuditRequest requestAuditRequest = new CreateRequestAuditRequest()
            {
                ActionName = "view_resident",
                UserName = _faker.Person.Email,
                Metadata = metadata
            };

            _databaseGateway.CreateRequestAudit(requestAuditRequest);

            var requestAudit = DatabaseContext.RequestAudits.FirstOrDefault();

            requestAudit?.ActionName.Should().Be(requestAuditRequest.ActionName);
            requestAudit?.UserName.Should().Be(requestAuditRequest.UserName);
            requestAudit?.Timestamp.Should().BeCloseTo(DateTime.Now, precision: 1000);
            requestAudit?.Metadata.Should().BeEquivalentTo(requestAuditRequest.Metadata);
        }
    }
}
