using System;
using MongoDB.Driver;

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

    public class PatchWorkerException : Exception
    {
        public PatchWorkerException(string message) : base(message) { }
    }

    public class PatchWarningNoteException : Exception
    {
        public PatchWarningNoteException(string message) : base(message) { }
    }

    public class PersonNotFoundException : Exception
    {
        public PersonNotFoundException(string message) : base(message) { }
    }

    public class PersonalRelationshipNotFoundException : Exception
    {
        public PersonalRelationshipNotFoundException(string message) : base(message) { }
    }

    public class CaseStatusTypeNotFoundException : Exception
    {
        public CaseStatusTypeNotFoundException(string message) : base(message) { }
    }
    public class CaseStatusAlreadyExistsException : Exception
    {
        public CaseStatusAlreadyExistsException(string message) : base(message) { }
    }

    public class UpdatePersonException : Exception
    {
        public UpdatePersonException(string message) : base(message) { }
    }

    public class GetTeamException : Exception
    {
        public GetTeamException(string message) : base(message) { }
    }

    public class PostTeamException : Exception
    {
        public PostTeamException(string message) : base(message) { }
    }

    public class DocumentNotFoundException : Exception
    {
        public DocumentNotFoundException(string message) : base(message) { }
    }

    public class GetRelationshipsException : Exception
    {
        public GetRelationshipsException(string message) : base(message) { }
    }

    public class GetCaseStatusesException : Exception
    {
        public GetCaseStatusesException(string message) : base(message) { }
    }

    public class CaseStatusNotFoundException : Exception
    {
        public override string Message { get; } = "Case Status Type does not exist.";
    }

    public class GetSubmissionException : Exception
    {
        public GetSubmissionException(string message) : base(message) { }
    }

    public class UpdateSubmissionException : Exception
    {
        public UpdateSubmissionException(string message) : base(message) { }
    }

    public class PersonalRelationshipTypeNotFoundException : Exception
    {
        public PersonalRelationshipTypeNotFoundException(string message) : base(message) { }
    }

    public class PersonalRelationshipAlreadyExistsException : Exception
    {
        public PersonalRelationshipAlreadyExistsException(string message) : base(message) { }
    }

    public class QueryCaseSubmissionsException : Exception
    {
        public QueryCaseSubmissionsException(string message) : base(message) { }
    }

    public class CaseStatusDoesNotExistException : Exception
    {
        public override string Message { get; } = "Case Status not found.";
    }

    public class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {
        }
    }
}
