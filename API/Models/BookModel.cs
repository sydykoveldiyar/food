using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class BookModel
    {
        public int TableId { get; set; }
        [Required(ErrorMessage = "Заполните дату")]
        public DateTime BookDate { get; set; }
        [Required(ErrorMessage = "Укажите количество человек")]
        public int MenQuantity { get; set; }
        [Required(ErrorMessage = "Укажите имя клиента")]
        public string ClientName { get; set; }
        [Required(ErrorMessage = "Заполните номер телефона в правильном формате")]
        [RegularExpression(@"^\(?\+([9]{2}?[6])\)?[-. ]?([0-9]{3})[-. ]?([0-9]{3})[-. ]?([0-9]{3})$", ErrorMessage = "Некорректный номер телефона")]
        public string PhoneNumber { get; set; }
    }
}
