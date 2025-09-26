using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AgroBazaar.Repositories.UnitOfWork;
using AgroBazaar.Models.Entities;

namespace AgroBazaar.Controllers
{
    [Authorize(Roles = "Consumer")]
    public class ConsumerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConsumerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get featured products
            var featuredProducts = await _unitOfWork.Products.GetFeaturedProductsAsync(8);
            
            // Get categories with product count
            var categories = await _unitOfWork.Categories.GetCategoriesWithProductCountAsync();

            // Get user's cart info
            var cartItemCount = await _unitOfWork.Carts.GetCartItemCountAsync(userId);
            var cartTotal = await _unitOfWork.Carts.GetCartTotalAsync(userId);

            // Get user's recent orders
            var recentOrders = (await _unitOfWork.Orders.GetByCustomerIdAsync(userId)).Take(5);

            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.Categories = categories;
            ViewBag.CartItemCount = cartItemCount;
            ViewBag.CartTotal = cartTotal;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.UserName = $"{User.FindFirst("FirstName")?.Value} {User.FindFirst("LastName")?.Value}";

            return View();
        }

        public async Task<IActionResult> Products(int? categoryId, string? search, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 12)
        {
            IEnumerable<Models.Entities.Product> products;

            if (!string.IsNullOrEmpty(search))
            {
                products = await _unitOfWork.Products.SearchProductsAsync(search);
            }
            else if (categoryId.HasValue)
            {
                products = await _unitOfWork.Products.GetByCategoryIdAsync(categoryId.Value);
            }
            else
            {
                products = await _unitOfWork.Products.GetAvailableProductsAsync();
            }

            // Apply price filter if specified
            if (minPrice.HasValue || maxPrice.HasValue)
            {
                var min = minPrice ?? 0;
                var max = maxPrice ?? decimal.MaxValue;
                products = products.Where(p => p.Price >= min && p.Price <= max);
            }

            // Pagination
            var totalProducts = products.Count();
            var paginatedProducts = products.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.Categories = await _unitOfWork.Categories.GetActiveCategoriesAsync();
            ViewBag.CurrentCategory = categoryId;
            ViewBag.SearchTerm = search;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.TotalProducts = totalProducts;

            return View(paginatedProducts);
        }

        [HttpGet]
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _unitOfWork.Products.GetWithDetailsAsync(id);
            if (product == null || !product.IsActive || product.QuantityAvailable <= 0)
            {
                TempData["ErrorMessage"] = "Product not found or not available.";
                return RedirectToAction(nameof(Products));
            }

            // Get related products from same category
            var relatedProducts = (await _unitOfWork.Products.GetByCategoryIdAsync(product.CategoryId))
                .Where(p => p.Id != id)
                .Take(4);

            // Ratings info
            var ratings = await _unitOfWork.ProductRatings.GetByProductIdAsync(id);
            var avgRating = await _unitOfWork.ProductRatings.GetAverageRatingAsync(id);
            ViewBag.AverageRating = avgRating;
            ViewBag.Ratings = ratings.Take(10);

            // Can current user rate?
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool canRate = false;
            ProductRating? userRating = null;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(currentUserId);
                canRate = orders.Any(o => o.OrderItems.Any(oi => oi.ProductId == id));
                userRating = await _unitOfWork.ProductRatings.GetUserRatingAsync(currentUserId, id);
            }
            ViewBag.CanRate = canRate;
            ViewBag.UserRating = userRating;

            ViewBag.RelatedProducts = relatedProducts;
            return View(product);
        }

        public async Task<IActionResult> Cart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var cart = await _unitOfWork.Carts.GetWithItemsAsync(userId);
            return View(cart);
        }

        public async Task<IActionResult> Orders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(userId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var cart = await _unitOfWork.Carts.GetWithItemsAsync(userId);
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Add some products before checkout.";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Cart = cart;
            ViewBag.Total = cart.CartItems.Sum(i => i.UnitPrice * i.Quantity);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string deliveryAddress, string paymentMethod = "Cash on Delivery")
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var cart = await _unitOfWork.Carts.GetWithItemsAsync(userId);
                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction(nameof(Cart));
                }

                // Verify product availability
                foreach (var item in cart.CartItems)
                {
                    var isAvailable = await _unitOfWork.Products.IsProductAvailableAsync(item.ProductId, item.Quantity);
                    if (!isAvailable)
                    {
                        TempData["ErrorMessage"] = $"Product '{item.Product.Name}' is not available in requested quantity.";
                        return RedirectToAction(nameof(Cart));
                    }
                }

                // Create order
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    CustomerId = userId,
                    TotalAmount = cart.CartItems.Sum(i => i.UnitPrice * i.Quantity),
                    Status = "Pending",
                    PaymentMethod = paymentMethod,
                    PaymentStatus = "Pending",
                    DeliveryAddress = deliveryAddress,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = cart.CartItems.Select(ci => new OrderItem
                    {
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.UnitPrice,
                        TotalPrice = ci.UnitPrice * ci.Quantity
                    }).ToList()
                };

                await _unitOfWork.Orders.AddAsync(order);

                // Update product quantities
                foreach (var item in cart.CartItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.QuantityAvailable -= item.Quantity;
                        product.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Products.Update(product);
                    }
                }

                // Clear cart
                foreach (var item in cart.CartItems)
                {
                    await _unitOfWork.Carts.RemoveItemFromCartAsync(cart.Id, item.ProductId);
                }

                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Order placed successfully! Order Number: {order.OrderNumber}";
                return RedirectToAction("OrderConfirmation", new { orderNumber = order.OrderNumber });
            }
            catch (Exception _)
            {
                TempData["ErrorMessage"] = "Error placing order. Please try again.";
                return RedirectToAction(nameof(Cart));
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(string orderNumber)
        {
            var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
            if (order == null || order.CustomerId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(Orders));
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _unitOfWork.Orders.GetWithItemsAsync(id);
            if (order == null || order.CustomerId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(Orders));
            }

            // Provide existing user ratings for items in this order
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var productIds = order.OrderItems.Select(oi => oi.ProductId).Distinct().ToList();
            var ratings = new Dictionary<int, ProductRating?>();
            foreach (var pid in productIds)
            {
                ratings[pid] = await _unitOfWork.ProductRatings.GetUserRatingAsync(userId, pid);
            }
            ViewBag.UserRatings = ratings;

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Invoice(string orderNumber)
        {
            var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (order == null || order.CustomerId != userId)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(Orders));
            }
            return View(order);
        }

        // ===== Ratings =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateProduct(int productId, int rating, string? comment, string? returnUrl)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Please login to rate products.";
                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? Redirect(returnUrl)
                    : RedirectToAction("ProductDetails", new { id = productId });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Rating must be between 1 and 5.";
                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? Redirect(returnUrl)
                    : RedirectToAction("ProductDetails", new { id = productId });
            }

            // Ensure user has purchased the product
            var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(userId);
            var hasPurchased = orders.Any(o => o.OrderItems.Any(oi => oi.ProductId == productId));
            if (!hasPurchased)
            {
                TempData["ErrorMessage"] = "You can only rate products you have purchased.";
                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? Redirect(returnUrl)
                    : RedirectToAction("ProductDetails", new { id = productId });
            }

            await _unitOfWork.ProductRatings.AddOrUpdateAsync(userId, productId, rating, comment);
            await _unitOfWork.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thanks for your review!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("ProductDetails", new { id = productId });
        }

        // ===== Cart AJAX Endpoints =====
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Please login to add items to cart." });
                }

                if (quantity <= 0)
                {
                    return Json(new { success = false, message = "Quantity must be at least 1." });
                }

                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null || !product.IsActive)
                {
                    return Json(new { success = false, message = "Product not found." });
                }

                var isAvailable = await _unitOfWork.Products.IsProductAvailableAsync(productId, quantity);
                if (!isAvailable)
                {
                    return Json(new { success = false, message = "Requested quantity is not available." });
                }

                await _unitOfWork.Carts.AddItemToCartAsync(userId, productId, quantity);
                await _unitOfWork.SaveChangesAsync();

                var cartItemCount = await _unitOfWork.Carts.GetCartItemCountAsync(userId);
                return Json(new { success = true, message = "Added to cart.", cartItemCount });
            }
            catch
            {
                return Json(new { success = false, message = "Failed to add item to cart." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(int productId, int quantity)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Please login to update cart." });
                }

                var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    return Json(new { success = false, message = "Cart not found." });
                }

                if (quantity < 0)
                {
                    return Json(new { success = false, message = "Quantity cannot be negative." });
                }

                if (quantity > 0)
                {
                    var available = await _unitOfWork.Products.IsProductAvailableAsync(productId, quantity);
                    if (!available)
                    {
                        return Json(new { success = false, message = "Requested quantity is not available." });
                    }
                }

                await _unitOfWork.Carts.UpdateCartItemQuantityAsync(cart.Id, productId, quantity);
                await _unitOfWork.SaveChangesAsync();

                var cartTotal = await _unitOfWork.Carts.GetCartTotalAsync(userId);
                var cartItemCount = await _unitOfWork.Carts.GetCartItemCountAsync(userId);
                return Json(new { success = true, cartTotal, cartItemCount });
            }
            catch
            {
                return Json(new { success = false, message = "Failed to update cart item." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Please login to modify cart." });
                }

                var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    return Json(new { success = false, message = "Cart not found." });
                }

                await _unitOfWork.Carts.RemoveItemFromCartAsync(cart.Id, productId);
                await _unitOfWork.SaveChangesAsync();

                var cartTotal = await _unitOfWork.Carts.GetCartTotalAsync(userId);
                var cartItemCount = await _unitOfWork.Carts.GetCartItemCountAsync(userId);
                return Json(new { success = true, cartTotal, cartItemCount });
            }
            catch
            {
                return Json(new { success = false, message = "Failed to remove item from cart." });
            }
        }

        private string GenerateOrderNumber()
        {
            return "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
        }
    }
}
