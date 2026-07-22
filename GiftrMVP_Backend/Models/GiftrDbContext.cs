using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace GiftrMVP_Backend.Models
{
    public class GiftrDbContext : IdentityDbContext<Profile, IdentityRole<int>, int>
    {
        //Is this needed? Beginner examples just show it as blank..
        public GiftrDbContext(DbContextOptions<GiftrDbContext> options) : base(options)
        {
        }

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Recipient> Recipients { get; set; }
        public DbSet<Gift> Gifts { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<RecipientGroup> RecipientGroups { get; set; }
        public DbSet<ImageAsset> ImageAssets { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Images. The FK sits on ImageAsset pointing back at each possible owner, so deleting an
            //owner cascades the image away with it. Putting image_id on the owner instead (as the ERD
            //draws it) would send the cascade the wrong way and leave dangling rows.
            //All three FKs are nullable, so EF would default to SET NULL - override to Cascade.
            modelBuilder.Entity<ImageAsset>(e =>
            {
                e.HasOne(i => i.Gift)
                    .WithOne(g => g.ImageAsset)
                    .HasForeignKey<ImageAsset>(i => i.GiftId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(i => i.Profile)
                    .WithOne(p => p.ImageAsset)
                    .HasForeignKey<ImageAsset>(i => i.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(i => i.Interest)
                    .WithOne(x => x.ImageAsset)
                    .HasForeignKey<ImageAsset>(i => i.InterestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //1:Many only required here since we want to alter default ON DELETE behaviour
            modelBuilder.Entity<Recipient>()
                .HasMany(r => r.Gifts)
                .WithOne(g => g.Recipient)
                .HasForeignKey(g => g.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            //Same reasoning for interests - losing a recipient should take their interests with them.
            modelBuilder.Entity<Interest>()
                .HasOne(i => i.Recipient)
                .WithMany(r => r.Interests)
                .HasForeignKey(i => i.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            //Group membership. Composite PK, and deleting either side drops the membership row.
            modelBuilder.Entity<RecipientGroup>(e =>
            {
                e.HasKey(rg => new { rg.RecipientId, rg.GroupId });

                e.HasOne(rg => rg.Recipient)
                    .WithMany(r => r.RecipientGroups)
                    .HasForeignKey(rg => rg.RecipientId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(rg => rg.Group)
                    .WithMany(g => g.RecipientGroups)
                    .HasForeignKey(rg => rg.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Event>(e =>
            {
                //Free-form JSON, so jsonb rather than text - lets PG index/query into the rule later.
                e.Property(ev => ev.RecurringRule).HasColumnType("jsonb");

                //These two joins carry no extra columns, so EF can own the join entity outright.
                //The FK names are spelled out because EF's default would combine the navigation name
                //with the PK name and give us recipients_recipient_id / events_event_id.
                e.HasMany(ev => ev.Recipients)
                    .WithMany(r => r.Events)
                    .UsingEntity(
                        "RecipientEvent",
                        l => l.HasOne(typeof(Recipient)).WithMany().HasForeignKey("RecipientId"),
                        r => r.HasOne(typeof(Event)).WithMany().HasForeignKey("EventId"),
                        j => j.ToTable("recipient_events"));

                e.HasMany(ev => ev.Groups)
                    .WithMany(g => g.Events)
                    .UsingEntity(
                        "GroupEvent",
                        l => l.HasOne(typeof(Group)).WithMany().HasForeignKey("GroupId"),
                        r => r.HasOne(typeof(Event)).WithMany().HasForeignKey("EventId"),
                        j => j.ToTable("group_events"));
            });

            //Store the location enum as text - see the note on GiftLocation.
            modelBuilder.Entity<Gift>()
                .Property(g => g.Location)
                .HasConversion<string>()
                .HasMaxLength(32);

            //Refresh tokens: index the hash for fast lookup, cascade-delete with the owning profile.
            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.HasIndex(rt => rt.TokenHash);
                e.HasOne(rt => rt.Profile)
                    .WithMany()
                    .HasForeignKey(rt => rt.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
