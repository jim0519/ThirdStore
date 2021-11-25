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


--fetch neto products
EXEC sp_rename 'NetoProducts.ID', 'NetoProductID', 'COLUMN';

ALTER TABLE NetoProducts ADD ID int identity(1,1) not null
GO
ALTER TABLE NetoProducts
add CONSTRAINT pk_NetoProducts_ID primary key(ID)
GO

ALTER TABLE dbo.NetoProducts ADD Qty varchar(50) NULL DEFAULT '0'






IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_Return]') AND type in (N'U'))
DROP TABLE [dbo].[D_Return]
GO

/****** Object:  Table [dbo].[D_Return]    Script Date: 09/15/2014 11:07:31 ******/

CREATE TABLE [dbo].[D_Return](
	[ID] INT IDENTITY(1,1) NOT NULL,
	[Reason] [varchar](4000) NOT NULL,
	[Detail] [nvarchar](max) NOT NULL,
	[ChildItemQty] INT NOT NULL,
	[DisplayOrder] INT NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,
 CONSTRAINT [PK_D_Return] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_D_Return_D_JobItem_JobItemID]') AND parent_object_id = OBJECT_ID(N'[dbo].[D_Return]'))
ALTER TABLE [dbo].[D_Return]  WITH CHECK ADD CONSTRAINT [FK_D_Return_D_JobItem] FOREIGN KEY([JobItemID])
REFERENCES [dbo].[D_JobItem] ([ID])
GO




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



--Bulk Sync Dropship SKU


DECLARE @ID VARCHAR(max) 
SELECT @ID = COALESCE(@ID + ',', '') +cast( ID as varchar)

--select *
 from D_Item
where
 SupplierID=1
 and Cost<=200 and Cost>100
 and Type in (2,3)
 and IsReadyForList=0
 and ((LEN(SKU)>23 and Ref2<>'' )or LEN(SKU)<=23)
 and Name<>''
 and Description <>''

select @ID



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
and Type<>1
and ((LEN(sku)<=23) or
(LEN(sku)>23 and Ref2<>''))
) I

select SUBSTRING(@tmp, 0, LEN(@tmp))





--neto上的active listing但是没有我们系统上job item，也不是ds的

select * from ThirdStore.dbo.NetoProducts
where SKU not in 
(
	select 
	distinct
	case when H.DesignatedSKU<>'' then H.DesignatedSKU else L.SKU end HSKU
	from D_JobItem H
	inner join D_JobItemLine L on H.ID=L.HeaderID
	where StatusID in (1,2,3) and Type=1
)
and sku not in
(
	select SKU from D_Item
	where Cost<=100
	and SupplierID=1
	and IsActive=1
	and Name<>'' and Description<>''
	and Type<>1
	and ((LEN(sku)<=23) or
	(LEN(sku)>23 and Ref2<>''))
)
and SKU not like '%_D'
and convert(int,Qty)>0


--Import fastway rate on neto

select 
'AU' as Country,
'Fastway Jim 20200728' as Courier, 
Town as [City/Suburb],
Postcode as [From Post Code],
Postcode as [To Post Code],
ZoneCode,
ZoneCode as ZoneName
from
(
	select Town,Postcode,[Local Product] as ZoneCode from ThirdStoreFastwayPostcode
	where [Local Product] is not null

union all

	select Town,Postcode,[Road Product] as ZoneCode from ThirdStoreFastwayPostcode
	where [Road Product] is not null

union all
	
	select Town,Postcode,[Shorthaul Product] as ZoneCode from ThirdStoreFastwayPostcode
	where [Shorthaul Product] is not null

union all

	select Town,Postcode,[Satchel Product] as ZoneCode from ThirdStoreFastwayPostcode
	where [Satchel Product] is not null

union all

	select Town,Postcode,[Fastway Boxes Product] as ZoneCode from ThirdStoreFastwayPostcode
	where [Fastway Boxes Product] is not null
) S




--Permission Change


insert into T_Role
select 
'Administrator' as Name,
'Administrator' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'



insert into T_Role
select 
'CustomerService' as Name,
'Customer Service' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'


insert into T_Role
select 
'WarehouseStaff' as Name,
'Warehouse Staff' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'

insert into T_Role
select 
'WarehouseManager' as Name,
'Warehouse Manager' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'

insert into T_Role
select 
'ListingManager' as Name,
'Listing Manager' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'



insert into T_Permission
select 
'KPIReport' as Name,
'KPI Report' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'


insert into T_Permission
select 
'OrderList' as Name,
'Order List' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'


