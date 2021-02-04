using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class TableModel
    {
        [Required(ErrorMessage = "Заполните название")]
        public string Name { get; set; }
    }
}
