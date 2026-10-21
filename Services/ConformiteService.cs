using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class ConformiteService
{
    private readonly AkarangaBankContext _db;
    private const decimal SeuilAML = 5_000_000m;

    public ConformiteService(AkarangaBankContext db) => _db = db;

    public async Task AnalyserDepotAsync(Operation op, Compte compte)
    {
        if (op.Montant >= SeuilAML)
            await CreerAlerteAsync(compte.ClientId, op.Id, "DepassementSeuil",
                $"Dépôt de {op.Montant:N0} BIF ≥ seuil AML ({SeuilAML:N0} BIF).",
                op.Montant >= 10_000_000m ? "Eleve" : "Moyen");
    }

    public async Task AnalyserRetraitAsync(Operation op, Compte compte)
    {
        if (op.Montant >= SeuilAML)
            await CreerAlerteAsync(compte.ClientId, op.Id, "DepassementSeuil",
                $"Retrait de {op.Montant:N0} BIF ≥ seuil AML ({SeuilAML:N0} BIF).",
                op.Montant >= 10_000_000m ? "Eleve" : "Moyen");
    }

    public async Task AnalyserVirementAsync(Operation op, Compte source)
    {
        if (op.Montant >= SeuilAML)
            await CreerAlerteAsync(source.ClientId, op.Id, "DepassementSeuil",
                $"Virement de {op.Montant:N0} BIF ≥ seuil AML ({SeuilAML:N0} BIF).",
                op.Montant >= 10_000_000m ? "Eleve" : "Moyen");
    }

    private async Task CreerAlerteAsync(int clientId, int opId, string type, string desc, string niveau)
    {
        _db.AlertesAML.Add(new AlerteAML
        {
            ClientId = clientId,
            OperationId = opId,
            TypeAlerte = type,
            Niveau = niveau,
            Description = desc,
            Statut = "Ouverte",
            DateAlerte = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}