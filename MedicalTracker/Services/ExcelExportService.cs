using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using MedicalTracker.Data;
using MedicalTracker.Models;

namespace MedicalTracker.Services;

/// <summary>
/// Exportación del listado completo a Excel con formato y filtros automáticos.
/// </summary>
public class ExcelExportService
{
    private readonly PacienteRepository _pacientes = new();
    private readonly EstudioRepository _estudios = new();

    /// <summary>
    /// Exporta todos los pacientes con sus estudios.
    /// </summary>
    /// <param name="rutaDestino">Ruta .xlsx</param>
    /// <param name="unaHojaPorPaciente">true: una hoja por paciente; false: una sola hoja con todos los registros.</param>
    public void Exportar(string rutaDestino, bool unaHojaPorPaciente)
    {
        var listaPacientes = _pacientes.ObtenerTodos();
        var mapa = _estudios.ObtenerTodosAgrupadosPorPaciente();

        using var libro = new XLWorkbook();
        if (unaHojaPorPaciente)
        {
            foreach (var p in listaPacientes)
            {
                mapa.TryGetValue(p.Id, out var est);
                est ??= new List<Estudio>();
                var nombreHoja = SanearNombreHoja(p.NombreApellido, p.Id);
                var hoja = libro.Worksheets.Add(nombreHoja);
                EscribirPacienteEnHoja(hoja, p, est);
            }

            if (listaPacientes.Count == 0)
            {
                var hoja = libro.Worksheets.Add("Sin datos");
                hoja.Cell(1, 1).Value = "No hay pacientes para exportar.";
            }
        }
        else
        {
            var hoja = libro.Worksheets.Add("Pacientes y estudios");
            EscribirTodoEnUnaHoja(hoja, listaPacientes, mapa);
        }

        libro.SaveAs(rutaDestino);
    }

