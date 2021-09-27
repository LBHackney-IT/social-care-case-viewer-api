using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.WorkerGatewayTests
{
    public class GetWorkerByWorkerIdTests : DatabaseTests
    {
        private WorkerGateway _workerGateway = null!;

        [SetUp]
        public void Setup()
        {
            _workerGateway = new WorkerGateway(DatabaseContext);
        }

        [Test]
        public void GetWorkerByWorkerIdReturnsWorker()
        {
            var worker = SaveWorkerToDatabase(TestHelpers.CreateWorker());

            var response = _workerGateway.GetWorkerByWorkerId(worker.Id);

            response.Should().BeEquivalentTo(worker.ToDomain(true));
        }

        [Test]
        public void GetWorkerByWorkerIdReturnsNullWhenIdNotPresent()
        {
            var worker = SaveWorkerToDatabase(TestHelpers.CreateWorker());
            var response = _workerGateway.GetWorkerByWorkerId(worker.Id + 1);

            response.Should().BeNull();
        }

        private Worker SaveWorkerToDatabase(Worker worker)
        {
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();
            return worker;
        }
    }
}
