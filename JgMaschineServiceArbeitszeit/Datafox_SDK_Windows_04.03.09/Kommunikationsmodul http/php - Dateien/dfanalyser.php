<?php
include 'df4.php';

// Verarbeitung der eingehenden und ausgehenden Daten.
class CDFDataAnalyser
{
	var $m_crc;                      // Erhaelt den berechneten CRC
	var $m_settimeEnable;            // Aktivierung des DatumUhrzeit stellen per Antwort
	var $m_settimeTemplate;          // Enthaelt das Template von DatumUhrzeitfeldern	
	var $m_emailEnable;              // Aktivierung des Email-Versants
	var $m_emailTemplate;            // Enthaelt das Template fuer Email-Adressen
	var $m_emailFrom;                // Email-Adresse des Absenders
	var $m_email;                    // Erhaelt die Email-Addresse des Empfaengers
	var $m_timestamp;                // Erhaelt das DatumUhrzeit Feld
	var $m_timediff;                 // Erhaelt die Zeitdifferenz von DU-Feld zu aktueller Serverzeit

	var $m_sendmessageEnable;        // Aktivierung des Nachrichtenversants
	var $m_sendmessageText;          // Enthaelt der zu sendende Nachrichtentext
	var $m_sendmessagePeepcode;      // Enthaelt den anzuwendenden Peep
	var $m_sendmessageShowTime;      // Enthaelt die Anzeigedauer der Message
	
	var $m_serviceEnable;            // Wird eine Wartungsverbindung gefordert.
	
	var $m_rawRequest;               // Erhaelt die entschluesselte Anfrage. (Fuer Log/Debug)
	var $m_rawAnswer;                // Erhaelt die unverschluesselte Antwort. (Fuer Log/Debug)
	
	var $m_crypt;                    // Feldinhalte werden Verschluesselt kommuniziert.
	var $m_key;                      // Enthaelt das anzuwendende Passwort.
	var $m_df4;                      // Klasse fuer Verschluesselung.
	var $m_cryptStream;              // Enthaelt die gesamte verschluesselte Zeichenfolge, bestehend aus den Feldwerten. (Fuer Log/Debug)
	var $m_dfc;                      // Enthaelt die Information ob die evnt. Entschluesselung erfolgreich war.
	var $m_dfcb;                     // Enthaelt die Information des Crypt-Beginn
	var $m_dfce;                     // Enthaelt die Information des Crypt-End
	
	// Konstruktor
	function CDFDataAnalyser()
	{
		// ** Aktivierung der Zusatzoptionen          **
		//###############################################
		$this->m_settimeEnable = true;
		$this->m_emailEnable = true;
		$this->m_emailFrom = "gprs@datafox.de";
		$this->m_sendmessageEnable = false;
		//###############################################
		
		// Template zum ermitteln von DatumUhrzeit-Felder
		$this->m_settimeTemplate = "^20[0-9]{2}-[0-9]{2}-[0-9]{2}_[0-9]{2}:[0-9]{2}:[0-9]{2}";
		// Template fuer die Ermittlung der Email-Adresse
		$this->m_emailTemplate = "^.+@.+\..+";
		
		// Parameter fuer Messageversant
		$this->m_sendmessageText = "Antwort vom Datafox Server";
		$this->m_sendmessagePeepcode = 5;
		$this->m_sendmessageShowTime = 5;

		// Wartungsverbindung gefordert.
		$this->m_serviceEnable = true;
		
		// Zu verarbeitenden Anfrage-Antwort Zeichenfolgen halten.
		$this->m_rawRequest = "";
		$this->m_rawAnswer = "";
		
		// ** Verschluesselung der Daten **
		//###############################################
		$this->m_key = "Datafox"; // Hier bitte das zu verwendene Passwort zuweisen.
		$this->m_df4 = new CDF4();
	}
	
