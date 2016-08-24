/**
 * @file   DFComSLink.h
 * @author Bernd Ottmann
 * @date   28.04.2005
 * @brief  Deklarationen der Exportroutinen zur DFComDLL.dll
 * @note   Die Auflistung der Routinen ist wie in der zugehoerigen Dokumentation.
 */
//////////////////////////////////////////////////////////////////////
#ifndef _DFCOMSLINK_H_
#define _DFCOMSLINK_H_

/** 
 @mainpage Dokumentation zur DFComDLL.
 <center> @code 

   ########################################################
   ##       |                   |                  |     ##
   ##                          \_/                       ##
   ##       |                 -(_)-                |  \  ##
   ##                          / \                    |\\##
   ##  /|   |                                      |  || ##
   ##/ ||                  _\|||/_                    || ##
   ##  ||   |               /_=_\                  |  || ##
   ##  ||                  ({}-{})                    || ##
   ##  ||   |+-----oOo-------(_)---------------+   |  || ##
   ## //     |                                 |      || ##
   ##/      ||          Datafox GmbH           |   |  || ##
   ##        |                                 |      || ##
   ##       |+-------------------------oOo-----+   |  || ##
   ##                     |  |~|  |                   || ##
   ##       |_ __ _ __ _ _|__| |__|_ __ _ __ _ __ _|  || ##
   ##      /               ||   ||                  \ || ##
   ##    /               .ooO   Ooo.                  \ \##
   ##  /                                                \##
   ########################################################

 @endcode </center>
 @author Bernd Ottmann
 @date   21.02.2003
*/

// ########################################################
// ###################### DEFINES #########################
// ########################################################

/**
 * Version der verwendeten DLL.
 * 
 * @todo Version der verwendeten DLL pruefen, um nur die enthaltenen Routinen zu 
 * importieren oder auch zu exportieren.
 */
// Benutzung ab Version 1.1.i
//#define COM_VERSION           1
//#define COM_VERSION           0x02000000
//#define COM_VERSION           0x02000100
//#define COM_VERSION           0x02000200
//#define COM_VERSION           0x02000300
//#define COM_VERSION           0x02000400
//#define COM_VERSION           0x02000600
//#define COM_VERSION           0x02000900
//#define COM_VERSION           0x02001000
//#define COM_VERSION           0x02002100
//#define COM_VERSION           0x02002200
//#define COM_VERSION           0x04010000
//#define COM_VERSION           0x04010400
//#define COM_VERSION           0x04010500
//#define COM_VERSION           0x04010600
//#define COM_VERSION           0x04010700
//#define COM_VERSION           0x04020000
//#define COM_VERSION           0x040203000
#define COM_VERSION           0x04030400

/**
 * Gibt export oder import der Routinen an. Siehe auch EXTERN_DECL.
 *
 * @todo Mit 1 im Quelltext vor #include zu definieren um die Routinen als Import zu erhalten.
 *
 * @code #define AE3COM_EXPORT   1 @endcode
 */
#ifndef AE3COM_EXPORT
	#define AE3COM_EXPORT 0
#endif

#ifndef USE_PRIVATE
	#define USE_PRIVATE   0
#endif

// Eventuell fehlende typedefs ersetzen
#ifdef _WIN32

#ifndef HANDLE
	#define HANDLE void*
#endif

#endif
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// +------------+ EXPORTIERTE FUNKTIONEN +----------------+
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++

/**
 @def EXTERN_DECL Gibt den Praefix fuer Funktionen an. (Dekorierung und Export-, Importangabe)
*/

#ifdef _WIN32

#if (AE3COM_EXPORT)
      // Einzubindende Bibliotheken: Version.lib, WSock32.lib
      #define EXTERN_DECL         extern "C" __declspec(dllexport)
      // DllGetVersion
      #include "shlwapi.h"
#else
      #ifdef __cplusplus
         #define EXTERN_DECL     extern "C" __declspec(dllimport)
      #else
         #define EXTERN_DECL     extern  __declspec(dllimport)
      #endif
 #endif

#else

#ifdef __cplusplus
   #define EXTERN_DECL     extern "C"
#else
   #define EXTERN_DECL     extern
#endif

#define __declspec(dllexport)
#define __stdcall

#endif

// ########################################################
// ############## ALLGEMEINE FUNKTIONEN ###################
// ########################################################

// ########################################################
/** 
 * @brief Oeffnet die serielle Schnittstelle fuer RS232 / RS485 Direktkommunikation. 
 *
 * Die Funktion ruft intern die Funktion DFCComOpenSerial() mit den Standardparametern
 * iBaudRate = 38400 und iTimeOut = 150 auf.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 *
 * @return Kommunikationskanal oeffnen.
 * @retval 1 Schnittstelle erfolgreich geoeffnet.
 * @retval 0 Fehler bei Initialisierung.
 */
EXTERN_DECL int __stdcall DFCComInit(int iChannelNum);

#if (COM_VERSION >= 0x02000300)

// ########################################################
/** 
 * @brief Oeffnet die serielle Schnittstelle fuer RS232 / RS485 Direktkommunikation. 
 *
 * Es koennen zusaetzliche Parameter verwendet werden. Die Baudrate ist standardmaessig
 * auf 38400bd einzustellen und der Timeout liegt bei 150ms pro Lesevorgang.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param szComPort Zeiger auf String mit Namen der Schnittstelle "COM1", "COM2" usw..
 * @param iBaudRate Baudrate mit der die Schnittstelle verwendet wird, 38400bd ist Standard.
 * @param iTimeOut Zu verwendender Timeout fuer Read - Befehl. (ReadFile)
 *
 * @return Kommunikationskanal oeffnen.
 * @retval 1 Schnittstelle erfolgreich geoeffnet.
 * @retval 0 Fehler bei Initialisierung.
 */
EXTERN_DECL int __stdcall DFCComOpenSerial(int iChannelNum, char *szComPort, int iBaudRate, int iTimeOut);

#endif

// ########################################################
/** 
 * @brief Oeffnet die TCP/IP Schnittstelle. 
 *
 * Die Funktion ruft intern die Funktion DFCComOpenSocket() mit den Standardparametern
 * iPortNum = 8000 und iTimeOut = 3000 auf.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param *szHost Zeiger auf String mit Hostnamen oder IP - Adresse in Punktnotation.
 *
 * @return Kommunikationskanal oeffnen.
 * @retval 1 Schnittstelle erfolgreich geoeffnet.
 * @retval 0 Fehler bei Initialisierung.
 */
EXTERN_DECL int __stdcall DFCComOpen(int iChannelNum, char *szHost);

#if (COM_VERSION >= 0x02000300)

// ########################################################
/** 
 * @brief Oeffnet die TCP/IP Schnittstelle. 
 *
 * Es koennen zusaetzliche Parameter verwendet werden. Die Portnummer ist standardmaessig
 * auf 8000 einzustellen und der Timeout (ein intern verwalteter) 
 * liegt bei 3000 pro Lesevorgang.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param *szRemoteIP Zeiger auf String mit Hostnamen oder IP - Adresse in Punktnotation.
 * @param iPortNum Portnummer 
 * @param iTimeOut Zu verwendender Timeout bei Lesevorgang. (keine ms - Einheit)
 *
 * @return Kommunikationskanal oeffnen.
 * @retval 1 Schnittstelle erfolgreich geoeffnet.
 * @retval 0 Fehler bei Initialisierung.
 */
EXTERN_DECL int __stdcall DFCComOpenSocket(int iChannelNum, char *szRemoteIP, int iPortNum, int iTimeOut);

#endif

#if (COM_VERSION >= 0x02001000)

