using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class ClientService
{
    private readonly AkarangaBankContext _db;
    public ClientService(AkarangaBankContext db) => _db = db;

    public async Task<ClientListVM> GetListAsync(string? recherche, int? agenceId, string? statut)
    {
        var q = _db.Clients.AsNoTracking().Include(c => c.Agence).AsQueryable();

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var r = recherche.Trim();
            q = q.Where(c => c.NumeroClient.Contains(r) || c.Nom.Contains(r)
                          || c.Prenom.Contains(r) || c.NumeroCNI.Contains(r));
        }
        if (agenceId.HasValue) q = q.Where(c => c.AgenceId == agenceId.Value);
        if (!string.IsNullOrWhiteSpace(statut)) q = q.Where(c => c.Statut == statut);

        return new ClientListVM
        {
            Clients = await q.OrderBy(c => c.Nom).ThenBy(c => c.Prenom).ToListAsync(),
            Recherche = recherche,
            AgenceId = agenceId,
            Statut = statut,
            Agences = await _db.Agences.AsNoTracking().OrderBy(a => a.Code).ToListAsync()
        };
    }

    public async Task<Client?> GetByIdAsync(int id)
        => await _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<ClientDetailsVM?> GetDetailsAsync(int id)
    {
        var c = await _db.Clients.AsNoTracking().Include(x => x.Agence)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return null;

        return new ClientDetailsVM
        {
            Client = c,
            Comptes = await _db.Comptes.AsNoTracking().Include(x => x.TypeCompte)
                .Where(x => x.ClientId == id).ToListAsync(),
            Verifications = await _db.VerificationsKYC.AsNoTracking()
                .Where(v => v.ClientId == id).OrderByDescending(v => v.Id).ToListAsync()
        };
    }

    public async Task<bool> NumeroExisteAsync(string num, int? excludeId = null)
    {
        var q = _db.Clients.Where(c => c.NumeroClient == num);
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<bool> CniExisteAsync(string cni, int? excludeId = null)
    {
        var q = _db.Clients.Where(c => c.NumeroCNI == cni);
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<Client> CreateAsync(ClientFormVM vm)
    {
        var c = new Client
        {
            NumeroClient = vm.NumeroClient.Trim().ToUpperInvariant(),
            Nom = vm.Nom.Trim(),
            Prenom = vm.Prenom.Trim(),
            DateNaissance = vm.DateNaissance,
            NumeroCNI = vm.NumeroCNI.Trim().ToUpperInvariant(),
            Telephone = vm.Telephone?.Trim(),
            Email = vm.Email?.Trim(),
            Adresse = vm.Adresse?.Trim(),
            AgenceId = vm.AgenceId,
            Statut = "Actif",
            DateCreation = DateTime.UtcNow
        };
        _db.Clients.Add(c);
        await _db.SaveChangesAsync();
        return c;
    }

    public async Task<bool> UpdateAsync(ClientFormVM vm)
    {
        var c = await _db.Clients.FindAsync(vm.Id);
        if (c is null) return false;
        c.NumeroClient = vm.NumeroClient.Trim().ToUpperInvariant();
        c.Nom = vm.Nom.Trim();
        c.Prenom = vm.Prenom.Trim();
        c.DateNaissance = vm.DateNaissance;
        c.NumeroCNI = vm.NumeroCNI.Trim().ToUpperInvariant();
        c.Telephone = vm.Telephone?.Trim();
        c.Email = vm.Email?.Trim();
        c.Adresse = vm.Adresse?.Trim();
        c.AgenceId = vm.AgenceId;
        c.Statut = vm.Statut;
        c.DateModification = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var c = await _db.Clients.Include(x => x.Comptes).FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return false;
        if (c.Comptes.Any())
            throw new InvalidOperationException("Ce client possède des comptes bancaires.");

        var kycs = await _db.VerificationsKYC.Where(k => k.ClientId == id).ToListAsync();
        _db.VerificationsKYC.RemoveRange(kycs);
        _db.Clients.Remove(c);
        await _db.SaveChangesAsync();
        return true;
    }
}