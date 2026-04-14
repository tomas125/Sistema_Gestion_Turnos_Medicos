namespace MedicalTracker;

/// <summary>
/// Carga del icono y logo copiados junto al ejecutable (carpeta IMAGENES del proyecto).
/// </summary>
public static class Branding
{
    public const string NombreArchivoIcono = "logo.ico";
    public const string NombreArchivoLogo = "logo.png";

    /// <summary>Ruta de un recurso de marca en el directorio de la aplicación.</summary>
    public static string RutaEnSalida(string nombreArchivo) =>
        Path.Combine(AppContext.BaseDirectory, nombreArchivo);

    /// <summary>Asigna el icono de ventana y barra de tareas si existe logo.ico.</summary>
    public static void AplicarIcono(Form formulario)
    {
        var ruta = RutaEnSalida(NombreArchivoIcono);
        if (!File.Exists(ruta)) return;

        try
        {
            // Constructor con ruta: no mantiene el archivo abierto.
            formulario.Icon = new Icon(ruta);
        }
        catch
        {
            // Icono dañado o formato no soportado: se deja el icono por defecto.
        }
    }

    /// <summary>Carga el logo PNG en memoria (sin bloquear el archivo en disco).</summary>
    public static Image? CargarLogo()
    {
        var ruta = RutaEnSalida(NombreArchivoLogo);
        if (!File.Exists(ruta)) return null;

        try
        {
            using var fs = File.OpenRead(ruta);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Position = 0;
            return Image.FromStream(ms);
        }
        catch
        {
            return null;
        }
    }
}
