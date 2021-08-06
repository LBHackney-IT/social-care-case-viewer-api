using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Linq;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Data.MongoDB
{
    [TestFixture]
    public class UpdateCFScaseNotesToASCcaseNotes
    {
        private const string MainFormCollection = "resident-case-submissions";
        private IMongoDatabase? _mongoDatabase;
        private readonly FilterDefinition<CaseSubmission> _filterByChildCaseNoteFormID = Builders<CaseSubmission>.Filter.Eq("FormId", "child-case-note");
        private readonly FilterDefinition<CaseSubmission> _filterByAdultCaseNoteFormID = Builders<CaseSubmission>.Filter.Eq("FormId", "adult-case-note");

        private static readonly JsonCommand<BsonDocument> _dbCommandUnderTest = new JsonCommand<BsonDocument>(
                @"{
                update: ""resident-case-submissions"",
                updates:
                    [
                        {
                            q: { FormId: ""child-case-note"",} , u: { $set: { FormId: ""adult-case-note"" } }, multi: true
                        }
                    ]
            }");

        /// <summary>
        ///  Updates all records with FormId: child-case-note to FormId: adult-case-note
        /// </summary>
        /// <howto>
        /// To run this script manually on the server:
        /// - remove double quotes
        /// - add appropriate IDs (one in the query (q) is the current one and the one in the $set is the new master id)
        /// - pass it to the db.runCommand() like this:
        ///
        ///db.runCommand({
        ///update: "resident-case-submissions",
        ///        updates:
        ///            [
        ///                {
        ///                    q: { FormId: "child-case-note",} , u: { $set: { FormId: "adult-case-note" } }, multi: true
        ///                }
        ///            ]
        ///    });
        ///</howto>      


        [SetUp]
        public void SetUp()
        {
            string mongoConnectionString = Environment.GetEnvironmentVariable("SCCV_MONGO_CONN_STRING") ??
                                            Environment.GetEnvironmentVariable("MONGO_DB_TEST_CONN_STRING") ??
                                            @"mongodb://localhost:1433/";

            string databaseName = Environment.GetEnvironmentVariable("SCCV_MONGO_DB_NAME") ?? "social_care_db_name";

            var mongoClient = new MongoClient(new MongoUrl(mongoConnectionString));
            _mongoDatabase = mongoClient.GetDatabase(databaseName);        
        }

        [TearDown]
        public void TearDown()
        {
            _mongoDatabase?.DropCollection(MainFormCollection);
        }

        [Test]
        public void UpdatesFormIdFromChildCaseNoteToAdultCaseNoteForAllMatchingRecords()
        {
            var caseNoteOne = TestHelpers.CreateCaseSubmission(formId: "child-case-note");
            var caseNoteTwo = TestHelpers.CreateCaseSubmission(formId: "child-case-note");

            var collection = _mongoDatabase?.GetCollection<CaseSubmission>(MainFormCollection);

            collection?.InsertOne(caseNoteOne);
            collection?.InsertOne(caseNoteTwo);

            collection.Find(_filterByChildCaseNoteFormID).ToList().Count.Should().Be(2);

            _mongoDatabase?.RunCommand(_dbCommandUnderTest);

            collection.Find(_filterByChildCaseNoteFormID).ToList().Count.Should().Be(0);
            collection.Find(_filterByAdultCaseNoteFormID).ToList().Count.Should().Be(2);
        }

        [Test]
        public void DoesNotUpdateOtherThanMatchingRecords()
        {
            var caseNoteOne = TestHelpers.CreateCaseSubmission(formId: "child-case-note");
            var caseNoteTwo = TestHelpers.CreateCaseSubmission(formId: "adult-case-note");
            var caseNoteThree = TestHelpers.CreateCaseSubmission(formId: "other-case-note");

            var collection = _mongoDatabase?.GetCollection<CaseSubmission>(MainFormCollection);

            collection?.InsertOne(caseNoteOne);
            collection?.InsertOne(caseNoteTwo);
            collection?.InsertOne(caseNoteThree);

            _mongoDatabase?.RunCommand(_dbCommandUnderTest);

            collection.Find(Builders<CaseSubmission>.Filter.Eq("FormId", "other-case-note")).ToList().Count.Should().Be(1);
            collection.Find(_filterByAdultCaseNoteFormID).ToList().Count.Should().Be(2);
            collection.Find(_filterByChildCaseNoteFormID).ToList().Count.Should().Be(0);
        }
    }
}
