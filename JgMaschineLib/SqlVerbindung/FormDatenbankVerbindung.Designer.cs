namespace JgMaschineLib
{
  partial class FormDatenbankVerbindung
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDatenbankVerbindung));
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.pgConnectionString = new System.Windows.Forms.PropertyGrid();
      this.btnTesten = new System.Windows.Forms.Button();
      this.btnEintragen = new System.Windows.Forms.Button();
      this.btnAbbrechen = new System.Windows.Forms.Button();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
      this.tableLayoutPanel1.Controls.Add(this.pgConnectionString, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnTesten, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.btnEintragen, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.btnAbbrechen, 1, 4);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(7);
      this.tableLayoutPanel1.RowCount = 5;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(547, 540);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // pgConnectionString
      // 
      this.pgConnectionString.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.pgConnectionString.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pgConnectionString.Location = new System.Drawing.Point(7, 7);
      this.pgConnectionString.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
      this.pgConnectionString.Name = "pgConnectionString";
      this.tableLayoutPanel1.SetRowSpan(this.pgConnectionString, 5);
      this.pgConnectionString.Size = new System.Drawing.Size(365, 526);
      this.pgConnectionString.TabIndex = 0;
      // 
      // btnTesten
      // 
      this.btnTesten.Dock = System.Windows.Forms.DockStyle.Fill;
      this.btnTesten.Image = ((System.Drawing.Image)(resources.GetObject("btnTesten.Image")));
      this.btnTesten.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnTesten.Location = new System.Drawing.Point(383, 393);
      this.btnTesten.Name = "btnTesten";
      this.btnTesten.Padding = new System.Windows.Forms.Padding(5);
      this.btnTesten.Size = new System.Drawing.Size(154, 35);
      this.btnTesten.TabIndex = 1;
      this.btnTesten.Text = "  &Verbindung testen";
      this.btnTesten.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnTesten.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
      this.btnTesten.UseVisualStyleBackColor = true;
      this.btnTesten.Click += new System.EventHandler(this.BtnTesten_Click);
      // 
      // btnEintragen
      // 
      this.btnEintragen.Dock = System.Windows.Forms.DockStyle.Fill;
      this.btnEintragen.Image = ((System.Drawing.Image)(resources.GetObject("btnEintragen.Image")));
      this.btnEintragen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnEintragen.Location = new System.Drawing.Point(383, 454);
      this.btnEintragen.Name = "btnEintragen";
      this.btnEintragen.Padding = new System.Windows.Forms.Padding(5);
      this.btnEintragen.Size = new System.Drawing.Size(154, 35);
      this.btnEintragen.TabIndex = 2;
      this.btnEintragen.Text = "  &Eintragen";
      this.btnEintragen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnEintragen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
      this.btnEintragen.UseVisualStyleBackColor = true;
      this.btnEintragen.Click += new System.EventHandler(this.BtnEintragen_Click);
      // 
      // btnAbbrechen
      // 
      this.btnAbbrechen.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnAbbrechen.Dock = System.Windows.Forms.DockStyle.Fill;
      this.btnAbbrechen.Image = ((System.Drawing.Image)(resources.GetObject("btnAbbrechen.Image")));
      this.btnAbbrechen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnAbbrechen.Location = new System.Drawing.Point(383, 495);
      this.btnAbbrechen.Name = "btnAbbrechen";
      this.btnAbbrechen.Padding = new System.Windows.Forms.Padding(5);
      this.btnAbbrechen.Size = new System.Drawing.Size(154, 35);
      this.btnAbbrechen.TabIndex = 3;
      this.btnAbbrechen.Text = "  &Abbrechen";
      this.btnAbbrechen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnAbbrechen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
      this.btnAbbrechen.UseVisualStyleBackColor = true;
      // 
      // FormDatenbankVerbindung
      // 
      this.AcceptButton = this.btnEintragen;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnAbbrechen;
      this.ClientSize = new System.Drawing.Size(547, 540);
      this.Controls.Add(this.tableLayoutPanel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormDatenbankVerbindung";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Datenbankverbindungseinstellungen";
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.PropertyGrid pgConnectionString;
    private System.Windows.Forms.Button btnTesten;
    private System.Windows.Forms.Button btnEintragen;
    private System.Windows.Forms.Button btnAbbrechen;
  }
}