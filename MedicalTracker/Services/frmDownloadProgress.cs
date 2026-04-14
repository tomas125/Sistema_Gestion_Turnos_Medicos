namespace MedicalTracker.Services;

/// <summary>
/// Descarga del instalador con barra de progreso y cancelación.
/// </summary>
public partial class frmDownloadProgress : Form
{
    private const string DownloadErrorTitle = "Error al descargar";

    private readonly HttpClient _http;
    private readonly Uri _downloadUri;
    private readonly string _destinationPath;

    private CancellationTokenSource? _cts;
    private bool _cerrarPorFlujo;

    public frmDownloadProgress(HttpClient http, Uri downloadUri, string destinationPath)
    {
        _http = http;
        _downloadUri = downloadUri;
        _destinationPath = destinationPath;
        InitializeComponent();
    }

    private void frmDownloadProgress_Shown(object? sender, EventArgs e)
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        var progress = new Progress<DownloadProgressReport>(ActualizarProgreso);
        _ = EjecutarDescargaAsync(progress, token);
    }

    private void ActualizarProgreso(DownloadProgressReport r)
    {
        if (r.TotalBytes is long tot && tot > 0)
        {
            if (progressBar.Style != ProgressBarStyle.Continuous)
            {
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.MarqueeAnimationSpeed = 0;
            }

            progressBar.Maximum = 100;
            var pct = (int)Math.Min(100, r.BytesRead * 100.0 / tot);
            progressBar.Value = pct;
            lblEstado.Text = $"Descargando… {pct} %";
        }
        else
        {
            if (progressBar.Style != ProgressBarStyle.Marquee)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.MarqueeAnimationSpeed = 30;
            }

            var mb = r.BytesRead / (1024.0 * 1024.0);
            lblEstado.Text = $"Descargando… {mb:F1} MB";
        }
    }

    private async Task EjecutarDescargaAsync(IProgress<DownloadProgressReport> progress, CancellationToken token)
    {
        try
        {
            await AppUpdateService.DownloadToFileAsync(_http, _downloadUri, _destinationPath, progress, token)
                .ConfigureAwait(true);
            if (IsDisposed) return;
            _cerrarPorFlujo = true;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (OperationCanceledException)
        {
            if (IsDisposed) return;
            BorrarParcial();
            _cerrarPorFlujo = true;
            DialogResult = DialogResult.Cancel;
            Close();
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            BorrarParcial();
            MessageBox.Show(this,
                ex.Message,
                DownloadErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            _cerrarPorFlujo = true;
            DialogResult = DialogResult.Abort;
            Close();
        }
    }

    private void BorrarParcial()
    {
        try
        {
            if (File.Exists(_destinationPath))
                File.Delete(_destinationPath);
        }
        catch
        {
            // ignorar
        }
    }

    private void btnCancelar_Click(object? sender, EventArgs e)
    {
        btnCancelar.Enabled = false;
        try
        {
            _cts?.Cancel();
        }
        catch
        {
            // ignorar
        }
    }

    private void frmDownloadProgress_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_cerrarPorFlujo)
            return;

        if (_cts is { IsCancellationRequested: false })
        {
            try
            {
                _cts.Cancel();
            }
            catch
            {
                // ignorar
            }
        }

        BorrarParcial();
        if (DialogResult == DialogResult.None)
            DialogResult = DialogResult.Cancel;
    }
}
