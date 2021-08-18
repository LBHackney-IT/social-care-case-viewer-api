using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetCaseStatusFieldsByType : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private Mock<IProcessDataGateway> _mockProcessDataGateway = new Mock<IProcessDataGateway>();
        private Mock<ISystemTime> _mockSystemTime;

        [SetUp]
        public void Setup()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _databaseGateway =
                new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object, _mockSystemTime.Object);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void WhenNoFieldsExistForType()
        {
            var response = _databaseGateway.GetCaseStatusFieldsByType("non-existent");

            response.Should().BeEmpty();
        }

        [Test]
        public void WhenASingleFieldExistsForType()
        {
            var caseStatusType = new CaseStatusType() { Name = "Test", Description = "Test Type" };
            DatabaseContext.CaseStatusTypes.Add(caseStatusType);

            var caseStatusTypeFields = DatabaseGatewayTests.GetValidCaseStatusTypeFields(caseStatusType);
            foreach (CaseStatusTypeField caseStatusTypeField in caseStatusTypeFields)
            {
                DatabaseContext.CaseStatusTypeFields.Add(caseStatusTypeField);
            }
            DatabaseContext.SaveChanges();

            var response = _databaseGateway.GetCaseStatusFieldsByType("Test");

            response.First().Type.Name.Should().Be("Test");
            response.First().Name.Should().Be("someThing");
            response.First().Options.First().Name.Should().Be("One");
            response.First().Options.Last().Name.Should().Be("Two");
        }
    }
}
