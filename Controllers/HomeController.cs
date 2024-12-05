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
    

    // Helper method to get current user
    private async Task<User> GetCurrentUser()
    {
        if (!User.Identity.IsAuthenticated)
            return null;

        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        // Get the actual user from your database
        var user = await _context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Email == userEmail);
            
        return user;
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
    
    /* ------------------- VIEW Accounts.cshtml  ------------------- */
    public async Task<IActionResult> Accounts()
    {
        var currentUser = await GetCurrentUser();
        if (currentUser == null)
        {
            return RedirectToAction("Login");
        }
        return View(currentUser);
    }

    /* ------------------- VIEW Admin.cshtml  ------------------- */
    [AllowAnonymous]
    public async Task<IActionResult> Admin()
    {
        // Returns IEnumerable
        var users = _context.Users
            .Include(u => u.Accounts)
            .ThenInclude(a => a.Transactions)
            .ToList();

        return View(users);
    }

    /* ------------------- HELPER function for Admin.cshtml ------------------- */
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> FindAccountByUser(Guid id){

        return RedirectToAction($"AdminViewAccount", "Home");
    }
    /* ------------------- HELPER function from Admin.cshtml ------------------- */
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> FindDetailsUser(Guid id){
        return View();
    }
    
    /* ------------------- HELPER function from Admin.cshtml ------------------- */
    [AllowAnonymous]    
    [HttpPost]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        // Find the user in the database
        var user = await _context.Users.FindAsync(id);

        // Must remove all accounts first

        if (user == null)
        {
            return NotFound(); // Handle case where user is not found
        }

        // Remove the user from the database
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Admin", "Home");
    }

    /* -------------------VIEW  AdminViewAccount.cshtml  ------------------- */
    [AllowAnonymous]
    public async Task<IActionResult> AdminViewAccount(Guid id)
    {
        // Get user -> retrieve all accounts associated with user id
        var userAccountsList = await _context.Accounts
            .Where(account => account.UserId == id) // Assuming 'UserId' is the foreign key in the Accounts table
            .ToListAsync();
        
        return View(userAccountsList);
    }
    
    /* ------------------- VIEW Index.cshtml  ------------------- */
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }
    
    /* ------------------- VIEW Login.cshtml  ------------------- */
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    /* ------------------- HELPER for Login.cshtml  ------------------- */
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user != null /* && verify password */)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                principal);

            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = "Invalid login attempt";
        return View();
    }
    
    /* ------------------- HELPER for Login.cshtml  ------------------- */
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> RegistrationDone(User model){
        model.Id = Guid.NewGuid();

        User newUser = new User{
            Id = Guid.NewGuid(),
            Email = model.Email,
            Name = model.Name,
            Password = model.Password,
            SecurityAnswer = model.SecurityAnswer,
            Accounts = [],
            Goals = ["i love myself"]
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
    
    /* ------------------- VIEW Logout.cshtml  ------------------- */
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Index));
    }

    /* ------------------- VIEW Privacy.cshtml  ------------------- */
    public IActionResult Privacy()
    {
        return View();
    }

    
    /* ------------------- VIEW Transfers.cshtml  ------------------- */
    public async Task<IActionResult> Transfers()
    {
        var currentUser = await GetCurrentUser();
        if (currentUser == null)
        {
            return RedirectToAction("Login");
        }
        return View(currentUser);
    }

    /* ------------------- HELPER Etransfer  ------------------- */

    [HttpPost]
    public async Task<IActionResult> Etransfer(Account sender, double amount, String email){
        if (sender.Balance < amount){
            
            
        }
        else{
            
            Transaction newTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Sender = sender.Id,
                    Receiver = Guid.Empty,
                    Amount = amount,
                    Message = email
                };

            
            sender.Balance -= amount;
            
            _context.Accounts.Update(sender);

            _context.Transactions.Add(newTransaction);
            
            await _context.SaveChangesAsync();
        }  
        return View();
    }
    
    /* ------------------- VIEW Goals.cshtml  ------------------- */
    public IActionResult Goals()
    {

        var usersWithGoals = _context.Users
            .ToList();

    return View(usersWithGoals);
    }

    [HttpPost]
    public async Task<IActionResult> AddGoal(Guid userId, string newGoal)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        if (user.Goals == null)
        {
            user.Goals = new List<string>();
        }

        user.Goals.Add(newGoal);

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Goals");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteGoal(Guid userId, string goal)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        if (user.Goals == null || !user.Goals.Contains(goal))
        {
            return NotFound("Goal not found");
        }

        user.Goals.Remove(goal); 

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Goals");
    }

    /* ------------------- VIEW Preferences.cshtml  ------------------- */
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
