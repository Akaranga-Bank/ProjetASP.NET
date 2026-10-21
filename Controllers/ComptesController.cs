using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin,gestionnaire,guichetier")]
public class ComptesController : Controller
{
    private readonly CompteService _service;
    private readonly AkarangaBankContext _db;

    public ComptesController(CompteService service, AkarangaBankContext db)
    {
        _service = service;
        _db = db;
    }

    public async Task<IActionResult> Index(string? recherche, int? agenceId, int? typeId, string? statut)
    {
        var vm = await _service.GetListAsync(recherche, agenceId, typeId, statut);
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vm = await _service.GetDetailsAsync(id);
        if (vm is null) return NotFound();
        return View(vm);
    }

    public async Task<IActionResult> Create(int? clientId)
    {
        await ChargerListesAsync(clientId, null, null);
        return View(new CompteFormVM { ClientId = clientId ?? 0, NumeroCompte = ProchainNumero() });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompteFormVM vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(vm.ClientId, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }
        if (await _service.NumeroExisteAsync(vm.NumeroCompte.Trim().ToUpperInvariant()))
        {
            ModelState.AddModelError(nameof(vm.NumeroCompte), "Ce numéro existe déjà.");
            await ChargerListesAsync(vm.ClientId, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }
        await _service.CreateAsync(vm);
        TempData["Success"] = "Compte ouvert.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var c = await _service.GetByIdAsync(id);
        if (c is null) return NotFound();
        await ChargerListesAsync(c.ClientId, c.TypeCompteId, c.AgenceId);
        return View(new CompteFormVM
        {
            Id = c.Id, NumeroCompte = c.NumeroCompte, ClientId = c.ClientId,
            TypeCompteId = c.TypeCompteId, AgenceId = c.AgenceId,
            Statut = c.Statut, Solde = c.Solde, Devise = c.Devise,
            ClientNom = $"{c.Client.Nom} {c.Client.Prenom}"
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CompteFormVM vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(vm.ClientId, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }
        var ok = await _service.UpdateAsync(vm);
        if (!ok) return NotFound();
        TempData["Success"] = "Compte modifié.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "admin,gestionnaire")]
    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _service.GetDetailsAsync(id);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = "admin,gestionnaire")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try { await _service.DeleteAsync(id); TempData["Success"] = "Compte supprimé."; }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private async Task ChargerListesAsync(int? clientId, int? typeId, int? agenceId)
    {
        ViewBag.Clients = new SelectList(
            await _db.Clients.OrderBy(c => c.Nom).ToListAsync(),
            "Id", "NumeroClient", clientId);
        ViewBag.Types = new SelectList(
            await _db.TypesCompte.OrderBy(t => t.Code).ToListAsync(),
            "Id", "Libelle", typeId);
        ViewBag.Agences = new SelectList(
            await _db.Agences.OrderBy(a => a.Code).ToListAsync(),
            "Id", "Code", agenceId);
    }

    private string ProchainNumero()
    {
        var dernier = _db.Comptes.OrderByDescending(c => c.Id).FirstOrDefault();
        int n = (dernier?.Id ?? 0) + 1;
        return $"BJM-{n / 10000 + 1:D4}-{n % 10000:D4}";
    }
}