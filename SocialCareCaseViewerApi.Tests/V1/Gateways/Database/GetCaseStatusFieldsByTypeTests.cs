using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetCaseStatusFieldsByType : DatabaseTests
    {
        private ICaseStatusGateway _caseStatusGateway;

        [SetUp]
        public void Setup()
        {
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void WhenNoFieldsExistForType()
        {
            var response = _caseStatusGateway.GetCaseStatusTypeWithFields("non-existent");

            response.Should().BeNull();
        }

        [Test]
        public void WhenASingleFieldExistsForType()
        {
            var caseStatusType = DatabaseGatewayTests.GetValidCaseStatusTypeWithFields("Test");
            DatabaseContext.CaseStatusTypes.Add(caseStatusType);
            DatabaseContext.SaveChanges();

            var response = _caseStatusGateway.GetCaseStatusTypeWithFields("Test");

            response.Name.Should().Be("Test");
            response.Fields.First().Name.Should().Be("someThing");
            response.Fields.First().Options.First().Name.Should().Be("One");
            response.Fields.First().Options.Last().Name.Should().Be("Two");
        }
    }
}
