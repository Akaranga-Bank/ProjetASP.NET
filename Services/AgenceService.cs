using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class AgenceService
{
    private readonly AkarangaBankContext _db;
    public AgenceService(AkarangaBankContext db) => _db = db;

    public async Task<AgenceListVM> GetListAsync(string? recherche)
    {
        var q = _db.Agences.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var r = recherche.Trim();
            q = q.Where(a => a.Code.Contains(r) || a.Nom.Contains(r) || a.Ville.Contains(r));
        }
        var list = await q.OrderByDescending(a => a.EstSiege).ThenBy(a => a.Ville).ToListAsync();
        return new AgenceListVM { Agences = list, Recherche = recherche };
    }

    public async Task<Agence?> GetByIdAsync(int id)
        => await _db.Agences.FirstOrDefaultAsync(a => a.Id == id);

    public async Task<bool> CodeExisteAsync(string code, int? excludeId = null)
    {
        var q = _db.Agences.Where(a => a.Code == code);
        if (excludeId.HasValue) q = q.Where(a => a.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<Agence> CreateAsync(AgenceFormVM vm)
    {
        var a = new Agence
        {
            Code = vm.Code.Trim().ToUpperInvariant(),
            Nom = vm.Nom.Trim(),
            Ville = vm.Ville.Trim(),
            EstSiege = vm.EstSiege,
            DateCreation = DateTime.UtcNow
        };
        _db.Agences.Add(a);
        await _db.SaveChangesAsync();
        return a;
    }

    public async Task<bool> UpdateAsync(AgenceFormVM vm)
    {
        var a = await _db.Agences.FindAsync(vm.Id);
        if (a is null) return false;
        a.Code = vm.Code.Trim().ToUpperInvariant();
        a.Nom = vm.Nom.Trim();
        a.Ville = vm.Ville.Trim();
        a.EstSiege = vm.EstSiege;
        a.DateModification = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> APeuEtreSupprimeeAsync(int id)
    {
        var a = await _db.Agences.FindAsync(id);
        if (a is null) return false;
        if (a.EstSiege) return false; // règle métier : le siège ne se supprime jamais
        return !await _db.Clients.AnyAsync(c => c.AgenceId == id)
            && !await _db.Comptes.AnyAsync(c => c.AgenceId == id)
            && !await _db.Utilisateurs.AnyAsync(u => u.AgenceId == id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await APeuEtreSupprimeeAsync(id))
            throw new InvalidOperationException(
                "Impossible de supprimer cette agence : c'est le siège ou elle a des dépendances.");
        var a = await _db.Agences.FindAsync(id);
        if (a is null) return false;
        _db.Agences.Remove(a);
        await _db.SaveChangesAsync();
        return true;
    }
}