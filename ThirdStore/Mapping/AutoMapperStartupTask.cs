using AutoMapper;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;
using System;
using System.Linq.Expressions;
using ThirdStoreCommon.Models;
using ThirdStoreCommon.Models.AccessControl;
using ThirdStore.Models.AccessControl;
using System.Collections.Generic;
using System.Linq;
using ThirdStore.Models.Item;
using ThirdStoreCommon.Models.Item;
using ThirdStore.Models.JobItem;
using ThirdStoreCommon.Models.JobItem;
using ThirdStoreCommon.Models.Image;
using ThirdStoreCommon.Models.Order;
using ThirdStore.Models.Order;

namespace ThirdStore.Infrastructure
{

    public class AutoMapperStartupTask : IStartupTask
    {
        public void Execute()
        {
            //Access Control
            Mapper.CreateMap<T_User, CreateUserViewModel>();
            Mapper.CreateMap<CreateUserViewModel, T_User>();

            //Item
            Mapper.CreateMap<D_Item, ItemGridViewModel>()
                .ForMember(dest => dest.Type, mce => mce.MapFrom(s => s.Type.ToEnumName<ThirdStoreItemType>()))
                .ForMember(dest => dest.Supplier, mce => mce.MapFrom(s => s.SupplierID.ToEnumName<ThirdStoreSupplier>()));
            Mapper.CreateMap<D_Item, ItemViewModel>()
                .ForMember(dest => dest.IsActive, mce => mce.MapFrom(s => Convert.ToInt32(s.IsActive)))
                .ForMember(dest => dest.IsReadyForList, mce => mce.MapFrom(s => Convert.ToInt32(s.IsReadyForList)));
            Mapper.CreateMap<D_Item_Relationship, ItemViewModel.ChildItemLineViewModel>()
                .ForMember(dest => dest.ChildItemSKU, mce => mce.MapFrom<string>(r => r.ChildItem.SKU));
            Mapper.CreateMap<M_ItemImage, ItemViewModel.ItemImageViewModel>();

            Mapper.CreateMap<ItemViewModel, D_Item>()
                .ForMember(dest => dest.IsActive, mce => mce.MapFrom(s => Convert.ToBoolean(s.IsActive)))
                .ForMember(dest => dest.IsReadyForList, mce => mce.MapFrom(s => Convert.ToBoolean(s.IsReadyForList)));
            Mapper.CreateMap<ItemViewModel.ChildItemLineViewModel, D_Item_Relationship>();
            Mapper.CreateMap<ItemViewModel.ItemImageViewModel, M_ItemImage>();

            //Job Item
            Mapper.CreateMap<D_JobItem, JobItemGridViewModel>()
                .ForMember(dest => dest.Type, mce => mce.MapFrom(s => s.Type.ToEnumName<ThirdStoreJobItemType>()))
                .ForMember(dest => dest.Status, mce => mce.MapFrom(s => s.StatusID.ToEnumName<ThirdStoreJobItemStatus>()))
                .ForMember(dest => dest.Condition, mce => mce.MapFrom(s => s.ConditionID.ToEnumName<ThirdStoreJobItemCondition>()));
                //.ForMember(dest => dest.Supplier, mce => mce.MapFrom(s => s.SupplierID.ToEnumName<ThirdStoreSupplier>()))
            Mapper.CreateMap<D_JobItem, JobItemViewModel>();
            Mapper.CreateMap<D_JobItemLine, JobItemViewModel.JobItemLineViewModel>();
            Mapper.CreateMap<M_JobItemImage, JobItemViewModel.JobItemImageViewModel>()
                .ForMember(dest=>dest.StatusID,mce=>mce.MapFrom(s=>Convert.ToBoolean( s.StatusID)));

            Mapper.CreateMap<JobItemViewModel,D_JobItem>();
            Mapper.CreateMap<JobItemViewModel.JobItemLineViewModel, D_JobItemLine>();
            Mapper.CreateMap<JobItemViewModel.JobItemImageViewModel, M_JobItemImage>()
                .ForMember(dest => dest.StatusID, mce => mce.MapFrom(s => Convert.ToInt32(s.StatusID)));

            //Order
            Mapper.CreateMap<D_Order_Header, OrderGridViewModel>();


        }

        public int Order
        {
            get
            {
                return 0;
            }
        }
    }
}
