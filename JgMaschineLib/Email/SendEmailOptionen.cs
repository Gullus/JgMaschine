namespace JgMaschineLib.Email
{
  public class SendEmailOptionen
  {
    public string AdresseAbsender { get; set; }
    public string AdressenEmpfaenger { get; set; }
    public string Betreff { get; set; }
    public string Koerper { get; set; } // "<html><body>UtilMailMessage001 - success</body></html>";
    public string ServerAdresse { get; set; }
    public int ServerPort { get; set; } = 25;
    public string ServerBenutzername { get; set; }
    public string ServerPasswort { get; set; }
  }
}
