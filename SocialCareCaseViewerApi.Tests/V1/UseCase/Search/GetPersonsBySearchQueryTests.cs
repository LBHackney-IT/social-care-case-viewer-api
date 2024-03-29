using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.Search
{
    [TestFixture]
    public class GetPersonsBySearchQueryTests
    {
        private SearchUseCase _searchUseCase = null!;
        private Mock<ISearchGateway> _mockSearchGateway = null!;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockSearchGateway = new Mock<ISearchGateway>();
            _searchUseCase = new SearchUseCase(_mockSearchGateway.Object);
        }

        [Test]
        public void WhenBothFirstAndLastNameGivenCallsSearchGateway()
        {
            _mockSearchGateway.Setup(x => x.GetPersonRecordsBySearchQuery(It.IsAny<PersonSearchRequest>())).Returns((new List<ResidentInformation>(), 0, 0));

            _searchUseCase.GetResidentsByQuery(new PersonSearchRequest() { FirstName = "foo", LastName = "bar" });

            _mockSearchGateway.Verify(x => x.GetPersonRecordsBySearchQuery(It.IsAny<PersonSearchRequest>()), Times.Once);
        }

        [Test]
        public void WhenOnlyFirstNameGivenCallsSearchGateway()
        {
            _mockSearchGateway.Setup(x => x.GetPersonRecordsBySearchQuery(It.IsAny<PersonSearchRequest>())).Returns((new List<ResidentInformation>(), 0, 0));

            _searchUseCase.GetResidentsByQuery(new PersonSearchRequest() { FirstName = "foo" });

            _mockSearchGateway.Verify(x => x.GetPersonRecordsBySearchQuery(It.IsAny<PersonSearchRequest>()), Times.Once);
        }

        [Test]
        public void WhenOnlyLastNameGivenCallsSearchGateway()
        {
            _mockSearchGateway.Setup(x => x.GetPersonRecordsBySearchQuery(It.IsAny<PersonSearchRequest>())).Returns((new List<ResidentInformation>(), 0, 0));

            _searchUseCase.GetResidentsByQuery(new PersonSearchRequest() { LastName = "foo" });

            _mockSearchGateway.Verify(x => x.GetPersonRecordsBySearchQuery(It.IsAny<PersonSearchRequest>()), Times.Once);
        }

        [Test]
        public void ReturnsTheNextCursor()
        {
            var residents = _fixture.CreateMany<ResidentInformation>(21).ToList();

            var expectedNextCursor = "20";

            _mockSearchGateway.Setup(x =>
                    x.GetPersonRecordsBySearchQuery(It.IsAny<PersonSearchRequest>()))
                .Returns((residents.Take(20).ToList(), 0, 20));

            _searchUseCase.GetResidentsByQuery(new PersonSearchRequest() { FirstName = "foo" }).NextCursor.Should().Be(expectedNextCursor);
        }

        [Test]
        public void WhenAtTheEndOfTheResidentListReturnsTheNextCursorAsEmptyString()
        {
            var residents = _fixture.CreateMany<ResidentInformation>(7);

            _mockSearchGateway.Setup(x =>
                    x.GetPersonRecordsBySearchQuery(It.IsAny<PersonSearchRequest>()))
                .Returns((residents.ToList(), 0, null));

            _searchUseCase.GetResidentsByQuery(new PersonSearchRequest() { FirstName = "foo" }).NextCursor.Should().Be("");
        }

        [Test]
        public void ReturnsNullorZeroAsValuesForAllPropertiesWhenAllQueryParametersAreNullOrEmpty()
        {
            var request = new PersonSearchRequest();

            var result = _searchUseCase.GetResidentsByQuery(request);

            result.Residents.Should().BeNull();
            result.NextCursor.Should().BeNull();
            result.TotalCount.Should().Be(0);
        }
    }
}
