using BulkyBook.Models;
using BulkyBook.DataAccess;
using Microsoft.AspNetCore.Mvc;
using BulkyBook.Business.Services.IServices;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IProductService _productservice;
        private readonly ICategoryService _categoryservice;

        public ProductController(IProductService productservice, ICategoryService categoryservice)
        {
            _productservice = productservice;
            _categoryservice = categoryservice;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }

        

        public async Task<IActionResult> Upsert()
        {
            IEnumerable<SelectListItem> categoryList = (await _categoryservice.GetAllCategoriesAsync())
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

            ViewData["categoryList"] = categoryList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Upsert")]
        public async Task<IActionResult> UpsertPOST(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productservice.CreateProductAsync(product);
                TempData["success"] = "Product Created Successfully.";
                return RedirectToAction("Index");
            }
            return View();
            
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var product = await _productservice.GetProductByIdAsync(id.Value);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            await _productservice.DeleteProductAsync(id);
            TempData["success"] = "Product Deleted Successfully.";
            return RedirectToAction("Index");

        }

        #region "API CALLS"
        public async Task<IActionResult> GetAll()
        {
            var products = await _productservice.GetAllProductsAsync(true);
            return Json(new { data = products });
        }

        #endregion


    }
}
