namespace MedicalTracker.Models;

/// <summary>
/// Valores fijos de tipos de estudio y estados permitidos en la base de datos.
/// </summary>
public static class EstudioCatalogo
{
    /// <summary>
    /// Tipos de estudio según la guía operativa (croquis prequirúrgico).
    /// Orden: estudios de la guía y al final turno de cirugía.
    /// </summary>
    public static readonly string[] Tipos =
    {
        "Interconsulta con especialista",
        "Cardiología (Prequirúrgico)",
        "Anestesista (Prequirúrgico)",
        "Estudio Diagnóstico por Imagen",
        "Laboratorio",
        "Turno Cirugía"
    };

    /// <summary>Estados posibles de un estudio.</summary>
    public static readonly string[] Estados =
    {
        "Pendiente",
        "En curso",
        "Realizado",
        "Cancelado"
    };

    public const string EstadoPendiente = "Pendiente";
    public const string EstadoEnCurso = "En curso";
    public const string EstadoRealizado = "Realizado";
    public const string EstadoCancelado = "Cancelado";

    /// <summary>Prefijo en observaciones cuando el estudio se guarda sin marcar «Requiere» (queda en historial como cancelado/no aplica).</summary>
    public const string MarcadorSinRequerimiento = "[Sin requerimiento]";

    /// <summary>Texto para listados/exportación (oculta el marcador interno).</summary>
    public static string FormatearObservacionesParaMostrar(string? observaciones)
    {
        if (string.IsNullOrWhiteSpace(observaciones)) return "—";
        if (observaciones.StartsWith(MarcadorSinRequerimiento, StringComparison.Ordinal))
        {
            var resto = observaciones.Length > MarcadorSinRequerimiento.Length
                ? observaciones[MarcadorSinRequerimiento.Length..].TrimStart()
                : "";
            return string.IsNullOrEmpty(resto) ? "No requerido" : $"No requerido: {resto}";
        }

        return observaciones;
    }
}
