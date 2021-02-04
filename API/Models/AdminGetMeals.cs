using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class AdminGetMeals
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Department DepartmentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Weight { get; set; }
        public string MealStatus { get; set; }
        public string ImageURL { get; set; }
    }
}
