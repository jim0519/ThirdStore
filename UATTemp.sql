select * from NetoProducts
where PrimarySupplier='P'
and SKU='MO-DIN-03-GYX2'
order by ID


select * 
from DropshipzoneSKU DS
where SKU='MO-DIN-03-GY'
where [Weight (kg)] is null


select * 
from DropshipzoneSKU DS
left join NewAimSKUDimension NA on NA.SKU=DS.SKU
where NA.SKU is null


select * from 
NewAimSKUDimension NA
inner join DropshipzoneSKU DS on NA.SKU=DS.SKU





select 
CurrentOnline.SKU,
2 as Type,
coalesce(ds.Title,CurrentOnline.Name) as Name,
coalesce(ds.Description,CurrentOnline.Description) as Description,
convert(decimal(18,2),coalesce(DS.price,0)) as Cost,
convert(decimal(18,2),coalesce(case when DS.price is null then null else DS.Price*1.2 end,0)) as Price,
convert(decimal(18,2),coalesce(DS.[Weight (kg)],NA.GrossWeight,0)) as GrossWeight,
convert(decimal(18,2),coalesce(DS.[Weight (kg)],NA.GrossWeight,0)) as NetWeight,
convert(decimal(18,2),0) as CubicWeight,
convert(decimal(18,2),coalesce(DS.[Carton Length (cm)],NA.Length,0)) as Length,
convert(decimal(18,2),coalesce(DS.[Carton Width (cm)],NA.Width,0)) as Width,
convert(decimal(18,2),coalesce(DS.[Carton Height (cm)],NA.Height,0)) as Height,
convert(bit,1) as SupplierID,
convert(bit,1) as IgnoreListing,
convert(bit,1) as IsActive,
'' as Ref1,
'' as Ref2,
'' as Ref3,
'' as Ref4,
'' as Ref5,
coalesce(DS.[Image 1],CurrentOnline.Image1) as Image1,
coalesce(DS.[Image 2],CurrentOnline.Image2) as Image2,
coalesce(DS.[Image 3],CurrentOnline.Image3) as Image3,
coalesce(DS.[Image 4],CurrentOnline.Image4) as Image4,
coalesce(DS.[Image 5],CurrentOnline.Image5) as Image5,
coalesce(DS.[Image 6],CurrentOnline.Image6) as Image6,
isnull(DS.[Image 7],'') as Image7,
isnull(DS.[Image 8],'') as Image8,
isnull(DS.[Image 9],'') as Image9,
isnull(DS.[Image 10],'') as Image10,
isnull(DS.[Image 11],'') as Image11,
isnull(DS.[Image 12],'') as Image12,
GETDATE() as CreateTime,
'System' as CreateBy,
GETDATE() as EditTime,
'System' as EditBy
--*
from
(
select * from NetoProducts
where PrimarySupplier='P'
) CurrentOnline
left join DropshipzoneSKU DS on CurrentOnline.SKU=DS.SKU
left join NewAimSKUDimension NA on CurrentOnline.SKU=NA.SKU
--where DS.SKU is null
--and NA.SKU is null
where 
convert(decimal,CurrentOnline.DefaultPrice)>0 and
(DS.SKU is not null
or NA.SKU is not null)


select 
* 
from 
NewAimSKUDimension NA
--where 
inner join DropshipzoneSKU DS on DS.SKU like '%'+NA.SKU+'%'
where NA.SKU<>DS.SKU
and NA.SKU like '%-[A-G]'


select 
* 
from 
NewAimSKUDimension NA
where SKU like '%-[A-Z]'









--insert part sku

insert into D_Item
select 
--T.*
distinct
T.ChildSKU as SKU,
1 as Type,
T.ChildSKU as Name,
T.ChildSKU as Description,
0 as Cost,
0 as Price,
convert(decimal(18,8),T.GrossWeight) as GrossWeight,
convert(decimal(18,8),T.GrossWeight) as NetWeight,
0 as CubicWeight,
convert(decimal(18,8),T.Length/100) as Length,
convert(decimal(18,8),T.Width/100) as Width,
convert(decimal(18,8),T.Height/100) as Height,
1 as SupplierID,
1 as IgnoreListing,
1 as IsActive,
'' as Ref1,
'' as Ref2,
'' as Ref3,
'' as Ref4,
'' as Ref5,
GETDATE() as CreateTime,
'System' as CreateBy,
GETDATE() as EditTime,
'System' as EditBy
from
(
	select 
	R.package_sku ParentSKU,
	R.sku ChildSKU,
	R.num ChildQty,
	D.*
	from D_Item I
	inner join NewAimSKURelation R on I.SKU=R.package_sku
	inner join NewAimSKUDimension D on R.sku=D.SKU
) T
where not exists (
	select * from D_Item IT
	where  exists (
		select 
		R.*,
		I.*,
		D.*
		from D_Item I
		inner join NewAimSKURelation R on I.SKU=R.package_sku
		inner join NewAimSKUDimension D on R.sku=D.SKU
		where IT.SKU=R.sku
	--order by R.package_sku
	)--select out already exists part skus 
and IT.SKU=T.SKU
)
order by ParentSKU



