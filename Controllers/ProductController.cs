using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NG_Core_Auth.Data;
using NG_Core_Auth.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NG_Core_Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        // dependency injection 
        private readonly ApplicationDbContext applicationDbContext;
        public ProductController(ApplicationDbContext applicationDb)
        {
            applicationDbContext = applicationDb;
        }

        // GET: api/<ProductController>
        [HttpGet("[action]")]
        [Authorize(Policy = "LoggedIn")]
       //  public IEnumerable<string> GetProducts() change IEnum to IActionResult
        public IActionResult GetProducts()
        {
            return Ok(applicationDbContext.Products.ToList());
        }

        // add product method 
        [HttpPost("[action]")]
        [Authorize(Policy = "AdministratorOnly")]
        public async Task<IActionResult> AddProduct ([FromBody] ProductModel formData)
        {
            // create a new product 
            var newProduct = new ProductModel()
            {
                ProductName = formData.ProductName,
                ImageUrl = formData.ImageUrl,
                price = formData.price,
                ProductDescription = formData.ProductDescription,
                OutOfStock = formData.OutOfStock
            };

           await applicationDbContext.Products.AddAsync(newProduct);

            await applicationDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "AdministratorOnly")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] ProductModel formData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var findProduct = await applicationDbContext.Products.FindAsync(id);

            if(findProduct == null)
            {
                return NotFound();
            }

            findProduct.ImageUrl = formData.ImageUrl;
            findProduct.ProductDescription = formData.ProductDescription;
            findProduct.price = formData.price;
            findProduct.ProductName = formData.ProductName;
            findProduct.OutOfStock = formData.OutOfStock;

            applicationDbContext.Entry(findProduct).State = EntityState.Modified;

            await applicationDbContext.SaveChangesAsync();

            return Ok(new JsonResult($"Product with id {id} is updated"));
        }

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "AdministratorOnly")]
        public async Task<IActionResult> DeleteProduct ([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // get product
            var findProduct = await applicationDbContext.Products.FirstOrDefaultAsync(product => product.ProductId == id);

            if(findProduct == null)
            {
                return NotFound();
            }

            applicationDbContext.Products.Remove(findProduct);
            await applicationDbContext.SaveChangesAsync();

            return Ok(new JsonResult($"Product with id {id} is deleted."));
        }
    }
}
