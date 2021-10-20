# Update case statuses table structure

## The problem we're trying to solve

Added discarded_at column for case statuses answers

## Justification for doing a manual update

We don't have mautomated migrations yet

## The plan

1. Run the SQL script below

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1354

## SQL statement(s)

```sql
--altering person_case_status_answers

ALTER TABLE dbo.sccv_person_case_status_answers
  ADD COLUMN discarded_at timestamp;

ALTER TABLE dbo.sccv_person_case_status_answers
  ADD COLUMN group_id varchar(36) not null;

```
