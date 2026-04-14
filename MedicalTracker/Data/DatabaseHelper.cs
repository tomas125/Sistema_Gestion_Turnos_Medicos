using Microsoft.Data.Sqlite;

namespace MedicalTracker.Data;

/// <summary>
/// Inicialización de SQLite y utilidades de conexión.
/// La base se crea automáticamente en %LocalAppData%\MedicalTracker.
/// </summary>
public static class DatabaseHelper
{
    /// <summary>Ruta física del archivo .db.</summary>
    public static string ObtenerRutaBaseDatos()
    {
        var carpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MedicalTracker");
        Directory.CreateDirectory(carpeta);
        return Path.Combine(carpeta, "medicaltracker.db");
    }

    /// <summary>Cadena de conexión para Microsoft.Data.Sqlite.</summary>
    public static string ObtenerCadenaConexion()
    {
        return new SqliteConnectionStringBuilder
        {
            DataSource = ObtenerRutaBaseDatos(),
            Mode = SqliteOpenMode.ReadWriteCreate,
            // Activa PRAGMA foreign_keys en cada conexión (necesario para ON DELETE CASCADE).
            ForeignKeys = true
        }.ToString();
    }

    /// <summary>Crea tablas e índices si no existen.</summary>
    public static void InicializarBaseDatos()
    {
        using var conexion = new SqliteConnection(ObtenerCadenaConexion());
        conexion.Open();

        using (var cmd = conexion.CreateCommand())
        {
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
        }

        using (var cmd = conexion.CreateCommand())
        {
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Pacientes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    NombreApellido TEXT NOT NULL,
                    Patologia TEXT,
                    FechaIngreso TEXT,
                    Observaciones TEXT
                );
                """;
            cmd.ExecuteNonQuery();
        }

        using (var cmd = conexion.CreateCommand())
        {
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Estudios (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PacienteId INTEGER NOT NULL,
                    Tipo TEXT NOT NULL,
                    FechaTurno TEXT,
                    Estado TEXT NOT NULL,
                    SeRealizo INTEGER NOT NULL DEFAULT 0,
                    Observaciones TEXT,
                    FOREIGN KEY (PacienteId) REFERENCES Pacientes(Id) ON DELETE CASCADE
                );
                """;
            cmd.ExecuteNonQuery();
        }

        using (var cmd = conexion.CreateCommand())
        {
            cmd.CommandText = """
                CREATE UNIQUE INDEX IF NOT EXISTS IX_Estudios_Paciente_Tipo
                ON Estudios(PacienteId, Tipo);
                """;
            cmd.ExecuteNonQuery();
        }

        // Alineación con la guía: renombra tipos antiguos si ya había datos cargados.
        MigrarTiposEstudioGuiLegacy(conexion);
    }

    /// <summary>
    /// Actualiza textos de Tipo en filas guardadas antes del cambio de nomenclatura (guía en papel).
    /// </summary>
    private static void MigrarTiposEstudioGuiLegacy(SqliteConnection conexion)
    {
        var reemplazos = new (string Viejo, string Nuevo)[]
        {
            ("Cardiología", "Cardiología (Prequirúrgico)"),
            ("Anestesista", "Anestesista (Prequirúrgico)")
        };

        foreach (var (viejo, nuevo) in reemplazos)
        {
            using var cmd = conexion.CreateCommand();
            cmd.CommandText = "UPDATE Estudios SET Tipo = @nuevo WHERE Tipo = @viejo;";
            cmd.Parameters.AddWithValue("@nuevo", nuevo);
            cmd.Parameters.AddWithValue("@viejo", viejo);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>Ejecuta una acción dentro de una transacción (commit o rollback automático).</summary>
    public static void EjecutarEnTransaccion(Action<SqliteConnection, SqliteTransaction> accion)
    {
        using var conexion = new SqliteConnection(ObtenerCadenaConexion());
        conexion.Open();
        using var tx = conexion.BeginTransaction();
        try
        {
            accion(conexion, tx);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
