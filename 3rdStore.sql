IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_JobItem]') AND type in (N'U'))
DROP TABLE [dbo].[D_JobItem]
GO
CREATE TABLE [dbo].[D_JobItem](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[JobItemCreateTime] datetime not null,
	[Type] int NOT NULL,
	[StatusID] int NOT NULL,
	[ConditionID] int NOT NULL,
	[ItemName] [varchar](500) NOT NULL,
	[ItemDetail] [varchar](4000) NOT NULL,
	[ItemPrice] decimal(18,2) NOT NULL,
	[Location] [varchar](500) NOT NULL,
	[DesignatedSKU] [varchar](500) not null,
	[ShipTime] datetime null,
	[TrackingNumber] [varchar](100) not null,
	[Ref1] [varchar](4000) NOT NULL,
	[Ref2] [varchar](4000) NOT NULL,
	[Ref3] [varchar](4000) NOT NULL,
	[Ref4] [varchar](4000) NOT NULL,
	[Ref5] [varchar](4000) NOT NULL,
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
	[ItemID] [int] NOT NULL,
	[SKU] [varchar](500) NOT NULL,
	[Qty] [int] not null,
	[SupplierRef] [varchar](500) NOT NULL,
	--[IsOrginalPackage] bit NOT NULL,
	[Weight] decimal(18,8) NOT NULL,
	[Length] decimal(18,8) NOT NULL,
	[Width] decimal(18,8) NOT NULL,
	[Height] decimal(18,8) NOT NULL,
	[CubicWeight] decimal(18,8) NOT NULL,
	[Ref1] [varchar](4000) NOT NULL,
	[Ref2] [varchar](4000) NOT NULL,
	[Ref3] [varchar](4000) NOT NULL,
	[Ref4] [varchar](4000) NOT NULL,
	[Ref5] [varchar](4000) NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_D_JobItemLine] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_D_JobItemLine_D_JobItem]') AND parent_object_id = OBJECT_ID(N'[dbo].[D_JobItemLine]'))
ALTER TABLE [dbo].[D_JobItemLine]  WITH CHECK ADD CONSTRAINT [FK_D_JobItemLine_D_JobItem] FOREIGN KEY([HeaderID])
REFERENCES [dbo].[D_JobItem] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO









/*****************************************************
User, Role, Permission
*****************************************************/


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_User]') AND type in (N'U'))
DROP TABLE [dbo].[T_User]
GO
CREATE TABLE [dbo].[T_User](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[Password] [varchar](500) NOT NULL,
	[PasswordSalt] [varchar](500) NOT NULL,
	[StatusID] [int] NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_T_User] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Role]') AND type in (N'U'))
DROP TABLE [dbo].[T_Role]
GO
CREATE TABLE [dbo].[T_Role](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[IsActive] bit NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_T_Role] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_UserRole]') AND type in (N'U'))
DROP TABLE [dbo].[M_UserRole]
GO
CREATE TABLE [dbo].[M_UserRole](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[RoleID] [int] NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_M_UserRole] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_M_UserRole_T_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[M_UserRole]'))
ALTER TABLE [dbo].[M_UserRole]  WITH CHECK ADD CONSTRAINT [FK_M_UserRole_T_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[T_User] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_M_UserRole_T_Role]') AND parent_object_id = OBJECT_ID(N'[dbo].[M_UserRole]'))
ALTER TABLE [dbo].[M_UserRole]  WITH CHECK ADD CONSTRAINT [FK_M_UserRole_T_Role] FOREIGN KEY([RoleID])
REFERENCES [dbo].[T_Role] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO




IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Permission]') AND type in (N'U'))
DROP TABLE [dbo].[T_Permission]
GO
CREATE TABLE [dbo].[T_Permission](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[IsActive] bit NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_T_Permission] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_RolePermission]') AND type in (N'U'))
DROP TABLE [dbo].[M_RolePermission]
GO
CREATE TABLE [dbo].[M_RolePermission](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RoleID] [int] NOT NULL,
	[PermissionID] [int] NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_M_RolePermission] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_M_RolePermission_T_Role]') AND parent_object_id = OBJECT_ID(N'[dbo].[M_RolePermission]'))
