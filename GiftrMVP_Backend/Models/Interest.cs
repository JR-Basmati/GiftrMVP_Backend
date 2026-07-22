using System.ComponentModel.DataAnnotations;

namespace GiftrMVP_Backend.Models
{
    public class Interest
    {
        [Key]
        public int InterestId { get; set; }

        [MaxLength(255)]
        public required string Title { get; set; }

        [MaxLength(255)]
        public string? Subtitle { get; set; }


        //Required link back to recipient - an interest only means anything attached to a person.
        public int RecipientId { get; set; }
        public Recipient Recipient { get; set; } = null!;

        //Reverse navigation to image
        public ImageAsset? ImageAsset { get; set; }
    }
}
