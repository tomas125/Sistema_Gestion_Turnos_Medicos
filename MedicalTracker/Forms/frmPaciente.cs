using System.Globalization;
using MedicalTracker;
using MedicalTracker.Data;
using MedicalTracker.Models;

namespace MedicalTracker.Forms;

/// <summary>
/// Alta o edición de paciente y sus cinco tipos de estudio posibles.
/// </summary>
public partial class frmPaciente : Form
{
    private readonly PacienteRepository _repoPacientes = new();
    private readonly EstudioRepository _repoEstudios = new();

    private readonly int? _pacienteIdExistente;
    private readonly List<FilaEstudioUi> _filasEstudio = new();

    private bool _cargando;
    private bool _modificado;
    private bool _guardado;

    /// <summary>
    /// Indica si hay cambios locales sin persistir (para el cierre del formulario principal).
    /// </summary>
    public bool TieneCambiosSinGuardar => _modificado && !_guardado;

    /// <summary>Nuevo paciente.</summary>
    public frmPaciente() : this(null)
    {
    }

    /// <summary>Edición de paciente existente.</summary>
    public frmPaciente(int? pacienteId)
    {
        _pacienteIdExistente = pacienteId;
        InitializeComponent();
        Branding.AplicarIcono(this);
        ConstruirFilasEstudios();
        WireDirtyHandlers();
        Load += frmPaciente_Load;
    }

