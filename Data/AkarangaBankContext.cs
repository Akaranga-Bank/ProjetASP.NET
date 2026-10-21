using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Data;

public class AkarangaBankContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public AkarangaBankContext(DbContextOptions<AkarangaBankContext> options)
        : base(options) { }

    // Bloc 1
    public DbSet<Agence> Agences => Set<Agence>();
    public DbSet<Parametre> Parametres => Set<Parametre>();

    // Bloc 2
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<VerificationKYC> VerificationsKYC => Set<VerificationKYC>();

    // Bloc 3
    public DbSet<TypeCompte> TypesCompte => Set<TypeCompte>();
    public DbSet<Compte> Comptes => Set<Compte>();
    public DbSet<Carte> Cartes => Set<Carte>();

    // Bloc 4
    public DbSet<Pret> Prets => Set<Pret>();
    public DbSet<EcheancePret> EcheancesPret => Set<EcheancePret>();

    // Bloc 5
    public DbSet<Virement> Virements => Set<Virement>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<Ecriture> Ecritures => Set<Ecriture>();

    // Bloc 6
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AlerteAML> AlertesAML => Set<AlerteAML>();
    public DbSet<SessionUtilisateur> SessionsUtilisateur => Set<SessionUtilisateur>();

    // Alias pratique pour le code métier
    public DbSet<ApplicationUser> Utilisateurs => Set<ApplicationUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // ---------- RENOMMAGE DES TABLES IDENTITY ----------
        b.Entity<ApplicationUser>(e =>
        {
            e.ToTable("Utilisateurs");
            e.Property(u => u.NomComplet).HasMaxLength(100).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).IsRequired();
            e.HasOne(u => u.Agence).WithMany()
                .HasForeignKey(u => u.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        b.Entity<ApplicationRole>(e => e.ToTable("Roles"));
        b.Entity<IdentityUserRole<int>>(e => e.ToTable("UtilisateursRoles"));
        b.Entity<IdentityUserClaim<int>>(e => e.ToTable("UtilisateursClaims"));
        b.Entity<IdentityUserLogin<int>>(e => e.ToTable("UtilisateursLogins"));
        b.Entity<IdentityRoleClaim<int>>(e => e.ToTable("RolesClaims"));
        b.Entity<IdentityUserToken<int>>(e => e.ToTable("UtilisateursTokens"));

        // ---------- AGENCE ----------
        b.Entity<Agence>(e =>
        {
            e.ToTable("Agences");
            e.HasIndex(a => a.Code).IsUnique();
        });

        // ---------- PARAMETRE ----------
        b.Entity<Parametre>(e =>
        {
            e.ToTable("Parametres");
            e.HasIndex(p => new { p.Cle, p.AgenceId }).IsUnique();
            e.HasOne(p => p.Agence).WithMany()
                .HasForeignKey(p => p.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- CLIENT ----------
        b.Entity<Client>(e =>
        {
            e.ToTable("Clients");
            e.HasIndex(c => c.NumeroClient).IsUnique();
            e.HasIndex(c => c.NumeroCNI).IsUnique();
            e.Property(c => c.Statut).HasDefaultValue("Actif");
            e.HasOne(c => c.Agence).WithMany()
                .HasForeignKey(c => c.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- KYC ----------
        b.Entity<VerificationKYC>(e =>
        {
            e.ToTable("VerificationsKYC");
            e.Property(v => v.Statut).HasDefaultValue("EnAttente");
            e.HasOne(v => v.Client).WithMany()
                .HasForeignKey(v => v.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- TYPE COMPTE ----------
        b.Entity<TypeCompte>(e =>
        {
            e.ToTable("TypesCompte");
            e.HasIndex(t => t.Code).IsUnique();
            e.Property(t => t.TauxInteret).HasColumnType("decimal(5,2)");
            e.Property(t => t.SoldeMinimum).HasColumnType("decimal(18,2)");
        });

        // ---------- COMPTE ----------
        b.Entity<Compte>(e =>
        {
            e.ToTable("Comptes");
            e.HasIndex(c => c.NumeroCompte).IsUnique();
            e.Property(c => c.Solde).HasColumnType("decimal(18,2)");
            e.Property(c => c.Devise).HasDefaultValue("BIF");
            e.Property(c => c.Statut).HasDefaultValue("Actif");
            e.HasOne(c => c.Client).WithMany(cl => cl.Comptes)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.TypeCompte).WithMany()
                .HasForeignKey(c => c.TypeCompteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Agence).WithMany()
                .HasForeignKey(c => c.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- CARTE ----------
        b.Entity<Carte>(e =>
        {
            e.ToTable("Cartes");
            e.HasIndex(c => c.NumeroCarte).IsUnique();
            e.HasOne(c => c.Compte).WithMany()
                .HasForeignKey(c => c.CompteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Client).WithMany()
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- PRET ----------
        b.Entity<Pret>(e =>
        {
            e.ToTable("Prets");
            e.HasIndex(p => p.Reference).IsUnique();
            e.Property(p => p.MontantPrincipal).HasColumnType("decimal(18,2)");
            e.Property(p => p.TauxInteret).HasColumnType("decimal(5,2)");
            e.Property(p => p.Mensualite).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.Client).WithMany()
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.CompteDebit).WithMany()
                .HasForeignKey(p => p.CompteDebitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- ECHEANCE ----------
        b.Entity<EcheancePret>(e =>
        {
            e.ToTable("EcheancesPret");
            e.HasIndex(x => new { x.PretId, x.NumeroEcheance }).IsUnique();
            e.Property(x => x.Capital).HasColumnType("decimal(18,2)");
            e.Property(x => x.Interet).HasColumnType("decimal(18,2)");
            e.Property(x => x.MontantTotal).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Pret).WithMany(p => p.Echeances)
                .HasForeignKey(x => x.PretId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- VIREMENT ----------
        b.Entity<Virement>(e =>
        {
            e.ToTable("Virements");
            e.HasIndex(v => v.Reference).IsUnique();
            e.Property(v => v.Montant).HasColumnType("decimal(18,2)");
            e.HasOne(v => v.CompteSource).WithMany()
                .HasForeignKey(v => v.CompteSourceId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.CompteDestination).WithMany()
                .HasForeignKey(v => v.CompteDestinationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- OPERATION ----------
        b.Entity<Operation>(e =>
        {
            e.ToTable("Operations");
            e.HasIndex(o => o.Reference).IsUnique();
            e.Property(o => o.Montant).HasColumnType("decimal(18,2)");
            e.HasOne(o => o.Compte).WithMany()
                .HasForeignKey(o => o.CompteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Agence).WithMany()
                .HasForeignKey(o => o.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- ECRITURE ----------
        b.Entity<Ecriture>(e =>
        {
            e.ToTable("Ecritures");
            e.Property(x => x.Montant).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Operation).WithMany(o => o.Ecritures)
                .HasForeignKey(x => x.OperationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Compte).WithMany(c => c.Ecritures)
                .HasForeignKey(x => x.CompteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- AUDIT ----------
        b.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLog");
            e.HasIndex(a => a.DateEvenement);
        });

        // ---------- AML ----------
        b.Entity<AlerteAML>(e =>
        {
            e.ToTable("AlertesAML");
            e.Property(a => a.Niveau).HasDefaultValue("Moyen");
            e.Property(a => a.Statut).HasDefaultValue("Ouverte");
            e.HasOne(a => a.Client).WithMany()
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Operation).WithMany()
                .HasForeignKey(a => a.OperationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ---------- SESSION ----------
        b.Entity<SessionUtilisateur>(e =>
        {
            e.ToTable("SessionsUtilisateur");
            e.HasIndex(s => s.Token).IsUnique();
        });
    }
}