# Index By CreatedBy Email (Worker Email Address for Case Submission)

## The problem we're trying to solve

Querying DocumentDB for case submissions by CreatedBy Email (worker email address) is slow.

## Justification for doing a manual update

Currently the only way to update our indexes.

## The plan

1. Connect to DocumentDB instance
2. Run the query
3. Observe that initial execution states showed a COLLSCAN being performed and that after indexing this became an IXSCAN

## MongoDB query

```
use resident-case-submissions;
```

```
db['resident-case-submissions'].find({ "CreatedBy.Email": "john.farrell@hackney.gov.uk" }).explain("executionStats")
```

```
db['resident-case-submissions'].createIndex(
    {
        "CreatedBy.Email": 1
    },
    {
        name: "createdby_email", background: true
    }
);
```

```
db['resident-case-submissions'].find({ "CreatedBy.Email": "john.farrell@hackney.gov.uk" }).explain("executionStats")
```
