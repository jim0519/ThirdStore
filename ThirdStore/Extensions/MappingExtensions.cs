using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreCommon.Models.AccessControl;
using ThirdStore.Models.AccessControl;
using ThirdStore.Models.Item;
using ThirdStoreCommon.Models.Item;
using ThirdStore.Models.JobItem;
using ThirdStoreCommon.Models.JobItem;
using ThirdStoreCommon.Models.Image;
using ThirdStoreCommon.Models.Order;
using ThirdStore.Models.Order;
using ThirdStore.Models.Misc;
using ThirdStoreCommon.Models.Misc;
using ThirdStore.Models.ReturnItem;
using ThirdStoreCommon.Models.ReturnItem;
using ThirdStoreCommon.Models.Attachment;

namespace ThirdStore.Extensions
{
    public static class MappingExtensions
    {
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return Mapper.Map(source, destination);
        }

        #region AccessControl
        public static UserGridViewModel ToModel(this T_User entity)
        {
            return entity.MapTo<T_User, UserGridViewModel>();
        }

        public static UserViewModel ToCreateNewModel(this T_User entity)
        {
            return entity.MapTo<T_User, UserViewModel>();
        }

        public static T_User ToCreateNewEntity(this UserViewModel model)
        {
            return model.MapTo<UserViewModel, T_User>();
        }

        public static T_User ToCreateNewEntity(this UserViewModel model, T_User destination)
        {
            return model.MapTo<UserViewModel, T_User>(destination);
        }


        public static RoleGridViewModel ToModel(this T_Role entity)
        {
            return entity.MapTo<T_Role, RoleGridViewModel>();
        }

        public static RoleViewModel ToCreateNewModel(this T_Role entity)
        {
            return entity.MapTo<T_Role, RoleViewModel>();
        }

        public static T_Role ToCreateNewEntity(this RoleViewModel model)
        {
            return model.MapTo<RoleViewModel, T_Role>();
        }

        public static T_Role ToCreateNewEntity(this RoleViewModel model, T_Role destination)
        {
            return model.MapTo<RoleViewModel, T_Role>(destination);
        }
        #endregion

        #region Item

        //Item
        public static ItemGridViewModel ToModel(this D_Item entity)
        {
            return entity.MapTo<D_Item, ItemGridViewModel>();
        }

        public static ItemViewModel ToCreateNewModel(this D_Item entity)
        {
            return entity.MapTo<D_Item, ItemViewModel>();
        }

        public static ItemViewModel.ChildItemLineViewModel ToModel(this D_Item_Relationship entity)
        {
            return entity.MapTo<D_Item_Relationship, ItemViewModel.ChildItemLineViewModel>();
        }

        public static D_Item ToCreateNewEntity(this ItemViewModel model)
        {
            return model.MapTo<ItemViewModel, D_Item>();
        }

        public static D_Item ToCreateNewEntity(this ItemViewModel model, D_Item destination)
        {
            return model.MapTo(destination);
        }

        public static D_Item_Relationship ToEntity(this ItemViewModel.ChildItemLineViewModel model)
        {
            return model.MapTo<ItemViewModel.ChildItemLineViewModel, D_Item_Relationship>();
        }

        public static D_Item_Relationship ToEntity(this ItemViewModel.ChildItemLineViewModel model, D_Item_Relationship destination)
        {
            return model.MapTo(destination);
        }

        public static ItemViewModel.ItemImageViewModel ToModel(this M_ItemImage entity)
        {
            return entity.MapTo<M_ItemImage, ItemViewModel.ItemImageViewModel>();
        }

        public static M_ItemImage ToEntity(this ItemViewModel.ItemImageViewModel model)
        {
            return model.MapTo<ItemViewModel.ItemImageViewModel, M_ItemImage>();
        }

        public static M_ItemImage ToEntity(this ItemViewModel.ItemImageViewModel model, M_ItemImage destination)
        {
            return model.MapTo(destination);
        }

        public static ItemViewModel.ItemAttachmentViewModel ToModel(this M_ItemAttachment entity)
        {
            return entity.MapTo<M_ItemAttachment, ItemViewModel.ItemAttachmentViewModel>();
        }

