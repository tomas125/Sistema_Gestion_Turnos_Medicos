namespace MedicalTracker.Models;

/// <summary>
/// Entidad paciente almacenada en la tabla Pacientes.
/// </summary>
public class Paciente
{
    public int Id { get; set; }

    /// <summary>Nombre completo del paciente.</summary>
    public string NombreApellido { get; set; } = string.Empty;

    /// <summary>Número de DNI (texto para permitir formatos con puntos o sin ellos).</summary>
    public string? NumeroDni { get; set; }

    /// <summary>Fecha de nacimiento en formato ISO 8601 (fecha, ej. yyyy-MM-dd).</summary>
    public string? FechaNacimiento { get; set; }

    public string? Patologia { get; set; }

    /// <summary>Fecha de ingreso en formato ISO 8601 (fecha, ej. yyyy-MM-dd).</summary>
    public string? FechaIngreso { get; set; }

    public string? Observaciones { get; set; }
}
