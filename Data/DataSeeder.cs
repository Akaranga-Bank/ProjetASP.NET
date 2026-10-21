using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<AkarangaBankContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();

        // ---- Rôles ----
        string[] roles = { "admin", "gestionnaire", "guichetier", "auditeur", "client" };
        foreach (var r in roles)
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new ApplicationRole { Name = r });

        // ---- Agences ----
        if (!context.Agences.Any())
        {
            context.Agences.AddRange(
                new Agence { Code = "AKR-BJM", Nom = "Akaranga Bank Bujumbura (Siège)", Ville = "Bujumbura", EstSiege = true },
                new Agence { Code = "AKR-GIT", Nom = "Akaranga Bank Gitega", Ville = "Gitega" },
                new Agence { Code = "AKR-NGO", Nom = "Akaranga Bank Ngozi", Ville = "Ngozi" },
                new Agence { Code = "AKR-RUM", Nom = "Akaranga Bank Rumonge", Ville = "Rumonge" }
            );
            await context.SaveChangesAsync();
        }

        // ---- Types de compte ----
        if (!context.TypesCompte.Any())
        {
            context.TypesCompte.AddRange(
                new TypeCompte { Code = "COURANT", Libelle = "Compte Courant", TauxInteret = 0m, SoldeMinimum = 0m },
                new TypeCompte { Code = "EPARGNE", Libelle = "Compte Épargne", TauxInteret = 3.5m, SoldeMinimum = 10000m },
                new TypeCompte { Code = "CREDIT", Libelle = "Compte Crédit", TauxInteret = 12m, SoldeMinimum = 0m, AutoriseDecouvert = true }
            );
            await context.SaveChangesAsync();
        }

        // ---- Paramètres ----
        if (!context.Parametres.Any())
        {
            context.Parametres.AddRange(
                new Parametre { Cle = "SEUIL_AML", Valeur = "5000000", Description = "Seuil AML en BIF" },
                new Parametre { Cle = "DEVISE_DEFAULT", Valeur = "BIF", Description = "Devise par défaut" }
            );
            await context.SaveChangesAsync();
        }

        var siege = context.Agences.First(a => a.EstSiege);

        // ---- Admin ----
        const string adminEmail = "admin@akaranga.bi";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                NomComplet = "Administrateur Système",
                Role = "admin",
                AgenceId = siege.Id
            };
            await userManager.CreateAsync(admin, "Admin@2025");
            await userManager.AddToRoleAsync(admin, "admin");
        }

        // ---- Clients de test ----
        if (!context.Clients.Any())
        {
            var clients = new[]
            {
                new Client { NumeroClient = "CLI-000001", Nom = "Nshimirimana", Prenom = "Emmanuel", DateNaissance = new DateTime(1985,3,12), NumeroCNI = "BJM-1234567", Telephone = "+25779123456", Email = "emmanuel@example.bi", AgenceId = siege.Id },
                new Client { NumeroClient = "CLI-000002", Nom = "Nduwimana", Prenom = "Marie", DateNaissance = new DateTime(1990,7,25), NumeroCNI = "BJM-2345678", Telephone = "+25779234567", Email = "marie@example.bi", AgenceId = siege.Id },
                new Client { NumeroClient = "CLI-000003", Nom = "Hategekimana", Prenom = "Joseph", DateNaissance = new DateTime(1978,11,8), NumeroCNI = "GIT-3456789", Telephone = "+25779345678", AgenceId = context.Agences.First(a => a.Code == "AKR-GIT").Id },
            };
            context.Clients.AddRange(clients);
            await context.SaveChangesAsync();

            // ---- Comptes de test ----
            var typeCourant = context.TypesCompte.First(t => t.Code == "COURANT");
            var typeEpargne = context.TypesCompte.First(t => t.Code == "EPARGNE");
            context.Comptes.AddRange(
                new Compte { NumeroCompte = "BJM-0001-0001", ClientId = clients[0].Id, TypeCompteId = typeCourant.Id, AgenceId = siege.Id, Solde = 150000m },
                new Compte { NumeroCompte = "BJM-0001-0002", ClientId = clients[0].Id, TypeCompteId = typeEpargne.Id, AgenceId = siege.Id, Solde = 1250000m },
                new Compte { NumeroCompte = "BJM-0002-0001", ClientId = clients[1].Id, TypeCompteId = typeEpargne.Id, AgenceId = siege.Id, Solde = 8500000m }
            );
            await context.SaveChangesAsync();

            // ---- Utilisateurs de test (guichetier, gestionnaire, auditeur) ----
            var guichetier = new ApplicationUser { UserName = "guichetier@akaranga.bi", Email = "guichetier@akaranga.bi", EmailConfirmed = true, NomComplet = "Niyo Jean", Role = "guichetier", AgenceId = siege.Id };
            await userManager.CreateAsync(guichetier, "Guichet@2025");
            await userManager.AddToRoleAsync(guichetier, "guichetier");

            var gestionnaire = new ApplicationUser { UserName = "gestionnaire@akaranga.bi", Email = "gestionnaire@akaranga.bi", EmailConfirmed = true, NomComplet = "Ndayishimiye Alice", Role = "gestionnaire", AgenceId = siege.Id };
            await userManager.CreateAsync(gestionnaire, "Gestion@2025");
            await userManager.AddToRoleAsync(gestionnaire, "gestionnaire");

            var auditeur = new ApplicationUser { UserName = "auditeur@akaranga.bi", Email = "auditeur@akaranga.bi", EmailConfirmed = true, NomComplet = "Bizimana Claudine", Role = "auditeur", AgenceId = siege.Id };
            await userManager.CreateAsync(auditeur, "Audit@2025");
            await userManager.AddToRoleAsync(auditeur, "auditeur");
        }
    }
}