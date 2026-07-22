using System.ComponentModel.DataAnnotations;

namespace GiftrMVP_Backend.Models
{
    public class Gift
    {
        [Key]
        public int GiftId { get; set; }

        [MaxLength(255)]
        public required string Title { get; set; }

        public Boolean Given { get; set; } = false;
        public string? Url { get; set; }
        public string? Notes { get; set; }

        //Where the gift is expected to come from. Defaults to Unsure rather than being nullable -
        //"I don't know yet" is a real answer, so it doesn't need to also be an empty column.
        public GiftLocation Location { get; set; } = GiftLocation.Unsure;

        //Required Profile association
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        //FK back to recipient - optional because a gift may just be an un-associated "idea" initally.
        public int? RecipientId { get; set; }
        public Recipient? Recipient { get; set; }

        //Reverse navigation to image
        public ImageAsset? ImageAsset { get; set; }
    }
}
