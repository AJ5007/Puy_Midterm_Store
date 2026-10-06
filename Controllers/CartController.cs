using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Puy_Midterm_Store.Data;
using Puy_Midterm_Store.Models;

namespace Puy_Midterm_Store.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // READ: Display Cart Page and calculate Grand Total
        public async Task<IActionResult> Index()
        {
            var cartItems = await _context.CartItems.ToListAsync();
            ViewBag.GrandTotal = cartItems.Sum(item => item.Price * item.Quantity);
            return View(cartItems);
        }

        // ADD TO CART: Save or increment product in CartItem table
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1;
                _context.Update(existingCartItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Products");
        }

        // UPDATE QUANTITY: Change item quantity in cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                    _context.CartItems.Update(cartItem);
                }
                else
                {
                    _context.CartItems.Remove(cartItem);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // DELETE: Remove item from cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}