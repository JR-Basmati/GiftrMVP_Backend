using System.ComponentModel.DataAnnotations;

namespace GiftrMVP_Backend.Models
{
    public class Recipient
    {
        [Key]
        public int RecipientId { get; set; }

        [MaxLength(100)]
        public required string FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public string? Notes { get; set; }


        //Required link back to profile
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        //Zero or many gifts
        public ICollection<Gift> Gifts { get; set; } = new List<Gift>();

        //Zero or many interests
        public ICollection<Interest> Interests { get; set; } = new List<Interest>();

        //Group membership carries its own title/notes, so it goes through an explicit entity
        //rather than a plain many-to-many.
        public ICollection<RecipientGroup> RecipientGroups { get; set; } = new List<RecipientGroup>();

        //Plain many-to-many - EF owns the recipient_events join table.
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
