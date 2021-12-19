using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI1._0.Models
{
    public class Movie
    {
        public int Id { get; set; }
        //makes users require to input a name, cant be null or empty
        //Model validation 
        [Required(ErrorMessage ="Enter a name please")]
        public string Name { get; set; }
        [Required]
        public string Language { get; set; }
        [Required]
        public double Rating { get; set; }
        public string Duration { get; set; }
        public DateTime PlayingDate { get; set; }
        public DateTime PlayingTime { get; set; }
        public double TicketPrice { get; set; }
        public string Genre { get; set; }
        public string TrailorUrl { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public ICollection<Reservation> Reservations { get; set; }
        //Not a part of our db 
        [NotMapped]
        public IFormFile Image { get; set; }
    }
}
