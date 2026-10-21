using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class ClientListVM
{
    public IEnumerable<AkarangaBank.Web.Models.Client> Clients { get; set; }
        = new List<AkarangaBank.Web.Models.Client>();
    public string? Recherche { get; set; }
    public int? AgenceId { get; set; }
    public string? Statut { get; set; }
    public IEnumerable<AkarangaBank.Web.Models.Agence> Agences { get; set; }
        = new List<AkarangaBank.Web.Models.Agence>();
    public int Total => Clients.Count();
}

public class ClientFormVM
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Numéro client")]
    [RegularExpression(@"^CLI-\d{6}$", ErrorMessage = "Format : CLI-NNNNNN.")]
    public string NumeroClient { get; set; } = "";

    [Required, StringLength(100)]
    public string Nom { get; set; } = "";

    [Required, StringLength(100)]
    public string Prenom { get; set; } = "";

    [Required, DataType(DataType.Date)]
    [Display(Name = "Date de naissance")]
    public DateTime DateNaissance { get; set; } = DateTime.Today.AddYears(-18);

    [Required, StringLength(30)]
    [Display(Name = "Numéro CNI")]
    public string NumeroCNI { get; set; } = "";

    [Phone, StringLength(20)]
    public string? Telephone { get; set; }

    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Adresse { get; set; }

    [Required]
    [Display(Name = "Agence")]
    public int AgenceId { get; set; }

    [StringLength(20)]
    public string Statut { get; set; } = "Actif";
}

public class ClientDetailsVM
{
    public AkarangaBank.Web.Models.Client Client { get; set; } = null!;
    public IEnumerable<AkarangaBank.Web.Models.Compte> Comptes { get; set; }
        = new List<AkarangaBank.Web.Models.Compte>();
    public IEnumerable<AkarangaBank.Web.Models.VerificationKYC> Verifications { get; set; }
        = new List<AkarangaBank.Web.Models.VerificationKYC>();
    public decimal SoldeTotal => Comptes.Sum(c => c.Solde);
}