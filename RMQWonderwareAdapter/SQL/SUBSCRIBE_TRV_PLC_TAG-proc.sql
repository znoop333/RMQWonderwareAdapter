USE [OmmcMes]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

alter proc [dbo].[PLC_TAG_SUBSCRIBED] (
	@PLC_IP varchar(15) 
	,@TAG_ID varchar(50) 
	,@RequesterIP varchar(15) 
	,@RequesterName varchar(50) 
	,@CorrelationId varchar(100) 
) as begin
SET NOCOUNT ON;

declare @rows int;

if not exists (
	select 1 from [dbo].[TRV_PLC_TAGS]
	where [PLC_IP]=@PLC_IP and [TAG_ID]=@TAG_ID
)
begin
	INSERT INTO [dbo].[TRV_PLC_TAGS]
			   ([PLC_IP]
			   ,[TAG_ID]
			   ,[USE_YN]
			   ,[ADVISED_YN]
			   ,[RequesterIP]
			   ,[RequesterName]
			   ,[CorrelationId])
		 VALUES
			   (@PLC_IP
			   ,@TAG_ID
			   ,'Y'
			   ,'Y'
			   ,@RequesterIP
			   ,@RequesterName
			   ,@CorrelationId)

	select @rows = @@ROWCOUNT;
end
else
begin
	update [dbo].[TRV_PLC_TAGS]
	set [USE_YN]='Y', [ADVISED_YN]='Y'
	, RequesterIP = @RequesterIP
	, RequesterName = @RequesterName
	, CorrelationId = @CorrelationId
	where [PLC_IP]=@PLC_IP and [TAG_ID]=@TAG_ID

	select @rows = @@ROWCOUNT;
end

select @rows insertedRows;
return @rows ;

end

