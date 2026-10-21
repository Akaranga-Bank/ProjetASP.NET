Les membres du groupe

1. BIZIMANA Samo Didier
2. ABIMANA Mediateur


# 🏦 Akaranga Bank

Application de gestion bancaire développée dans le cadre du cours **Développement Web avec ASP.NET Core** (BAC+3 Génie Logiciel — Université Polytechnique de Gitega).

---

## 📋 Description

Application bancaire complète couvrant :

- Authentification sécurisée avec **5 rôles** distincts
- Gestion des **clients** avec vérifications **KYC**
- Gestion des **comptes** bancaires
- **Opérations** : dépôt, retrait, virement
- **Comptabilité en partie double** (débit = crédit)
- **Conformité AML** : seuil 5 000 000 BIF + alertes automatiques
- **Piste d'audit** : traçabilité automatique des actions
- **Espace client** : consultation et virement personnel

---

## 🛠️ Technologies

| Technologie | Version |
|-------------|---------|
| .NET | 10.0 |
| ASP.NET Core MVC | 10.0 |
| Entity Framework Core | 10.0 |
| Base de données | SQLite (`akaranga.db`) |
| Authentification | ASP.NET Core Identity |
| Frontend | Bootstrap 5 + Razor |

---

## 🚀 Lancement

```bash
cd C:\AkarangaBank
dotnet restore
dotnet build
cd AkarangaBank.Web
dotnet run
La base akaranga.db est créée automatiquement au premier lancement avec les données de test.

Puis ouvrir : https://localhost:XXXX

👥 Comptes de test
Rôle	Email	Mot de passe
Administrateur	admin@akaranga.bi	Admin@2025
Gestionnaire	gestionnaire@akaranga.bi	Gestion@2025
Guichetier	guichetier@akaranga.bi	Guichet@2025
Auditeur	auditeur@akaranga.bi	Audit@2025
Client	emmanuel@example.bi	Client@2025
🎭 Rôles et permissions
Rôle	Permissions
Admin	Gestion complète : agences, clients, comptes, utilisateurs, AML, audit
Gestionnaire	Clients + comptes + validation KYC + traitement AML
Guichetier	Dépôt, retrait, virement
Auditeur	Lecture seule : audit, AML, journal
Client	Ses comptes + virement personnel
📸 Aperçu de l'application
1. Page de connexion
https://docs/01-login.png

2. Tableau de bord administrateur
https://docs/02-dashboard-admin.png

3. Gestion des clients
https://docs/03-clients.png

4. Virement bancaire
https://docs/04-virement.png

5. Alertes AML (anti-blanchiment)
https://docs/05-alertes-aml.png

6. Piste d'audit
https://docs/06-audit.png

🏗️ Architecture
Pattern MVC avec couche Service :

text
Controller → Service → DbContext → Base de données
    ↓
ViewModel → View
Dossiers principaux :

Controllers/ : 11 contrôleurs

Services/ : 5 services métier

Models/ : 16 entités + Identity

Views/ : vues Razor avec sidebar fixe

Data/ : DbContext + Seed

💰 Principes bancaires
Comptabilité en partie double
Chaque opération génère au moins 2 écritures : total débit = total crédit.

Atomicité (ACID)
Transactions EF Core : si une étape échoue → rollback complet.

Conformité AML
Seuil : 5 000 000 BIF

Niveaux : Moyen (5M–10M) / Élevé (≥ 10M)

Piste d'audit
Chaque action sensible est tracée : qui, quand, quoi.

🔮 Améliorations futures
Internationalisation (kirundi, anglais, swahili)

Accessibilité WCAG 2.1 AA

Prêts et échéanciers

Cartes bancaires

API REST mobile

Notifications email/SMS

Génération PDF

👨‍🎓 Auteur
Projet académique — Université Polytechnique de Gitega
Cours : Développement Web avec ASP.NET Core
Année : 2025 – 2026
