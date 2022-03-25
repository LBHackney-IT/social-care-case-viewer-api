using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    public class GetAllocationsPagingTests : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private Mock<IProcessDataGateway> _mockProcessdataGateway;
        private readonly Mock<ISystemTime> _mockIsystemTime = new Mock<ISystemTime>();

        [SetUp]
        public void SetUp()
        {
            _mockProcessdataGateway = new Mock<IProcessDataGateway>();
            _databaseGateway = new DatabaseGateway(DatabaseContext, _mockProcessdataGateway.Object, _mockIsystemTime.Object);

            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void GetAllocationsReturnsAmountOfResultsSetByTheLimit()
        {
            var limit = 20; //hard coded in the gateway for now

            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);

            for (int i = 0; i < 25; i++)
            {
                DatabaseContext.Add(TestHelpers.CreateAllocation(personId: (int) person.Id));
            }

            DatabaseContext.SaveChanges();

            var (result, _) = _databaseGateway.SelectAllocations(mosaicId: person.Id, workerId: 0, workerEmail: "", 0);

            result.Count.Should().Be(limit);
        }

        [Test]
        public void GivenMoreThanTwentyResultsWhenNextCursorUnspecifiedNextCursorIsTwenty()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);

            for (int i = 0; i < 25; i++)
            {
                DatabaseContext.Add(TestHelpers.CreateAllocation(personId: (int) person.Id));
            }

            DatabaseContext.SaveChanges();

            var (_, cursor) = _databaseGateway.SelectAllocations(mosaicId: person.Id, workerId: 0, workerEmail: "", 0);

            cursor.Should().Be(20);
        }

        [Test]
        public void GivenLessThanTwentyResultsWhenNextCursorIsUnspecifiedNextCursorIsNull()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);

            for (int i = 0; i < 19; i++)
            {
                DatabaseContext.Add(TestHelpers.CreateAllocation(personId: (int) person.Id));
            }

            DatabaseContext.SaveChanges();

            var (_, cursor) = _databaseGateway.SelectAllocations(mosaicId: person.Id, workerId: 0, workerEmail: "", 0);

            cursor.Should().Be(null);
        }

        [Test]
        public void GivenTwentyResultsWhenNextCursorIsUnspecifiedNextCursorIsNull()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);

            for (int i = 0; i < 20; i++)
            {
                DatabaseContext.Add(TestHelpers.CreateAllocation(personId: (int) person.Id));
            }

            DatabaseContext.SaveChanges();

            var (_, cursor) = _databaseGateway.SelectAllocations(mosaicId: person.Id, workerId: 0, workerEmail: "", 0);

            cursor.Should().Be(null);
        }

        [Test]
        public void GivenLessThanTwentyResultsWhenNextCursorIsTwentyNextCursorIsNull()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);

            for (int i = 0; i < 10; i++)
            {
                DatabaseContext.Add(TestHelpers.CreateAllocation(personId: (int) person.Id));
            }

            DatabaseContext.SaveChanges();

            var (_, cursor) = _databaseGateway.SelectAllocations(mosaicId: person.Id, workerId: 0, workerEmail: "", 0);

            cursor.Should().Be(null);
        }

        [Test]
        public void GivenMoreThanFortyResultsWhenNextCursorIsTwentyNextCursorIsForty()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);

            for (int i = 0; i < 50; i++)
            {
                DatabaseContext.Add(TestHelpers.CreateAllocation(personId: (int) person.Id));
            }

            DatabaseContext.SaveChanges();

            var (_, cursor) = _databaseGateway.SelectAllocations(mosaicId: person.Id, workerId: 0, workerEmail: "", cursor: 20, teamId: 0);

            cursor.Should().Be(40);
        }

        [Test]
        public void GivenFiftyResultsWhenNextCursorIsFortyThenTenResultsAreReturnedAndTheCursorIsNull()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);

            for (int i = 0; i < 50; i++)
            {
                DatabaseContext.Add(TestHelpers.CreateAllocation(personId: (int) person.Id));
            }

            DatabaseContext.SaveChanges();

            var (results, cursor) = _databaseGateway.SelectAllocations(mosaicId: person.Id, workerId: 0, workerEmail: "", cursor: 40, teamId: 0);

            cursor.Should().Be(null);
            results.Count.Should().Be(10);
        }
    }
}
