using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreBusiness.DSChannel;
using ThirdStoreBusiness.Item;
using ThirdStoreBusiness.JobItem;
using ThirdStoreBusiness.Order;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.Item;

namespace ThirdStoreBusiness.ScheduleTask
{
    
    public class UpdateDSDataAndSync : ITask
    {
        private readonly IItemService _itemService;
        private readonly IJobItemService _jobItemService;
        private readonly CsvContext _csvContext;
        private readonly CsvFileDescription _csvFileDescription;
        private readonly IEnumerable<IDSChannel> _dsChannels;

        public UpdateDSDataAndSync(IItemService itemService,
            IJobItemService jobItemService,
            IEnumerable<IDSChannel> dsChannels,
            CsvContext csvContext,
            CsvFileDescription csvFileDescription)
        {
            _itemService = itemService;
            _jobItemService = jobItemService;
            _csvContext = csvContext;
            _csvFileDescription = csvFileDescription;
            _dsChannels = dsChannels.OrderBy(c=>c.Order);
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        //public void Execute()
        //{
        //    try
        //    {
        //        //update ds data from channel(dsz)
        //        _itemService.UpdateChannelData();

        //        //compare current and last time channel data and get the items which were changed and need to be sync
        //        List<int> syncItemIDs = new List<int>(); 
        //        var di = new DirectoryInfo(ThirdStoreConfig.Instance.ThirdStoreDSZData);
        //        if (di.Exists)
        //        {
        //            FileInfo[] files = di.GetFiles().ToArray();
        //            if (files.Count() > 0)
        //            {
        //                var top2DataFile = files.OrderByDescending(fi => fi.CreationTime).Take(2);
        //                var syncDSPriceBelow = Convert.ToDecimal(ThirdStoreConfig.Instance.SyncDSPriceBelow);
        //                var dsInventoryThredshold = Convert.ToInt32(ThirdStoreConfig.Instance.DSInventoryThreshold);
        //                if (top2DataFile.Count()==2)
        //                {
        //                    var latestDataFile = top2DataFile.First();
        //                    var secondLatestDataFile = top2DataFile.Last();

        //                    var latestData = _csvContext.Read<DSZSKUModel>(latestDataFile.FullName, _csvFileDescription);
        //                    var secondLatestData = _csvContext.Read<DSZSKUModel>(secondLatestDataFile.FullName, _csvFileDescription);

        //                    //var changedData = from ld in latestData
        //                    //                  join sld in secondLatestData on ld.SKU equals sld.SKU
        //                    //                  where (ld.InventoryQty>0&&sld.InventoryQty==0)||(ld.InventoryQty==0 && sld.InventoryQty>0)
        //                    //                  select ld;

        //                    var leftJoinResult= from ld in latestData
        //                                        join sld in secondLatestData on ld.SKU equals sld.SKU into leftJoin
        //                                        from lj in leftJoin.DefaultIfEmpty()
        //                                        where lj==null 
        //                                        || 
        //                                        ((((ld.InventoryQty >= dsInventoryThredshold && lj.InventoryQty< dsInventoryThredshold) || (ld.InventoryQty < dsInventoryThredshold && lj.InventoryQty >= dsInventoryThredshold))
        //                                        ||(ld.Price!=lj.Price))&& (ld.Price <= syncDSPriceBelow || lj.Price <= syncDSPriceBelow))
        //                                        select ld.SKU;

        //                    var rightJoinResult = from sld in secondLatestData
        //                                          join ld in latestData on sld.SKU equals ld.SKU into rightJoin
        //                                          from rj in rightJoin.DefaultIfEmpty()
        //                                          where rj == null 
        //                                          || 
        //                                          ((((sld.InventoryQty >= dsInventoryThredshold && rj.InventoryQty < dsInventoryThredshold) || (sld.InventoryQty < dsInventoryThredshold && rj.InventoryQty >= dsInventoryThredshold))
        //                                          ||(sld.Price!=rj.Price)) && (sld.Price <= syncDSPriceBelow || rj.Price <= syncDSPriceBelow))
        //                                          select sld.SKU;

        //                    syncItemIDs = (from sku in leftJoinResult.Union(rightJoinResult).Distinct()
        //                                      join itm in _itemService.GetAllItems() on sku.ToLower() equals itm.SKU.ToLower()
        //                                   select itm.ID).ToList();

        //                }
        //                else if(top2DataFile .Count()==1)
        //                {
        //                    var latestDataFile = top2DataFile.First();
        //                    var latestData = _csvContext.Read<DSZSKUModel>(latestDataFile.FullName, _csvFileDescription);

        //                    syncItemIDs = (from ld in latestData
        //                                   join itm in _itemService.GetAllItems() on ld.SKU.ToLower() equals itm.SKU.ToLower()
        //                                   where itm.Cost <= syncDSPriceBelow
        //                                   select itm.ID).ToList();
        //                }
        //            }
        //        }

        //        //sync online inventory for the specific item ids
        //        if (syncItemIDs.Count > 0)
        //            LogManager.Instance.Info("Sync affected item ids for update DSZ data: " + syncItemIDs.Select(id => id.ToString()).Aggregate((current, next) => current + "," + next));
        //        _jobItemService.SyncInventory(syncItemIDs);

        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Instance.Error(ex.Message);
        //    }
        //}

        public void Execute()
        {
            try
            {
                var allDSItems = new List<D_Item>();
                foreach(var dsChannel in _dsChannels)
                {
                    allDSItems.AddRange(dsChannel.GetDSData());
                }

                _itemService.AddOrUpdateItem(allDSItems);
                LogManager.Instance.Info("Update or add item succeed.");

                var allSyncSKUs = new List<string>();
                foreach(var dsChannel in _dsChannels)
                {
                    allSyncSKUs.AddRange(dsChannel.GetNeedSyncSKUs());
                }

                var allSyncItemIDs= (from sku in allSyncSKUs
                                     join itm in _itemService.GetAllItems() on sku.ToLower() equals itm.SKU.ToLower()
                                     select itm.ID).ToList();

                if (allSyncItemIDs.Count > 0)
                    LogManager.Instance.Info("Sync affected item ids for update DS data: " + allSyncItemIDs.Select(id => id.ToString()).Aggregate((current, next) => current + "," + next));
                _jobItemService.SyncInventory(allSyncItemIDs);

            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }
        }
    }
}
