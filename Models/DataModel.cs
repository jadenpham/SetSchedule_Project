using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    //public class GoogleResult
    //{
    //    public string BusinessName { get; set; }
    //    public string Business_Status { get; set; }
    //    public string Address { get; set; }
    //    public double Rating { get; set; }
    //    public string[] BusinessType { get; set; }
    //    public override string ToString()
    //    {
    //        return $"{BusinessName} : {Business_Status}, {Address}, {Rating}, {BusinessType}";
    //    }
    //}
    public class Data
    {
        public Data(string name, string status, string address, double rating, string businessId)
        {
            Name = name;
            Business_Status = status;
            formatted_address = address;
            this.rating = rating;
            BusinessId = businessId;
        }
        public string Name { get; set; }
        public string Business_Status { get; set; }
        public string formatted_address { get; set; }
        public double rating { get; set; }
        public string BusinessId { get; set; }

        public int PhoneNumber { get; set; }
        public override string ToString()
        {
            return $"{Name} : {Business_Status}, {formatted_address}, Rating: {rating}";
        }
    }

    public class AddressLngLat
    {
        public AddressLngLat(double lat, double lng)
        {
            Lattitude = lat;
            Longitude = lng;
        }
        public double Lattitude { get; set; }
        public double Longitude { get; set; }

        public override string ToString()
        {
            return $"Lattiude: {Lattitude}, Longitude: {Longitude}";
        }
    }
    public class BusinessDetail
    {
        public BusinessDetail(string name, string phonenumber)
        {
            Name = name;
            PhoneNumber = phonenumber;
        }
        public BusinessDetail(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
       
    }
    public class Yelp
    {
        public Yelp(string url, string name)
        {
            Name = name;
            Url = url;
        }
        public Yelp(string name)
        {
            Name = name;
        }
        public string Url { get; set; }
        public string Name { get; set; }
    }
}
