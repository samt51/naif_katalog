using System.ComponentModel.DataAnnotations;

namespace atasay_katalog.Models;

public class HomePageViewModel
{
    public List<Product> NewProducts { get; set; } = [];
    public List<HomeCategoryCard> Categories { get; set; } = [];
    public bool LoadFailed { get; set; }
}

public record HomeCategoryCard(int Id, string Name, string? ImageUrl);

public class NewsletterSubscription
{
    [Required, StringLength(80)]
    public string FirstName { get; set; } = "";
    [Required, StringLength(80)]
    public string LastName { get; set; } = "";
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    public DateTimeOffset SubscribedAt { get; set; }
}
