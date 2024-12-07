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
using System.Diagnostics.Metrics;


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
            .ThenInclude(a => a.Transactions)
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

    /* ------------------- HELPER Accounts.cshtml  ------------------- */
    // Adds $100 dollars to the account 
    [HttpPost]
    public async Task<IActionResult> AddMoney(Guid accountId)
    {
        // Get the account
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null)
        {
            TempData["Error"] = "Account not found";
            return RedirectToAction("Accounts");
        }

        // Add the transaction
        var transaction = new Transaction
        {
            AccountId = accountId,
            Amount = 100,
            Date = DateTime.Now,
            Message = "Added $100",
            Receiver = accountId
        };

        // Update account balance
        account.Balance += 100;

        // Update the account transactions list
        // account.Transactions.Add(transaction);

        // Save changes
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        TempData["Success"] = "$100 has been added to your account";
        return RedirectToAction("Accounts");
    }
    
    [HttpPost]
    public async Task<IActionResult> RemoveMoney(Guid accountId)
{

    // Verify the account belongs to the current user
    var account = await _context.Accounts
        .FirstOrDefaultAsync(a => a.Id == accountId);

    if (account == null)
    {
        TempData["Error"] = "Account not found or access denied";
        return RedirectToAction("Accounts");
    }

    // Check if there's enough balance
    if (account.Balance < 100)
    {
        TempData["Error"] = "Insufficient funds";
        return RedirectToAction("Accounts");
    }

    // Add the transaction
    var transaction = new Transaction
    {
        AccountId = accountId,
        Amount = -100, // Negative amount for withdrawal
        Date = DateTime.Now,
        Message = "Removed $100",
        Receiver = accountId
    };

    // Update account balance
    account.Balance -= 100;

     // Update the account transactions list
    // account.Transactions.Add(transaction);

    // Save changes
    _context.Transactions.Add(transaction);
    
    await _context.SaveChangesAsync();

    TempData["Success"] = "$100 has been removed from your account";
        return RedirectToAction("Accounts");
    }

    [HttpPost]
public async Task<IActionResult> DeleteAccount(Guid accountId)
{
    var currentUser = await GetCurrentUser();
    if (currentUser == null)
    {
        return RedirectToAction("Login");
    }

    // Find the account
    var accountToDelete = await _context.Accounts
        .Include(a => a.Transactions)
        .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == currentUser.Id);

    if (accountToDelete == null)
    {
        TempData["Error"] = "Account not found or access denied";
        return RedirectToAction("Accounts");
    }

    // Remove related transactions first
    if (accountToDelete.Transactions != null)
    {
        _context.Transactions.RemoveRange(accountToDelete.Transactions);
    }

    // Remove the account
    _context.Accounts.Remove(accountToDelete);
    
    await _context.SaveChangesAsync();

    TempData["Success"] = "Account successfully deleted";
    return RedirectToAction("Accounts");
}

    /* ------------------- HELPER Accounts.cshtml  ------------------- */
    [HttpPost]
    public async Task<IActionResult> AddNewAccount(string accountName, double initialBalance)
    {
        var currentUser = await GetCurrentUser();
        if (currentUser == null)
        {
            return RedirectToAction("Login");
        }

        // Generate new account
        Account newAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountName = accountName,
            Balance = initialBalance,
            Transactions =[],

            UserId = currentUser.Id,
            User = currentUser
        };

        currentUser.Accounts.Add(newAccount);
        _context.Accounts.Add(newAccount);

        await _context.SaveChangesAsync();

        return RedirectToAction("Accounts");
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
            Goals = []
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
    public async Task<IActionResult> Goals()
    {
        
        var currentUser = await GetCurrentUser(); // Get the currently logged-in user
        if (currentUser == null)
        {
            return RedirectToAction("Login");
        }

        return View(currentUser);
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

   /* ------------------- HELPER Preferences.cshtml  ------------------- */
    [HttpPost]
    public async Task<IActionResult> ChangePassword(string CurrentPassword, string SecurityAnswer, string NewPassword, string PasswordReentry)
    {
        var currentUser = await GetCurrentUser();
        if (currentUser == null)
        {
            return RedirectToAction("Login");
        }

        if (currentUser.Password != CurrentPassword)
        {
            TempData["Error"] = "Current password is incorrect";
            return RedirectToAction("Preferences");
        }

        if (currentUser.SecurityAnswer != SecurityAnswer)
        {
            TempData["Error"] = "Security answer is incorrect";
            return RedirectToAction("Preferences");
        }

        if (NewPassword != PasswordReentry)
        {
            TempData["Error"] = "Passwords do not match";
            return RedirectToAction("Preferences");
        }

        currentUser.Password = NewPassword;
        _context.Users.Update(currentUser);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Password has been changed";
        return RedirectToAction("Preferences");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