ALTER TABLE [dbo].[M_RolePermission]  WITH CHECK ADD CONSTRAINT [FK_M_RolePermission_T_Role] FOREIGN KEY([RoleID])
REFERENCES [dbo].[T_Role] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_M_RolePermission_T_Permission]') AND parent_object_id = OBJECT_ID(N'[dbo].[M_RolePermission]'))
ALTER TABLE [dbo].[M_RolePermission]  WITH CHECK ADD CONSTRAINT [FK_M_RolePermission_T_Permission] FOREIGN KEY([PermissionID])
REFERENCES [dbo].[T_Permission] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

/*****************************************************
User, Role, Permission
*****************************************************/


--Schedule Task

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_ScheduleTask]') AND type in (N'U'))
DROP TABLE [dbo].[T_ScheduleTask]
GO
CREATE TABLE [dbo].[T_ScheduleTask](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Seconds] [int] NOT NULL,
	[Type] [nvarchar](max) NOT NULL,
	[Enabled] [bit] NOT NULL,
	[StopOnError] [bit] NOT NULL,
	[LastStartTime] [datetime] NULL,
	[LastEndTime] [datetime] NULL,
	[LastSuccessTime] [datetime] NULL,

 CONSTRAINT [PK_D_Customer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



--Item

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_Item]') AND type in (N'U'))
DROP TABLE [dbo].[D_Item]
GO
CREATE TABLE [dbo].[D_Item](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SKU] [varchar](500) NOT NULL,
	[Type] int NOT NULL,
	[Name] [varchar](500) NOT NULL,	
	[Description] [nvarchar](max) NOT NULL,
	[Cost] decimal(18,2) NOT NULL,
	[Price] decimal(18,2) NOT NULL,
	[GrossWeight] decimal(18,8) NOT NULL,
	[NetWeight] decimal(18,8) NOT NULL,
	[CubicWeight] decimal(18,8) NOT NULL,
	[Length] decimal(18,8) NOT NULL,
	[Width] decimal(18,8) NOT NULL,
	[Height] decimal(18,8) NOT NULL,
	[SupplierID] int not null,
	[IgnoreListing] bit NOT NULL,
	[IsActive] bit NOT NULL,
	[Ref1] [varchar](4000) NOT NULL,
	[Ref2] [varchar](4000) NOT NULL,
	[Ref3] [varchar](4000) NOT NULL,
	[Ref4] [varchar](4000) NOT NULL,
	[Ref5] [varchar](4000) NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_D_Item] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO





IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_Item_Relationship]') AND type in (N'U'))
DROP TABLE [dbo].[D_Item_Relationship]
GO

/****** Object:  Table [dbo].[ProductRelationship]    Script Date: 09/15/2014 11:07:31 ******/

CREATE TABLE [dbo].[D_Item_Relationship](
	[ID] INT IDENTITY(1,1) NOT NULL,
	[ParentItemID] INT NOT NULL,
	[ChildItemID] INT NOT NULL,
	[ChildItemQty] INT NOT NULL,
	[DisplayOrder] INT NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,
 CONSTRAINT [PK_D_Item_Relationship] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_D_Item_Relationship_D_Item_ParentItemID]') AND parent_object_id = OBJECT_ID(N'[dbo].[D_Item_Relationship]'))
ALTER TABLE [dbo].[D_Item_Relationship]  WITH CHECK ADD CONSTRAINT [FK_D_Item_Relationship_D_Item_ParentItemID] FOREIGN KEY([ParentItemID])
REFERENCES [dbo].[D_Item] ([ID])
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_D_Item_Relationship_D_Item_ChildItemID]') AND parent_object_id = OBJECT_ID(N'[dbo].[D_Item_Relationship]'))
ALTER TABLE [dbo].[D_Item_Relationship]  WITH CHECK ADD CONSTRAINT [FK_D_Item_Relationship_D_Item_ChildItemID] FOREIGN KEY([ChildItemID])
REFERENCES [dbo].[D_Item] ([ID])
GO






