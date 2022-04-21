using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private MyContext _context;

        public HomeController(ILogger<HomeController> logger, MyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }



        [HttpPost("/users/create")]
        public IActionResult createUser(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(s => s.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                _context.Add(newUser);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("UserId", newUser.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }





        [HttpPost("/users/login")]
        public IActionResult PostLogin(LoginUser loginUser)
        {
            if (ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(d => d.Email == loginUser.LoginEmail);
                if (userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(loginUser, userInDb.Password, loginUser.LoginPassword);
                if (result == 0)
                {
                    ModelState.AddModelError("Password", "Invalid Email/Password");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }


        [HttpGet("/DashBoard")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            var AllWeddings = _context.Weddings.Include(e => e.GuestList).ThenInclude(w => w.Wedding).OrderBy(d => d.DateOfWedding).ToList();
            ViewBag.UserId =  (int)HttpContext.Session.GetInt32("UserId");
            return View("Dashboard", AllWeddings);
        }


        [HttpGet("/create/wedding")]
        public IActionResult NewWedding()
        {
            return View("NewWedding");
        }

        [HttpPost("/wedding/create/post")]
        public IActionResult NewWeddingPost(Wedding newWedding)
        {
            if(ModelState.IsValid)
            {
                if(newWedding.DateOfWedding < DateTime.Now)
                {
                    ModelState.AddModelError("DateOfWedding", "Date has to be in the future");
                    return View("NewWedding");
                }
                newWedding.UserId = (int)HttpContext.Session.GetInt32("UserId");
                _context.Add(newWedding);
                _context.SaveChanges();
                return Redirect($"/wedding/view/{newWedding.WeddingId}");
            }else
            {
                return View("NewWedding");
            }
        }


        [HttpGet("/wedding/view/{id}")]
        public IActionResult InfoWedding(int id)
        {
            ViewBag.WeddingInfo = _context.Weddings.FirstOrDefault(a => a.WeddingId == id);
            ViewBag.List = _context.Weddings.Include(e => e.GuestList).ThenInclude(w => w.User).FirstOrDefault(w => w.WeddingId == id);
            return View("InfoWedding");
        }


        [HttpPost("/addguest")]
        public IActionResult AddGuest(Managment newManagment)
        {
            _context.Add(newManagment);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }




        [HttpPost("/removeguest")]
        public IActionResult RemoveGuest(Managment newManagment)
        {
            Managment removeManagment = _context.Managments.SingleOrDefault(a => a.WeddingId == newManagment.WeddingId && a.UserId == newManagment.UserId);
            _context.Remove(removeManagment);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }



        [HttpPost("/deletewedding")]
        public IActionResult DeleteWedding(Wedding newManagment)
        {
            Wedding removeWedding = _context.Weddings.SingleOrDefault(a => a.WeddingId == newManagment.WeddingId);
            _context.Remove(removeWedding);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }
















        [HttpPost("/users/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
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