--insert relationship

insert into D_Item_Relationship
select 
I.ID as ParentID,
--I.SKU as ParentSKU,
P.ID as ChildItemID,
--P.SKU as ChildItemSKU,
R.num as ChildItemQty,
0 as DisplayOrder,
GETDATE() as CreateTime,
'System' as CreateBy,
GETDATE() as EditTime,
'System' as EditBy
--* 
from
(
select * from D_Item
--where ID>3933 and Type=1
) P
inner join NewAimSKURelation R on P.SKU=R.sku
inner join D_Item I on R.package_sku=I.SKU
where not exists 
(select 1 from D_Item_Relationship IR where IR.ParentItemID=I.ID)
order by I.SKU









































--Temp SQL

select 
RIGHT('00'+CONVERT(varchar, DATEPART(d,JobItemCreateTime)),2)+RIGHT('00'+ CONVERT(varchar,DATEPART(m,JobItemCreateTime)),2)+RIGHT('00'+ Ref1,2) as OriginalRef,

JobItemCreateTime,
Ref1,
* from D_JobItem
where ID>3008


select * from d_jobItem
where id>=3054

select * from D_Image
where id>43335

-- delete from D_JobItem
--where ID>3008

--delete from D_Image
--where ID>=20536



--insert into D_Item
select 
DS.SKU,
2 as Type,
DS.Title as Name,
DS.Description as Description,
convert(decimal(18,2),isnull(DS.price,0)) as Cost,
convert(decimal(18,2),isnull(DS.Price*1.2,0)) as Price,
convert(decimal(18,8),isnull(DS.[Weight (kg)],0)) as GrossWeight,
convert(decimal(18,8),isnull(DS.[Weight (kg)],0)) as NetWeight,
convert(decimal(18,8),0) as CubicWeight,
convert(decimal(18,8),isnull(DS.[Carton Length (cm)],0)/100) as Length,
convert(decimal(18,8),isnull(DS.[Carton Width (cm)],0)/100) as Width,
convert(decimal(18,8),isnull(DS.[Carton Height (cm)],0)/100) as Height,
1 as SupplierID,
convert(bit,1) as IgnoreListing,
convert(bit,1) as IsActive,
'' as Ref1,
'' as Ref2,
'' as Ref3,
'' as Ref4,
'' as Ref5,
DS.[Image 1] as Image1,
DS.[Image 2] as Image2,
DS.[Image 3] as Image3,
DS.[Image 4] as Image4,
DS.[Image 5] as Image5,
DS.[Image 6] as Image6,
'' as Image7,
'' as Image8,
'' as Image9,
'' as Image10,
'' as Image11,
'' as Image12,
GETDATE() as CreateTime,
'System' as CreateBy,
GETDATE() as EditTime,
'System' as EditBy
--*
from
DropshipzoneSKU DS 
where [EAN Code] is not null
and not exists 
(select 1 from D_Item E where E.SKU=DS.SKU)

select * from
DropshipzoneSKU DS 
where [EAN Code] is null

select * from NewAimSKUDimension
where SKU like '%SSKB-430S-76-WHEEL'

select * from NewAimSKURelation
where package_sku='SSKB-430S-76-WHEEL-60'






select * from D_Item
where CreateTime>='20190806'

select * from D_JobItem
where ID>=3088


select * from M_JobItemImage
where ImageID=41700

select * from D_Image
where CreateTime>='20190806'
order by ID












--Sold Item 

select 
*
from
(
	select 
	case when charindex('/',Reference)=0 then Reference else  SUBSTRING(Reference,1,charindex('/',Reference)-1) end  as Reference,
	SKU
	from
	(
	select 
	replace(RTRIM(LTRIM(Reference)),char(160),'') as Reference,
	--SUBSTRING( replace(RTRIM(LTRIM(Reference)),char(160),''),1,charindex('/',replace(RTRIM(LTRIM(Reference)),char(160),''))) Reference,
	replace(RTRIM(LTRIM(SKU)),char(160),'') SKU
	from
	SoldItem20190808Trimmed
	) S1
) S






