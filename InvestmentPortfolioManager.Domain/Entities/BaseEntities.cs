// Entités du domaine pour le système de gestion de portefeuille d'investissement
// Domain entities for the investment portfolio management system
// Base entities and value objects representing core business concepts
// Entités de base et objets de valeur représentant les concepts métier principaux

using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Domain.Entities
{
    /// <summary>
    /// Base entity class with common properties
    /// Classe d'entité de base avec des propriétés communes
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
        public bool IsActive { get; protected set; } = true;

        /// <summary>
        /// Updates the timestamp when entity is modified
        /// Met à jour l'horodatage lorsque l'entité est modifiée
        /// </summary>
        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks entity as inactive (soft delete)
        /// Marque l'entité comme inactive (suppression logique)
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            UpdateTimestamp();
        }

        /// <summary>
        /// Reactivates the entity
        /// Réactive l'entité
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// User entity representing system users
    /// Entité utilisateur représentant les utilisateurs du système
    /// </summary>
    public class User : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Username { get; private set; } = string.Empty;

        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; private set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FirstName { get; private set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; private set; } = string.Empty;

        [Required, MaxLength(500)]
        public string PasswordHash { get; private set; } = string.Empty;

        [Required]
        public UserRole Role { get; private set; }

        [Required, MaxLength(2)]
        public string PreferredLanguage { get; private set; } = "EN";

        public string FullName => $"{FirstName} {LastName}";
        public string FullNameFrench => $"{FirstName} {LastName}";

        // Navigation properties / Propriétés de navigation
        public virtual ICollection<Portfolio> ManagedPortfolios { get; private set; } = new List<Portfolio>();

        protected User() { } // For EF Core

        public User(string username, string email, string firstName, string lastName, 
                   string passwordHash, UserRole role, string preferredLanguage = "EN")
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            Role = role;
            PreferredLanguage = preferredLanguage;
        }

        /// <summary>
        /// Updates user profile information
        /// Met à jour les informations de profil utilisateur
        /// </summary>
        public void UpdateProfile(string firstName, string lastName, string email, string preferredLanguage)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PreferredLanguage = preferredLanguage;
            UpdateTimestamp();
        }

        /// <summary>
        /// Changes user role
        /// Modifie le rôle de l'utilisateur
        /// </summary>
        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// User roles in the system
    /// Rôles d'utilisateur dans le système
    /// </summary>
    public enum UserRole
    {
        Admin,              // Administrateur
        PortfolioManager,   // Gestionnaire de portefeuille
        Analyst,            // Analyste
        ReadOnly            // Lecture seule
    }

    /// <summary>
    /// Currency entity
    /// Entité devise
    /// </summary>
    public class Currency : BaseEntity
    {
        [Required, MaxLength(3)]
        public string Code { get; private set; } = string.Empty; // ISO 4217

        [Required, MaxLength(50)]
        public string NameEN { get; private set; } = string.Empty;

        [Required, MaxLength(50)]
        public string NameFR { get; private set; } = string.Empty;

        [MaxLength(5)]
        public string Symbol { get; private set; } = string.Empty;

        protected Currency() { } // For EF Core

        public Currency(string code, string nameEN, string nameFR, string symbol)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            NameEN = nameEN ?? throw new ArgumentNullException(nameof(nameEN));
            NameFR = nameFR ?? throw new ArgumentNullException(nameof(nameFR));
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        }

        /// <summary>
        /// Gets localized name based on language
        /// Obtient le nom localisé selon la langue
        /// </summary>
        public string GetLocalizedName(string language) => language?.ToUpper() == "FR" ? NameFR : NameEN;
    }

    /// <summary>
    /// Country entity
    /// Entité pays
    /// </summary>
    public class Country : BaseEntity
    {
        [Required, MaxLength(2)]
        public string Code { get; private set; } = string.Empty; // ISO 3166-1 alpha-2

        [Required, MaxLength(100)]
        public string NameEN { get; private set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameFR { get; private set; } = string.Empty;

        [MaxLength(50)]
        public string Region { get; private set; } = string.Empty;

        protected Country() { } // For EF Core

        public Country(string code, string nameEN, string nameFR, string region)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            NameEN = nameEN ?? throw new ArgumentNullException(nameof(nameEN));
            NameFR = nameFR ?? throw new ArgumentNullException(nameof(nameFR));
            Region = region ?? string.Empty;
        }

        /// <summary>
        /// Gets localized name based on language
        /// Obtient le nom localisé selon la langue
        /// </summary>
        public string GetLocalizedName(string language) => language?.ToUpper() == "FR" ? NameFR : NameEN;
    }

    /// <summary>
    /// Asset class entity
    /// Entité classe d'actifs
    /// </summary>
    public class AssetClass : BaseEntity
    {
        [Required, MaxLength(10)]
        public string Code { get; private set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameEN { get; private set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameFR { get; private set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; private set; } = string.Empty;

        // Navigation properties / Propriétés de navigation
        public virtual ICollection<Security> Securities { get; private set; } = new List<Security>();

        protected AssetClass() { } // For EF Core

        public AssetClass(string code, string nameEN, string nameFR, string description = "")
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            NameEN = nameEN ?? throw new ArgumentNullException(nameof(nameEN));
            NameFR = nameFR ?? throw new ArgumentNullException(nameof(nameFR));
            Description = description;
        }

        /// <summary>
        /// Gets localized name based on language
        /// Obtient le nom localisé selon la langue
        /// </summary>
        public string GetLocalizedName(string language) => language?.ToUpper() == "FR" ? NameFR : NameEN;
    }

    /// <summary>
    /// Sector entity
    /// Entité secteur
    /// </summary>
    public class Sector : BaseEntity
    {
        [Required, MaxLength(10)]
        public string Code { get; private set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameEN { get; private set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameFR { get; private set; } = string.Empty;

        // Navigation properties / Propriétés de navigation
        public virtual ICollection<Security> Securities { get; private set; } = new List<Security>();

        protected Sector() { } // For EF Core

        public Sector(string code, string nameEN, string nameFR)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            NameEN = nameEN ?? throw new ArgumentNullException(nameof(nameEN));
            NameFR = nameFR ?? throw new ArgumentNullException(nameFR);
        }

        /// <summary>
        /// Gets localized name based on language
        /// Obtient le nom localisé selon la langue
        /// </summary>
        public string GetLocalizedName(string language) => language?.ToUpper() == "FR" ? NameFR : NameEN;
    }
}