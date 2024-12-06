using Cursus.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Metadata;

namespace Cursus.Data.Models
{
    public class CursusDbContext : IdentityDbContext<ApplicationUser>
    {
        public CursusDbContext()
        {
            
        }
        public CursusDbContext(DbContextOptions<CursusDbContext> options) : base(options)
        {

        }

        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Step> Steps { get; set; } = null!;
        public virtual DbSet<StepComment> StepComments { get; set; } = null!;
        public virtual DbSet<StepContent> StepContents { get; set; } = null!;
        public virtual DbSet<CourseVersion> CourseVersions { get; set; } = null!;
        public virtual DbSet<CourseProgress> CourseProgresses { get; set; } = null!;
        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public virtual DbSet<InstructorInfo> InstructorInfos { get; set; } = null!;
        public virtual DbSet<CourseComment> CourseComments { get; set; } = null!;
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public virtual DbSet<Cart> Cart { get; set; } = null!;
        public virtual DbSet<CartItems> CartItems { get; set; } = null!;
        public virtual DbSet<Order> Order { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<Bookmark> Bookmarks { get; set; }=null!;
        public virtual DbSet<Reason> Reason { get; set; }=null!;

        public virtual DbSet<AdminComment> AdminComments { get; set; } = null!;
        public virtual DbSet<Wallet> Wallets { get; set; }
        public virtual DbSet<TransactionHistory> TransactionHistories { get; set; }
        public virtual DbSet<PlatformWallet> PlatformWallets { get; set; }
        public virtual DbSet<Certificate> Certificates { get; set; }
        
        public virtual DbSet<PayoutRequest> PayoutRequests { get; set; } = null!;

        public virtual DbSet<WalletHistory> WalletHistories { get; set; } = null!;

        public virtual DbSet<TrackingProgress> TrackingProgresses { get; set; } = null!;

        public virtual DbSet<ArchivedTransaction> ArchivedTransactions { get; set; }
		public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<HomePage> HomePages { get; set; }
        public virtual DbSet<PrivacyPolicy> PrivacyPolicies { get; set; }
        public virtual DbSet<Term> Terms { get; set; }
        public virtual DbSet<InstructorCertificate> InstructorCertificates { get; set; } = null!;
        public virtual DbSet<Voucher> Vouchers { get; set; } = null!;

        public virtual DbSet<Message> Messages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>().HasData(
                 new IdentityRole
                 {
                     Id = Guid.NewGuid().ToString(),
                     Name = "Admin",
                     NormalizedName = "ADMIN"
                 },
                new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Instructor",
                    NormalizedName = "INSTRUCTOR"
                },
                new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "User",
                    NormalizedName = "USER"
                }
            );
            modelBuilder.Entity<AdminComment>()
                .HasOne(c => c.Commenter)
                .WithMany()
                .HasForeignKey(c => c.CommenterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
            .ToTable(tb => tb.HasTrigger("trg_Transaction_Log"));
            
            modelBuilder.Entity<Wallet>()
            .ToTable(tb => tb.HasTrigger("trg_Wallet_BalanceChange"));

            modelBuilder.Entity<PlatformWallet>().HasData(
                new
                {
                    Id = 1,
                    Balance = 0.0
                }
            );


            modelBuilder.Entity<WalletHistory>()
                .HasOne(wh => wh.Wallet)
                .WithMany()
                .HasForeignKey(wh => wh.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TrackingProgress>()
            .HasOne(tp => tp.CourseProgress)
            .WithMany()
            .HasForeignKey(tp => tp.ProgressId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrackingProgress>()
                .HasOne(tp => tp.Step)
                .WithMany()
                .HasForeignKey(tp => tp.StepId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
            .HasIndex(o => o.DateCreated)
            .HasFilter("[Status] = 1")
            .IncludeProperties(o => new { o.PaidAmount, o.CartId })
            .HasDatabaseName("IDX_Orders_Status1_DateCreated");

            modelBuilder.Entity<CartItems>()
            .HasIndex(ci => ci.CartId)
            .HasDatabaseName("IDX_CartItems_CartId");

            modelBuilder.Entity<InstructorInfo>()
            .HasIndex(ii => ii.UserId)
            .HasDatabaseName("IDX_InstructorInfo_UserId");

        }
    }
}