insert into T_Permission
select 
'OrderEdit' as Name,
'Order Edit' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'

insert into T_Permission
select 
'JobItemSync' as Name,
'Job Item Sync' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'


insert into T_Permission
select 
'SKUEdit' as Name,
'SKU Edit' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'

insert into T_Permission
select 
'SKUCreate' as Name,
'SKU Create' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'


insert into T_Permission
select 
'UserAccessControl' as Name,
'User Access Control' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'

insert into T_Permission
select 
'SKUList' as Name,
'SKU List' as Description,
1 as IsActive,
GETDATE(),
'System',
GETDATE(),
'System'


--User Role

insert into M_UserRole
select 
ID as UserID,
 (select ID from T_Role where Name='Administrator') as RoleID,
 GETDATE(),
'System',
GETDATE(),
'System'
from T_User
where Name in ('Jim','Sandy','Keith')


insert into M_UserRole
select 
ID as UserID,
 (select ID from T_Role where Name='CustomerService') as RoleID,
 GETDATE(),
'System',
GETDATE(),
'System'
from T_User
where Name in ('Kun','Danni','William','Jaizen','Peggy','Chenny','Floria')


insert into M_UserRole
select 
ID as UserID,
 (select ID from T_Role where Name='WarehouseManager') as RoleID,
 GETDATE(),
'System',
GETDATE(),
'System'
from T_User
where Name in ('Kun','Danni','William','Jaizen','Peggy','Chenny','Floria')



insert into M_UserRole
select 
ID as UserID,
 (select ID from T_Role where Name='WarehouseStaff') as RoleID,
 GETDATE(),
'System',
GETDATE(),
'System'
from T_User
where Name in ('Linyan','Sunny','Isa','Qiang','Ken','Raisy','Kevin','Jeffery')



insert into M_UserRole
select 
ID as UserID,
 (select ID from T_Role where Name='ListingManager') as RoleID,
 GETDATE(),
'System',
GETDATE(),
'System'
from T_User
where Name in ('Danni','Peggy')






--Role Permission
insert into M_RolePermission
select
 (select id from T_Role where Name='Administrator') as RoleID,
 ID as PermissionID,
 GETDATE(),
'System',
GETDATE(),
'System'
 from T_Permission


 insert into M_RolePermission
select
 (select id from T_Role where Name='CustomerService') as RoleID,
 ID as PermissionID,
 GETDATE(),
'System',
GETDATE(),
'System'
 from T_Permission
 where Name in ('OrderList','OrderEdit')



-- insert into M_RolePermission
--select
-- (select id from T_Role where Name='WarehouseStaff') as RoleID,
-- ID as PermissionID,
-- GETDATE(),
--'System',
--GETDATE(),
--'System'
-- from T_Permission
-- where Name in ('SKUEdit')


  insert into M_RolePermission
select
 (select id from T_Role where Name='WarehouseManager') as RoleID,
 ID as PermissionID,
 GETDATE(),
'System',
GETDATE(),
'System'
 from T_Permission
 where Name in ('SKUEdit')


 
 insert into M_RolePermission
select
 (select id from T_Role where Name='ListingManager') as RoleID,
 ID as PermissionID,
 GETDATE(),
'System',
GETDATE(),
'System'
 from T_Permission
 where Name in ('JobItemSync','OrderList','OrderEdit','SKUEdit')





select
U.Name,
P.Name as PermissionName
from T_User U
inner join M_UserRole UR on U.ID=UR.UserID
inner join M_RolePermission RP on RP.RoleID=UR.RoleID
inner join T_Permission P on RP.PermissionID=P.ID
where U.Name in ('Jaizen')








IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Setting]') AND type in (N'U'))
DROP TABLE [dbo].[T_Setting]
GO
CREATE TABLE [dbo].[T_Setting](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[Value] [varchar](max) NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](500) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](500) not null,

 CONSTRAINT [PK_T_Setting] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO




