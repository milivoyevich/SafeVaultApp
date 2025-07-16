using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using SafeVaultApp.Helpers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction("Dashboard");

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(User user)
    {
        ModelState.Clear();
        user.Role = "default"; // Clear role to avoid validation issues
        user.ConfirmPassword = user.Password;  
        TryValidateModel(user);
        if (!ModelState.IsValid)
            return View(user);

        var hashedPassword = PasswordHelper.HashPassword(user.Password);
        var validUser = _db.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == hashedPassword);
        if (validUser != null)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, validUser.Role)
            };
            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme))
            );
            if (validUser.Role == "Admin")
                return RedirectToAction("AdminPanel");
            return RedirectToAction("Dashboard");
        }

        ViewBag.Error = "Invalid credentials";
        return View(user);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(User user)
    {
        if (!ModelState.IsValid)
            return View(user);

        if (_db.Users.Any(u => u.Username == user.Username))
        {
            ViewBag.Error = "Username already exists.";
            return View(user);
        }
        if (string.IsNullOrWhiteSpace(user.Role))
        {
            ViewBag.Error = "Role is required.";
            return View(user);
        }
        user.Password = PasswordHelper.HashPassword(user.Password);
        _db.Users.Add(new User {
            Username = user.Username,
            Password = user.Password,
            Role = user.Role
        });
        _db.SaveChanges();
        return RedirectToAction("Login");
    }

    [Authorize]
    public IActionResult Dashboard() => View();

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminPanel() => View();
    [HttpGet]
    public IActionResult AccessDenied() => View();  
}