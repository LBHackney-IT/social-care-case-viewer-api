using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests
{
    [TestFixture]
    public class CreateCaseStatusTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway;
        private Mock<ISystemTime> _mockSystemTime;
        private Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext, _mockSystemTime.Object);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void CreatesACaseStatus()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();
            var request = CaseStatusHelper.CreateCaseStatusRequest(personId: person.Id);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.FirstOrDefault();

            caseStatus.Notes.Should().Be(request.Notes);
            caseStatus.PersonId.Should().Be(person.Id);
            caseStatus.Type.Should().Be(request.Type);
            caseStatus.StartDate.Should().Be(request.StartDate);
            caseStatus.EndDate.Should().BeNull();
        }

        [Test]
        public void CreatesACaseStatusWithoutAnswers()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var request = CaseStatusHelper.CreateCaseStatusRequest(personId: person.Id, answers: new List<CaseStatusRequestAnswers>());

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault();

            caseStatus?.Answers.Count.Should().Be(0);
        }

        [Test]
        public void CreatesACaseStatusWithAnswers()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var answersOne = new CaseStatusRequestAnswers()
            {
                Option = _faker.Random.String2(10),
                Value = _faker.Random.String2(2)
            };

            var answersTwo = new CaseStatusRequestAnswers()
            {
                Option = _faker.Random.String2(10),
                Value = _faker.Random.String2(2)
            };

            var requestAnswers = new List<CaseStatusRequestAnswers>() { answersOne, answersTwo };

            var request = CaseStatusHelper.CreateCaseStatusRequest(personId: person.Id, answers: requestAnswers);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault();

            caseStatus.Answers.Count.Should().Be(2);
            caseStatus.Answers.Any(a => a.Option == answersOne.Option).Should().Be(true);
            caseStatus.Answers.Any(a => a.Option == answersTwo.Option).Should().Be(true);
            caseStatus.Answers.Any(a => a.Value == answersOne.Value).Should().Be(true);
            caseStatus.Answers.Any(a => a.Value == answersTwo.Value).Should().Be(true);
            caseStatus.Answers[0].GroupId.Should().NotBe(null);
            caseStatus.Answers[1].GroupId.Should().NotBe(null);
            caseStatus.Answers[1].GroupId.Should().Equals(caseStatus.Answers[0].GroupId);
            caseStatus.Notes.Should().Be(request.Notes);
            caseStatus.PersonId.Should().Be(person.Id);
            caseStatus.Type.Should().Be(request.Type);
            caseStatus.StartDate.Should().Be(request.StartDate);
            caseStatus.EndDate.Should().BeNull();
        }

        [Test]
        public void SetsStartDateToProvidedDateTime()
        {
            var fakeTime = new DateTime(2000, 1, 1, 15, 30, 0);

            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var requestField = new List<CaseStatusRequestAnswers>() { new CaseStatusRequestAnswers() { Option = _faker.Random.String2(10), Value = _faker.Random.String2(2) } };
            var request = CaseStatusHelper.CreateCaseStatusRequest(personId: person.Id, answers: requestField, startDate: fakeTime);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.FirstOrDefault();
            caseStatus?.StartDate.Should().Be(fakeTime);
        }

        [Test]
        public void UpdatesCreatedAtAndCreatedByAuditPropertiesForTheCaseStatus()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var requestField = new List<CaseStatusRequestAnswers> { new CaseStatusRequestAnswers() { Option = _faker.Random.String2(10), Value = _faker.Random.String2(2) } };
            var request = CaseStatusHelper.CreateCaseStatusRequest(person.Id, answers: requestField);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.FirstOrDefault();
            caseStatus?.CreatedAt.Should().NotBeNull();
            caseStatus?.CreatedBy.Should().Be(request.CreatedBy);
        }

        [Test]
        public void UpdatesCreatedAtAndCreatedByAuditPropertiesForTheAnswers()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var requestField = new List<CaseStatusRequestAnswers> { new CaseStatusRequestAnswers() { Option = _faker.Random.String2(10), Value = _faker.Random.String2(2) } };
            var request = CaseStatusHelper.CreateCaseStatusRequest(person.Id, answers: requestField);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault();

            caseStatus.Answers.First().CreatedAt.Should().NotBeNull();
            caseStatus.Answers.First().CreatedBy.Should().Be(request.CreatedBy);
        }

        [Test]
        public void CreatesACaseStatusWithoutAnswersShouldPassIfNoOverlappingCaseStatusExists()
        {
            var (_, person) = CaseStatusHelper.SavePersonWithPastCaseStatusToDatabase(DatabaseContext);

            var request = CaseStatusHelper.CreateCaseStatusRequest(personId: person.Id, answers: new List<CaseStatusRequestAnswers>(), startDate: DateTime.Today);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.OrderByDescending(cs => cs.StartDate).FirstOrDefault();

            caseStatus.EndDate.Should().BeNull();
            caseStatus.StartDate.Should().Be(DateTime.Today);
        }
    }
}