--Images

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_Image]') AND type in (N'U'))
DROP TABLE [dbo].[D_Image]
GO
CREATE TABLE [dbo].[D_Image](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ImageName][varchar](500) NOT NULL,
	--[ImageOnlinePath] [varchar](max) NOT NULL,
	[ImageLocalPath] [nvarchar](max) NOT NULL,
	--[DisplayOrder] [int] NOT NULL,
	--[Status][varchar](20) NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_D_Image] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_ItemImage]') AND type in (N'U'))
DROP TABLE [dbo].[M_ItemImage]
GO
CREATE TABLE [dbo].[M_ItemImage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ItemID] [int] NOT NULL,
	[ImageID] [int] NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[StatusID] [int] NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_M_ItemImage] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_M_ItemImage_D_Image]') AND parent_object_id = OBJECT_ID(N'[dbo].[M_ItemImage]'))
ALTER TABLE [dbo].[M_ItemImage]  WITH CHECK ADD CONSTRAINT [FK_M_ItemImage_D_Image] FOREIGN KEY([ImageID])
REFERENCES [dbo].[D_Image] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_M_ItemImage_D_Item]') AND parent_object_id = OBJECT_ID(N'[dbo].[M_ItemImage]'))
ALTER TABLE [dbo].[M_ItemImage]  WITH CHECK ADD CONSTRAINT [FK_M_ItemImage_D_Item] FOREIGN KEY([ItemID])
REFERENCES [dbo].[D_Item] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO


--Job Item Images



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_JobItemImage]') AND type in (N'U'))
DROP TABLE [dbo].[M_JobItemImage]
GO
CREATE TABLE [dbo].[M_JobItemImage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[JobItemID] [int] NOT NULL,
	[ImageID] [int] NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[StatusID] [int] NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_M_JobItemImage] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_M_JobItemImage_D_Image]') AND parent_object_id = OBJECT_ID(N'[dbo].[M_JobItemImage]'))
ALTER TABLE [dbo].[M_JobItemImage]  WITH CHECK ADD CONSTRAINT [FK_M_JobItemImage_D_Image] FOREIGN KEY([ImageID])
REFERENCES [dbo].[D_Image] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_M_JobItemImage_D_Item]') AND parent_object_id = OBJECT_ID(N'[dbo].[M_JobItemImage]'))
ALTER TABLE [dbo].[M_JobItemImage]  WITH CHECK ADD CONSTRAINT [FK_M_JobItemImage_D_JobItem] FOREIGN KEY([JobItemID])
REFERENCES [dbo].[D_JobItem] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO



