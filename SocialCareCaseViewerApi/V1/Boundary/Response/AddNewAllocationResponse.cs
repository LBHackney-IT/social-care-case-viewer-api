namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class AddNewAllocationResponse
    {
        public long MosaicId { get; set; }

        public string WorkerEmail { get; set; }

        public string AllocatedWorkerTeam { get; set; }
    }
}