        public static M_ItemAttachment ToEntity(this ItemViewModel.ItemAttachmentViewModel model)
        {
            return model.MapTo<ItemViewModel.ItemAttachmentViewModel, M_ItemAttachment>();
        }

        public static M_ItemAttachment ToEntity(this ItemViewModel.ItemAttachmentViewModel model, M_ItemAttachment destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Job Item

        public static JobItemGridViewModel ToModel(this D_JobItem entity)
        {
            return entity.MapTo<D_JobItem, JobItemGridViewModel>();
        }

        public static JobItemViewModel ToCreateNewModel(this D_JobItem entity)
        {
            return entity.MapTo<D_JobItem, JobItemViewModel>();
        }

        public static JobItemViewModel.JobItemLineViewModel ToModel(this D_JobItemLine entity)
        {
            return entity.MapTo<D_JobItemLine, JobItemViewModel.JobItemLineViewModel>();
        }

        public static D_JobItem ToCreateNewEntity(this JobItemViewModel model)
        {
            return model.MapTo<JobItemViewModel, D_JobItem>();
        }

        public static D_JobItem ToCreateNewEntity(this JobItemViewModel model, D_JobItem destination)
        {
            return model.MapTo(destination);
        }

        public static D_JobItemLine ToEntity(this JobItemViewModel.JobItemLineViewModel model)
        {
            return model.MapTo<JobItemViewModel.JobItemLineViewModel, D_JobItemLine>();
        }

        public static D_JobItemLine ToEntity(this JobItemViewModel.JobItemLineViewModel model, D_JobItemLine destination)
        {
            return model.MapTo(destination);
        }

        public static JobItemViewModel.JobItemImageViewModel ToModel(this M_JobItemImage entity)
        {
            return entity.MapTo<M_JobItemImage, JobItemViewModel.JobItemImageViewModel>();
        }

        public static M_JobItemImage ToEntity(this JobItemViewModel.JobItemImageViewModel model)
        {
            return model.MapTo<JobItemViewModel.JobItemImageViewModel, M_JobItemImage>();
        }

        public static M_JobItemImage ToEntity(this JobItemViewModel.JobItemImageViewModel model, M_JobItemImage destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Order

        public static OrderGridViewModel ToModel(this D_Order_Header entity)
        {
            return entity.MapTo<D_Order_Header, OrderGridViewModel>();
        }

        #endregion

        #region Log

        public static LogGridViewModel ToModel(this T_Log entity)
        {
            return entity.MapTo<T_Log, LogGridViewModel>();
        }

        #endregion

        #region Return Item

        public static ReturnItemGridViewModel ToModel(this D_ReturnItem entity)
        {
            return entity.MapTo<D_ReturnItem, ReturnItemGridViewModel>();
        }

        public static ReturnItemViewModel ToCreateNewModel(this D_ReturnItem entity)
        {
            return entity.MapTo<D_ReturnItem, ReturnItemViewModel>();
        }

        public static ReturnItemViewModel.ReturnItemLineViewModel ToModel(this D_ReturnItemLine entity)
        {
            return entity.MapTo<D_ReturnItemLine, ReturnItemViewModel.ReturnItemLineViewModel>();
        }

        public static D_ReturnItem ToCreateNewEntity(this ReturnItemViewModel model)
        {
            return model.MapTo<ReturnItemViewModel, D_ReturnItem>();
        }

        public static D_ReturnItem ToCreateNewEntity(this ReturnItemViewModel model, D_ReturnItem destination)
        {
            return model.MapTo(destination);
        }

        public static D_ReturnItemLine ToEntity(this ReturnItemViewModel.ReturnItemLineViewModel model)
        {
            return model.MapTo<ReturnItemViewModel.ReturnItemLineViewModel, D_ReturnItemLine>();
        }

        public static D_ReturnItemLine ToEntity(this ReturnItemViewModel.ReturnItemLineViewModel model, D_ReturnItemLine destination)
        {
            return model.MapTo(destination);
        }
        

        #endregion
    }
}