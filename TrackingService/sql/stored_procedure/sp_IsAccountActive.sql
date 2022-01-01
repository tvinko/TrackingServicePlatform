USE Accounts;
GO

CREATE PROCEDURE [dbo].[sp_IsAccountActive](@AccountID INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT is_active FROM [dbo].[t_Accounts] WHERE account_id= @AccountID
	
END
GO
