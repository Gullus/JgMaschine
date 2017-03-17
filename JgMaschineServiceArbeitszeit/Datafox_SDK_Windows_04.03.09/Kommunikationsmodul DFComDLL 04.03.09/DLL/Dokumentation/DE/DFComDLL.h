#ifndef DFCOMDLL_H
#define DFCOMDLL_H

#include <QLibrary>
#include "SoftwareVersion.h"
#ifdef WITH_DLLGETVERSION
#ifndef _HRESULT_DEFINED
#define _HRESULT_DEFINED
#ifdef __midl
typedef LONG HRESULT;
#else
typedef __success(return >= 0) long HRESULT;
#endif // __midl
#endif // !_HRESULT_DEFINED

struct _DLLVERSIONINFO;
#endif

#if defined(__GNUG__) && !defined(__MINGW32__)
#define __stdcall
#endif

class DFComDLL : public QLibrary
{
public:
    DFComDLL(QObject *parent = 0);
    virtual ~DFComDLL();

public:
    QString m_errorText;
    void init();
    int Validate(int version);
    int GetVersion( SoftwarebuildVersion &version );
    QString &GetValidateErrorText() { return m_errorText; };

public:
    // ########################################################
    // ############ FUNKTIONEN FUER ALLGEMEIN #################
    // ########################################################

    typedef int (__stdcall *__DFCComInit)(int);
    __DFCComInit _DFCComInit;
    int DFCComInit(int channelID) 
    { 
        return _DFCComInit(channelID); 
    }
    int ComInit(int channelID) 
    { 
        return DFCComInit(channelID); 
    }

    typedef int (__stdcall *__DFCComOpenSerial)(int, const char*, int, int);
    __DFCComOpenSerial _DFCComOpenSerial;
    int DFCComOpenSerial(int channelID, const char *commString, int commValue, int commTimeout)
    {
        return _DFCComOpenSerial(channelID, commString, commValue, commTimeout);
    }
    int ComOpenSerial(int channelID, const char *commString, int commValue, int commTimeout)
    {
        return DFCComOpenSerial(channelID, commString, commValue, commTimeout);
    }

    typedef int (__stdcall *__DFCComOpen)(int, const char*);
    __DFCComOpen _DFCComOpen;
    int DFCComOpen(int channelID, const char *commString)
    {
        return _DFCComOpen(channelID, commString);
    }
    int ComOpen(int channelID, const char *commString)
    {
        return DFCComOpen(channelID, commString);
    }

    typedef int (__stdcall *__DFCComOpenSocket)(int, const char*, int, int);
    __DFCComOpenSocket _DFCComOpenSocket;
    int DFCComOpenSocket(int channelID, const char *commString, int commValue, int commTimeout)
    {
        return _DFCComOpenSocket(channelID, commString, commValue, commTimeout);
    }
    int ComOpenSocket(int channelID, const char *commString, int commValue, int commTimeout)
    {
        return DFCComOpenSocket(channelID, commString, commValue, commTimeout);
    }

private:
    typedef int (__stdcall *__DFCComOpenIV)(int, int, int, const char*, int, int);
    __DFCComOpenIV _DFCComOpenIV;
public:
    int DFCComOpenIV(int channelID, int DLL_deviceID, int commType, const char *commString, int commValue, int commTimeout)
    {
        return _DFCComOpenIV(channelID, DLL_deviceID, commType, commString, commValue, commTimeout);
    }
    int ComOpenIV(int channelID, int DLL_deviceID, int commType, const char *commString, int commValue, int commTimeout)
    {
        return DFCComOpenIV(channelID, DLL_deviceID, commType, commString, commValue, commTimeout);
    }

    typedef void (__stdcall *__DFCComClose)(int);
    __DFCComClose _DFCComClose;
    void DFCComClose(int channelID)
    {
        _DFCComClose(channelID);
    }
    void ComClose(int channelID)
    {
        DFCComClose(channelID);
    }

    typedef int (__stdcall *__DFCCheckAE)(int, int);
    __DFCCheckAE _DFCCheckAE;
    int DFCCheckAE(int channelID, int deviceID)
    {
        return _DFCCheckAE(channelID, deviceID);
    }
    int CheckAE(int channelID, int deviceID)
    {
        return DFCCheckAE(channelID, deviceID);
    }

    typedef int (__stdcall *__DFCCheckDevice)(int, int, int*, int);
    __DFCCheckDevice _DFCCheckDevice;
    int DFCCheckDevice(int channelID, int deviceID, int *piError, int retries)
    {
        return _DFCCheckDevice(channelID, deviceID, piError, retries);
    }
    int CheckDevice(int channelID, int deviceID, int *piError, int retries)
    {
        return DFCCheckDevice(channelID, deviceID, piError, retries);
    }

    typedef int (__stdcall *__DFCComSetTime)(int, int, const unsigned char*);
    __DFCComSetTime _DFCComSetTime;
    int DFCComSetTime(int channelID, int deviceID, const unsigned char *dt)
    {
        return _DFCComSetTime(channelID, deviceID, dt);
    }
    int ComSetTime(int channelID, int deviceID, const unsigned char *dt)
    {
        return DFCComSetTime(channelID, deviceID, dt);
    }

    typedef int (__stdcall *__DFCComGetTime)(int, int, unsigned char*);
    __DFCComGetTime _DFCComGetTime;
    int DFCComGetTime(int channelID, int deviceID, unsigned char *dt)
    {
        return _DFCComGetTime(channelID, deviceID, dt);
    }
    int ComGetTime(int channelID, int deviceID, unsigned char *dt)
    {
        return DFCComGetTime(channelID, deviceID, dt);
    }

    typedef int (__stdcall *__DFCComSendMessage)(int, int, unsigned char, unsigned char, unsigned char, const char*, int);
    __DFCComSendMessage _DFCComSendMessage;
    int DFCComSendMessage(int channelID, int deviceID, unsigned char showTime, unsigned char unused, unsigned char peepSignal, const char *message, int messageLength)
    {
        return _DFCComSendMessage(channelID, deviceID, showTime, unused, peepSignal, message, messageLength);
    }
    int ComSendMessage(int channelID, int deviceID, unsigned char showTime, unsigned char unused, unsigned char peepSignal, const char *message, int messageLength)
    {
        return DFCComSendMessage(channelID, deviceID, showTime, unused, peepSignal, message, messageLength);
    }

    typedef int (__stdcall *__DFCComSendInfotext)(int, int, const char*, int);
    __DFCComSendInfotext _DFCComSendInfotext;
    int DFCComSendInfotext(int channelID, int deviceID, const char *message, int messageLength)
    {
        return _DFCComSendInfotext(channelID, deviceID, message, messageLength);
    }
    int ComSendInfotext(int channelID, int deviceID, const char *message, int messageLength)
    {
        return DFCComSendInfotext(channelID, deviceID, message, messageLength);
    }

    typedef int (__stdcall *__DFCGetSeriennummer)(int, int, int*, int*);
    __DFCGetSeriennummer _DFCGetSeriennummer;
    int DFCGetSeriennummer(int channelID, int deviceID, int *piError, int *piSerialNumber)
    {
        return _DFCGetSeriennummer(channelID, deviceID, piError, piSerialNumber);
    }
    int GetSeriennummer(int channelID, int deviceID, int *piError, int *piSerialNumber)
    {
        return DFCGetSeriennummer(channelID, deviceID, piError, piSerialNumber);
    }

    typedef void (__stdcall *__DFCSetLogOn)(int);
    __DFCSetLogOn _DFCSetLogOn;
    void DFCSetLogOn(int channelID)
    {
        _DFCSetLogOn(channelID);
    }
    void SetLogOn(int channelID)
    {
        DFCSetLogOn(channelID);
    }

    typedef void (__stdcall *__DFCSetLogOff)(int);
    __DFCSetLogOff _DFCSetLogOff;
    void DFCSetLogOff(int channelID)
    {
        _DFCSetLogOff(channelID);
    }
    void SetLogOff(int channelID)
    {
        DFCSetLogOff(channelID);
    }

    typedef void (__stdcall *__DFCSetCallBack)(int, int (*CBFunction)(void));
    __DFCSetCallBack _DFCSetCallBack;
    void DFCSetCallBack(int channelID, int (*CBFunction)(void))
    {
        _DFCSetCallBack(channelID, CBFunction);
    }
    void SetCallBack(int channelID, int (*CBFunction)(void))
    {
        DFCSetCallBack(channelID, CBFunction);
    }

    typedef void (__stdcall *__DFCSetLogFileName)(int, const char*);
    __DFCSetLogFileName _DFCSetLogFileName;
    void DFCSetLogFileName(int channelID, const char *fileName)
    {
        _DFCSetLogFileName(channelID, fileName);
    }
    void SetLogFileName(int channelID, const char *fileName)
    {
        DFCSetLogFileName(channelID, fileName);
    }

    typedef void (__stdcall *__DFCGetErrorText)(int, int, int, char*, int);
    __DFCGetErrorText _DFCGetErrorText;
    void DFCGetErrorText(int channelID, int error, int language, char *text, int textLength)
    {
        _DFCGetErrorText(channelID, error, language, text, textLength);
    }
    void GetErrorText(int channelID, int error, int language, char *text, int textLength)
    {
        DFCGetErrorText(channelID, error, language, text, textLength);
    }

    typedef int (__stdcall *__DFCSetGlobVar)(int, int, const char*, int, const char*, int*);
    __DFCSetGlobVar _DFCSetGlobVar;
    int DFCSetGlobVar(int channelID, int deviceID, const char *varName, int nameType, const char *varValue, int *piError)
    {
        return _DFCSetGlobVar(channelID, deviceID, varName, nameType, varValue, piError);
    }
    int SetGlobVar(int channelID, int deviceID, const char *varName, int nameType, const char *varValue, int *piError)
    {
        return DFCSetGlobVar(channelID, deviceID, varName, nameType, varValue, piError);
    }