	// Die mit GET oder POST gesendeten Parameternamen und die zugehörigen Parameterwerte in das
	// Array "$name_value" kopieren. Dabei ggf. entschluesseln.
	//
	function ParseData(&$name_value)
	{
		// Von unverschluesselten Feldinhalten ausgehen.
		$this->m_crypt = false;
		// Initialisierung der zusaetzlich verwendeten variablen fuer Verschluesselung.
		$this->m_dfc = "";
		$this->m_dfcb = "";
		$this->m_dfce = "";
		$dfceDetect = false;

		
		// Get oder Post Anfrage?
		$request = $_GET;
		if(sizeof($_POST) != 0)
		{
			$request = $_POST;
		}
		
		// Anfrage-Antwort Zeichenfolge loeschen
		$search = "";
		$this->m_rawRequest = $_SERVER["REQUEST_URI"];
		$this->m_rawAnswer = "";

		// Die einzelnen Key=Value Paare abarbeiten.
		foreach($request as $getKey=>$getVal)
		{
			// ggf. zu ersetzende Zeichenfolge
			$search = "";
			
			// Beginnkennung fuer Verschluesselte Daten (Datafox crypt begin)
			if ($getKey == "dfcb")
			{
				$this->m_crypt = true; // Ab jetzt alle Feldinhalte Entschluesseln
				
				// 1. Der Wert von Crypt-Beginn muss zwischen 1000 und 9999 liegen, nur Ziffern sind erlaubt.
				if (strlen($getVal) != 4 || intval($getVal) < 1000)
				{
					// Bereichsfehler fuer dfcb als Rueckantwort senden
					$this->m_dfc = "error";
					$this->m_dfcb = "range";
					if (strlen($request["dfce"])) // Fehlt dfce oder wird nicht ausgewertet?
					{
						$this->m_dfce = "unknown";
					}
					else
					{
						$this->m_dfce = "missing";						
					}
					
					// Anfragestring $this->m_rawRequest bleibt wie er ist.
					
					return false;
				}
				
				// Wert fuer weitere Auswertung halten
				$this->m_dfcb = $getVal;
				
				// Der Schluessel setzt sich zusammen aus dem Key + Zufallswert
				// Verschluesselung initialisieren
				$this->m_df4->DF4Init($this->m_key . $getVal);
				
				// Entschluesselung aktiveren, es werden alle Felder bis einschliesslich dfce entschluesselt.
				$dfceDetect = true;
			}
			else 
			{
				// Feldinhalte entschluesseln?
				if ($this->m_crypt && $dfceDetect)
				{
					// zu ersetzende Zeichenfolge
					$search = $getKey . "=" . $getVal;
					
					$this->m_cryptStream .= $getVal;
					$getVal = $this->m_df4->Decrypt($getVal);					
				}
				
				if ($getKey == "dfce") // Endekennung fuer Verschluesselte Daten (Datafox crypt end)
				{
					// 3. Der Wert von Crypt-Ende muss zwischen 1000 und 9999 liegen, nur Ziffern sind erlaubt.

					if (strlen($getVal) != 4 || intval($getVal) < 1000)
					{
						$this->m_dfc = "error";
						$this->m_dfcb = $this->m_dfcb;
						$this->m_dfce = "range";
						
						// Anfragestring $this->m_rawRequest muss zurueckgesetzt werden, da die Entschluesselung nicht erfolgreich war.
						$this->m_rawRequest = $_SERVER["REQUEST_URI"];
						
						return false;
					}
					else if ($this->m_dfcb != $getVal) // Der Wert von Crypt-Ende muss gleich Crypt-Beginn sein.
					{
						$this->m_dfc = "error";
						$this->m_dfcb = $this->m_dfcb;
						$this->m_dfce = "different";
						
						// Anfragestring $this->m_rawRequest muss zurueckgesetzt werden, da die Entschluesselung nicht erfolgreich war.
						$this->m_rawRequest = $_SERVER["REQUEST_URI"];
						
						return false;
					}
					
					// Wert fuer weitere Auswertung halten
					$this->m_dfce = $getVal;
					
					// Entschluesselung deaktivieren
					$dfceDetect = false;
				}
			}
			
			// Zu ersetzende Zeichenfolge?
			if (strlen($search))
			{
				$this->m_rawRequest = str_replace($search, $getKey . "=" . $getVal, $this->m_rawRequest);
			}
			
			// Key-Value Paar zur weiteren Verarbeitung uebernehmen
			$name_value["$getKey"] = $getVal;			
		}
		
		// Crypt-Beginn und Crypt-Ende muessen immer zusammen vorkommen.
		if (strlen($this->m_dfcb) && !strlen($this->m_dfce))
		{
			$this->m_dfc = "error";
			$this->m_dfcb = $this->m_dfcb;
			$this->m_dfce = "missing"; // Kein Crypt-Ende gefunden,
			
			// Anfragestring $this->m_rawRequest muss zurueckgesetzt werden, da die Entschluesselung nicht erfolgreich war.
			$this->m_rawRequest = $_SERVER["REQUEST_URI"];
			
			return false;
		}
		if (!strlen($this->m_dfcb) && strlen($this->m_dfce))
		{
			$this->m_dfc = "error";
			$this->m_dfcb = "missing"; // Kein Crypt-Beginn gefunden.
			$this->m_dfce = "unknown";
			
			// Anfragestring $this->m_rawRequest muss zurueckgesetzt werden, da die Entschluesselung nicht erfolgreich war.
			$this->m_rawRequest = $_SERVER["REQUEST_URI"];
			
			return false;	
		}
		
		// Zusaetzlich in den Daten nach Feldern vom Type Datum-Uhrzeit suchen, auch eine eMail-Angabe wird gehalten um ggf. eine
		// Mail mit dem Empfangenen Dateninhalt an diese Addresse zu senden.
		foreach($name_value as $name=>$value)
		{
			// Ist noch kein DatumUhrzeitfeld ermittelt und ist DatumUhrzeit-Feld?
			if (!strlen($this->m_timestamp) && eregi($this->m_settimeTemplate, $value))
			{
				$this->m_timestamp = $value;
         		$this->m_timediff = strtotime(strftime("%Y-%m-%d %H:%M:%S", time())) - strtotime(str_replace("_", " ", $value));    		
 			}
 			else
 			{
				// Ist noch kein Emailfeld ermittelt und ist Email-Feld?
				if (!strlen($this->m_email) && eregi($this->m_emailTemplate, $value))
				{		
					$this->m_email = $value;
				}
 			}
		}
		
		return true;
	}
	
