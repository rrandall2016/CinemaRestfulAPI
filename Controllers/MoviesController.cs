using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestAPI1._0.Data;
using RestAPI1._0.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI1._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private CinemaDbContext _dbContext;
        public MoviesController(CinemaDbContext dbContext)
        {
            //Access db
            _dbContext = dbContext;
        }
        [Authorize]
        [HttpGet("[action]")]
        public IActionResult AllMovies(string sort, int? pageNumber, int? pageSize)
        {
            //If pagenumber and pagesize is null take these values 1 and 2 else assign them the values from the url query
            var currentPageNumber = pageNumber ?? 1;
            var currentPageSize = pageSize ?? 2;
            //Anonymous class? 
            var movies = from movie in _dbContext.Movies
            select new
            {
                Id = movie.Id,
                Name = movie.Name,
                Duration = movie.Duration,
                Languague = movie.Language,
                Rating = movie.Rating,
                Genre = movie.Genre,
                ImageUrl = movie.ImageUrl,
            };
            //User will query this in url as either ascending or descending and if neither than default to all movies any order 
            switch (sort)
            {
                case "desc":
                    return Ok(movies.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize).OrderByDescending(m => m.Rating));
                case "asc":
                    return Ok(movies.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize).OrderBy(m => m.Rating));
                // Skip the 5 records from the previous page, or if you are on the first page, then show first 5 records
                default: return Ok(movies.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize));
            }
            
        }
        //Used because doesn't start with get in method name
        //api/movies/moviedetail/1
        [Authorize]
        [HttpGet("[action]/{id}")]
        public IActionResult MovieDetail(int id)
        {
            var movie = _dbContext.Movies.Find(id);
            if(movie == null)
            {
                return NotFound();
            }
            return Ok(movie);

        }
        [Authorize]
        [HttpGet("[action]")]
        public IActionResult FindMovies(string movieName)
        {
            //Check if movie name starts with movieName
            var movies = from movie in _dbContext.Movies
                         where movie.Name.StartsWith(movieName)
                         select new
                         { 
                             Id = movie.Id,
                             Name = movie.Name,
                             Rating = movie.Rating,
                             ImageUrl = movie.ImageUrl
                         };
            return Ok(movies);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromForm] Movie movieObj)
        {
            //Creates new Guid everytime for duplicate name prevention errors 
            var guid = Guid.NewGuid();

            //combine root folder of wwwoot and file name
            var filePath = Path.Combine("wwwroot", guid + ".jpg");

            if (movieObj.Image != null)
            {
                var fileStream = new FileStream(filePath, FileMode.Create);
                movieObj.Image.CopyTo(fileStream);
            }
            //Save file path inside db
            movieObj.ImageUrl = filePath.Remove(0, 7);
            _dbContext.Movies.Add(movieObj);
            _dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] Movie movie)
        {
            var oldInfo = _dbContext.Movies.Find(id);
            if (oldInfo == null)
            {
                return BadRequest();
            }
            else
            {

                var guid = Guid.NewGuid();
                var filePath = Path.Combine("wwwroot", guid + ".jpg");

                if (movie.Image != null)
                {
                    var fileStream = new FileStream(filePath, FileMode.Create);
                    movie.Image.CopyTo(fileStream);
                    //Save file path inside db
                    oldInfo.ImageUrl = filePath.Remove(0, 7);
                }
                oldInfo.Name = movie.Name;
                oldInfo.Language = movie.Language;
                oldInfo.Rating = movie.Rating;
                oldInfo.Description = movie.Description;
                oldInfo.Duration = movie.Duration;
                oldInfo.PlayingDate = movie.PlayingDate;
                oldInfo.PlayingTime = movie.PlayingTime;
                oldInfo.Genre = movie.Genre;
                oldInfo.TrailorUrl = movie.TrailorUrl;
                _dbContext.SaveChanges();
                return Ok("Updated Record");
            }
        }
        [Authorize(Roles ="Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var movie = _dbContext.Movies.Find(id);
            if (movie == null)
            {
                return BadRequest();
            }
            _dbContext.Movies.Remove(movie);
            _dbContext.SaveChanges();
            return Ok("Deleted Record");
        }



    }
}
