using DataTier.Entities.Abstract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Table
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заполните имя")]
        public string Name { get; set; }

        public TableStatus Status { get; set; }
        [JsonIgnore]
        public ICollection<Order> Orders { get; set; }
        public Table()
        {
            Orders = new List<Order>();
        }
    }
}