--Order 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_Order_Header]') AND type in (N'U'))
DROP TABLE [dbo].[D_Order_Header]
GO
CREATE TABLE [dbo].[D_Order_Header](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TypeID] [int] NOT NULL,
	[StatusID] [int] NOT NULL,--Downloaded,NotPaid,AddressIncorrect,Released,Shipped,Cancelled
	[OrderTime] datetime not null,
	[ChannelOrderID] [varchar](500) NOT NULL,
	[CustomerID] [varchar](500) NOT NULL,
	[ConsigneeName][varchar](500) NOT NULL,
	[ShippingAddress1][varchar](500) NOT NULL,
	[ShippingAddress2][varchar](500) NOT NULL,
	[ShippingSuburb][varchar](500) NOT NULL,
	[ShippingState][varchar](500) NOT NULL,
	[ShippingPostcode][varchar](500) NOT NULL,
	[ShippingCountry][varchar](500) NOT NULL,
	[ConsigneeEmail][varchar](500) NOT NULL,
	[ConsigneePhoneNo][varchar](500) NOT NULL,
	[BillingName][varchar](500) NOT NULL,
	[BillingAddress1][varchar](500) NOT NULL,
	[BillingAddress2][varchar](500) NOT NULL,
	[BillingSuburb][varchar](500) NOT NULL,
	[BillingState][varchar](500) NOT NULL,
	[BillingPostcode][varchar](500) NOT NULL,
	[BillingCountry][varchar](500) NOT NULL,
	[BillingEmail][varchar](500) NOT NULL,
	[BillingPhoneNo][varchar](500) NOT NULL,
	[SubTotal] decimal(18,2) NOT NULL,	
	[Postage] decimal(18,2) NOT NULL,
	[TotalAmount] decimal(18,2) NOT NULL,
	[ShippingMethod] [varchar](500) NOT NULL,
	[PaymentMethod] [varchar](500) NOT NULL,
	[PaymentTransactionID] [varchar](500) NOT NULL,
	[PaidTime] datetime null,
	[Carrier] [varchar](500) NOT NULL,
	[BuyerNote] [varchar](500) NOT NULL,
	[OrderNote] [varchar](500) NOT NULL,
	[ShipmentTime] datetime null,
	[Ref1] [varchar](500) NOT NULL,
	[Ref2] [varchar](500) NOT NULL,
	[Ref3] [varchar](500) NOT NULL,
	[Ref4] [varchar](500) NOT NULL,
	[Ref5] [varchar](500) NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](500) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](500) not null,

 CONSTRAINT [PK_D_Order_Header] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO




IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_Order_Line]') AND type in (N'U'))
DROP TABLE [dbo].[D_Order_Line]
GO
CREATE TABLE [dbo].[D_Order_Line](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[SKU] [varchar](500) NOT NULL,
	[Qty] [int] not null,
	[ItemPrice] decimal(18,2) NOT NULL,
	[SubTotal] decimal(18,2) NOT NULL,
	[Ref1] [varchar](500) NOT NULL,
	[Ref2] [varchar](500) NOT NULL,
	[Ref3] [varchar](500) NOT NULL,
	[Ref4] [varchar](500) NOT NULL,
	[Ref5] [varchar](500) NOT NULL,
	[Ref6] [varchar](500) NOT NULL,
	[Ref7] [varchar](500) NOT NULL,
	[Ref8] [varchar](500) NOT NULL,
	[Ref9] [varchar](500) NOT NULL,
	[Ref10] [varchar](500) NOT NULL,
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



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_IDSequence]') AND type in (N'U'))
DROP TABLE [dbo].[T_IDSequence]
GO
CREATE TABLE [dbo].[T_IDSequence](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](500) NOT NULL,
	[IDType] [varchar](20) NOT NULL,
	[IDSequence] [int] NOT NULL,
	[PreFix] [varchar](10) NOT NULL,
	[Length] [int] NOT NULL,
	[DateStr] [varchar](10) NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](500) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](500) not null,
 CONSTRAINT [PK_T_IDSequence] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

insert into T_IDSequence
select 
'Job Item Sequence Number By Day' as Description,
'JOBITEM' as IDType,
1 as IDSequence,
'' as PreFix,
0 as Length,
'20190718' as CurrentDateStr,
GETDATE() as CreateTime,
'System' as CreateBy,
GETDATE() as EditTime,
'System' as EditBy





Create  PROCEDURE [dbo].[GetSequenceID]
		  @id int


