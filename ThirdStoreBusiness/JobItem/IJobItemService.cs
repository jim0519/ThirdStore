using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon.Models.JobItem;
using ThirdStoreCommon;


namespace ThirdStoreBusiness.JobItem
{
    public interface IJobItemService
    {
        IList<D_JobItem> GetAllJobItems();

        D_JobItem GetJobItemByID(int id);

        IList<D_JobItem> GetJobItemsByIDs(IList<int> ids);

        IPagedList<D_JobItem> SearchJobItems(
            string id = null,
            string reference = null,
            string sku = null,
            DateTime? jobItemCreateTimeFrom = null,
            DateTime? jobItemCreateTimeTo = null,
            ThirdStoreJobItemType? jobItemType = null,
            ThirdStoreJobItemStatus? jobItemStatus = null,
            ThirdStoreJobItemCondition? jobItemCondition = null,
            ThirdStoreSupplier? jobItemSupplier = null,
            string location = null,
            List<string> inspector = null,
            DateTime? shipTimeFrom = null,
            DateTime? shipTimeTo = null,
            string trackingNumber = null,
            int hasStocktakeTime = -1,
            bool isExcludeShippedStatus = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        IPagedList<D_JobItem> SearchJobItems(
            int jobItemLineID = 0,
            string jobItemReference = null,
            DateTime? stocktakeTimeFrom = null,
            DateTime? stocktakeTimeTo = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        void InsertJobItem(D_JobItem jobItem);

        void UpdateJobItem(D_JobItem jobItem);

        void DeleteJobItem(D_JobItem jobItem);

        void GetNetoProducts();

        ThirdStoreReturnMessage SyncInventory(IList<int> itemids=null);

        ThirdStoreReturnMessage SyncInventory(DateTime? fromTime, DateTime? toTime);

        IList<int> GetAffectedItemIDsByJobItemIDs(IList<int> jobItemIDs);

        D_JobItemLine GetJobItemLineByID(int jobItemLineID);

        string GetJobItemReference(DateTime jobItemCreateTime,string sequenceNum);

        string GetJobItemReference(D_JobItem jobItem);

        string GetJobItemReference(int jobItemID);

        string GetJobItemLineReference(D_JobItemLine jobItemLine);

        void DeleteJobItemLine(D_JobItemLine jobItemLine);

        ThirdStoreReturnMessage ShipOut(string jobItemLineID,string jobItemLineRef, string trackingNumber);

        ThirdStoreReturnMessage ConfirmStock(IList<string> jobItemLineIDs, IList<string> jobItemLineRefs,string location);

        void PrintJobItemLabel(IList<int> ids);

        bool CanConsistInv(string jobItemIDs);

        IList<string> ConvertToJobItemReference(IList<string> invJobItemIDs);

        ThirdStoreReturnMessage SyncInventory(int[] jobItemIDs);
    }
}
