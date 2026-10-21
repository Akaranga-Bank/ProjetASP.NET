using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class CompteListVM
{
    public IEnumerable<AkarangaBank.Web.Models.Compte> Comptes { get; set; }
        = new List<AkarangaBank.Web.Models.Compte>();
    public string? Recherche { get; set; }
    public int? AgenceId { get; set; }
    public int? TypeCompteId { get; set; }
    public string? Statut { get; set; }
    public IEnumerable<AkarangaBank.Web.Models.Agence> Agences { get; set; }
        = new List<AkarangaBank.Web.Models.Agence>();
    public IEnumerable<AkarangaBank.Web.Models.TypeCompte> Types { get; set; }
        = new List<AkarangaBank.Web.Models.TypeCompte>();
    public int Total => Comptes.Count();
}

public class CompteFormVM
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Numéro de compte")]
    [RegularExpression(@"^[A-Z]{3}-\d{4}-\d{4}$", ErrorMessage = "Format : XXX-NNNN-NNNN.")]
    public string NumeroCompte { get; set; } = "";

    [Required]
    [Display(Name = "Client")]
    public int ClientId { get; set; }

    [Required]
    [Display(Name = "Type de compte")]
    public int TypeCompteId { get; set; }

    [Required]
    [Display(Name = "Agence")]
    public int AgenceId { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Solde initial (BIF)")]
    public decimal SoldeInitial { get; set; } = 0m;

    [StringLength(20)]
    public string Statut { get; set; } = "Actif";

    public string? ClientNom { get; set; }
    public decimal Solde { get; set; }
    public string Devise { get; set; } = "BIF";
}

public class CompteDetailsVM
{
    public AkarangaBank.Web.Models.Compte Compte { get; set; } = null!;
    public IEnumerable<AkarangaBank.Web.Models.Ecriture> Ecritures { get; set; }
        = new List<AkarangaBank.Web.Models.Ecriture>();
}