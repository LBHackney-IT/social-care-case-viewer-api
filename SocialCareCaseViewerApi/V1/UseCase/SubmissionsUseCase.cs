using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class SubmissionsUseCase : ISubmissionsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IMongoGateway _mongoGateway;
        private const string CollectionName = "resident-case-submissions";

        public SubmissionsUseCase(IDatabaseGateway databaseGateway, IMongoGateway mongoGateway)
        {
            _databaseGateway = databaseGateway;
            _mongoGateway = mongoGateway;
        }

        public CaseSubmissionResponse ExecutePost(CreateCaseSubmissionRequest request)
        {
            var worker = _databaseGateway.GetWorkerByEmail(request.CreatedBy);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email {request.CreatedBy} not found");
            }

            var resident = _databaseGateway.GetPersonByMosaicId(request.ResidentId);
            if (resident == null)
            {
                throw new PersonNotFoundException($"Person with id {request.ResidentId} not found");
            }

            var dateTimeNow = DateTime.Now;

            var caseSubmission = new CaseSubmission
            {
                FormId = Guid.NewGuid(),
                Residents = new List<Person> {resident},
                Workers = new List<Worker> {worker},
                CreatedAt = dateTimeNow,
                CreatedBy = worker,
                SubmissionState = SubmissionState.InProgress,
                EditHistory = new List<(Worker, DateTime)> {(worker, dateTimeNow)},
                FormAnswers = new Dictionary<int, Dictionary<int, string[]>>()
            };

            _mongoGateway.InsertRecord(CollectionName, caseSubmission);

            return caseSubmission.ToDomain().ToResponse();
        }
    }
}
