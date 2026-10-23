using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentas.Models.ViewModels;

namespace SistemaVentas.Controllers;

public class AccountController : Controller
{
    // ──────────────────────────────────────────────────────────────
    // Credenciales fijas (sin base de datos)
    // ──────────────────────────────────────────────────────────────
    private static readonly List<(string Username, string Password, string Nombre, string Rol)> _usuarios = new()
    {
        ("admin",    "admin123",    "Administrador Principal", "Administrador"),
        ("vendedor", "vendedor123", "Vendedor de Turno",       "Vendedor"),
    };

    // ──────────────────────────────────────────────────────────────
    // GET: /Account/Login
    // ──────────────────────────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // ──────────────────────────────────────────────────────────────
    // POST: /Account/Login
    // ──────────────────────────────────────────────────────────────
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["ReturnUrl"] = model.ReturnUrl;

        if (!ModelState.IsValid)
            return View(model);

        // Buscar usuario en la lista fija (sin base de datos)
        var usuario = _usuarios.FirstOrDefault(u =>
            u.Username.Equals(model.Username.Trim(), StringComparison.OrdinalIgnoreCase) &&
            u.Password == model.Password);

        if (usuario == default)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }

        // Crear los Claims de identidad
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Username),
            new Claim(ClaimTypes.Name,            usuario.Nombre),
            new Claim("Username",                 usuario.Username),
            new Claim(ClaimTypes.Role,            usuario.Rol)
        };

        var claimsIdentity    = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties    = new AuthenticationProperties
        {
            IsPersistent = model.Recordarme,
            ExpiresUtc   = model.Recordarme
                ? DateTimeOffset.UtcNow.AddDays(7)
                : DateTimeOffset.UtcNow.AddHours(10)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        TempData["Success"] = $"¡Bienvenido de nuevo, {usuario.Nombre}!";

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ──────────────────────────────────────────────────────────────
    // GET: /Account/Logout
    // ──────────────────────────────────────────────────────────────
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Sesión cerrada correctamente.";
        return RedirectToAction("Login", "Account");
    }

    // ──────────────────────────────────────────────────────────────
    // GET: /Account/Perfil
    // ──────────────────────────────────────────────────────────────
    [Authorize]
    [HttpGet]
    public IActionResult Perfil()
    {
        var username = User.FindFirst("Username")?.Value ?? "";
        var nombre   = User.Identity?.Name ?? "";
        var rol      = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

        ViewBag.Username = username;
        ViewBag.Nombre   = nombre;
        ViewBag.Rol      = rol;

        return View();
    }

    // ──────────────────────────────────────────────────────────────
    // GET: /Account/AccessDenied
    // ──────────────────────────────────────────────────────────────
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
