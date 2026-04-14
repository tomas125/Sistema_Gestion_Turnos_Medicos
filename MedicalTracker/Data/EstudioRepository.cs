using MedicalTracker.Models;
using Microsoft.Data.Sqlite;

namespace MedicalTracker.Data;

/// <summary>
/// Acceso a datos de la tabla Estudios.
/// </summary>
public class EstudioRepository
{
    /// <summary>Obtiene todos los estudios de un paciente ordenados por tipo.</summary>
    public IReadOnlyList<Estudio> ObtenerPorPacienteId(int pacienteId)
    {
        var lista = new List<Estudio>();
        using var conexion = new SqliteConnection(DatabaseHelper.ObtenerCadenaConexion());
        conexion.Open();
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT Id, PacienteId, Tipo, FechaTurno, Estado, SeRealizo, Observaciones
            FROM Estudios
            WHERE PacienteId = @pid
            ORDER BY Tipo;
            """;
        cmd.Parameters.AddWithValue("@pid", pacienteId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(Mapear(reader));
        }

        return lista;
    }

    /// <summary>Mapa PacienteId -> lista de estudios (una sola consulta).</summary>
    public Dictionary<int, List<Estudio>> ObtenerTodosAgrupadosPorPaciente()
    {
        var mapa = new Dictionary<int, List<Estudio>>();
        using var conexion = new SqliteConnection(DatabaseHelper.ObtenerCadenaConexion());
        conexion.Open();
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT Id, PacienteId, Tipo, FechaTurno, Estado, SeRealizo, Observaciones
            FROM Estudios
            ORDER BY PacienteId, Tipo;
            """;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var e = Mapear(reader);
            if (!mapa.TryGetValue(e.PacienteId, out var lista))
            {
                lista = new List<Estudio>();
                mapa[e.PacienteId] = lista;
            }

            lista.Add(e);
        }

        return mapa;
    }

    /// <summary>Elimina todos los estudios de un paciente (usado antes de reinsertar en guardado completo).</summary>
    public void EliminarPorPacienteId(SqliteConnection conexion, SqliteTransaction? tx, int pacienteId)
    {
        using var cmd = conexion.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "DELETE FROM Estudios WHERE PacienteId = @pid;";
        cmd.Parameters.AddWithValue("@pid", pacienteId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Inserta un estudio dentro de una transacción existente.</summary>
    public void Insertar(SqliteConnection conexion, SqliteTransaction? tx, Estudio estudio)
    {
        using var cmd = conexion.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO Estudios (PacienteId, Tipo, FechaTurno, Estado, SeRealizo, Observaciones)
            VALUES (@PacienteId, @Tipo, @FechaTurno, @Estado, @SeRealizo, @Observaciones);
            """;
        cmd.Parameters.AddWithValue("@PacienteId", estudio.PacienteId);
        cmd.Parameters.AddWithValue("@Tipo", estudio.Tipo);
        cmd.Parameters.AddWithValue("@FechaTurno", (object?)estudio.FechaTurno ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Estado", estudio.Estado);
        cmd.Parameters.AddWithValue("@SeRealizo", estudio.SeRealizo);
        cmd.Parameters.AddWithValue("@Observaciones", (object?)estudio.Observaciones ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    private static Estudio Mapear(SqliteDataReader reader)
    {
        return new Estudio
        {
            Id = reader.GetInt32(0),
            PacienteId = reader.GetInt32(1),
            Tipo = reader.GetString(2),
            FechaTurno = reader.IsDBNull(3) ? null : reader.GetString(3),
            Estado = reader.GetString(4),
            SeRealizo = reader.GetInt32(5),
            Observaciones = reader.IsDBNull(6) ? null : reader.GetString(6)
        };
    }
}
