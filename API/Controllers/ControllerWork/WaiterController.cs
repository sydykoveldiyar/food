using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Hubs;
using API.Models;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.ControllerWork
{
    [Authorize(Roles = "waiter")]
    [Route("api/[controller]")]
    [ApiController]
    public class WaiterController : ControllerBase
    {
        private readonly EFDbContext _context;
        private readonly IHubContext<FoodHub> _hubContext;
        public WaiterController(EFDbContext context, IHubContext<FoodHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Route("getActiveOrders")]
        [HttpGet]
        public IActionResult GetOrders()
        {
            var orders = _context.Orders
                .Where(o => o.UserId == GetUserId())
                .Where(o => o.OrderStatus == OrderStatus.Active || o.OrderStatus == OrderStatus.MealCooked || o.OrderStatus == OrderStatus.BarCooked)
                .Select(o => new
                {
                    id = o.Id,
                    tableName = o.Table.Name,
                    sum = o.MealOrders.Select(mo => mo.Meal.Price * mo.OrderedQuantity).Sum(),
                    mealOrders = o.MealOrders.Select(mo => new
                    {
                        meal = mo.Meal.Name,
                        mo.OrderedQuantity,
                        status = mo.MealOrderStatus.ToString()
                    }),
                    
                });
            return Ok(orders);
        }

        [Route("getFinishedOrders")]
        [HttpGet]
        public IActionResult GetFinishedOrders()
        {
            var orders = _context.Orders
                .Where(o => o.UserId == GetUserId())
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => new
                {
                    id = o.Id,
                    tableId = o.TableId,
                    tableName = o.Table.Name,
                    mealOrders = o.MealOrders.Select(mo => new
                    {
                        mealId = mo.MealId
                    })
                });
            return Ok(orders);
        }

        [HttpGet("getOrder/{id}")]
        public async Task<IActionResult> GetOrder([FromRoute] int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
               return NotFound(new { status = "error", message = "Order was not found"});
            }
            return Ok(order);
        }

        [Route("getTables")]
        [HttpGet]
        public IActionResult GetTables()
        {
            var tables = _context.Tables
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    status = t.Status
                });
            return Ok(tables);
        }

        [Route("getFreeTables")]
        [HttpGet]
        public IActionResult GetFreeTables()
        {
            var tables = _context.Tables
                .Where(t => t.Status == TableStatus.Free)
                .Select(t => new { id = t.Id, name = t.Name});
            return Ok(tables);
        }

        [Route("getKitchenMenu")]
        [HttpGet]
        public IActionResult GetKitchen()
        {
            var menu = _context.Categories.Where(c => c.Department == Department.Kitchen)
                .Select(c => new
                {
                    catetegoryId = c.Id,
                    category = c.Name,
                    image = c.ImageURL,
                    departmentId = c.Department,
                    departmentName = c.Department.ToString(),
                    meals = c.Meals.Select(m => new
                    {
                        mealId = m.Id,
                        mealName = m.Name,
                        mealWeight = m.Weight,
                        mealStatus = m.MealStatus,
                        price = m.Price,
                    })
                });
            return Ok(menu);
        }

        [Route("getBarMenu")]
        [HttpGet]
        public IActionResult GetBar()
        {
            var menu = _context.Categories.Where(c => c.Department == Department.Bar)
                .Select(c => new
                {
                    categoryId = c.Id,
                    category = c.Name,
                    image = c.ImageURL,
                    departmentId = c.Department,
                    departmentName = c.Department.ToString(),
                    meals = c.Meals.Select(m => new
                    {
                        mealId = m.Id,
                        mealName = m.Name,
                        mealWeight = m.Weight,
                        mealStatus = m.MealStatus,
                        price = m.Price,
                    })
                });
            return Ok(menu);
        }

        [Route("GetWaiterStatistics")]
        [HttpGet]
        public IActionResult GetWaiterStatistics()
        {
            var statisctics = _context.Users
                .Where(u => u.Id == GetUserId())
                .Select(u => new
            {
                orderCount = u.Orders.Count(),

                totalSum = u.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice)
                .Sum()
            });

            var statiscticsToday = _context.Users
                .Where(u => u.Id == GetUserId())
                .Select(u => new
                {
                    orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.Today)
                .Count(),

                    totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Today)
                .Select(o => o.TotalPrice).Sum()
                });

            var statiscticsWeek = _context.Users
                .Where(u => u.Id == GetUserId())
                .Select(u => new
                {
                    orderCount = u.Orders
                    .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-7))
                    .Count(),

                    totalSum = u.Orders
                    .Where(o => o.OrderStatus == OrderStatus.NotActive)
                    .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-7))
                    .Select(o => o.TotalPrice).Sum()
                });

            var statiscticsMonth = _context.Users
                .Where(u => u.Id == GetUserId())
                .Select(u => new
                {
                    orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Count(),

                    totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Select(o => o.TotalPrice).Sum()
                });

            return Ok(new { statisctics, statiscticsMonth, statiscticsWeek, statiscticsToday});
        }

        [Route("getMeals")]
        [HttpPost]
        public IActionResult GetMeals([FromBody] GetMealModel model)
        {
            var meals = _context.Meals
                .Where(m => m.CategoryId == model.CategoryId)
                .Select(m => new 
            {
                m.Id,
                m.Name,
                m.Weight,
                m.Price,
                m.Description,
                m.MealStatus
            });
            return Ok(meals);
        }

        [Route("createOrder")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderModel model)
        {
            if (model.MealOrders.Count == 0)
            {
                return BadRequest(new { status = "error", message = "Список блюд не может быть пустым" });
            }
            if (model.MealOrders.FirstOrDefault(mo => mo.OrderedQuantity == 0) != null)
            {
                return BadRequest(new { status = "error", message = "Количество порций не может быть равным нулю" });
            }
            if (TableIsNull(model.TableId))
            {
                return NotFound(new { status = "error", message = "Table is not exists in DB"});
            }
            var order = new Order()
            {
                UserId = GetUserId(),
                TableId = model.TableId,
                DateTimeOrdered = DateTime.Now,
                Comment = model.Comment,
                MealOrders = model.MealOrders
            };
            using (var transaction = _context.Database.BeginTransaction())
            {
                order.DateTimeOrdered = DateTime.Now;
                order.OrderStatus = OrderStatus.Active;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                var ord = _context.Orders.Include(o => o.Table).FirstOrDefault(o => o.Id == order.Id);
                foreach (var item in ord.MealOrders)
                {
                    item.FinishedQuantity = 0;
                }
                if (ord.Table.Status == TableStatus.Busy || ord.Table.Status == TableStatus.Booked)
                {
                    return BadRequest(new { status = "error", message = "Стол занят или забронирован" });
                }
                ord.Table.Status = TableStatus.Busy;
                await _context.SaveChangesAsync();
                transaction.Commit();
            }
            return Ok(new { status = "success", message = "Заказ был создан"});
        }

        [Route("addMealsOrder")]
        [HttpPost]
        public async Task<IActionResult> AddMealToOrder([FromBody] AddMealOrderModel model)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == model.OrderId);
            if (order == null)
            {
                return NotFound(new { status = "error", message = "Заказ не был найден" });
            }
            if (GetUserId() != order.UserId)
            {
                return BadRequest(new { staus = "error", message = "Вы можете добавлять блюда только в свои заказы"});
            }
            foreach (var item in model.MealOrders)
            {
                if (MealIsNull(item.MealId))
                {
                    return NotFound(new { status = "error", message = "Блюдо не было найдено в базе данных" });
                }
                var mealOrder = _context.MealOrders.FirstOrDefault(mo => mo.OrderId == order.Id && mo.MealId == item.MealId);
                if (mealOrder == null)
                {
                    var mo = new MealOrder
                    {
                        OrderId = order.Id,
                        MealId = item.MealId,
                        OrderedQuantity = item.AddQuantity,
                        FinishedQuantity = 0,
                        MealOrderStatus = MealOrderStatus.NotReady
                    };
                    _context.MealOrders.Add(mo);
                }
                else
                {
                    mealOrder.OrderedQuantity += item.AddQuantity;
                    mealOrder.MealOrderStatus = MealOrderStatus.NotReady;
                }
            }
            order.DateTimeOrdered = DateTime.Now;
            order.OrderStatus = OrderStatus.Active;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Блюда были добавлены в заказ" });
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
            if (model.MealOrders.Count == 0)
            {
                return BadRequest(new { status = "error", message = "Список блюд не может быть пустым"});
            }
            if (GetUserId() != order.UserId)
            {
                return BadRequest(new { status = "error", message = "Вы можете удалять блюда только из своего заказа" });
            }
            foreach (var item in model.MealOrders)
            {
                if (MealIsNull(item.MealId))
                {
                    return NotFound(new { status = "error", message = "Блюда не были найдены в базе данных" });
                }
                var mealOrder = _context.MealOrders.FirstOrDefault(mo => mo.OrderId == order.Id && mo.MealId == item.MealId);
                if (mealOrder == null)
                {
                    return NotFound(new { status = "error", message = $"Блюдо {item.MealId} в заказе с id {order.Id} не существует" });
                }
                if (mealOrder.MealOrderStatus == MealOrderStatus.NotReady)
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
                        return BadRequest(new { status = "error", message = "Количество удаляемых порций не может быть выше чем количество заказанных" });
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
                    return BadRequest(new { status = "error", message = "Вы не можете удалять порции из готовых блюд или замороженных" });
                }
            }
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Блюда были удалены из заказа" });
        }

        [Route("deleteFreezedMeals")]
        [HttpPost]
        public async Task<IActionResult> DeleteFreezedMeals([FromBody] DeleteFreezedMealModel model)
        {
            var mealOrder = _context.MealOrders.FirstOrDefault(mo => mo.OrderId == model.OrderId && mo.MealId == model.MealId);
            if (mealOrder == null)
            {
                return NotFound(new { status = "error", message = "Order was not found" });
            }
            if (GetUserId() != mealOrder.Order.UserId)
            {
                return BadRequest(new { staus = "error", message = "You can add meals only to own orders" });
            }
            if (mealOrder.MealOrderStatus == MealOrderStatus.NotReady || mealOrder.MealOrderStatus == MealOrderStatus.Ready)
            {
                return BadRequest(new { status = "error", message = "Meals can't be not ready or ready"});
            }
            mealOrder.OrderedQuantity = mealOrder.FinishedQuantity;
            mealOrder.MealOrderStatus = MealOrderStatus.Ready;
            _context.Entry(mealOrder).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Meals was deleted from order" });
        }

        [Route("closeCheque")]
        [HttpPost]
        public async Task<IActionResult> CloseCheque([FromBody] ChequeModel model)
        {
            var order = await _context.Orders.Include(o => o.Table).FirstOrDefaultAsync(o => o.Id == model.OrderId);
            if (order == null)
            {
                return NotFound(new { status = "error", message = "Order was not found" });
            }
            if (GetUserId() != order.UserId)
            {
                return BadRequest(new { staus = "error", message = "You can only close own orders" });
            }
            if (order.OrderStatus == OrderStatus.Active || order.OrderStatus == OrderStatus.MealCooked || order.OrderStatus == OrderStatus.BarCooked)
            {
                var mealOrders = _context.MealOrders.Where(mo => mo.OrderId == model.OrderId).Select(mo => new
                {
                    meal = mo.Meal,
                    quantity = mo.FinishedQuantity
                });
                decimal sum = 0;
                foreach (var item in mealOrders)
                {
                    var itemPrice = item.meal.Price;
                    var itemQuantity = item.quantity;
                    var itemTotalPrice = itemPrice * itemQuantity;
                    sum += itemTotalPrice;
                }
                order.TotalPrice = sum;
                order.DateTimeClosed = DateTime.Now;
                order.Table.Status = TableStatus.Free;
                order.OrderStatus = OrderStatus.NotActive;
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return CreatedAtAction("getOrder", new { id = order.Id }, order);
            }
            return BadRequest(new { status = "error", message = "Order is not active" });
        }

        [Route("GetWaiterStatisticsRange")]
        [HttpPost]
        public IActionResult GetWaiterStatisticsRange([FromBody] DateRange model)
        {
            if (DateIsNull(model))
            {
                return BadRequest(new { status = "error", message = "Date model is not valid" });
            }
            var statisctics = _context.Users
                //.Where(u => u.Id == GetUserId())
                .Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeOrdered >= model.StartDate && o.DateTimeClosed <= o.DateTimeClosed)
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeOrdered >= model.StartDate && o.DateTimeClosed <= model.EndDate)
                .Select(o => o.TotalPrice)
                .Sum()
            });
            return Ok(statisctics);
        }
        private int GetUserId()
        {
            return int.Parse(User.Claims.First(i => i.Type == "UserId").Value);
        }
        private bool DateIsNull(DateRange dateRange)
        {
            if (dateRange.StartDate <= DateTime.Parse("01.01.0001 0:00:00") || dateRange.EndDate <= DateTime.Parse("01.01.0001 0:00:00"))
            {
                return true;
            }
            return false;
        }
        private bool TableIsNull(int id)
        {
            var table = _context.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null)
            {
                return true;
            }
            return false;
        }
        private bool MealIsNull(int id)
        {
            var meal = _context.Meals.FirstOrDefault(m => m.Id == id);
            if (meal == null)
            {
                return true;
            }
            return false;
        }
    }
}