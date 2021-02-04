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
    public class TablesController : ControllerBase
    {
        private readonly EFDbContext _context;

        public TablesController(EFDbContext context)
        {
            _context = context;
        }

        // GET: api/Tables
        [HttpGet]
        public IActionResult GetTables()
        {
            var tables = _context.Tables.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                status = t.Status.ToString()
            });
            return Ok(tables);
        }

        // GET: api/Tables/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTable([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var table = await _context.Tables.FindAsync(id);

            if (table == null)
            {
                return NotFound(new { status = "error", message = "Table was not found"});
            }

            return Ok(table);
        }

        // PUT: api/Tables/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTable([FromRoute] int id, [FromBody] Table table)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != table.Id)
            {
                return BadRequest(new { status = "error", message = "Table id is not equal to id from route" });
            }

            var tableExists = _context.Tables.FirstOrDefault(t => t.Name == table.Name && t.Id != table.Id);
            if (tableExists != null)
            {
                return BadRequest(new { status = "error", message = "Стол с таким названием уже существует" });
            }

            _context.Entry(table).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TableExists(id))
                {
                    return NotFound(new { status = "error", message = "Table was not found" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { status = "success", message = "Changes was add" });
        }

        // POST: api/Tables
        [HttpPost]
        public async Task<IActionResult> PostTable([FromBody] TableModel model)
        {
            var table = new Table()
            {
                Name = model.Name
            };
            var tableExists = _context.Tables.FirstOrDefault(t => t.Name == table.Name);
            if (tableExists != null)
            {
                return BadRequest(new { status = "error", message = "Стол с таким названием уже существует" });
            }
            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTable", new { id = table.Id }, table);
        }

        // DELETE: api/Tables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound(new { status = "error", message = "Table was not found" });
            }

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return Ok(table);
        }

        private bool TableExists(int id)
        {
            return _context.Tables.Any(e => e.Id == id);
        }
    }
}