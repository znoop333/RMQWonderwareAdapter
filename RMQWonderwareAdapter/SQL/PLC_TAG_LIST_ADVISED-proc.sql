USE [OmmcMes]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create proc [dbo].[PLC_TAG_LIST_ADVISED] 
as begin

SELECT isnull(PLC_IP, '') PLC_IP
      ,isnull(TAG_ID, '') TAG_ID
      ,isnull([Value], '') [Value]
      ,isnull(Updated, '') Updated
      ,isnull(Quality, '') Quality
      ,isnull(RequesterIP, '') RequesterIP
      ,isnull(RequesterName, '') RequesterName
      ,isnull(CorrelationId, '') CorrelationId
      ,isnull(USE_YN, '') USE_YN
      ,isnull(ADVISED_YN, '') ADVISED_YN
  FROM [dbo].[TRV_PLC_TAGS]
  where ADVISED_YN='Y'

end

