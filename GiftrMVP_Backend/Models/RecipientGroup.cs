using System.ComponentModel.DataAnnotations;

namespace GiftrMVP_Backend.Models
{
    //Join table between Recipient and Group. Explicit rather than an EF-managed many-to-many because
    //the membership itself carries data - e.g. Dad is "The Birthday Boy" inside the "Smith Family" group.
    public class RecipientGroup
    {
        //Composite PK of (RecipientId, GroupId) - configured in GiftrDbContext.
        public int RecipientId { get; set; }
        public Recipient Recipient { get; set; } = null!;

        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Notes { get; set; }
    }
}
