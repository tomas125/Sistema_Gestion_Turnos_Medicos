using System.Globalization;
using System.Text;
using MedicalTracker.Models;
using Microsoft.Data.Sqlite;

namespace MedicalTracker.Data;

/// <summary>
/// Acceso a datos de pacientes y armado del listado con agregados.
/// </summary>
public class PacienteRepository
{
    private readonly EstudioRepository _estudios = new();

    /// <summary>Obtiene un paciente por Id o null.</summary>
    public Paciente? ObtenerPorId(int id)
    {
        using var conexion = new SqliteConnection(DatabaseHelper.ObtenerCadenaConexion());
        conexion.Open();
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT Id, NombreApellido, Patologia, FechaIngreso, Observaciones
            FROM Pacientes WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return MapearPaciente(reader);
    }

    /// <summary>Listado de todos los pacientes (sin agregados).</summary>
    public IReadOnlyList<Paciente> ObtenerTodos()
    {
        var lista = new List<Paciente>();
        using var conexion = new SqliteConnection(DatabaseHelper.ObtenerCadenaConexion());
        conexion.Open();
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT Id, NombreApellido, Patologia, FechaIngreso, Observaciones
            FROM Pacientes
            ORDER BY NombreApellido COLLATE NOCASE;
            """;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(MapearPaciente(reader));
        }

        return lista;
    }

    /// <summary>Elimina un paciente (CASCADE elimina estudios).</summary>
    public void Eliminar(int id)
    {
        using var conexion = new SqliteConnection(DatabaseHelper.ObtenerCadenaConexion());
        conexion.Open();
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "DELETE FROM Pacientes WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Guarda paciente y reemplaza todos sus estudios en una transacción.
    /// </summary>
    public void GuardarPacienteConEstudios(Paciente paciente, IReadOnlyList<Estudio> estudiosAGuardar)
    {
        DatabaseHelper.EjecutarEnTransaccion((conexion, tx) =>
        {
            if (paciente.Id == 0)
            {
                using var cmd = conexion.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = """
                    INSERT INTO Pacientes (NombreApellido, Patologia, FechaIngreso, Observaciones)
                    VALUES (@NombreApellido, @Patologia, @FechaIngreso, @Observaciones)
                    RETURNING Id;
                    """;
                cmd.Parameters.AddWithValue("@NombreApellido", paciente.NombreApellido);
                cmd.Parameters.AddWithValue("@Patologia", (object?)paciente.Patologia ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaIngreso", (object?)paciente.FechaIngreso ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)paciente.Observaciones ?? DBNull.Value);
                var result = cmd.ExecuteScalar();
                paciente.Id = Convert.ToInt32(result, CultureInfo.InvariantCulture);
            }
            else
            {
                using var cmd = conexion.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = """
                    UPDATE Pacientes SET
                        NombreApellido = @NombreApellido,
                        Patologia = @Patologia,
                        FechaIngreso = @FechaIngreso,
                        Observaciones = @Observaciones
                    WHERE Id = @Id;
                    """;
                cmd.Parameters.AddWithValue("@Id", paciente.Id);
                cmd.Parameters.AddWithValue("@NombreApellido", paciente.NombreApellido);
                cmd.Parameters.AddWithValue("@Patologia", (object?)paciente.Patologia ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaIngreso", (object?)paciente.FechaIngreso ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)paciente.Observaciones ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }

            _estudios.EliminarPorPacienteId(conexion, tx, paciente.Id);
            foreach (var e in estudiosAGuardar)
            {
                e.PacienteId = paciente.Id;
                _estudios.Insertar(conexion, tx, e);
            }
        });
    }

    /// <summary>Construye el listado principal aplicando filtros en memoria.</summary>
    public IReadOnlyList<PacienteListaItem> ObtenerListaResumen(FiltroPacienteListado filtro)
    {
        var pacientes = ObtenerTodos();
        var mapaEstudios = _estudios.ObtenerTodosAgrupadosPorPaciente();
        var resultado = new List<PacienteListaItem>();

        foreach (var p in pacientes)
        {
            mapaEstudios.TryGetValue(p.Id, out var estudios);
            estudios ??= new List<Estudio>();

            if (!PasaFiltro(p, estudios, filtro))
                continue;

            resultado.Add(ConstruirItemLista(p, estudios));
        }

        return resultado;
    }

    /// <summary>Cantidad de pacientes con al menos un estudio vencido y no finalizado.</summary>
    public int ContarPacientesConEstudiosVencidos()
    {
        var pacientes = ObtenerTodos();
        var mapa = _estudios.ObtenerTodosAgrupadosPorPaciente();
        var count = 0;
        foreach (var p in pacientes)
        {
            if (!mapa.TryGetValue(p.Id, out var lista) || lista.Count == 0)
                continue;
            if (lista.Any(EsEstudioVencido))
                count++;
        }

        return count;
    }

    private static bool PasaFiltro(Paciente p, List<Estudio> estudios, FiltroPacienteListado f)
    {
        // Texto libre: coincide si aparece en nombre, patología u observaciones del paciente.
        if (!string.IsNullOrWhiteSpace(f.TextoNombre))
        {
            var q = f.TextoNombre.Trim();
            var enNombre = p.NombreApellido.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
            var enPatologia = !string.IsNullOrEmpty(p.Patologia) &&
                              p.Patologia.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
            var enObs = !string.IsNullOrEmpty(p.Observaciones) &&
                        p.Observaciones.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
            if (!enNombre && !enPatologia && !enObs)
                return false;
        }

        if (!string.IsNullOrEmpty(f.EstadoEstudio))
        {
            if (!estudios.Any(e => e.Estado == f.EstadoEstudio))
                return false;
        }

        if (!string.IsNullOrEmpty(f.TipoEstudio))
        {
            if (!estudios.Any(e => e.Tipo == f.TipoEstudio))
                return false;
        }

        if (f.FechaDesde.HasValue || f.FechaHasta.HasValue)
        {
            bool algunoEnRango = false;
            foreach (var e in estudios)
            {
                if (!TryParseFecha(e.FechaTurno, out var fecha)) continue;
                if (f.FechaDesde.HasValue && fecha.Date < f.FechaDesde.Value.Date) continue;
                if (f.FechaHasta.HasValue && fecha.Date > f.FechaHasta.Value.Date) continue;
                algunoEnRango = true;
                break;
            }

            if (!algunoEnRango) return false;
        }

        return true;
    }

    private static PacienteListaItem ConstruirItemLista(Paciente p, List<Estudio> estudios)
    {
        var pendientes = estudios.Count(EsPendienteActivo);
        var proximo = CalcularProximoTurno(estudios);
        var (estadoTexto, categoria) = CalcularEstadoGeneral(estudios);
        var detalle = ConstruirDetalleTooltip(estudios);

        return new PacienteListaItem
        {
            Id = p.Id,
            NombreApellido = p.NombreApellido,
            Patologia = p.Patologia,
            EstudiosPendientes = pendientes,
            ProximoTurno = proximo,
            EstadoGeneral = estadoTexto,
            DetalleEstudios = detalle,
            CategoriaColor = categoria
        };
    }

    private static string CalcularProximoTurno(List<Estudio> estudios)
    {
        DateTime? min = null;
        foreach (var e in estudios)
        {
            if (!EsPendienteActivo(e)) continue;
            if (!TryParseFecha(e.FechaTurno, out var fecha)) continue;
            if (min == null || fecha.Date < min.Value.Date)
                min = fecha.Date;
        }

        return min.HasValue ? min.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "—";
    }

    private static (string texto, CategoriaListadoPaciente categoria) CalcularEstadoGeneral(List<Estudio> estudios)
    {
        if (estudios.Count == 0)
            return ("Sin estudios", CategoriaListadoPaciente.SinEstudios);

        if (estudios.Any(EsEstudioVencido))
            return ("Con turnos vencidos", CategoriaListadoPaciente.Vencido);

        var todosRealizados = estudios.All(e =>
            e.SeRealizo == 1 || e.Estado == EstudioCatalogo.EstadoRealizado);
        if (todosRealizados)
            return ("Todo realizado", CategoriaListadoPaciente.TodoRealizado);

        var todosPendientesPuros = estudios.All(e =>
            e.Estado == EstudioCatalogo.EstadoPendiente && e.SeRealizo == 0);
        if (todosPendientesPuros)
            return ("Todo pendiente", CategoriaListadoPaciente.TodoPendiente);

        return ("En curso / mixto", CategoriaListadoPaciente.Normal);
    }

    private static string ConstruirDetalleTooltip(List<Estudio> estudios)
    {
        if (estudios.Count == 0)
            return "Sin estudios registrados.";

        var sb = new StringBuilder();
        foreach (var e in estudios.OrderBy(x => x.Tipo))
        {
            var fechaTxt = TryParseFecha(e.FechaTurno, out var fd)
                ? fd.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                : (string.IsNullOrWhiteSpace(e.FechaTurno) ? "sin fecha" : e.FechaTurno!);
            var real = e.SeRealizo == 1 ? "Sí" : "No";
            var obs = string.IsNullOrWhiteSpace(e.Observaciones) ? "—" : e.Observaciones;
            sb.AppendLine($"• {e.Tipo}");
            sb.AppendLine($"  Estado: {e.Estado} | Turno: {fechaTxt} | Realizado: {real}");
            sb.AppendLine($"  Obs.: {obs}");
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static bool EsPendienteActivo(Estudio e)
    {
        if (e.SeRealizo == 1) return false;
        if (e.Estado == EstudioCatalogo.EstadoRealizado || e.Estado == EstudioCatalogo.EstadoCancelado)
            return false;
        return true;
    }

    private static bool EsEstudioVencido(Estudio e)
    {
        if (!EsPendienteActivo(e)) return false;
        if (!TryParseFecha(e.FechaTurno, out var fecha)) return false;
        return fecha.Date < DateTime.Today.Date;
    }

    private static bool TryParseFecha(string? texto, out DateTime fecha)
    {
        fecha = default;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        return DateTime.TryParse(texto, CultureInfo.InvariantCulture,
                   DateTimeStyles.AssumeLocal, out fecha)
               || DateTime.TryParse(texto, CultureInfo.GetCultureInfo("es-AR"),
                   DateTimeStyles.AssumeLocal, out fecha);
    }

    private static Paciente MapearPaciente(SqliteDataReader reader)
    {
        return new Paciente
        {
            Id = reader.GetInt32(0),
            NombreApellido = reader.GetString(1),
            Patologia = reader.IsDBNull(2) ? null : reader.GetString(2),
            FechaIngreso = reader.IsDBNull(3) ? null : reader.GetString(3),
            Observaciones = reader.IsDBNull(4) ? null : reader.GetString(4)
        };
    }
}
