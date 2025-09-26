using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AgroBazaar.Models;
using AgroBazaar.Repositories.UnitOfWork;

namespace AgroBazaar.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var featured = await _unitOfWork.Products.GetFeaturedProductsAsync(8);
        ViewBag.FeaturedProducts = featured;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> TrackOrder(string? orderNumber)
    {
        if (!string.IsNullOrWhiteSpace(orderNumber))
        {
            var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
            ViewBag.Order = order;
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found. Please check the order number.";
            }
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}