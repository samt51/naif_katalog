using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace atasay_katalog.Controllers;

[Authorize]
public sealed class CatalogMediaController(IHttpClientFactory clients, IConfiguration configuration) : Controller
{
    // The host comes only from deployment configuration, never from the request.
    [HttpGet]
    public async Task<IActionResult> Image(string? path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Length > 500 || path.StartsWith('/') ||
            path.Contains('\\') || path.Contains(':') || path.Contains('?') || path.Contains('#') ||
            path.Split('/').Any(segment => segment is "" or "." or "..") ||
            !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(path).ToLowerInvariant()))
            return BadRequest();

        var imageBase = configuration["CatalogImageBaseUrl"];
        if (!Uri.TryCreate(imageBase, UriKind.Absolute, out var origin) || origin.Scheme is not ("https" or "http"))
            return StatusCode(503);
        var url = new Uri(origin.AbsoluteUri.TrimEnd('/') + "/" + string.Join('/', path.Split('/').Select(Uri.EscapeDataString)));
        try
        {
            using var response = await clients.CreateClient("CatalogImages").GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode) return NotFound();
            var mime = response.Content.Headers.ContentType?.MediaType;
            if (mime is not ("image/jpeg" or "image/png" or "image/webp")) return NotFound();
            const int limit = 15 * 1024 * 1024;
            if (response.Content.Headers.ContentLength > limit) return StatusCode(413);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var image = new MemoryStream();
            var buffer = new byte[81920];
            int count;
            while ((count = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                if (image.Length + count > limit) return StatusCode(413);
                await image.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
            }
            Response.Headers.CacheControl = "private, max-age=3600";
            return File(image.ToArray(), mime);
        }
        catch (HttpRequestException) { return StatusCode(503); }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested) { return StatusCode(504); }
    }
}
