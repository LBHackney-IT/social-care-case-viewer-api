using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using Microsoft.EntityFrameworkCore;
using Moq;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Helpers;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests
{
    [TestFixture]
    public class GetCaseStatusesByPersonIdTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway;
        private Mock<ISystemTime> _mockSystemTime;

        [SetUp]
        public void Setup()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext, _mockSystemTime.Object);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void WhenNoMatchingIDReturnsEmptyList()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var response = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            response.Should().BeEmpty();
        }

        [Test]
        public void WhenMatchingIDReturnsCaseStatuses()
        {
            var (caseStatus, person, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var response = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            response.Count.Should().Be(1);
            var responseElement = response.First();

            CaseStatusHelper.TrimMilliseconds(responseElement.StartDate).Should().Be(CaseStatusHelper.TrimMilliseconds(caseStatus.ToDomain().StartDate));
            CaseStatusHelper.TrimMilliseconds(responseElement.EndDate).Should().Be(CaseStatusHelper.TrimMilliseconds(caseStatus.ToDomain().EndDate));
            responseElement.Id.Should().Be(caseStatus.ToDomain().Id);
            responseElement.Type.Should().Be(caseStatus.ToDomain().Type);
            //responseElement.Answers.Count.Should().Be(answers.Count);

            //TODO: better way of handling this?
            //foreach(var a in responseElement.Answers)
            //{
            //    a.CreatedAt = CaseStatusHelper.TrimMilliseconds(a.CreatedAt).Value;
            //    a.StartDate = CaseStatusHelper.TrimMilliseconds(a.StartDate).Value;
            //}

            //foreach (var a in caseStatus.Answers)
            //{
            //    a.CreatedAt = CaseStatusHelper.TrimMilliseconds(a.CreatedAt).Value;
            //    a.StartDate = CaseStatusHelper.TrimMilliseconds(a.StartDate).Value;
            //}

            //responseElement.Answers.Should().BeEquivalentTo(caseStatus.ToDomain().Answers);
        }

        [Test]
        public void WhenMatchingIDAndPastCaseStatusReturnEmptyList()
        {
            var (_, person) = CaseStatusHelper.SavePersonWithPastCaseStatusToDatabase(DatabaseContext);

            var response = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            response.Should().BeEmpty();
        }

        [Test]
        public void WhenMatchingIDReturnsActiveCaseStatusesWhenMultiple()
        {
            var (_, person) = CaseStatusHelper.SavePersonWithMultipleCaseStatusToDatabase(DatabaseContext);

            var response = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            response.Count.Should().Be(2);
        }
    }
}
