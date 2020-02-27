using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon.Models.Item;
using ThirdStoreCommon;


namespace ThirdStoreBusiness.Item
{
    public interface IItemService
    {
        IList<D_Item> GetAllItems();

        D_Item GetItemByID(int id);

        D_Item GetItemBySKU(string sku);

        IList<D_Item> GetItemsBySKUs(IList<string> skus);

        IList<D_Item> GetItemsByIDs(IList<int> ids);

        bool IsDuplicateSKU(string sku);

        IPagedList<D_Item> SearchItems(string sku = null,
             ThirdStoreItemType? itemType = null,
            string name = null,
            string aliasSKU = null,
            string refSKU=null,
            ThirdStoreSupplier? supplier = null,
            int isReadyForList = -1,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        D_Item_Relationship GetChildItemByID(int id);

        IList<V_ItemRelationship> GetAllItemsWithRelationship();

        void InsertItem(D_Item item);

        void UpdateItem(D_Item item);

        void DeleteItem(D_Item item);

        void DeleteChildItem(D_Item_Relationship childItem);

        void UpdateChannelData();

        void FetchNetoProducts();
    }
}
