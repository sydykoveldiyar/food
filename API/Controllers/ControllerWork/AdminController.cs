using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Hubs;
using API.Models;
using API.Week;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Controllers.ControllerWork
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly EFDbContext _context;
        private readonly IHubContext<FoodHub> _hubContext;
        public AdminController(EFDbContext context, IHubContext<FoodHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Route("getMeals")]
        [HttpGet]
        public IActionResult GetMeals([FromQuery] PaginationModel model)
        {
            var source = (from meal in _context.Meals select meal).AsQueryable().Select(m => new
            {
                id = m.Id,
                name = m.Name,
                description = m.Description,
                DepartmentId = m.Category.Department,
                categoryId = m.CategoryId,
                categoryName = m.Category.Name,
                price = m.Price,
                weight = m.Weight,
                mealStatus = m.MealStatus.ToString(),
                imageURL = m.ImageURL
            });

            int count = source.Count();

            int CurrentPage = model.PageNumber;

            int PageSize = model.PageSize;

            int TotalCount = count;

            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            var items = source.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

            var previousPage = CurrentPage > 1 ? "Yes" : "No";

            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize = PageSize,
                currentPage = CurrentPage,
                totalPages = TotalPages,
                previousPage,
                nextPage
            };

            HttpContext.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));

            return Ok(items);
        }

        [Route("getWaiters")]
        [HttpGet]
        public IActionResult GetWaiters()
        {
            var waiters = _context.Users.Where(u => u.Role == Role.waiter).Select(u => new
            {
                u.Id,
                name = u.LastName + " " + u.FirstName + " " + u.MiddleName,
                u.Login,
                u.Password
            });
            return Ok(waiters);
        }

        [Route("getDismissed")]
        [HttpGet]
        public IActionResult GetDismissedWaiters()
        {
            var waiters = _context.Users.Where(u => u.Role == Role.waiter).Where(u => u.Status == EmployeeStatus.NotActive).Select(u => new
            {
                u.Id,
                name = u.LastName + " " + u.FirstName + " " + u.MiddleName,
                u.Login,
                u.Password
            });
            return Ok(waiters);
        }

        [Route("getBooks")]
        [HttpGet]
        public IActionResult GetBooks()
        {
            var books = _context.Books.Select(b => new 
            {
                b.Id,
                b.ClientName,
                b.BookDate,
                b.MenQuantity,
                b.TableId,
                b.PhoneNumber
            });
            return Ok(books);
        }

        [Route("bookTable")]
        [HttpPost]
        public async Task<IActionResult> BookTable([FromBody] BookModel model)
        {
            var table = _context.Tables.FirstOrDefault(t => t.Id == model.TableId);
            if (table == null)
            {
                return NotFound(new { status = "error", message = "Стол не был найден"});
            }
            var bookExists = _context.Books.FirstOrDefault(b => b.BookDate.AddMinutes(-30) <= model.BookDate && b.BookDate.AddMinutes(30) >= model.BookDate && b.TableId == model.TableId);
            if (bookExists != null)
            {
                return BadRequest(new { status = "error", message = "Уже существует бронь, примерно, на это время" });
            }
            if (model.BookDate <= DateTime.Now)
            {
                return BadRequest(new { status = "error", message = "Невозможно забронировать на прошлое время" });
            }
            var book = new Book()
            {
                TableId = model.TableId,
                ClientName = model.ClientName,
                BookDate = model.BookDate,
                MenQuantity = model.MenQuantity,
                PhoneNumber = model.PhoneNumber
            };
            _context.Books.Add(book);
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Бронь была добавлена!" });
        }

        [HttpDelete("deleteBook/{id}")]
        public async Task<IActionResult> DeleteBook([FromRoute] int id)
        {
            var book = _context.Books.Include(b => b.Table).FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound(new { status = "error", message = "Бронь не была найдена" });
            }
            if (book.Table.Status == TableStatus.Booked && book.BookDate <= DateTime.Now.AddMinutes(30))
            {
                book.Table.Status = TableStatus.Free;
            }
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Book is deleted" });
        }

        [Route("changeMealStatus/{id}")]
        [HttpPut]
        public async Task<IActionResult> ChangeMealStatus([FromRoute] int id)
        {
            var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Id == id);
            if (meal != null)
            {
                if (meal.MealStatus == MealStatus.Have)
                {
                    meal.MealStatus = MealStatus.HaveNot;
                    await _context.SaveChangesAsync();
                    return Ok(meal);
                }
                else if (meal.MealStatus == MealStatus.HaveNot)
                {
                    meal.MealStatus = MealStatus.Have;
                    await _context.SaveChangesAsync();
                    return Ok(meal);
                }
            }
            return NotFound(new { status = "error", message = "Блюдо или напиток не был найден"});
        }

        [Route("deleteMealsOrder")]
        [HttpPost]
        public async Task<IActionResult> DeleteMealFromOrder([FromBody] DeleteMealOrderModel model)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == model.OrderId);
            if (order == null)
            {
                return NotFound(new { status = "error", message = "Заказ не был найден" });
            }
            foreach (var item in model.MealOrders)
            {
                var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Id == item.MealId);
                if (meal == null)
                {
                    return NotFound(new { status = "error", message = "Блюдо не было найдено в базе данных" });
                }

                var mealOrder = _context.MealOrders.FirstOrDefault(mo => mo.OrderId == order.Id && mo.MealId == item.MealId);

                if (mealOrder == null)
                {
                    return NotFound(new { status = "error", message = $"Блюда с Id:{item.MealId} нет в заказе:{order.Id}" });
                }
                if (mealOrder.MealOrderStatus == MealOrderStatus.Ready)
                {
                    if (mealOrder.OrderedQuantity == item.DeleteQuantity)
                    {
                        _context.MealOrders.Remove(mealOrder);
                    }
                    else if (mealOrder.OrderedQuantity > item.DeleteQuantity)
                    {
                        mealOrder.OrderedQuantity -= item.DeleteQuantity;
                    }
                    else if (mealOrder.OrderedQuantity < item.DeleteQuantity)
                    {
                        return BadRequest(new { status = "error", message = "Количество удаляемых порций не может быть больше количества заказанных" });
                    }
                    if (mealOrder.OrderedQuantity <= mealOrder.FinishedQuantity)
                    {
                        mealOrder.MealOrderStatus = MealOrderStatus.Ready;
                    }
                    else
                    {
                        mealOrder.MealOrderStatus = MealOrderStatus.NotReady;
                    }
                }
                else
                {
                    return BadRequest(new { status = "error", message = "This method can't delete meal from not ready or freezed MealOrder" });
                }
            }
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Блюда были успешно удалены" });
        }
    }
}