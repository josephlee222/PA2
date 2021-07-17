using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PA2.Models
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }
        [DisplayName("Username")]
        public string CustomerUsername { get; set; }
        [DisplayName("Password")]
        public string CustomerPassword { get; set; }
        [DisplayName("Admin Permissions")]
        public string CustomerAdmin { get; set; }
    }
}