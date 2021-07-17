using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PA2.Models
{
    [Table("Orders")]
    public class Orders
    {
        [Key]
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        [DataType(DataType.MultilineText)]
        [DisplayName("Description")]
        public string OrderDescription { get; set; }
        [DisplayName("Status")]
        public string OrderStatus { get; set; }
        [DisplayName("Delivery Address")]
        public string DeliveryAddress { get; set; }
        [DisplayName("Delivery Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DeliveryDate { get; set; }
        [DisplayName("Delivery Time")]
        public TimeSpan DeliveryTime { get; set; }
        [DisplayName("Contact")]
        public int DeliveryContact { get; set; }
        [ForeignKey("CustomerID")]
        public virtual Customer Customer { get; set; }
    }
}