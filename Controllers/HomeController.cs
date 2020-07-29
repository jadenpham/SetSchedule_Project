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

        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search(Form newForm)
        {
            if (ModelState.IsValid)
            {
                var httpClient = HttpClientFactory.Create();

            //converting miles to meter
                double meter = ConvertMilesToMeters(newForm.Distance);

            //Grabbing googlekey from appsettings.json
                var googleKey = _configuration.GetSection("API_KEY").GetSection("GoogleKey").Value;

                string googleUrl = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={newForm.Topic}&location={newForm.Lattitude},{newForm.Longitude}&radius={meter}&key={googleKey}";

            //if no results, returns closest to location
                //try/catch block
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(googleUrl);
                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var UrlContent = httpResponseMessage.Content;
                    string data = await UrlContent.ReadAsStringAsync();

                //converting data(json) as a string to Data type, push into list<data>
                    List<Data> DataList = ConvertJsonToList(data);
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
        
    }
}