// ########################################################
/** 
 * @brief Oeffnet die Schnittstelle fuer die Kommunikation mit MasterIV.
 *
 * Initialisiert den Kanal. 
 * Je nach angegebenem Typ wird ein anderes Initialisierungsverfahren verwendet 
 * um die entsprechend nachfolgenden Funktionsaufrufe der exportierten Funktionen 
 * richtig umzusetzen.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Die von der DLL zu verwendende Busnummer. (Multimasteransatz)
 * @param iType Der zu verwendende Schnittstellentyp.
 *              1 -> RS232 direkt oder über Umsetzter (RS232 <-> RS485)
 *              2 -> Funk über den Umsetzer (RS232 <-> BIM2) ohne eigenen Kontroller.
 *              3 -> TCP / IP - Verbindung.
 * @param szComString Je nach Schnittstellentyp:
 *                    1,2 -> Zeiger auf Zeichenkette für die Schnittstelle. "COM1" .. "COM4".
 *                    3 -> Zeiger auf Zeichenkette mit IP - Adresse oder Hostname.
 * @param iComValue Je nach Schnittstellentyp:
 *                  1,2 -> Übertragungsrate der Schnittstelle. (Standard ist: 38400)
 *                  3 -> Portnummer des TCP / IP Anschlusses.
 * @param iComTimeOut Je nach Schnittstellentyp:
 *                    1,2 ->Timeout für Lesen von Schnittstelle (Standard ist: 150)
 *                    3 -> Timeout für Lesen von Socket (Standard ist: 3000)
 *
 * @return Kommunikationskanal oeffnen.
 * @retval 1 Schnittstelle erfolgreich geoeffnet.
 * @retval 0 Fehler bei Initialisierung.
 */
EXTERN_DECL int __stdcall DFCComOpenIV(int iChannelNum, int iBusNum, int iType, char *szComString, int iComValue, int iComTimeOut);

#endif

// ########################################################
/** 
 * @brief Schliessen des geoeffneten Kanals.
 *
 * Je nach dem Typ des Kanals wird entweder die serielle Schnittstelle oder
 * der Socket geschlossen.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 *
 * @return Keine.
 */
EXTERN_DECL void __stdcall DFCComClose(int iChannelNum);

// ########################################################
/** 
 * @brief Pruefen ob das Geraet erreichbar ist.
 *
 * Ruft intern die Funktion DFCCheckDevice() mit standard - Parametern auf.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 *
 * @return Ergebnis der Erreichbarkeit.
 * @retval 0 Geraet ist nicht erreichbar.
 * @retval 1 Es wurde erfolgreich auf das Geraet zugegriffen.
 */
EXTERN_DECL int __stdcall DFCCheckAE(int iChannelNum, int iBusNum);

#if (COM_VERSION >= 0x02000300)

// ########################################################
/** 
 * @brief Pruefen ob das Geraet erreichbar ist.
 *
 * Prueft aufgrund einer kleinen Datenanfrage die Erreichbarkeit des Geraetes.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 * @param iDevicePollRetry Anzahl der Wiederholversuche.
 *
 * @return Ergebnis der Erreichbarkeit.
 * @retval 0 Geraet ist nicht erreichbar.
 * @retval 1 Es wurde erfolgreich auf das Geraet zugegriffen.
 */
EXTERN_DECL int __stdcall DFCCheckDevice(int iChannelNum, int iBusNum, int *piError, int iDevicePollRetry);

#endif

// ########################################################
/** 
 * @brief Setzt das uebergebene Datum und die Uhrzeit in dem Geraet.
 *
 * Das zuvor aufbereitete Datum mit Uhrzeit muss in einem speziellen Format
 * vorliegen, welches in der Dokumentation zur DLL beschrieben ist.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param pucDateTime Zeiger auf Bytearray mit Werten fuer Datum und Uhrzeit.
 *
 * @return Ergebnis der Wertsetzung im Geraet.
 * @retval 0 Wert konnte nicht gesetz werden.
 * @retval 1 Wert wurde erfolgreich gesetzt.
 */
EXTERN_DECL int __stdcall DFCComSetTime(int iChannelNum, int iBusNum, unsigned char *pucDateTime);

// ########################################################
/**
 * @brief Holt das Datum und die Uhrzeit aus dem Geraet.
 *
 * Das Datum mit Uhrzeit wird in einem speziellen Format
 * abgelegt, welches in der Dokumentation zur DLL beschrieben ist.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param pucDateTime Zeiger auf Bytearray mit Werten fuer Datum und Uhrzeit.
 *
 * @return Ergebnis der Wertermittlung aus dem Geraet.
 * @retval 0 Wert konnte nicht ermittelt werden.
 * @retval 1 Wert wurde erfolgreich ermittelt.
 */
EXTERN_DECL int __stdcall DFCComGetTime(int iChannelNum, int iBusNum, unsigned char *pucDateTime);

// ########################################################
/** 
 * @brief Direktes schreiben einer Textnachricht.
 *
 * Der gesendete Text wird fuer eine bestimmbare Zeit und mit einer
 * eventuell akustischen Signalisierung auf der Anzeige ausgegeben. Die
 * Ausgabe ist vom Geraetezustand abhaengig.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param ucDelayTime Anzeigedauer. (ucDelayTime * 250ms). 255->nicht automatisch wegblenden.
 * @param ucAction Ist fuer eingriffe in die Bedienung vorgesehen.
 * @param ucAudio Meldung aktustisch untermalen.
 * @param szMessage Zeiger auf den anzuzeigenden Text.
 * @param iLength Laenge des anzuzeigenden Textes.
 *
 * @return Ergebnis der Textsendung an Geraet.
 * @retval 0 Text konnte nicht geschrieben werden.
 * @retval 1 Text wurde erfolgreich geschrieben.
 */
EXTERN_DECL int __stdcall DFCComSendMessage(int iChannelNum, int iBusNum, unsigned char ucDelayTime, unsigned char ucAction, unsigned char ucAudio, char *szMessage, int iLength);

// ########################################################
/** 
 * @brief Indirektes schreiben einer Textnachricht.
 *
 * Der gesendete Text wird in einem eigens dafuer vorgesehenen Speicherbereich 
 * abgelegt und kann ueber die Pfeiltasten in den "nicht PZE-Moden" erreicht werden.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param szMessage Zeiger auf den anzuzeigenden Text.
 * @param iLength Laenge des anzuzeigenden Textes.
 *
 * @return Ergebnis der Textsendung an Geraet.
 * @retval 0 Text konnte nicht geschrieben werden.
 * @retval 1 Text wurde erfolgreich geschrieben.
 */
EXTERN_DECL int __stdcall DFCComSendInfotext(int iChannelNum, int iBusNum, char *szMessage,int iLength);

// ########################################################
/**
 * @brief Die Seriennummer aus dem Geraet lesen.
 *
 * Gibt die gelesene Seriennummer als Ganzzahl ueber Parameter zurueck.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 * @param piSerialNum Zeiger auf Variable welche die Seriennummer erhaelt.
 *
 * @return Ergebnis der Werteermittlung.
 * @retval 0 Wert konnte nicht ermittelt werden und Fehlercode wurde ueber piError abgelegt.
 * @retval 1 Seriennummer wurde ueber piSerialNum in Variable uebergeben.
 */
EXTERN_DECL int __stdcall DFCGetSeriennummer(int iChannelNum, int iBusNum, int *piError, int *piSerialNum);


// ########################################################
/** 
 * @brief Schaltet die Logfunktionalitaet der DLL ein.
 *
 * Loggen von Kommunikationsablaeufen in die angegebene Datei.
 *
 * @param szFileName Zeiger auf die Zeichenfolge, welche den Pfad und/oder Dateinamen enthaelt.
 *
 * @return Ergebnis der Dateioeffnung.
 * @retval 0 Datei konnte nicht erstellt oder geoeffnet werden.
 * @retval 1 Logfunktionalitaet wurde erfolgreich eingeschaltet.
 *
 * @warning Ab der DLL - Version 02.00.03 wird diese Funktion nicht mehr unterstuetzt.
 * @warning Verwenden Sie bitte statt dessen die Funktionen DFCSetLogFileName() / DFCSetLogOn().
 */
