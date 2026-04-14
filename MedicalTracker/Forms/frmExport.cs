using MedicalTracker;

namespace MedicalTracker.Forms;

/// <summary>
/// Diálogo para elegir el modo de exportación a Excel (una hoja o una por paciente).
/// </summary>
public partial class frmExport : Form
{
    public frmExport()
    {
        InitializeComponent();
        Branding.AplicarIcono(this);
    }

    /// <summary>true si el usuario eligió una hoja por paciente.</summary>
    public bool UnaHojaPorPaciente => rbUnaPorPaciente.Checked;

    private void btnAceptar_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancelar_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
