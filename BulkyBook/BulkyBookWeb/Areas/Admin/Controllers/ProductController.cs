using BulkyBook.Models;
using BulkyBook.DataAccess;
using Microsoft.AspNetCore.Mvc;
using BulkyBook.Business.Services.IServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using BulkyBook.Utility;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class ProductController : Controller
    {
        private readonly IProductService _productservice;
        private readonly ICategoryService _categoryservice;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductService productservice, ICategoryService categoryservice, IWebHostEnvironment webHostEnvironment)
        {
            _productservice = productservice;
            _categoryservice = categoryservice;
            _webHostEnvironment = webHostEnvironment;
        }

        // it overrides the parent permission
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        
        public async Task<IActionResult> Upsert(int? id)
        {
            var categories = await _categoryservice.GetAllCategoriesAsync();
            ProductVM productVM = new()
            {
                CategoryList = categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                Product = new Product()
            };

            if(id == null || id == 0)
            {
                // create
                return View(productVM);
            }
            else
            {
                // update
                productVM.Product = await _productservice.GetProductByIdAsync(id.Value);
                return View(productVM);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Upsert")]
        public async Task<IActionResult> UpsertPOST(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine("images", "products");
                    string finalPath = Path.Combine(wwwRootPath, productPath);

                    if (!Directory.Exists(finalPath))
                        Directory.CreateDirectory(finalPath);

                    using (var filestream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }

                    productVM.Product.ImageUrl = Path.Combine(@"\", productPath, fileName).Replace("\\", "/");

                }

                if (productVM.Product.Id == null || productVM.Product.Id == 0)
                {
                    // create
                    await _productservice.CreateProductAsync(productVM.Product);
                    TempData["success"] = "Product Created Successfully.";
                }
                else
                {
                    // update
                    await _productservice.UpdateProductAsync(productVM.Product);
                    TempData["success"] = "Product Updated Successfully.";
                }

                
                
                return RedirectToAction("Index");
            }
            else
            {
                var categories = await _categoryservice.GetAllCategoriesAsync();
                productVM = new()
                {
                    CategoryList = categories.Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    }),
                    Product = new Product()
                };

                return View(productVM);
            }
        }

        #region "API CALLS"
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productservice.GetAllProductsAsync(true);
            return Json(new { data = products });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            var productToBeDeleted = await _productservice.GetProductByIdAsync(id.Value);

            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            // delete product image if that exists
            if (!string.IsNullOrEmpty(productToBeDeleted.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.Trim('\\','/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _productservice.DeleteProductAsync(id.Value);
            return Json(new { success = true, message = "Deleted Successfully." });

        }

        #endregion


    }
}
