IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_JobItem]') AND type in (N'U'))
DROP TABLE [dbo].[D_JobItem]
GO
CREATE TABLE [dbo].[D_JobItem](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SupplierID] int NOT NULL,
	[ItemName] [varchar](500) NOT NULL,--Downloaded,NotPaid,AddressIncorrect,Released,Shipped,Cancelled
	[SKU] [varchar](100) not null,
	[ConditionID] int NOT NULL,
	[ItemDetail] [varchar](500) NOT NULL,
	[Ref1] [varchar](500) NOT NULL,
	[Ref2] [varchar](500) NOT NULL,
	[Ref3] [varchar](500) NOT NULL,
	[Ref4] [varchar](500) NOT NULL,
	[Ref5] [varchar](500) NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_D_JobItem] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO




IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_JobItemLine]') AND type in (N'U'))
DROP TABLE [dbo].[D_JobItemLine]
GO
CREATE TABLE [dbo].[D_JobItemLine](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[SKU] [varchar](100) NOT NULL,
	[Qty] [int] not null,
	[SupplierRef] [varchar](100) NOT NULL,
	[Weight] decimal(18,8) NOT NULL,
	[Length] decimal(18,8) NOT NULL,
	[Width] decimal(18,8) NOT NULL,
	[Height] decimal(18,8) NOT NULL,
	[Ref1] [varchar](500) NOT NULL,
	[Ref2] [varchar](500) NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](500) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](500) not null,

 CONSTRAINT [PK_D_Order_Line] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_D_Order_Line_D_Order_Header]') AND parent_object_id = OBJECT_ID(N'[dbo].[D_Order_Line]'))
ALTER TABLE [dbo].[D_Order_Line]  WITH CHECK ADD CONSTRAINT [FK_D_Order_Line_D_Order_Header] FOREIGN KEY([HeaderID])
REFERENCES [dbo].[D_Order_Header] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO