using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "client")]
public class MesComptesController : Controller
{
    private readonly AkarangaBankContext _db;
    private readonly ComptabiliteService _compta;
    private readonly UserManager<ApplicationUser> _users;

    public MesComptesController(AkarangaBankContext db, ComptabiliteService compta, UserManager<ApplicationUser> users)
    {
        _db = db;
        _compta = compta;
        _users = users;
    }

    private async Task<Client?> GetCurrentClientAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return null;

        return await _db.Clients.FirstOrDefaultAsync(c => c.Email == user.Email)
            ?? await _db.Clients.OrderBy(c => c.Id).FirstOrDefaultAsync();
    }

    public async Task<IActionResult> Index()
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Content("Aucun client associé à votre compte.");

        var comptes = await _db.Comptes.AsNoTracking()
            .Include(c => c.TypeCompte)
            .Include(c => c.Agence)
            .Where(c => c.ClientId == client.Id)
            .ToListAsync();

        ViewBag.Client = client;
        return View(comptes);
    }

    public async Task<IActionResult> Releve(int id)
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Content("Aucun client associé.");

        var compte = await _db.Comptes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (compte is null || compte.ClientId != client.Id) return NotFound();

        var vm = await _compta.GetReleveAsync(id);
        if (vm is null) return NotFound();
        return View("~/Views/Operations/Releve.cshtml", vm);
    }

    public async Task<IActionResult> Virement()
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Content("Aucun client associé.");

        await ChargerListesAsync(client.Id);
        return View("~/Views/Operations/Virement.cshtml", new VirementVM());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Virement(VirementVM vm)
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Content("Aucun client associé.");

        var source = await _db.Comptes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == vm.CompteSourceId);
        if (source is null || source.ClientId != client.Id)
        {
            ModelState.AddModelError("", "Compte source invalide.");
            await ChargerListesAsync(client.Id);
            return View("~/Views/Operations/Virement.cshtml", vm);
        }

        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(client.Id);
            return View("~/Views/Operations/Virement.cshtml", vm);
        }

        try
        {
            await _compta.VirerAsync(vm, 0);
            TempData["Success"] = "Virement effectué.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await ChargerListesAsync(client.Id);
            return View("~/Views/Operations/Virement.cshtml", vm);
        }
    }

    private async Task ChargerListesAsync(int clientId)
    {
        var mesComptes = await _db.Comptes.AsNoTracking()
            .Where(c => c.ClientId == clientId && c.Statut == "Actif")
            .Select(c => new { c.Id, Libelle = c.NumeroCompte + " (" + c.Solde + " BIF)" })
            .ToListAsync();

        var comptesDest = await _db.Comptes.AsNoTracking()
            .Where(c => c.Statut == "Actif" && c.ClientId != clientId)
            .Select(c => new { c.Id, Libelle = c.NumeroCompte + " — " + c.Client.Nom + " " + c.Client.Prenom })
            .ToListAsync();

        ViewBag.MesComptes = new SelectList(mesComptes, "Id", "Libelle");
        ViewBag.ComptesDest = new SelectList(comptesDest, "Id", "Libelle");
    }
}