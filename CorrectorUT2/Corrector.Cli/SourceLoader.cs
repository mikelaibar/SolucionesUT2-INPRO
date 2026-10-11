using System.Net;
using System.Text;

namespace Corrector.Cli;

/// <summary>Lee un archivo de C# o lo descarga; este paso no ejecuta su contenido.</summary>
public static class SourceLoader
{
    public const int MaximumBytes = 1024 * 1024;

    public static async Task<string> FromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        string fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"No se encuentra el archivo: {fullPath}");
        if (!string.Equals(Path.GetExtension(fullPath), ".cs", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Selecciona un archivo de C# con extensión .cs.");

        await using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await ReadSourceAsync(stream, cancellationToken);
    }

    public static async Task<string> FromUrlAsync(string address, CancellationToken cancellationToken = default)
    {
        Uri uri = NormalizeUrl(address);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("CorrectorUT2/1.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/plain");

        try
        {
            for (int redirect = 0; redirect <= 5; redirect++)
            {
                using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                if (IsRedirect(response.StatusCode))
                {
                    if (redirect == 5 || response.Headers.Location is null)
                        throw new InvalidOperationException("La URL tiene demasiadas redirecciones o una redirección inválida.");
                    uri = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location
                        : new Uri(uri, response.Headers.Location);
                    EnsureHttps(uri);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException($"No se pudo descargar el archivo: HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). Comprueba que la URL sea pública.");
                if (response.Content.Headers.ContentLength > MaximumBytes)
                    throw new InvalidOperationException("El archivo supera el límite de 1 MB.");
                string? mediaType = response.Content.Headers.ContentType?.MediaType;
                if (mediaType is "text/html" or "application/xhtml+xml")
                    throw new InvalidOperationException("La URL devuelve una página HTML. Usa el enlace al archivo Program.cs o su enlace Raw.");

                await using Stream stream = await response.Content.ReadAsStreamAsync(timeout.Token);
                return await ReadSourceAsync(stream, timeout.Token);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("La descarga no terminó en 20 segundos. Comprueba la conexión o utiliza el archivo local.");
        }
        catch (HttpRequestException exception)
        {
            throw new InvalidOperationException($"No se pudo descargar el archivo: {exception.Message}", exception);
        }

        throw new InvalidOperationException("No se pudo descargar el archivo.");
    }

    public static Uri NormalizeUrl(string address)
    {
        if (!Uri.TryCreate(address, UriKind.Absolute, out Uri? uri))
            throw new InvalidOperationException("La URL no es válida. Utiliza un enlace HTTPS a Program.cs.");
        EnsureHttps(uri);

        // Los enlaces normales de GitHub muestran una página; Raw devuelve el código.
        if (string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
        {
            string[] parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 5 && parts[2] is "blob" or "raw")
            {
                string rawPath = string.Join('/', parts.Take(2).Concat(parts.Skip(3)));
                uri = new Uri($"https://raw.githubusercontent.com/{rawPath}");
            }
        }

        return uri;
    }

    private static void EnsureHttps(Uri uri)
    {
        if (uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(uri.Host) || !string.IsNullOrEmpty(uri.UserInfo))
            throw new InvalidOperationException("Utiliza una URL HTTPS sin usuario ni contraseña. También puedes elegir un archivo local.");
    }

    private static bool IsRedirect(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or
            HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

    private static async Task<string> ReadSourceAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var bytes = new MemoryStream();
        byte[] buffer = new byte[8192];
        int count;
        while ((count = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (bytes.Length + count > MaximumBytes)
                throw new InvalidOperationException("El archivo supera el límite de 1 MB.");
            await bytes.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
        }

        bytes.Position = 0;
        string source;
        try
        {
            using var reader = new StreamReader(bytes, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: true);
            source = await reader.ReadToEndAsync(cancellationToken);
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidOperationException("Guarda Program.cs con codificación UTF-8 y vuelve a intentarlo.", exception);
        }

        if (string.IsNullOrWhiteSpace(source))
            throw new InvalidOperationException("El archivo está vacío.");
        string beginning = source.TrimStart();
        if (beginning.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) ||
            beginning.StartsWith("<html", StringComparison.OrdinalIgnoreCase) || source.Contains('\0'))
            throw new InvalidOperationException("El contenido no es un archivo de C#. Usa Program.cs o su enlace Raw.");
        return source;
    }
}
