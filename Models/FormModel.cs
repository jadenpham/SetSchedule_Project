using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Form

    {
        [Required]
        public string Topic { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        [Range(1, 30, ErrorMessage ="Input value between 1 and 30")]
        public int Distance { get; set; }
        public double Lattitude { get; set; }
        public double Longitude { get; set; }
    }
}
