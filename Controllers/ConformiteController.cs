using AkarangaBank.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin,gestionnaire,auditeur")]
public class ConformiteController : Controller
{
    private readonly AkarangaBankContext _db;
    public ConformiteController(AkarangaBankContext db) => _db = db;

    public async Task<IActionResult> Alertes(string? statut, string? niveau)
    {
        var q = _db.AlertesAML.AsNoTracking()
            .Include(a => a.Client)
            .Include(a => a.Operation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statut)) q = q.Where(a => a.Statut == statut);
        if (!string.IsNullOrWhiteSpace(niveau)) q = q.Where(a => a.Niveau == niveau);

        ViewBag.Statut = statut;
        ViewBag.Niveau = niveau;

        return View(await q.OrderByDescending(a => a.DateAlerte).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "admin,gestionnaire")]
    public async Task<IActionResult> Traiter(int id, string statut)
    {
        var a = await _db.AlertesAML.FindAsync(id);
        if (a is null) return NotFound();
        a.Statut = statut;
        a.DateTraitement = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Alerte mise à jour.";
        return RedirectToAction(nameof(Alertes));
    }
}