using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThirdStoreCommon;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using Newtonsoft.Json;
using LINQtoCSV;
using ThirdStoreBusiness.Setting;
using System.Threading;

namespace ThirdStoreBusiness.API.Dropshipzone
{

    #region Dropshipzone Basic Class

    public class DropshipzoneApiCredential
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ResourceURL { get; set; }
    }

    public class GetDropshipzoneProductResponse
    {
        public List<DropshipzoneProduct> result { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
        public int current_page { get; set; }
        public int page_size { get; set; }
    }

    public class DropshipzoneProduct
    {
        public string sku { get; set; }
        public string Category { get; set; }
        public string RrpPrice { get; set; }
        public int Vendor_Product { get; set; }
        public string brand { get; set; }
        public string cost { get; set; }
        public string currency { get; set; }
        public string desc { get; set; }
        public string discontinuedproduct { get; set; }
        public string eancode { get; set; }
        public int entity_id { get; set; }
        public string height { get; set; }
        public string in_stock { get; set; }
        public string length { get; set; }
        public string price { get; set; }
        public int product_status { get; set; }
        public string status { get; set; }
        public string stock_qty { get; set; }
        public string title { get; set; }
        public string website_url { get; set; }
        public string weight { get; set; }
        public string width { get; set; }
        public string ETA { get; set; }
        public string bulky_item { get; set; }
        public string colour { get; set; }
        public string discontinued { get; set; }
        [JsonConverter(typeof(SingleValueArrayConverter<ZoneRates>))]
        public List<ZoneRates> zone_rates { get; set; }
        public string special_price { get; set; }
        public string special_price_end_date { get; set; }
        public List<string> gallery { get; set; }
        public string freeshipping { get; set; }

        public class ZoneRates
        {
            public string sku { get; set; }
            public string act { get; set; }
            public string nsw_m { get; set; }
            public string nsw_r { get; set; }
            public string nt_m { get; set; }
            public string nt_r { get; set; }
            public string qld_m { get; set; }
            public string qld_r { get; set; }
            public string remote { get; set; }
            public string sa_m { get; set; }
            public string sa_r { get; set; }
            public string tas_m { get; set; }
            public string tas_r { get; set; }
            public string vic_m { get; set; }
            public string vic_r { get; set; }
            public string wa_m { get; set; }
            public string wa_r { get; set; }
        }
    }

    public class DropshipzoneAuthRequest
    {
        public string email { get; set; }
        public string password { get; set; }
    }

    public class DropshipzoneAuthResponse
    {
        public int iat { get; set; }
        public int exp { get; set; }
        public string token { get; set; }
    }



    #endregion

    public interface IDropshipzoneCredentialProvider
    {
        DropshipzoneApiCredential GetAPICredential();
    }

    public interface IDropshipzoneAPICallManager
    {
        IList<DropshipzoneProduct> GetAllProducts();

    }


    public class DropshipzoneAPICredentialProvider : IDropshipzoneCredentialProvider
    {
        private readonly ISettingService _settingService;
        private readonly DropshipzoneAPISettings _apiSetting;
        public DropshipzoneAPICredentialProvider(ISettingService settingService)
        {
            _settingService = settingService;
            _apiSetting = _settingService.LoadSetting<DropshipzoneAPISettings>();
        }

        public DropshipzoneApiCredential GetAPICredential()
        {
            try
            {
                var credential = new DropshipzoneApiCredential();
                var client = new RestClient(_apiSetting.URL);
                var accessTokenRequest = new RestRequest("/auth");
                accessTokenRequest.Method = Method.POST;
                accessTokenRequest.AddHeader("Content-Type", "application/json");
                var auth = new DropshipzoneAuthRequest() { email = _apiSetting.Email, password=_apiSetting.Password };
                var jsonString = JsonConvert.SerializeObject(auth);
                accessTokenRequest.AddParameter("application/json", jsonString, ParameterType.RequestBody);
                var accessTokenResponse = client.Execute<DropshipzoneAuthResponse>(accessTokenRequest);
                if (accessTokenResponse != null && accessTokenResponse.IsSuccessful && accessTokenResponse.Data != null)
                {
                    var tokenData = accessTokenResponse.Data;
                    credential.Token = tokenData.token;
                    credential.ResourceURL = _apiSetting.URL;
                    credential.Email = _apiSetting.Email;
                    credential.Password = _apiSetting.Password;
                }
                else
                    throw new Exception("Token request error.");

                return credential;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return default(DropshipzoneApiCredential);
            }
        }
    }

    public class DropshipzoneAPICallManager : IDropshipzoneAPICallManager
    {
        private readonly IDropshipzoneCredentialProvider _myDealAPICredentialProvider;
        private DropshipzoneApiCredential _myDealAPICredential;
        private RestClient _restClient;
        public DropshipzoneAPICallManager(IDropshipzoneCredentialProvider myDealAPICredentialProvider)
        {
            _myDealAPICredentialProvider = myDealAPICredentialProvider;
            //_myDealAPICredential = myDealAPICredentialProvider.GetAPICredential();
            //if (_myDealAPICredential != null)
            //{
            //    _restClient = new RestClient(_myDealAPICredential.ResourceURL);
            //    _restClient.Authenticator = new JwtAuthenticator(_myDealAPICredential.BearerToken);
            //}
        }

        protected DropshipzoneApiCredential APICredential
        {
            get
            {
                if(_myDealAPICredential==null)
                    _myDealAPICredential = _myDealAPICredentialProvider.GetAPICredential();
                return _myDealAPICredential;
            }
        }

        protected RestClient APIClient
        {
            get
            {
                if(_restClient==null)
                {
                    _restClient = new RestClient(APICredential.ResourceURL);
                    _restClient.Authenticator = new JwtAuthenticator(APICredential.Token);
                }
                return _restClient;
            }
        }


        public IList<DropshipzoneProduct> GetAllProducts()
        {
            try
            {
                if (APICredential == null)
                    return default(IList<DropshipzoneProduct>);

                var retProducts = new List<DropshipzoneProduct>();

                var pageNum = 1;
                var limit = 160;
                var totalPage=1;
                var exceedLimit = 0;
                do
                {
                    if(pageNum/60!=exceedLimit)
                    {
                        Thread.Sleep(1000 * 61);
                        exceedLimit = pageNum / 60;
                    }
                    //var getProductsRequest = new RestRequest("/products?limit={limit}&page_no={pageNum}");
                    var getProductsRequest = new RestRequest("/products");
                    //getProductsRequest.AddHeader("api-version", "2.4");
                    getProductsRequest.AddHeader("Content-Type", "application/json");
                    getProductsRequest.AddParameter("limit", limit, ParameterType.QueryString);
                    getProductsRequest.AddParameter("page_no", pageNum, ParameterType.QueryString);
                    var getProductsResponse = APIClient.Execute<GetDropshipzoneProductResponse>(getProductsRequest);
                    if (getProductsResponse != null && getProductsResponse.StatusCode == HttpStatusCode.OK && getProductsResponse.Data != null)
                    {
                        var actionResponse = getProductsResponse.Data;
                        if (actionResponse.result != null
                            && actionResponse.result.Count > 0)
                        {
                            var fetchedProducts = actionResponse.result;
                            retProducts.AddRange(fetchedProducts);
                            totalPage = actionResponse.total_pages;
                            pageNum++;
                        }
                    }
                    else
                    {
                        throw new Exception(getProductsResponse.Content);
                    }


                }
                while (pageNum <= totalPage);

                return retProducts;

            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        
    }
}
