using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestAPI1._0.Data;
using RestAPI1._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI1._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {

        private CinemaDbContext _dbContext;

        //ctor tab tab to auto create constructor 
        public ReservationsController(CinemaDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [Authorize]
        [HttpPost]
        public IActionResult Post([FromBody] Reservation reservationObj)
        {
            reservationObj.ReservationTime = DateTime.Now;
            _dbContext.Reservations.Add(reservationObj);
            _dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetReservations()
        {
            var reservations = from reservation in _dbContext.Reservations
                               join customer in _dbContext.Users on reservation.UserId equals customer.Id
                               join movie in _dbContext.Movies on reservation.MovieId equals movie.Id
                               select new
                               {
                                   Id = reservation.Id,
                                   ReservationTime = reservation.ReservationTime,
                                   CustomerName = customer.Id,
                                   MovieName = movie.Name
                               };
            return Ok(reservations);

        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public IActionResult GetReservationsDetail(int id)
        {
            //Use FirstOfDefault to prevent single object inside JSON array
            var reservationResult = (from reservation in _dbContext.Reservations
                                     join customer in _dbContext.Users on reservation.UserId equals customer.Id
                                     join movie in _dbContext.Movies on reservation.MovieId equals movie.Id
                                     //Add id match
                                     where reservation.Id == id
                                     select new
                                     {
                                         Id = reservation.Id,
                                         ReservationTime = reservation.ReservationTime,
                                         CustomerName = customer.Id,
                                         MovieName = movie.Name,
                                         Email = customer.Email,
                                         Qty = reservation.Qty,
                                         Price = reservation.Price,
                                         Phone = reservation.Phone,
                                         PlayingDate = movie.PlayingDate,
                                         PlayingTime = movie.PlayingTime
                                     }).FirstOrDefault();
            return Ok(reservationResult);

        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var reservation = _dbContext.Reservations.Find(id);
            if (reservation == null)
            {
                return BadRequest("No record found with id");
            }
            _dbContext.Reservations.Remove(reservation);
            _dbContext.SaveChanges();
            return Ok("Deleted Record");
        }
    }
}
