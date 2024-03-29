using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ThirdStoreCommon;
using ThirdStoreCommon.Models;

namespace ThirdStoreBusiness.API.Neto
{
    public class UpdateOrderResponse 
    {
        public UpdateOrderResponseAck Ack { get; set; }
        [JsonConverter(typeof(SingleValueArrayConverter<UpdateOrderResponseOrder>))]
        public List<UpdateOrderResponseOrder> Order { get; set; }
        public UpdateOrderResponseMessages Messages { get; set; }
    }

    public partial class UpdateOrderResponseOrder
    {
        [JsonConverter(typeof(SingleValueArrayConverter<string>))]
        public List<string> OrderID { get; set; }
    }

    public partial class UpdateOrderResponseMessages
    {
        public UpdateOrderResponseMessagesError[] Error { get; set; }
        public UpdateOrderResponseMessagesWarning[] Warning { get; set; }
    }

    public partial class UpdateOrderResponseMessagesError
    {
        public string Message { get; set; }
        public string SeverityCode { get; set; }
        public string Description { get; set; }
    }

    public partial class UpdateOrderResponseMessagesWarning
    {
        public string Message { get; set; }
        public string SeverityCode { get; set; }
    }

    public enum UpdateOrderResponseAck
    {

        /// <remarks/>
        Error,

        /// <remarks/>
        Warning,

        /// <remarks/>
        Success,
    }
}
