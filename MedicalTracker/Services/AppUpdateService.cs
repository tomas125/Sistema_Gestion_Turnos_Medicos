using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace MedicalTracker.Services;

/// <summary>
/// Comprobación de actualizaciones desde un manifiesto JSON y descarga del instalador (Inno Setup).
/// </summary>
public static class AppUpdateService
{
    /// <summary>Nombre corto para User-Agent y prefijo del instalador temporal.</summary>
    public const string ProductUpdateName = "SeguimientoTurnosMedicos";

    private const string DialogTitle = "Actualizar sistema";
    private const string ConfigFileName = "update-config.json";
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromMinutes(15);
    private const int DownloadBufferBytes = 80 * 1024;

    private static readonly JsonSerializerOptions JsonReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static async Task RunUpdateCheckAsync(Form owner)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        if (!File.Exists(configPath))
        {
            MessageBox.Show(owner,
                $"No se encontró el archivo de configuración de actualizaciones.{Environment.NewLine}{Environment.NewLine}" +
                $"Debe existir un archivo llamado «{ConfigFileName}» en la carpeta del programa con la clave «manifestUrl» " +
                $"apuntando al manifiesto publicado en internet.{Environment.NewLine}{Environment.NewLine}" +
                $"Ruta esperada:{Environment.NewLine}{configPath}",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        UpdateConfigDto? config;
        try
        {
            await using var fs = File.OpenRead(configPath);
            config = await JsonSerializer.DeserializeAsync<UpdateConfigDto>(fs, JsonReadOptions);
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner,
                $"No se pudo leer «{ConfigFileName}».{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var manifestUrlRaw = config?.ManifestUrl?.Trim();
        if (string.IsNullOrEmpty(manifestUrlRaw))
        {
            MessageBox.Show(owner,
                "Falta la clave «manifestUrl» en el archivo de configuración o está vacía. " +
                "El proveedor del sistema debe configurar esa URL para poder buscar actualizaciones.",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (!Uri.TryCreate(manifestUrlRaw, UriKind.Absolute, out var manifestUri))
        {
            MessageBox.Show(owner,
                $"La URL del manifiesto no es una dirección absoluta válida:{Environment.NewLine}{manifestUrlRaw}",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!IsUrlSecurityAllowed(manifestUri))
        {
            MessageBox.Show(owner,
                "La URL del manifiesto debe usar HTTPS (HTTP solo está permitido para localhost o 127.0.0.1 en pruebas).",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        using var http = CreateHttpClient();

        UpdateManifestDto? manifest;
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, manifestUri);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(true);
            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                MessageBox.Show(owner,
                    "No se encontró el manifiesto de actualización (404). " +
                    "Compruebe que el archivo esté publicado en la rama o repositorio correcto (por ejemplo en GitHub) " +
                    $"y que la URL sea la adecuada:{Environment.NewLine}{manifestUri}",
                    DialogTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            resp.EnsureSuccessStatusCode();
            await using var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(true);
            manifest = await JsonSerializer.DeserializeAsync<UpdateManifestDto>(stream, JsonReadOptions)
                .ConfigureAwait(true);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show(owner,
                $"No se pudo obtener el manifiesto de actualización. Compruebe la conexión a internet.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner,
                $"Error al obtener el manifiesto.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (manifest == null ||
            string.IsNullOrWhiteSpace(manifest.Version) ||
            string.IsNullOrWhiteSpace(manifest.DownloadUrl))
        {
            MessageBox.Show(owner,
                "El manifiesto remoto tiene un formato incorrecto: deben existir «version» y «downloadUrl» con valores no vacíos.",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!TryParseManifestVersion(manifest.Version.Trim(), out var remoteVersion))
        {
            MessageBox.Show(owner,
                $"No se pudo interpretar la versión del manifiesto: «{manifest.Version}».",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!Uri.TryCreate(manifest.DownloadUrl.Trim(), UriKind.Absolute, out var downloadUri))
        {
            MessageBox.Show(owner,
                $"La URL de descarga del manifiesto no es válida:{Environment.NewLine}{manifest.DownloadUrl}",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!IsUrlSecurityAllowed(downloadUri))
        {
            MessageBox.Show(owner,
                "La URL de descarga debe usar HTTPS (HTTP solo está permitido para localhost o 127.0.0.1 en pruebas).",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);
        if (remoteVersion <= currentVersion)
        {
            MessageBox.Show(owner,
                $"Su sistema está al día.{Environment.NewLine}{Environment.NewLine}" +
                $"Versión instalada: {FormatVersionDisplay(currentVersion)}{Environment.NewLine}" +
                $"Última versión publicada: {FormatVersionDisplay(remoteVersion)}",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var notas = string.IsNullOrWhiteSpace(manifest.ReleaseNotes)
            ? ""
            : $"{Environment.NewLine}{Environment.NewLine}Notas de la versión:{Environment.NewLine}{manifest.ReleaseNotes.Trim()}";

        var descargar = MessageBox.Show(owner,
            $"Hay una nueva versión disponible.{Environment.NewLine}{Environment.NewLine}" +
            $"Versión instalada: {FormatVersionDisplay(currentVersion)}{Environment.NewLine}" +
            $"Nueva versión: {FormatVersionDisplay(remoteVersion)}" +
            notas +
            $"{Environment.NewLine}{Environment.NewLine}¿Descargar el instalador ahora?",
            DialogTitle,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (descargar != DialogResult.Yes)
            return;

        var tempPath = BuildTempInstallerPath(remoteVersion);
        try
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { /* ignorar */ }
            }
        }
        catch { /* ignorar */ }

        using var dlg = new frmDownloadProgress(http, downloadUri, tempPath);
        dlg.Owner = owner;
        var dlgResult = dlg.ShowDialog(owner);
        if (dlgResult != DialogResult.OK || !File.Exists(tempPath))
            return;

        if (!string.IsNullOrWhiteSpace(manifest.Sha256))
        {
            var expected = NormalizeSha256Hex(manifest.Sha256);
            if (expected == null)
            {
                try { File.Delete(tempPath); } catch { }
                MessageBox.Show(owner,
                    "El valor «sha256» del manifiesto no es un hash hexadecimal válido.",
                    DialogTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string actualHex;
            try
            {
                actualHex = await ComputeFileSha256HexAsync(tempPath).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                try { File.Delete(tempPath); } catch { }
                MessageBox.Show(owner,
                    $"No se pudo verificar el archivo descargado.{Environment.NewLine}{ex.Message}",
                    DialogTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (!actualHex.Equals(expected, StringComparison.OrdinalIgnoreCase))
            {
                try { File.Delete(tempPath); } catch { }
                MessageBox.Show(owner,
                    "La verificación SHA256 del instalador descargado falló. El archivo no coincide con el manifiesto y no se instalará.",
                    DialogTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
        }

        var instalar = MessageBox.Show(owner,
            "El instalador se descargó correctamente. Para aplicar la actualización se cerrará esta aplicación " +
            "y se ejecutará el instalador en segundo plano sin ventanas. Al finalizar, el instalador volverá a abrir el sistema automáticamente. " +
            $"{Environment.NewLine}{Environment.NewLine}Guarde su trabajo antes de continuar.{Environment.NewLine}{Environment.NewLine}" +
            "¿Instalar ahora?",
            DialogTitle,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (instalar != DialogResult.Yes)
        {
            MessageBox.Show(owner,
                $"Puede ejecutar el instalador manualmente cuando desee:{Environment.NewLine}{tempPath}",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var workDir = Path.GetDirectoryName(tempPath) ?? Path.GetTempPath();
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = false,
                WorkingDirectory = workDir
            };
            psi.ArgumentList.Add("/VERYSILENT");
            psi.ArgumentList.Add("/NORESTART");
            psi.ArgumentList.Add("/SUPPRESSMSGBOXES");
            psi.ArgumentList.Add("/CLOSEAPPLICATIONS");
            psi.ArgumentList.Add("/RESTARTAPPLICATIONS");
            psi.ArgumentList.Add("/SP-");
            psi.ArgumentList.Add("/CURRENTUSER");

            using var proc = Process.Start(psi);
            proc?.Dispose();
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner,
                $"No se pudo iniciar el instalador.{Environment.NewLine}{ex.Message}{Environment.NewLine}{Environment.NewLine}" +
                $"El archivo sigue disponible en:{Environment.NewLine}{tempPath}",
                DialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Environment.Exit(0);
    }

    internal static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = HttpTimeout };
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"{ProductUpdateName}-Update/1.0");
        return client;
    }

    internal static async Task DownloadToFileAsync(
        HttpClient http,
        Uri downloadUri,
        string destinationPath,
        IProgress<DownloadProgressReport>? progress,
        CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, downloadUri);
        using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct)
            .ConfigureAwait(false);
        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            throw new HttpRequestException(
                "No se encontró el instalador en internet (404). " +
                "Hace falta publicar en GitHub el archivo del instalador como adjunto de un Release " +
                "(nombre exacto, p. ej. MedicalTracker_Setup_1.0.2.exe) y que coincida con «downloadUrl» en release/manifest.json del repositorio.",
                inner: null,
                statusCode: HttpStatusCode.NotFound);
        }

        resp.EnsureSuccessStatusCode();

        long? total = null;
        var len = resp.Content.Headers.ContentLength;
        if (len.HasValue && len.Value >= 0)
            total = len.Value;

        await using var httpStream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

        var buffer = new byte[DownloadBufferBytes];
        long readTotal = 0;
        int read;
        while ((read = await httpStream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(false)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), ct).ConfigureAwait(false);
            readTotal += read;
            progress?.Report(new DownloadProgressReport(readTotal, total));
        }
    }

    private static string BuildTempInstallerPath(Version remoteVersion)
    {
        var name = $"{ProductUpdateName}_Instalador_{remoteVersion.Major}_{remoteVersion.Minor}_{remoteVersion.Build}.exe";
        return Path.Combine(Path.GetTempPath(), name);
    }

    private static bool IsUrlSecurityAllowed(Uri uri)
    {
        if (string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            var h = uri.Host;
            return string.Equals(h, "localhost", StringComparison.OrdinalIgnoreCase) || h == "127.0.0.1";
        }

        return false;
    }

    private static bool TryParseManifestVersion(string s, out Version version)
    {
        var t = s.Trim();
        if (t.Length > 0 && (t[0] == 'v' || t[0] == 'V'))
            t = t[1..].TrimStart();
        if (!Version.TryParse(t, out var v))
        {
            version = new Version(0, 0);
            return false;
        }

        version = v;
        return true;
    }

    private static int NormalizePart(int p) => p < 0 ? 0 : p;

    private static string FormatVersionDisplay(Version v)
    {
        var rev = NormalizePart(v.Revision);
        if (rev == 0)
            return $"{NormalizePart(v.Major)}.{NormalizePart(v.Minor)}.{NormalizePart(v.Build)}";
        return $"{NormalizePart(v.Major)}.{NormalizePart(v.Minor)}.{NormalizePart(v.Build)}.{rev}";
    }

    private static string? NormalizeSha256Hex(string raw)
    {
        var s = raw.Trim();
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            s = s[2..];
        s = s.Replace(" ", "").Replace("-", "");
        if (s.Length != 64) return null;
        foreach (var c in s)
        {
            var ok = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
            if (!ok) return null;
        }

        return s;
    }

    private static async Task<string> ComputeFileSha256HexAsync(string path)
    {
        await using var fs = File.OpenRead(path);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(fs).ConfigureAwait(false);
        return Convert.ToHexString(hash);
    }

    private sealed class UpdateConfigDto
    {
        public string? ManifestUrl { get; set; }
    }

    private sealed class UpdateManifestDto
    {
        public string? Version { get; set; }
        public string? DownloadUrl { get; set; }
        public string? Sha256 { get; set; }
        public string? ReleaseNotes { get; set; }
    }
}

internal readonly struct DownloadProgressReport
{
    public DownloadProgressReport(long bytesRead, long? totalBytes)
    {
        BytesRead = bytesRead;
        TotalBytes = totalBytes;
    }

    public long BytesRead { get; }
    public long? TotalBytes { get; }
}