    typedef int (__stdcall *__DFCGetGlobVar)(int, int, const char*, int, char*, int, int*);
    __DFCGetGlobVar _DFCGetGlobVar;
    int DFCGetGlobVar(int channelID, int deviceID, const char *varName, int nameType, char *varValue, int valueLength, int *piError)
    {
        return _DFCGetGlobVar(channelID, deviceID, varName, nameType, varValue, valueLength, piError);
    }
    int GetGlobVar(int channelID, int deviceID, const char *varName, int nameType, char *varValue, int valueLength, int *piError)
    {
        return DFCGetGlobVar(channelID, deviceID, varName, nameType, varValue, valueLength, piError);
    }

    typedef int (__stdcall *__DFCCloseRelay)(int, int, int, int, int*);
    __DFCCloseRelay _DFCCloseRelay;	
    int DFCCloseRelay(int channelID, int deviceID, int relayNum, int closeTime, int *piError)
    {
        return _DFCCloseRelay(channelID, deviceID, relayNum, closeTime, piError);
    }
    int CloseRelay(int channelID, int deviceID, int relayNum, int closeTime, int *piError)
    {
        return DFCCloseRelay(channelID, deviceID, relayNum, closeTime, piError);
    }

    typedef int (__stdcall *__DFCGetRelayState)(int, int, int, int*, int*, int*);
    __DFCGetRelayState _DFCGetRelayState;
    int DFCGetRelayState(int channelID, int deviceID, int relayNum, int *relayState, int *closeTime, int *piError)
    {
        return _DFCGetRelayState(channelID, deviceID, relayNum, relayState, closeTime, piError);
    }
    int GetRelayState(int channelID, int deviceID, int relayNum, int *relayState, int *closeTime, int *piError)
    {
        return DFCGetRelayState(channelID, deviceID, relayNum, relayState, closeTime, piError);
    }

    typedef int (__stdcall *__DFCOpenRelay)(int, int, int, int*);
    __DFCOpenRelay _DFCOpenRelay;
    int DFCOpenRelay(int channelID, int deviceID, int relayNum, int *piError)
    {
        return _DFCOpenRelay(channelID, deviceID, relayNum, piError);
    }
    int OpenRelay(int channelID, int deviceID, int relayNum, int *piError)
    {
        return DFCOpenRelay(channelID, deviceID, relayNum, piError);
    }

    typedef int (__stdcall *__DFCGetDevicePollRetry)(int);
    __DFCGetDevicePollRetry _DFCGetDevicePollRetry;
    int DFCGetDevicePollRetry(int channelID)
    {
        return _DFCGetDevicePollRetry(channelID);
    }
    int GetDevicePollRetry(int channelID)
    {
        return DFCGetDevicePollRetry(channelID);
    }

    typedef Qt::HANDLE (__stdcall *__DFCGetComPort)(int);
    __DFCGetComPort _DFCGetComPort;
    Qt::HANDLE DFCGetComPort(int channelID)
    {
        return _DFCGetComPort(channelID);
    }
    Qt::HANDLE GetComPort(int channelID)
    {
        return DFCGetComPort(channelID);
    }

    typedef int (__stdcall *__DFCSetComPort)(int, Qt::HANDLE, int, int, int);
    __DFCSetComPort _DFCSetComPort;
    int DFCSetComPort(int channelID, Qt::HANDLE comm, int commType, int commValue, int commTimeout)
    {
        return _DFCSetComPort(channelID, comm, commType, commValue, commTimeout);
    }
    int SetComPort(int channelID, Qt::HANDLE comm, int commType, int commValue, int commTimeout)
    {
        return DFCSetComPort(channelID, comm, commType, commValue, commTimeout);
    }

    typedef int (__stdcall *__DFCWrite)(int, const char*, int, int*, int*);
    __DFCWrite _DFCWrite;
    int DFCWrite(int channelID, char *buf, int bufLength, int *writtenLength, int *piError)
    {
        return _DFCWrite(channelID, buf, bufLength, writtenLength, piError);
    }
    int Write(int channelID, char *buf, int bufLength, int *writtenLength, int *piError)
    {
        return DFCWrite(channelID, buf, bufLength, writtenLength, piError);
    }

    typedef int (__stdcall *__DFCRead)(int, char*, int, int*, int*);
    __DFCRead _DFCRead;
    int DFCRead(int channelID, char *buf, int bufLength, int *amountLength, int *piError)
    {
        return _DFCRead(channelID, buf, bufLength, amountLength, piError);
    }
    int Read(int channelID, char *buf, int bufLength, int *amountLength, int *piError)
    {
        return DFCRead(channelID, buf, bufLength, amountLength, piError);
    }

    typedef int (__stdcall *__DFCUpload)(int, int, const char*, int*);
    __DFCUpload _DFCUpload;
    int DFCUpload(int channelID, int deviceID, const char *fileName, int *piError)
    {
        return _DFCUpload(channelID, deviceID, fileName, piError);
    }
    int Upload(int channelID, int deviceID, const char *fileName, int *piError)
    {
        return DFCUpload(channelID, deviceID, fileName, piError);
    }

    typedef int (__stdcall *__DFCGetVersionFirmware)(int, int, char*, int*);
    __DFCGetVersionFirmware _DFCGetVersionFirmware;
    int DFCGetVersionFirmware(int channelID, int deviceID, char *version, int *piError)
    {
        return _DFCGetVersionFirmware(channelID, deviceID, version, piError);
    }
    int GetVersionFirmware(int channelID, int deviceID, char *version, int *piError)
    {
        return DFCGetVersionFirmware(channelID, deviceID, version, piError);
    }

    typedef int (__stdcall *__DFCGetVersionFirmwareFromFile)(int, const char*, char*, int*);
    __DFCGetVersionFirmwareFromFile _DFCGetVersionFirmwareFromFile;
    int DFCGetVersionFirmwareFromFile(int channelID, const char *fileName, char *version, int *piError)
    {
        return _DFCGetVersionFirmwareFromFile(channelID, fileName, version, piError);
    }
    int GetVersionFirmwareFromFile(int channelID, const char *fileName, char *version, int *piError)
    {
        return DFCGetVersionFirmwareFromFile(channelID, fileName, version, piError);
    }

    typedef int (__stdcall *__DFCGetInfo)(int, int, const char*, const char*, char*, int*, int*);
    __DFCGetInfo _DFCGetInfo;
    int DFCGetInfo(int channelID, int deviceID, const char *name, const char *params, char *info, int *infoLength, int *piError)
    {
        return _DFCGetInfo(channelID, deviceID, name, params, info, infoLength, piError);
    }
    int GetInfo(int channelID, int deviceID, const char *name, const char *params, char *info, int *infoLength, int *piError)
    {
        return DFCGetInfo(channelID, deviceID, name, params, info, infoLength, piError);
    }

    typedef int (__stdcall *__DFCOpenComServerMode)(int, int, const char*, int, int, int, int, int, int, int*);
    __DFCOpenComServerMode _DFCOpenComServerMode;
    int DFCOpenComServerMode(int channelID, int deviceID, const char *commString, int baudRate, int byteSize, int parity, int stopBits, int flags, int timeoutToClose, int *piError)
    {
        return _DFCOpenComServerMode(channelID, deviceID, commString, baudRate, byteSize, parity, stopBits, flags, timeoutToClose, piError);
    }
    int OpenComServerMode(int channelID, int deviceID, const char *commString, int baudRate, int byteSize, int parity, int stopBits, int flags, int timeoutToClose, int *piError)
    {
        return DFCOpenComServerMode(channelID, deviceID, commString, baudRate, byteSize, parity, stopBits, flags, timeoutToClose, piError);
    }

    typedef int (__stdcall *__DFCCloseComServerMode)(int, int, int*);
    __DFCCloseComServerMode _DFCCloseComServerMode;
    int DFCCloseComServerMode(int channelID, int deviceID, int *piError)
    {
        return _DFCCloseComServerMode(channelID, deviceID, piError);
    }
    int CloseComServerMode(int channelID, int deviceID, int *piError)
    {
        return DFCCloseComServerMode(channelID, deviceID, piError);
    }

    typedef int (__stdcall *__DFCIsChannelOpen)(int);
    __DFCIsChannelOpen _DFCIsChannelOpen;
    int DFCIsChannelOpen(int channelID)
    {
        return _DFCIsChannelOpen(channelID);
    }
    int IsChannelOpen(int channelID)
    {
        return DFCIsChannelOpen(channelID);
    }

    typedef int (__stdcall *__DFCUploadModule)(int, int, int, const char*, int*);
    __DFCUploadModule _DFCUploadModule;
    int DFCUploadModule(int channelID, int deviceID, int moduleType, const char *fileName, int *piError)
    {
        return _DFCUploadModule(channelID, deviceID, moduleType, fileName, piError);
    }
    int UploadModule(int channelID, int deviceID, int moduleType, const char *fileName, int *piError)
    {
        return DFCUploadModule(channelID, deviceID, moduleType, fileName, piError);
    }


    typedef int (__stdcall *__DFCSetOptionFirmware)(int, int, int, int, int*);
    __DFCSetOptionFirmware _DFCSetOptionFirmware;
    int DFCSetOptionFirmware(int channelID, int deviceID, int mask, int options, int *piError)
    {
        return _DFCSetOptionFirmware(channelID, deviceID, mask, options, piError);
    }
    int SetOptionFirmware(int channelID, int deviceID, int mask, int options, int *piError)
    {
        return DFCSetOptionFirmware(channelID, deviceID, mask, options, piError);
    }

