using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AkarangaBank.Web.Models;

// ============================================
// BLOC 1 - RÉFÉRENTIEL
// ============================================

public class Agence
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string Code { get; set; } = "";
    [Required, MaxLength(100)] public string Nom { get; set; } = "";
    [Required, MaxLength(50)] public string Ville { get; set; } = "";
    public bool EstSiege { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }
}

public class Parametre
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string Cle { get; set; } = "";
    [Required, MaxLength(255)] public string Valeur { get; set; } = "";
    [MaxLength(255)] public string? Description { get; set; }
    public int? AgenceId { get; set; }
    public Agence? Agence { get; set; }
    public DateTime DateModification { get; set; } = DateTime.UtcNow;
}

// ============================================
// BLOC 2 - CLIENTS ET KYC
// ============================================

public class Client
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string NumeroClient { get; set; } = "";
    [Required, MaxLength(100)] public string Nom { get; set; } = "";
    [Required, MaxLength(100)] public string Prenom { get; set; } = "";
    public DateTime DateNaissance { get; set; }
    [Required, MaxLength(30)] public string NumeroCNI { get; set; } = "";
    [MaxLength(20)] public string? Telephone { get; set; }
    [MaxLength(100)] public string? Email { get; set; }
    [MaxLength(255)] public string? Adresse { get; set; }
    public int AgenceId { get; set; }
    public Agence Agence { get; set; } = null!;
    [MaxLength(20)] public string Statut { get; set; } = "Actif";
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }
    public ICollection<Compte> Comptes { get; set; } = new List<Compte>();
}

public class VerificationKYC
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    [MaxLength(50)] public string TypeDocument { get; set; } = "CNI";
    [MaxLength(50)] public string NumeroDocument { get; set; } = "";
    [MaxLength(500)] public string? CheminDocument { get; set; }
    [MaxLength(20)] public string Statut { get; set; } = "EnAttente";
    public int? VerifieParId { get; set; }
    public DateTime? DateVerification { get; set; }
    [MaxLength(500)] public string? Commentaire { get; set; }
}

// ============================================
// BLOC 3 - COMPTES ET CARTES
// ============================================

public class TypeCompte
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string Code { get; set; } = "";
    [Required, MaxLength(50)] public string Libelle { get; set; } = "";
    public decimal TauxInteret { get; set; }
    public decimal SoldeMinimum { get; set; }
    public bool AutoriseDecouvert { get; set; }
}

public class Compte
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string NumeroCompte { get; set; } = "";
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public int TypeCompteId { get; set; }
    public TypeCompte TypeCompte { get; set; } = null!;
    public int AgenceId { get; set; }
    public Agence Agence { get; set; } = null!;
    public decimal Solde { get; set; }
    [MaxLength(3)] public string Devise { get; set; } = "BIF";
    [MaxLength(20)] public string Statut { get; set; } = "Actif";
    public DateTime DateOuverture { get; set; } = DateTime.UtcNow;
    public DateTime? DateFermeture { get; set; }
    public ICollection<Ecriture> Ecritures { get; set; } = new List<Ecriture>();
}

public class Carte
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string NumeroCarte { get; set; } = "";
    public int CompteId { get; set; }
    public Compte Compte { get; set; } = null!;
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    [MaxLength(20)] public string TypeCarte { get; set; } = "Visa";
    public DateTime DateExpiration { get; set; }
    [MaxLength(20)] public string Statut { get; set; } = "Active";
}

// ============================================
// BLOC 4 - PRÊTS
// ============================================

public class Pret
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string Reference { get; set; } = "";
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public int CompteDebitId { get; set; }
    public Compte CompteDebit { get; set; } = null!;
    public int? ValideParId { get; set; }
    public decimal MontantPrincipal { get; set; }
    public decimal TauxInteret { get; set; }
    public int DureeMois { get; set; }
    public decimal Mensualite { get; set; }
    [MaxLength(20)] public string Statut { get; set; } = "EnAttente";
    public DateTime DateDemande { get; set; } = DateTime.UtcNow;
    public DateTime? DateValidation { get; set; }
    public ICollection<EcheancePret> Echeances { get; set; } = new List<EcheancePret>();
}

public class EcheancePret
{
    public int Id { get; set; }
    public int PretId { get; set; }
    public Pret Pret { get; set; } = null!;
    public int NumeroEcheance { get; set; }
    public decimal Capital { get; set; }
    public decimal Interet { get; set; }
    public decimal MontantTotal { get; set; }
    public DateTime DateEcheance { get; set; }
    public DateTime? DatePaiement { get; set; }
    [MaxLength(20)] public string Statut { get; set; } = "EnAttente";
}

