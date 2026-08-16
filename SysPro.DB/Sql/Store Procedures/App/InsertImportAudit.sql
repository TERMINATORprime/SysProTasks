CREATE OR ALTER PROCEDURE App.InsertImportAudit
    @FileName     nvarchar(max),
    @Considered   int,
    @Applied      int,
    @Invalid      int,
    @ProcessedUtc datetime2 = NULL
    AS
BEGIN
    SET NOCOUNT ON;

INSERT INTO App.ImportAudits (FileName, ProcessedUtc, Considered, Applied, Invalid)
    OUTPUT inserted.ImportId
VALUES (@FileName, ISNULL(@ProcessedUtc, SYSUTCDATETIME()), @Considered, @Applied, @Invalid);
END
GO