    typedef int (__stdcall *__DFCGetOptionFirmware)(int, int, int*, int*, int*);
    __DFCGetOptionFirmware _DFCGetOptionFirmware;
    int DFCGetOptionFirmware(int channelID, int deviceID, int *mask, int *options, int *piError)
    {
        return _DFCGetOptionFirmware(channelID, deviceID, mask, options, piError);
    }
    int GetOptionFirmware(int channelID, int deviceID, int *mask, int *options, int *piError)
    {
        return DFCGetOptionFirmware(channelID, deviceID, mask, options, piError);
    }

    typedef int (__stdcall *__DFCReset)(int, int, int, int*);
    __DFCReset _DFCReset;
    int DFCReset(int channelID, int deviceID, int mode, int *piError)
    {
        return _DFCReset(channelID, deviceID, mode, piError);
    }
    int Reset(int channelID, int deviceID, int mode, int *piError)
    {
        return DFCReset(channelID, deviceID, mode, piError);
    }

    typedef int (__stdcall *__DFCSetFontType)(int, int, int, int*);
    __DFCSetFontType _DFCSetFontType;
    int DFCSetFontType(int channelID, int deviceID, int iType, int *piError)
    {
        return _DFCSetFontType(channelID, deviceID, iType, piError);
    }
    int SetFontType(int channelID, int deviceID, int iType, int *piError)
    {
        return DFCSetFontType(channelID, deviceID, iType, piError);
    }

    typedef int (__stdcall *__DFCSetPassword)(int, const char*, int*);
    __DFCSetPassword _DFCSetPassword;
    int DFCSetPassword(int channelID, const char *password, int *piError)
    {
        return _DFCSetPassword(channelID, password, piError);
    }
    int SetPassword(int channelID, const char *password, int *piError)
    {
        return DFCSetPassword(channelID, password, piError);
    }

    typedef int (__stdcall *__DFCGetPasswordKey)(int, int, char*, int*, int*);
    __DFCGetPasswordKey _DFCGetPasswordKey;
    int DFCGetPasswordKey(int channelID, int deviceID, char *key, int *len, int *piError)
    {
        return _DFCGetPasswordKey(channelID, deviceID, key, len, piError);
    }
    int GetPasswordKey(int channelID, int deviceID, char *key, int *len, int *piError)
    {
        return DFCGetPasswordKey(channelID, deviceID, key, len, piError);
    }

    typedef int (__stdcall *__DFCPressVirtualKey)(int, int, int, int, int*);
    __DFCPressVirtualKey _DFCPressVirtualKey;
    int DFCPressVirtualKey(int channelID, int deviceID, int vk, int flags, int *piError)
    {
        return _DFCPressVirtualKey(channelID, deviceID, vk, flags, piError);
    }
    int PressVirtualKey(int channelID, int deviceID, int vk, int flags, int *piError)
    {
        return DFCPressVirtualKey(channelID, deviceID, vk, flags, piError);
    }

    typedef int (__stdcall *__DFCCommandInspectionStation)(int channelID, int deviceID, int address, const char *data, int sizeData, int *addressAnswer, char *dataAnswer, int *sizeAnswer, int toPending);
    __DFCCommandInspectionStation _DFCCommandInspectionStation;
    int DFCCommandInspectionStation(int channelID, int deviceID, int address, const char *data, int sizeData, int *addressAnswer, char *dataAnswer, int *sizeAnswer, int toPending)
    {
        return _DFCCommandInspectionStation(channelID, deviceID, address, data, sizeData, addressAnswer, dataAnswer, sizeAnswer, toPending);
    }
    int CommandInspectionStation(int channelID, int deviceID, int address, const char *data, int sizeData, int *addressAnswer, char *dataAnswer, int *sizeAnswer, int toPending)
    {
        return DFCCommandInspectionStation(channelID, deviceID, address, data, sizeData, addressAnswer, dataAnswer, sizeAnswer, toPending);
    }

    typedef int (__stdcall *__DFCGetFlashStatus)(int, int, int*, int*);
    __DFCGetFlashStatus _DFCGetFlashStatus;
    int DFCGetFlashStatus(int channelID, int deviceID, int *piStatus, int *piError)
    {
        return _DFCGetFlashStatus(channelID, deviceID, piStatus, piError);
    }
    int GetFlashStatus(int channelID, int deviceID, int *piStatus, int *piError)
    {
        return DFCGetFlashStatus(channelID, deviceID, piStatus, piError);
    }

    typedef int (__stdcall *__DFCSetCommunicationPassword)(int, const char*, int, int, int*);
    __DFCSetCommunicationPassword _DFCSetCommunicationPassword;
    int DFCSetCommunicationPassword(int channelID, const char *password, int size, int sessionTimeout, int *piError)
    {
        return _DFCSetCommunicationPassword(channelID, password, size, sessionTimeout, piError);
    }
    int SetCommunicationPassword(int channelID, const char *password, int size, int sessionTimeout, int *piError)
    {
        return DFCSetCommunicationPassword(channelID, password, size, sessionTimeout, piError);
    }

    typedef int (__stdcall *__DFCReadHardwareInfo)(int, int, char*, int*, int, int*);
    __DFCReadHardwareInfo _DFCReadHardwareInfo;
    int DFCReadHardwareInfo(int channelID, int deviceID, char *infoString, int *infoStringLength, int flags, int *piError)
    {
        return _DFCReadHardwareInfo(channelID, deviceID, infoString, infoStringLength, flags, piError);
    }
    int ReadHardwareInfo(int channelID, int deviceID, char *infoString, int *infoStringLength, int flags, int *piError)
    {
        return DFCReadHardwareInfo(channelID, deviceID, infoString, infoStringLength, flags, piError);
    }

    typedef int (__stdcall *__DFCFileUpload)(int, int, int, const char*, int, int*);
    __DFCFileUpload _DFCFileUpload;
    int DFCFileUpload(int channelId, int deviceId, int type, const char *fileName, int flags, int *error)
    {
        return _DFCFileUpload(channelId, deviceId, type, fileName, flags, error);
    }
    int FileUpload(int channelId, int deviceId, int type, const char *fileName, int flags, int *error)
    {
        return DFCFileUpload(channelId, deviceId, type, fileName, flags, error);
    }

    typedef int (__stdcall *__DFCFileDownload)(int, int, int, const char*, int, int*);
    __DFCFileDownload _DFCFileDownload;
    int DFCFileDownload(int channelId, int deviceId, int type, const char *fileName, int flags, int *error)
    {
        return _DFCFileDownload(channelId, deviceId, type, fileName, flags, error);
    }
    int FileDownload(int channelId, int deviceId, int type, const char *fileName, int flags, int *error)
    {
        return DFCFileDownload(channelId, deviceId, type, fileName, flags, error);
    }

    typedef int (__stdcall *__DFCRecordVolume)(int, int, int*, int*, int*, int*);
    __DFCRecordVolume _DFCRecordVolume;
    int DFCRecordVolume(int channelID, int deviceID, int *number, int *memoryUsage, int *memorySize, int *piError)
    {
        return _DFCRecordVolume(channelID, deviceID, number, memoryUsage, memorySize, piError);
    }
    int RecordVolume(int channelID, int deviceID,  int *number, int *memoryUsage, int *memorySize, int *piError)
    {
        return DFCRecordVolume(channelID, deviceID, number, memoryUsage, memorySize, piError);
    }

    typedef int (__stdcall *__DFCVersionLibrary)(char*, int);
    __DFCVersionLibrary _DFCVersionLibrary;
    int DFCVersionLibrary(char *version, int length)
    {
        return _DFCVersionLibrary(version, length);
    }

    typedef int (__stdcall *__DFCCustomCommand)( int, int, int, int, const char *, int, char *, int *, int * );
    __DFCCustomCommand _DFCCustomCommand;
    int DFCCustomCommand( int channelId, int deviceId,int identification, int customCommand, const char *dataIn, int sizeIn, char *dataOut, int *sizeOut, int *piError )
    {
        return _DFCCustomCommand( channelId, deviceId, identification, customCommand, dataIn, sizeIn, dataOut, sizeOut, piError );
    }
    int customCommand( int channelId, int deviceId, int identification, int customCommand, const char *dataIn, int sizeIn, char *dataOut, int *sizeOut, int *piError )
    {
        return DFCCustomCommand( channelId, deviceId, identification, customCommand, dataIn, sizeIn, dataOut, sizeOut, piError );
    }

    typedef int (__stdcall *__DFCGetLastErrorNumber)(int, int);
    __DFCGetLastErrorNumber _DFCGetLastErrorNumber;
    int DFCGetLastErrorNumber(int connectionId, int deviceAddress)
    {
        return _DFCGetLastErrorNumber(connectionId, deviceAddress);
    }
    int GetLastErrorNumber(int connectionId, int deviceAddress)
    {
        return DFCGetLastErrorNumber(connectionId, deviceAddress);
    }

    typedef int (__stdcall *__DFCBlockTransferSetDuration)(int, int, int, int);
    __DFCBlockTransferSetDuration _DFCBlockTransferSetDuration;
    int DFCBlockTransferSetDuration(int connectionId, int deviceAddress, int blockTypeFlags, int duration)
    {
        return _DFCBlockTransferSetDuration(connectionId, deviceAddress, blockTypeFlags, duration);
    }
    int BlockTransferSetDuration(int connectionId, int deviceAddress, int blockTypeFlags, int duration)
    {
        return DFCBlockTransferSetDuration(connectionId, deviceAddress, blockTypeFlags, duration);
    }

    typedef int (__stdcall *__DFCBlockTransferResume)(int, int);
    __DFCBlockTransferResume _DFCBlockTransferResume;
    int DFCBlockTransferResume(int connectionId, int deviceAddress)
    {
        return _DFCBlockTransferResume(connectionId, deviceAddress);
    }
    int BlockTransferResume(int connectionId, int deviceAddress)
    {
        return DFCBlockTransferResume(connectionId, deviceAddress);
    }

