using BulkyBook.Models;
using BulkyBook.DataAccess;
using Microsoft.AspNetCore.Mvc;
using BulkyBook.Business.Services.IServices;

namespace BulkyBookWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryservice;

        public CategoryController(ICategoryService categoryservice)
        {
            _categoryservice = categoryservice;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryservice.GetAllCategoriesAsync();
            return View("Index", categories);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(Category category)
        {
            if (!string.IsNullOrEmpty(category.Name) && !await _categoryservice.IsCategoryNameUniqueAsync(category.Name))
            {
                ModelState.AddModelError("", "Category already exists!");
            }

            if (ModelState.IsValid)
            {
                await _categoryservice.CreateCategoryAsync(category);
                TempData["success"] = "Category Created Successfully.";
                return RedirectToAction("Index");
            }
            return View();
            
        }

        public async Task<IActionResult> Update(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }

            var category = await _categoryservice.GetCategoryByIdAsync(id.Value);

            if(category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Update")]
        public async Task<IActionResult> UpdatePost(Category category)
        {
            if (!string.IsNullOrEmpty(category.Name) && 
               !await _categoryservice.IsCategoryNameUniqueAsync(category.Name,category.Id))
            {
                ModelState.AddModelError("", "Category already exists!");
            }

            if (ModelState.IsValid)
            {
                await _categoryservice.UpdateCategoryAsync(category);
                TempData["success"] = "Category Updated Successfully.";
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

            var category = await _categoryservice.GetCategoryByIdAsync(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            await _categoryservice.DeleteCategoryAsync(id);
            TempData["success"] = "Category Deleted Successfully.";
            return RedirectToAction("Index");

        }


    }
}
