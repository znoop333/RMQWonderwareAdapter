USE [OmmcMes]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

alter proc [dbo].[PLC_TAG_LOOKUP_PLC_IP] (
	@WW_ITEM_NAME varchar(100) = 'DDESuite_CLX02.MTqStopper02.TQ2_2130'
) as begin
SET NOCOUNT ON;

SELECT isnull(PLC_IP, '') PLC_IP
      ,isnull(TAG_ID, '') TAG_ID
      ,isnull(WW_ITEM_NAME, '') WW_ITEM_NAME
      ,isnull([DESCRIPTION], '') [DESCRIPTION]
  FROM [OmmcMes].[dbo].[MST_PLC_TAGS]
  where WW_ITEM_NAME=@WW_ITEM_NAME

end