    typedef int (__stdcall *__DFCBlockTransferGetState)(int, int, int *, int *);
    __DFCBlockTransferGetState _DFCBlockTransferGetState;
    int DFCBlockTransferGetState(int connectionId, int deviceAddress, int *blockTypeFlag, int *state)
    {
        return _DFCBlockTransferGetState(connectionId, deviceAddress, blockTypeFlag, state);
    }
    int BlockTransferGetState(int connectionId, int deviceAddress, int *blockTypeFlag, int *state)
    {
        return DFCBlockTransferGetState(connectionId, deviceAddress, blockTypeFlag, state);
    }

    typedef int (__stdcall *__DFCBlockTransferDiscard)(int, int);
    __DFCBlockTransferDiscard _DFCBlockTransferDiscard;
    int DFCBlockTransferDiscard(int connectionId, int deviceAddress)
    {
        return _DFCBlockTransferDiscard(connectionId, deviceAddress);
    }
    int BlockTransferDiscard(int connectionId, int deviceAddress)
    {
        return DFCBlockTransferDiscard(connectionId, deviceAddress);
    }

    // ########################################################
    // ############ FUNKTIONEN FUER SYTEM LOG #################
    // ########################################################

    typedef int (__stdcall *__DFCSystemReadRecord)(int channelID, int deviceID, unsigned char *buf, int *length, int *error);
    __DFCSystemReadRecord _DFCSystemReadRecord;
    int DFCSystemReadRecord(int channelID, int deviceID, unsigned char *buf, int *length, int *error)
    {
        return _DFCSystemReadRecord(channelID, deviceID, buf, length, error);
    }
    int SystemReadRecord(int channelID, int deviceID, unsigned char *buf, int *length, int *error)
    {
        return DFCSystemReadRecord(channelID, deviceID, buf, length, error);
    }

    typedef int (__stdcall *__DFCSystemQuitRecord)(int channelID, int deviceID, int *error);
    __DFCSystemQuitRecord _DFCSystemQuitRecord;
    int DFCSystemQuitRecord(int channelID, int deviceID, int *error)
    {
        return _DFCSystemQuitRecord(channelID, deviceID, error);
    }
    int SystemQuitRecord(int channelID, int deviceID, int *error)
    {
        return DFCSystemQuitRecord(channelID, deviceID, error);
    }

    typedef int (__stdcall *__DFCSystemRestoreRecords)(int channelID, int deviceID, int *piError);
    __DFCSystemRestoreRecords _DFCSystemRestoreRecords;
    int DFCSystemRestoreRecords(int channelID, int deviceID, int *piError)
    {
        return _DFCSystemRestoreRecords(channelID, deviceID, piError);
    }
    int SystemRestoreRecords(int channelID, int deviceID, int *piError)
    {
        return DFCSystemRestoreRecords(channelID, deviceID, piError);
    }


    // ########################################################
    // ############## FUNKTIONEN FUER SETUP ###################
    // ########################################################

    typedef int (__stdcall *__DFCSetupLaden)(int,int, const char*, int*);
    __DFCSetupLaden _DFCSetupLaden;
    int DFCSetupLaden(int channelID, int deviceID, const char *fileName, int *piError)
    {
        return _DFCSetupLaden(channelID, deviceID, fileName, piError);
    }
    int SetupLaden(int channelID, int deviceID, const char *fileName, int *piError)
    {
        return DFCSetupLaden(channelID, deviceID, fileName, piError);
    }

    typedef int (__stdcall *__DFCDownload)(int,int, const char*, int*);
    __DFCDownload _DFCDownload;
    int DFCDownload(int channelID, int deviceID, const char *fileName, int *piError)
    {
        return _DFCDownload(channelID, deviceID, fileName, piError);
    }
    int Download(int channelID, int deviceID, const char *fileName, int *piError)
    {
        return DFCDownload(channelID, deviceID, fileName, piError);
    }

    typedef int (__stdcall *__DFCModifyStudioFile)(const char*, const char*, const char*, int, int*);
    __DFCModifyStudioFile _DFCModifyStudioFile;
    int DFCModifyStudioFile(const char *sourceFile, const char *destFile, const char *changeData, int flags, int *piError)
    {
        return _DFCModifyStudioFile(sourceFile, destFile, changeData, flags, piError);
    }
    int ModifyStudioFile(const char *sourceFile, const char *destFile, const char *changeData, int flags, int *piError)
    {
        return DFCModifyStudioFile(sourceFile, destFile, changeData, flags, piError);
    }


    // ########################################################
    // FUNKTIONEN FUER LISTEN, SOWIE ZUTRITTSLISTEN (VERSION 2)
    // ########################################################

    typedef int (__stdcall *__DFCOpenTable)(int, int, const char*, int, int*, const char*, int*);
    __DFCOpenTable _DFCOpenTable;
    int DFCOpenTable(int channelID, int deviceID, const char *name, int options, int *handle, const char *reserved, int *piError)
    {
        return _DFCOpenTable(channelID, deviceID, name, options, handle, reserved, piError);
    }
    int OpenTable(int channelID, int deviceID, const char *name, int options, int *handle, const char *reserved, int *piError)
    {
        return DFCOpenTable(channelID, deviceID, name, options, handle, reserved, piError);
    }

    typedef int (__stdcall *__DFCCloseTable)(int, int, int, int*);
    __DFCCloseTable _DFCCloseTable;
    int DFCCloseTable(int channelID, int deviceID, int handle, int *piError)
    {
        return _DFCCloseTable(channelID, deviceID, handle, piError);
    }
    int CloseTable(int channelID, int deviceID, int handle, int *piError)
    {
        return DFCCloseTable(channelID, deviceID, handle, piError);
    }

    typedef int (__stdcall *__DFCSetFilter)(int, int, int, const char*, int*);
    __DFCSetFilter _DFCSetFilter;
    int DFCSetFilter(int channelID, int deviceID, int handle, const char *filter, int *piError)
    {
        return _DFCSetFilter(channelID, deviceID, handle, filter, piError);
    }
    int SetFilter(int channelID, int deviceID, int handle, const char *filter, int *piError)
    {
        return DFCSetFilter(channelID, deviceID, handle, filter, piError);
    }

    typedef int (__stdcall *__DFCGetFilter)(int, int, int, char*, int*, int*);
    __DFCGetFilter _DFCGetFilter;
    int DFCGetFilter(int channelID, int deviceID, int handle, char *filter, int *len, int *piError)
    {
        return _DFCGetFilter(channelID, deviceID, handle, filter, len, piError);
    }
    int GetFilter(int channelID, int deviceID, int handle, char *filter, int *len, int *piError)
    {
        return DFCGetFilter(channelID, deviceID, handle, filter, len, piError);
    }

    typedef int (__stdcall *__DFCClearFilter)(int, int, int, int*);
    __DFCClearFilter _DFCClearFilter;
    int DFCClearFilter(int channelID, int deviceID, int handle, int *piError)
    {
        return _DFCClearFilter(channelID, deviceID, handle, piError);
    }
    int ClearFilter(int channelID, int deviceID, int handle, int *piError)
    {
        return DFCClearFilter(channelID, deviceID, handle, piError);
    }

    typedef int (__stdcall *__DFCSkip)(int, int, int, int, int, int*);
    __DFCSkip _DFCSkip;
    int DFCSkip(int channelID, int deviceID, int handle, int offset, int origin, int *piError)
    {
        return _DFCSkip(channelID, deviceID, handle, offset, origin, piError);
    }
    int Skip(int channelID, int deviceID, int handle, int offset, int origin, int *piError)
    {
        return DFCSkip(channelID, deviceID, handle, offset, origin, piError);
    }

    typedef int (__stdcall *__DFCSetField)(int, int, int, const char*, const char*, int*);
    __DFCSetField _DFCSetField;
    int DFCSetField(int channelID, int deviceID, int handle, const char *name, const char *value, int *piError)
    {
        return _DFCSetField(channelID, deviceID, handle, name, value, piError);
    }
    int SetField(int channelID, int deviceID, int handle, const char *name, const char *value, int *piError)
    {
        return DFCSetField(channelID, deviceID, handle, name, value, piError);
    }

    typedef int (__stdcall *__DFCGetField)(int, int, int, const char*, char*, int*, int*);
    __DFCGetField _DFCGetField;
    int DFCGetField(int channelID, int deviceID, int handle, const char *name, char *value, int *len, int *piError)
    {
        return _DFCGetField(channelID, deviceID, handle, name, value, len, piError);
    }
    int GetField(int channelID, int deviceID, int handle, const char *name, char *value, int *len, int *piError)
    {
        return DFCGetField(channelID, deviceID, handle, name, value, len, piError);
    }

    typedef int (__stdcall *__DFCTableOpen)(int, int, const char *, int, int *);
    __DFCTableOpen _DFCTableOpen;
    int DFCTableOpen(int connectionId, int deviceAddress, const char *name, int options, int *tableHandle)
    {
        return _DFCTableOpen(connectionId, deviceAddress, name, options, tableHandle);
    }
    int TableOpen(int connectionId, int deviceAddress, const char *name, int options, int *tableHandle)
    {
        return DFCTableOpen(connectionId, deviceAddress, name, options, tableHandle);
    }
    
    typedef int (__stdcall *__DFCTableClose)(int, int, int);
    __DFCTableClose _DFCTableClose;
    int DFCTableClose(int connectionId, int deviceAddress, int tableHandle)
    {
        return _DFCTableClose(connectionId, deviceAddress, tableHandle);
    }
    int TableClose(int connectionId, int deviceAddress, int tableHandle)
    {
        return DFCTableClose(connectionId, deviceAddress, tableHandle);
    }

