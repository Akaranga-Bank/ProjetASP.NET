using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin,gestionnaire,guichetier")]
public class ClientsController : Controller
{
    private readonly ClientService _service;
    private readonly AkarangaBankContext _db;

    public ClientsController(ClientService service, AkarangaBankContext db)
    {
        _service = service;
        _db = db;
    }

    public async Task<IActionResult> Index(string? recherche, int? agenceId, string? statut)
    {
        var vm = await _service.GetListAsync(recherche, agenceId, statut);
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vm = await _service.GetDetailsAsync(id);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [Authorize(Roles = "admin,gestionnaire,guichetier")]
    public async Task<IActionResult> Create()
    {
        await ChargerAgencesAsync();
        return View(new ClientFormVM { NumeroClient = ProchainNumero() });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientFormVM vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }

        if (await _service.NumeroExisteAsync(vm.NumeroClient.Trim().ToUpperInvariant()))
        {
            ModelState.AddModelError(nameof(vm.NumeroClient), "Ce numéro existe déjà.");
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }
        if (await _service.CniExisteAsync(vm.NumeroCNI.Trim().ToUpperInvariant()))
        {
            ModelState.AddModelError(nameof(vm.NumeroCNI), "Ce CNI existe déjà.");
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }

        await _service.CreateAsync(vm);
        TempData["Success"] = "Client créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var c = await _service.GetByIdAsync(id);
        if (c is null) return NotFound();
        await ChargerAgencesAsync(c.AgenceId);
        return View(new ClientFormVM
        {
            Id = c.Id, NumeroClient = c.NumeroClient, Nom = c.Nom, Prenom = c.Prenom,
            DateNaissance = c.DateNaissance, NumeroCNI = c.NumeroCNI,
            Telephone = c.Telephone, Email = c.Email, Adresse = c.Adresse,
            AgenceId = c.AgenceId, Statut = c.Statut
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClientFormVM vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }
        if (await _service.NumeroExisteAsync(vm.NumeroClient.Trim().ToUpperInvariant(), vm.Id))
        {
            ModelState.AddModelError(nameof(vm.NumeroClient), "Ce numéro existe déjà.");
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }
        if (await _service.CniExisteAsync(vm.NumeroCNI.Trim().ToUpperInvariant(), vm.Id))
        {
            ModelState.AddModelError(nameof(vm.NumeroCNI), "Ce CNI existe déjà.");
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }

        var ok = await _service.UpdateAsync(vm);
        if (!ok) return NotFound();
        TempData["Success"] = "Client modifié.";
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
        try
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Client supprimé.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // ============ ACTIONS KYC ============

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "admin,gestionnaire")]
    public async Task<IActionResult> ValiderKYC(int id)
    {
        var k = await _db.VerificationsKYC.FindAsync(id);
        if (k is null) return NotFound();
        k.Statut = "Valide";
        k.DateVerification = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "KYC validé.";
        return RedirectToAction(nameof(Details), new { id = k.ClientId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "admin,gestionnaire")]
    public async Task<IActionResult> RejeterKYC(int id, string commentaire)
    {
        var k = await _db.VerificationsKYC.FindAsync(id);
        if (k is null) return NotFound();
        k.Statut = "Rejete";
        k.DateVerification = DateTime.UtcNow;
        k.Commentaire = commentaire;
        await _db.SaveChangesAsync();
        TempData["Success"] = "KYC rejeté.";
        return RedirectToAction(nameof(Details), new { id = k.ClientId });
    }

    // ============ HELPERS ============

    private async Task ChargerAgencesAsync(int? selected = null)
    {
        ViewBag.Agences = new SelectList(
            await _db.Agences.OrderBy(a => a.Code).ToListAsync(),
            "Id", "Code", selected);
    }

    private string ProchainNumero()
    {
        var dernier = _db.Clients.OrderByDescending(c => c.Id).FirstOrDefault();
        int n = (dernier?.Id ?? 0) + 1;
        return $"CLI-{n:D6}";
    }

    [HttpPost, ValidateAntiForgeryToken]
[Authorize(Roles = "admin,gestionnaire")]
public async Task<IActionResult> AjouterKYC(int clientId)
{
    var client = await _db.Clients.FindAsync(clientId);
    if (client is null) return NotFound();

    _db.VerificationsKYC.Add(new Models.VerificationKYC
    {
        ClientId = clientId,
        TypeDocument = "CNI",
        NumeroDocument = client.NumeroCNI,
        Statut = "EnAttente"
    });
    await _db.SaveChangesAsync();
    TempData["Success"] = "KYC ajouté en attente de validation.";
    return RedirectToAction(nameof(Details), new { id = clientId });
}



}