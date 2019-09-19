using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThirdStoreCommon;
using RestSharp;
using Newtonsoft.Json;

namespace ThirdStoreBusiness.API.Neto
{
    public class NetoApiCredential
    {
        public string APIKey { get; set; }
        public string UserName { get; set; }
        public string URL { get; set; }
    }

    public class NetoOrder
    {

    }

    public interface INetoCredentialProvider
    {
        IList<NetoApiCredential> GetAPICredential();
    }

    public interface INetoAPICallManager
    {
        //Download New Catch Orders by period
        IEnumerable<GetOrderResponseOrder> DownloadNewNetoOrder(DateTime? fromTime, DateTime? toTime);

        IEnumerable<GetItemResponseItem> GetNetoProducts(bool isWithDesc=false);

        IEnumerable<GetItemResponseItem> GetNetoProductBySKUs(string[] skus, bool isWithDesc = false);

        ThirdStoreReturnMessage UpdateProducts(UpdateItem updateItem);

        ThirdStoreReturnMessage AddProducts(AddItem addItem);

        ThirdStoreReturnMessage UpdateOrderDeliveryInstruction(UpdateOrder updateOrders);

    }


    public class NetoAPICredentialProvider : INetoCredentialProvider
    {
        public IList<NetoApiCredential> GetAPICredential()
        {
            var lstApiCredential = new List<NetoApiCredential>();
            lstApiCredential.Add(new NetoApiCredential() { URL= "https://www.3rdstore.com.au/",  APIKey= "qP6664t8KVxpcG9lhNZwO94ULkoGPCr2",UserName= "tech" });
            return lstApiCredential;
        }
    }

    public class NetoAPICallManager : INetoAPICallManager
    {

        private readonly NetoApiCredential _netoAPICredential;
        private readonly RestClient _restClient;
        public NetoAPICallManager(INetoCredentialProvider netoAPICredentialProvider)
        {
            _netoAPICredential = netoAPICredentialProvider.GetAPICredential().FirstOrDefault();
            _restClient = new RestClient(_netoAPICredential.URL);
            //_restClient.Timeout = 600000;
        }

        

