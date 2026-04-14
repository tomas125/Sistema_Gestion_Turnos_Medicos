namespace MedicalTracker.Models;

/// <summary>
/// Estudio o trámite asociado a un paciente (tabla Estudios).
/// </summary>
public class Estudio
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    /// <summary>Uno de los valores de <see cref="EstudioCatalogo.Tipos"/> (guía prequirúrgica).</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Fecha del turno agendada (texto ISO o vacío).</summary>
    public string? FechaTurno { get; set; }

    /// <summary>Pendiente, En curso, Realizado o Cancelado.</summary>
    public string Estado { get; set; } = EstudioCatalogo.EstadoPendiente;

    /// <summary>0 = No, 1 = Sí.</summary>
    public int SeRealizo { get; set; }

    public string? Observaciones { get; set; }
}
