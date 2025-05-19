using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSMachine.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int UserId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal CGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Change { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
