using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers.Gateway;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class CaseStatusGatewayTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway;
        private readonly Mock<IProcessDataGateway> _mockProcessDataGateway = new Mock<IProcessDataGateway>();
        private readonly Mock<ISystemTime> _mockSystemTime = new Mock<ISystemTime>();
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _caseStatusGateway =
                new CaseStatusGateway(DatabaseContext, _mockProcessDataGateway.Object, _mockSystemTime.Object);
        }

        [Test]
        public void WhenAPersonDoesNotExist()
        {
            var caseStatuses = _caseStatusGateway.GetCaseStatusesByPersonId(0);

            caseStatuses.Should().BeEmpty();
        }

        [Test]
        public void WhenAPersonHasNoCaseStatuses()
        {
            var person = PersonGatewayHelper.StoreNewPerson(DatabaseContext);

            var caseStatuses = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            caseStatuses.Should().BeEmpty();
        }

        [Test]
        public void WhenAPersonHasACurrentCaseStatus()
        {
            var person = PersonGatewayHelper.StoreNewPerson(DatabaseContext);
            var caseStatusType = CaseStatusGatewayHelper.StoreNewCaseStatusType(DatabaseContext);
            var caseStatus =
                CaseStatusGatewayHelper.StoreNewCaseStatusForPerson(DatabaseContext, caseStatusType, person);

            var caseStatuses = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            caseStatuses.Single().Notes.Should().Be(caseStatus.Notes);
            caseStatuses.Single().StartDate.Should().Be(caseStatus.StartDate.ToString("s"));
            caseStatuses.Single().Fields.First().SelectedOption.Name.Should()
                .Be(caseStatusType.Fields.First().Options.First().Name);
            caseStatuses.Single().Fields.First().SelectedOption.Description.Should()
                .Be(caseStatusType.Fields.First().Options.First().Description);
        }

        [Test]
        public void WhenAPersonHasAPastCaseStatus()
        {
            var person = PersonGatewayHelper.StoreNewPerson(DatabaseContext);
            var caseStatusType = CaseStatusGatewayHelper.StoreNewCaseStatusType(DatabaseContext);
            CaseStatusGatewayHelper.StoreNewCaseStatusForPerson(
                DatabaseContext,
                caseStatusType,
                person,
                _faker.Date.Past()
            );

            var caseStatuses = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            caseStatuses.Should().BeEmpty();
        }

        [Test]
        public void WhenAPersonHasMultipleCurrentCaseStatuses()
        {
            var person = PersonGatewayHelper.StoreNewPerson(DatabaseContext);
            var caseStatusType = CaseStatusGatewayHelper.StoreNewCaseStatusType(DatabaseContext);
            CaseStatusGatewayHelper.StoreNewCaseStatusForPerson(DatabaseContext, caseStatusType, person);
            CaseStatusGatewayHelper.StoreNewCaseStatusForPerson(DatabaseContext, caseStatusType, person);
            CaseStatusGatewayHelper.StoreNewCaseStatusForPerson(DatabaseContext, caseStatusType, person);

            var caseStatuses = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            caseStatuses.Count.Should().Be(3);
        }

        [Test]
        public void WhenAPersonHasMultipleCurrentCaseStatusesAndSomeAreInThePast()
        {
            var person = PersonGatewayHelper.StoreNewPerson(DatabaseContext);
            var caseStatusType = CaseStatusGatewayHelper.StoreNewCaseStatusType(DatabaseContext);
            CaseStatusGatewayHelper.StoreNewCaseStatusForPerson(DatabaseContext, caseStatusType, person);
            CaseStatusGatewayHelper.StoreNewCaseStatusForPerson(DatabaseContext, caseStatusType, person);
            CaseStatusGatewayHelper.StoreNewCaseStatusForPerson(
                DatabaseContext,
                caseStatusType,
                person,
                _faker.Date.Past()
            );

            var caseStatuses = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            caseStatuses.Count.Should().Be(2);
        }
    }
}
