using CmsShop.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsShop.Models.ViewModels.Shop
{
    public class ReviewVM
    {
        public ReviewVM() { }

        public ReviewVM(ReviewDTO row) {

            Id = row.Id;
            Review = row.Review;
            UserId = row.UserId;
            CreateAt = row.CreateAt;
            UserName = row.UserName;
            ProductId=row.ProductId;
        }

        public int Id { get; set; }
        [Display(Name = "Wystawił")]
        public string UserName { get; set; }
        public int UserId { get; set; }
        [Required]
        [Display(Name = "Opinia")]
        public string Review { get; set; }
        [Display(Name = "Data wystawienia")]
        public DateTime CreateAt { get; set; }
    
        public int ProductId { get; set; }
    }
}