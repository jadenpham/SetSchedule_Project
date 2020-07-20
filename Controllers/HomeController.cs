using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public HomeController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        //private readonly IOptions<SettingsModel> appSettings;
        //public HomeController(IOptions<SettingsModel> app)
        //{
        //    appSettings = app;
        //}
        public IActionResult Index()
        {
            return View();

        }

        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search(Form newForm)
        {
            if (ModelState.IsValid)
            {
            //need to figure out how to deal w address and location proximity(converted miles to meter) done
            //use location lat and long and determine distance from that?
            //grabbing user's input data
                var httpClient = HttpClientFactory.Create();
                string searchContent = newForm.Topic;
                string address = newForm.Address;
                double distance = newForm.Distance;
                
            //Converting user miles to meter to be used w api call
                double meter = Converter.ConvertMilesToMeters(distance);
            //Converting user address to long and lat, use w api call
                string addressLongLat = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key=AIzaSyD8sx3lRHlJFandpCju6sfAvIpbTQ0Qcwc";
                
                HttpResponseMessage addressResponseMessage = await httpClient.GetAsync(addressLongLat);

                var addressContent = addressResponseMessage.Content;
                
                string LongLat = await addressContent.ReadAsStringAsync();
                
                AddressLngLat addressLatLng = Converter.ConvertJsonToLatLng(LongLat);
                
                string googleUrl = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={searchContent}&location=&{addressLatLng.Lattitude},{addressLatLng.Longitude}&radius={meter}&key=AIzaSyD8sx3lRHlJFandpCju6sfAvIpbTQ0Qcwc";



                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(googleUrl);
                if(httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var UrlContent = httpResponseMessage.Content;
                    string data = await UrlContent.ReadAsStringAsync();
                //converting data(json) as a string to Data type, push into list<data>
                    List<Data> DataList = Converter.ConvertJsonToList(data);
                    TempData["data"] = JsonConvert.SerializeObject(DataList);
                    return RedirectToAction("Result");
                }
            }
            return View("Index");
        }
        [HttpGet("result")]
        public IActionResult Result()
        {

            ViewBag.data = JsonConvert.DeserializeObject<List<Data>>(Convert.ToString(TempData["data"]));
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public class Converter
        {
            public static List<Data> ConvertJsonToList(string json)
            {
                List<Data> DataList = new List<Data>();
                JObject jObject = JObject.Parse(json);
                JToken jResults = jObject["results"];
                JArray items = (JArray)jResults;
                for (int i = 0; i < items.Count; i++)
                {
                    DataList.Add(new Data((string)items[i]["name"], (string)items[i]["business_status"], (string)items[i]["formatted_address"], (double)items[i]["rating"]));
                }
                return DataList;
            }
            public static double ConvertMilesToMeters(double miles)
            {
                return miles * 1609.344;
            }

            //convert longlat json to
            public static AddressLngLat ConvertJsonToLatLng(string json)
            {
                JObject jOjbect = JObject.Parse(json);
                JToken jResults = jOjbect["results"];
                return new AddressLngLat((double)jResults[0]["geometry"]["location"]["lat"], (double)jResults[0]["geometry"]["location"]["lng"]);
            }

        }
    }
}
