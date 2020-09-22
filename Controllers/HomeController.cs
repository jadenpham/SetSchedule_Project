using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApplication1.Models;
using static WebApplication1.Converter;

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

        //testing git add feature
        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search(Form newForm)
        {
            if (ModelState.IsValid)
            {
                var httpClient = HttpClientFactory.Create();

                double meter = ConvertMilesToMeters(newForm.Distance);

                var googleKey = _configuration.GetSection("API_KEY").GetSection("GoogleKey").Value;

                string googleUrl = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={newForm.Topic}&location={newForm.Lattitude},{newForm.Longitude}&radius={meter}&key={googleKey}";

                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(googleUrl);
                try
                {
                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        var UrlContent = httpResponseMessage.Content;
                        string data = await UrlContent.ReadAsStringAsync();

                        List<Data> DataList = ConvertJsonToList(data);
                        //order list by rating and status (operation vs closed_temp)
                        TempData["data"] = JsonConvert.SerializeObject(DataList);

                        return RedirectToAction("Result");
                    }
                    else
                    {
                        //throw new Exception("Try again");
                        return View("Index");
                    }
                }
                catch(HttpRequestException)
                {
                    return View("Index");
                }
            }
            return View("Index");
        }
        [HttpGet("result")]
        public IActionResult Result()
        {
            //add google map here? click on address and pops up a map w location
            try
            {
                 object data = JsonConvert.DeserializeObject<List<Data>>(Convert.ToString(TempData["data"]));
                TempData.Keep("data");
                ViewBag.data = data;
                return View();
            }
            catch (Exception)
            {
                return View("Index");
            }
        }

        [HttpGet("details/{businessId}")]
        public async Task<IActionResult> Details(string businessId)
        {
            var googleKey = _configuration.GetSection("API_KEY").GetSection("GoogleKey").Value;
            
            var httpClient = HttpClientFactory.Create();

            string detailSearch = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={businessId}&fields=name,formatted_phone_number&key={googleKey}";


            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(detailSearch);
            try
            {
                if(httpResponseMessage.IsSuccessStatusCode)
                {
                    var content = httpResponseMessage.Content;
                    string data = await content.ReadAsStringAsync();

                    BusinessDetail detail= ConvertToDetail(data);
                    ViewBag.detailData = detail;

                    var client = new HttpClient();
                    var apiKey = _configuration.GetSection("API_KEY").GetSection("YelpKey").Value;

                    var YelpSearch = $"https://api.yelp.com/v3/businesses/search/phone?phone=+1{detail.PhoneNumber}";
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

                    string response = await client.GetStringAsync(YelpSearch);

                    Yelp yelpUrl = ConvertToYelpUrl(response);

                    if (yelpUrl.Url == "Null")
                    {
                        ViewBag.url = "Null";
                    }
                    ViewBag.url = yelpUrl.Url;
                    return View();
                }
                else
                {
                    return View("Index");
                }
            }
            catch (HttpRequestException)
            {
                return View("Index");
            }
        }
        [HttpGet("map/{address}")]
        public IActionResult Map(string address)
        {
            //ViewBag.address = address;
            string key = _configuration.GetSection("API_KEY").GetSection("GoogleKey").Value;
            string mapUrl = $"https://www.google.com/maps/embed/v1/place?key={key}&q={address}";
            ViewBag.mapUrl = mapUrl;
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
        //[HttpGet("yelp/{phoneNumber}")]
        //public async Task<IActionResult> Yelp(string phoneNumber)
        //{
        //    //add error handling
        //    var client = new HttpClient();
        //    var apiKey = _configuration.GetSection("API_KEY").GetSection("YelpKey").Value;

        //    var YelpSearch = $"https://api.yelp.com/v3/businesses/search/phone?phone=+1{phoneNumber}";
        //    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

        //    string response = await client.GetStringAsync(YelpSearch);
        //    Yelp yelpUrl = ConvertToYelpUrl(response);

        //    //yelpUrl.Url is converted to null if there's no results
        //    if(yelpUrl.Url == "Null")
        //    {
        //        ViewBag.url = "Null";
        //    }
        //    ViewBag.url = yelpUrl.Url;
        //    return View();

        //}
        
    }
}