--update sold item to existing job item
select *
from
(
	select 
	distinct
	H.ID,
	case when H.Ref5='' then CONVERT(varchar, DATEPART(d,H.JobItemCreateTime))+RIGHT('00'+ CONVERT(varchar,DATEPART(m,H.JobItemCreateTime)),2)+RIGHT('00'+ H.Ref1,2) else SUBSTRING(H.Ref5,1,CHARINDEX('-',H.Ref5)-1) end as CalculateRef,
	--CONVERT(varchar, DATEPART(d,H.JobItemCreateTime))+RIGHT('00'+ CONVERT(varchar,DATEPART(m,H.JobItemCreateTime)),2)+RIGHT('00'+ H.Ref1,2) as CalculateRef,
	--L.SKU,
	case when H.DesignatedSKU<>'' then H.DesignatedSKU else L.SKU end SKU,
	H.Ref5
	--SUBSTRING(H.Ref5,1,CHARINDEX('-',H.Ref5)-1) as OriginalRef,
	--CHARINDEX('-',H.Ref5),
	--*
	from D_JobItem H
	inner join D_JobItemLine L on H.ID=L.HeaderID
	where 
	H.StatusID=1 and
	H.ID>2010 --and
	--H.Ref5=''
	--and CONVERT(varchar, DATEPART(d,H.JobItemCreateTime))+RIGHT('00'+ CONVERT(varchar,DATEPART(m,H.JobItemCreateTime)),2)+RIGHT('00'+ H.Ref1,2)+'-'+L.SKU<>H.Ref5
) E
inner join 
(
	select 
	case when charindex('/',Reference)=0 then Reference else  SUBSTRING(Reference,1,charindex('/',Reference)-1) end  as Reference,
	SKU
	from
	(
	select 
	replace(RTRIM(LTRIM(Reference)),char(160),'') as Reference,
	--SUBSTRING( replace(RTRIM(LTRIM(Reference)),char(160),''),1,charindex('/',replace(RTRIM(LTRIM(Reference)),char(160),''))) Reference,
	replace(RTRIM(LTRIM(SKU)),char(160),'') SKU
	from
	SoldItem20190808Trimmed
	) S1
) S on E.CalculateRef=S.Reference and E.SKU=S.SKU










update 
D_JobItem
set StatusID=2
where ID in
(
	select 
	H.ID
	--*
	from D_JobItem H
	inner join D_JobItemLine L on H.ID=L.HeaderID
	where 
	H.StatusID in (1) and
	L.SKU in
	(
	select 
distinct
top 10
	 case when H.DesignatedSKU<>'' then H.DesignatedSKU else L.SKU end SKU
	--L.HeaderID
	from D_JobItem H
	inner join D_JobItemLine L on H.ID=L.HeaderID
	where H.StatusID=1
	and H.ItemPrice<>0
	and H.ID>2010
	)
	or H.DesignatedSKU in
	(select 
distinct
top 10
	 case when H.DesignatedSKU<>'' then H.DesignatedSKU else L.SKU end SKU
	--L.HeaderID
	from D_JobItem H
	inner join D_JobItemLine L on H.ID=L.HeaderID
	where H.StatusID=1
	and H.ItemPrice<>0
	and H.ID>2010)
)



update D_Item
set
IgnoreListing=0
where ID in
(
	select ID from D_Item I
	inner join
	(
	select 

	distinct 
	 case when H.DesignatedSKU<>'' then H.DesignatedSKU else L.SKU end SKU
	from D_JobItem H
		inner join D_JobItemLine L on H.ID=L.HeaderID
		where H.StatusID=2
		) JSKU on I.SKU=JSKU.SKU
)








--update job item and item status for specific day

select 
distinct
case when H.DesignatedSKU<>'' then H.DesignatedSKU else L.SKU end SKU
--*
from D_JobItem H
inner join D_JobItemLine L on H.ID=L.HeaderID
where DATEDIFF(day,JobItemCreateTime,'20190819')=0
and Type=1
and StatusID<>4







select JI.ID
	from
	(
		select 
		distinct
		case when H.DesignatedSKU<>'' then H.DesignatedSKU else L.SKU end HSKU,
		H.*
		from D_JobItem H
		inner join D_JobItemLine L on H.ID=L.HeaderID
		where  Type=1
		and StatusID not in (3,4)
	) JI
	inner join 
	(
		select I.SKU HSKU
		from D_Item I
		where I.SKU in (select distinct SKU from OKSKUList20190829)
		and Price<>0 and Description<>'' and Name<>'' and Type<>1 and LEN(SKU)<=23
		--and (Price=0 or Description='' or Name='' or Type=1 or LEN(SKU)>23)
	) S on JI.HSKU=S.HSKU




	
CREATE FUNCTION [dbo].[fn_GetAffectedItems]
(
	@FromDate varchar(10),
	@ToDate varchar(10)
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
	--ship out or allocated skus
	(StatusID in (4) and DATEDIFF(DAY,@fromDate,ShipTime)>=0 and DATEDIFF(DAY,@toDate,ShipTime)<=0)
	--create new job item
	or (StatusID in (2) and DATEDIFF(DAY,@fromDate,H.CreateTime)>=0 and DATEDIFF(DAY,@toDate,H.CreateTime)<=0)
	or StatusID in (3)
) AFF on I.SKU=AFF.HSKU
inner join V_SKURelationship_Recursive O on I.BottomSKU=O.BottomSKU
inner join D_Item ITEM on ITEM.SKU=O.SKU and ITEM.IgnoreListing=0
	