EXTERN_DECL int __stdcall DFCLogOn(char *szFileName);

#if (COM_VERSION >= 0x02000300)

// ########################################################
/** 
 * @brief Schaltet die Logfunktionalitaet des Kanals ein.
 *
 * Loggen von Kommunikationsablaeufen in die angegebene Datei.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 *
 * @return Keine.
 *
 * @version Ab DLL - Version 02.00.03 enthalten.
 */
EXTERN_DECL void __stdcall DFCSetLogOn(int iChannelNum);

#endif

// ########################################################
/** 
 * @brief Schaltet die Logfunktionalitaet der DLL aus.
 *
 * Abschalten des Logvorganges.
 *
 * @return Keine.
 *
 * @warning Ab der DLL - Version 02.00.03 wird diese Funktion nicht mehr unterstuetzt.
 * @warning Verwenden Sie bitte statt dessen die Funktion DFCSetLogOff().
 */
EXTERN_DECL void __stdcall DFCLogOff(void);

#if (COM_VERSION >= 0x02000300)

// ########################################################
/** 
 * @brief Schaltet die Logfunktionalitaet des Kanals aus.
 *
 * Abschalten des Logvorganges.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 *
 * @return Keine.
 *
 * @version Ab DLL - Version 02.00.03 enthalten.
 */
EXTERN_DECL void __stdcall DFCSetLogOff(int iChannelNum);

#endif

#if (COM_VERSION >= 0x02000000)

// ########################################################
/** 
 * @brief Setzen einer Rueckruffunktion.
 *
 * Die Rueckruffunktion wird waehrend des Kommunikationsablaufs fortwaehrend 
 * aufgerufen und bietet die Moeglichkeit eines fruehzeitigen abbruches des selbigen.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param CBFunction Zeiger auf die Rueckruffunktion oder 0 fuer abschalten.
 *
 * @return Keine.
 *
 * @version Ab DLL - Version 02.00.00 enthalten.
 */
EXTERN_DECL void __stdcall DFCSetCallBack(int iChannelNum, int (*CBFunction)(void));

#endif

#if (COM_VERSION >= 0x02000300)

// ########################################################
/** 
 * @brief Setzen des Dateinamen fuer die Logfunktionalitaet des Kanals.
 *
 * Bekanntgeben der Datei die fuer das Loggen verwendet wird.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param szLogFileName Zeiger auf die Zeichenfolge, welche den Pfad und/oder Dateinamen enthaelt.
 *
 * @return Keine.
 *
 * @version Ab DLL - Version 02.00.03 enthalten.
 */
EXTERN_DECL void __stdcall DFCSetLogFileName(int iChannelNum, const unsigned char *szLogFileName);

// ########################################################
/** 
 * @brief Aufloesen einer Fehlernummer in seine lesbare Bedeutung.
 *
 * Die von den meisten Funktionen ueber den Parameter piError zurueckgegebene
 * Fehlernummer kann mit dieser Funktion in seine lesbare Bedeutung umgewandelt werden.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iError Fehlernummer.
 * @param iLanguage Angabe der zu verwendenden Sprache.
 * @param szText Zeiger auf die Zeichenfolge, welche den Fehlertext erhaelt.
 * @param iLength Laenge in Anzahl Byte des Speicherbereichs auf den szText verweist.
 *
 * @return Keine.
 *
 * @version Ab DLL - Version 02.00.03 enthalten.
 */
EXTERN_DECL void __stdcall DFCGetErrorText(int iChannelNum, int iError, int iLanguage, char *szText, int iLength);

#endif

#if (COM_VERSION >= 0x02000400)

// ########################################################
/** 
 * @brief Setzten des Wertes einer Globalen Variablen.
 *
 * Der Wert einer Globalen Variablen kann mit Hilfe dieser Funktion vom PC aus
 * abgeaendert werden. Diese muss natuerlich im verwendeten Geraetesetup definiert sein.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param pucVar Zeiger auf Byte welches die logische Variablennummer enthaelt, oder
 *               auf eine Zeichenfolge, welche den Variablennamen darstellt.
 * @param iType Gibt an ob pucVar auf eine Variablennummer oder auf einen Variablennamen verweist.
 * @param pucValue Zeiger auf den Wert der gesetzt werden soll.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Wertuebernahme im Geraet.
 * @retval 0 Wert konnte nicht uebernommen werden.
 * @retval 1 Wert wurde erfolgreich uebernommen.
 *
 * @version Ab DLL - Version 02.00.04 enthalten.
 */
EXTERN_DECL int __stdcall DFCSetGlobVar(int iChannelNum, int iBusNum, unsigned char *pucVar, int iType, unsigned char *pucValue, int *piError);

// ########################################################
/** 
 * @brief Lesen des Wertes einer Globalen Variablen.
 *
 * Der Wert einer Globalen Variablen kann mit Hilfe dieser Funktion vom PC aus
 * gelesen werden. Diese muss natuerlich im verwendeten Geraetesetup definiert sein.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param pucVar Zeiger auf Byte welches die logische Variablennummer enthaelt, oder
 *               auf eine Zeichenfolge, welche den Variablennamen darstellt.
 * @param iType Gibt an ob pucVar auf eine Variablennummer oder auf einen Variablennamen verweist.
 * @param pucValue Zeiger auf ermittelte nullterminierte Zeichenfolge.
 * @param iValueLength Laenge in Anzahl Byte des Speicherbereichs auf den pucValue verweist.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Wertermittlung aus Geraet.
 * @retval 0 Wert konnte nicht ermittelt werden.
 * @retval 1 Wert wurde erfolgreich ermittelt.
 *
 * @version Ab DLL - Version 02.00.04 enthalten.
 */
EXTERN_DECL int __stdcall DFCGetGlobVar(int iChannelNum, int iBusNum, unsigned char *pucVar, int iType, unsigned char *pucValue, int iValueLength, int *piError);

#endif

#if (COM_VERSION >= 0x02000500)

// ########################################################
/** 
 * @brief Schliessen eines angeschlossenen Relais.
 *
 * Internen digitalen Ausgang oder externen digitalen Ausgang (z.B. IOModule)
 * fuer eine bestimmbare Zeit schalten.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param iNum Gibt die Nummer des zu schaltenden Relais an.
 * @param iTime Gibt die Zeit in Sekunden an, in welcher das Relais geschaltet bleibt.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.05 enthalten.
 */
EXTERN_DECL int __stdcall DFCCloseRelay(int iChannelNum, int iBusNum, int iNum, int iTime, int *piError);

// ########################################################
/** 
 * @brief Abfragen von aktuellem Zustand und verbleibender Zeit.
 *
 * Der aktuelle Zustand und die verbleibende Zeit bei geschlossenem Relais abfragen.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param iNum Gibt die Nummer des zu schaltenden Relais an.
 * @param *piState Zeiger auf eine Variable, welche den aktuellen Schaltzustand des Ausgangs erhaelt.
 * @param *piTime Zeiger auf die Variable, welche die verbleibende Zeit in Sekunden erhaelt.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.05 enthalten.
 */
EXTERN_DECL int __stdcall DFCGetRelayState(int iChannelNum, int iBusNum, int iNum, int *piState, int *piTime, int *piError);

// ########################################################
/**
 * @brief Oeffnen eines Relais.
 *
 * Internen digitalen Ausgang oder externen digitalen Ausgang (z.B. IOModule) schalten.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param iNum Gibt die Nummer des zu schaltenden Relais an.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.05 enthalten.
 */
EXTERN_DECL int __stdcall DFCOpenRelay(int iChannelNum, int iBusNum, int iNum, int *piError);

