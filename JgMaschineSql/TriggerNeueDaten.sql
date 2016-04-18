-- =============================================
-- Author:		Gullus, Jörg
-- Create date: 29.11.2015
-- Description:	
-- =============================================

IF OBJECT_ID ('DatensatzUpdate','TR') IS NOT NULL
    DROP TRIGGER DatensatzUpdate;
GO

CREATE TRIGGER DatensatzUpdate 
   ON  tabDatenSet
   AFTER INSERT
AS 
BEGIN

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Select *  into Test From inserted

END
GO
