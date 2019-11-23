using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreBusiness.Item;
using ThirdStoreBusiness.JobItem;
using ThirdStoreBusiness.Order;
using ThirdStoreCommon;

namespace ThirdStoreBusiness.ScheduleTask
{
    
    public class UpdateDSDataAndSync : ITask
    {
        private readonly IItemService _itemService;
        private readonly IJobItemService _jobItemService;
        private readonly CsvContext _csvContext;
        private readonly CsvFileDescription _csvFileDescription;

        public UpdateDSDataAndSync(IItemService itemService,
            IJobItemService jobItemService,
            CsvContext csvContext,
            CsvFileDescription csvFileDescription)
        {
            _itemService = itemService;
            _jobItemService = jobItemService;
            _csvContext = csvContext;
            _csvFileDescription = csvFileDescription;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            try
            {
                //update ds data from channel(dsz)
                _itemService.UpdateChannelData();

                //compare current and last time channel data and get the items which were changed and need to be sync
                List<int> syncItemIDs = new List<int>(); 
                var di = new DirectoryInfo(ThirdStoreConfig.Instance.ThirdStoreDSZData);
                if (di.Exists)
                {
                    FileInfo[] files = di.GetFiles().ToArray();
                    if (files.Count() > 0)
                    {
                        var top2DataFile = files.OrderByDescending(fi => fi.CreationTime).Take(2);
                        var syncDSPriceBelow = Convert.ToDecimal(ThirdStoreConfig.Instance.SyncDSPriceBelow);
                        if (top2DataFile.Count()==2)
                        {
                            var latestDataFile = top2DataFile.First();
                            var secondLatestDataFile = top2DataFile.Last();

                            var latestData = _csvContext.Read<DSZSKUModel>(latestDataFile.FullName, _csvFileDescription);
                            var secondLatestData = _csvContext.Read<DSZSKUModel>(secondLatestDataFile.FullName, _csvFileDescription);

                            //var changedData = from ld in latestData
                            //                  join sld in secondLatestData on ld.SKU equals sld.SKU
                            //                  where (ld.InventoryQty>0&&sld.InventoryQty==0)||(ld.InventoryQty==0 && sld.InventoryQty>0)
                            //                  select ld;

                            var leftJoinResult= from ld in latestData
                                                join sld in secondLatestData on ld.SKU equals sld.SKU into leftJoin
                                                from lj in leftJoin.DefaultIfEmpty()
                                                where lj==null || ((ld.InventoryQty > 0 && lj.InventoryQty == 0) || (ld.InventoryQty == 0 && lj.InventoryQty > 0))
                                                select ld.SKU;

                            var rightJoinResult = from sld in secondLatestData
                                                  join ld in latestData on sld.SKU equals ld.SKU into rightJoin
                                                  from rj in rightJoin.DefaultIfEmpty()
                                                  where rj == null || ((sld.InventoryQty > 0 && rj.InventoryQty == 0) || (sld.InventoryQty == 0 && rj.InventoryQty > 0))
                                                  select sld.SKU;

                            syncItemIDs = (from sku in leftJoinResult.Union(rightJoinResult).Distinct()
                                              join itm in _itemService.GetAllItems() on sku.ToLower() equals itm.SKU.ToLower()
                                           where itm.Cost<= syncDSPriceBelow
                                           select itm.ID).ToList();

                        }
                        else if(top2DataFile .Count()==1)
                        {
                            var latestDataFile = top2DataFile.First();
                            var latestData = _csvContext.Read<DSZSKUModel>(latestDataFile.FullName, _csvFileDescription);

                            syncItemIDs = (from ld in latestData
                                           join itm in _itemService.GetAllItems() on ld.SKU.ToLower() equals itm.SKU.ToLower()
                                           where ld.InventoryQty > 0 && itm.Cost <= syncDSPriceBelow
                                           select itm.ID).ToList();
                        }
                    }
                }

                //sync online inventory for the specific item ids
                _jobItemService.SyncInventory(syncItemIDs);

            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }
        }
    }
}