// ########################################################
/** 
 * @brief Abfrage der zuletzt mit DFCCheckDevice() benoetigten Versuche.
 *
 * Die Anzahl der benoetigten Versuche koennen fuer eventuelle Statistikzwecke 
 * verwendet werden.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 *
 * @return Anzahl der benoetigten Versuche bis Geraet als verfuegbar galt.
 *
 * @version Ab DLL - Version 02.00.05 enthalten.
 */
EXTERN_DECL int __stdcall DFCGetDevicePollRetry(int iChannelNum);

#endif

#if (COM_VERSION >= 0x02000300)

// ########################################################
/**
 * @brief Rückgabe des Schnittstellenhandle.
 *
 * Gibt ein Schnittstellen handle zurück.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 *
 * @return Schnittstellenhandle
 *
 * @version Ab DLL - Version 02.00.03 enthalten.
 */
EXTERN_DECL HANDLE __stdcall DFCGetComPort(int iChannelNum);

// ########################################################
/**
 * @brief Setzen des Schnittstellenhandle.
 *
 * 
 *
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param hComPort Schnittstellenhandle
 * @param iType Schnittstellenart
 * @param iComValue Baudrate(seriell) Port(TCP/IP)
 * @param iTimeOut Schnittstellentimeout
 *
 * @return Ergebnis der Ausführung
 * @retval 0 Funktion konnte nicht ausgeführt werden
 * @retval 1 Funktion erfolgreich ausgeführt
 *
 * @version Ab DLL - Version 02.00.03 enthalten.
 */
EXTERN_DECL int __stdcall DFCSetComPort(int iChannelNum, HANDLE hComPort, int bPortIsSocket, int iBaudRate, int iTimeOut);

#endif

#if (COM_VERSION >= 0x02001800)

// ########################################################
/** 
 * @brief Schreibt Zeichen in den Kanal.
 *
 * Schreiben von Daten in den Kanal.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param *pBuf Zeiger auf Daten.
 * @param iLength Laenge der zu schreiben Daten.
 * @param *piWriteLength Anzahl der geschriebenen Daten.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.18 enthalten.
 */
EXTERN_DECL int __stdcall DFCWrite(int iChannelNum, char *pBuf, int iLength, int *piWriteLength, int *piError);

// ########################################################
/** 
 * @brief Liest Zeichen vom Kanal.
 *
 * Lesen von Daten aus dem Kanal.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param *pBuf Zeiger auf Daten.
 * @param iLength Laenge der zu lesenden Daten.
 * @param *piReadLength Anzahl der gelesenen Daten.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.18 enthalten.
 */
EXTERN_DECL int __stdcall DFCRead(int iChannelNum, char *pBuf, int iLength, int *piReadLength, int *piError);

// ########################################################
/** 
 * @brief Uebertraegt Dateidaten zum Geraet.
 *
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer
 * @param *szFileName Name mit eventueller Pfadangabe der Datei.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.18 enthalten.
 */
EXTERN_DECL int __stdcall DFCUpload(int iChannelNum, int iBusNum, char *szFileName, int *piError);
#endif

#if (COM_VERSION >= 0x02001800)
// ########################################################
/** 
 * @brief Lesen der Gerätesoftwareversion
 *
 * Lesen der Gerätesoftwareversion im Textformat. z. B.: 04.00.19
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer
 * @param *szVersion Zeiger auf Zeichenkette für die Versionsnummer (min 32 Zeichen)
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.18 enthalten.
 */
EXTERN_DECL int __stdcall DFCGetVersionFirmware(int iChannelNum, int iBusNum, char *szVersion, int *piError);
#endif


#if (COM_VERSION >= 0x02002100)
// ########################################################
/**
 * @brief Lesen der Gerätesoftwareversion
 *
 * Lesen der Gerätesoftwareversion im Textformat. 
 * z. B.: 04.00.23 direkt aus der Firmwaredatei.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param *szFileName Dateiname der Gerätedatei (wenn nötig mit Pfadangabe)
 * @param *szVersion Zeiger auf Zeichenkette für die Versionsnummer (min 32 Zeichen)
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval >7 Anzahl der kopierten Zeichen in den Puffer "szVersion"
 *
 * @version Ab DLL - Version 02.00.21 enthalten.
 */
EXTERN_DECL int __stdcall DFCGetVersionFirmwareFromFile(int iChannelNum, char *szFileName, char *szVersion, int *piError);
#endif


#if (COM_VERSION >= 0x04010400)
// ########################################################
/**
 * @brief Lesen der Information eines bestimmten Moduls.
 *
 * @param channelID verwendete Kanal
 * @param deviceID verwendete Busnummer
 * @param *name Name der abzurufenden Information
 * @param *params Zeiger auf Parameterliste
 * @param *info Zeiger auf Zeichenkette für Infostring
 * @param *len Länge des Buffers/Zeichenkette
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.04 enthalten.
 */
EXTERN_DECL int __stdcall DFCGetInfo(int channelID, int deviceID, char *name, char *params, char *info, int *len, int *piError);

// ########################################################
/** 
 * @brief Aktiviert Transparentmodus
 *
 * @param channelID verwendete Kanal
 * @param deviceID verwendete Busnummer
 * @param *comm Zeiger für virtuellen COM-Port
 * @param baudRate Baud Rate der Schnittstelle
 * @param byteSize Anzahl "Bits/Byte" der Schnittstelle
 * @param parity Paritätsprüfung
 * @param stopBits Anzal Stoppbits
 * @param flags Zusätzliche Informationen um den Com-Server-Mode zu beeinflussen
 * @param timeoutToClose Timeout zum schließen des COM-Port
 * @param *error Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.04 enthalten.
 */
EXTERN_DECL int __stdcall DFCOpenComServerMode(int channelID, int deviceID, char *comm, int baudRate, int byteSize, int parity, int stopBits, int flags, int timeoutToClose, int *error);

// ########################################################
/** 
 * @brief Deaktiviert Transparentmodus
 *
 *
 * @param channelID verwendete Kanal
 * @param deviceID verwendete Busnummer
 * @param *error Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.04 enthalten.
 */
EXTERN_DECL int __stdcall DFCCloseComServerMode(int channelID, int deviceID, int *error);
#endif

#if (COM_VERSION >= 0x04010500)
EXTERN_DECL int __stdcall DFCIsChannelOpen(int channelID);
#endif

#if (COM_VERSION >= 0x04010600)

EXTERN_DECL int __stdcall DFCSetOptionFirmware(int channelID, int deviceID, int mask, int options, int *piError);

EXTERN_DECL int __stdcall DFCGetOptionFirmware(int channelID, int deviceID, int *mask, int *options, int *piError);

#endif

#if (COM_VERSION >= 0x04010700)

EXTERN_DECL int __stdcall DFCReset(int channelID, int deviceID, int mode, int *error);

EXTERN_DECL int __stdcall DFCSetFontType(int channelID, int deviceID, int iType, int *error);

EXTERN_DECL int __stdcall DFCSetPassword(int channelID, char *password, int *piError);

EXTERN_DECL int __stdcall DFCGetPasswordKey(int channelID, int deviceID, char *key, int *len, int *piError);

EXTERN_DECL int __stdcall DFCPressVirtualKey(int channelID, int deviceID, int virtualKey, int flags, int *piError);

EXTERN_DECL int __stdcall DFCGetFlashStatus(int channelID, int deviceID, int *status, int *piError);

#endif

#if (COM_VERSION >= 0x04020300)
EXTERN_DECL int __stdcall DFCSetCommunicationPassword(int channelID, unsigned char *password, int size, int sessionTimeout, int *piError);

EXTERN_DECL int __stdcall DFCReadHardwareInfo(int channelID, int deviceID, char *infoString, int *infoStringLength, int flags, int *piError);

EXTERN_DECL int __stdcall DFCFileUpload(int channelID, int deviceID, int fileType, char *fileName, int flags, int *piError);
EXTERN_DECL int __stdcall DFCFileDownload(int channelID, int deviceID, int fileType, char *fileName, int flags, int *piError);

#endif

