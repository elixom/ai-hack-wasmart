using Microsoft.AspNetCore.Mvc;
using GreenSync.Lib.Services;

namespace GreenSync_app.Controllers;

public class EcoCreditsController : Controller
{
    private readonly IEcoCreditService _ecoCreditService;
    private readonly IAuthService _authService;

    public EcoCreditsController(IEcoCreditService ecoCreditService, IAuthService authService)
    {
        _ecoCreditService = ecoCreditService;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var ecoCredit = await _ecoCreditService.GetEcoCreditByUserIdAsync(currentUser.Id);
        if (ecoCredit == null)
        {
            ecoCredit = await _ecoCreditService.CreateEcoCreditAccountAsync(currentUser.Id);
        }

        return View(ecoCredit);
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var transactions = await _ecoCreditService.GetTransactionHistoryAsync(currentUser.Id);
        return View(transactions);
    }

    [HttpGet]
    public async Task<IActionResult> Redeem()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var balance = await _ecoCreditService.GetBalanceAsync(currentUser.Id);
        ViewBag.CurrentBalance = balance;

        var redeemOptions = new List<RedeemOption>
        {
            new() { Name = "Municipal Service Discount (5%)", Cost = 20, Description = "Get 5% off your next municipal service bill" },
            new() { Name = "Water Bill Discount (10%)", Cost = 50, Description = "Reduce your water bill by 10% for one month" },
            new() { Name = "Waste Collection Priority", Cost = 30, Description = "Priority waste collection for your next 3 reports" },
            new() { Name = "Environmental Certificate", Cost = 100, Description = "Receive an official environmental stewardship certificate" },
            new() { Name = "Community Garden Voucher", Cost = 75, Description = "Access to community garden resources and tools" }
        };

        return View(redeemOptions);
    }

    [HttpPost]
    public async Task<IActionResult> Redeem(string optionName, decimal cost, string description)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var success = await _ecoCreditService.RedeemCreditsAsync(currentUser.Id, cost, $"Redeemed: {optionName}");
        
        if (success)
        {
            TempData["Success"] = $"Successfully redeemed {optionName}! {cost} Eco-Credits have been deducted from your account.";
        }
        else
        {
            TempData["Error"] = "Insufficient Eco-Credits for this redemption.";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Balance()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return Json(new { balance = 0 });
        }

        var balance = await _ecoCreditService.GetBalanceAsync(currentUser.Id);
        return Json(new { balance });
    }
}

public class RedeemOption
{
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string Description { get; set; } = string.Empty;
}
