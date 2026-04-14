namespace MedicalTracker.Models;

/// <summary>
/// Valores fijos de tipos de estudio y estados permitidos en la base de datos.
/// </summary>
public static class EstudioCatalogo
{
    /// <summary>
    /// Tipos de estudio según la guía operativa (croquis prequirúrgico).
    /// Orden: estudios con seguimiento “¿Se realizó?” y al final turno de cirugía.
    /// </summary>
    public static readonly string[] Tipos =
    {
        "Interconsulta con especialista",
        "Cardiología (Prequirúrgico)",
        "Anestesista (Prequirúrgico)",
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
}
