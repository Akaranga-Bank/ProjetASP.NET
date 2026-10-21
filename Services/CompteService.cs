using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class CompteService
{
    private readonly AkarangaBankContext _db;
    public CompteService(AkarangaBankContext db) => _db = db;

    public async Task<CompteListVM> GetListAsync(string? recherche, int? agenceId, int? typeId, string? statut)
    {
        var q = _db.Comptes.AsNoTracking()
            .Include(c => c.Client).Include(c => c.TypeCompte).Include(c => c.Agence)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var r = recherche.Trim();
            q = q.Where(c => c.NumeroCompte.Contains(r)
                          || c.Client.Nom.Contains(r) || c.Client.Prenom.Contains(r));
        }
        if (agenceId.HasValue) q = q.Where(c => c.AgenceId == agenceId.Value);
        if (typeId.HasValue) q = q.Where(c => c.TypeCompteId == typeId.Value);
        if (!string.IsNullOrWhiteSpace(statut)) q = q.Where(c => c.Statut == statut);

        return new CompteListVM
        {
            Comptes = await q.OrderBy(c => c.NumeroCompte).ToListAsync(),
            Recherche = recherche,
            AgenceId = agenceId,
            TypeCompteId = typeId,
            Statut = statut,
            Agences = await _db.Agences.AsNoTracking().OrderBy(a => a.Code).ToListAsync(),
            Types = await _db.TypesCompte.AsNoTracking().OrderBy(t => t.Code).ToListAsync()
        };
    }

    public async Task<Compte?> GetByIdAsync(int id)
        => await _db.Comptes.AsNoTracking()
            .Include(c => c.Client).Include(c => c.TypeCompte).Include(c => c.Agence)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<CompteDetailsVM?> GetDetailsAsync(int id)
    {
        var c = await _db.Comptes.AsNoTracking()
            .Include(x => x.Client).Include(x => x.TypeCompte).Include(x => x.Agence)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return null;

        return new CompteDetailsVM
        {
            Compte = c,
            Ecritures = await _db.Ecritures.AsNoTracking()
                .Include(e => e.Operation)
                .Where(e => e.CompteId == id)
                .OrderByDescending(e => e.DateEcriture)
                .Take(50)
                .ToListAsync()
        };
    }

    public async Task<bool> NumeroExisteAsync(string numero, int? excludeId = null)
    {
        var q = _db.Comptes.Where(c => c.NumeroCompte == numero);
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<Compte> CreateAsync(CompteFormVM vm)
    {
        var c = new Compte
        {
            NumeroCompte = vm.NumeroCompte.Trim().ToUpperInvariant(),
            ClientId = vm.ClientId,
            TypeCompteId = vm.TypeCompteId,
            AgenceId = vm.AgenceId,
            Solde = vm.SoldeInitial,
            Devise = "BIF",
            Statut = "Actif",
            DateOuverture = DateTime.UtcNow
        };
        _db.Comptes.Add(c);
        await _db.SaveChangesAsync();
        return c;
    }

    public async Task<bool> UpdateAsync(CompteFormVM vm)
    {
        var c = await _db.Comptes.FindAsync(vm.Id);
        if (c is null) return false;
        c.NumeroCompte = vm.NumeroCompte.Trim().ToUpperInvariant();
        c.TypeCompteId = vm.TypeCompteId;
        c.AgenceId = vm.AgenceId;
        c.Statut = vm.Statut;
        if (vm.Statut == "Cloture") c.DateFermeture = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var c = await _db.Comptes.Include(x => x.Ecritures).FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return false;
        if (c.Ecritures.Any())
            throw new InvalidOperationException("Ce compte a des écritures comptables.");
        _db.Comptes.Remove(c);
        await _db.SaveChangesAsync();
        return true;
    }
}