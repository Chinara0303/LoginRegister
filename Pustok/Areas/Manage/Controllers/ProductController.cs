using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustok.DAL;
using Pustok.Extentions;
using Pustok.Helpers;
using Pustok.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles ="SuperAdmin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            IEnumerable<Product> products = await _context.Products
                .Where(p => !p.IsDeleted).ToListAsync();

            ViewBag.PageCount = Math.Ceiling((double)products.Count() / 5);
            ViewBag.CurentPage = page;

            IEnumerable<Product> model = products.OrderByDescending(p=>p.Id)
                .Skip((page -1) * 5)
                .Take(5)
                .ToList();

            return View(model);
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .FirstOrDefaultAsync(p=>p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Authors = await _context.Authors.ToListAsync();
            ViewBag.Genres = await _context.Genres.ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Authors = await _context.Authors.ToListAsync();
            ViewBag.Genres = await _context.Genres.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View();
            }

            if (product.AuthorId != null && !await _context.Authors.AnyAsync(a => a.Id == product.AuthorId))
            {
                ModelState.AddModelError("AuthorId", "Daxil Edilen Muellif Sehfdir");
                return View();
            }

            if (product.GenreId != null && !await _context.Genres.AnyAsync(a => a.Id == product.GenreId))
            {
                ModelState.AddModelError("GenreId", "Daxil Edilen Janr Sehfdir");
                return View();
            }

            if (product.MainImgFile == null)
            {
                ModelState.AddModelError("MainImgFile", "Main Sekil Mutleq Secilmelidi");
                return View();
            }

            if (product.HoverImgFile == null)
            {
                ModelState.AddModelError("HoverImgFile", "Hover Sekil Mutleq Secilmelidi");
                return View();
            }

            if (product.MainImgFile.CheckContentType("image/jpeg"))
            {
                ModelState.AddModelError("MainImgFile", "Main Sekil Novu Ancaq jpe ve ya jpeg Mutleq Secilmelidi");
                return View();
            }

            if (product.HoverImgFile.CheckContentType("image/jpeg"))
            {
                ModelState.AddModelError("HoverImgFile", "Hover Sekil Novu Ancaq jpe ve ya jpeg Mutleq Secilmelidi");
                return View();
            }

            if (product.MainImgFile.CheckSize(50))
            {
                ModelState.AddModelError("MainImgFile", "Main Sekil 20 kb ola biler");
                return View();
            }

            if (product.HoverImgFile.CheckSize(50))
            {
                ModelState.AddModelError("HoverImgFile", "Hover Sekil 20 kb ola biler");
                return View();
            }

            product.MainImage  = await product.MainImgFile.FileCreateAsync(_env, "image", "products");
            product.HoverImage = await product.HoverImgFile.FileCreateAsync(_env, "image", "products");

            List<ProductImage> productImages = new List<ProductImage>();

            if (product.ProductImagesFile != null)
            {
                bool error = false;

                foreach (IFormFile productimage in product.ProductImagesFile)
                {
                    if (productimage.CheckContentType("image/jpeg"))
                    {
                        error = true;
                        ModelState.AddModelError("", $"Mehsul Sekil - {productimage.FileName} Novu Ancaq jpe ve ya jpeg Mutleq Secilmelidi");
                    }

                    if (productimage.CheckSize(50))
                    {
                        error = true;
                        ModelState.AddModelError("", $"Mehsul Sekil - {productimage.FileName} olcusu 50 kb olmalidir Mutleq Secilmelidi");
                    }

                    ProductImage productImage = new ProductImage
                    {
                        Name = await productimage.FileCreateAsync(_env, "image", "products")
                    };

                    productImages.Add(productImage);
                }

                if (error)
                {
                    return View();
                }

                product.ProductImages = productImages;
            }



            product.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
            //return RedirectToAction("Index");
            //return RedirectToAction("Index","Home",new { area="manage"});
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products.Include(p=>p.ProductImages).FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            ViewBag.Authors = await _context.Authors.ToListAsync();
            ViewBag.Genres = await _context.Genres.ToListAsync();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (id == null || id != product.Id) return BadRequest();

            Product dbProduct = await _context.Products
                .Include(p=>p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (dbProduct == null) return NotFound();

            ViewBag.Authors = await _context.Authors.ToListAsync();
            ViewBag.Genres = await _context.Genres.ToListAsync();

            if (5-dbProduct.ProductImages.Count() < product.ProductImagesFile.Count())
            {
                ModelState.AddModelError("ProductImagesFile", $"Maksimum Bu {5-dbProduct.ProductImages.Count()} qeder");
                return View(dbProduct);
            }

            if (product.AuthorId != null && !await _context.Authors.AnyAsync(a => a.Id == product.AuthorId))
            {
                ModelState.AddModelError("AuthorId", "Daxil Edilen Muellif Sehfdir");
                return View(product);
            }

            if (product.GenreId != null && !await _context.Genres.AnyAsync(a => a.Id == product.GenreId))
            {
                ModelState.AddModelError("GenreId", "Daxil Edilen Janr Sehfdir");
                return View(product);
            }

            if (product.MainImgFile != null)
            {
                if (product.MainImgFile.CheckContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainImgFile", "Main Sekil Novu Ancaq jpe ve ya jpeg Mutleq Secilmelidi");
                    return View();
                }

                if (product.MainImgFile.CheckSize(50))
                {
                    ModelState.AddModelError("MainImgFile", "Main Sekil 20 kb ola biler");
                    return View();
                }

                Helper.DeleteFile(_env, dbProduct.MainImage, "image", "products");

                product.MainImage  = await product.MainImgFile.FileCreateAsync(_env, "image", "products");
            }

            if (product.HoverImgFile != null)
            {
                if (product.HoverImgFile.CheckContentType("image/jpeg"))
                {
                    ModelState.AddModelError("HoverImgFile", "Hover Sekil Novu Ancaq jpe ve ya jpeg Mutleq Secilmelidi");
                    return View();
                }

                if (product.HoverImgFile.CheckSize(50))
                {
                    ModelState.AddModelError("HoverImgFile", "Hover Sekil 20 kb ola biler");
                    return View();
                }

                Helper.DeleteFile(_env, dbProduct.HoverImage, "image", "products");

                product.HoverImage = await product.HoverImgFile.FileCreateAsync(_env, "image", "products");
            }

            List<ProductImage> productImages = new List<ProductImage>();

            if (product.ProductImagesFile != null)
            {
                bool error = false;

                foreach (IFormFile productimage in product.ProductImagesFile)
                {
                    if (productimage.CheckContentType("image/jpeg"))
                    {
                        error = true;
                        ModelState.AddModelError("", $"Mehsul Sekil - {productimage.FileName} Novu Ancaq jpe ve ya jpeg Mutleq Secilmelidi");
                    }

                    if (productimage.CheckSize(50))
                    {
                        error = true;
                        ModelState.AddModelError("", $"Mehsul Sekil - {productimage.FileName} olcusu 50 kb olmalidir Mutleq Secilmelidi");
                    }

                    ProductImage productImage = new ProductImage
                    {
                        Name = await productimage.FileCreateAsync(_env, "image", "products")
                    };

                    productImages.Add(productImage);
                }

                if (error)
                {
                    return View();
                }

                dbProduct.ProductImages.AddRange(productImages);
            }

            dbProduct.Title = product.Title;
            dbProduct.Price = product.Price;
            dbProduct.DiscountPrice = product.DiscountPrice;
            dbProduct.MainImage = product.MainImage;
            dbProduct.HoverImage = product.HoverImage;
            dbProduct.AuthorId = product.AuthorId;
            dbProduct.GenreId = product.GenreId;
            dbProduct.IsFeature = product.IsFeature;
            dbProduct.IsArrival = product.IsArrival;
            dbProduct.IsMostView = product.IsMostView;
            dbProduct.UpdatedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            return View(product);
        }

        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            //_context.Products.Remove(product);

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteFile(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductImages.Any(pi=>pi.Id == id) && !p.IsDeleted);

            if (product == null) return NotFound();

            ViewBag.Authors = await _context.Authors.ToListAsync();
            ViewBag.Genres = await _context.Genres.ToListAsync();

            ProductImage productImage = product.ProductImages.FirstOrDefault(p => p.Id == id);

            string filename = productImage.Name;

            Helper.DeleteFile(_env, filename, "image", "products");

            product.ProductImages.Remove(productImage);

            await _context.SaveChangesAsync();

            return PartialView("_UpdateProductPartial", product);
        }
    }
}
