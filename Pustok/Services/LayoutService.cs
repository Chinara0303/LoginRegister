using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pustok.DAL;
using Pustok.Models;
using Pustok.ViewModels;
using Pustok.ViewModels.Basket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Services
{
    public class LayoutService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public LayoutService(IHttpContextAccessor httpContext, AppDbContext context, UserManager<AppUser> userManager)
        {
            _httpContextAccessor = httpContext;
            _context = context;
            _userManager = userManager;
        }

        public List<BasketVM> GetBasket()
        {
            string basket = _httpContextAccessor.HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (basket != null)
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);
            }
            else
            {
                basketVMs = new List<BasketVM>();
            }

            foreach (BasketVM basketVM in basketVMs)
            {
                basketVM.Title = _context.Products.FirstOrDefault(p => p.Id == basketVM.Id).Title;
                basketVM.MainImage = _context.Products.FirstOrDefault(p => p.Id == basketVM.Id).MainImage;
                basketVM.Price = _context.Products.FirstOrDefault(p => p.Id == basketVM.Id).Price;
                basketVM.GenreName = _context.Products.Include(p=>p.Genre).FirstOrDefault(p => p.Id == basketVM.Id).Genre.Name;
            }
            

            return basketVMs;
        }
        public async Task<UserVM> GetUserAsync()
        {
            UserVM userVM = null;
            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                userVM = await _context.AppUsers.Where(p => p.UserName == _httpContextAccessor.HttpContext.User.Identity.Name)
                    .Select(u => new UserVM
                    {
                        Name = u.Name,
                        SurName = u.SurName

                    }).FirstOrDefaultAsync();
            }
            return userVM;
        }
    }
}
