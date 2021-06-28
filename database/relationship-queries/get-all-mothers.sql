-- Get number of mothers (including mothers of unborn children)
SELECT
  COUNT(DISTINCT p.person_id) AS number_of_mothers
FROM
  dbo.sccv_personal_relationship AS pr
  INNER JOIN dbo.dm_persons AS p ON pr.fk_person_id = p.person_id
  INNER JOIN dbo.sccv_personal_relationship_type AS prt ON pr.fk_personal_relationship_type_id = prt.id
WHERE
  (
    prt.description = 'child'
    OR prt.description = 'unbornChild'
  )
  AND p.gender = 'F';

-- Get children of mothers (including mothers of unborn children)
SELECT
  pr.id AS personal_relationship_id,
  p.person_id AS mother_person_id,
  p.first_name AS mother_first_name,
  p.last_name AS mother_last_name,
  p.gender AS mother_gender,
  pi.person_id AS child_person_id,
  pi.first_name AS child_first_name,
  pi.last_name AS child_last_name
FROM
  dbo.sccv_personal_relationship AS pr
  INNER JOIN dbo.dm_persons AS p ON pr.fk_person_id = p.person_id
  INNER JOIN dbo.dm_persons AS pi ON pr.fk_other_person_id = pi.person_id
  INNER JOIN dbo.sccv_personal_relationship_type AS prt ON pr.fk_personal_relationship_type_id = prt.id
WHERE
  (
    prt.description = 'child'
    OR prt.description = 'unbornChild'
  )
  AND p.gender = 'F';

-- Get number of mothers (excluding mothers of unborn children)
SELECT
  COUNT(DISTINCT p.person_id) AS number_of_mothers
FROM
  dbo.sccv_personal_relationship AS pr
  INNER JOIN dbo.dm_persons AS p ON pr.fk_person_id = p.person_id
  INNER JOIN dbo.sccv_personal_relationship_type AS prt ON pr.fk_personal_relationship_type_id = prt.id
WHERE
  prt.description = 'child'
  AND p.gender = 'F';

-- Get children of mothers (excluding mothers of unborn children)
SELECT
  pr.id AS personal_relationship_id,
  p.person_id AS mother_person_id,
  p.first_name AS mother_first_name,
  p.last_name AS mother_last_name,
  p.gender AS mother_gender,
  pi.person_id AS child_person_id,
  pi.first_name AS child_first_name,
  pi.last_name AS child_last_name
FROM
  dbo.sccv_personal_relationship AS pr
  INNER JOIN dbo.dm_persons AS p ON pr.fk_person_id = p.person_id
  INNER JOIN dbo.dm_persons AS pi ON pr.fk_other_person_id = pi.person_id
  INNER JOIN dbo.sccv_personal_relationship_type AS prt ON pr.fk_personal_relationship_type_id = prt.id
WHERE
  prt.description = 'child'
  AND p.gender = 'F';