// ============================================
// BLOC 5 - VIREMENTS, OPÉRATIONS, ÉCRITURES
// ============================================

public class Virement
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string Reference { get; set; } = "";
    public int CompteSourceId { get; set; }
    public Compte CompteSource { get; set; } = null!;
    public int CompteDestinationId { get; set; }
    public Compte CompteDestination { get; set; } = null!;
    public int UtilisateurId { get; set; }
    [MaxLength(255)] public string? Motif { get; set; }
    public decimal Montant { get; set; }
    [MaxLength(3)] public string Devise { get; set; } = "BIF";
    [MaxLength(20)] public string Statut { get; set; } = "Valide";
    public bool ControleAML { get; set; }
    public DateTime DateVirement { get; set; } = DateTime.UtcNow;
}

public class Operation
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string Reference { get; set; } = "";
    [Required, MaxLength(30)] public string TypeOperation { get; set; } = "";
    public int CompteId { get; set; }
    public Compte Compte { get; set; } = null!;
    public int UtilisateurId { get; set; }
    public int AgenceId { get; set; }
    public Agence Agence { get; set; } = null!;
    public int? VirementId { get; set; }
    public decimal Montant { get; set; }
    [MaxLength(3)] public string Devise { get; set; } = "BIF";
    [MaxLength(20)] public string Statut { get; set; } = "Valide";
    public DateTime DateOperation { get; set; } = DateTime.UtcNow;
    [MaxLength(255)] public string? Commentaire { get; set; }
    public ICollection<Ecriture> Ecritures { get; set; } = new List<Ecriture>();
}

public class Ecriture
{
    public int Id { get; set; }
    public int OperationId { get; set; }
    public Operation Operation { get; set; } = null!;
    public int CompteId { get; set; }
    public Compte Compte { get; set; } = null!;
    [Required, MaxLength(10)] public string Sens { get; set; } = ""; // Debit / Credit
    public decimal Montant { get; set; }
    [MaxLength(3)] public string Devise { get; set; } = "BIF";
    public DateTime DateEcriture { get; set; } = DateTime.UtcNow;
    [MaxLength(255)] public string? Libelle { get; set; }
}

// ============================================
// BLOC 6 - AUDIT, AML, SESSIONS
// ============================================

public class AuditLog
{
    public int Id { get; set; }
    public DateTime DateEvenement { get; set; } = DateTime.UtcNow;
    public int? UtilisateurId { get; set; }
    public int? AgenceId { get; set; }
    [MaxLength(50)] public string Action { get; set; } = "";
    [MaxLength(50)] public string? TableConcerne { get; set; }
    public int? EnregistrementId { get; set; }
    public string? ValeurAvant { get; set; }
    public string? ValeurApres { get; set; }
    [MaxLength(45)] public string? AdresseIP { get; set; }
    [MaxLength(255)] public string? UserAgent { get; set; }
    public string? Details { get; set; }
}

public class AlerteAML
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public int? OperationId { get; set; }
    public Operation? Operation { get; set; }
    [MaxLength(50)] public string TypeAlerte { get; set; } = "";
    [MaxLength(20)] public string Niveau { get; set; } = "Moyen";
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(20)] public string Statut { get; set; } = "Ouverte";
    public int? TraiteParId { get; set; }
    public DateTime DateAlerte { get; set; } = DateTime.UtcNow;
    public DateTime? DateTraitement { get; set; }
}

public class SessionUtilisateur
{
    public int Id { get; set; }
    public int UtilisateurId { get; set; }
    [Required, MaxLength(255)] public string Token { get; set; } = "";
    [MaxLength(45)] public string? AdresseIP { get; set; }
    [MaxLength(255)] public string? UserAgent { get; set; }
    public bool DeuxFA_Valide { get; set; }
    public DateTime DateConnexion { get; set; } = DateTime.UtcNow;
    public DateTime DateExpiration { get; set; }
    public bool EstActive { get; set; } = true;
}





public class ApplicationUser : IdentityUser<int>
{
    [MaxLength(100)] public string NomComplet { get; set; } = "";
    [MaxLength(20)] public string Role { get; set; } = "client";
    public int AgenceId { get; set; }
    public Agence? Agence { get; set; }
    public bool DeuxFA_Active { get; set; }
    [MaxLength(255)] public string? DeuxFA_Secret { get; set; }
    public DateTime? DerniereConnexion { get; set; }
    public bool EstActif { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}

public class ApplicationRole : IdentityRole<int> { }