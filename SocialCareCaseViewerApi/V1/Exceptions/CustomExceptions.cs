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

    public class DeleteSubmissionException : Exception
    {
        public DeleteSubmissionException(string message) : base(message) { }
    }

    public class SubmissionAlreadyDeletedException : Exception
    {
        public SubmissionAlreadyDeletedException(string message) : base(message) { }
    }

    public class UnsupportedSubmissionTypeException : Exception
    {
        public UnsupportedSubmissionTypeException(string message) : base(message) { }
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
        public CaseStatusDoesNotExistException(string message) : base(message) { }
    }

    public class CaseStatusDoesNotMatchPersonException : Exception
    {
        public CaseStatusDoesNotMatchPersonException(string message) : base(message) { }
    }

    public class CaseStatusAlreadyClosedException : Exception
    {
        public CaseStatusAlreadyClosedException(string message) : base(message) { }
    }

    public class InvalidAgeContextException : Exception
    {
        public InvalidAgeContextException(string message) : base(message) { }
    }

    public class InvalidEndDateException : Exception
    {
        public InvalidEndDateException(string message) : base(message) { }
    }

    public class InvalidStartDateException : Exception
    {
        public InvalidStartDateException(string message) : base(message) { }
    }


    public class InvalidCaseStatusTypeException : Exception
    {
        public InvalidCaseStatusTypeException(string message) : base(message) { }
    }

    public class InvalidCaseStatusAnswersStartDateException : Exception
    {
        public InvalidCaseStatusAnswersStartDateException(string message) : base(message) { }
    }

    public class InvalidCaseStatusUpdateRequestException : Exception
    {
        public InvalidCaseStatusUpdateRequestException(string message) : base(message) { }
    }

    public class InvalidCaseStatusAnswersRequestException : Exception
    {
        public InvalidCaseStatusAnswersRequestException(string message) : base(message) { }
    }

    public class ProcessDataGatewayException : Exception
    {
        public ProcessDataGatewayException(string message) : base(message) { }
    }

    public class CaseNoteIdConversionException : Exception
    {
        public CaseNoteIdConversionException(string message) : base(message) { }
    }

    public class CaseNoteNotFoundException : Exception
    {
        public CaseNoteNotFoundException() : base() { }
    }

    public class DatabaseConfigurationException : Exception
    {
        public DatabaseConfigurationException(string message) : base(message) { }
    }

    public class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {
        }
    }
}