as 
BEGIN 
	declare @returnSequenceNumber varchar(100)
	Declare @idSequence int
	Declare @length int
	Declare @preFix	VarChar(10)
	Declare @DateStr varChar(10)
	Declare @CurrentDateStr varchar(10)

	set @returnSequenceNumber=''
	Set @Length=0

	begin tran
		Select @idSequence = IDSequence,
		@length=Length,
		@preFix=PreFix,
		@DateStr=DateStr
		From T_IDSequence with(updlock)
		where ID=@id

		if @idSequence is null or @@error<>0
		begin
			rollback tran
			set @returnSequenceNumber=''
			select @returnSequenceNumber as ReturnSequenceNumber
			return
		end

		if @DateStr<>''
		begin
			Set @CurrentDateStr=convert(char(8),getdate(),112)
			if @CurrentDateStr=@DateStr
				set @idSequence=isnull(@idSequence,0)+1
			else
				set @idSequence=1
		end
		else
		begin
			Set @idSequence=@idSequence + 1
			IF LEN(@idSequence)>@Length Set @idSequence=1
		end

		set @Length=@Length - len(cast(@idSequence as varchar(50)))
		if @Length>0 
		   set @returnSequenceNumber=@PreFix+replace(space(@Length)+rtrim(cast(@idSequence as varchar(50))),' ','0')
		else
		   set @returnSequenceNumber=@PreFix+rtrim(cast(@idSequence as varchar(50)))

		update T_IDSequence
		set 
		IDSequence=@idSequence,
		DateStr=@CurrentDateStr
		where ID=@id
		if @@error<>0
		begin
			rollback tran
			set @returnSequenceNumber=''
			select @returnSequenceNumber as ReturnSequenceNumber
			return
		end
	commit tran

	select @returnSequenceNumber as ReturnSequenceNumber
END


--Add Note
IF NOT EXISTS (SELECT * FROM SysObjects O INNER JOIN SysColumns C ON O.ID=C.ID WHERE
 ObjectProperty(O.ID,'IsUserTable')=1 AND O.Name='D_JobItem' AND C.Name='Note')
	ALTER TABLE dbo.D_JobItem ADD
		Note varchar(4000) NOT NULL CONSTRAINT DF_D_JobItem_Note DEFAULT ''
GO
		
IF EXISTS (SELECT [name] FROM sysobjects WHERE [name] = 'DF_D_JobItem_Note')
	ALTER TABLE dbo.D_JobItem
		DROP CONSTRAINT DF_D_JobItem_Note
GO


--Add PricePercentage
IF NOT EXISTS (SELECT * FROM SysObjects O INNER JOIN SysColumns C ON O.ID=C.ID WHERE
 ObjectProperty(O.ID,'IsUserTable')=1 AND O.Name='D_JobItem' AND C.Name='PricePercentage')
	ALTER TABLE dbo.D_JobItem ADD
		PricePercentage decimal(18,2) NOT NULL CONSTRAINT DF_D_JobItem_PricePercentage DEFAULT 1
GO
		
IF EXISTS (SELECT [name] FROM sysobjects WHERE [name] = 'DF_D_JobItem_PricePercentage')
	ALTER TABLE dbo.D_JobItem
		DROP CONSTRAINT DF_D_JobItem_PricePercentage
GO


--Add StocktakeTime
IF NOT EXISTS (SELECT * FROM SysObjects O INNER JOIN SysColumns C ON O.ID=C.ID WHERE
 ObjectProperty(O.ID,'IsUserTable')=1 AND O.Name='D_JobItem' AND C.Name='StocktakeTime')
	ALTER TABLE dbo.D_JobItem ADD
		StocktakeTime datetime null CONSTRAINT DF_D_JobItem_StocktakeTime DEFAULT null
GO
		
IF EXISTS (SELECT [name] FROM sysobjects WHERE [name] = 'DF_D_JobItem_StocktakeTime')
	ALTER TABLE dbo.D_JobItem
		DROP CONSTRAINT DF_D_JobItem_StocktakeTime
GO



--Schedule Rule
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_ScheduleRule]') AND type in (N'U'))
DROP TABLE [dbo].[T_ScheduleRule]
GO
CREATE TABLE [dbo].[T_ScheduleRule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[IntervalDay] [int] NOT NULL,
	[LastSuccessTime] datetime not null,
	[IsActive] bit not null,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_T_ScheduleRule] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_ScheduleRuleLine]') AND type in (N'U'))
DROP TABLE [dbo].[T_ScheduleRuleLine]
GO
CREATE TABLE [dbo].[T_ScheduleRuleLine](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ScheduleRuleID] [int] NOT NULL,
	[TimeRangeFrom] datetime NOT NULL,
	[TimeRangeTo] datetime NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_T_ScheduleRuleLine] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