        public IEnumerable<GetOrderResponseOrder> DownloadNewNetoOrder(DateTime? fromTime, DateTime? toTime)
        {
            try
            {
                int pageNumber = 0;
                int recordsPerPage = 200;
                var retOrders = new List<GetOrderResponseOrder>();
                var getOrderObj = new GetOrder();
                getOrderObj.Filter = new GetOrderFilter();
                if (fromTime != null)
                    getOrderObj.Filter.DatePlacedFrom = ((DateTime)fromTime).ToUniversalTime();
                if (toTime != null)
                    getOrderObj.Filter.DatePlacedTo = ((DateTime)toTime).ToUniversalTime();

                var outputSelector = GetOrderOutputSelector();
                getOrderObj.Filter.OutputSelector = outputSelector;

                
                var gotLastPage = false;
                while (!gotLastPage)
                {
                    getOrderObj.Filter.Page = pageNumber.ToString();
                    getOrderObj.Filter.Limit = recordsPerPage.ToString();

                    var rawXML = CommonFunc.ConvertObjectToXMLString(getOrderObj, Encoding.UTF8);
                    var getOrderRequest = new RestRequest("do/WS/NetoAPI");
                    getOrderRequest.Method = Method.POST;
                    getOrderRequest.AddHeader("NETOAPI_ACTION", "GetOrder");
                    getOrderRequest.AddHeader("NETOAPI_USERNAME", _netoAPICredential.UserName);
                    getOrderRequest.AddHeader("NETOAPI_KEY", _netoAPICredential.APIKey);
                    getOrderRequest.AddParameter("application/xml", rawXML, ParameterType.RequestBody);

                    IRestResponse response = _restClient.Execute(getOrderRequest);
                    if (response.IsSuccessful)
                    {
                        var getOrderResponse = JsonConvert.DeserializeObject<GetOrderResponse>(response.Content);
                        if (getOrderResponse.Ack != GetOrderResponseAck.Error)
                        {
                            retOrders.AddRange(getOrderResponse.Order);
                            if (getOrderResponse.Order.Count() < recordsPerPage)
                                gotLastPage = true;
                        }
                        else
                        {
                            throw new Exception(getOrderResponse.Messages.Error.Select(err => err.Message).Aggregate((current, next) => (current + ";" + next)));
                        }
                    }
                    else
                    {
                        throw new Exception(response.ErrorMessage);
                    }
                    pageNumber++;
                }

                return retOrders;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public IEnumerable<GetItemResponseItem> GetNetoProductBySKUs(string[] skus, bool isWithDesc = false)
        {
            try
            {
                var getItemObj = new GetItem();
                getItemObj.Filter = new GetItemFilter();
                getItemObj.Filter.SKU = skus;
                getItemObj.Filter.OrderBy = GetItemFilterOrderBy.ID;
                getItemObj.Filter.OrderBySpecified = true;
                getItemObj.Filter.OrderDirection = GetItemFilterOrderDirection.ASC;
                getItemObj.Filter.OrderDirectionSpecified = true;
                var outputSelector = GetOutputSelector(isWithDesc);
                getItemObj.Filter.OutputSelector = outputSelector;
                getItemObj.Filter.Page = 0.ToString();
                getItemObj.Filter.Limit = skus.Count().ToString();

                var rawXML = CommonFunc.ConvertObjectToXMLString(getItemObj, Encoding.UTF8);
                var getItemRequest = new RestRequest("do/WS/NetoAPI");
                getItemRequest.Method = Method.POST;
                getItemRequest.AddHeader("NETOAPI_ACTION", "GetItem");
                getItemRequest.AddHeader("NETOAPI_USERNAME", _netoAPICredential.UserName);
                getItemRequest.AddHeader("NETOAPI_KEY", _netoAPICredential.APIKey);
                getItemRequest.AddParameter("application/xml", rawXML, ParameterType.RequestBody);

                IRestResponse response = _restClient.Execute(getItemRequest);
                if (response.IsSuccessful)
                {
                    var getItemResponse = JsonConvert.DeserializeObject<GetItemResponse>(response.Content);
                    if (getItemResponse.Ack != GetItemResponseAck.Error)
                    {
                        return getItemResponse.Item;
                    }
                    else
                    {
                        throw new Exception(getItemResponse.Messages.Error.Select(err => err.Message).Aggregate((current, next) => (current + ";" + next)));
                    }
                }
                else
                {
                    throw new Exception(response.ErrorMessage);
                }

            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public IEnumerable<GetItemResponseItem> GetNetoProducts(bool isWithDesc=false)
        {
            try
            {
                var retProducts = new List<GetItemResponseItem>();

                int pageNumber = 0;
                int recordsPerPage = 1000;
                var getItemObj = new GetItem();
                getItemObj.Filter = new GetItemFilter();
                //getItemObj.Filter.DateAddedFrom = DateTime.Now;
                //getItemObj.Filter.DateAddedFromSpecified = true;
                getItemObj.Filter.IsActive = new string[] { "True" };
                getItemObj.Filter.OrderBy = GetItemFilterOrderBy.ID;
                getItemObj.Filter.OrderBySpecified = true;
                getItemObj.Filter.OrderDirection = GetItemFilterOrderDirection.ASC;
                getItemObj.Filter.OrderDirectionSpecified = true;
                //var outputSelector = CommonFunc.GetEnumList<GetItemFilterOutputSelector>();
                

                var outputSelector=GetOutputSelector(isWithDesc);

                getItemObj.Filter.OutputSelector = outputSelector;
                var gotLastPage = false;
                while(!gotLastPage)
                { 
                    getItemObj.Filter.Page = pageNumber.ToString();
                    getItemObj.Filter.Limit = recordsPerPage.ToString();
                    var rawXML = CommonFunc.ConvertObjectToXMLString(getItemObj, Encoding.UTF8);
                    var getItemRequest = new RestRequest("do/WS/NetoAPI");
                    getItemRequest.Method = Method.POST;
                    getItemRequest.AddHeader("NETOAPI_ACTION", "GetItem");
                    getItemRequest.AddHeader("NETOAPI_USERNAME", _netoAPICredential.UserName);
                    getItemRequest.AddHeader("NETOAPI_KEY", _netoAPICredential.APIKey);
                    getItemRequest.AddParameter("application/xml", rawXML, ParameterType.RequestBody);

                    IRestResponse response = _restClient.Execute(getItemRequest);
                    if (response.IsSuccessful)
                    {
                        var getItemResponse = JsonConvert.DeserializeObject<GetItemResponse>(response.Content);
                        if (getItemResponse.Ack != GetItemResponseAck.Error)
                        {
                            retProducts.AddRange(getItemResponse.Item);
                            if (getItemResponse.Item.Count() < recordsPerPage)
                                gotLastPage = true;
                        }
                        else
                        {
                            throw new Exception(getItemResponse.Messages.Error.Select(err=>err.Message).Aggregate((current,next)=>(current+";"+next)));
                        }
                    }
                    else
                    {
                        throw new Exception(response.ErrorMessage);
                    }
                    pageNumber++;
                }
                return retProducts;
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        

        public ThirdStoreReturnMessage UpdateOrderDeliveryInstruction(UpdateOrder updateOrders)
        {
            try
            {
                if (updateOrders == null || updateOrders.Order == null || updateOrders.Order.Count() == 0)
                    return new ThirdStoreReturnMessage() { IsSuccess = true };

                var rawXML = CommonFunc.ConvertObjectToXMLString(updateOrders, Encoding.UTF8);
                var updateOrderRequest = new RestRequest("do/WS/NetoAPI");
                updateOrderRequest.Method = Method.POST;
                updateOrderRequest.AddHeader("NETOAPI_ACTION", "UpdateOrder");
                updateOrderRequest.AddHeader("NETOAPI_USERNAME", _netoAPICredential.UserName);
                updateOrderRequest.AddHeader("NETOAPI_KEY", _netoAPICredential.APIKey);
                updateOrderRequest.AddParameter("application/xml", rawXML, ParameterType.RequestBody);

                IRestResponse response = _restClient.Execute(updateOrderRequest);
                if (response.IsSuccessful)
                {
                    try
                    {
                        var updateOrderResponse = JsonConvert.DeserializeObject<UpdateOrderResponse>(response.Content);
                        if (updateOrderResponse.Ack != UpdateOrderResponseAck.Error)
                        {
                            return new ThirdStoreReturnMessage() { IsSuccess = true };
                        }
                        else
                        {
                            throw new Exception(updateOrderResponse.Messages.Error.Select(err => err.Message).Aggregate((current, next) => (current + ";" + next)));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error(ex.Message);
                        return new ThirdStoreReturnMessage() { IsSuccess = true, ErrorMesage = ex.Message };
                    }
                }
                else
                {
                    throw new Exception(response.ErrorMessage);
                }
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return new ThirdStoreReturnMessage() { IsSuccess = false, ErrorMesage = ex.Message };
            }
        }

        public ThirdStoreReturnMessage UpdateProducts(UpdateItem updateItem)
        {
            try
            {
                if(updateItem==null||updateItem.Item==null||updateItem.Item.Count()==0)
                    return new ThirdStoreReturnMessage() { IsSuccess = true };

                var rawXML = CommonFunc.ConvertObjectToXMLString(updateItem, Encoding.UTF8);
                var updateItemRequest = new RestRequest("do/WS/NetoAPI");
                updateItemRequest.Method = Method.POST;
                updateItemRequest.AddHeader("NETOAPI_ACTION", "UpdateItem");
                updateItemRequest.AddHeader("NETOAPI_USERNAME", _netoAPICredential.UserName);
                updateItemRequest.AddHeader("NETOAPI_KEY", _netoAPICredential.APIKey);
                updateItemRequest.AddParameter("application/xml", rawXML, ParameterType.RequestBody);

                IRestResponse response = _restClient.Execute(updateItemRequest);
                if (response.IsSuccessful)
                {
                    try
                    {
                        var updateItemResponse = JsonConvert.DeserializeObject<UpdateItemResponse>(response.Content);
                        if (updateItemResponse.Ack != UpdateItemResponseAck.Error)
                        {
                            return new ThirdStoreReturnMessage() { IsSuccess = true };
                        }
                        else
                        {
                            throw new Exception(updateItemResponse.Messages.Error.Select(err => err.Message).Aggregate((current, next) => (current + ";" + next)));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error(ex.Message);
                        return new ThirdStoreReturnMessage() { IsSuccess = true,ErrorMesage=ex.Message };
                    }
                }
                else
                {
                    throw new Exception(response.ErrorMessage);
                }

                //foreach (var item in updateItem.Item)
                //{

                //}

                //return new ThirdStoreReturnMessage() { IsSuccess = true };
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return new ThirdStoreReturnMessage() { IsSuccess=false, ErrorMesage=ex.Message };
            }
        }

        public ThirdStoreReturnMessage AddProducts(AddItem addItem)
        {
            try
            {
                if (addItem == null || addItem.Item == null || addItem.Item.Count() == 0)
                    return new ThirdStoreReturnMessage() { IsSuccess = true };

                var rawXML = CommonFunc.ConvertObjectToXMLString(addItem, Encoding.UTF8);
                var addItemRequest = new RestRequest("do/WS/NetoAPI");
                addItemRequest.Method = Method.POST;
                addItemRequest.AddHeader("NETOAPI_ACTION", "AddItem");
                addItemRequest.AddHeader("NETOAPI_USERNAME", _netoAPICredential.UserName);
                addItemRequest.AddHeader("NETOAPI_KEY", _netoAPICredential.APIKey);
                addItemRequest.AddParameter("application/xml", rawXML, ParameterType.RequestBody);

                IRestResponse response = _restClient.Execute(addItemRequest);
                if (response.IsSuccessful)
                {
                    try
                    {
                        var addItemResponse = JsonConvert.DeserializeObject<AddItemResponse>(response.Content);
                        if (addItemResponse.Ack != AddItemResponseAck.Error)
                        {
                            return new ThirdStoreReturnMessage() { IsSuccess = true };
                        }
                        else
                        {
                            throw new Exception(addItemResponse.Messages.Error.Select(err => err.Message).Aggregate((current, next) => (current + ";" + next)));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error(ex.Message);
                        return new ThirdStoreReturnMessage() { IsSuccess = true, ErrorMesage = ex.Message };
                    }
                }
                else
                {
                    throw new Exception(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return new ThirdStoreReturnMessage() { IsSuccess = false, ErrorMesage = ex.Message };
            }
        }


        private GetOrderFilterOutputSelector[] GetOrderOutputSelector()
        {
            var outputSelector = new List<GetOrderFilterOutputSelector>();
            outputSelector.Add(GetOrderFilterOutputSelector.ID);
            outputSelector.Add(GetOrderFilterOutputSelector.ShippingOption);
            outputSelector.Add(GetOrderFilterOutputSelector.DeliveryInstruction);
            outputSelector.Add(GetOrderFilterOutputSelector.Username);
            outputSelector.Add(GetOrderFilterOutputSelector.Email);
            outputSelector.Add(GetOrderFilterOutputSelector.ShipAddress);
            outputSelector.Add(GetOrderFilterOutputSelector.BillAddress);
            outputSelector.Add(GetOrderFilterOutputSelector.CustomerRef1);
            outputSelector.Add(GetOrderFilterOutputSelector.CustomerRef2);
            outputSelector.Add(GetOrderFilterOutputSelector.SalesChannel);
            outputSelector.Add(GetOrderFilterOutputSelector.GrandTotal);
            outputSelector.Add(GetOrderFilterOutputSelector.ShippingTotal);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderType);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderStatus);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderPayment);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderPaymentPaymentType);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderPaymentDatePaid);
            outputSelector.Add(GetOrderFilterOutputSelector.DatePlaced);
            outputSelector.Add(GetOrderFilterOutputSelector.DateRequired);
            outputSelector.Add(GetOrderFilterOutputSelector.DateInvoiced);
            outputSelector.Add(GetOrderFilterOutputSelector.DatePaid);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLine);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineProductName);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLinePickQuantity);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineBackorderQuantity);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineUnitPrice);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineWarehouseID);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineWarehouseName);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineWarehouseReference);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineQuantity);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLinePercentDiscount);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineProductDiscount);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineCostPrice);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineShippingMethod);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineShippingTracking);
            outputSelector.Add(GetOrderFilterOutputSelector.ShippingSignature);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineeBayeBayUsername);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineeBayeBayStoreName);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineeBayeBayTransactionID);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineeBayeBayAuctionID);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineeBayListingType);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineeBayDateCreated);
            outputSelector.Add(GetOrderFilterOutputSelector.OrderLineeBayDatePaid);
            

            return outputSelector.ToArray();
        }

        private GetItemFilterOutputSelector[] GetOutputSelector(bool isWithDesc = false)
        {
            var outputSelector = new List<GetItemFilterOutputSelector>();
            outputSelector.Add(GetItemFilterOutputSelector.ParentSKU);
            outputSelector.Add(GetItemFilterOutputSelector.ID);
            outputSelector.Add(GetItemFilterOutputSelector.Brand);
            outputSelector.Add(GetItemFilterOutputSelector.Model);
            outputSelector.Add(GetItemFilterOutputSelector.Name);
            outputSelector.Add(GetItemFilterOutputSelector.PrimarySupplier);
            outputSelector.Add(GetItemFilterOutputSelector.Approved);
            outputSelector.Add(GetItemFilterOutputSelector.IsActive);
            outputSelector.Add(GetItemFilterOutputSelector.PriceGroups);
            outputSelector.Add(GetItemFilterOutputSelector.ShippingLength);
            outputSelector.Add(GetItemFilterOutputSelector.ShippingWidth);
            outputSelector.Add(GetItemFilterOutputSelector.ShippingHeight);
            outputSelector.Add(GetItemFilterOutputSelector.ShippingWeight);
            outputSelector.Add(GetItemFilterOutputSelector.CubicWeight);
            outputSelector.Add(GetItemFilterOutputSelector.WarehouseQuantity);
            outputSelector.Add(GetItemFilterOutputSelector.WarehouseLocations);
            outputSelector.Add(GetItemFilterOutputSelector.Categories);
            outputSelector.Add(GetItemFilterOutputSelector.DefaultPrice);
            if (isWithDesc)
                outputSelector.Add(GetItemFilterOutputSelector.Description);
            outputSelector.Add(GetItemFilterOutputSelector.Images);
            outputSelector.Add(GetItemFilterOutputSelector.DateAdded);

            return outputSelector.ToArray();
        }

        
    }
}
