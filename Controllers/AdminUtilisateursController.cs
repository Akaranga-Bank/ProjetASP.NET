using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin")]
public class AdminUtilisateursController : Controller
{
    private readonly AkarangaBankContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public AdminUtilisateursController(AkarangaBankContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _db.Utilisateurs
            .Include(u => u.Agence)
            .OrderBy(u => u.NomComplet)
            .ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> Create()
    {
        await ChargerListesAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string nomComplet, string email, string password, string role, int agenceId)
    {
        if (await _users.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError("", "Cet email est déjà utilisé.");
            await ChargerListesAsync();
            return View();
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            NomComplet = nomComplet,
            Role = role,
            AgenceId = agenceId
        };

        var result = await _users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", string.Join(" | ", result.Errors.Select(e => e.Description)));
            await ChargerListesAsync();
            return View();
        }

        await _users.AddToRoleAsync(user, role);
        TempData["Success"] = "Utilisateur créé.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActif(int id)
    {
        var u = await _db.Utilisateurs.FindAsync(id);
        if (u is null) return NotFound();
        u.EstActif = !u.EstActif;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Statut modifié.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ChargerListesAsync()
    {
        ViewBag.Agences = new SelectList(
            await _db.Agences.OrderBy(a => a.Code).ToListAsync(), "Id", "Code");
        ViewBag.Roles = new SelectList(new[] { "admin", "gestionnaire", "guichetier", "auditeur", "client" });
    }
}