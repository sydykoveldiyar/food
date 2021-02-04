using DataTier.Entities.Abstract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class CategoryModel
    {
        [Required(ErrorMessage = "Заполните название")]
        public string Name { get; set; }
        public Department Department { get; set; }
        [Required(ErrorMessage = "Укажите ссылку на картинку")]
        public string ImageURL { get; set; }
    }
}