    private void frmPaciente_Load(object? sender, EventArgs e)
    {
        try
        {
            _cargando = true;
            if (_pacienteIdExistente.HasValue)
            {
                Text = "Editar paciente";
                var p = _repoPacientes.ObtenerPorId(_pacienteIdExistente.Value);
                if (p == null)
                {
                    MessageBox.Show(this, "No se encontró el paciente.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                txtNombre.Text = p.NombreApellido;
                txtPatologia.Text = p.Patologia ?? "";
                if (!string.IsNullOrWhiteSpace(p.FechaIngreso) &&
                    DateTime.TryParse(p.FechaIngreso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fi))
                {
                    dtpFechaIngreso.Checked = true;
                    dtpFechaIngreso.Value = fi.Date;
                }
                else
                {
                    dtpFechaIngreso.Checked = false;
                }

                txtObservacionesPaciente.Text = p.Observaciones ?? "";

                var estudios = _repoEstudios.ObtenerPorPacienteId(p.Id);
                var mapa = estudios.ToDictionary(x => x.Tipo, x => x);
                foreach (var fila in _filasEstudio)
                {
                    if (!mapa.TryGetValue(fila.Tipo, out var est))
                    {
                        fila.Requiere.Checked = false;
                        continue;
                    }

                    var obs = est.Observaciones ?? "";
                    var sinRequerimiento = obs.StartsWith(EstudioCatalogo.MarcadorSinRequerimiento, StringComparison.Ordinal);
                    if (sinRequerimiento)
                    {
                        var resto = obs.Length > EstudioCatalogo.MarcadorSinRequerimiento.Length
                            ? obs[EstudioCatalogo.MarcadorSinRequerimiento.Length..].TrimStart()
                            : "";
                        fila.Requiere.Checked = true;
                        fila.Estado.SelectedItem = EstudioCatalogo.EstadoCancelado;
                        fila.Observaciones.Text = resto;
                        fila.Requiere.Checked = false;
                    }
                    else
                    {
                        fila.Requiere.Checked = true;
                        fila.Estado.SelectedItem = est.Estado;
                        if (fila.Estado.SelectedIndex < 0)
                            fila.Estado.SelectedItem = EstudioCatalogo.EstadoPendiente;
                        fila.Observaciones.Text = obs;
                    }

                    if (!string.IsNullOrWhiteSpace(est.FechaTurno) &&
                        DateTime.TryParse(est.FechaTurno, CultureInfo.InvariantCulture, DateTimeStyles.None, out var ft))
                    {
                        fila.FechaTurno.Checked = true;
                        fila.FechaTurno.Value = ft.Date;
                    }
                    else
                    {
                        fila.FechaTurno.Checked = false;
                    }
                }
            }
            else
            {
                Text = "Nuevo paciente";
                foreach (var fila in _filasEstudio)
                {
                    fila.Requiere.Checked = false;
                    fila.Estado.SelectedItem = EstudioCatalogo.EstadoPendiente;
                    fila.FechaTurno.Checked = false;
                    fila.Observaciones.Clear();
                }

                dtpFechaIngreso.Checked = true;
                dtpFechaIngreso.Value = DateTime.Today;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"No se pudieron cargar los datos.{Environment.NewLine}{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
        finally
        {
            _cargando = false;
            _modificado = false;
            _guardado = false;
        }
    }

    private void ConstruirFilasEstudios()
    {
        int y = 0;
        foreach (var tipo in EstudioCatalogo.Tipos)
        {
            var fila = new FilaEstudioUi(panelEstudios, tipo, y);
            y += fila.Altura + 8;
            _filasEstudio.Add(fila);
        }
    }

    private void WireDirtyHandlers()
    {
        void marcar(object? s, EventArgs e)
        {
            if (_cargando) return;
            _modificado = true;
        }

        txtNombre.TextChanged += marcar;
        txtPatologia.TextChanged += marcar;
        dtpFechaIngreso.ValueChanged += marcar;
        txtObservacionesPaciente.TextChanged += marcar;

        foreach (var f in _filasEstudio)
        {
            f.Requiere.CheckedChanged += marcar;
            f.FechaTurno.ValueChanged += marcar;
            f.Estado.SelectedIndexChanged += marcar;
            f.Observaciones.TextChanged += marcar;
        }
    }

    private void btnGuardar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MessageBox.Show(this, "El nombre y apellido son obligatorios.", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNombre.Focus();
            return;
        }

        var paciente = new Paciente
        {
            Id = _pacienteIdExistente ?? 0,
            NombreApellido = txtNombre.Text.Trim(),
            Patologia = string.IsNullOrWhiteSpace(txtPatologia.Text) ? null : txtPatologia.Text.Trim(),
            FechaIngreso = dtpFechaIngreso.Checked
                ? dtpFechaIngreso.Value.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : null,
            Observaciones = string.IsNullOrWhiteSpace(txtObservacionesPaciente.Text)
                ? null
                : txtObservacionesPaciente.Text.Trim()
        };

        var estudios = new List<Estudio>();
        foreach (var fila in _filasEstudio)
        {
            if (!fila.Requiere.Checked)
            {
                var obsSinReq = string.IsNullOrWhiteSpace(fila.Observaciones.Text)
                    ? EstudioCatalogo.MarcadorSinRequerimiento
                    : $"{EstudioCatalogo.MarcadorSinRequerimiento} {fila.Observaciones.Text.Trim()}";
                estudios.Add(new Estudio
                {
                    Tipo = fila.Tipo,
                    FechaTurno = null,
                    Estado = EstudioCatalogo.EstadoCancelado,
                    SeRealizo = 0,
                    Observaciones = obsSinReq
                });
                continue;
            }

            var estado = fila.Estado.SelectedItem as string ?? EstudioCatalogo.EstadoPendiente;
            var fechaTxt = fila.FechaTurno.Checked
                ? fila.FechaTurno.Value.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : null;

            estudios.Add(new Estudio
            {
                Tipo = fila.Tipo,
                FechaTurno = fechaTxt,
                Estado = estado,
                SeRealizo = estado == EstudioCatalogo.EstadoRealizado ? 1 : 0,
                Observaciones = string.IsNullOrWhiteSpace(fila.Observaciones.Text)
                    ? null
                    : fila.Observaciones.Text.Trim()
            });
        }

        try
        {
            _repoPacientes.GuardarPacienteConEstudios(paciente, estudios);
            _guardado = true;
            _modificado = false;
            MessageBox.Show(this, "Los datos se guardaron correctamente.", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"No se pudo guardar la información.{Environment.NewLine}{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnCancelar_Click(object? sender, EventArgs e)
    {
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (TieneCambiosSinGuardar)
        {
            var r = MessageBox.Show(this,
                "Hay cambios sin guardar. ¿Desea descartarlos y cerrar esta ventana?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (r == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        base.OnFormClosing(e);
    }

    /// <summary>Fila de controles para un tipo de estudio fijo.</summary>
    private sealed class FilaEstudioUi
    {
        public string Tipo { get; }
        public CheckBox Requiere { get; }
        public DateTimePicker FechaTurno { get; }
        public ComboBox Estado { get; }
        public TextBox Observaciones { get; }

        public int Altura { get; }

        public FilaEstudioUi(Panel contenedor, string tipo, int top)
        {
            Tipo = tipo;
            var borde = new GroupBox
            {
                Text = tipo,
                Location = new Point(0, top),
                Size = new Size(contenedor.ClientSize.Width - 16, 112),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Requiere = new CheckBox
            {
                Text = "Requiere este estudio",
                Location = new Point(12, 24),
                AutoSize = true
            };

            var lblFecha = new Label { Text = "Fecha turno:", Location = new Point(12, 52), AutoSize = true };
            FechaTurno = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Location = new Point(120, 48),
                Width = 130,
                Checked = false
            };

            var lblEstado = new Label { Text = "Estado:", Location = new Point(270, 52), AutoSize = true };
            Estado = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(330, 48),
                Width = 260
            };
            Estado.Items.AddRange(EstudioCatalogo.Estados.Cast<object>().ToArray());
            Estado.SelectedItem = EstudioCatalogo.EstadoPendiente;

            var lblObs = new Label { Text = "Observaciones:", Location = new Point(12, 80), AutoSize = true };
            Observaciones = new TextBox
            {
                Location = new Point(120, 76),
                Width = borde.Width - 140,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            borde.Controls.Add(Requiere);
            borde.Controls.Add(lblFecha);
            borde.Controls.Add(FechaTurno);
            borde.Controls.Add(lblEstado);
            borde.Controls.Add(Estado);
            borde.Controls.Add(lblObs);
            borde.Controls.Add(Observaciones);

            contenedor.Controls.Add(borde);
            Altura = borde.Height;

            Requiere.CheckedChanged += (_, _) => ActualizarHabilitado();
            ActualizarHabilitado();
        }

        private void ActualizarHabilitado()
        {
            var on = Requiere.Checked;
            FechaTurno.Enabled = on;
            Estado.Enabled = on;
            Observaciones.Enabled = on;
        }
    }
}