    typedef int (__stdcall *__DFCTableSetFilter)(int, int, int, const char *);
    __DFCTableSetFilter _DFCTableSetFilter;
    int DFCTableSetFilter(int connectionId, int deviceAddress, int tableHandle, const char *value)
    {
        return _DFCTableSetFilter(connectionId, deviceAddress, tableHandle, value);
    }
    int TableSetFilter(int connectionId, int deviceAddress, int tableHandle, const char *value)
    {
        return DFCTableSetFilter(connectionId, deviceAddress, tableHandle, value);
    }
    
    typedef int (__stdcall *__DFCTableGetFilter)(int, int, int, char *, int *);
    __DFCTableGetFilter _DFCTableGetFilter;
    int DFCTableGetFilter(int connectionId, int deviceAddress, int tableHandle, char *value, int *valueSize)
    {
        return _DFCTableGetFilter(connectionId, deviceAddress, tableHandle, value, valueSize);
    }
    int TableGetFilter(int connectionId, int deviceAddress, int tableHandle, char *value, int *valueSize)
    {
        return DFCTableGetFilter(connectionId, deviceAddress, tableHandle, value, valueSize);
    }
    
    typedef int (__stdcall *__DFCTableRemoveFilter)(int, int, int);
    __DFCTableRemoveFilter _DFCTableRemoveFilter;
    int DFCTableRemoveFilter(int connectionId, int deviceAddress, int tableHandle)
    {
        return _DFCTableRemoveFilter(connectionId, deviceAddress, tableHandle);
    }
    int TableRemoveFilter(int connectionId, int deviceAddress, int tableHandle)
    {
        return DFCTableRemoveFilter(connectionId, deviceAddress, tableHandle);
    }

    typedef int (__stdcall *__DFCTableGetRowCount)(int, int, int, int *, int *, int *);
    __DFCTableGetRowCount _DFCTableGetRowCount;
    int DFCTableGetRowCount(int connectionId, int deviceAddress, int tableHandle, int *currentCount, int *unsortedCount, int *deletedCount)
    {
        return _DFCTableGetRowCount(connectionId, deviceAddress, tableHandle, currentCount, unsortedCount, deletedCount);
    }
    int TableGetRowCount(int connectionId, int deviceAddress, int tableHandle, int *currentCount, int *unsortedCount, int *deletedCount)
    {
        return DFCTableGetRowCount(connectionId, deviceAddress, tableHandle, currentCount, unsortedCount, deletedCount);
    }
    
    typedef int (__stdcall *__DFCTableGetCurrentRow)(int, int, int, int *);
    __DFCTableGetCurrentRow _DFCTableGetCurrentRow;
    int DFCTableGetCurrentRow(int connectionId, int deviceAddress, int tableHandle, int *currentRow)
    {
        return _DFCTableGetCurrentRow(connectionId, deviceAddress, tableHandle, currentRow);
    }
    int TableGetCurrentRow(int connectionId, int deviceAddress, int tableHandle, int *currentRow)
    {
        return DFCTableGetCurrentRow(connectionId, deviceAddress, tableHandle, currentRow);
    }
    
    typedef int (__stdcall *__DFCTableSetCurrentRow)(int, int, int, int, int);
    __DFCTableSetCurrentRow _DFCTableSetCurrentRow;
    int DFCTableSetCurrentRow(int connectionId, int deviceAddress, int tableHandle, int offset, int direction)
    {
        return _DFCTableSetCurrentRow(connectionId, deviceAddress, tableHandle, offset, direction);
    }
    int TableSetCurrentRow(int connectionId, int deviceAddress, int tableHandle, int offset, int direction)
    {
        return DFCTableSetCurrentRow(connectionId, deviceAddress, tableHandle, offset, direction);
    }

    typedef int (__stdcall *__DFCTableSetCurrentRowData)(int, int, int, int, char, const char *, int);
    __DFCTableSetCurrentRowData _DFCTableSetCurrentRowData;
    int DFCTableSetCurrentRowData(int connectionId, int deviceAddress, int tableHandle, int flags, char separator, const char *data, int dataSize)
    {
        return _DFCTableSetCurrentRowData(connectionId, deviceAddress, tableHandle, flags, separator, data, dataSize);
    }
    int TableSetCurrentRowData(int connectionId, int deviceAddress, int tableHandle, int flags, char separator, const char *data, int dataSize)
    {
        return DFCTableSetCurrentRowData(connectionId, deviceAddress, tableHandle, flags, separator, data, dataSize);
    }
    
    typedef int (__stdcall *__DFCTableSetCurrentColumnData)(int, int, int, int, const char *, const char *, int);
    __DFCTableSetCurrentColumnData _DFCTableSetCurrentColumnData;
    int DFCTableSetCurrentColumnData(int connectionId, int deviceAddress, int tableHandle, int flags, const char *name, const char *data, int dataSize)
    {
        return _DFCTableSetCurrentColumnData(connectionId, deviceAddress, tableHandle, flags, name, data, dataSize);
    }
    int TableSetCurrentColumnData(int connectionId, int deviceAddress, int tableHandle, int flags, const char *name, const char *data, int dataSize)
    {
        return DFCTableSetCurrentColumnData(connectionId, deviceAddress, tableHandle, flags, name, data, dataSize);
    }

    typedef int (__stdcall *__DFCTableSetAllRowsToColumnData)(int, int, int, int, const char *, const char *, int);
    __DFCTableSetAllRowsToColumnData _DFCTableSetAllRowsToColumnData;
    int DFCTableSetAllRowsToColumnData(int connectionId, int deviceAddress, int tableHandle, int flags, const char *name, const char *data, int dataSize)
    {
        return _DFCTableSetAllRowsToColumnData(connectionId, deviceAddress, tableHandle, flags, name, data, dataSize);
    }
    int TableSetAllRowsToColumnData(int connectionId, int deviceAddress, int tableHandle, int flags, const char *name, const char *data, int dataSize)
    {
        return DFCTableSetAllRowsToColumnData(connectionId, deviceAddress, tableHandle, flags, name, data, dataSize);
    }

    typedef int (__stdcall *__DFCTableGetCurrentRowData)(int, int, int, int, char, char *, int *);
    __DFCTableGetCurrentRowData _DFCTableGetCurrentRowData;
    int DFCTableGetCurrentRowData(int connectionId, int deviceAddress, int tableHandle, int flags, char separator, char *data, int *dataSize)
    {
        return _DFCTableGetCurrentRowData(connectionId, deviceAddress, tableHandle, flags, separator, data, dataSize);
    }
    int TableGetCurrentRowData(int connectionId, int deviceAddress, int tableHandle, int flags, char separator, char *data, int *dataSize)
    {
        return DFCTableGetCurrentRowData(connectionId, deviceAddress, tableHandle, flags, separator, data, dataSize);
    }
    
    typedef int (__stdcall *__DFCTableGetCurrentColumnData)(int, int, int, int, const char *, char *, int *);
    __DFCTableGetCurrentColumnData _DFCTableGetCurrentColumnData;
    int DFCTableGetCurrentColumnData(int connectionId, int deviceAddress, int tableHandle, int flags, const char *name, char *data, int *dataSize)
    {
        return _DFCTableGetCurrentColumnData(connectionId, deviceAddress, tableHandle, flags, name, data, dataSize);
    }
    int TableGetCurrentColumnData(int connectionId, int deviceAddress, int tableHandle, int flags, const char *name, char *data, int *dataSize)
    {
        return DFCTableGetCurrentColumnData(connectionId, deviceAddress, tableHandle, flags, name, data, dataSize);
    }

    typedef int (__stdcall *__DFCTableAppendRowData)(int, int, int, int, char, const char *, int );
    __DFCTableAppendRowData _DFCTableAppendRowData;
    int DFCTableAppendRowData(int connectionId, int deviceAddress, int tableHandle, int flags, char separator, const char *data, int dataSize)
    {
        return _DFCTableAppendRowData(connectionId, deviceAddress, tableHandle, flags, separator, data, dataSize);
    }
    int TableAppendRowData(int connectionId, int deviceAddress, int tableHandle, int flags, char separator, const char *data, int dataSize)
    {
        return DFCTableAppendRowData(connectionId, deviceAddress, tableHandle, flags, separator, data, dataSize);
    }
    
    typedef int (__stdcall *__DFCTableDeleteCurrentRow)(int, int, int, int);
    __DFCTableDeleteCurrentRow _DFCTableDeleteCurrentRow;
    int DFCTableDeleteCurrentRow(int connectionId, int deviceAddress, int tableHandle, int flags)
    {
        return _DFCTableDeleteCurrentRow(connectionId, deviceAddress, tableHandle, flags);
    }
    int TableDeleteCurrentRow(int connectionId, int deviceAddress, int tableHandle, int flags)
    {
        return DFCTableDeleteCurrentRow(connectionId, deviceAddress, tableHandle, flags);
    }

    typedef int (__stdcall *__DFCTableDeleteAvailableRows)(int, int, int, int);
    __DFCTableDeleteAvailableRows _DFCTableDeleteAvailableRows;
    int DFCTableDeleteAvailableRows(int connectionId, int deviceAddress, int tableHandle, int flags)
    {
        return _DFCTableDeleteAvailableRows(connectionId, deviceAddress, tableHandle, flags);
    }
    int TableDeleteAvailableRows(int connectionId, int deviceAddress, int tableHandle, int flags)
    {
        return DFCTableDeleteAvailableRows(connectionId, deviceAddress, tableHandle, flags);
    }


    // ########################################################
    // ############## FUNKTIONEN FUER DATEN ###################
    // ########################################################

    typedef int (__stdcall *__DFCComClearData)(int, int);
    __DFCComClearData _DFCComClearData;
    int DFCComClearData(int channelID,int deviceID)
    {
        return _DFCComClearData(channelID, deviceID);
    }
    int ComClearData(int channelID,int deviceID)
    {
        return DFCComClearData(channelID, deviceID);
    }

