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
using ThirdStoreBusiness.Item;
using ThirdStoreCommon.Models.Misc;
using ThirdStore.Models.Misc;
using ThirdStoreCommon.Models.ReturnItem;
using ThirdStore.Models.ReturnItem;

namespace ThirdStore.Infrastructure
{

    public class AutoMapperStartupTask : IStartupTask
    {
        public void Execute()
        {
            //Access Control
            Mapper.CreateMap<T_User, UserGridViewModel>();

            Mapper.CreateMap<T_User, UserViewModel>()
                .ForMember((dest => dest.Roles), mce => mce.MapFrom(s => s.UserRoles.Select(ur => ur.RoleID).ToList())); ;
            //Mapper.CreateMap<UserViewModel, T_User>().ForAllMembers(opts => opts.Condition(vm =>!vm.IsSourceValueNull));
            Mapper.CreateMap<UserViewModel, T_User>().ForMember(dest=>dest.Password,mce=>mce.Condition(src=>!src.IsSourceValueNull));

            Mapper.CreateMap<T_Role, RoleGridViewModel>();

            Mapper.CreateMap<T_Role, RoleViewModel>()
                .ForMember(dest => dest.IsActive, mce => mce.MapFrom(s => Convert.ToInt32(s.IsActive)))
                .ForMember((dest => dest.Permissions), mce => mce.MapFrom(s => s.RolePermissions.Select(ur => ur.PermissionID).ToList())); 
            Mapper.CreateMap<RoleViewModel, T_Role>()
                .ForMember(dest => dest.IsActive, mce => mce.MapFrom(s => Convert.ToBoolean(s.IsActive)));


            //Item
            Mapper.CreateMap<D_Item, D_Item>();
            Mapper.CreateMap<D_Item, ItemGridViewModel>()
                .ForMember(dest => dest.Type, mce => mce.MapFrom(s => s.Type.ToEnumName<ThirdStoreItemType>()))
                .ForMember(dest => dest.Supplier, mce => mce.MapFrom(s => s.SupplierID.ToEnumName<ThirdStoreSupplier>()));
            Mapper.CreateMap<D_Item, ItemViewModel>()
                .ForMember(dest => dest.IsActive, mce => mce.MapFrom(s => Convert.ToInt32(s.IsActive)))
                .ForMember(dest => dest.IsReadyForList, mce => mce.MapFrom(s => Convert.ToInt32(s.IsReadyForList)))
            .ForMember(dest => dest.DisableDropship, mce => mce.MapFrom(s => Convert.ToInt32(s.DisableDropship)));
            Mapper.CreateMap<D_Item_Relationship, ItemViewModel.ChildItemLineViewModel>()
                .ForMember(dest => dest.ChildItemSKU, mce => mce.MapFrom<string>(r => r.ChildItem.SKU));
            Mapper.CreateMap<M_ItemImage, ItemViewModel.ItemImageViewModel>()
                .ForMember(dest => dest.StatusID, mce => mce.MapFrom(s => Convert.ToBoolean(s.StatusID)));

            Mapper.CreateMap<ItemViewModel, D_Item>()
                .ForMember(dest => dest.IsActive, mce => mce.MapFrom(s => Convert.ToBoolean(s.IsActive)))
                .ForMember(dest => dest.IsReadyForList, mce => mce.MapFrom(s => Convert.ToBoolean(s.IsReadyForList)))
                .ForMember(dest => dest.DisableDropship, mce => mce.MapFrom(s => Convert.ToBoolean(s.DisableDropship)));
            Mapper.CreateMap<ItemViewModel.ChildItemLineViewModel, D_Item_Relationship>();
            Mapper.CreateMap<ItemViewModel.ItemImageViewModel, M_ItemImage>()
                .ForMember(dest => dest.StatusID, mce => mce.MapFrom(s => Convert.ToInt32(s.StatusID)));

            //Job Item
            Mapper.CreateMap<D_JobItem, JobItemGridViewModel>()
                .ForMember(dest => dest.Type, mce => mce.MapFrom(s => s.Type.ToEnumName<ThirdStoreJobItemType>()))
                .ForMember(dest => dest.Status, mce => mce.MapFrom(s => s.StatusID.ToEnumName<ThirdStoreJobItemStatus>()))
                .ForMember(dest => dest.Condition, mce => mce.MapFrom(s => s.ConditionID.ToEnumName<ThirdStoreJobItemCondition>()));
                //.ForMember(dest => dest.Supplier, mce => mce.MapFrom(s => s.SupplierID.ToEnumName<ThirdStoreSupplier>()))
            Mapper.CreateMap<D_JobItem, JobItemViewModel>()
                .ForMember((dest => dest.Ref2), mce => mce.MapFrom(s => s.Ref2.ToCharArray().Select(c => c.ToString()).ToList()))
                .ForMember(dest => dest.ReviewComments, mce => mce.MapFrom(s => s.Ref5));
            Mapper.CreateMap<D_JobItemLine, JobItemViewModel.JobItemLineViewModel>();
            Mapper.CreateMap<M_JobItemImage, JobItemViewModel.JobItemImageViewModel>()
                .ForMember(dest=>dest.StatusID,mce=>mce.MapFrom(s=>Convert.ToBoolean( s.StatusID)));

            Mapper.CreateMap<JobItemViewModel, D_JobItem>()
                .ForMember(dest => dest.Ref2, mce => mce.MapFrom(s => (s.Ref2 != null && s.Ref2.Count > 0 ? string.Join("", s.Ref2) : string.Empty)))
            .ForMember(dest => dest.Ref5, mce => mce.MapFrom(s =>s.ReviewComments));
            Mapper.CreateMap<JobItemViewModel.JobItemLineViewModel, D_JobItemLine>();
            Mapper.CreateMap<JobItemViewModel.JobItemImageViewModel, M_JobItemImage>()
                .ForMember(dest => dest.StatusID, mce => mce.MapFrom(s => Convert.ToInt32(s.StatusID)));

            //Order
            Mapper.CreateMap<D_Order_Header, OrderGridViewModel>();
            //.ForMember(dest => dest.StatusText, mce => mce.MapFrom(s => s.StatusID.ToEnumName<ThirdStoreOrderStatus>()));

            //Log
            Mapper.CreateMap<T_Log, LogGridViewModel>();

            //Return Item
            Mapper.CreateMap<D_ReturnItem, ReturnItemGridViewModel>()
               .ForMember(dest => dest.Status, mce => mce.MapFrom(s => s.StatusID.ToEnumName<ThirdStoreReturnItemStatus>()));
            Mapper.CreateMap<D_ReturnItem, ReturnItemViewModel>();
            Mapper.CreateMap<D_ReturnItemLine, ReturnItemViewModel.ReturnItemLineViewModel>();


            Mapper.CreateMap<ReturnItemViewModel, D_ReturnItem>();
            Mapper.CreateMap<ReturnItemViewModel.ReturnItemLineViewModel, D_ReturnItemLine>();

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
