using MedicalTracker;
using MedicalTracker.Data;
using MedicalTracker.Models;
using MedicalTracker.Services;

namespace MedicalTracker.Forms;

/// <summary>
/// Listado principal de pacientes con filtros, colores y acceso a edición y exportación.
/// </summary>
public partial class frmMain : Form
{
    private readonly PacienteRepository _repoPacientes = new();
    private readonly ExcelExportService _export = new();

    private readonly BindingSource _binding = new();

    public frmMain()
    {
        InitializeComponent();
        var tips = new ToolTip();
        tips.SetToolTip(txtBuscar,
            "Busca en nombre, patología u observaciones del paciente (se actualiza al escribir). " +
            "Los filtros de tipo de estudio, estado y fechas se combinan: si no ve resultados, use «Limpiar».");
        Branding.AplicarIcono(this);
        picLogo.Image = Branding.CargarLogo();
        InicializarComboFiltros();
        ConfigurarColumnasGrid();
        dgvPacientes.DataSource = _binding;
    }

    private void InicializarComboFiltros()
    {
        cmbFiltroEstado.Items.Add("(Todos)");
        foreach (var e in EstudioCatalogo.Estados)
            cmbFiltroEstado.Items.Add(e);
        cmbFiltroEstado.SelectedIndex = 0;

        cmbFiltroTipo.Items.Add("(Todos)");
        foreach (var t in EstudioCatalogo.Tipos)
            cmbFiltroTipo.Items.Add(t);
        cmbFiltroTipo.SelectedIndex = 0;
    }