    typedef int (__stdcall *__DFCComCollectData)(int, int, int*);
    __DFCComCollectData _DFCComCollectData;
    int DFCComCollectData(int channelID, int deviceID, int *piError)
    {
        return _DFCComCollectData(channelID, deviceID, piError);
    }
    int ComCollectData(int channelID, int deviceID, int *piError)
    {
        return DFCComCollectData(channelID, deviceID, piError);
    }

    typedef int (__stdcall *__DFCComGetDatensatz)(int, unsigned char*, int*, int*);
    __DFCComGetDatensatz _DFCComGetDatensatz;
    int DFCComGetDatensatz(int channelID, unsigned char *buf, int *bufLength, int *piError)
    {
        return _DFCComGetDatensatz(channelID, buf, bufLength, piError);
    }
    int ComGetDatensatz(int channelID, unsigned char *buf, int *bufLength, int *piError)
    {
        return DFCComGetDatensatz(channelID, buf, bufLength, piError);
    }

    typedef int (__stdcall *__DFCLoadDatensatzbeschreibung)(int, int, int*);
    __DFCLoadDatensatzbeschreibung _DFCLoadDatensatzbeschreibung;
    int DFCLoadDatensatzbeschreibung(int channelID, int deviceID, int *piError)
    {
        return _DFCLoadDatensatzbeschreibung(channelID, deviceID, piError);
    }
    int LoadDatensatzbeschreibung(int channelID, int deviceID, int *piError)
    {
        return DFCLoadDatensatzbeschreibung(channelID, deviceID, piError);
    }

    typedef int (__stdcall *__DFCDatBCnt)(int);
    __DFCDatBCnt _DFCDatBCnt;
    int DFCDatBCnt(int channelID)
    {
        return _DFCDatBCnt(channelID);
    }
    int DatBCnt(int channelID)
    {
        return DFCDatBCnt(channelID);
    }

    typedef int (__stdcall *__DFCDatBDatensatz)(int, int, char*, int*);
    __DFCDatBDatensatz _DFCDatBDatensatz;
    int DFCDatBDatensatz(int channelID, int datBNum, char *name, int *fieldCount)
    {
        return _DFCDatBDatensatz(channelID, datBNum, name, fieldCount);
    }
    int DatBDatensatz(int channelID, int datBNum, char *name, int *fieldCount)
    {
        return DFCDatBDatensatz(channelID, datBNum, name, fieldCount);
    }

    typedef int (__stdcall *__DFCDatBFeld)(int, int, int, char*, int*, int*);
    __DFCDatBFeld _DFCDatBFeld;
    int DFCDatBFeld(int channelID, int datBNum, int datBFeldNum, char *name, int *type, int *size)
    {
        return _DFCDatBFeld(channelID, datBNum, datBFeldNum, name, type, size);
    }
    int DatBFeld(int channelID, int datBNum, int datBFeldNum, char *name, int *type, int *size)
    {
        return DFCDatBFeld(channelID, datBNum, datBFeldNum, name, type, size);
    }

    typedef int (__stdcall *__DFCReadRecord)(int, int, unsigned char*, int*, int*);
    __DFCReadRecord _DFCReadRecord;
    int DFCReadRecord(int channelID, int deviceID, unsigned char *buf, int *bufLength, int *piError)
    {
        return _DFCReadRecord(channelID, deviceID, buf, bufLength, piError);
    }
    int ReadRecord(int channelID, int deviceID, unsigned char *buf, int *bufLength, int *piError)
    {
        return DFCReadRecord(channelID, deviceID, buf, bufLength, piError);
    }

    typedef int (__stdcall *__DFCQuitRecord)(int, int, int*);
    __DFCQuitRecord _DFCQuitRecord;
    int DFCQuitRecord(int channelID, int deviceID, int *piError)
    {
        return _DFCQuitRecord(channelID, deviceID, piError);
    }
    int QuitRecord(int channelID, int deviceID, int *piError)
    {
        return DFCQuitRecord(channelID, deviceID, piError);
    }

    typedef int (__stdcall *__DFCRestoreRecords)(int, int, int*);
    __DFCRestoreRecords _DFCRestoreRecords;
    int DFCRestoreRecords(int channelID, int deviceID, int *piError)
    {
        return _DFCRestoreRecords(channelID, deviceID, piError);
    }
    int RestoreRecords(int channelID, int deviceID, int *piError)
    {
        return DFCRestoreRecords(channelID, deviceID, piError);
    }


    // ########################################################
    // ############## FUNKTIONEN FUER LISTEN ###################
    // ########################################################

    typedef int (__stdcall *__DFCMakeListe)(int, int, int, int, const unsigned char*, int);
    __DFCMakeListe _DFCMakeListe;
    int DFCMakeListe(int channelID, int listBNum, int iLineCount, int iListSize, const unsigned char *pucBuf, int iListMemSizeNum = 0)
    {
        return _DFCMakeListe(channelID, listBNum, iLineCount, iListSize, pucBuf, iListMemSizeNum);
    }
    int MakeListe(int channelID, int listBNum, int iLineCount, int iListSize, const unsigned char *pucBuf, int iListMemSizeNum = 0)
    {
        return DFCMakeListe(channelID, listBNum, iLineCount, iListSize, pucBuf, iListMemSizeNum);
    }

    typedef int (__stdcall *__DFCLoadListen)(int, int, int*);
    __DFCLoadListen _DFCLoadListen;
    int DFCLoadListen(int channelID, int deviceID, int *piError)
    {
        return _DFCLoadListen(channelID, deviceID, piError);
    }
    int LoadListen(int channelID, int deviceID, int *piError)
    {
        return DFCLoadListen(channelID, deviceID, piError);
    }

    typedef void (__stdcall *__DFCClrListenBuffer)(int);
    __DFCClrListenBuffer _DFCClrListenBuffer;
    void DFCClrListenBuffer(int channelID)
    {
        _DFCClrListenBuffer(channelID);
    }
    void ClrListenBuffer(int channelID)
    {
        DFCClrListenBuffer(channelID);
    }

    typedef int (__stdcall *__DFCLoadListenbeschreibung)(int, int, int*);
    __DFCLoadListenbeschreibung _DFCLoadListenbeschreibung;
    int DFCLoadListenbeschreibung(int channelID, int deviceID, int *piError)
    {
        return _DFCLoadListenbeschreibung(channelID, deviceID, piError);
    }
    int LoadListenbeschreibung(int channelID, int deviceID, int *piError)
    {
        return DFCLoadListenbeschreibung(channelID, deviceID, piError);
    }

    typedef int (__stdcall *__DFCListBCnt)(int);
    __DFCListBCnt _DFCListBCnt;
    int DFCListBCnt(int channelID)
    {
        return _DFCListBCnt(channelID);
    }
    int ListBCnt(int channelID)
    {
        return DFCListBCnt(channelID);
    }

    typedef int (__stdcall *__DFCListBDatensatz)(int, int, char*, int*, int*);
    __DFCListBDatensatz _DFCListBDatensatz;
    int DFCListBDatensatz(int channelID, int listBNum, char *name, int *fieldCount, int *varList)
    {
        return _DFCListBDatensatz(channelID, listBNum, name, fieldCount, varList);
    }
    int ListBDatensatz(int channelID, int listBNum, char *name, int *fieldCount, int *varList)
    {
        return DFCListBDatensatz(channelID, listBNum, name, fieldCount, varList);
    }

    typedef int (__stdcall *__DFCListBFeld)(int, int, int, char*, int*, int*);
    __DFCListBFeld _DFCListBFeld;
    int DFCListBFeld(int channelID, int listBNum, int listBFeldNum, char *name, int *type, int *size)
    {
        return _DFCListBFeld(channelID, listBNum, listBFeldNum, name, type, size);
    }
    int ListBFeld(int channelID, int listBNum, int listBFeldNum, char *name, int *type, int *size)
    {
        return DFCListBFeld(channelID, listBNum, listBFeldNum, name, type, size);
    }


    // ########################################################
    // ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN #########
    // ########################################################

    typedef int (__stdcall *__DFCMakeEntranceList)(int, int, int, int, const unsigned char*, int*);
    __DFCMakeEntranceList _DFCMakeEntranceList;
    int DFCMakeEntranceList(int channelID, int iListNum, int iLineCount, int iListSize, const unsigned char *pucBuf, int *piError)
    {
        return _DFCMakeEntranceList(channelID, iListNum, iLineCount, iListSize, pucBuf, piError);
    }
    int MakeEntranceList(int channelID, int iListNum, int iLineCount, int iListSize, const unsigned char *pucBuf, int *piError)
    {
        return DFCMakeEntranceList(channelID, iListNum, iLineCount, iListSize, pucBuf, piError);
    }

    typedef int (__stdcall *__DFCLoadEntranceList)(int, int, int, int*);
    __DFCLoadEntranceList _DFCLoadEntranceList;
    int DFCLoadEntranceList(int channelID, int deviceID, int iListNum, int *piError)
    {
        return _DFCLoadEntranceList(channelID, deviceID, iListNum, piError);
    }
    int LoadEntranceList(int channelID, int deviceID, int iListNum, int *piError)
    {
        return DFCLoadEntranceList(channelID, deviceID, iListNum, piError);
    }

    typedef void (__stdcall *__DFCClearEntranceListBuffer)(int, int);
    __DFCClearEntranceListBuffer _DFCClearEntranceListBuffer;
    void DFCClearEntranceListBuffer(int channelID, int iListNum)
    {
        _DFCClearEntranceListBuffer(channelID, iListNum);
    }
    void ClearEntranceListBuffer(int channelID, int iListNum)
    {
        DFCClearEntranceListBuffer(channelID, iListNum);
    }


    // ########################################################
    // ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN 2 #######
    // ########################################################

