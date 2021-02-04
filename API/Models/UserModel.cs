using DataTier.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class UserModel
    {
        [Required(ErrorMessage = "Заполните имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Заполните фамилию")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Укажите пол")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Укажите дату рождения")]
        public DateTime DateBorn { get; set; }

        [Required(ErrorMessage = "Заполните номер телефона")]
        [RegularExpression(@"^\(?\+([9]{2}?[6])\)?[-. ]?([0-9]{3})[-. ]?([0-9]{3})[-. ]?([0-9]{3})$", ErrorMessage = "Некорректный номер телефона")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Заполните логин")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Заполните пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Укажите email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public DateTime StartWorkDay { get; set; }
        [Required(ErrorMessage = "Укажите роль")]
        public Role Role { get; set; }

        public string Comment { get; set; }
        public string ImageURL { get; set; }

    }
}
