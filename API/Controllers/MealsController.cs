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
    public class MealsController : ControllerBase
    {
        private readonly EFDbContext _context;

        public MealsController(EFDbContext context)
        {
            _context = context;
        }

        // GET: api/Meals
        [HttpGet]
        public IActionResult GetMeals()
        {
            var meals = _context.Meals.Select(m => new
            {
                id = m.Id,
                name = m.Name,
                categoryId = m.CategoryId,
                categoryName = m.Category.Name,
                description = m.Description,
                price = m.Price,
                weight = m.Weight,
                imageURL = m.ImageURL
            });
            return Ok(meals);
        }

        // GET: api/Meals/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeal([FromRoute] int id)
        {
            var meal = await _context.Meals.FindAsync(id);

            if (meal == null)
            {
                return NotFound(new { status = "error", message = "Meal was not found"});
            }

            return Ok(meal);
        }

        // PUT: api/Meals/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMeal([FromRoute] int id, [FromBody] Meal meal)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != meal.Id)
            {
                return BadRequest(new { status = "error", message = "Meal id is not equal to id from route" });
            }

            var mealExists = _context.Meals.FirstOrDefault(m => m.Name == meal.Name && m.Weight == meal.Weight && m.Id != meal.Id);
            if (mealExists != null)
            {
                return BadRequest(new { status = "error", message = "Блюдо с таким названием и описанем уже существует" });
            }

            _context.Entry(meal).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MealExists(id))
                {
                    return NotFound(new { status = "error", message = "Meal was not found" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { status = "success", message = "Changes was add" });
        }

        // POST: api/Meals
        [HttpPost]
        public async Task<IActionResult> PostMeal([FromBody] MealModel model)
        {
            var meal = new Meal()
            {
                CategoryId = model.CategoryId,
                Name = model.Name,
                Price = model.Price,
                ImageURL = model.ImageURL,
                Weight = model.Weight,
                Description = model.Description
            };
            var mealExists = _context.Meals.FirstOrDefault(m => m.Name == meal.Name && m.Weight == meal.Weight);
            if (mealExists != null)
            {
                return BadRequest(new { status = "error", message = "Блюдо с таким названием и описанем уже существует" });
            }
            _context.Meals.Add(meal);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMeal", new { id = meal.Id }, meal);
        }

        // DELETE: api/Meals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeal([FromRoute] int id)
        {
            var meal = await _context.Meals.FindAsync(id);
            if (meal == null)
            {
                return NotFound(new { status = "error", message = "Meal was not found"});
            }

            _context.Meals.Remove(meal);
            await _context.SaveChangesAsync();

            return Ok(meal);
        }

        private bool MealExists(int id)
        {
            return _context.Meals.Any(e => e.Id == id);
        }
    }
}