insert into T_ScheduleTask
select 
'Sync Inventory For Last Day',
300,
'ThirdStoreBusiness.ScheduleTask.SyncInventoryForLastDayTask, ThirdStoreBusiness',
0,
0,
GETDATE(),
GETDATE(),
GETDATE()

insert into T_ScheduleRule
select 'SyncInventoryForLastDayTask','SyncInventoryForLastDayTask',1,GETDATE(),1,getdate(),'System',GETDATE(),'System'


insert into T_ScheduleRuleLine
select 1,'2016-02-01 00:00:30.000','2016-02-01 23:59:59.000',getdate(),'System',GETDATE(),'System'


--insert fake data


insert into D_JobItem
select 
GETDATE() as JobItemCreateTime,
1 as Type,
1 as StatusID,
1 as ConditionID,
1 as SupplierID,
'Giantz 2.05 x 2.57m Steel Garden Shed with Roof - Grey' as ItemName,
'item is wrong sent with perfect condition,the second qty for this item' as ItemDetail,
280 as ItemPrice,
'' as DesignatedSKU,
null as ShipTime,
'' as TrackingNumber,
'' as Ref1,
'' as Ref2,
'' as Ref3,
'' as Ref4,
'' as Ref5,
GETDATE() as CreateTime,
'System' as CreateBy,
GETDATE() as EditTime,
'System' as EditBy


insert into D_JobItemLine
select 
3 as HeaderID,
1 as ItemID,
'SHED-GAB-6X8-A' as SKU,
1 as Qty,
'' as SupplierRef,
1 as IsOrginalPackage,
41.00000000 as Weight,
0.33000000 as Length,
0.64000000 as Width,
0.32000000 as Height,
0 as CubicWeight,
'' as Ref1,
'' as Ref2,
'' as Ref3,
'' as Ref4,
'' as Ref5,
GETDATE() as CreateTime,
'System' as CreateBy,
GETDATE() as EditTime,
'System' as EditBy




insert into D_JobItemLine
select 
3 as HeaderID,
2 as ItemID,
'SHED-GAB-6X8-B' as SKU,
1 as Qty,
'' as SupplierRef,
1 as IsOrginalPackage,
41.00000000 as Weight,
0.33000000 as Length,
0.64000000 as Width,
0.32000000 as Height,
0 as CubicWeight,
'' as Ref1,
'' as Ref2,
'' as Ref3,
'' as Ref4,
'' as Ref5,
GETDATE() as CreateTime,
'System' as CreateBy,
GETDATE() as EditTime,
'System' as EditBy



insert into D_JobItemLine
select 
3 as HeaderID,
3 as ItemID,
'SHED-GAB-6X8-C' as SKU,
1 as Qty,
'' as SupplierRef,
1 as IsOrginalPackage,
41.00000000 as Weight,
0.33000000 as Length,
0.64000000 as Width,
0.32000000 as Height,
0 as CubicWeight,
'' as Ref1,
'' as Ref2,
'' as Ref3,
'' as Ref4,
'' as Ref5,
GETDATE() as CreateTime,
'System' as CreateBy,
GETDATE() as EditTime,
'System' as EditBy




insert into D_JobItemLine
select 
3 as HeaderID,
4 as ItemID,
'SHED-GAB-6X8-D' as SKU,
1 as Qty,
'' as SupplierRef,
1 as IsOrginalPackage,
41.00000000 as Weight,
0.33000000 as Length,
0.64000000 as Width,
0.32000000 as Height,
0 as CubicWeight,
'' as Ref1,
'' as Ref2,
'' as Ref3,
'' as Ref4,
'' as Ref5,
GETDATE() as CreateTime,
'System' as CreateBy,
GETDATE() as EditTime,
'System' as EditBy




/****** Object:  UserDefinedFunction [dbo].[fn_SplitString]    Script Date: 28/09/2019 10:32:45 AM ******/
--DROP FUNCTION [dbo].[fn_SplitString]
--GO