insert into T_Setting
select
'commonsettings.shippingdescriptiontemplate',
'',
'<p>&nbsp;</p>
<p align="center"><strong><em><u>Shipping</u></em></strong></p>
<p>&nbsp;</p>
<p>We offer free shipping Australia wide; all items are shipped from our Melbourne warehouse after payment is confirmed. Delivery normally takes 4-7 days (usually faster) depending on delivery location. All shipping is set as <u>Authorize to Leave (ATL).</u></p>
<p><strong><u>Packaging maybe repacked to compromise on economic shipping size requirements.</u></strong></p>
<p><strong><em>Item cover&nbsp;6 months manufacture warranty with return shipping included. There will be a 15%&nbsp;restocking fee for change of mind.</em></strong></p>
<p>We require a postal address&nbsp;NOT a PO Box or Parcel Locker. Please ensure you provide a&nbsp;correct postal address&nbsp;and a&nbsp;daytime contact number&nbsp;or it may cause a delay in your shipment.</p>
<p>Due to the limited access of our carriers, there are certain postcodes that we are unable to deliver to. Please refer to the list below for more details.</p>
<table width="576">
<tbody>
<tr>
<td width="368">
<p>Postcode</p>
</td>
<td width="208">
<p>State</p>
</td>
</tr>
<tr>
<td width="368">
<p>0800-0999</p>
</td>
<td width="208">
<p>NT</p>
</td>
</tr>
<tr>
<td width="368">
<p>2641</p>
</td>
<td width="208">
<p>NSW</p>
</td>
</tr>
<tr>
<td width="368">
<p>4185,4450-4499, 4680, 4700-4805, 9920-9959</p>
</td>
<td width="208">
<p>QLD</p>
</td>
</tr>
<tr>
<td width="368">
<p>4806-4899, 4900-4999, 9960-9999</p>
</td>
<td width="208">
<p>QLD</p>
</td>
</tr>
<tr>
<td width="368">
<p>5701</p>
</td>
<td width="208">
<p>SA</p>
</td>
</tr>
<tr>
<td width="368">
<p>6055</p>
</td>
<td width="208">
<p>WA</p>
</td>
</tr>
<tr>
<td width="368">
<p>7151</p>
</td>
<td width="208">
<p>TAS</p>
</td>
</tr>
<tr>
<td width="368">
<p>6215-6699</p>
</td>
<td width="208">
<p>WA</p>
</td>
</tr>
<tr>
<td width="368">
<p>6700-6799</p>
</td>
<td width="208">
<p>WA</p>
</td>
</tr>
</tbody>
</table>
<p><strong><u>&nbsp;</u></strong></p>
<p><strong><u>We aim to provide the best saving for our customers. Item maybe sent via different courier services with different tracking no. in order to fit the economical shipping requirement for item size and weight. As a result, parcels may arrive in different day. Thanks for the understanding</u></strong></p>
',
GETDATE(),
'System',
GETDATE(),
'System'


insert into T_Setting
select
'commonsettings.dropshipmarkuprate',
'',
'1.4',
GETDATE(),
'System',
GETDATE(),
'System'




---Shrink log file
ALTER DATABASE ThirdStore SET RECOVERY SIMPLE
 DBCC SHRINKFILE ([3rdStore_log], 1)


-- Add Ref6 in D_Item
IF NOT EXISTS (SELECT * FROM SysObjects O INNER JOIN SysColumns C ON O.ID=C.ID WHERE
 ObjectProperty(O.ID,'IsUserTable')=1 AND O.Name='D_Item' AND C.Name='Ref6')
	ALTER TABLE dbo.D_Item ADD
		Ref6 varchar(4000) NOT NULL CONSTRAINT DF_D_Item_Ref6 DEFAULT ''
GO
		
IF EXISTS (SELECT [name] FROM sysobjects WHERE [name] = 'DF_D_Item_Ref6')
	ALTER TABLE dbo.D_Item
		DROP CONSTRAINT DF_D_Item_Ref6
GO




-- faulty rate
select 
AGGR.*,
--CONVERT(decimal(10,2), AGGR.DandP/AGGR.TotalQty) as FaultyRate
case 
 when I.SupplierID=1 then 'P'
 when I.SupplierID=2 then 'A'
 when I.SupplierID=3 then 'S'
 when I.SupplierID=4 then 'T'
 when I.SupplierID=5 then 'O' end  Supplier ,
 I.Name,
 I.Price,
 I.Ref4 Link,
CONVERT(decimal(10,2), AGGR.DandP)/CONVERT(decimal(10,2),AGGR.TotalQty)*100 as FaultyRate
from 
(
	select 
	SKU,
	SUM(case when Type=1 and ConditionID=1 then 1 else 0 end) SelfNew,
	SUM(case when Type=1 and ConditionID=2 then 1 else 0 end) SelfUsed,
	SUM(case when Type in (2,3) then 1 else 0 end) DandP,
	COUNT(1) TotalQty
	from
	(
		select 
		distinct
		case when H.DesignatedSKU<>'' then H.DesignatedSKU else L.SKU end SKU,
		H.*
		from D_JobItem H
		inner join D_JobItemLine L on H.ID=L.HeaderID

	) DA
	group by SKU
) AGGR
inner join D_Item I on AGGR.SKU=I.SKU




