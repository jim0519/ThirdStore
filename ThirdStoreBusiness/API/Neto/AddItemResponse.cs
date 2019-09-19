using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ThirdStoreCommon.Models;

namespace ThirdStoreBusiness.API.Neto
{
    public class AddItemResponse 
    {
        public AddItemResponseAck Ack { get; set; }
        public AddItemResponseItem[] Item { get; set; }
        public AddItemResponseMessages Messages { get; set; }
    }

    public partial class AddItemResponseItem
    {
        public string SKU { get; set; }
    }

    public partial class AddItemResponseMessages
    {
        public AddItemResponseMessagesError[] Error { get; set; }
        public AddItemResponseMessagesWarning[] Warning { get; set; }
    }

    public partial class AddItemResponseMessagesError
    {
        public string Message { get; set; }
        public string SeverityCode { get; set; }
        public string Description { get; set; }
    }

    public partial class AddItemResponseMessagesWarning
    {
        public string Message { get; set; }
        public string SeverityCode { get; set; }
    }

    public enum AddItemResponseAck
    {

        /// <remarks/>
        Error,

        /// <remarks/>
        Warning,

        /// <remarks/>
        Success,
    }
}
