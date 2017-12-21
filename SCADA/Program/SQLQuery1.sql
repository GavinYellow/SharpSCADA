USE [SCADA]
GO

DECLARE	@return_value Int

EXEC	@return_value = [dbo].[InitServer]
		@TYPE = 1

SELECT	@return_value as 'Return Value'

GO
