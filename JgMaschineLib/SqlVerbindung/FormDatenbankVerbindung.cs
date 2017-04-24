using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace JgMaschineLib
{
  public partial class FormDatenbankVerbindung : Form
  {
    public string ConnectionString
    {
      get { return (pgConnectionString.SelectedObject as SqlConnectionStringBuilder).ConnectionString; }
    } 

    public FormDatenbankVerbindung(string MyConnectionString)
    {
      InitializeComponent();
      
      SqlConnectionStringBuilder csb = null;

      try
      {
        csb = new SqlConnectionStringBuilder(MyConnectionString);
      }
      catch
      {
        csb = new SqlConnectionStringBuilder();
      }

      pgConnectionString.SelectedObject = csb;
    }

    private void BtnTesten_Click(object sender, EventArgs e)
    {
      SqlConnection cl = new SqlConnection(ConnectionString);
      try
      {
        cl.Open();
      }
      catch (Exception f)
      {
        MessageBox.Show(string.Format("Fehler der Herstellung einer Verbindung !\nGrund: {0}", f.Message), "Fehler !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      cl.Close();
      MessageBox.Show("Verbindungsaufbau erfolgreich !", "Erfolg !", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnEintragen_Click(object sender, EventArgs e)
    {
      DialogResult = System.Windows.Forms.DialogResult.OK;
    }
  }
}
