# Update Case Submission Forms Ids from child-case-note To adult-case-note

## The problem we're trying to solve

Case submissions got saved with the wrong form id saved as child-case-note when it should be adult-case-note.

## Justification for doing a manual update

There is no UI or other way in place for fixing this data.

## The plan

1. Create script for converting all our cse submissions with child-case-note to adult-case-note
2. Make a backup of our form data collection
3. Run script against our database to convert case submissions

## Link to Jira ticket

<!-- Add the link to the Jira ticket -->

## Mongo statement(s)

## database backup

```
db["resident-case-submissions"].aggregate([ { $match: {} }, { $out: "case_submissions_2021_08_10" } ]);
```

## database conversion script

```
db.runCommand({
update: "resident-case-submissions",
    updates:
        [
            {
                q: { FormId: "child-case-note",} , u: { $set: { FormId: "adult-case-note" } }, multi: true
            }
        ]
});
   ```
