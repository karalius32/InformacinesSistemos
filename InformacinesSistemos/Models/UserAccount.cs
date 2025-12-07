using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using InformacinesSistemos.Models.Enums;

namespace InformacinesSistemos.Models;

public partial class UserAccount
{
    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Address { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Status { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? IdentityUserId { get; set; }

    public UserRole Role { get; set; }

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Subscription? Subscription { get; set; }
}
