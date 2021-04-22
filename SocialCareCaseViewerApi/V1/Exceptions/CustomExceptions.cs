using System;

namespace SocialCareCaseViewerApi.V1.Exceptions
{
    public class MosaicApiException : Exception
    {
        public MosaicApiException(string message) : base(message) { }
    }

    public class EntityUpdateException : Exception
    {
        public EntityUpdateException(string message) : base(message) { }
    }

    public class UpdateAllocationException : Exception
    {
        public UpdateAllocationException(string message) : base(message) { }
    }

    public class CreateAllocationException : Exception
    {
        public CreateAllocationException(string message) : base(message) { }
    }


    public class WorkerNotFoundException : Exception
    {
        public WorkerNotFoundException(string message) : base(message) { }
    }

    public class TeamNotFoundException : Exception
    {
        public TeamNotFoundException(string message) : base(message) { }
    }

    public class SocialCarePlatformApiException : Exception
    {
        public SocialCarePlatformApiException(string message) : base(message) { }
    }

    public class PostWarningNoteException : Exception
    {
        public PostWarningNoteException(string message) : base(message) { }
    }

    public class PostWorkerException : Exception
    {
        public PostWorkerException(string message) : base(message) { }
    }
}
