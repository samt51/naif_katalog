using System.Text.Json;
using naif_katalog.Models;

namespace naif_katalog.Services.Concrete;

// Uploaded category pictures and subscriptions are persistent site content.
// Keep App_Data and wwwroot/images/categories when deploying the application.
public sealed class HomeContentStore(IWebHostEnvironment environment)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private string NewsletterPath => Path.Combine(environment.ContentRootPath, "App_Data", "newsletter.json");
    private string CategoryDirectory => Path.Combine(environment.WebRootPath, "images", "categories");

    public string? CategoryImage(int id)
    {
        foreach (var extension in new[] { ".jpg", ".png", ".webp" })
        {
            var path = Path.Combine(CategoryDirectory, $"{id}{extension}");
            if (File.Exists(path)) return $"/images/categories/{id}{extension}?v={File.GetLastWriteTimeUtc(path).Ticks}";
        }
        return null;
    }

    public async Task SaveCategoryImage(int id, IFormFile file)
    {
        if (file.Length is <= 0 or > 8 * 1024 * 1024)
            throw new ArgumentException("Görsel en fazla 8 MB olmalıdır.");
        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var read = await input.ReadAtLeastAsync(header, 12, throwOnEndOfStream: false);
        var extension = read >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff ? ".jpg"
            : read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) ? ".png"
            : read == 12 && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP" ? ".webp"
            : throw new ArgumentException("JPG, PNG veya WebP formatında bir görsel seçin.");
        await gate.WaitAsync();
        string? temporary = null;
        try
        {
            Directory.CreateDirectory(CategoryDirectory);
            temporary = Path.Combine(CategoryDirectory, $"{Guid.NewGuid():N}.tmp");
            await using (var output = File.Create(temporary))
            {
                await output.WriteAsync(header.AsMemory(0, read));
                await input.CopyToAsync(output);
            }
            File.Move(temporary, Path.Combine(CategoryDirectory, $"{id}{extension}"), true);
            foreach (var other in new[] { ".jpg", ".png", ".webp" }.Where(x => x != extension))
                File.Delete(Path.Combine(CategoryDirectory, $"{id}{other}"));
        }
        finally
        {
            if (temporary != null && File.Exists(temporary)) File.Delete(temporary);
            gate.Release();
        }
    }

    public async Task<List<NewsletterSubscription>> Subscribers()
    {
        await gate.WaitAsync();
        try { return await ReadSubscribers(); }
        finally { gate.Release(); }
    }

    private async Task<List<NewsletterSubscription>> ReadSubscribers() => File.Exists(NewsletterPath)
        ? JsonSerializer.Deserialize<List<NewsletterSubscription>>(await File.ReadAllTextAsync(NewsletterPath)) ?? [] : [];

    public async Task Subscribe(NewsletterSubscription subscription)
    {
        await gate.WaitAsync();
        var temporary = NewsletterPath + ".tmp";
        try
        {
            var all = await ReadSubscribers();
            if (all.Any(x => string.Equals(x.Email, subscription.Email.Trim(), StringComparison.OrdinalIgnoreCase))) return;
            subscription.FirstName = subscription.FirstName.Trim();
            subscription.LastName = subscription.LastName.Trim();
            subscription.Email = subscription.Email.Trim();
            subscription.SubscribedAt = DateTimeOffset.UtcNow;
            all.Add(subscription);
            Directory.CreateDirectory(Path.GetDirectoryName(NewsletterPath)!);
            await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(all, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, NewsletterPath, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            gate.Release();
        }
    }
}
