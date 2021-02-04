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
    public class CategoriesController : ControllerBase
    {
        private readonly EFDbContext _context;

        public CategoriesController(EFDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "cook, admin")]
        [HttpGet]
        public IQueryable GetCategories()
        {
            var categories = _context.Categories.Select(c => new
            {
                id = c.Id,
                departmentId = c.Department,
                departmentName = c.Department.ToString(),
                category = c.Name,
                image = c.ImageURL
            });
            return categories;
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory([FromRoute] int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new { status = "error", message = "Category was not found"});
            }

            return Ok(category);
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory([FromRoute] int id, [FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != category.Id)
            {
                return BadRequest(new { status = "error", message = "Category id is not equal to id from route" });
            }

            var categoryExist = _context.Categories.FirstOrDefault(c => c.Name == category.Name && c.Id != category.Id);
            if (categoryExist != null)
            {
                return BadRequest(new { status = "error", message = "Категория с таким названием уже существует" });
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound(new { status = "error", message = "Category was not found" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { status = "success", message = "Changes was add" });
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<IActionResult> PostCategory([FromBody] CategoryModel model)
        {
            var category = new Category()
            {
                Name = model.Name,
                Department = model.Department,
                ImageURL = model.ImageURL
            };
            var categoryExist = _context.Categories.FirstOrDefault(c => c.Name == category.Name);
            if (categoryExist != null)
            {
                return BadRequest(new { status = "error", message = "Категория с таким названием уже существует"});
            }
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { status = "error", message = "Category was not found"});
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}