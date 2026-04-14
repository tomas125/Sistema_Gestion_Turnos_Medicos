namespace MedicalTracker.Forms;

partial class frmExport
{
    private System.ComponentModel.IContainer components = null;
    private RadioButton rbTodoEnUnaHoja = null!;
    private RadioButton rbUnaPorPaciente = null!;
    private Button btnAceptar = null!;
    private Button btnCancelar = null!;
    private Label lblTitulo = null!;

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
        lblTitulo = new Label();
        rbTodoEnUnaHoja = new RadioButton();
        rbUnaPorPaciente = new RadioButton();
        btnAceptar = new Button();
        btnCancelar = new Button();
        SuspendLayout();
        //
        // lblTitulo
        //
        lblTitulo.AutoSize = true;
        lblTitulo.Location = new Point(16, 16);
        lblTitulo.MaximumSize = new Size(360, 0);
        lblTitulo.Text = "Seleccione el formato de exportación a Excel:";
        //
        // rbTodoEnUnaHoja
        //
        rbTodoEnUnaHoja.AutoSize = true;
        rbTodoEnUnaHoja.Checked = true;
        rbTodoEnUnaHoja.Location = new Point(20, 48);
        rbTodoEnUnaHoja.Text = "Una sola hoja con todos los pacientes y estudios";
        rbTodoEnUnaHoja.TabStop = true;
        //
        // rbUnaPorPaciente
        //
        rbUnaPorPaciente.AutoSize = true;
        rbUnaPorPaciente.Location = new Point(20, 76);
        rbUnaPorPaciente.Text = "Una hoja por cada paciente";
        //
        // btnAceptar
        //
        btnAceptar.Location = new Point(120, 120);
        btnAceptar.Text = "Aceptar";
        btnAceptar.DialogResult = DialogResult.None;
        btnAceptar.Click += btnAceptar_Click;
        //
        // btnCancelar
        //
        btnCancelar.Location = new Point(220, 120);
        btnCancelar.Text = "Cancelar";
        btnCancelar.DialogResult = DialogResult.None;
        btnCancelar.Click += btnCancelar_Click;
        //
        // frmExport
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(384, 161);
        Controls.Add(btnCancelar);
        Controls.Add(btnAceptar);
        Controls.Add(rbUnaPorPaciente);
        Controls.Add(rbTodoEnUnaHoja);
        Controls.Add(lblTitulo);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Exportar a Excel";
        ResumeLayout(false);
        PerformLayout();
    }
}