// ########################################################
// ############## FUNKTIONEN FUER SETUP ###################
// ########################################################


// ########################################################
/** 
 * @brief Uebertragen einer Setupdatei.
 *
 * Es wird versucht die angegebene Setupdatei in das Geraet zu uebertragen.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param *szFileName Zeiger auf die Zeichenfolge, welche den Pfad und/oder Dateinamen enthaelt.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 */
EXTERN_DECL int __stdcall DFCSetupLaden(int iChannelNum,int iBusNum, char *szFileName, int *piError);

#if (COM_VERSION >= 0x02000000)

// ########################################################
/** 
 * @brief Setupdaten aus Geraet lesen.
 *
 * Es wird versucht die Setupdaten aus dem Geraet in die angegebene Setupdatei
 * zu uebertragen. 
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param *szFileName Zeiger auf die Zeichenfolge, welche den Pfad und/oder Dateinamen enthaelt.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.00 enthalten.
 */
EXTERN_DECL int __stdcall DFCDownload(int iChannelNum,int iBusNum, char *szFileName, int *piError);

#endif

#if (COM_VERSION >= 0x04010500)
EXTERN_DECL int __stdcall DFCModifyStudioFile(char *sourceFile, char *destFile, char *changeData, int flags, int *piError);
#endif

// ###############################################################
// ######### FUNKTIONEN FÜR LISTEN, SOWIE ZUTRITTSLISTEN #########
// ###############################################################

#if (COM_VERSION >= 0x04010100)
// ###############################################################
/** 
 * @brief Öffnet Liste
 *  
 *  Öffnet eine Liste und gibt einen Handle darauf zurück.
 *  Alle weiteren Listenfunktionen unter diesem Kapitel arbeiten 
 *  mit dem hier zurückgelieferten Handle.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param *name Name der zu öffnenden Tabelle.
 * @param options ! Nicht verwendet 0 angeben !
 * @param *handle eindeutiges Handle.
 * @param *reserved ! Reserviert (Leerstring angeben) !
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.01 enthalten.
 */
 EXTERN_DECL int __stdcall DFCOpenTable(int channelID, int deviceID, char *name, int options, int *handle, char *reserved, int *piError);

// ###############################################################
/** 
 * @brief Schließt Liste
 *
 * Schließen der zuvor mit DFCOpenTable geöffneten Liste.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param handle Listenhandle vom vorherigen Aufruf von DFCOpenTable
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.01 enthalten.
 */
 EXTERN_DECL int __stdcall DFCCloseTable(int channelID, int deviceID, int handle, int *piError);


// ###############################################################
/** 
 * @brief Setzt Filter auf Liste
 *
 * Setzt einen Filter auf die angegebene Liste. Nach Aufruf dieser Funktion
 * muss mit DFCSkip der Datensatzzeiger auf einen gültigen Datensatz verschoben werden.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param handle Listenhandle vom vorherigen Aufruf von DFCOpenTable
 * @param *filter Filterzeichenfolge
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.01 enthalten.
 */
 EXTERN_DECL int __stdcall DFCSetFilter(int channelID, int deviceID, int handle, char *filter, int *piError);

// ###############################################################
/** 
 * @brief Ermittelt Filterkriterium
 *
 * Ermittelt das aktuell gesetzte Filterkriterium auf der Liste.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param handle Listenhandle vom vorherigen Aufruf von DFCOpenTable
 * @param *filter Filterzeichenfolge
 * @param *len Länge des reservierten Buffers/der übergebenen Zeichenkette.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.01 enthalten.
 */
 EXTERN_DECL int __stdcall DFCGetFilter(int channelID, int deviceID, int handle, char *filter, int *len, int *piError);

// ###############################################################
/**
 * @brief Entfernt gesetzten Filter
 *
 * Entfernt einen gesetzten Filter von der Liste. 
 * Nach Aufruf dieser Funktion muss mit DFCSkip der Datensatzzeiger
 * auf einen gültigen Datensatz verschoben werden.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param handle Listenhandle vom vorherigen Aufruf von DFCOpenTable
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.01 enthalten.
 */
 EXTERN_DECL int __stdcall DFCClearFilter(int channelID, int deviceID, int handle, int *piError);
 
 
// ###############################################################
/**
 * @brief Versetzen des Datensatzzeigers einer Liste.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param handle Listenhandle vom vorherigen Aufruf von DFCOpenTable
 * @param offset Anzahl der übergehenden Datensätze
 * @param origin Offset bezug.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.01 enthalten.
 */
 EXTERN_DECL int __stdcall DFCSkip(int channelID, int deviceID, int handle, int offset, int origin, int *piError);


// ###############################################################
/**
 * @brief Setzen eines Feldwerts.
 *
 * Bei Verwendung von sortierten Listen, darf die Sortierspalte (Schlüsselfeld)
 * nicht verändert werden, da keine automatische Umsortierung erfolgt.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param handle Listenhandle vom vorherigen Aufruf von DFCOpenTable.
 * @param *name Feldname.
 * @param *value Wert der gesetzt werden soll.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.01 enthalten.
 */
 EXTERN_DECL int __stdcall DFCSetField(int channelID, int deviceID, int handle, char *name, char *value, int *piError);


// ###############################################################
/**
 * @brief Lesen eines Feldwerts
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param handle Listenhandle vom vorherigen Aufruf von DFCOpenTable.
 * @param *name Feldname.
 * @param *value Wert der gesetzt werden soll.
 * @param *len Länge des reservierten Buffers/der übergebenen Zeichenkette.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.01 enthalten.
 */
 EXTERN_DECL int __stdcall DFCGetField(int channelID, int deviceID, int handle, char *name, char *value, int *len, int *piError);
 #endif


// #############################################################
// ############## FUNKTIONEN FUER DATEN ########################
// #############################################################

// #############################################################
/** 
 * @brief Datenzeiger ruecksetzten.
 *
 * Die Datenzeiger im Geraet werden rueckgesetzt. Die im Geraet eventuell enthaltenen  
 * Datensaetze werden dadurch geloescht. 
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.00 enthalten.
 */
EXTERN_DECL int __stdcall DFCComClearData(int iChannelNum, int iBusNum);

// #############################################################
/** 
 * @brief Daten aus Geraet lesen.
 *
 * Es wird versucht die Daten aus dem Geraet in die DLL zu uebertragen. 
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @note Ab DLL - Version 02.00.00 satzweise lesen.
 */
EXTERN_DECL int __stdcall DFCComCollectData(int iChannelNum, int iBusNum, int *piError);

// ########################################################
/** 
 * @brief Datensatz aus DLL / Geraet lesen.
 *
 * Es wird versucht einen Datensatz aus der DLL / oder Geraet an den Aufrufer zu  
 * uebergeben. Die Busnummer ist durch vorherigen Aufruf der Funktion DFCComCollectData()
 * bekannt.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param *pucBuf Zeiger auf den Puffer, welcher die Daten eines Datensatzes erhaelt.
 * @param *piLength Zeiger auf Variable welche die Datensatzlaenge erhaelt.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @note Ab DLL - Version 02.00.00 satzweise lesen.
 */
EXTERN_DECL int __stdcall DFCComGetDatensatz(int iChannelNum, unsigned char *pucBuf, int *piLength, int *piError);

#if (COM_VERSION >= 1)

// ########################################################
/** 
 * @brief Tabellenbeschreibungen fuer Datensaetze aus Geraet lesen.
 *
 * Es wird versucht die Tabellenbeschreibungen aus dem Geraet zu lesen. Hiernach koennen   
 * mit den Funktionen DFCDatBCnt(), DFCDatBDatensatz() und DFCDatBFeld() auf den 
 * ermittelten Daten gearbeitet werden.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 1.1.i enthalten.
 */
EXTERN_DECL int __stdcall DFCLoadDatensatzbeschreibung(int iChannelNum, int iBusNum, int *piError);

