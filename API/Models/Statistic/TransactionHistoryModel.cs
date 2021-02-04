using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.Statistic
{
    public class TransactionHistoryModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string WaiterName { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public int TableId { get; set; }
        public string Comment { get; set; }
        public ICollection<MealOrder> MealOrders { get; set; }
    }
}
