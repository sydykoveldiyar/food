using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class DateRange
    {
        [Required(ErrorMessage = "Укажите начальную дату")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "Укажите конечную дату")]
        public DateTime EndDate { get; set; }
    }
}