// ########################################################
/** 
 * @brief Anzahl der Tabellenbeschreibungen fuer Datensaetze ermitteln.
 *
 * Auf Grundlage der zuvor mit DFCLoadDatensatzbeschreibung() gelesenen Daten, wird 
 * die Anzahl der definierten Tabellen zurueckgegeben.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 *
 * @return Anzahl der mit DFCLoadDatensatzbeschreibung() gelesenen Tabellendefinitionen.
 *
 * @version Ab DLL - Version 1.1.i enthalten.
 */
EXTERN_DECL int __stdcall DFCDatBCnt(int iChannelNum);

// ########################################################
/** 
 * @brief Grunddaten einer Tabellenbeschreibungen fuer Datensaetze ermitteln.
 *
 * Auf Grundlage der zuvor mit DFCLoadDatensatzbeschreibung() gelesenen Daten, werden 
 * die Grunddaten zu einer Tabellen zurueckgegeben.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iDataNum Tabellennummer im Bereich von (0 - (DFCDatBCnt() - 1)).
 * @param *szName Zeiger auf Speicherbereich fuer Tabellenname. (Mindestens 17 Zeichen).
 * @param *piFieldCount Zeiger auf Variable, welche die Feldanzahl der Tabelle erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden. (Tabelle nicht definiert!)
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 1.1.i enthalten.
 */
EXTERN_DECL int __stdcall DFCDatBDatensatz(int iChannelNum, int iDataNum, unsigned char *szName, int *piFieldCount);

// ########################################################
/** 
 * @brief Grunddaten eines Tabellenfeldes fuer Datensaetze ermitteln.
 *
 * Auf Grundlage der zuvor mit DFCLoadDatensatzbeschreibung() gelesenen Daten, werden 
 * die Grunddaten zu einem Tabellenfeld zurueckgegeben.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iDataNum Tabellennummer im Bereich von (0 - (DFCDatBCnt() - 1)).
 * @param iField Feldnummer.
 * @param *szName Zeiger auf Speicherbereich fuer Feldname. (Mindestens 17 Zeichen).
 * @param *piType Zeiger auf Variable, welche den Feldtyp erhaelt.
 * @param *piLength Zeiger auf Variable, welche die definierte Laenge des Feldes erhaelt,
 *                 in dieser ist auch das Nullterminierungszeichen enthalten.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden. (Tabelle nicht definiert!)
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 1.1.i enthalten.
 */
EXTERN_DECL int __stdcall DFCDatBFeld(int iChannelNum, int iDataNum, int iField, unsigned char *szName, int *piType, int *piLength);

#endif


#if (COM_VERSION >= 0x02002300)
// ########################################################
/**
 * @brief Ließt Daten des nächsten Datensatzes
 *
 * Nachdem der Datensatz gelesen wurde, muss mit DFCQuitRecord 
 * dieser quittiert werden. Erst wenn der Datensatz mit DFCQuitRecord 
 * erfolgreich quittiert wurde, kann mit einem folgenden Aufruf von DFCReadRecord
 * der nächste anstehende Datensatz gelesen werden.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param *buf Speicher, in dem Daten abgelegt werden sollen.
 * @param *length Speicher in dem die Länge der gerade empfangen Daten gespeichert werden soll.
 * @param *err Error erhält eine Fehlernummer
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval -1 Fehler bei Durchführung.
 * @retval 0 Es liegen keine Datensätze vor.
 * @retval 1 Daten eines Online-Datensatzes gelesen.
 * @retval 2 Wiederholt die Daten eines Online-Datensatzes gelesen.
 * @retval 3 Daten eines Offline-Datensatzes gelesen.
 * @retval 4 Wiederholt die Daten eines Offline-Datensatzes gelesen.
 *
 * @version Ab DLL - Version 02.00.23 enthalten.
 */
 EXTERN_DECL int __stdcall DFCReadRecord(int channelID, int deviceID, char *buf, int *length, int *err);

// ########################################################
/**
 * @brief Quittieren der Daten (löschen)
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param *err Error, enthält Fehlernummer.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval -1 Fehler bei Durchführung.
 * @retval 0 Keine Quittung nötig.
 * @retval 1 Daten eines Online-Datensatzes quittiert.
 * @retval 2 Wiederholt die Daten eines Online-Datensatzes quittiert.
 * @retval 3 Daten eines Offline-Datensatzes quittiert.
 * @retval 4 Wiederholt die Daten eines Offline-Datensatzes quittiert.
 *
 * @version Ab DLL - Version 02.00.23 enthalten.
 */
 EXTERN_DECL int __stdcall DFCQuitRecord(int channelID, int deviceID, int *err);
#endif

#if (COM_VERSION >= 0x04010600)
 EXTERN_DECL int __stdcall DFCRestoreRecords(int channelID, int deviceID, int *piError);
#endif


// ########################################################
// ############## FUNKTIONEN FUER LISTEN ##################
// ########################################################

// ########################################################
/**
 * @brief Setzen des Listenbuffers fuer eine Liste.
 *
 * Die Listen muessen ihrer Reichenfolge nach (0 .. n) in den DLL internen 
 * Buffer uebertragen werden.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iListNum Tabellennummer.
 * @param iLineCount Anzahl der Zeilen, Datensaetze im Block.
 * @param iListSize Anzahl Byte im Block. Brechnet sich aus (Anzahl Datensaetzte * Datensatzlaenge).
 * @param *pucBuf Zeiger auf Bytearray, welches die Daten enthaelt.
 * @param iListMemSizeNum Kann immer mit 0 uebergeben werden. (Kompatibel mit Timeboy)
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 */
EXTERN_DECL int __stdcall DFCMakeListe(int iChannelNum, int iListNum, int iLineCount, int iListSize, unsigned char *pucBuf, int iListMemSizeNum);

// ########################################################
/** 
 * @brief Uebertragen der Listendaten zum Geraet.
 *
 * Es wird versucht, die zuvor mit DFCMakeListe() importierten Listendaten in 
 * das Geraet zu uebertragen.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 */
EXTERN_DECL int __stdcall DFCLoadListen(int iChannelNum, int iBusNum, int *piError);

// ########################################################
/** 
 * @brief Buffer fuer Listendaten loeschen.
 *
 * Loescht in der DLL den Buffer fuer Listendaten. Die zuvor schon importierten 
 * Listen waehren nach Ausfuehrung wiederholt zu importieren.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 *
 * @return Keine.
 */
EXTERN_DECL void __stdcall DFCClrListenBuffer(int iChannelNum);

#if (COM_VERSION >= 1)

// ########################################################
/** 
 * @brief Tabellenbeschreibungen fuer Listen aus Geraet lesen.
 *
 * Es wird versucht die Tabellenbeschreibungen aus dem Geraet zu lesen. Hiernach koennen   
 * mit den Funktionen DFCListBCnt(), DFCListBDatensatz() und DFCListBFeld() auf den ermittelten 
 * Daten gearbeitet werden.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 1.1.i enthalten.
 */
EXTERN_DECL int __stdcall DFCLoadListenbeschreibung(int iChannelNum, int iBusNum, int *piError);

// ########################################################
/** 
 * @brief Anzahl der Tabellenbeschreibungen fuer Listen ermitteln.
 *
 * Auf Grundlage der zuvor mit DFCLoadListenbeschreibung() gelesenen Daten, wird 
 * die Anzahl der definierten Tabellen zurueckgegeben.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 *
 * @return Anzahl der mit DFCLoadListenbeschreibung() gelesenen Tabellendefinitionen.
 *
 * @version Ab DLL - Version 1.1.i enthalten.
 */
EXTERN_DECL int __stdcall DFCListBCnt(int iChannelNum);