    private static void EscribirTodoEnUnaHoja(IXLWorksheet hoja, IReadOnlyList<Paciente> pacientes,
        Dictionary<int, List<Estudio>> mapa)
    {
        int fila = 1;
        string[] encabezados =
        {
            "Paciente", "Patología", "Fecha ingreso", "Obs. paciente",
            "Tipo estudio", "Fecha turno", "Estado", "¿Realizado?", "Obs. estudio"
        };

        for (int c = 0; c < encabezados.Length; c++)
        {
            var celda = hoja.Cell(fila, c + 1);
            celda.Value = encabezados[c];
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        fila++;
        foreach (var p in pacientes)
        {
            mapa.TryGetValue(p.Id, out var est);
            if (est == null || est.Count == 0)
            {
                EscribirFilaEstudio(hoja, ref fila, p, null);
            }
            else
            {
                foreach (var e in est.OrderBy(x => x.Tipo))
                {
                    EscribirFilaEstudio(hoja, ref fila, p, e);
                }
            }
        }

        if (fila > 2)
        {
            var rango = hoja.Range(1, 1, fila - 1, encabezados.Length);
            rango.SetAutoFilter();
        }

        hoja.Columns().AdjustToContents();
    }

    private static void EscribirFilaEstudio(IXLWorksheet hoja, ref int fila, Paciente p, Estudio? e)
    {
        int c = 1;
        hoja.Cell(fila, c++).Value = p.NombreApellido;
        hoja.Cell(fila, c++).Value = p.Patologia ?? "";
        hoja.Cell(fila, c++).Value = p.FechaIngreso ?? "";
        hoja.Cell(fila, c++).Value = p.Observaciones ?? "";

        if (e == null)
        {
            hoja.Cell(fila, c++).Value = "—";
            hoja.Cell(fila, c++).Value = "";
            hoja.Cell(fila, c++).Value = "";
            hoja.Cell(fila, c++).Value = "";
            hoja.Cell(fila, c++).Value = "";
        }
        else
        {
            hoja.Cell(fila, c++).Value = e.Tipo;
            hoja.Cell(fila, c++).Value = e.FechaTurno ?? "";
            var celEstado = hoja.Cell(fila, c++);
            celEstado.Value = e.Estado;
            hoja.Cell(fila, c++).Value = e.SeRealizo == 1 ? "Sí" : "No";
            hoja.Cell(fila, c++).Value = e.Observaciones ?? "";

            AplicarColorFila(hoja, fila, e);
        }

        fila++;
    }

    private static void AplicarColorFila(IXLWorksheet hoja, int fila, Estudio e)
    {
        var rango = hoja.Range(fila, 1, fila, 9);
        if (EsPendienteNoFinalizado(e))
        {
            rango.Style.Fill.BackgroundColor = XLColor.LightPink;
        }
        else if (e.SeRealizo == 1 || e.Estado == EstudioCatalogo.EstadoRealizado)
        {
            rango.Style.Fill.BackgroundColor = XLColor.LightGreen;
        }
    }

    private static bool EsPendienteNoFinalizado(Estudio e)
    {
        if (e.SeRealizo == 1) return false;
        if (e.Estado == EstudioCatalogo.EstadoRealizado || e.Estado == EstudioCatalogo.EstadoCancelado)
            return false;
        return e.Estado == EstudioCatalogo.EstadoPendiente || e.Estado == EstudioCatalogo.EstadoEnCurso;
    }

    private static void EscribirPacienteEnHoja(IXLWorksheet hoja, Paciente p, List<Estudio> estudios)
    {
        hoja.Cell(1, 1).Value = "Paciente";
        hoja.Cell(1, 2).Value = p.NombreApellido;
        hoja.Cell(2, 1).Value = "Patología";
        hoja.Cell(2, 2).Value = p.Patologia ?? "";
        hoja.Cell(3, 1).Value = "Fecha ingreso";
        hoja.Cell(3, 2).Value = p.FechaIngreso ?? "";
        hoja.Cell(4, 1).Value = "Observaciones";
        hoja.Cell(4, 2).Value = p.Observaciones ?? "";

        hoja.Range(1, 1, 4, 1).Style.Font.Bold = true;

        int fila = 6;
        string[] enc = { "Tipo", "Fecha turno", "Estado", "¿Realizado?", "Observaciones" };
        for (int i = 0; i < enc.Length; i++)
        {
            var c = hoja.Cell(fila, i + 1);
            c.Value = enc[i];
            c.Style.Font.Bold = true;
            c.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        fila++;
        if (estudios.Count == 0)
        {
            hoja.Cell(fila, 1).Value = "Sin estudios";
        }
        else
        {
            foreach (var e in estudios.OrderBy(x => x.Tipo))
            {
                hoja.Cell(fila, 1).Value = e.Tipo;
                hoja.Cell(fila, 2).Value = e.FechaTurno ?? "";
                hoja.Cell(fila, 3).Value = e.Estado;
                hoja.Cell(fila, 4).Value = e.SeRealizo == 1 ? "Sí" : "No";
                hoja.Cell(fila, 5).Value = e.Observaciones ?? "";

                var rango = hoja.Range(fila, 1, fila, 5);
                if (EsPendienteNoFinalizado(e))
                    rango.Style.Fill.BackgroundColor = XLColor.LightPink;
                else if (e.SeRealizo == 1 || e.Estado == EstudioCatalogo.EstadoRealizado)
                    rango.Style.Fill.BackgroundColor = XLColor.LightGreen;

                fila++;
            }
        }

        var ultima = fila - 1;
        if (ultima >= 7)
        {
            hoja.Range(6, 1, ultima, 5).SetAutoFilter();
        }

        hoja.Columns().AdjustToContents();
    }

    /// <summary>Excel limita el nombre de hoja a 31 caracteres y prohíbe ciertos símbolos.</summary>
    private static string SanearNombreHoja(string nombre, int id)
    {
        var sb = new StringBuilder();
        foreach (var ch in nombre.Trim())
        {
            if (ch is '\\' or '/' or '?' or '*' or '[' or ']' or ':')
                sb.Append('_');
            else
                sb.Append(ch);
            if (sb.Length >= 24) break;
        }

        if (sb.Length == 0) sb.Append("Paciente");
        sb.Append('_').Append(id.ToString(CultureInfo.InvariantCulture));
        var resultado = sb.ToString();
        if (resultado.Length > 31)
            resultado = resultado[..31];
        return resultado;
    }
}
