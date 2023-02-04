﻿using System.ComponentModel.DataAnnotations.Schema;

namespace lifeEcommerce.Models.Entities
{
    public class ShoppingCard
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Count { get; set; }

        [NotMapped]
        public double Price { get; set; }
    }
}
