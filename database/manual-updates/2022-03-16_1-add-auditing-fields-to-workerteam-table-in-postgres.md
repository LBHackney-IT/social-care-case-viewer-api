# Add trigram support

## The problem we're trying to solve

We need to add auditing fields to the worker team table in order to be able to run correct reports.

## Justification for doing a manual update

We don't have automated db migrations

## The plan

1. Run the SQL queries below one by one

## Link to Jira ticket

[SCT-1736] (https://hackney.atlassian.net/browse/SCT-1757)

## SQL statement(s)

```sql
ALTER TABLE dbo.SCCV_WORKERTEAM
    ADD COLUMN SCCV_CREATED_AT timestamp;

ALTER TABLE dbo.SCCV_WORKERTEAM
    ADD COLUMN SCCV_CREATED_BY varchar(300);

ALTER TABLE dbo.SCCV_WORKERTEAM
    ADD COLUMN SCCV_LAST_MODIFIED_AT timestamp;

ALTER TABLE dbo.SCCV_WORKERTEAM
    ADD COLUMN SCCV_LAST_MODIFIED_BY varchar(300);
```
