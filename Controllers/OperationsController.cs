using System.Security.Claims;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin,gestionnaire,guichetier")]
public class OperationsController : Controller
{
    private readonly ComptabiliteService _compta;
    private readonly AkarangaBankContext _db;

    public OperationsController(ComptabiliteService compta, AkarangaBankContext db)
    {
        _compta = compta;
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    // ============ DÉPÔT ============
    public async Task<IActionResult> Depot()
    {
        await ChargerComptesAsync();
        return View(new DepotVM());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Depot(DepotVM vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }
        try
        {
            await _compta.DeposerAsync(vm, CurrentUserId);
            TempData["Success"] = "Dépôt enregistré.";
            return RedirectToAction(nameof(Journal));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }
    }

    // ============ RETRAIT ============
    public async Task<IActionResult> Retrait()
    {
        await ChargerComptesAsync();
        return View(new RetraitVM());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Retrait(RetraitVM vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }
        try
        {
            await _compta.RetirerAsync(vm, CurrentUserId);
            TempData["Success"] = "Retrait enregistré.";
            return RedirectToAction(nameof(Journal));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }
    }

    // ============ VIREMENT ============
    public async Task<IActionResult> Virement()
    {
        await ChargerComptesAsync();
        return View(new VirementVM());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Virement(VirementVM vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerComptesAsync(vm.CompteSourceId, vm.CompteDestinationId);
            return View(vm);
        }
        try
        {
            await _compta.VirerAsync(vm, CurrentUserId);
            TempData["Success"] = "Virement enregistré.";
            return RedirectToAction(nameof(Journal));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await ChargerComptesAsync(vm.CompteSourceId, vm.CompteDestinationId);
            return View(vm);
        }
    }

    // ============ JOURNAL ============
    public async Task<IActionResult> Journal(DateTime? du, DateTime? au, string? type)
    {
        var vm = await _compta.GetJournalAsync(du, au, type);
        return View(vm);
    }

    // ============ RELEVÉ ============
    public async Task<IActionResult> Releve(int id)
    {
        var vm = await _compta.GetReleveAsync(id);
        if (vm is null) return NotFound();
        return View(vm);
    }

    // ============ CONTRÔLE ÉQUILIBRE ============
    [Authorize(Roles = "admin,auditeur")]
    public async Task<IActionResult> Equilibre()
    {
        ViewBag.Equilibre = await _compta.VerifierEquilibreAsync();
        return View();
    }

    // ============ HELPER ============
    private async Task ChargerComptesAsync(int? sourceId = null, int? destId = null)
    {
        var comptes = await _db.Comptes
            .Include(c => c.Client)
            .Where(c => c.Statut == "Actif")
            .OrderBy(c => c.NumeroCompte)
            .Select(c => new
            {
                c.Id,
                Libelle = $"{c.NumeroCompte} — {c.Client.Nom} {c.Client.Prenom} ({c.Solde:N0} BIF)"
            })
            .ToListAsync();

        ViewBag.Comptes = new SelectList(comptes, "Id", "Libelle", sourceId);
        ViewBag.ComptesDest = new SelectList(comptes, "Id", "Libelle", destId);
    }
}