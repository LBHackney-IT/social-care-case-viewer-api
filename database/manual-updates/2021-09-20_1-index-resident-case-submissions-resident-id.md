# Index By Case Submission Resident Id (Mosaic Id)

## The problem we're trying to solve

Querying DocumentDB for case submissions by Resident Id (Mosaic Id) is slow.

## Justification for doing a manual update

Currently the only way to update our indexes.

## The plan

1. Simply connect to DocumentDB instance
2. Run the query

## Link to Jira ticket

[Assess overloading Mongo DB queries](https://hackney.atlassian.net/browse/SCT-1128)

## MongoDB query

```
db['resident-case-submissions'].createIndex(
    {
        "Residents._id": 1
    },
    {
        name: "residents_id", background: true
    }
);
```