    private void ConfigurarColumnasGrid()
    {
        dgvPacientes.AutoGenerateColumns = false;
        dgvPacientes.Columns.Clear();

        dgvPacientes.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PacienteListaItem.NombreApellido),
            HeaderText = "Nombre",
            FillWeight = 200
        });
        dgvPacientes.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PacienteListaItem.Patologia),
            HeaderText = "Patología",
            FillWeight = 150
        });
        dgvPacientes.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PacienteListaItem.EstudiosPendientes),
            HeaderText = "Estudios pendientes",
            FillWeight = 80
        });
        dgvPacientes.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PacienteListaItem.ProximoTurno),
            HeaderText = "Próximo turno",
            FillWeight = 90
        });
        dgvPacientes.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PacienteListaItem.EstadoGeneral),
            HeaderText = "Estado general",
            FillWeight = 120
        });
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        try
        {
            RefrescarListado();
            var vencidos = _repoPacientes.ContarPacientesConEstudiosVencidos();
            MessageBox.Show(this,
                $"Resumen al iniciar:{Environment.NewLine}{Environment.NewLine}" +
                $"Pacientes con al menos un estudio vencido (fecha de turno pasada y aún no realizado): {vencidos}",
                "Resumen",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"No se pudo cargar el listado inicial.{Environment.NewLine}{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private FiltroPacienteListado ConstruirFiltroActual()
    {
        var f = new FiltroPacienteListado
        {
            TextoNombre = string.IsNullOrWhiteSpace(txtBuscar.Text) ? null : txtBuscar.Text.Trim()
        };

        if (cmbFiltroEstado.SelectedIndex > 0 && cmbFiltroEstado.SelectedItem is string est)
            f.EstadoEstudio = est;

        if (cmbFiltroTipo.SelectedIndex > 0 && cmbFiltroTipo.SelectedItem is string tipo)
            f.TipoEstudio = tipo;

        if (dtpFechaDesde.Checked)
            f.FechaDesde = dtpFechaDesde.Value.Date;
        if (dtpFechaHasta.Checked)
            f.FechaHasta = dtpFechaHasta.Value.Date;

        return f;
    }

    private void RefrescarListado()
    {
        var datos = _repoPacientes.ObtenerListaResumen(ConstruirFiltroActual());
        var lista = datos.ToList();

        // Sustituir la lista y notificar explícitamente (evita que el grid no repinte en algunos casos).
        _binding.DataSource = lista;
        _binding.ResetBindings(false);
        dgvPacientes.Refresh();

        var vencidos = _repoPacientes.ContarPacientesConEstudiosVencidos();
        var barra = $"Pacientes mostrados: {lista.Count}  |  Con estudios vencidos (total base): {vencidos}";
        if (lista.Count == 0 && HayFiltrosRestrictivosAdemasDelTexto())
            barra += "  —  Si esperaba ver filas, pruebe «Limpiar» o desactive tipo/fechas: todos los filtros se combinan.";
        lblEstadoBarra.Text = barra;
    }

    /// <summary>True si hay filtros por estado, tipo o rango de fechas (además del cuadro de búsqueda).</summary>
    private bool HayFiltrosRestrictivosAdemasDelTexto()
    {
        if (cmbFiltroEstado.SelectedIndex > 0) return true;
        if (cmbFiltroTipo.SelectedIndex > 0) return true;
        if (dtpFechaDesde.Checked || dtpFechaHasta.Checked) return true;
        return false;
    }

    private void txtBuscar_TextChanged(object? sender, EventArgs e)
    {
        try
        {
            RefrescarListado();
        }
        catch (Exception ex)
        {
            lblEstadoBarra.Text = $"Error al filtrar: {ex.Message}";
        }
    }

    private void btnAplicarFiltros_Click(object? sender, EventArgs e)
    {
        try
        {
            RefrescarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnLimpiarFiltros_Click(object? sender, EventArgs e)
    {
        txtBuscar.Clear();
        cmbFiltroEstado.SelectedIndex = 0;
        cmbFiltroTipo.SelectedIndex = 0;
        dtpFechaDesde.Checked = false;
        dtpFechaHasta.Checked = false;
        RefrescarListado();
    }

    private void btnNuevo_Click(object? sender, EventArgs e)
    {
        AbrirEditorPaciente(null);
    }

    private void btnEditar_Click(object? sender, EventArgs e)
    {
        var id = ObtenerIdSeleccionado();
        if (id == null)
        {
            MessageBox.Show(this, "Seleccione un paciente.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        AbrirEditorPaciente(id.Value);
    }

    private int? ObtenerIdSeleccionado()
    {
        if (dgvPacientes.CurrentRow?.DataBoundItem is not PacienteListaItem item)
            return null;
        return item.Id;
    }

    private void AbrirEditorPaciente(int? id)
    {
        var f = id.HasValue ? new frmPaciente(id.Value) : new frmPaciente();
        f.Owner = this;
        f.FormClosed += (_, _) => RefrescarListadoSeguro();
        f.Show(this);
    }

    private void RefrescarListadoSeguro()
    {
        try
        {
            RefrescarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error al actualizar listado",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void btnEliminar_Click(object? sender, EventArgs e)
    {
        var id = ObtenerIdSeleccionado();
        if (id == null)
        {
            MessageBox.Show(this, "Seleccione un paciente.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var r = MessageBox.Show(this,
            "¿Confirma eliminar el paciente seleccionado y todos sus estudios?",
            "Confirmar eliminación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (r != DialogResult.Yes) return;

        try
        {
            _repoPacientes.Eliminar(id.Value);
            RefrescarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"No se pudo eliminar el registro.{Environment.NewLine}{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void tsBtnBuscarActualizacion_Click(object? sender, EventArgs e)
    {
        tsBtnBuscarActualizacion.Enabled = false;
        try
        {
            await AppUpdateService.RunUpdateCheckAsync(this);
        }
        finally
        {
            tsBtnBuscarActualizacion.Enabled = true;
        }
    }

    private void btnExportar_Click(object? sender, EventArgs e)
    {
        using var dlg = new SaveFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = $"Pacientes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        using var opt = new frmExport();
        if (opt.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _export.Exportar(dlg.FileName, opt.UnaHojaPorPaciente);
            MessageBox.Show(this, "Exportación finalizada correctamente.", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"Error al exportar.{Environment.NewLine}{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void dgvPacientes_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        foreach (DataGridViewRow row in dgvPacientes.Rows)
        {
            if (row.DataBoundItem is not PacienteListaItem item) continue;

            Color fondo = item.CategoriaColor switch
            {
                CategoriaListadoPaciente.Vencido => Color.MistyRose,
                CategoriaListadoPaciente.TodoPendiente => Color.LightYellow,
                CategoriaListadoPaciente.TodoRealizado => Color.Honeydew,
                CategoriaListadoPaciente.SinEstudios => Color.WhiteSmoke,
                _ => Color.White
            };

            row.DefaultCellStyle.BackColor = fondo;
        }
    }

    private void dgvPacientes_CellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (dgvPacientes.Rows[e.RowIndex].DataBoundItem is not PacienteListaItem item) return;
        e.ToolTipText = item.DetalleEstudios;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        foreach (Form f in Application.OpenForms)
        {
            if (f is not frmPaciente fp || fp.IsDisposed) continue;
            if (!fp.TieneCambiosSinGuardar) continue;

            var r = MessageBox.Show(this,
                "Hay ventanas de paciente con cambios sin guardar. ¿Desea salir de todos modos? " +
                "(Se perderán esos cambios.)",
                "Confirmar salida",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (r == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            break;
        }

        base.OnFormClosing(e);
    }
}