	// Pruefsumme ermitteln
	function GetCRC($name_value)
	{
		$tableFound = false; // CRC-Berechnung von Feld table bis checksum durchfuehren.
		$crc = 0;
		
		foreach($name_value as $name=>$value)
		{
			// Nur Felder ab 'table' werden in die Pruefsumme aufgenommen.
			if ($name == "table")
			{
				$tableFound = true;
			}
			// Felder ab checksum werden aus der Pruefsumme ausgeschlossen!
			if($name == "checksum")
			{
				break;
			}
		
			// Addition jedes einzelnen ASCII-Werts zur Pruefsumme
			for($i=0; $tableFound && $i < strlen($value); $i++)
			{
				// neuen Wert der Pruefsumme durch Addition des
				// aktuellen Zeichenwerts berechnen
				$crc += ord($value[$i]);
			}			
		}
				
		return $crc;	
	}
	
	// CRC berechnen und mit dem aus den Daten vergleichen.
	function CheckCRC($name_value)
	{
		if (!strlen($name_value["checksum"]))
		{
			// Es liegt fuer den Vergleich keine Pruefsumme vor
			return false;
		}
					
		return ($this->GetCRC($name_value) == $name_value["checksum"]);	
	}
	
	// Die Rueckantwort aufbauen. Die Felder ggf. Verschluesseln.
	// $closure gibt an ob das Feld dfce bei Verschluesselung durch die Routine angefuegt wird oder vom Aufrufer erst spaeter
	// angefuegt wird, da noch daten wie gv selbst angefuegt werden.
	function GetAnswer($name_value, $closure)
	{
		// Zu sendende Antwort aufbereiten.
		$answer = "";
		// Rohe unverschluesselte Antwort halten.
		$this->m_rawAnswer = "";
		
		// Antwort verschluesseln?
		if ($this->m_crypt)
		{
			// Die Verschluesselung der Antwort wird dort fortgesetzt wo der letzte
			// Wert der Anfrage entschluesselt wurde.
			
			
			// Konnte keine Erfolgreiche Entschluesselung der Daten vorgenommen werden,
			// wird eine entsprechende Fehlerantwort gesendet.
			if ($this->m_dfc == "error") // Liegt Fehler vor?
			{
				$this->m_rawAnswer = "dfc=$this->m_dfc&dfcb=$this->m_dfcb&dfce=$this->m_dfce";
				return $this->m_rawAnswer;
			}
			
			// Erstes Feld bei verwendeter Verschluesselung ist dfcb mit dem erhaltenen dfcb Wert der Anfrage.
			$answer = "dfcb=" . $this->m_dfcb . "&";	
			
			$this->m_rawAnswer = $answer;	
		}
		
		// Pruefsumme ueber die Anfrage-Daten erstellen.
		$checksum = strval($this->GetCRC($name_value));
		
		// Gueltigkeit der Pruefsumme pruefen
		if($this->CheckCRC($name_value))
		{
			$status = "ok";
			
			// Antwort verschluesseln?
			if ($this->m_crypt)
			{
				$this->m_rawAnswer .= "status=$status&checksum=$checksum";
				
				$status = $this->m_df4->Crypt($status);
				$checksum = $this->m_df4->Crypt($checksum);
				$this->m_cryptStream .= $status . $checksum;
			}		
		}
		else // Pruefsumme falsch
		{
			$status = "error";
			
			if ($this->m_crypt)
			{
				$this->m_rawAnswer .= "status=$status&checksum=$checksum";
				
				$status = $this->m_df4->Crypt($status);
				$checksum = $this->m_df4->Crypt($checksum);
				$this->m_cryptStream .= $status . $checksum;
			}	
		}
		
		// Minimale Antwort
		$answer .= "status=$status&checksum=$checksum";
		
		// Soll die Aktuelle Systemzeit mitgesendet werden?
		if ($this->m_settimeEnable)
		{
			$time = strftime("%Y-%m-%d_%H:%M:%S", time ());
			if ($this->m_crypt)
			{
				$this->m_rawAnswer .= "&time=$time"; 
				
				$time = $this->m_df4->Crypt($time);
				$this->m_cryptStream .= $time;				
			}
			
			$answer .= "&time=$time";
		}
		// Soll eine Nachricht mitgesendet werden?
		if ($this->m_sendmessageEnable)
		{
			$message = $this->m_sendmessageText;
			$delay = $this->m_sendmessageShowTime;
			$beep = $this->m_sendmessagePeepcode;
			
			if ($this->m_crypt)
			{
				$this->m_rawAnswer .= "&message=$message&delay=$delay&beep=$beep";
				
				$message = $this->m_df4->Crypt($message);
				$delay = $this->m_df4->Crypt(strval($delay));
				$beep = $this->m_df4->Crypt(strval($beep));
				
				$this->m_cryptStream .= $message . $delay . $beep;
			}
			
			$answer .= "&message=$message&delay=$delay&beep=$beep";
		} 
		// Wartungsverbindung gefordert?
		if ($this->m_serviceEnable)
		{
			$service = "1";
			if ($this->m_crypt)
			{
				$this->m_rawAnswer .= "&service=$service";
				
				$service = $this->m_df4->Crypt($service);
				
				$this->m_cryptStream .= $service;
			}
			
			$answer .= "&service=$service";
		}
	
		// Bei Aktiver Verschluesselung wird als letztes Feld dfce angefuegt.
		// Wenn $closure nicht erwuenscht ist, muss das der Aufrufer selbst erledigen.
		if ($this->m_crypt && $closure)
		{
			$dfce = $this->m_dfce;
			$this->m_rawAnswer .= "&dfce=$dfce";
			
			$dfce = $this->m_df4->Crypt($dfce);
			$this->m_cryptStream .= $dfce;				
	
			$answer .= "&dfce=$dfce";
		}
		
		// Wenn nicht verschluesselt, dann ist der pure Antwortstring gleich dem eigentlichen.
		if (!$this->m_crypt)
		{
			$this->m_rawAnswer = $answer;
		}
				
		return $answer;
	}
	
