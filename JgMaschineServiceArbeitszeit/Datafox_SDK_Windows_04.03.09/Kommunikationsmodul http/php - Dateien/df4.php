<?php
// RC4 Verschluesselungsklasse
 /* Die Verschluesselung der Daten basiert auf dem RC4 Algorithmus.
  * 
  * Entschluesselungsablauf der Anfrage:
  * In der Anfrag der Gegenstelle traegt das einleitende verschluesselte Feld den Namen 'crypt'. 
  * Der Inhalt des Feldes (crypt) ist ein erzeugter Zufallswert, der von der Gegenstelle berechnet wurde.
  * Das vorhandensein des Feldes 'crypt' leitet die Entschluesselung dieses und aller folgenden Felder ein,
  * hierzu wird die Verschluesselungsklasse mit der Routein DF4Init initialisiert und jedes Feld
  * mit der Routine Decrypt entschluesselt.
  * Hinweis: Um das Datenvolumen der Felddaten zu reduzieren werden die Feldinhalte nach Verschluesselung in ihre 
  * Hexdarstellung konvertiert uerbertragen.
  * 
  * Verschluesselungsablauf der Antwort:
  * Lagen die Anfragefelder verschluesselt vor, werden auch die Antwortfelder verschluesselt uebertragen.
  * Bitte beachten: Hierzu wird keine Neuinitalisierung mittels DF4Init vorgenommen.
  * Bitte beachten: Das erste zu verschluesselte Feld traegt den namen 'crypt' der Wert ist der selbe wie in der Anfrag.
  * Die Verschluesselung der einzelnen Feldinhalte wird mit der Routine Crypt durchgeführt.
  * Hinweis: Um das Datenvolumen der Felddaten zu reduzieren werden die Feldinhalte nach Verschluesselung in ihre 
  * Hexdarstellung konvertiert uerbertragen.
  * 
  * */

if(!function_exists('str_split'))
{
	function str_split($string, $split_length = 1) 
	{
		$array = explode("\r\n", chunk_split($string, $split_length));
		array_pop($array);
		return $array;
	}
}

class CDF4
{
	var $m_key;              // Passwort fuer die Verschluesselung.
	var $m_sbox;             // Interne Verwendung zwischen den Routinen.
	var $m_sboxI;            // Interne Verwendung zwischen den Routinen.
	var $m_sboxJ;            // Interne Verwendung zwischen den Routinen.
	
	// Konstruktor
	function CDF4()
	{
		$this->m_key = ""; // Die Zuweisung des Passwortes erfolgt durch Aufruf der Funktion DF4Init
		$this->m_sbox = array();
		$this->m_sboxI = 0;
		$this->m_sboxJ = 0;
	}
	
	// Ver- oder Entschluesseln der Uebergebenen Daten 
	function DF4(&$data, $size)
	{
		// Wenn die Angabe einer Laenge fehlt, wird davon ausgegangen,
		// dass mit einer Zeichenkette gearbeitet wird.
		if ($size == 0)
		{
			if (is_array($data))
			{
				$size = sizeof($data);
			}
			else 
			{
				$size = strlen($data);	
			}
		}
	
		$pos = 0;
		for(; ($size - $pos) > 0; $pos++)
		{
			$this->m_sboxI = ($this->m_sboxI + 1) % 256;
			$this->m_sboxJ = ($this->m_sboxJ + $this->m_sbox[$this->m_sboxI]) % 256;
			$trans = $this->m_sbox[$this->m_sboxI];
			$this->m_sbox[$this->m_sboxI] = $this->m_sbox[$this->m_sboxJ];
			$this->m_sbox[$this->m_sboxJ] = $trans;
			$t = ($this->m_sbox[$this->m_sboxI] + $this->m_sbox[$this->m_sboxJ]) % 256;
			$data[$pos] ^= $this->m_sbox[$t];
		}
	}

	// Hexadezimal-Zeichenfolge in Array mit Dezimalwerten wandeln.
	function HexStringToDecArray($val)
	{
		$res = array();
		if (strlen($val) > 0)
		{
			$i = 0;
			$ss = str_split($val, 2);
			foreach ($ss as $hexVal)
			{
				$res[$i] = hexdec($hexVal);
				$i++;
			}
		}
		
		return $res;
	}
	
