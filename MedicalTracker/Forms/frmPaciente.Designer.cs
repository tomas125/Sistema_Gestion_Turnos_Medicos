namespace MedicalTracker.Forms;

partial class frmPaciente
{
    private System.ComponentModel.IContainer components = null;
    private GroupBox grpDatosPaciente = null!;
    private Label lblNombre = null!;
    private TextBox txtNombre = null!;
    private Label lblNumeroDni = null!;
    private TextBox txtNumeroDni = null!;
    private Label lblFechaNacimiento = null!;
    private DateTimePicker dtpFechaNacimiento = null!;
    private Label lblPatologia = null!;
    private TextBox txtPatologia = null!;
    private Label lblFechaIngreso = null!;
    private DateTimePicker dtpFechaIngreso = null!;
    private Label lblObsPaciente = null!;
    private TextBox txtObservacionesPaciente = null!;
    private GroupBox grpEstudios = null!;
    private Panel panelEstudios = null!;
    private FlowLayoutPanel panelBotones = null!;
    private Button btnGuardar = null!;
    private Button btnCancelar = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        grpDatosPaciente = new GroupBox();
        lblNombre = new Label();
        txtNombre = new TextBox();
        lblNumeroDni = new Label();
        txtNumeroDni = new TextBox();
        lblFechaNacimiento = new Label();
        dtpFechaNacimiento = new DateTimePicker();
        lblPatologia = new Label();
        txtPatologia = new TextBox();
        lblFechaIngreso = new Label();
        dtpFechaIngreso = new DateTimePicker();
        lblObsPaciente = new Label();
        txtObservacionesPaciente = new TextBox();
        grpEstudios = new GroupBox();
        panelEstudios = new Panel();
        panelBotones = new FlowLayoutPanel();
        btnGuardar = new Button();
        btnCancelar = new Button();
        grpDatosPaciente.SuspendLayout();
        grpEstudios.SuspendLayout();
        panelBotones.SuspendLayout();
        SuspendLayout();
        //
        // grpDatosPaciente
        //
        grpDatosPaciente.Controls.Add(lblNombre);
        grpDatosPaciente.Controls.Add(txtNombre);
        grpDatosPaciente.Controls.Add(lblNumeroDni);
        grpDatosPaciente.Controls.Add(txtNumeroDni);
        grpDatosPaciente.Controls.Add(lblFechaNacimiento);
        grpDatosPaciente.Controls.Add(dtpFechaNacimiento);
        grpDatosPaciente.Controls.Add(lblPatologia);
        grpDatosPaciente.Controls.Add(txtPatologia);
        grpDatosPaciente.Controls.Add(lblFechaIngreso);
        grpDatosPaciente.Controls.Add(dtpFechaIngreso);
        grpDatosPaciente.Controls.Add(lblObsPaciente);
        grpDatosPaciente.Controls.Add(txtObservacionesPaciente);
        grpDatosPaciente.Dock = DockStyle.Top;
        grpDatosPaciente.Size = new Size(860, 264);
        grpDatosPaciente.Text = "Datos del paciente";
        //
        // lblNombre
        //
        lblNombre.AutoSize = true;
        lblNombre.Location = new Point(16, 32);
        lblNombre.Text = "Nombre y apellido:";
        //
        // txtNombre
        //
        txtNombre.Location = new Point(160, 28);
        txtNombre.Size = new Size(680, 23);
        //
        // lblNumeroDni
        //
        lblNumeroDni.AutoSize = true;
        lblNumeroDni.Location = new Point(16, 64);
        lblNumeroDni.Text = "Número de DNI:";
        //
        // txtNumeroDni
        //
        txtNumeroDni.Location = new Point(160, 60);
        txtNumeroDni.Size = new Size(220, 23);
        //
        // lblFechaNacimiento
        //
        lblFechaNacimiento.AutoSize = true;
        lblFechaNacimiento.Location = new Point(400, 64);
        lblFechaNacimiento.Text = "Fecha de nacimiento:";
        //
        // dtpFechaNacimiento
        //
        dtpFechaNacimiento.Format = DateTimePickerFormat.Short;
        dtpFechaNacimiento.Location = new Point(528, 60);
        dtpFechaNacimiento.ShowCheckBox = true;
        dtpFechaNacimiento.Checked = false;
        //
        // lblPatologia
        //
        lblPatologia.AutoSize = true;
        lblPatologia.Location = new Point(16, 96);
        lblPatologia.Text = "Patología:";
        //
        // txtPatologia
        //
        txtPatologia.Location = new Point(160, 92);
        txtPatologia.Size = new Size(680, 23);
        //
        // lblFechaIngreso
        //
        lblFechaIngreso.AutoSize = true;
        lblFechaIngreso.Location = new Point(16, 128);
        lblFechaIngreso.Text = "Fecha de ingreso:";
        //
        // dtpFechaIngreso
        //
        dtpFechaIngreso.Format = DateTimePickerFormat.Short;
        dtpFechaIngreso.Location = new Point(160, 124);
        dtpFechaIngreso.ShowCheckBox = true;
        dtpFechaIngreso.Checked = false;
        //
        // lblObsPaciente
        //
        lblObsPaciente.AutoSize = true;
        lblObsPaciente.Location = new Point(16, 160);
        lblObsPaciente.Text = "Observaciones:";
        //
        // txtObservacionesPaciente
        //
        txtObservacionesPaciente.Location = new Point(160, 156);
        txtObservacionesPaciente.Multiline = true;
        txtObservacionesPaciente.ScrollBars = ScrollBars.Vertical;
        txtObservacionesPaciente.Size = new Size(680, 92);
        //
        // grpEstudios
        //
        grpEstudios.Controls.Add(panelEstudios);
        grpEstudios.Dock = DockStyle.Fill;
        grpEstudios.MinimumSize = new Size(400, 200);
        grpEstudios.Text = "Estudios prequirúrgicos y turno de cirugía";
        //
        // panelEstudios
        //
        panelEstudios.AutoScroll = true;
        panelEstudios.Dock = DockStyle.Fill;
        panelEstudios.Padding = new Padding(8);
        //
        // panelBotones
        //
        panelBotones.AutoSize = true;
        panelBotones.Dock = DockStyle.Bottom;
        panelBotones.FlowDirection = FlowDirection.RightToLeft;
        panelBotones.Padding = new Padding(8);
        panelBotones.Controls.Add(btnCancelar);
        panelBotones.Controls.Add(btnGuardar);
        //
        // btnGuardar
        //
        btnGuardar.Text = "Guardar";
        btnGuardar.AutoSize = true;
        btnGuardar.Click += btnGuardar_Click;
        //
        // btnCancelar
        //
        btnCancelar.Text = "Cancelar";
        btnCancelar.AutoSize = true;
        btnCancelar.Click += btnCancelar_Click;
        //
        // frmPaciente
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(884, 601);
        // Dock: Fill estudios, datos arriba, barra inferior.
        Controls.Add(grpEstudios);
        Controls.Add(grpDatosPaciente);
        Controls.Add(panelBotones);
        MinimumSize = new Size(700, 500);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Paciente";
        grpDatosPaciente.ResumeLayout(false);
        grpDatosPaciente.PerformLayout();
        grpEstudios.ResumeLayout(false);
        panelBotones.ResumeLayout(false);
        panelBotones.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
