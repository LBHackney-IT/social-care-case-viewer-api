using AutoFixture;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.Residents
{
    [TestFixture]
    public class GetUnallocatedListTests
    {
        private ResidentUseCase _residentUseCase = null!;
        private Mock<IDatabaseGateway> _mockDatabaseGateway = null!;
        // private readonly Fixture _fixture = new Fixture();
        // private const int MinimumLimit = 10;
        // private const int MaximumLimit = 100;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _residentUseCase = new ResidentUseCase(_mockDatabaseGateway.Object);

            _mockDatabaseGateway.Setup(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null, null, null))
               .Returns((new List<ResidentInformation>(), 0));
        }

        [Test]
        public void ReturnsEmptyListWhenNoResidentsAreAllocatedToTheTeam()
        {
            _residentUseCase
                .GetAllocatedList(1, "unallocated", cursor: 0, limit: 20)
                .Should()
                .BeEquivalentTo(new ResidentInformationList() { Residents = new List<ResidentInformation>(), NextCursor = "", TotalCount = 0 });
        }

        [Test]
        public void ReturnsListOfResidentsAllocatedToTheTeam()
        {
            var person = TestHelpers.CreatePerson();
            var residents = new List<ResidentInformation>();
            residents.Add(person.ToResidentInformationResponse());

            _mockDatabaseGateway.Setup(x => x.GetPersonsByTeamId(1, "unallocated")).Returns(new List<Person>() { person });
            _residentUseCase
                .GetAllocatedList(1, "unallocated", cursor: 0, limit: 20)
                .Should()
                .BeEquivalentTo(new ResidentInformationList() { Residents = residents, NextCursor = "", TotalCount = 1 });
        }
    }
}
