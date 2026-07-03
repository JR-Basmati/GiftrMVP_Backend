using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiftrMVP_Backend.Models
{
    public class GiftrDbContext : DbContext
    {
        //Is this needed? Beginner examples just show it as blank..
        public GiftrDbContext(DbContextOptions<GiftrDbContext> options) : base(options)
        {
        }

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Recipient> Recipients { get; set; }
        public DbSet<Gift> Gifts { get; set; }
        public DbSet<ImageAsset> ImageAssets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Profile>()
                .HasIndex(u => u.Email)
                .IsUnique();

            //1:1 between Gift and ImageAsset - Should be one to many, but I wanted to try doing 1:1 if EF
            //Almost FEELS like the FK belongs in gift, but that makes the CASCADE delete go the wrong way.
            modelBuilder.Entity<Gift>()
                .HasOne(g => g.ImageAsset)
                .WithOne(i => i.Gift)
                .HasForeignKey<ImageAsset>(i => i.GiftId)
                .OnDelete(DeleteBehavior.Cascade);//Need to explicitly set to cascade. Since FK is Nullable, EF defaults to "SET NULL", which could leave dangling images.

            //1:Many only required here since we want to alter default ON DELETE behaviour
            modelBuilder.Entity<Recipient>()
                .HasMany(r => r.Gifts)
                .WithOne(g => g.Recipient)
                .HasForeignKey(g => g.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class Profile
    {
        public int ProfileId { get; set; }

        [MaxLength(100)]
        public required string FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(255)]
        public required string Email { get; set; }

        [MaxLength(100)]
        [MinLength(6)]
        public required string Password { get; set; }

        //Zero or many recipients
        public ICollection<Recipient> Recipients { get; set; } = new List<Recipient>();
        //Zero or many gifts (Gift with RecipientId == null is an idea)
        public ICollection<Gift> Gifts { get; set; } = new List<Gift>();
    }

    public class Recipient
    {
        public int RecipientId { get; set; }
        [MaxLength(255)]
        public required string Name { get; set; }
        public string? Notes { get; set; }


        //Required link back to profile
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        //Zero or many gifts
        public ICollection<Gift> Gifts { get; set; } = new List<Gift>();

    }

    public class Gift
    {
        public int GiftId { get; set; }
        [MaxLength(255)]
        public required string Title { get; set; }
        public Boolean Given { get; set; } = false;
        public string? Url { get; set; }
        public string? Notes { get; set; }

        //Required Profile association
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        //FK back to recipient - optional because a gift may just be an un-associated "idea" initally.
        public int? RecipientId { get; set; }
        public Recipient? Recipient { get; set; }

        //Reverse navigation to image
        public ImageAsset? ImageAsset { get; set; }
    }

    public class ImageAsset
    {
        public int ImageAssetId { get; set; }

        [MaxLength(32)]
        public required string ImageKey { get; set; }

        public required int Height { get; set; }
        public required int Width { get; set; }

        //Optional FK to gift
        public int? GiftId { get; set; }
        public Gift? Gift { get; set; }

    }
}
