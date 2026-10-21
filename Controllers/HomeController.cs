using System.Security.Claims;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AkarangaBankContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public HomeController(AkarangaBankContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<IActionResult> Index()
{
    var user = await _users.GetUserAsync(User);
    if (user is null) return RedirectToAction("Login", "Account");

    ViewBag.User = user;
    ViewBag.Role = user.Role;

    // ========== RÔLE CLIENT : vue simplifiée ==========
    if (user.Role == "client")
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Email == user.Email)
            ?? await _db.Clients.FirstOrDefaultAsync();

        if (client is not null)
        {
            var comptes = await _db.Comptes.AsNoTracking()
                .Where(c => c.ClientId == client.Id)
                .ToListAsync();

            ViewBag.Client = client;
            ViewBag.MesComptes = comptes;
            ViewBag.MonSolde = comptes.Sum(c => c.Solde);

            ViewBag.MesOperations = await _db.Operations.AsNoTracking()
                .Include(o => o.Compte)
                .Where(o => o.Compte.ClientId == client.Id)
                .OrderByDescending(o => o.DateOperation)
                .Take(5)
                .ToListAsync();
        }

        return View("IndexClient");  // Vue dédiée client
    }

    // ========== AUTRES RÔLES : vue admin/global ==========
    ViewBag.NbAgences = await _db.Agences.CountAsync();
    ViewBag.NbClients = await _db.Clients.CountAsync();
    ViewBag.NbComptes = await _db.Comptes.CountAsync();
    ViewBag.NbOperations = await _db.Operations.CountAsync();
    ViewBag.NbAlertes = await _db.AlertesAML.CountAsync(a => a.Statut == "Ouverte");
    ViewBag.SoldeTotal = await _db.Comptes.SumAsync(c => (decimal?)c.Solde) ?? 0m;

    if (user.Role == "admin" || user.Role == "gestionnaire" || user.Role == "auditeur")
    {
        ViewBag.Alertes = await _db.AlertesAML
            .Include(a => a.Client)
            .OrderByDescending(a => a.DateAlerte)
            .Take(5)
            .ToListAsync();
    }

    ViewBag.Operations = await _db.Operations
        .Include(o => o.Compte)
        .OrderByDescending(o => o.DateOperation)
        .Take(5)
        .ToListAsync();

    return View("Index");  // Vue admin/global
}
    
    [AllowAnonymous]
    public IActionResult Privacy() => View();
}