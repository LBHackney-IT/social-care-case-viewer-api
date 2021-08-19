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
            var response = _databaseGateway.GetCaseStatusTypeWithFields("non-existent");

            response.Should().BeNull();
        }

        [Test]
        public void WhenASingleFieldExistsForType()
        {
            var caseStatusType = DatabaseGatewayTests.GetValidCaseStatusTypeWithFields("Test");
            DatabaseContext.CaseStatusTypes.Add(caseStatusType);
            DatabaseContext.SaveChanges();

            var response = _databaseGateway.GetCaseStatusTypeWithFields("Test");

            response.Name.Should().Be("Test");
            response.Fields.First().Name.Should().Be("someThing");
            response.Fields.First().Options.First().Name.Should().Be("One");
            response.Fields.First().Options.Last().Name.Should().Be("Two");
        }
    }
}
