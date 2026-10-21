using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class AgenceListVM
{
    public IEnumerable<AkarangaBank.Web.Models.Agence> Agences { get; set; }
        = new List<AkarangaBank.Web.Models.Agence>();
    public string? Recherche { get; set; }
    public int Total => Agences.Count();
}

public class AgenceFormVM
{
    public int Id { get; set; }

    [Required, StringLength(20, MinimumLength = 3)]
    [Display(Name = "Code agence")]
    [RegularExpression(@"^AKR-[A-Z]{3}$", ErrorMessage = "Format attendu : AKR-XXX (3 lettres majuscules).")]
    public string Code { get; set; } = "";

    [Required, StringLength(100)]
    public string Nom { get; set; } = "";

    [Required, StringLength(50)]
    public string Ville { get; set; } = "";

    [Display(Name = "Est le siège")]
    public bool EstSiege { get; set; }
}