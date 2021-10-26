# Add end date column to case status answers

## The problem we're trying to solve

We need end date column in the case status answers table so we can identify previous case status answers

## Justification for doing a manual update

We don't have automated db migrations

## The plan

1. Run the SQL script

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1373

## SQL statement(s)

```sql
ALTER TABLE dbo.sccv_person_case_status_answers
	ADD COLUMN end_date timestamp;
```

