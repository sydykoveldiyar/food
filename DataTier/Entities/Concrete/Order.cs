using DataTier.Entities.Abstract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Order
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Укажите Id официанта")]
        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        [Required(ErrorMessage = "Укажите Id стола")]
        public int TableId { get; set; }
        [JsonIgnore]
        public Table Table { get; set; }
        [JsonIgnore]
        public DateTime DateTimeOrdered { get; set; }
        [JsonIgnore]
        public DateTime? DateTimeClosed { get; set; }
        [JsonIgnore]
        public OrderStatus OrderStatus { get; set; }
        [JsonIgnore]
        public decimal TotalPrice { get; set; }
        public string Comment { get; set; }
        public ICollection<MealOrder> MealOrders { get; set; }
        public Order()
        {
            MealOrders = new List<MealOrder>();
        }
    }
}
