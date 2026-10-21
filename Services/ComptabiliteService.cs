using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class ComptabiliteService
{
    private readonly AkarangaBankContext _db;
    private readonly ConformiteService _conformite;

    public ComptabiliteService(AkarangaBankContext db, ConformiteService conformite)
    {
        _db = db;
        _conformite = conformite;
    }

    // ============================================
    // DÉPÔT
    // ============================================
    public async Task<Operation> DeposerAsync(DepotVM vm, int utilisateurId)
    {
        var compte = await _db.Comptes.FindAsync(vm.CompteId)
            ?? throw new InvalidOperationException("Compte introuvable.");
        if (compte.Statut != "Actif")
            throw new InvalidOperationException($"Compte {compte.Statut}. Opération impossible.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var op = new Operation
            {
                Reference = GenererReference("AKR"),
                TypeOperation = "Depot",
                CompteId = compte.Id,
                UtilisateurId = utilisateurId,
                AgenceId = compte.AgenceId,
                Montant = vm.Montant,
                Devise = compte.Devise,
                Statut = "Valide",
                DateOperation = DateTime.UtcNow,
                Commentaire = vm.Commentaire
            };
            _db.Operations.Add(op);
            await _db.SaveChangesAsync();

            _db.Ecritures.Add(new Ecriture
            {
                OperationId = op.Id,
                CompteId = compte.Id,
                Sens = "Credit",
                Montant = vm.Montant,
                Devise = compte.Devise,
                Libelle = $"Dépôt sur {compte.NumeroCompte}"
            });
            compte.Solde += vm.Montant;
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            Log(utilisateurId, "Depot", "Operations", op.Id,
                $"{op.Montant:N0} BIF sur {compte.NumeroCompte}");
            await _db.SaveChangesAsync();

            await _conformite.AnalyserDepotAsync(op, compte);

            return op;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ============================================
    // RETRAIT
    // ============================================
    public async Task<Operation> RetirerAsync(RetraitVM vm, int utilisateurId)
    {
        var compte = await _db.Comptes.FindAsync(vm.CompteId)
            ?? throw new InvalidOperationException("Compte introuvable.");
        if (compte.Statut != "Actif")
            throw new InvalidOperationException($"Compte {compte.Statut}. Opération impossible.");
        if (compte.Solde < vm.Montant)
            throw new InvalidOperationException("Solde insuffisant.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var op = new Operation
            {
                Reference = GenererReference("AKR"),
                TypeOperation = "Retrait",
                CompteId = compte.Id,
                UtilisateurId = utilisateurId,
                AgenceId = compte.AgenceId,
                Montant = vm.Montant,
                Devise = compte.Devise,
                Statut = "Valide",
                DateOperation = DateTime.UtcNow,
                Commentaire = vm.Commentaire
            };
            _db.Operations.Add(op);
            await _db.SaveChangesAsync();

            _db.Ecritures.Add(new Ecriture
            {
                OperationId = op.Id,
                CompteId = compte.Id,
                Sens = "Debit",
                Montant = vm.Montant,
                Devise = compte.Devise,
                Libelle = $"Retrait sur {compte.NumeroCompte}"
            });
            compte.Solde -= vm.Montant;
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            Log(utilisateurId, "Retrait", "Operations", op.Id,
                $"{op.Montant:N0} BIF de {compte.NumeroCompte}");
            await _db.SaveChangesAsync();

            await _conformite.AnalyserRetraitAsync(op, compte);

            return op;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ============================================
    // VIREMENT (partie double)
    // ============================================
    public async Task<Operation> VirerAsync(VirementVM vm, int utilisateurId)
    {
        if (vm.CompteSourceId == vm.CompteDestinationId)
            throw new InvalidOperationException("Les comptes source et destination doivent être différents.");

        var source = await _db.Comptes.FindAsync(vm.CompteSourceId)
            ?? throw new InvalidOperationException("Compte source introuvable.");
        var dest = await _db.Comptes.FindAsync(vm.CompteDestinationId)
            ?? throw new InvalidOperationException("Compte destination introuvable.");

        if (source.Statut != "Actif") throw new InvalidOperationException($"Compte source {source.Statut}.");
        if (dest.Statut != "Actif") throw new InvalidOperationException($"Compte destination {dest.Statut}.");
        if (source.Devise != dest.Devise) throw new InvalidOperationException("Devises différentes.");
        if (source.Solde < vm.Montant) throw new InvalidOperationException("Solde insuffisant.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var reference = GenererReference("VIR");

            var virement = new Virement
            {
                Reference = reference,
                CompteSourceId = source.Id,
                CompteDestinationId = dest.Id,
                UtilisateurId = utilisateurId,
                Montant = vm.Montant,
                Devise = source.Devise,
                Statut = "Valide",
                Motif = vm.Motif
            };
            _db.Virements.Add(virement);
            await _db.SaveChangesAsync();

            var op = new Operation
            {
                Reference = reference,
                TypeOperation = "Virement",
                CompteId = source.Id,
                UtilisateurId = utilisateurId,
                AgenceId = source.AgenceId,
                VirementId = virement.Id,
                Montant = vm.Montant,
                Devise = source.Devise,
                Statut = "Valide",
                DateOperation = DateTime.UtcNow,
                Commentaire = vm.Motif
            };
            _db.Operations.Add(op);
            await _db.SaveChangesAsync();

            _db.Ecritures.AddRange(
                new Ecriture
                {
                    OperationId = op.Id,
                    CompteId = source.Id,
                    Sens = "Debit",
                    Montant = vm.Montant,
                    Devise = source.Devise,
                    Libelle = $"Virement vers {dest.NumeroCompte}"
                },
                new Ecriture
                {
                    OperationId = op.Id,
                    CompteId = dest.Id,
                    Sens = "Credit",
                    Montant = vm.Montant,
                    Devise = dest.Devise,
                    Libelle = $"Virement depuis {source.NumeroCompte}"
                }
            );

            source.Solde -= vm.Montant;
            dest.Solde += vm.Montant;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            Log(utilisateurId, "Virement", "Operations", op.Id,
                $"{op.Montant:N0} BIF de {source.NumeroCompte} vers {dest.NumeroCompte}");
            await _db.SaveChangesAsync();

            await _conformite.AnalyserVirementAsync(op, source);

            return op;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ============================================
    // CONSULTATION
    // ============================================
    public async Task<JournalVM> GetJournalAsync(DateTime? du, DateTime? au, string? type)
    {
        var q = _db.Operations.AsNoTracking()
            .Include(o => o.Compte)
            .AsQueryable();

        if (du.HasValue) q = q.Where(o => o.DateOperation >= du.Value);
        if (au.HasValue) q = q.Where(o => o.DateOperation <= au.Value);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(o => o.TypeOperation == type);

        return new JournalVM
        {
            Operations = await q.OrderByDescending(o => o.DateOperation).Take(200).ToListAsync(),
            Du = du,
            Au = au,
            Type = type
        };
    }

    public async Task<ReleveVM?> GetReleveAsync(int compteId)
    {
        var compte = await _db.Comptes.AsNoTracking()
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == compteId);
        if (compte is null) return null;

        var ecritures = await _db.Ecritures.AsNoTracking()
            .Include(e => e.Operation)
            .Where(e => e.CompteId == compteId)
            .OrderBy(e => e.DateEcriture)
            .ToListAsync();

        var totalCredits = ecritures.Where(e => e.Sens == "Credit").Sum(e => e.Montant);
        var totalDebits = ecritures.Where(e => e.Sens == "Debit").Sum(e => e.Montant);

        return new ReleveVM
        {
            Compte = compte,
            SoldeInitial = compte.Solde - totalCredits + totalDebits,
            SoldeFinal = compte.Solde,
            Ecritures = ecritures
        };
    }

    public async Task<bool> VerifierEquilibreAsync()
{
    var desequilibres = await _db.Ecritures
        .GroupBy(e => e.OperationId)
        .Select(g => new
        {
            OperationId = g.Key,
            NbEcritures = g.Count(),
            Diff = g.Sum(e => e.Sens == "Debit" ? e.Montant : -e.Montant)
        })
        .Where(x => x.NbEcritures >= 2 && x.Diff != 0)
        .ToListAsync();

    return desequilibres.Count == 0;
}



    // ============================================
    // HELPERS
    // ============================================
    private void Log(int userId, string action, string table, int recordId, string details)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UtilisateurId = userId == 0 ? null : userId,
            Action = action,
            TableConcerne = table,
            EnregistrementId = recordId,
            Details = details,
            DateEvenement = DateTime.UtcNow
        });
    }

    private static string GenererReference(string prefix)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var rnd = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"{prefix}-{date}-{rnd}";
    }
}