using DataTier.Entities.Abstract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Meal
    { 
        public int Id { get; set; }
        public int CategoryId { get; set; }
        [JsonIgnore]
        public Category Category { get; set; }
        [Required(ErrorMessage = "Заполните название")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Заполните описание")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Заполните цену")]
        public decimal Price { get; set; }
        public string Weight { get; set; }
        public MealStatus MealStatus { get; set; }
        public string ImageURL { get; set; }

        public decimal CostPrice { get; set; }

        [JsonIgnore]
        public ICollection<MealOrder> MealOrders { get; set; }
        public Meal()
        {
            MealOrders = new List<MealOrder>();
        }
    }
}
