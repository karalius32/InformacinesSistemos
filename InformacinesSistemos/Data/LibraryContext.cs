using System;
using System.Collections.Generic;
using InformacinesSistemos.Models;
using InformacinesSistemos.Models.Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using InformacinesSistemos.Models.Enums;

namespace InformacinesSistemos.Data;

public partial class LibraryContext : IdentityDbContext<ApplicationUser>
{
    public LibraryContext()
    {
    }

    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookAuthor> BookAuthors { get; set; }

    public virtual DbSet<BookCategory> BookCategories { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Recommendation> Recommendations { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=informacinessistemos.postgres.database.azure.com;Database=postgres;Username=informacines;Password=Slaptazodis0;SSL Mode=Prefer;Persist Security Info=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register Postgres enums and extensions
        modelBuilder
            .HasPostgresEnum<AuthorRole>("public", "author_role")
            .HasPostgresEnum<SubscriptionLevel>("public", "subscription_level")
            .HasPostgresEnum<UserRole>("public", "user_role")
            .HasPostgresExtension("pg_catalog", "azure")
            .HasPostgresExtension("pg_catalog", "pg_cron")
            .HasPostgresExtension("pg_catalog", "pgaadauth");

        // Ensure identity mappings are configured
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(a => a.Profile)
            .WithOne() // or .WithOne(p => p.User) if you have the back-reference
            .HasForeignKey<Models.Library.UserAccount>(p => p.IdentityUserId)
            .HasPrincipalKey<ApplicationUser>(a => a.Id)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("author_pkey");

            entity.ToTable("authors");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.Country)
                .HasMaxLength(255)
                .HasColumnName("country");
            entity.Property(e => e.DeathDate).HasColumnName("death_date");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("last_name");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("book_pkey");

            entity.ToTable("books");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CoverUrl)
                .HasMaxLength(255)
                .HasColumnName("cover_url");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Format)
                .HasMaxLength(255)
                .HasColumnName("format");
            entity.Property(e => e.Identifier)
                .HasMaxLength(255)
                .HasColumnName("identifier");
            entity.Property(e => e.Keywords)
                .HasMaxLength(255)
                .HasColumnName("keywords");
            entity.Property(e => e.Language)
                .HasMaxLength(255)
                .HasColumnName("language");
            entity.Property(e => e.LoanId).HasColumnName("loan_id");
            entity.Property(e => e.PageCount).HasColumnName("page_count");
            entity.Property(e => e.PublishDate).HasColumnName("publish_date");
            entity.Property(e => e.Publisher)
                .HasMaxLength(255)
                .HasColumnName("publisher");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");

            entity.HasOne(d => d.Loan).WithMany(p => p.Books)
                .HasForeignKey(d => d.LoanId)
                .HasConstraintName("book_loan_fk");
        });

        modelBuilder.Entity<BookAuthor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("book_author_pkey");

            entity.ToTable("book_authors");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Contribution).HasColumnName("contribution");

            entity.HasOne(d => d.Author).WithMany(p => p.BookAuthors)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("book_author_author_fk");

            entity.HasOne(d => d.Book).WithMany(p => p.BookAuthors)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("book_author_book_fk");
        });

        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("book_category_pkey");

            entity.ToTable("book_categories");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");

            entity.HasOne(d => d.Book).WithMany(p => p.BookCategories)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("book_category_book_fk");

            entity.HasOne(d => d.Category).WithMany(p => p.BookCategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("book_category_category_fk");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("category_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoice_pkey");

            entity.ToTable("invoices");

            entity.HasIndex(e => e.SubscriptionId, "invoice_subscription_id_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");

            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.CoinbaseChargeCode)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.Currency)
                .HasMaxLength(255)
                .HasColumnName("currency");
            entity.Property(e => e.LoanId).HasColumnName("loan_id");
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(i => i.UserId).HasColumnName("user_id");
            entity.Property(i => i.CoinbaseHostedUrl).HasColumnName("coinbase_hosted_url");

            entity.HasOne(d => d.Loan).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.LoanId)
                .HasConstraintName("invoice_loan_fk");

            entity.HasOne(d => d.Subscription).WithOne(p => p.Invoice)
                .HasForeignKey<Invoice>(d => d.SubscriptionId)
                .HasConstraintName("invoice_subscription_fk");

            entity.HasOne(i => i.User)
            .WithMany(u => u.Invoices)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("loan_pkey");

            entity.ToTable("loans");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AccumulatedPenalties).HasColumnName("accumulated_penalties");
            entity.Property(e => e.ExtensionCount).HasColumnName("extension_count");
            entity.Property(e => e.LoanDate).HasColumnName("loan_date");
            entity.Property(e => e.ReturnDate).HasColumnName("return_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Loans)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("loan_user_fk");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_pkey");

            entity.ToTable("payments");

            entity.HasIndex(e => e.InvoiceId, "payment_invoice_id_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PaymentNumber)
                .HasMaxLength(255)
                .HasColumnName("payment_number");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(255)
                .HasColumnName("transaction_id");

            entity.HasOne(d => d.Invoice).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payment_invoice_fk");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rating_pkey");

            entity.ToTable("ratings");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.RatingDate).HasColumnName("rating_date");
            entity.Property(e => e.RatingValue).HasColumnName("rating_value");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Book).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rating_book_fk");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rating_user_fk");
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recommendation_pkey");

            entity.ToTable("recommendations");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.LoanId).HasColumnName("loan_id");
            entity.Property(e => e.RecommendationDate).HasColumnName("recommendation_date");
            entity.Property(e => e.RecommendationText)
                .HasMaxLength(255)
                .HasColumnName("recommendation_text");

            entity.HasOne(d => d.Book).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recommendation_book_fk");

            entity.HasOne(d => d.Loan).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.LoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recommendation_loan_fk");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscription_pkey");

            entity.ToTable("subscriptions");

            entity.HasIndex(e => e.UserId, "subscription_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ValidUntil).HasColumnName("valid_until");

            entity.HasOne(d => d.User).WithOne(p => p.Subscription)
                .HasForeignKey<Subscription>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subscription_user_fk");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_account_pkey");

            entity.ToTable("user_accounts");

            entity.HasIndex(e => e.IdentityUserId, "IX_user_accounts_identity_user_id").IsUnique();

            entity.HasIndex(e => e.Email, "user_account_email_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("first_name");
            entity.Property(e => e.IdentityUserId).HasColumnName("identity_user_id");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("last_name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(255)
                .HasColumnName("phone_number");
            entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Role)
                .HasColumnType("user_role")
                .HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
