using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GiftrMVP_Backend.Models
{
    public class Profile : IdentityUser<int>
    {
        //
        // Under the hood IdentityUser turns this into AspNetUser table.
        // Primary key is auto-populated as "Id".
        // Email comes from IdentityUser too, so the ERD's email column is already covered.
        //

        [MaxLength(100)]
        public required string FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }


        //Zero or many recipients
        public ICollection<Recipient> Recipients { get; set; } = new List<Recipient>();
        //Zero or many gifts (Gift with RecipientId == null is an idea)
        public ICollection<Gift> Gifts { get; set; } = new List<Gift>();
        //Zero or many groups - groups are scoped per user, not shared globally.
        public ICollection<Group> Groups { get; set; } = new List<Group>();
        //Zero or many events
        public ICollection<Event> Events { get; set; } = new List<Event>();

        //Reverse navigation to the avatar image - FK lives on ImageAsset so the delete cascades this way.
        public ImageAsset? ImageAsset { get; set; }
    }
}
