namespace MedicalTracker.Models;

/// <summary>
/// Categoría visual para colorear filas del listado principal.
/// </summary>
public enum CategoriaListadoPaciente
{
    Normal,
    Vencido,
    TodoPendiente,
    TodoRealizado,
    SinEstudios
}

/// <summary>
/// Fila del DataGridView principal con datos agregados y texto para tooltips.
/// </summary>
public class PacienteListaItem
{
    public int Id { get; set; }

    public string NombreApellido { get; set; } = string.Empty;

    public string? Patologia { get; set; }

    /// <summary>Cantidad de estudios aún no finalizados (no realizados ni cancelados).</summary>
    public int EstudiosPendientes { get; set; }

    /// <summary>Texto amigable del próximo turno pendiente (o vacío).</summary>
    public string ProximoTurno { get; set; } = string.Empty;

    /// <summary>Resumen del estado general del paciente.</summary>
    public string EstadoGeneral { get; set; } = string.Empty;

    /// <summary>Detalle multilínea para tooltips en celdas.</summary>
    public string DetalleEstudios { get; set; } = string.Empty;

    public CategoriaListadoPaciente CategoriaColor { get; set; } = CategoriaListadoPaciente.Normal;
}
