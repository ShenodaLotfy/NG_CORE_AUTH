using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NG_Core_Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NG_Core_Auth.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> 
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
           
        // add data to database tables here, instead of adding it in the database itself
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // add roles to IdentityRole table in running the application migrations
            builder.Entity<IdentityRole>().HasData(
                new { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new { Id = "2", Name = "Customer", NormalizedName = "CUSTOMER" },
                new { Id = "3", Name = "Moderator", NormalizedName = "MODERATOR" }
            );
        }

        // add the table that we are going to deal with (updating, deleting, reading, or creating) CRUD
        public DbSet<ProductModel> Products { get; set; } // make it Products not Product
    }
}
