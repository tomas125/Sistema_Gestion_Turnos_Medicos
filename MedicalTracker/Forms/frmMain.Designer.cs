namespace MedicalTracker.Forms;

partial class frmMain
{
    private System.ComponentModel.IContainer components = null;
    private DataGridView dgvPacientes = null!;
    private TextBox txtBuscar = null!;
    private Label lblBuscar = null!;
    private Label lblEstado = null!;
    private ComboBox cmbFiltroEstado = null!;
    private Label lblTipo = null!;
    private ComboBox cmbFiltroTipo = null!;
    private Label lblDesde = null!;
    private DateTimePicker dtpFechaDesde = null!;
    private Label lblHasta = null!;
    private DateTimePicker dtpFechaHasta = null!;
    private Button btnAplicarFiltros = null!;
    private Button btnLimpiarFiltros = null!;
    private FlowLayoutPanel panelBotones = null!;
    private Button btnNuevo = null!;
    private Button btnEditar = null!;
    private Button btnEliminar = null!;
    private Button btnExportar = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel lblEstadoBarra = null!;
    /// <summary>Encabezado en dos filas (filtros + botones) con altura correcta al redimensionar.</summary>
    private TableLayoutPanel tablaCabecera = null!;
    /// <summary>Contenedor de filtros con ajuste en varias líneas al achicar la ventana.</summary>
    private FlowLayoutPanel flowFiltros = null!;
    /// <summary>Logo institucional (logo.png junto al .exe).</summary>
    private PictureBox picLogo = null!;
    private ToolStrip toolStripPrincipal = null!;
    private ToolStripStatusLabel tsSpring = null!;
    private ToolStripButton tsBtnBuscarActualizacion = null!;

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
        dgvPacientes = new DataGridView();
        txtBuscar = new TextBox();
        lblBuscar = new Label();
        lblEstado = new Label();
        cmbFiltroEstado = new ComboBox();
        lblTipo = new Label();
        cmbFiltroTipo = new ComboBox();
        lblDesde = new Label();
        dtpFechaDesde = new DateTimePicker();
        lblHasta = new Label();
        dtpFechaHasta = new DateTimePicker();
        btnAplicarFiltros = new Button();
        btnLimpiarFiltros = new Button();
        panelBotones = new FlowLayoutPanel();
        btnNuevo = new Button();
        btnEditar = new Button();
        btnEliminar = new Button();
        btnExportar = new Button();
        statusStrip = new StatusStrip();
        lblEstadoBarra = new ToolStripStatusLabel();
        tablaCabecera = new TableLayoutPanel();
        flowFiltros = new FlowLayoutPanel();
        picLogo = new PictureBox();
        toolStripPrincipal = new ToolStrip();
        tsSpring = new ToolStripStatusLabel();
        tsBtnBuscarActualizacion = new ToolStripButton();
        ((System.ComponentModel.ISupportInitialize)dgvPacientes).BeginInit();
        ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
        panelBotones.SuspendLayout();
        statusStrip.SuspendLayout();
        tablaCabecera.SuspendLayout();
        flowFiltros.SuspendLayout();
        toolStripPrincipal.SuspendLayout();
        SuspendLayout();
        //
        // dgvPacientes
        //
        dgvPacientes.AllowUserToAddRows = false;
        dgvPacientes.AllowUserToDeleteRows = false;
        dgvPacientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvPacientes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvPacientes.Dock = DockStyle.Fill;
        dgvPacientes.MultiSelect = false;
        dgvPacientes.ReadOnly = true;
        dgvPacientes.RowHeadersVisible = false;
        dgvPacientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvPacientes.CellToolTipTextNeeded += dgvPacientes_CellToolTipTextNeeded;
        dgvPacientes.DataBindingComplete += dgvPacientes_DataBindingComplete;
        //
        // picLogo
        //
        picLogo.Dock = DockStyle.Fill;
        picLogo.Margin = new Padding(10, 10, 6, 10);
        picLogo.MinimumSize = new Size(96, 72);
        picLogo.SizeMode = PictureBoxSizeMode.Zoom;
        picLogo.TabStop = false;
        //
        // toolStripPrincipal
        //
        toolStripPrincipal.Dock = DockStyle.Top;
        toolStripPrincipal.Items.AddRange(new ToolStripItem[] { tsSpring, tsBtnBuscarActualizacion });
        //
        // tsSpring
        //
        tsSpring.Spring = true;
        tsSpring.DisplayStyle = ToolStripItemDisplayStyle.None;
        //
        // tsBtnBuscarActualizacion
        //
        tsBtnBuscarActualizacion.Alignment = ToolStripItemAlignment.Right;
        tsBtnBuscarActualizacion.AutoSize = false;
        tsBtnBuscarActualizacion.DisplayStyle = ToolStripItemDisplayStyle.Text;
        tsBtnBuscarActualizacion.Padding = new Padding(10, 0, 10, 0);
        tsBtnBuscarActualizacion.Text = "Buscar actualización";
        tsBtnBuscarActualizacion.ToolTipText = "Descarga la última versión desde internet (requiere configuración del proveedor)";
        tsBtnBuscarActualizacion.Width = 140;
        tsBtnBuscarActualizacion.Click += tsBtnBuscarActualizacion_Click;
        //
        // lblBuscar
        //
        lblBuscar.AutoSize = true;
        lblBuscar.Text = "Buscar:";
        lblBuscar.Margin = new Padding(3, 10, 6, 3);
        //
        // txtBuscar
        //
        txtBuscar.Margin = new Padding(3, 6, 12, 3);
        txtBuscar.MinimumSize = new Size(160, 0);
        txtBuscar.Width = 220;
        txtBuscar.TextChanged += txtBuscar_TextChanged;
        //
        // lblEstado
        //
        lblEstado.AutoSize = true;
        lblEstado.Text = "Estado estudio:";
        lblEstado.Margin = new Padding(12, 10, 6, 3);
        //
        // cmbFiltroEstado
        //
        cmbFiltroEstado.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFiltroEstado.Margin = new Padding(3, 4, 3, 3);
        cmbFiltroEstado.Width = 160;
        //
        // lblTipo
        //
        lblTipo.AutoSize = true;
        lblTipo.Text = "Tipo estudio:";
        lblTipo.Margin = new Padding(12, 10, 6, 3);
        //
        // cmbFiltroTipo
        //
        cmbFiltroTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFiltroTipo.Margin = new Padding(3, 4, 3, 3);
        cmbFiltroTipo.Width = 220;
        //
        // lblDesde
        //
        lblDesde.AutoSize = true;
        lblDesde.Text = "Turno desde:";
        lblDesde.Margin = new Padding(12, 10, 6, 3);
        //
        // dtpFechaDesde
        //
        dtpFechaDesde.Format = DateTimePickerFormat.Short;
        dtpFechaDesde.ShowCheckBox = true;
        dtpFechaDesde.Checked = false;
        dtpFechaDesde.Margin = new Padding(3, 4, 3, 3);
        //
        // lblHasta
        //
        lblHasta.AutoSize = true;
        lblHasta.Text = "Turno hasta:";
        lblHasta.Margin = new Padding(12, 10, 6, 3);
        //
        // dtpFechaHasta
        //
        dtpFechaHasta.Format = DateTimePickerFormat.Short;
        dtpFechaHasta.ShowCheckBox = true;
        dtpFechaHasta.Checked = false;
        dtpFechaHasta.Margin = new Padding(3, 4, 3, 3);
        //
        // btnAplicarFiltros
        //
        btnAplicarFiltros.Text = "Aplicar filtros";
        btnAplicarFiltros.AutoSize = true;
        btnAplicarFiltros.Margin = new Padding(8, 4, 3, 3);
        btnAplicarFiltros.Click += btnAplicarFiltros_Click;
        //
        // btnLimpiarFiltros
        //
        btnLimpiarFiltros.Text = "Limpiar";
        btnLimpiarFiltros.AutoSize = true;
        btnLimpiarFiltros.Margin = new Padding(3, 4, 3, 3);
        btnLimpiarFiltros.Click += btnLimpiarFiltros_Click;
        //
        // flowFiltros
        //
        flowFiltros.AutoSize = true;
        flowFiltros.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flowFiltros.Dock = DockStyle.Fill;
        flowFiltros.FlowDirection = FlowDirection.LeftToRight;
        flowFiltros.WrapContents = true;
        flowFiltros.Padding = new Padding(8, 6, 8, 4);
        flowFiltros.Controls.Add(lblBuscar);
        flowFiltros.Controls.Add(txtBuscar);
        flowFiltros.Controls.Add(lblEstado);
        flowFiltros.Controls.Add(cmbFiltroEstado);
        flowFiltros.Controls.Add(lblTipo);
        flowFiltros.Controls.Add(cmbFiltroTipo);
        flowFiltros.Controls.Add(lblDesde);
        flowFiltros.Controls.Add(dtpFechaDesde);
        flowFiltros.Controls.Add(lblHasta);
        flowFiltros.Controls.Add(dtpFechaHasta);
        //
        // panelBotones
        //
        panelBotones.AutoSize = true;
        panelBotones.Dock = DockStyle.Fill;
        panelBotones.FlowDirection = FlowDirection.LeftToRight;
        panelBotones.WrapContents = true;
        panelBotones.Padding = new Padding(8, 4, 8, 4);
        panelBotones.Controls.Add(btnNuevo);
        panelBotones.Controls.Add(btnEditar);
        panelBotones.Controls.Add(btnEliminar);
        panelBotones.Controls.Add(btnExportar);
        panelBotones.Controls.Add(btnAplicarFiltros);
        panelBotones.Controls.Add(btnLimpiarFiltros);
        //
        // btnNuevo
        //
        btnNuevo.Text = "Nuevo paciente";
        btnNuevo.AutoSize = true;
        btnNuevo.Click += btnNuevo_Click;
        //
        // btnEditar
        //
        btnEditar.Text = "Ver / Editar";
        btnEditar.AutoSize = true;
        btnEditar.Click += btnEditar_Click;
        //
        // btnEliminar
        //
        btnEliminar.Text = "Eliminar";
        btnEliminar.AutoSize = true;
        btnEliminar.Click += btnEliminar_Click;
        //
        // btnExportar
        //
        btnExportar.Text = "Exportar Excel";
        btnExportar.AutoSize = true;
        btnExportar.Click += btnExportar_Click;
        //
        // statusStrip
        //
        statusStrip.Items.AddRange(new ToolStripItem[] { lblEstadoBarra });
        statusStrip.Dock = DockStyle.Bottom;
        lblEstadoBarra.Spring = true;
        lblEstadoBarra.TextAlign = ContentAlignment.MiddleLeft;
        lblEstadoBarra.Text = "Listo.";
        //
        // tablaCabecera
        //
        tablaCabecera.ColumnCount = 2;
        tablaCabecera.RowCount = 2;
        tablaCabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 124F));
        tablaCabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tablaCabecera.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tablaCabecera.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tablaCabecera.Dock = DockStyle.Top;
        tablaCabecera.AutoSize = true;
        tablaCabecera.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tablaCabecera.Padding = new Padding(0, 0, 0, 4);
        tablaCabecera.Controls.Add(picLogo, 0, 0);
        tablaCabecera.SetRowSpan(picLogo, 2);
        tablaCabecera.Controls.Add(flowFiltros, 1, 0);
        tablaCabecera.Controls.Add(panelBotones, 1, 1);
        //
        // frmMain
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1040, 520);
        // Z-order: primero el grid (fondo), luego cabecera y barra encima para que no queden tapados.
        // ToolStrip superior: se agrega después de la cabecera para quedar en la franja superior (Dock Top).
        Controls.Add(dgvPacientes);
        Controls.Add(tablaCabecera);
        Controls.Add(toolStripPrincipal);
        Controls.Add(statusStrip);
        MinimumSize = new Size(520, 360);
        Text = "Seguimiento de turnos médicos prequirúrgicos";
        ((System.ComponentModel.ISupportInitialize)dgvPacientes).EndInit();
        ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
        panelBotones.ResumeLayout(false);
        panelBotones.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        tablaCabecera.ResumeLayout(false);
        tablaCabecera.PerformLayout();
        flowFiltros.ResumeLayout(false);
        flowFiltros.PerformLayout();
        toolStripPrincipal.ResumeLayout(false);
        toolStripPrincipal.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
