using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTier.Entities.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.ControllerWork
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TopController : ControllerBase
    {
        private EFDbContext _context;
        public TopController(EFDbContext context)
        {
            _context = context;
        }

        [Route("topMeals")]
        [HttpGet]
        public IActionResult SalesByMeal()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.MealId == m.Id)
                    .Select(mo => mo.FinishedQuantity)
                    .Sum()
                })
                .Take(10)
                .OrderByDescending(m => m.count);
            return Ok(meals);
        }

        #region Топ блюд по сезонам

        [Route("topMealsWinter")]
        [HttpGet]
        public IActionResult SalesByMealWinter()
        {
            if (DateTime.IsLeapYear(DateTime.Now.Year))
            {
                var leapYearMeals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 29))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
                return Ok(leapYearMeals);
            }

            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 28))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topMealsSpring")]
        [HttpGet]
        public IActionResult SalesByMealSpring()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 3, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 5, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        [Route("topMealsSummer")]
        [HttpGet]
        public IActionResult SalesByMealSummer()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 6, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 9, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topMealsAutumn")]
        [HttpGet]
        public IActionResult SalesByMealAutumn()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 10, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 11, 30))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        #endregion

        [Route("topDrinks")]
        [HttpGet]
        public IActionResult SalesByDrink()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.MealId == m.Id)
                    .Select(mo => mo.FinishedQuantity)
                    .Sum()
                })
                .Take(10)
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        #region Топ алкоголя по сезонам

        [Route("topDrinksWinter")]
        [HttpGet]
        public IActionResult SalesByMealDrinks()
        {
            if (DateTime.IsLeapYear(DateTime.Now.Year))
            {
                var leapYearMeals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 29))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
                return Ok(leapYearMeals);
            }

            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 28))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topDrinksSpring")]
        [HttpGet]
        public IActionResult SalesByDrinksSpring()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 3, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 5, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        [Route("topDrinksSummer")]
        [HttpGet]
        public IActionResult SalesByDrinksSummer()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 6, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 9, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topDrinksAutumn")]
        [HttpGet]
        public IActionResult SalesByDrinksAutumn()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 10, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 11, 30))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        #endregion

        [Route("waiterOrderTop")]
        [HttpGet]
        public IActionResult WaiterTop()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            if (users == null)
            {
                return BadRequest(new { status = "error", message = "Have no waiters" });
            }
            var top = users.Select(u => new
            {
                id = u.Id,
                name = u.LastName + " " + u.FirstName,
                orderCount = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Count()
            })
            .OrderByDescending(u => u.orderCount);

            return Ok(top);
        }

        [Route("waiterSumTop")]
        [HttpGet]
        public IActionResult WaiterSumTop()
        {
            var waitersSumTop = _context.Users
                .Where(u => u.Role == Role.waiter)
                .Select(u => new
                {
                    id = u.Id,
                    userName = u.LastName + " " + u.FirstName,
                    sum = _context.Orders
                    .Where(o => o.UserId == u.Id)
                    .Where(o => o.OrderStatus == OrderStatus.NotActive)
                    .Select(o => o.TotalPrice)
                    .Sum()
                }).OrderBy(mo => mo.sum);
            return Ok(waitersSumTop);
        }

        [Route("topWaitersKitchenSums")]
        [HttpGet]
        public IActionResult TopWaitersKitchenTotalSums()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var topWaiters = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                sum = _context.MealOrders
                .Include(mo => mo.Order)
                .Include(mo => mo.Meal)
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Where(mo => mo.Order.UserId == u.Id)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity).Sum()
            }).OrderBy(u => u.sum);
            return Ok(topWaiters);
        }

        [Route("topWaitersBarSums")]
        [HttpGet]
        public IActionResult TopWaitersBarTotalSums()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var topWaiters = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                sum = _context.MealOrders
                .Include(mo => mo.Order)
                .Include(mo => mo.Meal)
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Where(mo => mo.Order.UserId == u.Id)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity).Sum()
            }).OrderBy(u => u.sum);
            return Ok(topWaiters);
        }

        [Route("topWaitersKitchenMeals")]
        [HttpGet]
        public IActionResult TopWaitersKitchenTotalMeals()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var topWaiters = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                meals = _context.MealOrders
                .Include(mo => mo.Order)
                .Include(mo => mo.Meal)
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Where(mo => mo.Order.UserId == u.Id)
                .Select(mo => mo.FinishedQuantity).Sum()
            }).OrderBy(u => u.meals);
            return Ok(topWaiters);
        }

        [Route("topWaitersBarMeals")]
        [HttpGet]
        public IActionResult TopWaitersBarTotalMeals()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var topWaiters = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                meals = _context.MealOrders
                .Include(mo => mo.Order)
                .Include(mo => mo.Meal)
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Where(mo => mo.Order.UserId == u.Id)
                .Select(mo => mo.FinishedQuantity).Sum()
            }).OrderBy(u => u.meals);
            return Ok(topWaiters);
        }
    }
}