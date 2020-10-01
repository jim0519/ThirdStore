
declare @FromDate datetime='1970-01-01 00:00:00'
declare @ToDate datetime='2050-12-31 00:00:00'

--select 
--CONVERT(date,H.CreateTime) as CreateDate,
--count(distinct ST.letter) as StaffNum
--from D_JobItem H
----inner join D_JobItemLine L on H.ID=L.HeaderID
--cross apply fn_SplitLetters(H.Ref2) ST
--where DATEDIFF(day,@FromDate, H.CreateTime)>=0  and DATEDIFF(day, H.CreateTime,@ToDate)>=0
--group by CONVERT(date,H.CreateTime)


;WITH  JobItemH ( 
						ID,
						JobItemCreateTime,
						Type,
						StatusID,
						ConditionID,
						ItemName,
						ItemDetail,
						ItemPrice,
						DesignatedSKU,
						ShipTime,
						TrackingNumber,
						Ref1,
						Ref2,
						Ref3,
						Ref4,
						Ref5,
						CreateTime,
						CreateBy,
						EditTime,
						EditBy,
						Location,
						Note,
						PricePercentage,
						StocktakeTime

						)
						AS
						(
						select
						ID,
						JobItemCreateTime,
						Type,
						StatusID,
						ConditionID,
						ItemName,
						ItemDetail,
						ItemPrice,
						DesignatedSKU,
						ShipTime,
						TrackingNumber,
						Ref1,
						Ref2,
						Ref3,
						Ref4,
						Ref5,
						CreateTime,
						CreateBy,
						EditTime,
						EditBy,
						Location,
						Note,
						PricePercentage,
						StocktakeTime
						from 
						D_JobItem 
						where DATEDIFF(day,@FromDate, CreateTime)>=0  and DATEDIFF(day, CreateTime,@ToDate)>=0
						)

,JobItemL ( 
						ID,
						JobItemCreateTime,
						Type,
						StatusID,
						ConditionID,
						ItemName,
						ItemDetail,
						ItemPrice,
						DesignatedSKU,
						ShipTime,
						TrackingNumber,
						Ref1,
						Ref2,
						Ref3,
						Ref4,
						Ref5,
						CreateTime,
						CreateBy,
						EditTime,
						EditBy,
						Location,
						Note,
						PricePercentage,
						StocktakeTime,
						ItemID,
						Qty,
						SupplierRef,
						IsOrginalPackage,
						Weight,
						Length,
						Width,
						Height,
						CubicWeight,
						LRef1,
						LRef2,
						LRef3,
						LRef4,
						LRef5

						)
						AS
						(
						select
						H.ID,
						H.JobItemCreateTime,
						H.Type,
						H.StatusID,
						H.ConditionID,
						H.ItemName,
						H.ItemDetail,
						H.ItemPrice,
						H.DesignatedSKU,
						H.ShipTime,
						H.TrackingNumber,
						H.Ref1,
						H.Ref2,
						H.Ref3,
						H.Ref4,
						H.Ref5,
						H.CreateTime,
						H.CreateBy,
						H.EditTime,
						H.EditBy,
						Location,
						Note,
						PricePercentage,
						StocktakeTime,
						L.ItemID,
						L.Qty,
						L.SupplierRef,
						L.IsOrginalPackage,
						L.Weight,
						L.Length,
						L.Width,
						L.Height,
						L.CubicWeight,
						L.Ref1,
						L.Ref2,
						L.Ref3,
						L.Ref4,
						L.Ref5
						from 
						D_JobItem H
						inner join D_JobItemLine L on H.ID=L.HeaderID
						where DATEDIFF(day,@FromDate, H.CreateTime)>=0  and DATEDIFF(day, H.CreateTime,@ToDate)>=0
						)

select 
HT.CreateDate,
SF.StaffNum,
HT.TotalValue,
HT.TotalValue/SF.StaffNum as AVGValue,
HT.TotalQty,
HT.TotalQtyD,
HT.TotalQtyLocFRS,
HT.TotalQtyLocT,
HT.TotalQtyLocOther,
LT.QMW,
LT.QMWD,
LT.QMW/SF.StaffNum as AVGQMW
from
(

	select 
	CONVERT(date,H.CreateTime) as CreateDate,
	count(distinct ST.letter) as StaffNum
	--SUM(ItemPrice) as TotalValue
	from JobItemH H
	--inner join D_JobItemLine L on H.ID=L.HeaderID
	cross apply fn_SplitLetters(H.Ref2) ST
	group by CONVERT(date,H.CreateTime)
) SF
inner join
(
	select
	CONVERT(date,H.CreateTime) as CreateDate,
	SUM(ItemPrice) as TotalValue,
	count(1) as TotalQty,
	SUM(case when H.Type=1 then 0 else 1 end) as TotalQtyD,
	SUM(case when H.Location in ('F','R') then 1 else 0 end) TotalQtyLocFRS,
	SUM(case when H.Location in ('T') then 1 else 0 end) TotalQtyLocT,
	SUM(case when H.Location not in ('F','R','T') then 1 else 0 end) TotalQtyLocOther
	from JobItemH H
	group by CONVERT(date,H.CreateTime)
) HT on SF.CreateDate=HT.CreateDate
inner join
(
	select
	CONVERT(date,L.CreateTime) as CreateDate,
	SUM(cast(L.LRef1 as decimal(18,8))) QMW,
	SUM(case when Type=1 then 0 else cast(L.LRef1 as decimal(18,8)) end) QMWD
	--SUM(L.CubicWeight) QMW,
	--SUM(case when Type=1 then 0 else L.CubicWeight end ) QMWD
	--SUM(case when Type=1 then 0 else cast(L.Ref1 as decimal(18,8)) end) QMWD
	from JobItemL L
	group by CONVERT(date,L.CreateTime)
) LT on HT.CreateDate=LT.CreateDate

select * from JobItemL L
inner join JobItemH H on L.ID=H.ID 


select 
*
from D_JobItem H
--inner join D_JobItemLine L on H.ID=L.HeaderID
--cross apply fn_SplitLetters(H.Ref2) ST
where DATEDIFF(day,@FromDate, H.CreateTime)>=0  and DATEDIFF(day, H.CreateTime,@ToDate)>=0
