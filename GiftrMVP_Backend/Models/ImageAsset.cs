using System.ComponentModel.DataAnnotations;

namespace GiftrMVP_Backend.Models
{
    public class ImageAsset
    {
        [Key]
        public int ImageAssetId { get; set; }

        public required Guid ImageKey { get; set; }

        public required int Height { get; set; }
        public required int Width { get; set; }


        //Optional FK to each possible owner. The FK lives here rather than on the owner so that
        //deleting an owner cascades the image away with it - the other direction would leave
        //orphan rows behind for something else to sweep up.
        //Exactly one of these should be set; nothing at the DB level enforces that today.
        public int? GiftId { get; set; }
        public Gift? Gift { get; set; }

        public int? ProfileId { get; set; }
        public Profile? Profile { get; set; }

        public int? InterestId { get; set; }
        public Interest? Interest { get; set; }
    }
}
