# Add UPRN update table

## The problem we're trying to solve

We need a temporary table to store address ids and correct UPRN values, so we can add UPRNs to address records that haven't been populated correctly.

## Justification for doing a manual update

We don't have automated db migrations

## The plan

1. Run the SQL script

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1555

## SQL statement(s)

```sql
CREATE TABLE IF NOT EXISTS dbo.sccv_uprn_update
(
	id  bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL PRIMARY KEY,
	address_id bigint NOT NULL,
	uprn bigint NOT NULL
);
```