insert into T_Setting
select
'dropshipzoneapisettings.url',
'',
'https://api.dropshipzone.com.au',
GETDATE(),
'System',
GETDATE(),
'System'


insert into T_Setting
select
'dropshipzoneapisettings.email',
'',
'enquiry@3rdstore.com.au',
GETDATE(),
'System',
GETDATE(),
'System'


insert into T_Setting
select
'dropshipzoneapisettings.password',
'',
'Zh*UFatmeL6f',
GETDATE(),
'System',
GETDATE(),
'System'



--Add Log In DB Infrastructure

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Log]') AND type in (N'U'))
DROP TABLE [dbo].[T_Log]
GO
CREATE TABLE [dbo].[T_Log](
	[ID] [int] IDENTITY (1, 1) NOT NULL,
    [Date] [datetime] NOT NULL,
    [Thread] [varchar] (255) NOT NULL,
    [Level] [varchar] (50) NOT NULL,
    [Logger] [varchar] (255) NOT NULL,
    [Message] [varchar] (4000) NOT NULL,
    [Exception] [varchar] (2000) NULL

 CONSTRAINT [PK_T_Log] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


--New Aim SKU Barcode


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_NewAimSKUBarcode]') AND type in (N'U'))
DROP TABLE [dbo].[D_NewAimSKUBarcode]
GO
CREATE TABLE [dbo].[D_NewAimSKUBarcode](
	[ID] [int] IDENTITY (1, 1) NOT NULL,
    [SKU] [varchar] (500) NOT NULL,
    [AlternateSKU1] [varchar] (500) NOT NULL,
    [AlternateSKU2] [varchar] (500) NOT NULL,
    [CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null

 CONSTRAINT [PK_D_NewAimSKUBarcode] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO




IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_ReturnItem]') AND type in (N'U'))
DROP TABLE [dbo].[D_ReturnItem]
GO
CREATE TABLE [dbo].[D_ReturnItem](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StatusID] int NOT NULL,
	[DesignatedSKU] [varchar](500) not null,
	[TrackingNumber] [varchar](100) not null,
	[Note] varchar(4000) NOT NULL,
	[Ref1] [varchar](4000) NOT NULL,
	[Ref2] [varchar](4000) NOT NULL,
	[Ref3] [varchar](4000) NOT NULL,
	[Ref4] [varchar](4000) NOT NULL,
	[Ref5] [varchar](4000) NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_D_ReturnItem] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO




IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[D_ReturnItemLine]') AND type in (N'U'))
DROP TABLE [dbo].[D_ReturnItemLine]
GO
CREATE TABLE [dbo].[D_ReturnItemLine](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HeaderID] [int] NOT NULL,
	[ItemID] [int] NOT NULL,
	[SKU] [varchar](500) NOT NULL,
	[Qty] [int] not null,
	[Weight] decimal(18,8) NOT NULL,
	[Length] decimal(18,8) NOT NULL,
	[Width] decimal(18,8) NOT NULL,
	[Height] decimal(18,8) NOT NULL,
	[CubicWeight] decimal(18,8) NOT NULL,
	[Location] [varchar](500) NOT NULL,
	[Ref1] [varchar](4000) NOT NULL,
	[Ref2] [varchar](4000) NOT NULL,
	[Ref3] [varchar](4000) NOT NULL,
	[Ref4] [varchar](4000) NOT NULL,
	[Ref5] [varchar](4000) NOT NULL,
	[CreateTime] datetime not null,
	[CreateBy] [varchar](100) not null,
	[EditTime] datetime not null,
	[EditBy] [varchar](100) not null,

 CONSTRAINT [PK_D_ReturnItemLine] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_D_ReturnItemLine_D_ReturnItem]') AND parent_object_id = OBJECT_ID(N'[dbo].[D_ReturnItemLine]'))
ALTER TABLE [dbo].[D_ReturnItemLine]  WITH CHECK ADD CONSTRAINT [FK_D_ReturnItemLine_D_ReturnItem] FOREIGN KEY([HeaderID])
REFERENCES [dbo].[D_ReturnItem] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO




--insert into D_NewAimSKUBarcode
select 
sku,
[alternate sku1],
[alternate sku2],
GETDATE(),
'System',
GETDATE(),
'System'
from NewaimSKUBarcodeRaw
where sku is not null