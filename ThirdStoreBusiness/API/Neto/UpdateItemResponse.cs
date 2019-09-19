using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ThirdStoreCommon.Models;

namespace ThirdStoreBusiness.API.Neto
{
    public class UpdateItemResponse 
    {
        public UpdateItemResponseAck Ack { get; set; }
        public UpdateItemResponseItem[] Item { get; set; }
        public UpdateItemResponseMessages Messages { get; set; }
    }

    public partial class UpdateItemResponseItem
    {
        public string SKU { get; set; }
    }

    public partial class UpdateItemResponseMessages
    {
        public UpdateItemResponseMessagesError[] Error { get; set; }
        public UpdateItemResponseMessagesWarning[] Warning { get; set; }
    }

    public partial class UpdateItemResponseMessagesError
    {
        public string Message { get; set; }
        public string SeverityCode { get; set; }
        public string Description { get; set; }
    }

    public partial class UpdateItemResponseMessagesWarning
    {
        public string Message { get; set; }
        public string SeverityCode { get; set; }
    }

    public enum UpdateItemResponseAck
    {

        /// <remarks/>
        Error,

        /// <remarks/>
        Warning,

        /// <remarks/>
        Success,
    }
}