// ########################################################
/** 
 * @brief Grunddaten einer Tabellenbeschreibungen fuer Listen ermitteln.
 *
 * Auf Grundlage der zuvor mit DFCLoadListenbeschreibung() gelesenen Daten, werden 
 * die Grunddaten zu einer Tabellen zurueckgegeben.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iDataNum Tabellennummer im Bereich von (0 - (DFCListBCnt() - 1)).
 * @param *szName Zeiger auf Speicherbereich fuer Tabellenname. (Mindestens 17 Zeichen).
 * @param *piFieldCount Zeiger auf Variable, welche die Feldanzahl der Tabelle erhaelt.
 * @param *piVar Zeiger auf Variable, welche immer eine 0 erhaelt. (Kompatible zu Timeboy)
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden. (Tabelle nicht definiert!)
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 1.1.i enthalten.
 */
EXTERN_DECL int __stdcall DFCListBDatensatz(int iChannelNum, int iDataNum, unsigned char *szName, int *piFieldCount, int *piVar);

// ########################################################
/** 
 * @brief Grunddaten eines Tabellenfeldes fuer Listen ermitteln.
 *
 * Auf Grundlage der zuvor mit DFCLoadListenbeschreibung() gelesenen Daten, werden 
 * die Grunddaten zu einem Tabellenfeld zurueckgegeben.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iDataNum Tabellennummer im Bereich von (0 - (DFCListBCnt() - 1)).
 * @param iField Feldnummer.
 * @param *szName Zeiger auf Speicherbereich fuer Feldname. (Mindestens 17 Zeichen).
 * @param *piType Zeiger auf Variable, welche den Feldtyp erhaelt.
 * @param *piLength Zeiger auf Variable, welche die definierte Laenge des Feldes erhaelt,
 *                 in dieser ist auch das Nullterminierungszeichen enthalten.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden. (Tabelle nicht definiert!)
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 1.1.i enthalten.
 */
EXTERN_DECL int __stdcall DFCListBFeld(int iChannelNum, int iDataNum, int iField, unsigned char *szName, int *piType, int *piLength);

#endif


// ########################################################
// ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN #########
// ########################################################

#if (COM_VERSION >= 0x02000500)

// ########################################################
/** 
 * @brief Setzen des Listenbuffers fuer eine Liste.
 *
 * Die Listen werden zusaetzlich intern komprimiert.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iListNum Tabellennummer.
 * @param iLineCount Anzahl der Zeilen, Datensaetze im Block.
 * @param iListSize Anzahl Byte im Block. Brechnet sich aus (Anzahl Datensaetzte * Datensatzlaenge).
 * @param *pucBuf Zeiger auf Bytearray, welches die Daten enthaelt.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.05 enthalten.
 */
EXTERN_DECL int __stdcall DFCMakeEntranceList(int iChannelNum, int iListNum, int iLineCount, int iListSize, unsigned char *pucBuf, int *piError);

// ########################################################
/**
 * @brief Uebertragen der Listendaten zum Geraet.
 *
 * Es wird versucht, die zuvor mit DFCMakeEntranceListe() importierten Listendaten in 
 * das Geraet zu uebertragen.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param iListNum Tabellennummer der zu uebertragenden Liste. (-1 uebertraegt alle importierten)
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.05 enthalten.
 */
EXTERN_DECL int __stdcall DFCLoadEntranceList(int iChannelNum, int iBusNum, int iListNum, int *piError);

// ########################################################
/** 
 * @brief Buffer fuer Listendaten loeschen.
 *
 * Loescht in der DLL den Buffer fuer Listendaten.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iListNum Listennummer (0 - 3). Bei Angabe von -1 werden alle geloescht.
 *
 * @return Keine.
 *
 * @version Ab DLL - Version 02.00.05 enthalten.
 */
EXTERN_DECL void __stdcall DFCClearEntranceListBuffer(int iChannelNum, int iListNum);

#endif

// ########################################################
// ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN 2 #######
// ########################################################

#if (COM_VERSION >= 0x04010000)

// ########################################################
/** 
 * @brief Setzen des Listenbuffers fuer eine Liste.
 *
 * Die Listen werden zusaetzlich intern komprimiert.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iListNum Tabellennummer.
 * @param iLineCount Anzahl der Zeilen, Datensaetze im Block.
 * @param iListSize Anzahl Byte im Block. Brechnet sich aus (Anzahl Datensaetzte * Datensatzlaenge).
 * @param *pucBuf Zeiger auf Bytearray, welches die Daten enthaelt.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.05 enthalten.
 */
EXTERN_DECL int __stdcall DFCMakeEntrance2List(int iChannelNum, int iListNum, int iLineCount, int iListSize, unsigned char *pucBuf, int *piError);

// ########################################################
/**
 * @brief Uebertragen der Listendaten zum Geraet.
 *
 * Es wird versucht, die zuvor mit DFCMakeEntranceListe() importierten Listendaten in 
 * das Geraet zu uebertragen.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param iListNum Tabellennummer der zu uebertragenden Liste. (-1 uebertraegt alle importierten)
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 02.00.05 enthalten.
 */
EXTERN_DECL int __stdcall DFCLoadEntrance2List(int iChannelNum, int iBusNum, int iListNum, int *piError);

// ########################################################
/** 
 * @brief Buffer fuer Listendaten loeschen.
 *
 * Loescht in der DLL den Buffer fuer Listendaten.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iListNum Listennummer (0 - 3). Bei Angabe von -1 werden alle geloescht.
 *
 * @return Keine.
 *
 * @version Ab DLL - Version 02.00.05 enthalten.
 */
EXTERN_DECL void __stdcall DFCClearEntrance2ListBuffer(int iChannelNum, int iListNum);

#endif



#if (COM_VERSION >= 0x04010000)
// ########################################################
/** 
 * @brief Führt Zutrittsbuchung via DLL aus.
 *
 * Die Funktion lässt sich aus Sicherheitsgründen nur auf Ausweise 
 * anwenden die im Feld "ActiveGeneral" der "Identification" Liste entweder 
 * den Wert "2" oder "9" eingetragen haben.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param *TM Der Wert der Zeichenfolge auf den dieser Zeiger verweist, 
 *            muss dem Format des Feldes "TM" der "Reader" Liste folgen. 
 *            Er muss demnach immer 3 Ziffern umfassen.
 * @param *ld Der Wert der Zeichenfolge auf den dieser Zeiger verweist, 
 *            muss dem Format des Feldes "Id" der "Identification" Liste folgen.
 *			  Er darf maximal 20 Zeichen lang sein.
 * @param *Pin Der Wert der Zeichenfolge auf den dieser Zeiger verweist, 
 *			   muss dem Format des Feldes "Pin" der "Identification" Liste folgen. 
 *		       Er darf maximal 8 Ziffern beinhalten.
 * @param *status Zeiger auf Zeichenkette die nach erfolgreicher Abarbeitung der Funktion 
 *				  den Status der Zutrittsprüfung erhält. Sie sollten mindestens 5 Zeichen vorsehen.
 * @param *len Länge des reservierten Buffers/der übergebenen Zeichenkette.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.00 enthalten.
 */
EXTERN_DECL int __stdcall DFCEntrance2Identification(int channelID, int deviceID, char *TM, char *Id, char *Pin, char *status, int *len, int *piError);

#endif

#if (COM_VERSION >= 0x04010500)
EXTERN_DECL int __stdcall DFCEntrance2OnlineAction(int channelID, int deviceID, char *TM, int mask, int type, int duration, int *piError);
#endif

#if (COM_VERSION >= 0x04030400)
EXTERN_DECL int __stdcall DFCAccessControlIdentification(int channelID, int deviceID, int masterId, char *TM, char *Id, char *Pin, char *status, int *len, int *piError);
EXTERN_DECL int __stdcall DFCAccessControlOnlineAction(int channelID, int deviceID, int masterId, char *TM, int mask, int type, int duration, int *piError);
#endif

// ########################################################
// ########### FUNKTIONEN FUER FINGERPRINT ################
// ########################################################