CREATE FUNCTION [dbo].[fn_SplitString]
(
	@str VARCHAR(MAX),
	@delimiter varchar(10)
)
RETURNS TABLE
AS
	RETURN
	
	SELECT  Split.a.value('.', 'VARCHAR(1000)') AS SplittedValue  
	 FROM  
	 (
		 SELECT
			 CAST ('<M>' + REPLACE(@str, @delimiter, '</M><M>') + '</M>' AS XML) AS Data  
	 ) AS A CROSS APPLY Data.nodes ('/M') AS Split(a);
GO



--/****** Object:  UserDefinedFunction [dbo].[fn_GetAffectedItems]    Script Date: 28/09/2019 10:41:19 AM ******/
--DROP FUNCTION [dbo].[fn_GetAffectedItemsByJobItems]
--GO


CREATE FUNCTION [dbo].[fn_GetAffectedItemsByJobItems]
(
	@JobItemIDs varchar(MAX)
)
RETURNS TABLE
AS
	RETURN

select
distinct 
ITEM.ID
--ITEM.SKU
from V_SKURelationship_Recursive I
inner join
(
	select 
	distinct
	case when H.DesignatedSKU<>'' then H.DesignatedSKU else L.SKU end HSKU,
	H.*
	from D_JobItem H
	inner join D_JobItemLine L on H.ID=L.HeaderID
	where 
	H.ID in (select SplittedValue as ID from fn_SplitString(@JobItemIDs,','))
) AFF on I.SKU=AFF.HSKU
inner join V_SKURelationship_Recursive O on I.BottomSKU=O.BottomSKU
inner join D_Item ITEM on ITEM.SKU=O.SKU and ITEM.IgnoreListing=0
	
GO




--read neto product xml data
DECLARE @xml XML
SELECT @xml = BulkColumn
FROM OPENROWSET(BULK 'C:\Temp\SampleNetoProduct.xml', SINGLE_BLOB) x

insert into NetoProducts
select 
--top 10
CONVERT(VARCHAR, t.query('./*:ID/text()')) as ID,
CONVERT(VARCHAR(128), t.query('./*:SKU/text()')) as SKU,
CONVERT(VARCHAR, t.query('./*:DefaultPrice/text()')) as DefaultPrice,
CONVERT(VARCHAR(500), t.query('./*:Name/text()')) as Name,
CONVERT(nvarchar(max), t.query('./*:Description/text()')) as Description,
--t.query('./*:Description/text()') as Description,
CONVERT(VARCHAR, t.query('./*:PrimarySupplier/text()')) as PrimarySupplier,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[1]/URL/text()')) as Image1,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[2]/URL/text()')) as Image2,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[3]/URL/text()')) as Image3,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[4]/URL/text()')) as Image4,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[5]/URL/text()')) as Image5,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[6]/URL/text()')) as Image6,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[7]/URL/text()')) as Image7,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[8]/URL/text()')) as Image8,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[9]/URL/text()')) as Image9,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[10]/URL/text()')) as Image10,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[11]/URL/text()')) as Image11,
CONVERT(VARCHAR(500), t.query('./*:Images/Image[12]/URL/text()')) as Image12,
--CONVERT(VARCHAR(500), t.query('./*:Images/Image[13]/URL/text()')) as Image13,
CONVERT(VARCHAR, t.query('./*:ShippingLength/text()')) as ShippingLength,
CONVERT(VARCHAR, t.query('./*:ShippingHeight/text()')) as ShippingHeight,
CONVERT(VARCHAR, t.query('./*:ShippingWidth/text()')) as ShippingWidth,
CONVERT(VARCHAR, t.query('./*:ShippingWeight/text()')) as ShippingWeight
--into NetoProducts
from @xml.nodes('//*:GetItemResponse/*:Item') products(t) 




