using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.Residents
{
    [TestFixture]
    public class GetAllocatedListTests
    {
        private ResidentUseCase _residentUseCase = null!;
        private Mock<IDatabaseGateway> _mockDatabaseGateway = null!;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _residentUseCase = new ResidentUseCase(_mockDatabaseGateway.Object);
        }

        [Test]
        public void ReturnsEmptyListWhenTeamHasNoResidentsAssigned()
        {
            _residentUseCase
                .GetAllocatedList(teamId: 1, view: "unallocated")
                .Should()
                .BeEquivalentTo(new AllocatedResidentsToTheTeamList() { Residents = new List<ResidentInformation>() });
        }

        [Test]
        public void ReturnsListOfResidentsAllocatedToTheTeam()
        {
            var person = TestHelpers.CreatePerson();
            var residentTeam = new ResidentTeam()
            {
                TeamId = 1,
                RagRating = "Green"
            };

            person.ResidentTeams = new List<ResidentTeam>() { residentTeam };
            var residents = new List<ResidentInformation>();
            residents.Add(person.ToResidentInformationResponse());

            _mockDatabaseGateway.Setup(x => x.GetResidentsByTeamId(1, "unallocated")).Returns(new List<Person>() { person });
            _residentUseCase
                .GetAllocatedList(1, "unallocated")
                .Should()
                .BeEquivalentTo(new AllocatedResidentsToTheTeamList() { Residents = residents });
        }


        [Test]
        public void ReturnsOrderedListByRagRatingThenByTimeWhenTeamHasResidentsAssigned()
        {
            var residentsList = new List<Person>();
            var ragList = new List<string> { "purple", "red", "amber", "green", "purple", "red", "amber", "green", };
            var allocationTimes = new List<DateTime>
            {
                DateTime.Parse("01/03/2022 00:00:00"),
                DateTime.Parse("01/03/2022 00:00:00"),
                DateTime.Parse("01/03/2022 00:00:00"),
                DateTime.Parse("01/03/2022 00:00:00"),
                DateTime.Parse("01/01/2022 00:00:00"),
                DateTime.Parse("01/01/2022 00:00:00"),
                DateTime.Parse("01/01/2022 00:00:00"),
                DateTime.Parse("01/01/2022 00:00:00"),
            };

            var orderedRagList = new List<string> { "purple", "purple", "red", "red", "amber", "amber", "green", "green", };
            var orderedAllocationTimes = new List<DateTime>
            {
                DateTime.Parse("01/01/2022 00:00:00"),
                DateTime.Parse("01/03/2022 00:00:00"),
                DateTime.Parse("01/01/2022 00:00:00"),
                DateTime.Parse("01/03/2022 00:00:00"),
                DateTime.Parse("01/01/2022 00:00:00"),
                DateTime.Parse("01/03/2022 00:00:00"),
                DateTime.Parse("01/01/2022 00:00:00"),
                DateTime.Parse("01/03/2022 00:00:00")
            };

            for (int i = 0; i < 8; i++)
            {
                var residentTeam = new ResidentTeam() { TeamId = 1 };
                residentTeam.RagRating = ragList[i];
                residentTeam.AllocationDate = allocationTimes[i];
                var resident = new Person();
                resident.Id = i;
                resident.ResidentTeams = new List<ResidentTeam>() { residentTeam };
                residentsList.Add(resident);
            }

            _mockDatabaseGateway.Setup(x =>
                    x.GetResidentsByTeamId(1, "unallocated"))
                .Returns((residentsList.ToList()));

            var result = _residentUseCase
                .GetAllocatedList(teamId: 1, view: "unallocated");

            for (int i = 0; i < 8; i++)
            {
                result.Residents[i].ResidentTeams.First().RagRating.Should().BeEquivalentTo(orderedRagList[i]);
                result.Residents[i].ResidentTeams.First().AllocationDate.Should().Be(orderedAllocationTimes[i]);
            }
        }
    }
}