#if (COM_VERSION >= 0x04010400)
// ########################################################
/**
 * @brief Anfügen eines Fingertemplates 
 *        an die Datentabelle des Fingerprints.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param type Templatetype der übergeben wird.
 * @param *fingertemplate Zeiger auf die Templatedaten.
 * @param size Die Länge des Buffers auf den der Zeiger *fingertemplate verweist.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.04 enthalten.
 */
EXTERN_DECL int __stdcall DFCFingerprintAppendRecord(int channelID, int deviceID, int type, unsigned char *fingertemplate, int size, int *piError);


// ########################################################
/**
 * @brief Lesen eines Fingertemplates
 *
 * Über die eindeutige Kombination aus PID/FID können mit dieser Funktion die 
 * entsprechenden Templatedaten ermittelt werden.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param type Templatetype der übergeben wird.
 * @param pid Personal ID des Templates.
 * @param fid Finger ID des Templates.
 * @param *fingertemplate Zeiger auf die Templatedaten.
 * @param *size Als Eingabe die Länge des Buffers auf den der 
 *              Zeiger *template verweist, als Ausgabe die übergebene Anzahl Zeichen.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.04 enthalten.
 */
EXTERN_DECL int __stdcall DFCFingerprintGetRecord(int channelID, int deviceID, int type, int pid, int fid, unsigned char *fingertemplate,int *size, int *piError);
 

// ########################################################
/**
 * @brief Löschen von Fingertemplates
 *
 * Sie können einzelne Templates durch Angabe des entsprechenden pid,fid Pärchens löschen. 
 * Es ist auch möglich durch Angabe einer pid und einer fid = 0, alle Fingertemplates zu dieser pid zu löschen. 
 * Wird eine pid = 0 und fid = 0 angegeben, werden alle Fingertemplates gelöscht.
 *
 * @param channelID Der zu verwendende Kanal.
 * @param deviceID Die zu verwendende Busnummer.
 * @param pid Personal ID des Templates.
 * @param fid Finger ID des Templates.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.04 enthalten.
 */
EXTERN_DECL int __stdcall DFCFingerprintDeleteRecord(int channelID, int deviceID, int pid, int fid, int *piError);
#endif

#if (COM_VERSION >= 0x04010700)

EXTERN_DECL int __stdcall DFCFingerprintList(int channelID, int deviceID, int flags, int *piError);

EXTERN_DECL int __stdcall DFCFingerprintBackup(int channelID, int deviceID, char *backupFile, int flags, int *piError);

EXTERN_DECL int __stdcall DFCFingerprintRestore(int channelID, int deviceID, char *backupFile, int flags, int *piError);

#endif

// ########################################################
// ########### FUNKTIONEN FUER TIMEBOYLISTEN  #############
// ########################################################

#if (COM_VERSION >= 0x04010400)

// ########################################################
/** 
 * @brief Setzen des Listenbuffers fuer eine Timeboyliste.
 *
 * Die Listen werden intern kopiert.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iGroupID Gruppennummer der Liste
 * @param iListNum Listennummer.
 * @param iLineCount Anzahl der Zeilen, Datensaetze im Block.
 * @param iListSize Anzahl Byte im Block. Brechnet sich aus (Anzahl Datensaetzte * Datensatzlaenge).
 * @param *pucBuf Zeiger auf Bytearray, welches die Daten enthaelt.
 * @param flags Angebbare Flags zur Liste. Z.B. Bit 1 ob Liste geloescht werden soll.
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.04 enthalten.
 */
EXTERN_DECL int __stdcall DFCMakeTimeboyList(int iChannelNum, int iGroupID, int iListNum, int iLineCount, int iListSize, unsigned char *pucBuf, int flags, int *piError);

// ########################################################
/**
 * @brief Uebertragen der Listendaten zum Geraet.
 *
 * Es wird versucht, die zuvor mit DFCMakeTimeboyList() importierten Listendaten in 
 * das Geraet zu uebertragen.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iBusNum Busnummer des anzusprechenden Geraetes.
 * @param iGroupID Gruppennummer der Liste. (-1 bedeutet alle Gruppen)
 * @param iListNum Listennummer der zu uebertragenden Liste. (-1 uebertraegt alle importierten)
 * @param *piError Zeiger auf Variable die nach Fehler den Fehlercode erhaelt.
 *
 * @return Ergebnis der Ausfuehrung.
 * @retval 0 Funktion konnte nicht ausgefuehrt werden.
 * @retval 1 Funktion wurde erfolgreich ausgefuehrt.
 *
 * @version Ab DLL - Version 04.01.04 enthalten.
 */
EXTERN_DECL int __stdcall DFCLoadTimeboyList(int iChannelNum, int iBusNum, int iGroupID, int iListNum, int reserve, int *piError);

// ########################################################
/** 
 * @brief Buffer fuer Timeboylisten loeschen.
 *
 * Loescht in der DLL den Buffer fuer Timeboylisten.
 *
 * @param iChannelNum Kommunikationskanalnummer.
 * @param iGroupID Gruppennummer der Liste.
 * @param iListNum Listennummer (0 - 19). Bei Angabe von -1 werden alle geloescht.
 *
 * @return Keine.
 *
 * @version Ab DLL - Version 04.01.04 enthalten.
 */
EXTERN_DECL void __stdcall DFCClearTimeboyListBuffer(int iChannelNum, int iGroupID, int iListNum);

#endif

// ########################################################
// ########## FUNKTIONEN FUER AKTIVE VERBINDUNG ###########
// ########################################################

#if (COM_VERSION >= 0x04010500)
EXTERN_DECL int __stdcall DFCStartActiveConnection(char *ip, int port, int deviceID, int timeout, int aliveTO, int maxConnections, int infoFlags, int reserved, int *piError);
EXTERN_DECL int __stdcall DFCStopActiveConnection(int *piError);
EXTERN_DECL int __stdcall DFCGetFirstActiveChannelID();
EXTERN_DECL int __stdcall DFCGetNextActiveChannelID(int prev);
EXTERN_DECL int __stdcall DFCGetInfoActiveChannel(int channelID, char *infoString, int *infoStringLength);
EXTERN_DECL int __stdcall DFCSetRecordAvailable(int enable);
EXTERN_DECL int __stdcall DFCRecordAvailable(int *channelID, int *deviceID, char *infoString, int *infoStringLength, int *error);
EXTERN_DECL int __stdcall DFCBindDeviceToChannel(int channelID, int deviceType, int deviceSerial, char *deviceIp, int *piError);
#endif

// ########################################################
// ########### FUNKTIONEN FUER ERWEITERUNGEN ##############
// ########################################################



// ########################################################
// ############# ** PRIVATE FUNKTIONEN ** #################
// ########################################################

// Funktionen sind nicht dokumentiert.
// Sie werden von AESetup oder Talk verwendet.
// Unterliegen evntl. Aenderungen.

#if (USE_PRIVATE)

// ########################################################
EXTERN_DECL int __stdcall DFCComUseHandle(int iComNum, HANDLE hCom);

// ########################################################
EXTERN_DECL int __stdcall DFCComReleaseHandle(int iComNum);

#if (_WIN32 && AE3COM_EXPORT && (COM_VERSION >= 0x02000000))

// ########################################################
extern "C" HRESULT PASCAL EXPORT CALLBACK DllGetVersion(DLLVERSIONINFO *pdvi);

#endif

#if (COM_VERSION >= 0x02000200)

// ########################################################
EXTERN_DECL int __stdcall DFCComVersion(int iComNum, int iBusNum, int *piError);

#endif



#if (COM_VERSION >= 0x02000400)

// ########################################################
EXTERN_DECL int __stdcall DFCCheckData(int iComNum, int iBusNum, int *piError);

#endif

#if (COM_VERSION >= 0x04010000)

EXTERN_DECL void* __stdcall DFCGetClass(int iChannelNum, int id);

#endif

#endif // USE_PRIVATE

#endif


