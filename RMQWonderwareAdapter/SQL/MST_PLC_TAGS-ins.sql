USE [OmmcMes]
GO

INSERT INTO [dbo].[MST_PLC_TAGS]
           ([PLC_IP]
           ,[TAG_ID]
           ,[WW_ITEM_NAME]
           ,[DESCRIPTION])
SELECT [PLC_IP]
      ,[TAG_ID]
	  ,'DDESuite_CLX02.MTqStopper02.' + [TAG_ID]
      ,[DESCRIPTION]
  FROM [OmmcMes].[dbo].[MST_EQUIPALARM]
    where 1=1
  and TAG_ID like 'TQ2_%'
  and [LINE_CODE1]='22'
  and [STOP_TYPE]='TORQUE'