	// Array mit Dezimalwerten in Hexadezimal-Zeichenfolge wandeln.
	function DecArrayToHexString($val)
	{
		$res = "";
		if (is_array($val) && sizeof($val) > 0)
		{
			$tmp = "";
			$i = 0;
			for ($i=0; $i < count($val); $i++)
			{
				$tmp = dechex($val[$i]);
				if (strlen($tmp) == 1)
				{
					$tmp = "0" . $tmp;
				}
				$res .= $tmp;
			}
		}
		
		return $res;
	}	
	
	// Zeichenfolge in Array mit Dezimalwerten wandeln.
	function StringToDecArray($val)
	{
		$res = array();
		if (strlen($val) > 0)
		{
			$i = 0;
			for ($i=0; $i < strlen($val); $i++)
			{
				$res[$i] = ord($val[$i]);
			}
		}
		
		return $res;
	}
	
	// Array mit Dezimalwerten in Zeichenfolge wandeln.
	function DecArrayToString($val)
	{
		$res = "";
		if (is_array($val) && sizeof($val) > 0)
		{
			$i = 0;
			for ($i=0; $i < count($val); $i++)
			{
				$res .= chr($val[$i]);
			}			
		}
		
		return $res;
	}	

	// Array mit Dezimalwerten in Hexadezimal-Zeichenfolge wandeln.
	function StringToHexString($val)
	{
		$res = "";
		if (strlen($val) > 0)
		{
			$tmp = "";
			$i = 0;
			for ($i=0; $i < strlen($val); $i++)
			{
				$tmp = dechex(ord($val[$i]));
				if (strlen($tmp) == 1)
				{
					$tmp = "0" . $tmp;
				}
				$res .= $tmp;
			}	
		}
		
		return $res;
	}	
		
	// Hexadezimal-Zeichenfolge in Zeichenfolge wandeln.
	function HexStringToString($val)
	{
		$res = "";
		if (strlen($val) > 0)
		{
			$i = 0;
			$ss = str_split($val, 2);
			foreach ($ss as $hexVal)
			{
				$res .= chr(hexdec($hexVal));
				$i++;
			}
		}
		
		return $res;
	}
	
	// Initialisierungsroutine
	//#####################################################
	// Diese Routine ist aufzurufen, bevor eine fortlaufende Ver- oder Entschluesselung
	// Durchgefuehrt werden soll.
	// Hiernach kann dann mit jeweils Crypt oder Decrypt eine Verschluesselung (auch von Teilen) durchgefuehrt werden.
	function DF4Init($key)
	{
		$kptr = 0;
		$ksize = 0;
	
		// Wird mit einem Binaerschluessel gearbeitet?
		$binaryKey = is_array($key);
		if ($binaryKey)
		{
			$ksize = sizeof($key);
		}
		else 
		{
			$ksize = strlen($key);
		}
		$this->m_key = $key;
		$k = array();
	
		$this->m_sboxI = $this->m_sboxJ = 0;
		for($i=0; $i < 256; $i++)
		{
			$this->m_sbox[$i] = $i;
		}
	
		for($i=0; $i < 256; $i++)
		{
			$k[$i] = ($binaryKey == true) ? $this->m_key[$kptr] : ord($this->m_key[$kptr]);
			$kptr++;
	
			if ($kptr >= $ksize)
			{
				$kptr = 0;
			}
		}
	
		for($i=0, $j=0; $i < 256; $i++)
		{
			$j = ($j + $this->m_sbox[$i] + $k[$i]) % 256;
			$trans = $this->m_sbox[$i];
			$this->m_sbox[$i] = $this->m_sbox[$j];
			$this->m_sbox[$j] = $trans;
		}
	}
	
	// Hexadezimal-Zeichenfolge in DatenArray wandeln, entschluesseln und als Zeichenfolge zurueckliefern.
	function Decrypt($hexString)
	{
		$data = $this->HexStringToDecArray($hexString);
		
		$this->DF4($data, count($data));
		
		return $this->DecArrayToString($data);
	}
	
	// Zeichenfolge in DatenArray wandeln, verschluesseln und als Hexadezimal-Zeichenfolge zurueckliefern.
	function Crypt($str)
	{
		$data = $this->StringToDecArray($str);
		
		$this->DF4($data, count($data));
		
		return $this->DecArrayToHexString($data);
	}	
}
?>