-- Get number of informal carers
SELECT
  COUNT(DISTINCT p.person_id) AS number_of_informal_carers
FROM
  dbo.sccv_personal_relationship AS pr
  INNER JOIN dbo.dm_persons AS p ON pr.fk_person_id = p.person_id
WHERE
  pr.is_informal_carer = 'Y';

-- Get informal carer relationships
SELECT
  pr.id AS personal_relationship_id,
  p.person_id AS informal_carer_person_id,
  p.first_name AS informal_carer_first_name,
  p.last_name AS informal_carer_last_name,
  pi.person_id AS other_person_id,
  pi.first_name AS other_person_first_name,
  pi.last_name AS other_person_last_name
FROM
  dbo.sccv_personal_relationship AS pr
  INNER JOIN dbo.dm_persons AS p ON pr.fk_person_id = p.person_id
  INNER JOIN dbo.dm_persons AS pi ON pr.fk_other_person_id = pi.person_id
WHERE
  pr.is_informal_carer = 'Y';
