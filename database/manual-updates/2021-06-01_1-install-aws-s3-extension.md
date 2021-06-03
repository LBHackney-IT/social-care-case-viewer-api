# Install AWS S3 extension

## The problem we're trying to solve

Most of our person records that we've imported into the database are missing
gender values.

## Justification for doing a manual update

As there are approximately 200,000 records are missing a value, it's not
feasible to update them through the frontend.

## The plan

1. Install the AWS S3 PostgreSQL extension by running the SQL statement

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-12

## SQL statement(s)

```sql
CREATE EXTENSION aws_s3 CASCADE;
```

## Useful resources

- [Importing data into PostgreSQL on Amazon RDS](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/PostgreSQL.Procedural.Importing.html)
