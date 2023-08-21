CREATE PROCEDURE [dbo].[pEvent_Create]
	 @ProviderEventID int
	,@EventName nvarchar(50)
	,@EventDate DateTime
AS
BEGIN

INSERT INTO dbo.Event WITH (ROWLOCK)
 (ProviderEventID
 ,EventName
 ,EventDate)
 VALUES
 (@ProviderEventID
 ,@EventName
 ,@EventDate)

END
GO

CREATE PROCEDURE [dbo].[pEvent_Update]
	 @ProviderEventID int
	,@EventDate DateTime
AS
BEGIN
	-- update panel table
	UPDATE Event WITH (ROWLOCK)
	SET	  EventDate = @EventDate		
	WHERE ProviderEventID = @ProviderEventID
	
	IF (@@ROWCOUNT = 0)
	BEGIN;
		THROW 400000, 'Validation.EntityDoesNotExist', 1;
	    ROLLBACK
	    RETURN
	END
END
GO

CREATE PROCEDURE [dbo].[pOdd_Create]
	 @ProviderEventID int
	,@ProviderOddsID int
	,@OddsName nvarchar(50)
	,@OddsRate float
	,@Status bit
AS
BEGIN

INSERT INTO dbo.Odd WITH (ROWLOCK)
 (ProviderEventID
 ,ProviderOddsID
 ,OddsName
 ,OddsRate
 ,Status)
 VALUES
 (@ProviderEventID
 ,@ProviderOddsID
 ,@OddsName
 ,@OddsRate
 ,@Status)

END
GO

CREATE PROCEDURE [dbo].[pOdd_Update]
	 @ProviderOddsID int
	,@OddsRate float
	,@Status bit
AS
BEGIN

	---- update panel table
	--UPDATE Odd WITH (ROWLOCK)
	--SET	   OddsRate = @OddsRate
	--WHERE ProviderOddsId = @ProviderOddsId

	UPDATE Odd WITH (ROWLOCK)
		SET OddsRate = @OddsRate, Status = @Status
		WHERE (OddsRate <> @OddsRate OR Status <> @Status)
	AND ProviderOddsID = @ProviderOddsID;


	IF (@@ROWCOUNT = 0)
	BEGIN;
		THROW 400000, 'Validation.EntityDoesNotExist', 1;
	    ROLLBACK
	    RETURN
	END
END
GO

