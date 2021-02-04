using DataTier.Entities.Abstract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class MealOrder
    {
        [JsonIgnore]
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
        public int MealId { get; set; }
        [JsonIgnore]
        public Meal Meal { get; set; }
        [Required(ErrorMessage = "Укажите количество порций")]
        public int OrderedQuantity { get; set; }
        [JsonIgnore]
        public int FinishedQuantity { get; set; }
        [JsonIgnore]
        public MealOrderStatus MealOrderStatus { get; set; }
    }
}
