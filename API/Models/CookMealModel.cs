using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class MealReadyModel
    {
        public int OrderId { get; set; }
        public int MealId { get; set; }
        [Required(ErrorMessage = "Укажите количество законченных блюд")]
        public int FinishedQuantity { get; set; }
    }

    public class FreezeMealModel : CloseMealModel
    {
        [Required(ErrorMessage = "Укажите количество замоаживаемых блюд")]
        public int FreezedMeals { get; set; }
    }

    public class CloseMealModel
    {
        public int OrderId { get; set; }
        public int MealId { get; set; }
    }
}
