namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class CreateAllocationResponse
    {
        public long MosaicId { get; set; }

        public string WorkerEmail { get; set; }

        public string AllocatedWorkerTeam { get; set; }
    }
}