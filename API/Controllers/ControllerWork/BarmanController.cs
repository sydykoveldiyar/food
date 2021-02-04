using API.Hubs;
using API.Models;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers.ControllerWork
{
    [Authorize(Roles = "barman")]
    [Route("api/[controller]")]
    [ApiController]
    public class BarmanController : ControllerBase
    {
        private readonly EFDbContext _context;
        private readonly IHubContext<FoodHub> _hubContext;
        public BarmanController(EFDbContext context, IHubContext<FoodHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Route("getMeals")]
        [HttpGet]
        public IActionResult GetMeals()
        {
            var meals = _context.Meals.Select(m => new
            {
                id = m.Id,
                name = m.Name,
                description = m.Description,
                department = m.Category.Department.ToString(),
                category = m.Category.Name,
                price = m.Price,
                weight = m.Weight,
                status = m.MealStatus.ToString(),
                image = m.ImageURL
            });
            return Ok(meals);
        }

        [Route("getActiveOrders")]
        [HttpGet]
        public IActionResult ActiveOrders()
        {
            var orders = _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Active || o.OrderStatus == OrderStatus.MealCooked)
                .Select(o => new
                {
                    orderId = o.Id,
                    dateTimeOrdered = o.DateTimeOrdered,
                    comment = o.Comment,
                    mealsList = o.MealOrders.Select(mo => new
                    {
                        departmentName = mo.Meal.Category.Department.ToString(),
                        mealId = mo.MealId,
                        mealName = mo.Meal.Name,
                        orderedQuantity = mo.OrderedQuantity,
                        finishedQuantity = mo.FinishedQuantity,
                        statusId = mo.MealOrderStatus,
                        status = mo.MealOrderStatus.ToString()
                    }),
                    orderStatus = o.OrderStatus.ToString()
                }).OrderBy(o => o.dateTimeOrdered);
            return Ok(orders);
        }

        [Route("finishMeal")]
        [HttpPost]
        public async Task<IActionResult> EndMeal([FromBody] MealReadyModel model)
        {
            var mealOrder = _context.MealOrders
                .Include(mo => mo.Order)
                .Include(mo => mo.Meal)
                .Include(mo => mo.Meal.Category)
                .FirstOrDefault(mo => mo.OrderId == model.OrderId && mo.MealId == model.MealId);

            if (mealOrder == null)
            {
                return NotFound(new { status = "error", message = "Заказ или блюдо не было найдено" });
            }
            if (mealOrder.Meal.Category.Department != Department.Bar)
            {
                return BadRequest(new { status = "error", message = "Вы можете завершать блюда только своего департамента" });
            }
            int finishedQuantity = mealOrder.FinishedQuantity + model.FinishedQuantity;
            if (mealOrder.OrderedQuantity < finishedQuantity)
            {
                return BadRequest(new { status = "error", message = "Количество законченных порций не может быть выше чем количество заказанных" });
            }
            else if (mealOrder.OrderedQuantity > finishedQuantity)
            {
                mealOrder.FinishedQuantity = finishedQuantity;
            }
            else
            {
                mealOrder.FinishedQuantity = finishedQuantity;
                mealOrder.MealOrderStatus = MealOrderStatus.Ready;
            }

            await _context.SaveChangesAsync();
            //string message = $"Стол: {mealOrder.Order.Table.Name} блюдо {mealOrder.Meal.Name} готово";
            //await _hubContext.Clients.User(mealOrder.Order.UserId.ToString()).SendAsync($"Notify", message);

            return Ok(new { status = "success", message = "Блюда успешно были завершены" });
        }

        [Route("freezeMeal")]
        [HttpPost]
        public async Task<IActionResult> FreezeMeal(FreezeMealModel model)
        {
            var mealOrder = _context.MealOrders
                .Include(mo => mo.Order)
                .Include(mo => mo.Meal)
                .FirstOrDefault(mo => mo.OrderId == model.OrderId && mo.MealId == model.MealId);
            if (mealOrder == null)
            {
                return NotFound(new { status = "error", message = "Блюдо или заказ не были найдены" });
            }
            if (mealOrder.Meal.Category.Department != Department.Bar)
            {
                return BadRequest(new { status = "error", message = "Блюда должны быть вашего департамента" });
            }
            if (mealOrder.MealOrderStatus != MealOrderStatus.Ready && mealOrder.MealOrderStatus != MealOrderStatus.Freezed)
            {
                mealOrder.MealOrderStatus = MealOrderStatus.Freezed;
                var meal = _context.Meals.FirstOrDefault(m => m.Id == model.MealId);
                meal.MealStatus = MealStatus.HaveNot;
                await _context.SaveChangesAsync();
                //string message =
                //    $"Не хватает ингредиентов на {model.FreezedMeals} порций " +
                //    $"Стол: {mealOrder.Order.Table.Name} " +
                //    $"Блюдо: {mealOrder.Meal.Name}";
                //await _hubContext.Clients.User(mealOrder.Order.UserId.ToString()).SendAsync($"Notify", message);
                return Ok(new { status = "success", message = "Блюда успешно заморожены" });
            }
            return BadRequest(new { status = "error", message = "Блюда готовы, либо уже заморожены" });
        }

        [HttpPut("closeOrder/{id}")]
        public async Task<IActionResult> CloseOrder([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound(new { status = 404, message = "Заказ не был найден" });
            }
            order.OrderStatus = OrderStatus.BarCooked;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Заказ был закрыт" });
        }

        [Route("changeMealStatus/{id}")]
        [HttpPut]
        public async Task<IActionResult> ChangeMealStatus([FromRoute] int id)
        {
            var meal = await _context.Meals.Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (meal == null)
            {
                return NotFound(new { status = "error", message = "Напиток не был найден" });
            }
            if (meal.Category.Department != Department.Bar)
            {
                return BadRequest(new { status = "error", message = "Бармен не может менять статус блюд кухни"});
            }
            if (meal.MealStatus == MealStatus.Have)
            {
                meal.MealStatus = MealStatus.HaveNot;
                await _context.SaveChangesAsync();

                //string message = $"Ингредиентов для блюда {meal.Name} не осталось в наличии";
                //var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(User.Claims.FirstOrDefault(us => us.Type == "UserId").Value));
                //if (user.Role == Role.admin || user.Role == Role.cook)
                //{
                //    await _hubContext.Clients.User(user.Id.ToString()).SendAsync($"Notify", message);
                //}
                return Ok(meal);
            }
            else
            {
                meal.MealStatus = MealStatus.Have;
                await _context.SaveChangesAsync();

                //string message = $"Ингредиенты для блюда {meal.Name} появились в наличии";
                //var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(GetUserId()));
                //if (user.Role == Role.admin || user.Role == Role.cook)
                //{
                //    await _hubContext.Clients.User(user.Id.ToString()).SendAsync($"Notify", message);
                //}
                return Ok(meal);
            }
        }
        private int GetUserId()
        {
            return int.Parse(User.Claims.First(i => i.Type == "UserId").Value);
        }
    }
}