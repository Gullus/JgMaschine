<?php

/* Bitte Beachten:
 * Das in dieser Datei enthaltene Script dient lediglich als Beispielimplementierung und Orientierungshilfe.
 * Das Script berücksichtigt nicht alle eintretbaren Sonderfälle und darf nur zur Implementierungsunterstüzung
 * der Anwendungsentwicklung eingesetzt werden.
 * 
 * Firma: Datafox GmbH
 * Autor: B. Ottmann
 * Stand: 12.05.2011
 * */

require 'dflog.php';
require 'dfanalyser.php';

//------------------------------------------------------------------------------
$log = new CDFLog();
$analyser = new CDFDataAnalyser();

// Wenn verfuegbar, Hostname loggen!
//$log->logBegin("%s\t", gethostbyaddr(getenv("REMOTE_ADDR")));
$log->logBegin("%s\t", $_SERVER["REMOTE_ADDR"]);

// Das Passwort fuer die Ver- und Entschluesselung bitte im Konstruktor von CDFDataAnalyser eintragen.
// Daten einlesen
$analyser->ParseData($name_value);

/* Bitte Beachten:
 * Ein Datensatz des Gerätes, wird so lange gesendet, bis eine 'status=ok' Meldung als Antwort an das Gerät zurückgesendet wird.
 * Bitte stellen Sie sicher, dass ein mehrfach gesendeter Datensatz erkannt und irgedwann verworfen wird.
 * */

// Antwort aufbereiten
$answer = $analyser->GetAnswer($name_value, true);

// Anfrage und Antwortstring loggen.
// Verschluesselt loggen
//$log->logAppend("%s\t%s", urldecode($_SERVER["REQUEST_URI"]), $answer);
// Unverschluesselt loggen.
$log->logAppend("%s\t%s", urldecode($analyser->m_rawRequest), $analyser->m_rawAnswer);

// Ggf. eine email versenden.
if ($analyser->m_emailEnable)
{
	$analyser->SendMail($name_value);
}

// Logeintrag der Ausfuehrung beenden
$log->logEnd("");

// Antwort an Geraet senden
print $answer . "\r\n";

// Lediglich fuer Debugzwecke
//print "\r\n" . $analyser->m_cryptStream . "\r\n";

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
?>
