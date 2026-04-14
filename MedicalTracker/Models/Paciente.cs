namespace MedicalTracker.Models;

/// <summary>
/// Entidad paciente almacenada en la tabla Pacientes.
/// </summary>
public class Paciente
{
    public int Id { get; set; }

    /// <summary>Nombre completo del paciente.</summary>
    public string NombreApellido { get; set; } = string.Empty;

    public string? Patologia { get; set; }

    /// <summary>Fecha de ingreso en formato ISO 8601 (fecha, ej. yyyy-MM-dd).</summary>
    public string? FechaIngreso { get; set; }

    public string? Observaciones { get; set; }
}
