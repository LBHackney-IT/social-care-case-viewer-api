using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class SearchControllerTests
    {
        private Mock<IResidentUseCase> _mockResidentUseCase;
        private Mock<ISearchUseCase> _mockSearchUseCase;
        private SearchController _searchController;

        [SetUp]
        public void SetUp()
        {
            _mockResidentUseCase = new Mock<IResidentUseCase>();
            _mockSearchUseCase = new Mock<ISearchUseCase>();
            _searchController = new SearchController(_mockResidentUseCase.Object, _mockSearchUseCase.Object);
        }

        [Test]
        public void CallsResidentUseCaseWhenPersonIdIsProvided()
        {
            var request = new PersonSearchRequest() { PersonId = "123" };

            _mockResidentUseCase.Setup(x => x.GetResidentsByQuery(It.IsAny<ResidentQueryParam>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new ResidentInformationList());
            _searchController.GetPersonRecordsBySearchQuery(request);

            _mockResidentUseCase.Verify(x => x.GetResidentsByQuery(It.IsAny<ResidentQueryParam>(), It.IsAny<int>(), It.IsAny<int>()));
        }

        [Test]
        public void CallsSearchUseCaseIfPersonIdIsNotProvided()
        {
            var request = new PersonSearchRequest() { PersonId = null };

            _mockSearchUseCase.Setup(x => x.GetResidentsByQuery(It.IsAny<PersonSearchRequest>())).Returns(new ResidentInformationList());

            _searchController.GetPersonRecordsBySearchQuery(request);

            _mockSearchUseCase.Verify(x => x.GetResidentsByQuery(It.IsAny<PersonSearchRequest>()));
        }

        //[Test]
        //public void CallsResidentUseCaseWithCorrectMosaicIdWhenPersonIdProvided()
        //{
        //    var request = new PersonSearchRequest() { PersonId = "123" };
        //    var residentQueryParam = new ResidentQueryParam() { MosaicId = "123" };

        //    _mockResidentUseCase.Setup(x => x.GetResidentsByQuery(It.IsAny<ResidentQueryParam>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new ResidentInformationList());
        //    _searchController.GetPersonRecordsBySearchQuery(request);

        //    _mockResidentUseCase.Verify(x => x.GetResidentsByQuery(residentQueryParam, It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        //}

        //TODO: add other statuses
        [Test]
        public void Returns200AndResidentInformationListWhenPersonIdIsNotProvided()
        {
            var request = new PersonSearchRequest() { PersonId = null };

            var residentInformationList = new ResidentInformationList() { NextCursor = "0", Residents = new System.Collections.Generic.List<ResidentInformation>(), TotalCount = 0 };

            _mockSearchUseCase.Setup(x => x.GetResidentsByQuery(It.IsAny<PersonSearchRequest>())).Returns(residentInformationList);

            var r = _searchController.GetPersonRecordsBySearchQuery(request);

            var response = _searchController.GetPersonRecordsBySearchQuery(request) as OkObjectResult;

            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(residentInformationList);
        }

        //calls usecase with correct object x2
        //calls with default cursor and limit
        //resturns correct object result

    }
}
