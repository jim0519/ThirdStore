using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;
using ThirdStoreCommon.Models.ReturnItem;
using ThirdStoreData;

namespace ThirdStoreBusiness.ReturnItem
{
    public interface IReturnItemService
    {

        IPagedList<D_ReturnItem> SearchReturnItems(
            string sku = null,
            string trackingNumber = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);


        D_ReturnItem FindByTrackingNumber(string trackingNumber);

        D_ReturnItem GetReturnItemByID(int id);

        void InsertReturnItem(D_ReturnItem returnItem);

        void UpdateReturnItem(D_ReturnItem returnItem);

        void DeleteReturnItem(D_ReturnItem returnItem);

        D_ReturnItemLine GetReturnItemLineByID(int returnItemLineID);

        void DeleteReturnItemLine(D_ReturnItemLine returnItemLine);
    }

    public class ReturnItemService : IReturnItemService
    {
        private readonly IRepository<D_ReturnItem> _returnItemRepository;
        private readonly IRepository<D_ReturnItemLine> _returnItemLineRepository;
        private readonly IWorkContext _workContext;
        public ReturnItemService(IRepository<D_ReturnItem> returnItemRepository,
            IRepository<D_ReturnItemLine> returnItemLineRepository,
            IWorkContext workContext
            )
        {
            _returnItemRepository = returnItemRepository;
            _returnItemLineRepository = returnItemLineRepository;
            _workContext = workContext;
        }

        public D_ReturnItem FindByTrackingNumber(string trackingNumber)
        {
            try
            {
                var returnItems = _returnItemRepository.Table.Where(ri=>trackingNumber.ToUpper().Contains(ri.TrackingNumber.ToUpper()));

                if (returnItems.Count() == 1)
                    return returnItems.FirstOrDefault();
                else
                    return default(D_ReturnItem);
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public D_ReturnItem GetReturnItemByID(int id)
        {
            var item = _returnItemRepository.GetById(id);
            return item;
        }

        public IPagedList<D_ReturnItem> SearchReturnItems(
            string sku = null,
            string trackingNumber = null,
            int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            var query = _returnItemRepository.Table;

            if (!string.IsNullOrWhiteSpace(sku))
                query = query.Where(i => i.ReturnItemLines.Any(l => l.SKU.ToUpper().Contains(sku.ToUpper())) || i.DesignatedSKU.ToUpper().Contains(sku.ToUpper()));

            if (!string.IsNullOrWhiteSpace(trackingNumber))
                query = query.Where(i => i.TrackingNumber.ToUpper().Contains(trackingNumber.ToUpper()));

                query = query.OrderByDescending(i => i.CreateTime);

            return new PagedList<D_ReturnItem>(query, pageIndex, pageSize);
        }

        public void InsertReturnItem(D_ReturnItem returnItem)
        {
            if (returnItem == null)
                throw new ArgumentNullException("Job item null");
            var currentTime = DateTime.Now;
            var currentUser = Constants.SystemUser;
            if (_workContext.CurrentUser != null)
                currentUser = _workContext.CurrentUser.Email;

            returnItem.FillOutNull();
            returnItem.CreateBy = currentUser;
            if (returnItem.CreateTime.Equals(DateTime.MinValue))
                returnItem.CreateTime = currentTime;
            returnItem.EditBy = currentUser;
            if (returnItem.EditTime.Equals(DateTime.MinValue))
                returnItem.EditTime = currentTime;

            foreach (var line in returnItem.ReturnItemLines)
            {
                line.FillOutNull();
                line.CreateBy = currentUser;
                if (line.CreateTime.Equals(DateTime.MinValue))
                    line.CreateTime = currentTime;
                line.EditBy = currentUser;
                if (line.EditTime.Equals(DateTime.MinValue))
                    line.EditTime = currentTime;
            }

            //foreach (var img in returnItem.JobItemImages)
            //{
            //    img.FillOutNull();
            //    img.CreateBy = currentUser;
            //    if (img.CreateTime.Equals(DateTime.MinValue))
            //        img.CreateTime = currentTime;
            //    img.EditBy = currentUser;
            //    if (img.EditTime.Equals(DateTime.MinValue))
            //        img.EditTime = currentTime;
            //}

            _returnItemRepository.Insert(returnItem);
        }

        public void UpdateReturnItem(D_ReturnItem returnItem)
        {
            if (returnItem == null)
                throw new ArgumentNullException("Job item null");

            var currentTime = DateTime.Now;
            var currentUser = Constants.SystemUser;
            if (_workContext.CurrentUser != null)
            {
                currentUser = _workContext.CurrentUser.Email;
                returnItem.EditBy = currentUser;
                returnItem.EditTime = currentTime;
            }

            returnItem.FillOutNull();

            foreach (var line in returnItem.ReturnItemLines)
            {
                line.FillOutNull();
                if (line.ID > 0)
                {
                    line.EditBy = currentUser;
                    line.EditTime = currentTime;
                }
                else
                {
                    line.CreateBy = currentUser;
                    line.CreateTime = currentTime;
                    line.EditBy = currentUser;
                    line.EditTime = currentTime;
                }
            }

            //foreach (var img in returnItem.JobItemImages)
            //{
            //    img.FillOutNull();
            //    if (img.ID > 0)
            //    {
            //        img.EditBy = currentUser;
            //        img.EditTime = currentTime;
            //    }
            //    else
            //    {
            //        img.CreateBy = currentUser;
            //        img.CreateTime = currentTime;
            //        img.EditBy = currentUser;
            //        img.EditTime = currentTime;
            //    }
            //}

            _returnItemRepository.Update(returnItem);
        }

        public void DeleteReturnItem(D_ReturnItem returnItem)
        {
            throw new NotImplementedException();
        }

        public D_ReturnItemLine GetReturnItemLineByID(int returnItemLineID)
        {
            return _returnItemLineRepository.GetById(returnItemLineID);
        }

        public void DeleteReturnItemLine(D_ReturnItemLine returnItemLine)
        {
            if (returnItemLine == null)
                throw new ArgumentNullException("returnItemLine null");

            _returnItemLineRepository.Delete(returnItemLine);
        }
    }
}
