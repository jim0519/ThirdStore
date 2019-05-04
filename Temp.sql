select * from [V_SKURelationship]
where ParentSKU like '%Bike%Case%'

select * from [V_SKURelationship]
where ParentSKU in ('Bike-case-S','Bike-case-Sx2','S-Bike-case','Case-Clamp','Bike-case-Sx4')

select * from V_SKURelationship_3rdStore
where SKU in ('Bike-case-S','Bike-case-Sx2','S-Bike-case','Case-Clamp','Bike-case-Sx4')
order by SKU


select * from [V_SKUInventory_3rdStore]
where SKU in ('Bike-case-S','Bike-case-Sx2','S-Bike-case','Case-Clamp','Bike-case-Sx4')
order by SKU




select * from Inventory


select * from V_SKURelationship_3rdStore
where SKU in ('Bike-case-S','Bike-case-Sx2','S-Bike-case','Case-Clamp','Bike-case-Sx4')
order by SKU


select 
 
from V_SKURelationship_3rdStore SR
inner join Inventory INV on SR.SKU=INV.SKU


select * from [V_SKUInventory_DivideByJobItem_3rdStore]

select * from V_SKURelationship_3rdStore
where sku='Bike-case-XL-A'

select * from D_Item
where SKU='Bike-case-XL-A'

select * from V_SKURelationship
where ParentSKU='Bike-case-XL-A'

select 
SR.SKU,
SR.ItemType,
Min(BottomInventory.Qty/SR.Qty) as InventoryQty,
BottomInventory.JobItemID,
BottomInventory.CreateTime
from 
V_SKURelationship_3rdStore SR
inner join
(
	select 
	SRT.BottomSKU as SKU,
	Sum(SRT.Qty*INV.InventoryQty) as Qty,
	INV.JobItemID,
	INV.CreateTime
	from 
	V_SKURelationship_3rdStore SRT
	inner join Inventory INV on SRT.SKU=INV.SKU
	group by SRT.BottomSKU,INV.JobItemID,INV.CreateTime
) BottomInventory on SR.BottomSKU=BottomInventory.SKU
--where SR.SKU in ('Bike-case-S','Bike-case-Sx2','S-Bike-case','Case-Clamp','Bike-case-Sx4')
group by SR.SKU,SR.ItemType,BottomInventory.JobItemID,BottomInventory.CreateTime





select 
SR.SKU,
isnull(BottomInventory.Qty,0) as Qty
from 
V_SKURelationship_3rdStore SR
left join
(
	select 
	SRT.BottomSKU,
	Sum(SRT.Qty*INV.InventoryQty) as Qty,
	INV.JobItemID,
	INV.CreateTime
	from 
	V_SKURelationship_3rdStore SRT
	inner join Inventory INV on SRT.SKU=INV.SKU
	group by SRT.BottomSKU,INV.JobItemID,INV.CreateTime
) BottomInventory on SR.BottomSKU=BottomInventory.BottomSKU
where SR.SKU='Bike-case-XL-A'
--group by SR.SKU
--having min(isnull(BottomInventory,0))>



--IF NOT EXISTS (SELECT * FROM SysObjects O INNER JOIN SysColumns C ON O.ID=C.ID WHERE
-- ObjectProperty(O.ID,'IsUserTable')=1 AND O.Name='Inventory' AND C.Name='CreateTime')
--	ALTER TABLE dbo.Inventory ADD
--		CreateTime datetime NOT NULL CONSTRAINT DF_D_Inventory_CreateTime DEFAULT getdate()
--GO
		
--IF EXISTS (SELECT [name] FROM sysobjects WHERE [name] = 'DF_D_Inventory_CreateTime')
--	ALTER TABLE dbo.Inventory
--		DROP CONSTRAINT DF_D_Inventory_CreateTime
--GO







select 
SRL.SKU,
SRL.ItemType,
Min(isnull(INVL.BottomSKUQty,0)/SRL.Qty) as InventoryQty,
INVL.JobItemID
--INVL.CreateTime
from 
V_SKURelationship_3rdStore SRL
left join
(
	select 
	RelatedParentSKUWithJobItem.SKU,
	RelatedParentSKUWithJobItem.ItemType,
	RelatedParentSKUWithJobItem.BottomSKU,
	RelatedParentSKUWithJobItem.JobItemID,
	ISNULL(INV2.Qty,0) as BottomSKUQty
	from
	(
		select 
		SR2.*,AllRelatedParentSKU.JobItemID
		from V_SKURelationship_3rdStore SR2
		inner join 
		(
			select 
			distinct SR.SKU,INV.JobItemID
			from [V_BottomSKUInventory] INV
			inner join V_SKURelationship_3rdStore SR on INV.bottomSKU=SR.BottomSKU 
		) AllRelatedParentSKU on AllRelatedParentSKU.SKU=SR2.SKU
	) RelatedParentSKUWithJobItem
	--order by SR2.SKU
	left join [V_BottomSKUInventory] INV2 on RelatedParentSKUWithJobItem.BottomSKU=INV2.BottomSKU and RelatedParentSKUWithJobItem.JobItemID=INV2.JobItemID
) INVL on SRL.BottomSKU=INVL.BottomSKU
group by 
SRL.SKU,
SRL.ItemType,
INVL.JobItemID
having Min(isnull(INVL.BottomSKUQty,0)/SRL.Qty)>0
order by SRL.SKU


select * from V_SKUInventory_DivideByJobItem_3rdStore
where JobItemID is not null