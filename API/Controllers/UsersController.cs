using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using API.Models;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EFDbContext _context;

        public UsersController(EFDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _context.Users.Where(u => u.Status == EmployeeStatus.Active).Select(u => new
            {
                id = u.Id,
                firstName = u.FirstName,
                lastName = u.LastName,
                middleName = u.MiddleName,
                gender = u.Gender,
                dateBorn = u.DateBorn.ToShortDateString(),
                phoneNumber = u.PhoneNumber,
                login = u.Login,
                password = u.Password,
                email = u.Email,
                startWorkDate = u.StartWorkDay,
                roleName = u.Role.ToString(),
                comment = u.Comment
            });
            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { status = "error", message = "User was not found" });
            }
            
            return Ok(new 
            { 
                user.Id, 
                user.FirstName, 
                user.LastName, 
                user.MiddleName, 
                user.Gender, 
                dateBorn = user.DateBorn.ToShortDateString(),
                user.PhoneNumber, 
                user.Login,
                user.Password,
                user.Email,
                startWorkDay = user.StartWorkDay.ToShortDateString(),
                user.Role,
                user.Comment,
                user.ImageURL
            });
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser([FromRoute] int id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                return BadRequest(new { status = "error", message = "User id is not equal to id from route" });
            }
            var loginExist = _context.Users.FirstOrDefault(u => u.Login == user.Login && u.Id != user.Id);
            if (loginExist != null)
            {
                return BadRequest(new { status = "error", message = "Пользователь с таким логином уже существует" });
            }
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound(new { status = "error", message = "User was not found" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { status = "success", message = "Changes was add" });
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] UserModel model)
        {
            var user = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                MiddleName = model.MiddleName,
                Email = model.Email,
                Gender = model.Gender,
                Login = model.Login,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber,
                Role = model.Role,
                DateBorn = model.DateBorn,
                StartWorkDay = model.StartWorkDay,
                Comment = model.Comment,
                Status = EmployeeStatus.Active,
                ImageURL = model.ImageURL
            };
            var userExist = _context.Users.FirstOrDefault(u => u.Login == user.Login);
            if (userExist != null)
            {
                return BadRequest(new { status = "error", message = "Пользователь с таким логином уже существует" });
            }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [Route("DismissUser/{id}")]
        [HttpPut]
        public async Task<IActionResult> DismissUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { status = "error", message = "User was not found" });
            }

            user.Status = EmployeeStatus.NotActive;
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}