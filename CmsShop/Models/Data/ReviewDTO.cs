using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CmsShop.Models.Data
{
    [Table("tblReviews")]
    public class ReviewDTO
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }      
        public string Review { get; set; }
        public DateTime CreateAt { get; set; }
        public string UserName { get; set; }
        public int ProductId { get; set; }
       

        [ForeignKey("UserId")]
        public virtual UserDTO Users { get; set; }
        [ForeignKey("ProductId")]
        public virtual ProductDTO Products { get; set; }
    }
}