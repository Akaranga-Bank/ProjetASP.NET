using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class DepotVM
{
    [Required, Display(Name = "Compte")]
    public int CompteId { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être positif.")]
    public decimal Montant { get; set; }

    [StringLength(255)]
    public string? Commentaire { get; set; }
}

public class RetraitVM
{
    [Required, Display(Name = "Compte")]
    public int CompteId { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être positif.")]
    public decimal Montant { get; set; }

    [StringLength(255)]
    public string? Commentaire { get; set; }
}

public class VirementVM
{
    [Required, Display(Name = "Compte source")]
    public int CompteSourceId { get; set; }

    [Required, Display(Name = "Compte destination")]
    public int CompteDestinationId { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être positif.")]
    public decimal Montant { get; set; }

    [StringLength(255)]
    public string? Motif { get; set; }
}

public class JournalVM
{
    public IEnumerable<AkarangaBank.Web.Models.Operation> Operations { get; set; }
        = new List<AkarangaBank.Web.Models.Operation>();
    public DateTime? Du { get; set; }
    public DateTime? Au { get; set; }
    public string? Type { get; set; }
    public int Total => Operations.Count();
}

public class ReleveVM
{
    public AkarangaBank.Web.Models.Compte Compte { get; set; } = null!;
    public decimal SoldeInitial { get; set; }
    public decimal SoldeFinal { get; set; }
    public IEnumerable<AkarangaBank.Web.Models.Ecriture> Ecritures { get; set; }
        = new List<AkarangaBank.Web.Models.Ecriture>();
}