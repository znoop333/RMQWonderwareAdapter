USE [OmmcMes]
GO

/****** Object:  Table [dbo].[MST_EQUIPALARM]    Script Date: 4/16/2018 9:53:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
--drop TABLE [dbo].[TRV_PLC_TAGS]
--go

CREATE TABLE [dbo].[TRV_PLC_TAGS](
	[PLC_IP] varchar(15) NOT NULL,
	[TAG_ID] [varchar](50) NOT NULL,
	[Value] [varchar](50) NULL,
	[Updated] datetime2(7) NULL,
	[Quality] int NULL,
	[RequesterIP] [varchar](15) NULL,
	[RequesterName] [varchar](50) NULL,
	[CorrelationId] [varchar](100) NULL,
	[USE_YN] [varchar](1) NULL,
	[ADVISED_YN] [varchar](1) NULL,
 CONSTRAINT [PK_TRV_PLC_TAGS] PRIMARY KEY CLUSTERED 
(
	[PLC_IP] ASC,
	[TAG_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO


