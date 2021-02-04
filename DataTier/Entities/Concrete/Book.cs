using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Book
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public Table Table { get; set; }
        public DateTime BookDate { get; set; }
        public int MenQuantity { get; set; }
        public string ClientName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
