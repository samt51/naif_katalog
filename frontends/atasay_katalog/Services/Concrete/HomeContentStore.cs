namespace atasay_katalog.Services.Concrete;

public sealed class HomeContentStore
{
    public HomeContentStore(IWebHostEnvironment environment) { }

    // Category artwork belongs to this storefront, not to uploaded catalog content.
    public string CategoryImage(int id, string name = "")
    {
        var label = name.Trim().ToUpperInvariant()
            .Replace('İ', 'I').Replace('Ü', 'U').Replace('Ö', 'O')
            .Replace('Ş', 'S').Replace('Ç', 'C').Replace('Ğ', 'G');
        var image = label.Contains("GERDAN") ? "collar"
            : label.Contains("KELEPCE") || label.Contains("BILEZIK") || label.Contains("BANGLE") ? "bangle"
            : label.Contains("BILEKLIK") || label.Contains("BRACELET") ? "bracelet"
            : label.Contains("KOLYE") || label.Contains("NECKLACE") ? "necklace"
            : label.Contains("KUPE") || label.Contains("EARRING") ? "earring"
            : label.Contains("YUZUK") || label.Contains("RING") || label.Contains("ALYANS") ? "ring"
            : label.Contains("SET") || label.Contains("TAKIM") ? "set" : "bracelet";
        return $"/images/brand/category-{image}.jpg";
    }
}
