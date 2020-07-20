using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
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
            //grabbing user's input data
                var httpClient = HttpClientFactory.Create();
                string searchContent = newForm.Topic;
                string address = newForm.Address;
                double distance = newForm.Distance;
                
            //Converting user miles to meter to be used w api call
                double meter = Converter.ConvertMilesToMeters(distance);
                var googleKey = _configuration.GetSection("API_KEY").GetSection("GoogleKey").Value;
                Console.WriteLine(googleKey);
            //Converting user address to long and lat, use w api call
                string addressLongLat = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={googleKey}";
                
                HttpResponseMessage addressResponseMessage = await httpClient.GetAsync(addressLongLat);

                var addressContent = addressResponseMessage.Content;
                
                string LongLat = await addressContent.ReadAsStringAsync();
                
                AddressLngLat addressLatLng = Converter.ConvertJsonToLatLng(LongLat);
                
                string googleUrl = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={searchContent}&location=&{addressLatLng.Lattitude},{addressLatLng.Longitude}&radius={meter}&key={googleKey}";



                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(googleUrl);
                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
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

        //create route for details view, taking in businessID
        //pass id to be queried for name and phone number
        //from phone number, make it a link to another route that queries for yelp results, can grab yelp url from there
        [HttpGet("details/{businessId}")]
        public async Task<IActionResult> Details(string businessId)
        {
            var googleKey = _configuration.GetSection("API_KEY").GetSection("GoogleKey").Value;
            var httpClient = HttpClientFactory.Create();
            string detailSearch = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={businessId}&fields=name,formatted_phone_number&key={googleKey}";
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(detailSearch);
            if(httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = httpResponseMessage.Content;
                string data = await content.ReadAsStringAsync();
                BusinessDetail detail= Converter.ConvertToDetail(data);
                ViewBag.detailData = detail;
            }
            return View();
        }

        [HttpGet("yelp/{phoneNumber}")]
        public async Task<IActionResult> Yelp(string phoneNumber)
        {
            var client = new HttpClient();
            var apiKey = _configuration.GetSection("API_KEY").GetSection("YelpKey").Value;

            var YelpSearch = $"https://api.yelp.com/v3/businesses/search/phone?phone=+1{phoneNumber}";
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

            string response = await client.GetStringAsync(YelpSearch);
            Console.WriteLine(response);
            Yelp yelpUrl = Converter.ConvertToYelpUrl(response);
            //yelpUrl.Url is converted to null if there's no results
            if(yelpUrl.Url == "Null")
            {
                ViewBag.url = "Null";
            }
            ViewBag.url = yelpUrl.Url;
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
            public static Yelp ConvertToYelpUrl(string json)
            {
                JObject jObject = JObject.Parse(json);
                JToken jResults = jObject["businesses"];
                JArray length = (JArray)jResults;
                if (length.Count > 0)
                {
                    return new Yelp((string)jResults[0]["url"], (string)jResults[0]["name"]);
                }
                return new Yelp("Null", "Null");
            }
            public static List<Data> ConvertJsonToList(string json)
            {
                List<Data> DataList = new List<Data>();
                JObject jObject = JObject.Parse(json);
                JToken jResults = jObject["results"];
                JArray items = (JArray)jResults;
                for (int i = 0; i < items.Count; i++)
                {
                    DataList.Add(new Data((string)items[i]["name"], 
                                          (string)items[i]["business_status"], 
                                          (string)items[i]["formatted_address"], 
                                          (double)items[i]["rating"], 
                                          (string)items[i]["place_id"]));
                }
                return DataList;
            }
            //Converts phone number like (xxx) xxx-xxxx to xxxxxxxxxx
            public static string ConvertToNumber(string str)
            {
                string number = "";
                foreach (char ch in str)
                {
                    if (ch != ' ' && ch != '+' && ch != '(' && ch != ')' && ch != '-')
                    {
                        number = number + ch.ToString();
                    }
                }
                if (number.Length > 10)
                {
                    number = number.Substring(number.Length - 10, 10);
                }
                return number;
            }
            //convert object to data w name and phone number
            public static BusinessDetail ConvertToDetail(string str)
            {
                JObject jObject = JObject.Parse(str);
                JToken jResults = jObject["result"];
                //return new BusinessDetail((string)jResults[0]["name"], (string)jResults[0])
                //if theres a phone number then add it
                if (jResults["formatted_phone_number"] != null)
                {
                    jResults["formatted_phone_number"] = ConvertToNumber((string)jResults["formatted_phone_number"]);
                    return new BusinessDetail((string)jResults["name"], (string)jResults["formatted_phone_number"]);
                }
                //else just use constructor w name
                else
                {
                    return new BusinessDetail((string)jResults["name"]);
                }                
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
                return new AddressLngLat((double)jResults[0]["geometry"]["location"]["lat"], 
                                         (double)jResults[0]["geometry"]["location"]["lng"]);
            }

        }
    }
}
