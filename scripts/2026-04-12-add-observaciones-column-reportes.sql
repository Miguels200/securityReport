/* Add Observaciones column to Reportes and backfill from legacy Descripcion marker */
IF COL_LENGTH('dbo.Reportes', 'Observaciones') IS NULL
BEGIN
    ALTER TABLE dbo.Reportes
    ADD Observaciones NVARCHAR(4000) NOT NULL CONSTRAINT DF_Reportes_Observaciones DEFAULT ('');
END;
GO

/* Backfill Observaciones from lines like: Observaciones: ... */
UPDATE R
SET R.Observaciones = LTRIM(RTRIM(SUBSTRING(R.Descripcion, P.Pos + LEN('Observaciones:'), 4000)))
FROM dbo.Reportes AS R
CROSS APPLY (
    SELECT CHARINDEX('Observaciones:', R.Descripcion) AS Pos
) AS P
WHERE P.Pos > 0
  AND (R.Observaciones IS NULL OR LTRIM(RTRIM(R.Observaciones)) = '');
GO
