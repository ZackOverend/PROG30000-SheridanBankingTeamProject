using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SheridanBankingTeamProject.Models.Entities;
using SheridanBankingTeamProject.Models;
using Microsoft.EntityFrameworkCore;
using SheridanBankingTeamProject.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;


namespace SheridanBankingTeamProject.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> RegistrationDone(User model){
        
        model.Id = Guid.NewGuid();
        model.Accounts = new List<Account>();

        User newUser = new User{
            Id = Guid.NewGuid(),
            Email = model.Email,
            Name = model.Name,
            Password = model.Password,
            SecurityAnswer = model.SecurityAnswer,
            Accounts = []
        };

        Account newAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountName = "Savings Account", 
            Balance = 0.0,
            Transactions = [],
        };

        // Add Account to Account Database
        _context.Accounts.Add(newAccount);
        newUser.Accounts.Add(newAccount);

        // Add User to User Database
        _context.Users.Add(newUser);
        
        await _context.SaveChangesAsync();
        return View();
    }
    
    // This should remove each account from the db and update their amounts and push back to db
    [HttpPost]
    public async Task<IActionResult> TransferFunds(Account sender, Account receiver, double amount, String message){
        if (sender.Balance < amount){
            // Throw error if the balance is too low in the account
            
        }
        else{
            // Create a transaction object
            Transaction newTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Sender = sender.Id,
                    Receiver = receiver.Id,
                    Amount = amount,
                    Message = message
                };

            // Update balances in each account
            sender.Balance -= amount;
            receiver.Balance += amount;

            _context.Accounts.Update(sender);
            _context.Accounts.Update(receiver);

            _context.Transactions.Add(newTransaction);
            
            await _context.SaveChangesAsync();
        }


        
        
        
        return View();
    }


    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        

        if (user != null /* && verify password */)
        {
            await HttpContext.SignInAsync(
                
                CookieAuthenticationDefaults.AuthenticationScheme,

                new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.Name, user.Name) },
                    CookieAuthenticationDefaults.AuthenticationScheme))
            );

            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = "Invalid login attempt";
        return View();

    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Index));
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
