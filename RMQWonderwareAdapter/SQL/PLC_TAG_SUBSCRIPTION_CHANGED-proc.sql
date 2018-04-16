USE [OmmcMes]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

alter proc [dbo].[PLC_TAG_SUBSCRIPTION_CHANGED] (
	@PLC_IP varchar(15) 
	,@TAG_ID varchar(50) 
	,@USE_YN varchar(1) = 'N'
	,@ADVISED_YN varchar(1) = 'N'
	,@RequesterIP varchar(15) = null
	,@RequesterName varchar(50)  = null
	,@CorrelationId varchar(100)  = null
) as begin
SET NOCOUNT ON;

declare @rows int;

update [dbo].[TRV_PLC_TAGS]
set [USE_YN]=@USE_YN, [ADVISED_YN]=@ADVISED_YN
	, RequesterIP = isnull(@RequesterIP, RequesterIP)
	, RequesterName = isnull(@RequesterName, RequesterName)
	, CorrelationId = isnull(@CorrelationId, CorrelationId)
	where [PLC_IP]=@PLC_IP and [TAG_ID]=@TAG_ID

select @rows = @@ROWCOUNT;

select @rows updatedRows;
return @rows ;

end

