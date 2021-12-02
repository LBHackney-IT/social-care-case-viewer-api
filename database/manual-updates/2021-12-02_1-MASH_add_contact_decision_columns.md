# Add two columns for MASH: contact decision

## The problem we're trying to solve

New columns needed to store more data

## Justification for doing a manual update

We don't have automated db migrations

## The plan

1. Run the script


## SQL statement(s)

```sql
alter table if exists dbo.ref_mash_referrals
add column contact_decision_created_at timestamp,
add column contact_decision_urgent_contact bool;
```