    typedef int (__stdcall *__DFCMakeEntrance2List)(int, int, int, int, const unsigned char*, int*);
    __DFCMakeEntrance2List _DFCMakeEntrance2List;
    int DFCMakeEntrance2List(int channelID, int iListNum, int iLineCount, int iListSize, const unsigned char *pucBuf, int *piError)
    {
        return _DFCMakeEntrance2List(channelID, iListNum, iLineCount, iListSize, pucBuf, piError);
    }
    int MakeEntrance2List(int channelID, int iListNum, int iLineCount, int iListSize, const unsigned char *pucBuf, int *piError)
    {
        return DFCMakeEntrance2List(channelID, iListNum, iLineCount, iListSize, pucBuf, piError);
    }

    typedef int (__stdcall *__DFCLoadEntrance2List)(int, int, int, int*);
    __DFCLoadEntrance2List _DFCLoadEntrance2List;
    int DFCLoadEntrance2List(int channelID, int deviceID, int iListNum, int *piError)
    {
        return _DFCLoadEntrance2List(channelID, deviceID, iListNum, piError);
    }
    int LoadEntrance2List(int channelID, int deviceID, int iListNum, int *piError)
    {
        return DFCLoadEntrance2List(channelID, deviceID, iListNum, piError);
    }

    typedef void (__stdcall *__DFCClearEntrance2ListBuffer)(int, int);
    __DFCClearEntrance2ListBuffer _DFCClearEntrance2ListBuffer;
    void DFCClearEntrance2ListBuffer(int channelID, int iListNum)
    {
        _DFCClearEntrance2ListBuffer(channelID, iListNum);
    }
    void ClearEntrance2ListBuffer(int channelID, int iListNum)
    {
        DFCClearEntrance2ListBuffer(channelID, iListNum);
    }

    typedef int (__stdcall *__DFCEntrance2Identification)(int, int, const char*, const char*, const char*, char*, int*, int*);
    __DFCEntrance2Identification _DFCEntrance2Identification;
    int DFCEntrance2Identification(int channelID, int deviceID, const char *TM, const char *Id, const char *Pin, char *status, int *len, int *piError)
    {
        return _DFCEntrance2Identification(channelID, deviceID, TM, Id, Pin, status, len, piError);
    }
    int Entrance2Identification(int channelID, int deviceID, const char *TM, const char *Id, const char *Pin, char *status, int *len, int *piError)
    {
        return DFCEntrance2Identification(channelID, deviceID, TM, Id, Pin, status, len, piError);
    }

    typedef int (__stdcall *__DFCEntrance2OnlineAction)(int, int, const char*, int, int, int, int*);
    __DFCEntrance2OnlineAction _DFCEntrance2OnlineAction;
    int DFCEntrance2OnlineAction(int channelID, int deviceID, const char *TM, int mask, int type, int duration, int *piError)
    {
        return _DFCEntrance2OnlineAction(channelID, deviceID, TM, mask, type, duration, piError);
    }
    int Entrance2OnlineAction(int channelID, int deviceID, const char *TM, int mask, int type, int duration, int *piError)
    {
        return DFCEntrance2OnlineAction(channelID, deviceID, TM, mask, type, duration, piError);
    }

   typedef int (__stdcall *__DFCAccessControlIdentification)(int, int, int, const char*, const char*, const char*, char*, int*, int*);
    __DFCAccessControlIdentification _DFCAccessControlIdentification;
    int DFCAccessControlIdentification(int channelID, int deviceID, int masterId, const char *TM, const char *Id, const char *Pin, char *status, int *len, int *piError)
    {
        return _DFCAccessControlIdentification(channelID, deviceID, masterId, TM, Id, Pin, status, len, piError);
    }
    int AccessControlIdentification(int channelID, int deviceID, int masterId, const char *TM, const char *Id, const char *Pin, char *status, int *len, int *piError)
    {
        return DFCAccessControlIdentification(channelID, deviceID, masterId, TM, Id, Pin, status, len, piError);
    }

    typedef int (__stdcall *__DFCAccessControlOnlineAction)(int, int, int, const char*, int, int, int, int*);
    __DFCAccessControlOnlineAction _DFCAccessControlOnlineAction;
    int DFCAccessControlOnlineAction(int channelID, int deviceID, int masterId, const char *TM, int mask, int type, int duration, int *piError)
    {
        return _DFCAccessControlOnlineAction(channelID, deviceID, masterId, TM, mask, type, duration, piError);
    }
    int AccessControlOnlineAction(int channelID, int deviceID, int masterId, const char *TM, int mask, int type, int duration, int *piError)
    {
        return DFCAccessControlOnlineAction(channelID, deviceID, masterId, TM, mask, type, duration, piError);
    }

    typedef int (__stdcall *__DFCAccessControlKnobCommand)( int, int, int, int, int, const char *, char *, int *, int * );
    __DFCAccessControlKnobCommand _DFCAccessControlKnobCommand;
    int DFCAccessControlKnobCommand( int channelId, int deviceId, int knobCommand, int masterId, int knobId, const char *params, char *data, int *sizeData, int *piError )
    {
        return _DFCAccessControlKnobCommand( channelId, deviceId, knobCommand, masterId, knobId, params, data, sizeData, piError );
    }
    int AccessControlKnobCommand( int channelId, int deviceId, int knobCommand, int masterId, int knobId, const char *params, char *data, int *sizeData, int *piError )
    {
        return DFCAccessControlKnobCommand( channelId, deviceId, knobCommand, masterId, knobId, params, data, sizeData, piError );
    }

    // ########################################################
    // ####### FUNKTIONEN FUER FINGERPRINT ####################
    // ########################################################
    typedef int (__stdcall *__DFCFingerprintAppendRecord)(int, int, int, const unsigned char*, int, int*);
    __DFCFingerprintAppendRecord _DFCFingerprintAppendRecord;
    int DFCFingerprintAppendRecord(int channelID, int deviceID, int type, const unsigned char *fingertemplate, int size, int *piError)
    {
        return _DFCFingerprintAppendRecord(channelID, deviceID, type, fingertemplate, size, piError);
    }
    int FingerprintAppendRecord(int channelID, int deviceID, int type, const unsigned char *fingertemplate, int size, int *piError)
    {
        return DFCFingerprintAppendRecord(channelID, deviceID, type, fingertemplate, size, piError);
    }

    typedef int (__stdcall *__DFCFingerprintGetRecord)(int, int, int, int, int, unsigned char*, int*, int*);
    __DFCFingerprintGetRecord _DFCFingerprintGetRecord;
    int DFCFingerprintGetRecord(int channelID, int deviceID, int type, int pid, int fid, unsigned char *fingertemplate, int *size, int *piError)
    {
        return _DFCFingerprintGetRecord(channelID, deviceID, type, pid, fid, fingertemplate, size, piError);
    }
    int FingerprintGetRecord(int channelID, int deviceID, int type, int pid, int fid, unsigned char *fingertemplate, int *size, int *piError)
    {
        return DFCFingerprintGetRecord(channelID, deviceID, type, pid, fid, fingertemplate, size, piError);
    }

    typedef int (__stdcall *__DFCFingerprintDeleteRecord)(int, int, int, int, int*);
    __DFCFingerprintDeleteRecord _DFCFingerprintDeleteRecord;
    int DFCFingerprintDeleteRecord(int channelID, int deviceID, int pid, int fid, int *piError)
    {
        return _DFCFingerprintDeleteRecord(channelID, deviceID, pid, fid, piError);
    }
    int FingerprintDeleteRecord(int channelID, int deviceID, int pid, int fid, int *piError)
    {
        return DFCFingerprintDeleteRecord(channelID, deviceID, pid, fid, piError);
    }

    typedef int (__stdcall *__DFCFingerprintList)(int, int, int, int*);
    __DFCFingerprintList _DFCFingerprintList;
    int DFCFingerprintList(int channelID, int deviceID, int flags, int *piError)
    {
        return _DFCFingerprintList(channelID, deviceID, flags, piError);
    }
    int FingerprintList(int channelID, int deviceID, int flags, int *piError)
    {
        return DFCFingerprintList(channelID, deviceID, flags, piError);
    }

    typedef int (__stdcall *__DFCFingerprintBackup)(int, int, const char*, int, int*);
    __DFCFingerprintBackup _DFCFingerprintBackup;
    int DFCFingerprintBackup(int channelID, int deviceID, const char *filePath, int flags, int *piError)
    {
        return _DFCFingerprintBackup(channelID, deviceID, filePath, flags, piError);
    }
    int FingerprintBackup(int channelID, int deviceID, const char *filePath, int flags, int *piError)
    {
        return DFCFingerprintBackup(channelID, deviceID, filePath, flags, piError);
    }

    typedef int (__stdcall *__DFCFingerprintRestore)(int, int, const char*, int, int*);
    __DFCFingerprintRestore _DFCFingerprintRestore;
    int DFCFingerprintRestore(int channelID, int deviceID, const char *filePath, int flags, int *piError)
    {
        return _DFCFingerprintRestore(channelID, deviceID, filePath, flags, piError);
    }
    int FingerprintRestore(int channelID, int deviceID, const char *filePath, int flags, int *piError)
    {
        return DFCFingerprintRestore(channelID, deviceID, filePath, flags, piError);
    }


    // ########################################################
    // ####### FUNKTIONEN FUER TIMEBOYLISTEN ##################
    // ########################################################

    typedef int (__stdcall *__DFCMakeTimeboyList)(int, int, int, int, int, const unsigned char*, int, int*);
    __DFCMakeTimeboyList _DFCMakeTimeboyList;
    int DFCMakeTimeboyList(int channelID, int iGroupID, int iListNum, int iLineCount, int iListSize, const unsigned char *pucBuf, int flags, int *piError)
    {
        return _DFCMakeTimeboyList(channelID, iGroupID, iListNum, iLineCount, iListSize, pucBuf, flags, piError);
    }
    int MakeTimeboyList(int channelID, int iGroupID, int iListNum, int iLineCount, int iListSize, const unsigned char *pucBuf, int flags, int *piError)
    {
        return DFCMakeTimeboyList(channelID, iGroupID, iListNum, iLineCount, iListSize, pucBuf, flags, piError);
    }

