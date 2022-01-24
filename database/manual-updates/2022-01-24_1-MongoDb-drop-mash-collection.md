# Script to drop the mash-referrals collection

## The problem we're trying to solve

The  mash-referrals collection is not used in our codebase as we've switched to PostgreSQL (and MASH is being decommisioned) We dropped the collection in Staging to free up some space.

## Justification for doing a manual update

No other way to drop collections.

## The plan

1. Connect to DocumentDB instance
2. Run the mongodb scripts


## MongoDB scripts

```
use social_care_db;
```

```
db['mash-referrals'].drop()
```


