using System.ComponentModel.DataAnnotations;

namespace GiftrMVP_Backend.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        //FK to the owning profile
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        //We store a SHA-256 hash of the opaque token, never the raw value - so a DB leak can't hand out working tokens.
        [MaxLength(64)]
        public required string TokenHash { get; set; }

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Set when the token is rotated (on refresh) or explicitly revoked (on logout).
        public DateTime? RevokedAt { get; set; }

        //Hash of the token that replaced this one - lets us spot reuse of an already-rotated token.
        [MaxLength(64)]
        public string? ReplacedByHash { get; set; }
    }
}
