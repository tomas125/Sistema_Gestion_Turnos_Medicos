namespace MedicalTracker.Models;

/// <summary>
/// Criterios de filtrado del listado principal.
/// </summary>
public class FiltroPacienteListado
{
    /// <summary>Texto libre: nombre, patología u observaciones del paciente (contiene, sin distinguir mayúsculas).</summary>
    public string? TextoNombre { get; set; }

    /// <summary>Filtro por estado de algún estudio del paciente; null = todos.</summary>
    public string? EstadoEstudio { get; set; }

    /// <summary>Filtro por tipo de estudio; null = todos.</summary>
    public string? TipoEstudio { get; set; }

    /// <summary>Inclusive: pacientes con algún turno en o después de esta fecha.</summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>Inclusive: pacientes con algún turno en o antes de esta fecha.</summary>
    public DateTime? FechaHasta { get; set; }
}
