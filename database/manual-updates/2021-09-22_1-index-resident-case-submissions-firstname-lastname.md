# Index By Case Submission Resident Id (Mosaic Id)

## The problem we're trying to solve

Querying DocumentDB for case submissions by Resident Id (Mosaic Id) is slow.

## Justification for doing a manual update

Currently the only way to update our indexes.

## The plan

1. Simply connect to DocumentDB instance
2. Run the query

## Link to Jira ticket

[Optimise and index query for Residents.Firstname](https://hackney.atlassian.net/browse/SCT-1266)
[Optimise and index query for Residents.Lastname](https://hackney.atlassian.net/browse/SCT-1267)

## MongoDB query

```
db['resident-case-submissions'].createIndex(
    {
        "Residents.FirstName": 1
    },
    {
        name: "residents_firstname", background: true
    }
);

db['resident-case-submissions'].createIndex(
    {
        "Residents.LastName": 1
    },
    {
        name: "residents_lastname", background: true
    }
);
```
