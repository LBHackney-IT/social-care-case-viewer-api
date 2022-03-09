using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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

            _mockDatabaseGateway.Setup(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null, null, null))
               .Returns((new List<ResidentInformation>(), 0));
        }

        [Test]
        public void ReturnsEmptyListWhenTeamHasNoResidentsAssigned()
        {
            _residentUseCase
                .GetAllocatedList(teamId: 1, view: "unallocated", cursor: 0, limit: 20)
                .Should()
                .BeEquivalentTo(new ResidentInformationList() { Residents = new List<ResidentInformation>(), NextCursor = "", TotalCount = 0 });
        }


        [Test]
        public void ReturnsListWhenTeamHasResidentsAssigned()
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
                    x.GetPersonsByTeamId(1, "unallocated"))
                .Returns((residentsList.ToList()));

            var result = _residentUseCase
                .GetAllocatedList(teamId: 1, view: "unallocated", cursor: 0, limit: 20);

            for (int i = 0; i < 8; i++)
            {
                result.Residents[i].ResidentTeams.First().RagRating.Should().BeEquivalentTo(orderedRagList[i]);
                result.Residents[i].ResidentTeams.First().AllocationDate.Should().Be(orderedAllocationTimes[i]);
            }
        }
    }
}
