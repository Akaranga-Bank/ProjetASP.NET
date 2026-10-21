using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin,gestionnaire")]
public class AgencesController : Controller
{
    private readonly AgenceService _service;
    public AgencesController(AgenceService service) => _service = service;

    public async Task<IActionResult> Index(string? recherche)
    {
        var vm = await _service.GetListAsync(recherche);
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var a = await _service.GetByIdAsync(id);
        if (a is null) return NotFound();
        return View(a);
    }

    [Authorize(Roles = "admin")]
    public IActionResult Create() => View(new AgenceFormVM());

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(AgenceFormVM vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (await _service.CodeExisteAsync(vm.Code.Trim().ToUpperInvariant()))
        {
            ModelState.AddModelError(nameof(vm.Code), "Ce code existe déjà.");
            return View(vm);
        }
        await _service.CreateAsync(vm);
        TempData["Success"] = "Agence créée.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var a = await _service.GetByIdAsync(id);
        if (a is null) return NotFound();
        return View(new AgenceFormVM
        {
            Id = a.Id, Code = a.Code, Nom = a.Nom, Ville = a.Ville, EstSiege = a.EstSiege
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Edit(AgenceFormVM vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (await _service.CodeExisteAsync(vm.Code.Trim().ToUpperInvariant(), vm.Id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Ce code existe déjà.");
            return View(vm);
        }
        var ok = await _service.UpdateAsync(vm);
        if (!ok) return NotFound();
        TempData["Success"] = "Agence modifiée.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _service.GetByIdAsync(id);
        if (a is null) return NotFound();
        return View(a);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Agence supprimée.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}