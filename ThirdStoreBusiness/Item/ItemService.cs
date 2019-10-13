using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon.Models.Item;
using ThirdStoreData;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;

namespace ThirdStoreBusiness.Item
{
    public class ItemService:IItemService
    {
        private readonly IRepository<D_Item> _itemRepository;
        private readonly IRepository<D_Item_Relationship> _itemRelationshipRepository;
        private readonly IDbContext _dbContext;
        private readonly IWorkContext _workContext;

        public ItemService(IRepository<D_Item> itemRepository,
            IRepository<D_Item_Relationship> itemRelationshipRepository,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            _itemRepository = itemRepository;
            _itemRelationshipRepository = itemRelationshipRepository;
            _dbContext = dbContext;
            _workContext = workContext;
        }

        public IList<D_Item> GetAllItems()
        {
            var items = _itemRepository.Table.Where(i=>i.IsActive).ToList();
            return items;
        }

        public D_Item GetItemBySKU(string sku)
        {
            var item = _itemRepository.Table.Where(i => i.SKU.ToLower().Equals(sku.ToLower())&&i.IsActive).FirstOrDefault();
            return item;

        }


        public IPagedList<D_Item> SearchItems(string sku = null, 
            ThirdStoreItemType? itemType = null, 
            string name = null,
            ThirdStoreSupplier? supplier = null,
            int isReadyForList = -1,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _itemRepository.Table;

            if (sku != null)
                query = query.Where(i => i.SKU.Contains(sku.ToLower()));
            if (itemType.HasValue)
            {
                var itemTypeID = itemType.Value.ToValue();
                query = query.Where(i => i.Type.Equals(itemTypeID));
            }
            if (name != null)
                query = query.Where(i => i.Name.Contains(name.ToLower()));
            if (supplier.HasValue)
            {
                var supplierID = supplier.Value.ToValue();
                query = query.Where(i => i.SupplierID.Equals(supplierID));
            }
            if(isReadyForList != -1)
            {
                var blIsReadyForList = Convert.ToBoolean(isReadyForList);
                query = query.Where(l => l.IsReadyForList.Equals(blIsReadyForList));
            }

            query = query.OrderByDescending(i=>i.SKU);

            return new PagedList<D_Item>(query, pageIndex, pageSize);
        }


        public D_Item GetItemByID(int id)
        {
            var item = _itemRepository.GetById(id);
            return item;
        }


        public bool IsDuplicateSKU(string sku)
        {
            return GetItemBySKU(sku) != null ? true : false;
        }


        public void InsertItem(D_Item item)
        {
            if (item == null)
                throw new ArgumentNullException("item null");

            var currentTime = DateTime.Now;
            var currentUser = _workContext.CurrentUser.Email;
            if (item.CreateTime.Equals(DateTime.MinValue))
                item.CreateTime = currentTime;
            item.EditBy = currentUser;
            if (item.EditTime.Equals(DateTime.MinValue))
                item.EditTime = currentTime;

            _itemRepository.Insert(item);
        }

        public void UpdateItem(D_Item item)
        {
            if (item == null)
                throw new ArgumentNullException("item null");

            var currentTime = DateTime.Now;
            var currentUser = _workContext.CurrentUser.Email;
            item.EditBy = currentUser;
            item.EditTime = currentTime;

            _itemRepository.Update(item);
        }

        public void DeleteItem(D_Item item)
        {
            throw new NotImplementedException();
        }


        public D_Item_Relationship GetChildItemByID(int id)
        {
            return _itemRelationshipRepository.GetById(id);
        }


        public void DeleteChildItem(D_Item_Relationship childItem)
        {
            if (childItem == null)
                throw new ArgumentNullException("childItem null");

            _itemRelationshipRepository.Delete(childItem);
        }

        public IList<V_ItemRelationship> GetAllItemsWithRelationship()
        {
            var query = _dbContext.SqlQuery<V_ItemRelationship>("select * from V_SKURelationship_Recursive");
            return query.ToList();
        }

        public IList<D_Item> GetItemsBySKUs(IList<string> skus)
        {
            skus = skus.Select(s => s.ToLower()).ToList();
            var items = _itemRepository.Table.Where(i => skus.Contains(i.SKU.ToLower()) && i.IsActive);
            return items.ToList();
        }

        public IList<D_Item> GetItemsByIDs(IList<int> ids)
        {
            var query = _itemRepository.Table.Where(i => ids.Contains(i.ID));
            return query.ToList();
        }
    }
}
