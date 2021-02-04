using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using DataTier.Entities.Abstract;
using Newtonsoft.Json;
namespace DataTier.Entities.Concrete
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Заполните название")]
        public string Name { get; set; }
        public Department Department { get; set; }
        [JsonIgnore]
        public ICollection<Meal> Meals { get; set; }
        public string ImageURL { get; set; }
        public Category()
        {
            Meals = new List<Meal>();
        }
    }
}