	// Empfangene Daten als eMail aufbereiten und versenden.
	function SendMail($name_value)
	{
		// Email bei Bedarf aufbereiten und senden
		if($this->m_emailEnable && strlen($this->m_email))
		{
			// Betreff
			$subject = "GPRS-DATEN-TimeDiff: ". $this->m_timediff . " Sek. vom ". strftime("%Y-%m-%d_%H:%M:%S", time());
			// Sendestring aufbereiten
			foreach($name_value as $name=>$value)
			{
				$sendtext .= "$name =\t";
				if(strlen($name) < 8) $sendtext .= "\t";
				if(strlen($name) < 16) $sendtext .= "\t";
				$sendtext .= "$value\n";
			}
			// Antwortstring aufbereiten
			$answers = split("&", $answer);
			foreach($answers as $name=>$value)
			{
				$nv = split("=", $value);
				$answertext .= "$nv[0] =\t";
				if(strlen($nv[0]) < 8) $answertext .= "\t";
				if(strlen($nv[0]) < 16) $answertext .= "\t";
				$answertext .= "$nv[1]\n";
			}
			
			// Nachricht der Email erzeugen
			$message = '
				<html>
					<head>
						<title>GPRS-Daten vom ' . strftime("%Y-%m-%d_%H:%M:%S", time()) . ' </title>
					</head>
					<body>
						<pre>
							<font size="2" face="Verdana" color="#FF2B2B"><p><b>Folgende Daten wurden an den Server gesendet!</b></p></font>
							<font size="2" face="Verdana" color="#000000"><p>' . $sendtext . '</p></font>
							<font size="2" face="Verdana" color="#FF2B2B"><p><b>Folgende Daten wurden vom Server zurückgesendet!</b></p></font>
							<font size="2" face="Verdana" color="#000000"><p>' . $answertext . '</p></font>
						</pre>
					</body>
				</html>
			';
			
			// Um eine HTML-Mail zu senden, den den "Content-type"-Header. setzen
			$headers  = "MIME-Version: 1.0\r\n";
			$headers .= "Content-type: text/html; charset=iso-8859-1\r\n";
			
			// Zusaetzliche Headerangaben
			$headers .= "From: " . $this->m_emailFrom;
			
			// Versenden der Mail
			if(!mail($this->m_email, $subject, $message, $headers))
			{
				//echo "Es konnte keine eMail versendet werden!";
			}
		}
	}
}
?>
