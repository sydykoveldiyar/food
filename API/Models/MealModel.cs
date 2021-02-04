using DataTier.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class MealModel
    {
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Заполните название")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Заполните описание")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Укажите цену")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Укажите вес или количество")]
        public string Weight { get; set; }
        [Required(ErrorMessage = "Укажите ссылку на картинку")]
        public string ImageURL { get; set; }
    }
}
