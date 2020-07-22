using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.Order;


namespace ThirdStoreBusiness.Order
{
    public interface IOrderService
    {
        //void DownloadOrders(DateTime? fromTime, DateTime? toTime);

        void UpdateOrderDeliveryInstruction(DateTime? fromTime, DateTime? toTime);

        void UpdateOrderDeliveryInstruction(IList<D_Order_Header> orders);

        IPagedList<D_Order_Header> SearchOrders(
            DateTime? orderTimeFrom = null,
            DateTime? orderTimeTo = null,
            string channelOrderID = null,
            string jobItemID = null,
            string customerID=null,
            string consigneeName=null,
            int statusID=0,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        D_Order_Header GetOrderByChannelOrderID(string channelOrderID,int channelID);

        D_Order_Header GetOrderByID(int id);

        IList<D_Order_Header> GetOrdersByIDs(IList<int> orderids);

        D_Order_Header InsertOrder(D_Order_Header order);

        D_Order_Header UpdateOrder(D_Order_Header order);

        void DeleteOrder(D_Order_Header order);

        string GetOrderScreenshot(string orderTran);

        Stream ExportDSImportFile(IList<int> orderids);

        
    }
}
