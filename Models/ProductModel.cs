using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NG_Core_Auth.Models
{
    public class ProductModel
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        [MaxLength(50)]
        public string ProductName { get; set; }
        [Required]
        [MaxLength(50)]
        public string ProductDescription { get; set; }
        [Required]
        public bool OutOfStock { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public double price { get; set; }

    }
}
