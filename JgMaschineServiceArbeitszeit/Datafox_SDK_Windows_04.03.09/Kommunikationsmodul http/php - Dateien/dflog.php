<?php
// Dient zur Unterstuetzung von Logfunktionalitaet.
class CDFLog
{
	var $m_enable;                // Log aktiv?
	var $m_fileName;              // Dateiname der Logdatei
	var $m_eraseFilesEnable;      // Alte Dateien loeschen
	var $m_eraseFilesCount;       // Anzahl zu haltender Logdateien
	var $m_fileNameTemplate;      // YYYY_MM_DD_PHP.log
	var $m_base;                  // Basispfad
	var $m_sub;                   // Unterverzeichnis
	var $m_crLf;                  // Zeichenfolge für Zeilenumbruch
	var $m_withTimestamp;         // Logeintrag mit fuehrender Uhrzeit
	var $m_row;                   // Haelt die Daten einer Logzeile
	
	function CDFLog()
	{
		// ** Aktivierung der Logfunktionalitaet **
		// ########################################
		$this->m_enable = true;
		// ########################################
		if ($this->m_enable)
		{			
			// Basispfad der Logdateien
			$this->m_base = "./";
			// Unterordner der Logdateien
			$this->m_sub = "Logfiles/";
			// Dateiname der Logdatei: YYYY_MM_DD_PHP.log
			$this->m_fileNameTemplate = "20%02u_%02u_%02u_PHP.log";
			// Die Verwaltungsroutine fuer die automatische Loeschung
			// Aelterer Dateien geht davon aus, dass der Dateiname
			// mit dem Datumsstempel beginnt.
			$this->m_fileNameTemplateMatch = "^20[0-9]{2}_[0-9]{2}_[0-9]{2}_PHP.log";
			// Aktuellen Dateiname der Logdatei erstellen
			$t = time();
			$ltm = localtime($t, 1);
			$this->m_fileName = sprintf($this->m_fileNameTemplate, 
				$ltm["tm_year"] % 100, $ltm["tm_mon"] + 1, $ltm["tm_mday"]);
			// Loeschen von Dateien die aelter sind als m_eraseFilesCount Logtage
			$this->m_eraseFilesEnable = true;
			// Anzahl zu haltender Logdateien (in Tagen)
			$this->m_eraseFilesCount = 5;
			// Zeichenfolge fuer einen Zeilenumbruch
			$this->m_crLf = "\r\n";
			// Automatische Einfuegung eines Zeitstempels vor jedem Logeintrag
			$this->m_withTimestamp = true;
			// Pruefen ob der Logpfad existiert.
			if (!is_dir($this->m_base . $this->m_sub))
			{	
				if (!mkdir($this->m_base . $this->m_sub))
				{
					// Logsystem wegen Fehlschlag abstellen
					$this->m_enable = false;
				}
			}
			
			// Pruefung fuer eraseFile durchfuehren
			$this->logCheckOnWorkDate();
		}
	}

	// Behalten der Verzeichnisdateien die den letzten
	// m_eraseFilesCount Tagen entprechen. 
	// VORSICHT: Nur mit seperatem Unterverzeichnis fuer Log verwenden.
	function logCheckOnFixDate()
	{
		// Soll durchgefuehrt werden?
		if (!$this->m_enable || !$this->m_eraseFilesEnable)
		{	
			return 0;
		}
	
		// Namen der Logdateien fuer die letzten n Tage erstellen.
		for ($i=0; $i < $this->m_eraseFilesCount; $i++)
		{
			// Dateiname der Logdatei
			$t = time();
			$t -= 3600 * 24 * $i;
			$ltm = localtime($t, 1);
			$fileName[$i] = sprintf($this->m_fileNameTemplate, 
				$ltm["tm_year"] % 100, $ltm["tm_mon"] + 1, $ltm["tm_mday"]);
		}
		// Alle anderen Logdateien loeschen 
	
		$j = 0;
	
		// Suche nach Dateien
		$file = sprintf("%s%s", $this->m_base, $this->m_sub);
	
		if (($dir = opendir($file)) != false)
		{	
			while(($j < $this->m_eraseFilesCount) && (($ent = readdir($dir)) != false))
			{
				if ($ent == "." || $ent == "..") continue;
				
				for ($i=0; $i < $this->m_eraseFilesCount; $i++)
				{
					// Stimmt mit einem der zu erhaltenden Dateien ueberein?
					if (strstr($fileName[$i], $ent) != 0)
					{
						break;
					}
				}
				if ($i == $this->m_eraseFilesCount)
				{
					// Sammeln der zu loeschenden Dateien,
					// da erst nach closedir geloescht werden kann!
					$deleteFileName[$j++] = $ent;
				} 
			}
			closedir($dir);
	
			if ($j)
			{
				// Dateien loeschen
				for ($i=0; $i < $j; $i++)
				{
					$file = $this->m_base . $this->m_sub . $deleteFileName[$i];
					
					if (unlink($file) == false)
					{
						// Konnte nicht geloescht werden!
					}
				}
			}
		}
		
		return 1;
	}
	