    typedef int (__stdcall *__DFCLoadTimeboyList)(int, int, int, int, int, int*);
    __DFCLoadTimeboyList _DFCLoadTimeboyList;
    int DFCLoadTimeboyList(int channelID, int deviceID, int iGroupID, int iListNum, int reserve, int *piError)
    {
        return _DFCLoadTimeboyList(channelID, deviceID, iGroupID, iListNum, reserve, piError);
    }
    int LoadTimeboyList(int channelID, int deviceID, int iGroupID, int iListNum, int reserve, int *piError)
    {
        return DFCLoadTimeboyList(channelID, deviceID, iGroupID, iListNum, reserve, piError);
    }

    typedef void (__stdcall *__DFCClearTimeboyListBuffer)(int, int, int);
    __DFCClearTimeboyListBuffer _DFCClearTimeboyListBuffer;
    void DFCClearTimeboyListBuffer(int channelID, int iGroupID, int iListNum)
    {
        _DFCClearTimeboyListBuffer(channelID, iGroupID, iListNum);
    }
    void ClearTimeboyListBuffer(int channelID, int iGroupID, int iListNum)
    {
        DFCClearTimeboyListBuffer(channelID, iGroupID, iListNum);
    }


    // ########################################################
    // ######### FUNKTIONEN FUER AKTIVE VERBINDUNG ############
    // ########################################################

    typedef int (__stdcall *__DFCStartActiveConnection)(const char*, int, int, int, int, int, int, int, int*);
    __DFCStartActiveConnection _DFCStartActiveConnection;
    int DFCStartActiveConnection(const char *ip, int port, int deviceID, int timeout, int aliveTO, int maxConnections, int infoFlags, int reserved, int *piError)
    {
        return _DFCStartActiveConnection(ip, port, deviceID, timeout, aliveTO, maxConnections, infoFlags, reserved, piError);
    }
    int StartActiveConnection(const char *ip, int port, int deviceID, int timeout, int aliveTO, int maxConnections, int infoFlags, int reserved, int *piError)
    {
        return DFCStartActiveConnection(ip, port, deviceID, timeout, aliveTO, maxConnections, infoFlags, reserved, piError);
    }

    typedef int (__stdcall *__DFCStopActiveConnection)(int*);
    __DFCStopActiveConnection _DFCStopActiveConnection;
    int DFCStopActiveConnection(int *piError)
    {
        return _DFCStopActiveConnection(piError);
    }
    int StopActiveConnection(int *piError)
    {
        return DFCStopActiveConnection(piError);
    }

    typedef int (__stdcall *__DFCGetFirstActiveChannelID)();
    __DFCGetFirstActiveChannelID _DFCGetFirstActiveChannelID;
    int DFCGetFirstActiveChannelID()
    {
        return _DFCGetFirstActiveChannelID();
    }
    int GetFirstActiveChannelID()
    {
        return DFCGetFirstActiveChannelID();
    }

    typedef int (__stdcall *__DFCGetNextActiveChannelID)(int);
    __DFCGetNextActiveChannelID _DFCGetNextActiveChannelID;
    int DFCGetNextActiveChannelID(int prev)
    {
        return _DFCGetNextActiveChannelID(prev);
    }
    int GetNextActiveChannelID(int prev)
    {
        return DFCGetNextActiveChannelID(prev);
    }

    typedef int (__stdcall *__DFCGetInfoActiveChannel)(int, char*, int*);
    __DFCGetInfoActiveChannel _DFCGetInfoActiveChannel;
    int DFCGetInfoActiveChannel(int channelID, char *infoString, int *infoStringLength)
    {
        return _DFCGetInfoActiveChannel(channelID, infoString, infoStringLength);
    }
    int GetInfoActiveChannel(int channelID, char *infoString, int *infoStringLength)
    {
        return DFCGetInfoActiveChannel(channelID, infoString, infoStringLength);
    }

    typedef int (__stdcall *__DFCSetRecordAvailable)(int);
    __DFCSetRecordAvailable _DFCSetRecordAvailable;
    int DFCSetRecordAvailable(int enable)
    {
        return _DFCSetRecordAvailable(enable);
    }
    int SetRecordAvailable(int enable)
    {
        return DFCSetRecordAvailable(enable);
    }

    typedef int (__stdcall *__DFCRecordAvailable)(int*, int*, char*, int*, int*);
    __DFCRecordAvailable _DFCRecordAvailable;
    int DFCRecordAvailable(int *channelID, int *deviceID, char *infoString, int *infoStringLength, int *piError)
    {
        return _DFCRecordAvailable(channelID, deviceID, infoString, infoStringLength, piError);
    }
    int RecordAvailable(int *channelID, int *deviceID, char *infoString, int *infoStringLength, int *piError)
    {
        return DFCRecordAvailable(channelID, deviceID, infoString, infoStringLength, piError);
    }

    typedef int (__stdcall *__DFCBindDeviceToChannel)(int, int, int, const char*, int*);
    __DFCBindDeviceToChannel _DFCBindDeviceToChannel;
    int DFCBindDeviceToChannel(int channelID, int deviceType, int deviceSerial, const char *deviceIp, int *piError)
    {
        return _DFCBindDeviceToChannel(channelID, deviceType, deviceSerial, deviceIp, piError);
    }
    int BindDeviceToChannel(int channelID, int deviceType, int deviceSerial, const char *deviceIp, int *piError)
    {
        return DFCBindDeviceToChannel(channelID, deviceType, deviceSerial, deviceIp, piError);
    }


    // ########################################################
    // ############# ** PRIVATE FUNKTIONEN ** #################
    // ########################################################

    // Funktionen sind nicht dokumentiert.
    // Sie werden von AESetup oder Talk verwendet.
    // Unterliegen evntl. Aenderungen.
    /*
    extern "C" HRESULT PASCAL EXPORT CALLBACK DllGetVersion(DLLVERSIONINFO *pdvi);
    */
#ifdef WITH_DLLGETVERSION
    typedef HRESULT (__stdcall *__DllGetVersion)(_DLLVERSIONINFO*);
    __DllGetVersion _DllGetVersion;
    int DllGetVersion(_DLLVERSIONINFO *vi)
    {
        return _DllGetVersion(vi);
    }
    int GetVersion(_DLLVERSIONINFO *vi)
    {
        return DllGetVersion(vi);
    }
#endif

    typedef int (__stdcall *__DFCComUseHandle)(int, Qt::HANDLE);
    typedef int (__stdcall *__DFCComReleaseHandle)(int);

    typedef int (__stdcall *__DFCComVersion)(int, int, int*);
    __DFCComVersion _DFCComVersion;
    int DFCComVersion(int channelID, int deviceID, int *piError)
    {
        return _DFCComVersion(channelID, deviceID, piError);
    }
    int ComVersion(int channelID, int deviceID, int *piError)
    {
        return DFCComVersion(channelID, deviceID, piError);
    }

    typedef int (__stdcall *__DFCCheckData)(int, int, int*);
    __DFCCheckData _DFCCheckData;
    int DFCCheckData(int channelID, int deviceID, int *piError)
    {
        return _DFCCheckData(channelID, deviceID, piError);
    }
    int CheckData(int channelID, int deviceID, int *piError)
    {
        return DFCCheckData(channelID, deviceID, piError);
    }

    typedef void* (__stdcall *__DFCGetClass)(int, int);
    __DFCGetClass _DFCGetClass;
    void *DFCGetClass(int channelID, int id)
    {
        return _DFCGetClass(channelID, id);
    }
    void *GetClass(int channelID, int id)
    {
        return DFCGetClass(channelID, id);
    }

#if defined(_DEBUG) && defined(DATAFOX_STUDIO_IV)
    // ########################################################
    // ########### FUNKTIONEN FUER PRODUKTION #################
    // ########################################################

    typedef int (__stdcall *__DFPReadHardwareInfo)(int, int, const char*, int);
    __DFPReadHardwareInfo _DFPReadHardwareInfo;
    int DFPReadHardwareInfo(int channelID, int terminalID, const char *fileName, int flags)
    {
        return _DFPReadHardwareInfo(channelID, terminalID, fileName, flags);
    }
    int ReadHardwareInfo(int channelID, int deviceID, const char *fileName, int flags)
    {
        return DFPReadHardwareInfo(channelID, deviceID, fileName, flags);
    }

    typedef int (__stdcall *__DFPWriteHardwareInfo)(int, int, const char*, int);
    __DFPWriteHardwareInfo _DFPWriteHardwareInfo;
    int DFPWriteHardwareInfo(int channelID, int terminalID, const char *fileName, int flags)
    {
        return _DFPWriteHardwareInfo(channelID, terminalID, fileName, flags);
    }
    int WriteHardwareInfo(int channelID, int deviceID, const char *fileName, int flags)
    {
        return DFPWriteHardwareInfo(channelID, deviceID, fileName, flags);
    }

    typedef unsigned long (__stdcall *__DllGetProtectionKey)();
    __DllGetProtectionKey _DllGetProtectionKey;
    unsigned long DllGetProtectionKey()
    {
        return _DllGetProtectionKey();
    }
    unsigned long GetProtectionKey()
    {
        return DllGetProtectionKey();
    }

    typedef int (__stdcall *__DllUnlockProtection)(unsigned long);
    __DllUnlockProtection _DllUnlockProtection;
    int DllUnlockProtection(unsigned long key)
    {
        return _DllUnlockProtection(key);
    }
    int UnlockProtection(unsigned long key)
    {
        return DllUnlockProtection(key);
    }
#endif
};

#endif
