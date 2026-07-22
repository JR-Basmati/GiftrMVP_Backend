using System.ComponentModel.DataAnnotations;

namespace GiftrMVP_Backend.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [MaxLength(255)]
        public required string Title { get; set; }

        public required DateTime StartDateTime { get; set; }
        public required DateTime EndDateTime { get; set; }

        public Boolean RecurringEvent { get; set; } = false;

        //Recurrence stored as raw JSON (mapped to jsonb in GiftrDbContext) so the shape of the rule
        //can change without a migration. Null when RecurringEvent is false.
        public string? RecurringRule { get; set; }


        //Required link back to profile
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        //An event can hang off individual recipients, whole groups, or both.
        public ICollection<Recipient> Recipients { get; set; } = new List<Recipient>();
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