	// Sammeln aller Logdateien und erhalten der ersten m_eraseFilesCount.
	function logCheckOnWorkDate()
	{
		// Soll durchgefuehrt werden?
		if (!$this->m_enable || !$this->m_eraseFilesEnable)
		{	
			return 0;
		}
		
		// Suche nach Dateien in Verzeichnis
		$dir = sprintf("%s%s", $this->m_base, $this->m_sub);
		if (($resource = opendir($dir)) != false)
		{	
			while(($file = readdir($resource)) != false)
			{
				if ($file == "." || $file == "..") continue;
				
				// Passt Dateiname zu Format der Logdateinamen
				if (eregi($this->m_fileNameTemplateMatch, $file))
				{
					// In Liste aufnehmen
					$fileList[$fileCount++] = $file;
				}
			}
			closedir($resource);
	
			// Wurden Dateien gefunden.
			if ($fileCount)
			{	
				$fileCount = 0;
				// Array der gefundenen Dateien sortieren.
				rsort($fileList);
				
				for (; count($fileList);)
				{	
					// Ist Datei von selbigem Datum?
					if (strncmp($tmp, $fileList[0], 10) == false)
					{
						// Datei mit gleichem Datums-Zeichenfolgenbeginn
						// Kann sein, da fuer verschiedene Kanaele gelogt werden kann.
					}
					else
					{
						// Datums-Zeichenfolge des Dateinamen aufnehmen.
						$tmp = $fileList[0];
						$fileCount++; // Zu erhaltende Datei
					}
					
					// Datumsbereich der zu erhaltenden Dateien ueberschritten?
					if ($fileCount > $this->m_eraseFilesCount)
					{
						$file = $this->m_base . $this->m_sub . $fileList[0];
						if (unlink($file) == false)
						{
							// Konnte nicht geloescht werden!
						}
						
						//echo "Erase file: " . $fileList[0] . "\n";
					}
					else
					{	
						//echo "Hold file: " . $fileList[0] . "\n";					
					}
					
					// Datei aus Array entfernen
					array_shift($fileList);
				}
			}
		}
		
		return 1;
	}
	
	// Anfang der Logzeile schreiben.
	function logBegin($format)
	{
		// Log aktiv?
		if (!$this->m_enable)
		{
			return 0;
		}
		// Uhrzeit des Logeintrages
		$t = time();
		//m_clockBegin = clock();
		$ltm = localtime($t, 1);
		
		// Mit fuehrendem Zeitstring?
		if ($this->m_withTimestamp)
		{	
			$timeString = sprintf("%02u:%02u:%02u\t", $ltm["tm_hour"], $ltm["tm_min"], $ltm["tm_sec"]);
		}
		/*
		// Dateiname der Logdatei
		$fileName = sprintf($this->m_fileNameTemplate, 
			$ltm["tm_year"] % 100, $ltm["tm_mon"] + 1, $ltm["tm_mday"]);
		// Pruefen ob sich der Name geandert hat.
		if (strcmp($fileName, $this->m_fileName) != 0)
		{
			// Dateiname hat sich geaendert.
			$this->m_fileName = $fileName;
			$this->logCheck(); // Eventuell die aelteste Logdatei loeschen.
		}
		*/
		// Alle Argumente als Parameter aufbereiten
		$args = func_get_args();
		// Erstes Element ist Formatstring, diesen entfernen
		array_shift($args);
		// Logzeile aufbauen und halten
		$this->m_row = vsprintf($timeString . $format, $args);

		return 1;
	}
	
	// Teilstueck in Logzeile einfuegen
	function logAppend($format)
	{
		// Log aktiv?
		if (!$this->m_enable)
		{	
			return 0;
		}
		// Alle Argumente als Parameter aufbereiten
		$args = func_get_args();
		// Erstes Element ist Formatstring, diesen entfernen
		array_shift($args);
		// Logzeile aufbauen und halten
		$this->m_row .= vsprintf($format, $args);

		return 1;
	}
	
	// Ende der Logzeile mit Zeilenumbruch schreiben
	function logEnd($format) 
	{
		// Log aktiv?
		if (!$this->m_enable)
		{	
			return 0;
		}
		// Alle Argumente als Parameter aufbereiten
		$args = func_get_args();
		// Erstes Element ist Formatstring, diesen entfernen
		array_shift($args);
		// Datei oeffnen
		if (($logfp = fopen($this->m_base . $this->m_sub . $this->m_fileName, "a")) == false)
		{
			return 0;
		}
		// Logzeile aufbauen
		$pr_row=vsprintf($this->m_row . $format . $this->m_crLf, $args);
		fwrite($logfp, $pr_row);
		// Datei schliessen
		fclose($logfp);
		return 1;
	}
}
?>
