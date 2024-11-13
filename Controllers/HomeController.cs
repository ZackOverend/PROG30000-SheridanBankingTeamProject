using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SheridanBankingTeamProject.Models.Entities;
using SheridanBankingTeamProject.Models;
using Microsoft.EntityFrameworkCore;
using SheridanBankingTeamProject.Data;



namespace SheridanBankingTeamProject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    

    [HttpPost]
    public async Task<IActionResult> RegistrationDone(User model){
        

        model.Id = Guid.NewGuid();
        model.Accounts = new List<Account>();

        Account newAccount = new Account
        {
            
            Id = Guid.NewGuid(),
            AccountName = "Savings Account", 
            Balance = 0.0
        };

        model.Accounts.Add(newAccount);

        _context.Users.Add(model);
        await _context.SaveChangesAsync();

            
        
        
        return View();
        
    }


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Accounts() 
    {
        return View();   
    }

    public IActionResult Budgeting()
    {
        return View();
    }
    
    public IActionResult Goals()
    {
        return View();
    }

    public IActionResult Preferences()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
