using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApplication1
{
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
            if (jResults["formatted_phone_number"] != null)
            {
                jResults["formatted_phone_number"] = ConvertToNumber((string)jResults["formatted_phone_number"]);
                return new BusinessDetail((string)jResults["name"], 
                                            (string)jResults["formatted_phone_number"]);
            }
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
