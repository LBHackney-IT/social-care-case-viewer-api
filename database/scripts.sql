--import new NC persons
do
$$
        declare
                new_person record;
                new_person_mosaic_id bigint;
        begin
        --TODO: truncate table for full import
                for new_person
                in SELECT
                        imports.first_name,
                        imports.last_name,
                        imports.full_name,
                        imports.date_of_birth,
                        imports.person_id,
                        imports.gender,
                        imports.restricted,
                        imports.context_flag

        FROM
                dbo.SCCV_PERSONS_LOOKUP as lookup RIGHT OUTER JOIN
                dbo.SCCV_PERSONS_IMPORT as imports ON lookup.NC_ID = imports.PERSON_ID
        WHERE lookup.PERSON_ID is null AND LOWER(imports.PERSON_ID) like 'nc%'

        loop
                new_person_mosaic_id = null;

                insert into dbo.dm_persons(
                        first_name,
                        last_name,
                        full_name,
                        date_of_birth,
                        gender,
                        restricted,
                        context_flag,
                        from_dm_person
                )
                values (
                        new_person.first_name,
                        new_person.last_name,
                        new_person.full_name,
                        new_person.date_of_birth,
                        new_person.gender,
                        new_person.restricted,
                        new_person.context_flag,
                        'N'
                )
                returning person_id into new_person_mosaic_id;

                insert into dbo.sccv_persons_lookup(person_id, nc_id)
                        values (new_person_mosaic_id, new_person.person_id);

                raise notice '% - %', new_person.person_id, new_person_mosaic_id;
        end loop;
end;
$$