select 
S.sku,
CONVERT(NVARCHAR(4000), s.query('./*:Name/text()')) as ItemSpecificName,
CONVERT(NVARCHAR(4000), s.query('./*:Value/text()')) as ItemSpecificValue
from
(
	select 
	sku,
	CONVERT(xml,ItemSpecific) as ItemSpecific
	from OZPlazaeBaySpecific
) S
cross apply S.ItemSpecific.nodes('//*:ItemSpecifics/*:NameValueList') ebaySpec(s)







select * 
from NetoProducts N
left join
( 
	select 
	SKU,
	CONVERT(xml,ItemSpecific) as ItemSpecific
	from 
	(
		select * 
		from 
		OZPlazaeBaySpecificWithID S2
		where Num in
		(
			select
			MAX(Num) as ID
			--S1.SKU
			--S1.ItemSpecific
			from
			(
				select 
				Num,
				SKU,
				--CONVERT(xml,ItemSpecific) as ItemSpecific
				ItemSpecific
				from OZPlazaeBaySpecificWithID
				--order by SKU
				--where SKU='DIFF-R3B-LW'
			) S1
			group by S1.SKU
		) 
	) S
) SKUSpec on N.SKU=SKUSpec.SKU
 where N.PrimarySupplier='P'
and SKUSpec.SKU is null





 select 
 T1.SKU,
 Brand=(select dbo.UDFHTMLDecode(ItemSpecificValue)  FROM
	OZPlazaItemSpecificNameValue
	where SKU=T1.SKU and  itemspecificname in ('Brand')
 FOR XML PATH (''),TYPE).value('(./text())[1]','VARCHAR(MAX)'),
 STUFF(
(select ';' + ItemSpecificName+':'+dbo.UDFHTMLDecode(ItemSpecificValue)  FROM
	OZPlazaItemSpecificNameValue
	where SKU=T1.SKU and  itemspecificname not in ('Brand','Features','Package  Contents')
 FOR XML PATH (''),TYPE).value('(./text())[1]','VARCHAR(MAX)')
 ,1,1,'') AS NameValues
 from 
OZPlazaItemSpecificNameValue T1
group by T1.SKU




select 
ID as NetoProductID,
SKU,
Name,
DefaultPrice as Price
from NetoProducts NP
where  not exists (
select 1 from
(
	select 
	case when LEN(SKU)>25 and Ref2<>'' then Ref2 else SKU end as SKU
	from D_Item
	union
	select 
	case when LEN(SKU)>23 and Ref2<>'' then Ref2+'_D' else SKU+'_D' end as SKU
	from D_Item
) L
where NP.SKU=L.SKU
)

--^0\.\d{1,2}$





update T_User
set
Description='C'
where id in (4)

update T_User
set
Description='K'
where id in (6)

update T_User
set
Description='W'
where id in (11)

update T_User
set
Description='Q'
where id in (15)

update T_User
set
Description='L'
where id in (13)

update T_User
set
Description='S'
where id in (9)

update T_User
set
Description='T'
where id in (7)

update T_User
set
Description='Y'
where id in (8)

update T_User
set
Description='D'
where id in (10)

update T_User
set
Description='Z'
where id in (16)

update T_User
set
Description='A'
where id in (5)

update T_User
set
Description='J'
where id in (12)

update T_User
set
Description='E'
where id in (18)




insert into T_ScheduleTask
select 
'Update DSZ Data And Sync',
43200,
'ThirdStoreBusiness.ScheduleTask.UpdateDSDataAndSync, ThirdStoreBusiness',
0,
0,
GETDATE(),
GETDATE(),
GETDATE()


select * from fn_GetAffectedItems('20191219','20191219') A
inner join D_Item I on A.ID=I.ID


--connect row into string with comma

declare @tmp varchar(max)
SET @tmp = ''
select @tmp = @tmp + ID + ',' from 
(select CONVERT(varchar, ID) ID from D_Item
where Cost>50 and Cost<=100
and SupplierID=1
and IsActive=1
and Name<>'' and Description<>''
and Type<>1) I

select SUBSTRING(@tmp, 0, LEN(@tmp))