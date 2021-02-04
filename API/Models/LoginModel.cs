using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace API.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Укажите логин")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Укажите пароль")]
        public string Password { get; set; }
    }
}
