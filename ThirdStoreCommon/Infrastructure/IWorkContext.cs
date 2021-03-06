﻿using ThirdStoreCommon.Models.AccessControl;
namespace ThirdStoreCommon.Infrastructure
{
    /// <summary>
    /// Work context
    /// </summary>
    public interface IWorkContext
    {
        /// <summary>
        /// Gets or sets the current customer
        /// </summary>
        T_User CurrentUser { get; set; }

        string CurrentUserName { get; }
        
    }
}
