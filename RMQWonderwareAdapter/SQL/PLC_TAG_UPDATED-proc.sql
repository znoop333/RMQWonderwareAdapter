USE [OmmcMes]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

alter proc [dbo].[PLC_TAG_UPDATED] (
	@PLC_IP varchar(15) 
	,@TAG_ID varchar(50) 
	,@Value varchar(50) 
	,@Updated datetime2(7)
	,@Quality int 
) as begin
SET NOCOUNT ON;

UPDATE [dbo].[TRV_PLC_TAGS]
   SET [Value] = @Value
      ,[Updated] = @Updated
      ,[Quality] = @Quality
	where [PLC_IP]=@PLC_IP and [TAG_ID]=@TAG_ID

select @@ROWCOUNT updatedRows;


end


