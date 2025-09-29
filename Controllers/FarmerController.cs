using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AgroBazaar.Repositories.UnitOfWork;
using AgroBazaar.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroBazaar.Controllers
{
    [Authorize(Roles = "Farmer")]
    public class FarmerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FarmerController> _logger;

        public FarmerController(IUnitOfWork unitOfWork, ILogger<FarmerController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Removed negotiation expiration (repository not implemented)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get farmer's products count
            var products = await _unitOfWork.Products.GetByFarmerIdAsync(userId);
            var totalProducts = products.Count();
            var activeProducts = products.Count(p => p.IsActive && p.QuantityAvailable > 0);

            // Get farmer's orders
            var orders = await _unitOfWork.Orders.GetOrdersForFarmerAsync(userId);
            var totalOrders = orders.Count();
            var pendingOrders = orders.Count(o => o.Status == "Pending");

            // Calculate total revenue
            var totalRevenue = await _unitOfWork.Orders.GetTotalSalesByFarmerAsync(userId);

            ViewBag.TotalProducts = totalProducts;
            ViewBag.ActiveProducts = activeProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.UserName = $"{User.FindFirst("FirstName")?.Value} {User.FindFirst("LastName")?.Value}";

            return View();
        }

        public async Task<IActionResult> Products()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var products = await _unitOfWork.Products.GetByFarmerIdAsync(userId);
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetActiveCategoriesAsync(), "Id", "Name");
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product model, IFormFile? imageFile)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found during product submission");
                return RedirectToAction("Login", "Auth");
            }

            _logger.LogInformation("AddProduct called for user {UserId} with product name: {ProductName}", userId, model.Name);

            // Debug ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid for product submission. Errors: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                
                ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetActiveCategoriesAsync(), "Id", "Name");
                return View(model);
            }

            try
            {
                _logger.LogInformation("Processing product addition for user {UserId}", userId);

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    _logger.LogInformation("Processing image upload: {FileName}, Size: {Size}", imageFile.FileName, imageFile.Length);
                    
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    
                    model.ImageUrl = "/images/products/" + uniqueFileName;
                    _logger.LogInformation("Image uploaded successfully: {ImageUrl}", model.ImageUrl);
                }

                model.FarmerId = userId;
                model.CreatedAt = DateTime.UtcNow;
                model.IsActive = true;

                _logger.LogInformation("Adding product to database: {@Product}", new { model.Name, model.Price, model.CategoryId, model.FarmerId });

                await _unitOfWork.Products.AddAsync(model);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product added successfully with ID: {ProductId}", model.Id);

                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product for farmer: {FarmerId}. Model: {@Model}", userId, new { model.Name, model.Price, model.CategoryId });
                TempData["ErrorMessage"] = $"Error adding product: {ex.Message}";
                ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetActiveCategoriesAsync(), "Id", "Name");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var product = await _unitOfWork.Products.GetWithDetailsAsync(id);
            
            if (product == null || product.FarmerId != userId)
            {
                TempData["ErrorMessage"] = "Product not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetActiveCategoriesAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product model, IFormFile? imageFile)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id != model.Id)
            {
                return BadRequest();
            }

            var existingProduct = await _unitOfWork.Products.GetWithDetailsAsync(id);
            if (existingProduct == null || existingProduct.FarmerId != userId)
            {
                TempData["ErrorMessage"] = "Product not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Products));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetActiveCategoriesAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            try
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProduct.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    
                    model.ImageUrl = "/images/products/" + uniqueFileName;
                }
                else
                {
                    model.ImageUrl = existingProduct.ImageUrl;
                }

                // Update existing product
                existingProduct.Name = model.Name;
                existingProduct.Description = model.Description;
                existingProduct.Price = model.Price;
                existingProduct.Unit = model.Unit;
                existingProduct.QuantityAvailable = model.QuantityAvailable;
                existingProduct.CategoryId = model.CategoryId;
                existingProduct.ImageUrl = model.ImageUrl;
                existingProduct.IsActive = model.IsActive;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Products.Update(existingProduct);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId} for farmer: {FarmerId}", id, userId);
                TempData["ErrorMessage"] = "Error updating product. Please try again.";
                ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetActiveCategoriesAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var product = await _unitOfWork.Products.GetWithDetailsAsync(id);
            
            if (product == null || product.FarmerId != userId)
            {
                return Json(new { success = false, message = "Product not found or you don't have permission to delete it." });
            }

            try
            {
                // Delete image if exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _unitOfWork.Products.Remove(product);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, message = "Product deleted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId} for farmer: {FarmerId}", id, userId);
                return Json(new { success = false, message = "Error deleting product. Please try again." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleProductStatus(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var product = await _unitOfWork.Products.GetWithDetailsAsync(id);
            
            if (product == null || product.FarmerId != userId)
            {
                return Json(new { success = false, message = "Product not found or you don't have permission to modify it." });
            }

            try
            {
                product.IsActive = !product.IsActive;
                product.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Products.Update(product);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Product {(product.IsActive ? "activated" : "deactivated")} successfully!",
                    isActive = product.IsActive 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for product {ProductId}", id);
                return Json(new { success = false, message = "Error updating product status. Please try again." });
            }
        }

        public async Task<IActionResult> Orders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var orders = await _unitOfWork.Orders.GetOrdersForFarmerAsync(userId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var order = await _unitOfWork.Orders.GetWithItemsAsync(id);
            
            if (order == null || !order.OrderItems.Any(oi => oi.Product.FarmerId == userId))
            {
                TempData["ErrorMessage"] = "Order not found or you don't have permission to view it.";
                return RedirectToAction(nameof(Orders));
            }

            // Filter order items to show only this farmer's products
            var farmerOrderItems = order.OrderItems.Where(oi => oi.Product.FarmerId == userId).ToList();
            ViewBag.FarmerOrderItems = farmerOrderItems;
            ViewBag.FarmerTotal = farmerOrderItems.Sum(oi => oi.TotalPrice);

            // Average ratings for products (display-only)
            var avgRatings = new Dictionary<int, double>();
            foreach (var item in farmerOrderItems)
            {
                if (!avgRatings.ContainsKey(item.ProductId))
                {
                    avgRatings[item.ProductId] = await _unitOfWork.ProductRatings.GetAverageRatingAsync(item.ProductId);
                }
            }
            ViewBag.ProductAvgRatings = avgRatings;

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var order = await _unitOfWork.Orders.GetWithItemsAsync(orderId);
            
            if (order == null || !order.OrderItems.Any(oi => oi.Product.FarmerId == userId))
            {
                return Json(new { success = false, message = "Order not found or access denied." });
            }

            try
            {
                // Validate status transition
                var validStatuses = new[] { "Processing", "Shipped", "Delivered" };
                if (!validStatuses.Contains(status))
                {
                    return Json(new { success = false, message = "Invalid status." });
                }

                // Update order status
                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                // Update payment status when delivered
                if (status == "Delivered" && order.PaymentMethod == "Cash on Delivery")
                {
                    order.PaymentStatus = "Paid";
                }

                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Order status updated to {status} successfully!",
                    newStatus = status 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
                return Json(new { success = false, message = "Error updating order status. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId, string? cancellationReason = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Authentication required." });
            }

            try
            {
                // Verify order contains farmer's products
                var order = await _unitOfWork.Orders.GetWithItemsAsync(orderId);
                if (order == null || !order.OrderItems.Any(oi => oi.Product.FarmerId == userId))
                {
                    return Json(new { success = false, message = "Order not found or you don't have permission to cancel this order." });
                }

                // Check if order can be cancelled
                if (order.Status != "Pending" && order.Status != "Processing")
                {
                    return Json(new { success = false, message = $"Cannot cancel order. Order is already {order.Status}." });
                }

                // Cancel the order (this will restore product quantities)
                var success = await _unitOfWork.Orders.CancelOrderAsync(orderId, cancellationReason);
                
                if (success)
                {
                    await _unitOfWork.SaveChangesAsync();
                    return Json(new { 
                        success = true, 
                        message = "Order cancelled successfully. Product quantities have been restored.",
                        newStatus = "Cancelled"
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to cancel order. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                return Json(new { success = false, message = "An error occurred while cancelling the order. Please try again." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var order = await _unitOfWork.Orders.GetWithItemsAsync(id);
            if (order == null || !order.OrderItems.Any(oi => oi.Product.FarmerId == userId))
            {
                TempData["ErrorMessage"] = "Order not found or access denied.";
                return RedirectToAction(nameof(Orders));
            }

            // limit to this farmer's items
            var farmerItems = order.OrderItems.Where(oi => oi.Product.FarmerId == userId).ToList();
            ViewBag.FarmerItems = farmerItems;
            ViewBag.FarmerTotal = farmerItems.Sum(i => i.TotalPrice);
            return View(order);
        }


        public IActionResult Inventory()
        {
            return View();
        }

        public IActionResult PaymentHistory()
        {
            return View();
        }
    }
}
