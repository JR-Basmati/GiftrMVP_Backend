namespace GiftrMVP_Backend.Models
{
    //Persisted as text rather than an int (see GiftrDbContext) - keeps raw SQL readable, and means
    //re-ordering or inserting values later can't silently re-label existing rows.
    public enum GiftLocation
    {
        Online,
        PhysicalStore,
        Unsure,
        UsedMarket,
        Diy,
        AlreadyPurchased
    }
}
