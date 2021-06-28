-- Get number of foster carers (including private foster carers)
SELECT
  COUNT(DISTINCT p.person_id) AS number_of_foster_carers
FROM
  dbo.sccv_personal_relationship AS pr
  INNER JOIN dbo.dm_persons AS p ON pr.fk_person_id = p.person_id
  INNER JOIN dbo.sccv_personal_relationship_type AS prt ON pr.fk_personal_relationship_type_id = prt.id
WHERE
  prt.description = 'fosterChild'
  OR prt.description = 'privateFosterChild';

-- Get foster children of foster carers (including private foster carers)
SELECT
  pr.id AS personal_relationship_id,
  p.person_id AS foster_carer_person_id,
  p.first_name AS foster_carer_first_name,
  p.last_name AS foster_carer_last_name,
  pi.person_id AS foster_child_person_id,
  pi.first_name AS foster_child_first_name,
  pi.last_name AS foster_child_last_name
FROM
  dbo.sccv_personal_relationship AS pr
  INNER JOIN dbo.dm_persons AS p ON pr.fk_person_id = p.person_id
  INNER JOIN dbo.dm_persons AS pi ON pr.fk_other_person_id = pi.person_id
  INNER JOIN dbo.sccv_personal_relationship_type AS prt ON pr.fk_personal_relationship_type_id = prt.id
WHERE
  (
    prt.description = 'fosterChild'
    OR prt.description = 'privateFosterChild'
  );

-- Get number of foster carers (excluding private foster carers)
SELECT
  COUNT(DISTINCT p.person_id) AS number_of_foster_carers
FROM
  dbo.sccv_personal_relationship AS pr
  INNER JOIN dbo.dm_persons AS p ON pr.fk_person_id = p.person_id
  INNER JOIN dbo.sccv_personal_relationship_type AS prt ON pr.fk_personal_relationship_type_id = prt.id
WHERE
  prt.description = 'fosterChild';

-- Get foster children of foster carers (excluding private foster carers)
SELECT
  pr.id AS personal_relationship_id,
  p.person_id AS foster_carer_person_id,
  p.first_name AS foster_carer_first_name,
  p.last_name AS foster_carer_last_name,
  pi.person_id AS foster_child_person_id,
  pi.first_name AS foster_child_first_name,
  pi.last_name AS foster_child_last_name
FROM
  dbo.sccv_personal_relationship AS pr
  INNER JOIN dbo.dm_persons AS p ON pr.fk_person_id = p.person_id
  INNER JOIN dbo.dm_persons AS pi ON pr.fk_other_person_id = pi.person_id
  INNER JOIN dbo.sccv_personal_relationship_type AS prt ON pr.fk_personal_relationship_type_id = prt.id
WHERE
  prt.description = 'fosterChild';
