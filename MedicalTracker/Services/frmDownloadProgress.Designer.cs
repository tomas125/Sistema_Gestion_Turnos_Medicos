namespace MedicalTracker.Services;

partial class frmDownloadProgress
{
    private System.ComponentModel.IContainer components = null;
    private ProgressBar progressBar = null!;
    private Label lblEstado = null!;
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
        progressBar = new ProgressBar();
        lblEstado = new Label();
        btnCancelar = new Button();
        SuspendLayout();
        //
        // progressBar
        //
        progressBar.Location = new Point(16, 44);
        progressBar.Size = new Size(400, 23);
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.MarqueeAnimationSpeed = 30;
        //
        // lblEstado
        //
        lblEstado.AutoSize = false;
        lblEstado.Location = new Point(16, 16);
        lblEstado.Size = new Size(400, 40);
        lblEstado.Text = "Descargando…";
        //
        // btnCancelar
        //
        btnCancelar.Location = new Point(341, 80);
        btnCancelar.Text = "Cancelar";
        btnCancelar.Click += btnCancelar_Click;
        //
        // frmDownloadProgress
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(432, 118);
        Controls.Add(btnCancelar);
        Controls.Add(progressBar);
        Controls.Add(lblEstado);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Descargando actualización";
        FormClosing += frmDownloadProgress_FormClosing;
        Shown += frmDownloadProgress_Shown;
        ResumeLayout(false);
        PerformLayout();
    }
}
