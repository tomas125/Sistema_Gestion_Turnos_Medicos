using MedicalTracker.Forms;

namespace MedicalTracker;

static class Program
{
    /// <summary>
    /// Punto de entrada: inicializa configuración WinForms, crea la BD si hace falta y abre el formulario principal.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            Data.DatabaseHelper.InicializarBaseDatos();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo inicializar la base de datos.{Environment.NewLine}{ex.Message}",
                "Error de base de datos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Application.Run(new frmMain());
    }
}
