using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarungKu.Data;
using WarungKu.Models;
using WarungKu.Models.Dto;
using Microsoft.Extensions.Logging;

namespace WarungKu.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly WarungKuDbContext _context;
        private readonly ILogger<SalesController> _logger;

        public SalesController(WarungKuDbContext context, ILogger<SalesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {

            var loggedInUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(loggedInUsername))
            {
                return RedirectToAction("Login", "User");
            }


            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loggedInUsername);

            if (user == null)
            {
                TempData["Error"] = "User login tidak ditemukan di database.";
                return RedirectToAction("Login", "User");
            }

            var salesQuery = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.User)
                .Include(s => s.Items)
                .Where(s => s.UserId == user.Id)
                .AsQueryable();


            if (startDate.HasValue)
            {
                salesQuery = salesQuery.Where(s => s.Date.Date >= startDate.Value.Date);
                ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            }

            if (endDate.HasValue)
            {
                var localEndDate = endDate.Value.Date.AddDays(1);
                var utcEndDate = DateTime.SpecifyKind(localEndDate, DateTimeKind.Utc);
                salesQuery = salesQuery.Where(s => s.Date < utcEndDate);
            }

            salesQuery = salesQuery.OrderByDescending(s => s.Date);
            var sales = await salesQuery.ToListAsync();
            return View(sales);
        }


        public IActionResult Create()
        {
            var loggedInUsername = User.Identity!.Name;

            ViewBag.LoggedInUsername = loggedInUsername;
            ViewBag.Products = _context.Products.ToList();
            ViewBag.Customers = _context.Customers.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int CustomerId, string itemsJson, int discountPercent = 0, decimal voucher = 0, string paymentMethod = "Cash")
        {
            if (string.IsNullOrEmpty(itemsJson))
            {
                ModelState.AddModelError("", "Belum ada item yang ditambahkan");
            }

            var items = new List<SaleItemDto>();

            if (!string.IsNullOrEmpty(itemsJson))
            {
                items = System.Text.Json.JsonSerializer.Deserialize<List<SaleItemDto>>(itemsJson);
            }
            items!.ForEach(i =>
            {
                _logger.LogInformation("Deserialized item - ProductId: {ProductId}, Quantity: {Quantity}, Price: {Price}", i.ProductId, i.Quantity, i.Price);
            });


            var productIds = items.Select(i => i.ProductId).Distinct().ToList();


            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            bool insufficientStock = false;
            var productNames = new List<string>();
            var insufficientProductIds = new List<int>();


            foreach (var item in items)
            {
                if (products.TryGetValue(item.ProductId, out var product))
                {
                    if (item.Quantity > product.Stock)
                    {
                        insufficientStock = true;
                        productNames.Add($"{product.Name} (Stok: {product.Stock})");
                        insufficientProductIds.Add(item.ProductId);
                    }
                }
                else
                {
                    ModelState.AddModelError("", $"Produk dengan ID {item.ProductId} tidak ditemukan.");
                    return SetupViewAndReturn(items, insufficientProductIds);
                }
            }
            if (insufficientStock)
            {
                var errorMessage = $"Stok tidak cukup untuk item berikut: {string.Join(", ", productNames)}.";
                ModelState.AddModelError("", errorMessage);
                return SetupViewAndReturn(items, insufficientProductIds); // Kirim item dan ID error
            }

            if (ModelState.IsValid && items.Count > 0)
            {
                var customer = await _context.Customers.FindAsync(CustomerId);

                var loggedInUsername = User.Identity!.Name;
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loggedInUsername);
                if (user == null)
                {
                    ModelState.AddModelError("", "User login tidak ditemukan di database.");
                    return SetupViewAndReturn();
                }
                var sale = new Sale
                {
                    UserId = user.Id,
                    Discount = discountPercent,
                    PaymentMethod = paymentMethod,
                    Voucher = voucher,
                    User = user,
                    CustomerId = CustomerId,
                    Customer = customer!,
                    Date = DateTime.UtcNow,
                    Items = items.Select(i => new SaleItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                foreach (var item in sale.Items)
                {
                    if (products.TryGetValue(item.ProductId, out var product))
                    {
                        product.Stock -= item.Quantity;
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Sales");
            }

            ViewBag.Products = _context.Products.ToList();
            ViewBag.Customers = _context.Customers.ToList();
            return View();
        }

        IActionResult SetupViewAndReturn(List<SaleItemDto> currentItems = null, List<int> errorIds = null)
        {
            ViewBag.Products = _context.Products.ToList();
            ViewBag.Customers = _context.Customers.ToList();
            var loggedInUsername = User.Identity!.Name;

            ViewBag.LoggedInUsername = loggedInUsername;
            if (currentItems != null && currentItems.Count > 0)
            {
                ViewBag.ErrorItemsJson = System.Text.Json.JsonSerializer.Serialize(currentItems);
                ViewBag.ErrorProductIds = errorIds;
            }

            return View();
        }

    }
}
