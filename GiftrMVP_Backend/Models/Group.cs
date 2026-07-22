using System.ComponentModel.DataAnnotations;

namespace GiftrMVP_Backend.Models
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        [MaxLength(255)]
        public required string Name { get; set; }

        public string? Notes { get; set; }


        //Required link back to profile - groups are scoped per user, same as recipients.
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        //Membership carries per-recipient title/notes, so it goes through an explicit entity.
        public ICollection<RecipientGroup> RecipientGroups { get; set; } = new List<RecipientGroup>();

        //Plain many-to-many - EF owns the group_events join table.
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
