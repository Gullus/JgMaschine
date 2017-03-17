#include "DFComDLL.h"
#ifdef WITH_DLLGETVERSION
#include <shlwapi.h>
#endif

#if defined(_WIN64) || defined(__MINGW32__)
// Zu ladende Library
#ifdef __MINGW32__
#define DFCOMDLL_NAME "libDFCom.dll"
#else
#define DFCOMDLL_NAME "DFCom_x64.dll"
#endif
// Zu ladende Library im Release fuer Produktion
#define DFCOMPRODUCTION_NAME "DFComProduction_x64.dll"
// ########################################################
// ############ FUNKTIONEN FUER ALLGEMEIN #################
// ########################################################
#define DEF_DFCComInit                           "DFCComInit"
#define DEF_DFCComOpenSerial                     "DFCComOpenSerial"
#define DEF_DFCComOpen                           "DFCComOpen"
#define DEF_DFCComOpenSocket                     "DFCComOpenSocket"
#define DEF_DFCComOpenIV                         "DFCComOpenIV"
#define DEF_DFCComClose                          "DFCComClose"
#define DEF_DFCCheckAE                           "DFCCheckAE"
#define DEF_DFCCheckDevice                       "DFCCheckDevice"
#define DEF_DFCComSetTime                        "DFCComSetTime"
#define DEF_DFCComGetTime                        "DFCComGetTime"
#define DEF_DFCComSendMessage                    "DFCComSendMessage"
#define DEF_DFCComSendInfotext                   "DFCComSendInfotext"
#define DEF_DFCGetSeriennummer                   "DFCGetSeriennummer"
#define DEF_DFCSetLogOn                          "DFCSetLogOn"
#define DEF_DFCSetLogOff                         "DFCSetLogOff"
#define DEF_DFCSetCallBack                       "DFCSetCallBack"
#define DEF_DFCSetLogFileName                    "DFCSetLogFileName"
#define DEF_DFCGetErrorText                      "DFCGetErrorText"
#define DEF_DFCSetGlobVar                        "DFCSetGlobVar"
#define DEF_DFCGetGlobVar                        "DFCGetGlobVar"
#define DEF_DFCCloseRelay                        "DFCCloseRelay"
#define DEF_DFCGetRelayState                     "DFCGetRelayState"
#define DEF_DFCOpenRelay                         "DFCOpenRelay"
#define DEF_DFCGetDevicePollRetry                "DFCGetDevicePollRetry"
#define DEF_DFCGetComPort                        "DFCGetComPort"
#define DEF_DFCSetComPort                        "DFCSetComPort"
#define DEF_DFCWrite                             "DFCWrite"
#define DEF_DFCRead                              "DFCRead"
#define DEF_DFCUpload                            "DFCUpload"
#define DEF_DFCGetVersionFirmware                "DFCGetVersionFirmware"
#define DEF_DFCGetVersionFirmwareFromFile        "DFCGetVersionFirmwareFromFile"
#define DEF_DFCGetInfo                           "DFCGetInfo"
#define DEF_DFCOpenComServerMode                 "DFCOpenComServerMode"
#define DEF_DFCCloseComServerMode                "DFCCloseComServerMode"
#define DEF_DFCUploadModule                      "DFCUploadModule"
#define DEF_DFCIsChannelOpen                     "DFCIsChannelOpen"
#define DEF_DFCSetOptionFirmware                 "DFCSetOptionFirmware"
#define DEF_DFCGetOptionFirmware                 "DFCGetOptionFirmware"
#define DEF_DFCSystemReadRecord                  "DFCSystemReadRecord"
#define DEF_DFCSystemQuitRecord                  "DFCSystemQuitRecord"
#define DEF_DFCSystemRestoreRecords              "DFCSystemRestoreRecords"
#define DEF_DFCReset                             "DFCReset"
#define DEF_DFCSetFontType                       "DFCSetFontType"
#define DEF_DFCSetPassword                       "DFCSetPassword"
#define DEF_DFCGetPasswordKey                    "DFCGetPasswordKey"
#define DEF_DFCPressVirtualKey                   "DFCPressVirtualKey"
#define DEF_DFCGetFlashStatus                    "DFCGetFlashStatus"
#define DEF_DFCSetCommunicationPassword          "DFCSetCommunicationPassword"
#define DEF_DFCReadHardwareInfo                  "DFCReadHardwareInfo"
#define DEF_DFCFileUpload                        "DFCFileUpload"
#define DEF_DFCFileDownload                      "DFCFileDownload"
#define DEF_DFCRecordVolume                      "DFCRecordVolume"
#define DEF_DFCVersionLibrary                    "DFCVersionLibrary"
#define DEF_DFCCustomCommand					 "DFCCustomCommand"
// ########################################################
// ############## FUNKTIONEN FUER SETUP ###################
// ########################################################
#define DEF_DFCSetupLaden                        "DFCSetupLaden"
#define DEF_DFCDownload                          "DFCDownload"
#define DEF_DFCModifyStudioFile                  "DFCModifyStudioFile"
// ########################################################
// FUNKTIONEN FUER LISTEN, SOWIE ZUTRITTSLISTEN (VERSION 2)
// ########################################################
#define DEF_DFCOpenTable                         "DFCOpenTable"
#define DEF_DFCCloseTable                        "DFCCloseTable"
#define DEF_DFCSetFilter                         "DFCSetFilter"
#define DEF_DFCGetFilter                         "DFCGetFilter"
#define DEF_DFCClearFilter                       "DFCClearFilter"
#define DEF_DFCSkip                              "DFCSkip"
#define DEF_DFCSetField                          "DFCSetField"
#define DEF_DFCGetField                          "DFCGetField"
// ########################################################
// ############## FUNKTIONEN FUER DATEN ###################
// ########################################################
#define DEF_DFCComClearData                      "DFCComClearData"
#define DEF_DFCComCollectData                    "DFCComCollectData"
#define DEF_DFCComGetDatensatz                   "DFCComGetDatensatz"
#define DEF_DFCLoadDatensatzbeschreibung         "DFCLoadDatensatzbeschreibung"
#define DEF_DFCDatBCnt                           "DFCDatBCnt"
#define DEF_DFCDatBDatensatz                     "DFCDatBDatensatz"
#define DEF_DFCDatBFeld                          "DFCDatBFeld"
#define DEF_DFCReadRecord                        "DFCReadRecord"
#define DEF_DFCQuitRecord                        "DFCQuitRecord"
#define DEF_DFCRestoreRecords                    "DFCRestoreRecords"
// ########################################################
// ############## FUNKTIONEN FUER LISTEN ###################
// ########################################################
#define DEF_DFCMakeListe                         "DFCMakeListe"
#define DEF_DFCLoadListen                        "DFCLoadListen"
#define DEF_DFCClrListenBuffer                   "DFCClrListenBuffer"
#define DEF_DFCLoadListenbeschreibung            "DFCLoadListenbeschreibung"
#define DEF_DFCListBCnt                          "DFCListBCnt"
#define DEF_DFCListBDatensatz                    "DFCListBDatensatz"
#define DEF_DFCListBFeld                         "DFCListBFeld"
// ########################################################
// ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN #########
// ########################################################
#define DEF_DFCMakeEntranceList                  "DFCMakeEntranceList"
#define DEF_DFCLoadEntranceList                  "DFCLoadEntranceList"
#define DEF_DFCClearEntranceListBuffer           "DFCClearEntranceListBuffer"
// ########################################################
// ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN 2 #######
// ########################################################
#define DEF_DFCMakeEntrance2List                 "DFCMakeEntrance2List"
#define DEF_DFCLoadEntrance2List                 "DFCLoadEntrance2List"
#define DEF_DFCClearEntrance2ListBuffer          "DFCClearEntrance2ListBuffer"
#define DEF_DFCEntrance2Identification           "DFCEntrance2Identification"
#define DEF_DFCEntrance2OnlineAction             "DFCEntrance2OnlineAction"
#define DEF_DFCAccessControlIdentification       "DFCAccessControlIdentification"
#define DEF_DFCAccessControlOnlineAction         "DFCAccessControlOnlineAction"
#define DEF_DFCAccessControlKnobCommand          "DFCAccessControlKnobCommand"
// ########################################################
// ####### FUNKTIONEN FUER FINGERPRINT ####################
// ########################################################
#define DEF_DFCFingerprintAppendRecord           "DFCFingerprintAppendRecord"
#define DEF_DFCFingerprintGetRecord              "DFCFingerprintGetRecord"
#define DEF_DFCFingerprintDeleteRecord           "DFCFingerprintDeleteRecord"
#define DEF_DFCFingerprintList                   "DFCFingerprintList"
#define DEF_DFCFingerprintBackup                 "DFCFingerprintBackup"
#define DEF_DFCFingerprintRestore                "DFCFingerprintRestore"
// ########################################################
// ####### FUNKTIONEN FUER TIMEBOYLISTEN ##################
// ########################################################
#define DEF_DFCMakeTimeboyList                   "DFCMakeTimeboyList"
#define DEF_DFCLoadTimeboyList                   "DFCLoadTimeboyList"
#define DEF_DFCClearTimeboyListBuffer            "DFCClearTimeboyListBuffer"
// ########################################################
// ######### FUNKTIONEN FUER AKTIVE VERBINDUNG ############
// ########################################################
#define DEF_DFCStartActiveConnection             "DFCStartActiveConnection"
#define DEF_DFCStopActiveConnection              "DFCStopActiveConnection"
#define DEF_DFCGetFirstActiveChannelID           "DFCGetFirstActiveChannelID"
#define DEF_DFCGetNextActiveChannelID            "DFCGetNextActiveChannelID"
#define DEF_DFCGetInfoActiveChannel              "DFCGetInfoActiveChannel"
#define DEF_DFCSetRecordAvailable                "DFCSetRecordAvailable"
#define DEF_DFCRecordAvailable                   "DFCRecordAvailable"
#define DEF_DFCBindDeviceToChannel               "DFCBindDeviceToChannel"
// ########################################################
// ############# ** PRIVATE FUNKTIONEN ** #################
// ########################################################
#define DEF_DFCComVersion                        "DFCComVersion"
#define DEF_DFCCheckData                         "DFCCheckData"
#define DEF_DFCGetClass                          "DFCGetClass"
#elif _WIN32
// Zu ladende Library
#define DFCOMDLL_NAME "DFComDLL.dll"
// Zu ladende Library im Release fuer Produktion
#define DFCOMPRODUCTION_NAME "DFComProduction.dll"
// ########################################################
// ############ FUNKTIONEN FUER ALLGEMEIN #################
// ########################################################
#define DEF_DFCComInit                           "_DFCComInit@4"
#define DEF_DFCComOpenSerial                     "_DFCComOpenSerial@16"
#define DEF_DFCComOpen                           "_DFCComOpen@8"
#define DEF_DFCComOpenSocket                     "_DFCComOpenSocket@16"
#define DEF_DFCComOpenIV                         "_DFCComOpenIV@24"
#define DEF_DFCComClose                          "_DFCComClose@4"
#define DEF_DFCCheckAE                           "_DFCCheckAE@8"
#define DEF_DFCCheckDevice                       "_DFCCheckDevice@16"
#define DEF_DFCComSetTime                        "_DFCComSetTime@12"
#define DEF_DFCComGetTime                        "_DFCComGetTime@12"
#define DEF_DFCComSendMessage                    "_DFCComSendMessage@28"
#define DEF_DFCComSendInfotext                   "_DFCComSendInfotext@16"
#define DEF_DFCGetSeriennummer                   "_DFCGetSeriennummer@16"
#define DEF_DFCSetLogOn                          "_DFCSetLogOn@4"
#define DEF_DFCSetLogOff                         "_DFCSetLogOff@4"
#define DEF_DFCSetCallBack                       "_DFCSetCallBack@8"
#define DEF_DFCSetLogFileName                    "_DFCSetLogFileName@8"
#define DEF_DFCGetErrorText                      "_DFCGetErrorText@20"
#define DEF_DFCSetGlobVar                        "_DFCSetGlobVar@24"
#define DEF_DFCGetGlobVar                        "_DFCGetGlobVar@28"
#define DEF_DFCCloseRelay                        "_DFCCloseRelay@20"
#define DEF_DFCGetRelayState                     "_DFCGetRelayState@24"
#define DEF_DFCOpenRelay                         "_DFCOpenRelay@16"
#define DEF_DFCGetDevicePollRetry                "_DFCGetDevicePollRetry@4"
#define DEF_DFCGetComPort                        "_DFCGetComPort@4"
#define DEF_DFCSetComPort                        "_DFCSetComPort@20"
#define DEF_DFCWrite                             "_DFCWrite@20"
#define DEF_DFCRead                              "_DFCRead@20"
#define DEF_DFCUpload                            "_DFCUpload@16"
#define DEF_DFCGetVersionFirmware                "_DFCGetVersionFirmware@16"
#define DEF_DFCGetVersionFirmwareFromFile        "_DFCGetVersionFirmwareFromFile@16"
#define DEF_DFCGetInfo                           "_DFCGetInfo@28"
#define DEF_DFCOpenComServerMode                 "_DFCOpenComServerMode@40"
#define DEF_DFCCloseComServerMode                "_DFCCloseComServerMode@12"
#define DEF_DFCUploadModule                      "_DFCUploadModule@20"
#define DEF_DFCIsChannelOpen                     "_DFCIsChannelOpen@4"
#define DEF_DFCSetOptionFirmware                 "_DFCSetOptionFirmware@20"
#define DEF_DFCGetOptionFirmware                 "_DFCGetOptionFirmware@20"
#define DEF_DFCSystemReadRecord                  "_DFCSystemReadRecord@20"
#define DEF_DFCSystemQuitRecord                  "_DFCSystemQuitRecord@12"
#define DEF_DFCSystemRestoreRecords              "_DFCSystemRestoreRecords@12"
#define DEF_DFCReset                             "_DFCReset@16"
#define DEF_DFCSetFontType                       "_DFCSetFontType@16"
#define DEF_DFCSetPassword                       "_DFCSetPassword@12"
#define DEF_DFCGetPasswordKey                    "_DFCGetPasswordKey@20"
#define DEF_DFCPressVirtualKey                   "_DFCPressVirtualKey@20"
#define DEF_DFCGetFlashStatus                    "_DFCGetFlashStatus@16"
#define DEF_DFCSetCommunicationPassword          "_DFCSetCommunicationPassword@20"
#define DEF_DFCReadHardwareInfo                  "_DFCReadHardwareInfo@24"
#define DEF_DFCFileUpload                        "_DFCFileUpload@24"
#define DEF_DFCFileDownload                      "_DFCFileDownload@24"
#define DEF_DFCRecordVolume                      "_DFCRecordVolume@24"
#define DEF_DFCVersionLibrary                    "_DFCVersionLibrary@8"
#define DEF_DFCCustomCommand					 "_DFCCustomCommand@36"
// ########################################################
// ############## FUNKTIONEN FUER SETUP ###################
// ########################################################
#define DEF_DFCSetupLaden                        "_DFCSetupLaden@16"
#define DEF_DFCDownload                          "_DFCDownload@16"
#define DEF_DFCModifyStudioFile                  "_DFCModifyStudioFile@20"
// ########################################################
// FUNKTIONEN FUER LISTEN, SOWIE ZUTRITTSLISTEN (VERSION 2)
// ########################################################
#define DEF_DFCOpenTable                         "_DFCOpenTable@28"
#define DEF_DFCCloseTable                        "_DFCCloseTable@16"
#define DEF_DFCSetFilter                         "_DFCSetFilter@20"
#define DEF_DFCGetFilter                         "_DFCGetFilter@24"
#define DEF_DFCClearFilter                       "_DFCClearFilter@16"
#define DEF_DFCSkip                              "_DFCSkip@24"
#define DEF_DFCSetField                          "_DFCSetField@24"
#define DEF_DFCGetField                          "_DFCGetField@28"
// ########################################################
// ############## FUNKTIONEN FUER DATEN ###################
// ########################################################
#define DEF_DFCComClearData                      "_DFCComClearData@8"
#define DEF_DFCComCollectData                    "_DFCComCollectData@12"
#define DEF_DFCComGetDatensatz                   "_DFCComGetDatensatz@16"
#define DEF_DFCLoadDatensatzbeschreibung         "_DFCLoadDatensatzbeschreibung@12"
#define DEF_DFCDatBCnt                           "_DFCDatBCnt@4"
#define DEF_DFCDatBDatensatz                     "_DFCDatBDatensatz@16"
#define DEF_DFCDatBFeld                          "_DFCDatBFeld@24"
#define DEF_DFCReadRecord                        "_DFCReadRecord@20"
#define DEF_DFCQuitRecord                        "_DFCQuitRecord@12"
#define DEF_DFCRestoreRecords                    "_DFCRestoreRecords@12"
// ########################################################
// ############## FUNKTIONEN FUER LISTEN ###################
// ########################################################
#define DEF_DFCMakeListe                         "_DFCMakeListe@24"
#define DEF_DFCLoadListen                        "_DFCLoadListen@12"
#define DEF_DFCClrListenBuffer                   "_DFCClrListenBuffer@4"
#define DEF_DFCLoadListenbeschreibung            "_DFCLoadListenbeschreibung@12"
#define DEF_DFCListBCnt                          "_DFCListBCnt@4"
#define DEF_DFCListBDatensatz                    "_DFCListBDatensatz@20"
#define DEF_DFCListBFeld                         "_DFCListBFeld@24"
// ########################################################
// ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN #########
// ########################################################
#define DEF_DFCMakeEntranceList                  "_DFCMakeEntranceList@24"
#define DEF_DFCLoadEntranceList                  "_DFCLoadEntranceList@16"
#define DEF_DFCClearEntranceListBuffer           "_DFCClearEntranceListBuffer@8"
// ########################################################
// ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN 2 #######
// ########################################################
#define DEF_DFCMakeEntrance2List                 "_DFCMakeEntrance2List@24"
#define DEF_DFCLoadEntrance2List                 "_DFCLoadEntrance2List@16"
#define DEF_DFCClearEntrance2ListBuffer          "_DFCClearEntrance2ListBuffer@8"
#define DEF_DFCEntrance2Identification           "_DFCEntrance2Identification@32"
#define DEF_DFCEntrance2OnlineAction             "_DFCEntrance2OnlineAction@28"
#define DEF_DFCAccessControlIdentification       "_DFCAccessControlIdentification@36"
#define DEF_DFCAccessControlOnlineAction         "_DFCAccessControlOnlineAction@32"
#define DEF_DFCAccessControlKnobCommand          "_DFCAccessControlKnobCommand@36"
// ########################################################
// ####### FUNKTIONEN FUER FINGERPRINT ####################
// ########################################################
#define DEF_DFCFingerprintAppendRecord           "_DFCFingerprintAppendRecord@24"
#define DEF_DFCFingerprintGetRecord              "_DFCFingerprintGetRecord@32"
#define DEF_DFCFingerprintDeleteRecord           "_DFCFingerprintDeleteRecord@20"
#define DEF_DFCFingerprintList                   "_DFCFingerprintList@16"
#define DEF_DFCFingerprintBackup                 "_DFCFingerprintBackup@20"
#define DEF_DFCFingerprintRestore                "_DFCFingerprintRestore@20"
// ########################################################
// ####### FUNKTIONEN FUER TIMEBOYLISTEN ##################
// ########################################################
#define DEF_DFCMakeTimeboyList                   "_DFCMakeTimeboyList@32"
#define DEF_DFCLoadTimeboyList                   "_DFCLoadTimeboyList@24"
#define DEF_DFCClearTimeboyListBuffer            "_DFCClearTimeboyListBuffer@12"
// ########################################################
// ######### FUNKTIONEN FUER AKTIVE VERBINDUNG ############
// ########################################################
#define DEF_DFCStartActiveConnection             "_DFCStartActiveConnection@36"
#define DEF_DFCStopActiveConnection              "_DFCStopActiveConnection@4"
#define DEF_DFCGetFirstActiveChannelID           "_DFCGetFirstActiveChannelID@0"
#define DEF_DFCGetNextActiveChannelID            "_DFCGetNextActiveChannelID@4"
#define DEF_DFCGetInfoActiveChannel              "_DFCGetInfoActiveChannel@12"
#define DEF_DFCSetRecordAvailable                "_DFCSetRecordAvailable@4"
#define DEF_DFCRecordAvailable                   "_DFCRecordAvailable@20"
#define DEF_DFCBindDeviceToChannel               "_DFCBindDeviceToChannel@20"
// ########################################################
// ############# ** PRIVATE FUNKTIONEN ** #################
// ########################################################
#define DEF_DFCComVersion                        "_DFCComVersion@12"
#define DEF_DFCCheckData                         "_DFCCheckData@12"
#define DEF_DFCGetClass                          "_DFCGetClass@8"
#else
#define DFCOMDLL_NAME "DFCom"
// Zu ladende Library im Release fuer Produktion
#define DFCOMPRODUCTION_NAME "DFComProduction"
// ########################################################
// ############ FUNKTIONEN FUER ALLGEMEIN #################
// ########################################################
#define DEF_DFCComInit                           "DFCComInit"
#define DEF_DFCComOpenSerial                     "DFCComOpenSerial"
#define DEF_DFCComOpen                           "DFCComOpen"
#define DEF_DFCComOpenSocket                     "DFCComOpenSocket"
#define DEF_DFCComOpenIV                         "DFCComOpenIV"
#define DEF_DFCComClose                          "DFCComClose"
#define DEF_DFCCheckAE                           "DFCCheckAE"
#define DEF_DFCCheckDevice                       "DFCCheckDevice"
#define DEF_DFCComSetTime                        "DFCComSetTime"
#define DEF_DFCComGetTime                        "DFCComGetTime"
#define DEF_DFCComSendMessage                    "DFCComSendMessage"
#define DEF_DFCComSendInfotext                   "DFCComSendInfotext"
#define DEF_DFCGetSeriennummer                   "DFCGetSeriennummer"
#define DEF_DFCSetLogOn                          "DFCSetLogOn"
#define DEF_DFCSetLogOff                         "DFCSetLogOff"
#define DEF_DFCSetCallBack                       "DFCSetCallBack"
#define DEF_DFCSetLogFileName                    "DFCSetLogFileName"
#define DEF_DFCGetErrorText                      "DFCGetErrorText"
#define DEF_DFCSetGlobVar                        "DFCSetGlobVar"
#define DEF_DFCGetGlobVar                        "DFCGetGlobVar"
#define DEF_DFCCloseRelay                        "DFCCloseRelay"
#define DEF_DFCGetRelayState                     "DFCGetRelayState"
#define DEF_DFCOpenRelay                         "DFCOpenRelay"
#define DEF_DFCGetDevicePollRetry                "DFCGetDevicePollRetry"
#define DEF_DFCGetComPort                        "DFCGetComPort"
#define DEF_DFCSetComPort                        "DFCSetComPort"
#define DEF_DFCWrite                             "DFCWrite"
#define DEF_DFCRead                              "DFCRead"
#define DEF_DFCUpload                            "DFCUpload"
#define DEF_DFCGetVersionFirmware                "DFCGetVersionFirmware"
#define DEF_DFCGetVersionFirmwareFromFile        "DFCGetVersionFirmwareFromFile"
#define DEF_DFCGetInfo                           "DFCGetInfo"
#define DEF_DFCOpenComServerMode                 "DFCOpenComServerMode"
#define DEF_DFCCloseComServerMode                "DFCCloseComServerMode"
#define DEF_DFCUploadModule                      "DFCUploadModule"
#define DEF_DFCIsChannelOpen                     "DFCIsChannelOpen"
#define DEF_DFCSetOptionFirmware                 "DFCSetOptionFirmware"
#define DEF_DFCGetOptionFirmware                 "DFCGetOptionFirmware"
#define DEF_DFCSystemReadRecord                  "DFCSystemReadRecord"
#define DEF_DFCSystemQuitRecord                  "DFCSystemQuitRecord"
#define DEF_DFCSystemRestoreRecords              "DFCSystemRestoreRecords"
#define DEF_DFCReset                             "DFCReset"
#define DEF_DFCSetFontType                       "DFCSetFontType"
#define DEF_DFCSetPassword                       "DFCSetPassword"
#define DEF_DFCGetPasswordKey                    "DFCGetPasswordKey"
#define DEF_DFCPressVirtualKey                   "DFCPressVirtualKey"
#define DEF_DFCGetFlashStatus                    "DFCGetFlashStatus"
#define DEF_DFCSetCommunicationPassword          "DFCSetCommunicationPassword"
#define DEF_DFCReadHardwareInfo                  "DFCReadHardwareInfo"
#define DEF_DFCFileUpload                        "DFCFileUpload"
#define DEF_DFCFileDownload                      "DFCFileDownload"
#define DEF_DFCRecordVolume                      "DFCRecordVolume"
#define DEF_DFCVersionLibrary                    "DFCVersionLibrary"
#define DEF_DFCCustomCommand					 "DFCCustomCommand"
// ########################################################
// ############## FUNKTIONEN FUER SETUP ###################
// ########################################################
#define DEF_DFCSetupLaden                        "DFCSetupLaden"
#define DEF_DFCDownload                          "DFCDownload"
#define DEF_DFCModifyStudioFile                  "DFCModifyStudioFile"
// ########################################################
// FUNKTIONEN FUER LISTEN, SOWIE ZUTRITTSLISTEN (VERSION 2)
// ########################################################
#define DEF_DFCOpenTable                         "DFCOpenTable"
#define DEF_DFCCloseTable                        "DFCCloseTable"
#define DEF_DFCSetFilter                         "DFCSetFilter"
#define DEF_DFCGetFilter                         "DFCGetFilter"
#define DEF_DFCClearFilter                       "DFCClearFilter"
#define DEF_DFCSkip                              "DFCSkip"
#define DEF_DFCSetField                          "DFCSetField"
#define DEF_DFCGetField                          "DFCGetField"
// ########################################################
// ############## FUNKTIONEN FUER DATEN ###################
// ########################################################
#define DEF_DFCComClearData                      "DFCComClearData"
#define DEF_DFCComCollectData                    "DFCComCollectData"
#define DEF_DFCComGetDatensatz                   "DFCComGetDatensatz"
#define DEF_DFCLoadDatensatzbeschreibung         "DFCLoadDatensatzbeschreibung"
#define DEF_DFCDatBCnt                           "DFCDatBCnt"
#define DEF_DFCDatBDatensatz                     "DFCDatBDatensatz"
#define DEF_DFCDatBFeld                          "DFCDatBFeld"
#define DEF_DFCReadRecord                        "DFCReadRecord"
#define DEF_DFCQuitRecord                        "DFCQuitRecord"
#define DEF_DFCRestoreRecords                    "DFCRestoreRecords"
// ########################################################
// ############## FUNKTIONEN FUER LISTEN ###################
// ########################################################
#define DEF_DFCMakeListe                         "DFCMakeListe"
#define DEF_DFCLoadListen                        "DFCLoadListen"
#define DEF_DFCClrListenBuffer                   "DFCClrListenBuffer"
#define DEF_DFCLoadListenbeschreibung            "DFCLoadListenbeschreibung"
#define DEF_DFCListBCnt                          "DFCListBCnt"
#define DEF_DFCListBDatensatz                    "DFCListBDatensatz"
#define DEF_DFCListBFeld                         "DFCListBFeld"
// ########################################################
// ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN #########
// ########################################################
#define DEF_DFCMakeEntranceList                  "DFCMakeEntranceList"
#define DEF_DFCLoadEntranceList                  "DFCLoadEntranceList"
#define DEF_DFCClearEntranceListBuffer           "DFCClearEntranceListBuffer"
// ########################################################
// ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN 2 #######
// ########################################################
#define DEF_DFCMakeEntrance2List                 "DFCMakeEntrance2List"
#define DEF_DFCLoadEntrance2List                 "DFCLoadEntrance2List"
#define DEF_DFCClearEntrance2ListBuffer          "DFCClearEntrance2ListBuffer"
#define DEF_DFCEntrance2Identification           "DFCEntrance2Identification"
#define DEF_DFCEntrance2OnlineAction             "DFCEntrance2OnlineAction"
#define DEF_DFCAccessControlIdentification       "DFCAccessControlIdentification"
#define DEF_DFCAccessControlOnlineAction         "DFCAccessControlOnlineAction"
#define DEF_DFCAccessControlKnobCommand          "DFCAccessControlKnobCommand"
// ########################################################
// ####### FUNKTIONEN FUER FINGERPRINT ####################
// ########################################################
#define DEF_DFCFingerprintAppendRecord           "DFCFingerprintAppendRecord"
#define DEF_DFCFingerprintGetRecord              "DFCFingerprintGetRecord"
#define DEF_DFCFingerprintDeleteRecord           "DFCFingerprintDeleteRecord"
#define DEF_DFCFingerprintList                   "DFCFingerprintList"
#define DEF_DFCFingerprintBackup                 "DFCFingerprintBackup"
#define DEF_DFCFingerprintRestore                "DFCFingerprintRestore"
// ########################################################
// ####### FUNKTIONEN FUER TIMEBOYLISTEN ##################
// ########################################################
#define DEF_DFCMakeTimeboyList                   "DFCMakeTimeboyList"
#define DEF_DFCLoadTimeboyList                   "DFCLoadTimeboyList"
#define DEF_DFCClearTimeboyListBuffer            "DFCClearTimeboyListBuffer"
// ########################################################
// ######### FUNKTIONEN FUER AKTIVE VERBINDUNG ############
// ########################################################
#define DEF_DFCStartActiveConnection             "DFCStartActiveConnection"
#define DEF_DFCStopActiveConnection              "DFCStopActiveConnection"
#define DEF_DFCGetFirstActiveChannelID           "DFCGetFirstActiveChannelID"
#define DEF_DFCGetNextActiveChannelID            "DFCGetNextActiveChannelID"
#define DEF_DFCGetInfoActiveChannel              "DFCGetInfoActiveChannel"
#define DEF_DFCSetRecordAvailable                "DFCSetRecordAvailable"
#define DEF_DFCRecordAvailable                   "DFCRecordAvailable"
#define DEF_DFCBindDeviceToChannel               "DFCBindDeviceToChannel"
// ########################################################
// ############# ** PRIVATE FUNKTIONEN ** #################
// ########################################################
#define DEF_DFCComVersion                        "DFCComVersion"
#define DEF_DFCCheckData                         "DFCCheckData"
#define DEF_DFCGetClass                          "DFCGetClass"
#endif

// ########################################################
// ############ FUNKTIONEN FUER ALLGEMEIN #################
// ########################################################
#define DEF_DFCGetLastErrorNumber				 "DFCGetLastErrorNumber"
#define DEF_DFCBlockTransferSetDuration          "DFCBlockTransferSetDuration"
#define DEF_DFCBlockTransferResume               "DFCBlockTransferResume"
#define DEF_DFCBlockTransferGetState             "DFCBlockTransferGetState"
#define DEF_DFCBlockTransferDiscard              "DFCBlockTransferDiscard"
// ########################################################
// FUNKTIONEN FUER LISTEN, SOWIE ZUTRITTSLISTEN (VERSION 2)
// ########################################################
#define DEF_DFCTableOpen                         "DFCTableOpen"
#define DEF_DFCTableClose                        "DFCTableClose"
#define DEF_DFCTableSetFilter                    "DFCTableSetFilter"
#define DEF_DFCTableGetFilter                    "DFCTableGetFilter"
#define DEF_DFCTableRemoveFilter                 "DFCTableRemoveFilter"
#define DEF_DFCTableGetRowCount                  "DFCTableGetRowCount"
#define DEF_DFCTableGetCurrentRow                "DFCTableGetCurrentRow"
#define DEF_DFCTableSetCurrentRow                "DFCTableSetCurrentRow"
#define DEF_DFCTableSetCurrentRowData            "DFCTableSetCurrentRowData"
#define DEF_DFCTableSetCurrentColumnData         "DFCTableSetCurrentColumnData"
#define DEF_DFCTableSetAllRowsToColumnData       "DFCTableSetAllRowsToColumnData"
#define DEF_DFCTableGetCurrentRowData            "DFCTableGetCurrentRowData"
#define DEF_DFCTableGetCurrentColumnData         "DFCTableGetCurrentColumnData"
#define DEF_DFCTableAppendRowData                "DFCTableAppendRowData"
#define DEF_DFCTableDeleteCurrentRow             "DFCTableDeleteCurrentRow"
#define DEF_DFCTableDeleteAvailableRows          "DFCTableDeleteAvailableRows"
// ########################################################
// ############# ** PRIVATE FUNKTIONEN ** #################
// ########################################################
#define DEF_DllGetVersion                        "DllGetVersion"
// ########################################################
// ########### FUNKTIONEN FUER PRODUKTION #################
// ########################################################
#define DEF_DFPReadHardwareInfo                  "DFPReadHardwareInfo"
#define DEF_DFPWriteHardwareInfo                 "DFPWriteHardwareInfo"
#define DEF_DllGetProtectionKey                  "DllGetProtectionKey"
#define DEF_DllUnlockProtection                  "DllUnlockProtection"

DFComDLL::DFComDLL(QObject *parent) : QLibrary( QString::fromUtf8(DFCOMDLL_NAME), parent)
{
    init();
}

DFComDLL::~DFComDLL()
{
}

void DFComDLL::init()
{
    // Laden der Library.
    if (load()) {
        // ########################################################
        // ############ FUNKTIONEN FUER ALLGEMEIN #################
        // ########################################################
        _DFCComInit         = (__DFCComInit)resolve(DEF_DFCComInit);
        _DFCComOpenSerial   = (__DFCComOpenSerial)resolve(DEF_DFCComOpenSerial);
        _DFCComOpen         = (__DFCComOpen)resolve(DEF_DFCComOpen);
        _DFCComOpenSocket   = (__DFCComOpenSocket)resolve(DEF_DFCComOpenSocket);
        _DFCComOpenIV       = (__DFCComOpenIV)resolve(DEF_DFCComOpenIV);
        _DFCComClose        = (__DFCComClose)resolve(DEF_DFCComClose);
        _DFCCheckAE         = (__DFCCheckAE)resolve(DEF_DFCCheckAE);
        _DFCCheckDevice     = (__DFCCheckDevice)resolve(DEF_DFCCheckDevice);
        _DFCComSetTime      = (__DFCComSetTime)resolve(DEF_DFCComSetTime);
        _DFCComGetTime      = (__DFCComGetTime)resolve(DEF_DFCComGetTime);
        _DFCComSendMessage  = (__DFCComSendMessage)resolve(DEF_DFCComSendMessage);
        _DFCComSendInfotext = (__DFCComSendInfotext)resolve(DEF_DFCComSendInfotext);
        _DFCGetSeriennummer = (__DFCGetSeriennummer)resolve(DEF_DFCGetSeriennummer);
        _DFCSetLogOn        = (__DFCSetLogOn)resolve(DEF_DFCSetLogOn);
        _DFCSetLogOff       = (__DFCSetLogOff)resolve(DEF_DFCSetLogOff);
        _DFCSetCallBack     = (__DFCSetCallBack)resolve(DEF_DFCSetCallBack);
        _DFCSetLogFileName  = (__DFCSetLogFileName)resolve(DEF_DFCSetLogFileName);
        _DFCGetErrorText    = (__DFCGetErrorText)resolve(DEF_DFCGetErrorText);
        _DFCSetGlobVar      = (__DFCSetGlobVar)resolve(DEF_DFCSetGlobVar);
        _DFCGetGlobVar      = (__DFCGetGlobVar)resolve(DEF_DFCGetGlobVar);
        _DFCCloseRelay      = (__DFCCloseRelay)resolve(DEF_DFCCloseRelay);
        _DFCGetRelayState   = (__DFCGetRelayState)resolve(DEF_DFCGetRelayState);
        _DFCOpenRelay       = (__DFCOpenRelay)resolve(DEF_DFCOpenRelay);
        _DFCGetDevicePollRetry = (__DFCGetDevicePollRetry)resolve(DEF_DFCGetDevicePollRetry);
        _DFCGetComPort      = (__DFCGetComPort)resolve(DEF_DFCGetComPort);
        _DFCSetComPort      = (__DFCSetComPort)resolve(DEF_DFCSetComPort);
        _DFCWrite           = (__DFCWrite)resolve(DEF_DFCWrite);
        _DFCRead            = (__DFCRead)resolve(DEF_DFCRead);
        _DFCUpload          = (__DFCUpload)resolve(DEF_DFCUpload);
        _DFCGetVersionFirmware = (__DFCGetVersionFirmware)resolve(DEF_DFCGetVersionFirmware);
        _DFCGetVersionFirmwareFromFile = (__DFCGetVersionFirmwareFromFile)resolve(DEF_DFCGetVersionFirmwareFromFile);
        _DFCGetInfo         = (__DFCGetInfo)resolve(DEF_DFCGetInfo);
        _DFCOpenComServerMode  = (__DFCOpenComServerMode)resolve(DEF_DFCOpenComServerMode);
        _DFCCloseComServerMode = (__DFCCloseComServerMode)resolve(DEF_DFCCloseComServerMode);
        _DFCUploadModule    = (__DFCUploadModule)resolve(DEF_DFCUploadModule);
        _DFCIsChannelOpen   = (__DFCIsChannelOpen)resolve(DEF_DFCIsChannelOpen);
        _DFCSetOptionFirmware  = (__DFCSetOptionFirmware)resolve(DEF_DFCSetOptionFirmware);
        _DFCGetOptionFirmware  = (__DFCGetOptionFirmware)resolve(DEF_DFCGetOptionFirmware);
        _DFCSystemReadRecord = (__DFCSystemReadRecord)resolve(DEF_DFCSystemReadRecord);
        _DFCSystemQuitRecord = (__DFCSystemQuitRecord)resolve(DEF_DFCSystemQuitRecord);
        _DFCSystemRestoreRecords = (__DFCSystemRestoreRecords)resolve(DEF_DFCSystemRestoreRecords);
        _DFCReset  = (__DFCReset)resolve(DEF_DFCReset);
        _DFCSetFontType  = (__DFCSetFontType)resolve(DEF_DFCSetFontType);
        _DFCSetPassword  = (__DFCSetPassword)resolve(DEF_DFCSetPassword);
        _DFCGetPasswordKey  = (__DFCGetPasswordKey)resolve(DEF_DFCGetPasswordKey);
        _DFCPressVirtualKey  = (__DFCPressVirtualKey)resolve(DEF_DFCPressVirtualKey);
        _DFCGetFlashStatus  = (__DFCGetFlashStatus)resolve(DEF_DFCGetFlashStatus);
        _DFCSetCommunicationPassword  = (__DFCSetCommunicationPassword)resolve(DEF_DFCSetCommunicationPassword);
        _DFCReadHardwareInfo = (__DFCReadHardwareInfo)resolve(DEF_DFCReadHardwareInfo);
        _DFCFileUpload = (__DFCFileUpload)resolve(DEF_DFCFileUpload);
        _DFCFileDownload = (__DFCFileDownload)resolve(DEF_DFCFileDownload);
        _DFCRecordVolume = (__DFCRecordVolume)resolve(DEF_DFCRecordVolume);
        _DFCVersionLibrary = (__DFCVersionLibrary)resolve(DEF_DFCVersionLibrary);
        _DFCCustomCommand = (__DFCCustomCommand)resolve(DEF_DFCCustomCommand);
        _DFCGetLastErrorNumber = (__DFCGetLastErrorNumber)resolve(DEF_DFCGetLastErrorNumber);
        _DFCBlockTransferSetDuration = (__DFCBlockTransferSetDuration)resolve(DEF_DFCBlockTransferSetDuration);
        _DFCBlockTransferResume = (__DFCBlockTransferResume)resolve(DEF_DFCBlockTransferResume);
        _DFCBlockTransferGetState = (__DFCBlockTransferGetState)resolve(DEF_DFCBlockTransferGetState);
        _DFCBlockTransferDiscard = (__DFCBlockTransferDiscard)resolve(DEF_DFCBlockTransferDiscard);

        // ########################################################
        // ############## FUNKTIONEN FUER SETUP ###################
        // ########################################################
        _DFCSetupLaden      = (__DFCSetupLaden)resolve(DEF_DFCSetupLaden);
        _DFCDownload        = (__DFCDownload)resolve(DEF_DFCDownload);
        _DFCModifyStudioFile = (__DFCModifyStudioFile)resolve(DEF_DFCModifyStudioFile);
        // ########################################################
        // FUNKTIONEN FUER LISTEN, SOWIE ZUTRITTSLISTEN (VERSION 2)
        // ########################################################
        _DFCOpenTable       = (__DFCOpenTable)resolve(DEF_DFCOpenTable);
        _DFCCloseTable      = (__DFCCloseTable)resolve(DEF_DFCCloseTable);
        _DFCSetFilter       = (__DFCSetFilter)resolve(DEF_DFCSetFilter);
        _DFCGetFilter       = (__DFCGetFilter)resolve(DEF_DFCGetFilter);
        _DFCClearFilter     = (__DFCClearFilter)resolve(DEF_DFCClearFilter);
        _DFCSkip            = (__DFCSkip)resolve(DEF_DFCSkip);
        _DFCSetField        = (__DFCSetField)resolve(DEF_DFCSetField);
        _DFCGetField        = (__DFCGetField)resolve(DEF_DFCGetField);
        _DFCTableOpen                  = (__DFCTableOpen)resolve(DEF_DFCTableOpen);
        _DFCTableClose                 = (__DFCTableClose)resolve(DEF_DFCTableClose);
        _DFCTableSetFilter             = (__DFCTableSetFilter)resolve(DEF_DFCTableSetFilter);
        _DFCTableGetFilter             = (__DFCTableGetFilter)resolve(DEF_DFCTableGetFilter);
        _DFCTableRemoveFilter          = (__DFCTableRemoveFilter)resolve(DEF_DFCTableRemoveFilter);
        _DFCTableGetRowCount           = (__DFCTableGetRowCount)resolve(DEF_DFCTableGetRowCount);
        _DFCTableGetCurrentRow         = (__DFCTableGetCurrentRow)resolve(DEF_DFCTableGetCurrentRow);
        _DFCTableSetCurrentRow         = (__DFCTableSetCurrentRow)resolve(DEF_DFCTableSetCurrentRow);
        _DFCTableSetCurrentRowData     = (__DFCTableSetCurrentRowData)resolve(DEF_DFCTableSetCurrentRowData);
        _DFCTableSetCurrentColumnData  = (__DFCTableSetCurrentColumnData)resolve(DEF_DFCTableSetCurrentColumnData);
        _DFCTableSetAllRowsToColumnData  = (__DFCTableSetAllRowsToColumnData)resolve(DEF_DFCTableSetAllRowsToColumnData);
        _DFCTableGetCurrentRowData     = (__DFCTableGetCurrentRowData)resolve(DEF_DFCTableGetCurrentRowData);
        _DFCTableGetCurrentColumnData  = (__DFCTableGetCurrentColumnData)resolve(DEF_DFCTableGetCurrentColumnData);
        _DFCTableAppendRowData         = (__DFCTableAppendRowData)resolve(DEF_DFCTableAppendRowData);
        _DFCTableDeleteCurrentRow      = (__DFCTableDeleteCurrentRow)resolve(DEF_DFCTableDeleteCurrentRow);
        _DFCTableDeleteAvailableRows   = (__DFCTableDeleteAvailableRows)resolve(DEF_DFCTableDeleteAvailableRows);
        // ########################################################
        // ############## FUNKTIONEN FUER DATEN ###################
        // ########################################################
        _DFCComClearData    = (__DFCComClearData)resolve(DEF_DFCComClearData);
        _DFCComCollectData  = (__DFCComCollectData)resolve(DEF_DFCComCollectData);
        _DFCComGetDatensatz = (__DFCComGetDatensatz)resolve(DEF_DFCComGetDatensatz);
        _DFCLoadDatensatzbeschreibung = (__DFCLoadDatensatzbeschreibung)resolve(DEF_DFCLoadDatensatzbeschreibung);
        _DFCDatBCnt         = (__DFCDatBCnt)resolve(DEF_DFCDatBCnt);
        _DFCDatBDatensatz   = (__DFCDatBDatensatz)resolve(DEF_DFCDatBDatensatz);
        _DFCDatBFeld        = (__DFCDatBFeld)resolve(DEF_DFCDatBFeld);
        _DFCReadRecord      = (__DFCReadRecord)resolve(DEF_DFCReadRecord);
        _DFCQuitRecord      = (__DFCQuitRecord)resolve(DEF_DFCQuitRecord);
        _DFCRestoreRecords  = (__DFCRestoreRecords)resolve(DEF_DFCRestoreRecords);
        // ########################################################
        // ############## FUNKTIONEN FUER LISTEN ###################
        // ########################################################
        _DFCMakeListe       = (__DFCMakeListe)resolve(DEF_DFCMakeListe);
        _DFCLoadListen      = (__DFCLoadListen)resolve(DEF_DFCLoadListen);
        _DFCClrListenBuffer = (__DFCClrListenBuffer)resolve(DEF_DFCClrListenBuffer);
        _DFCLoadListenbeschreibung = (__DFCLoadListenbeschreibung)resolve(DEF_DFCLoadListenbeschreibung);
        _DFCListBCnt        = (__DFCListBCnt)resolve(DEF_DFCListBCnt);
        _DFCListBDatensatz  = (__DFCListBDatensatz)resolve(DEF_DFCListBDatensatz);
        _DFCListBFeld       = (__DFCListBFeld)resolve(DEF_DFCListBFeld);
        // ########################################################
        // ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN #########
        // ########################################################
        _DFCMakeEntranceList = (__DFCMakeEntranceList)resolve(DEF_DFCMakeEntranceList);
        _DFCLoadEntranceList = (__DFCLoadEntranceList)resolve(DEF_DFCLoadEntranceList);
        _DFCClearEntranceListBuffer = (__DFCClearEntranceListBuffer)resolve(DEF_DFCClearEntranceListBuffer);
        // ########################################################
        // ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN 2 #######
        // ########################################################
        _DFCMakeEntrance2List = (__DFCMakeEntrance2List)resolve(DEF_DFCMakeEntrance2List);
        _DFCLoadEntrance2List = (__DFCLoadEntrance2List)resolve(DEF_DFCLoadEntrance2List);
        _DFCClearEntrance2ListBuffer = (__DFCClearEntrance2ListBuffer)resolve(DEF_DFCClearEntrance2ListBuffer);
        _DFCEntrance2Identification  = (__DFCEntrance2Identification)resolve(DEF_DFCEntrance2Identification);
        _DFCEntrance2OnlineAction  = (__DFCEntrance2OnlineAction)resolve(DEF_DFCEntrance2OnlineAction);
        _DFCAccessControlIdentification  = (__DFCAccessControlIdentification)resolve(DEF_DFCAccessControlIdentification);
        _DFCAccessControlOnlineAction  = (__DFCAccessControlOnlineAction)resolve(DEF_DFCAccessControlOnlineAction);
        _DFCAccessControlKnobCommand = (__DFCAccessControlKnobCommand)resolve(DEF_DFCAccessControlKnobCommand);
        // ########################################################
        // ####### FUNKTIONEN FUER FINGERPRINT ####################
        // ########################################################
        _DFCFingerprintAppendRecord = (__DFCFingerprintAppendRecord)resolve(DEF_DFCFingerprintAppendRecord);
        _DFCFingerprintGetRecord = (__DFCFingerprintGetRecord)resolve(DEF_DFCFingerprintGetRecord);
        _DFCFingerprintDeleteRecord = (__DFCFingerprintDeleteRecord)resolve(DEF_DFCFingerprintDeleteRecord);
        _DFCFingerprintList = (__DFCFingerprintList)resolve(DEF_DFCFingerprintList);
        _DFCFingerprintBackup = (__DFCFingerprintBackup)resolve(DEF_DFCFingerprintBackup);
        _DFCFingerprintRestore = (__DFCFingerprintRestore)resolve(DEF_DFCFingerprintRestore);
        // ########################################################
        // ####### FUNKTIONEN FUER TIMEBOYLISTEN ##################
        // ########################################################
        _DFCMakeTimeboyList = (__DFCMakeTimeboyList)resolve(DEF_DFCMakeTimeboyList);
        _DFCLoadTimeboyList = (__DFCLoadTimeboyList)resolve(DEF_DFCLoadTimeboyList);
        _DFCClearTimeboyListBuffer = (__DFCClearTimeboyListBuffer)resolve(DEF_DFCClearTimeboyListBuffer);
        // ########################################################
        // ######### FUNKTIONEN FUER AKTIVE VERBINDUNG ############
        // ########################################################
        _DFCStartActiveConnection = (__DFCStartActiveConnection)resolve(DEF_DFCStartActiveConnection);
        _DFCStopActiveConnection = (__DFCStopActiveConnection)resolve(DEF_DFCStopActiveConnection);
        _DFCGetFirstActiveChannelID = (__DFCGetFirstActiveChannelID)resolve(DEF_DFCGetFirstActiveChannelID);
        _DFCGetNextActiveChannelID = (__DFCGetNextActiveChannelID)resolve(DEF_DFCGetNextActiveChannelID);
        _DFCGetInfoActiveChannel = (__DFCGetInfoActiveChannel)resolve(DEF_DFCGetInfoActiveChannel);
        _DFCSetRecordAvailable = (__DFCSetRecordAvailable)resolve(DEF_DFCSetRecordAvailable);
        _DFCRecordAvailable = (__DFCRecordAvailable)resolve(DEF_DFCRecordAvailable);
        _DFCBindDeviceToChannel = (__DFCBindDeviceToChannel)resolve(DEF_DFCBindDeviceToChannel);
        // ########################################################
        // ############# ** PRIVATE FUNKTIONEN ** #################
        // ########################################################
#ifdef WITH_DLLGETVERSION
        _DllGetVersion = (__DllGetVersion)resolve(DEF_DllGetVersion);
#endif
        _DFCComVersion = (__DFCComVersion)resolve(DEF_DFCComVersion);
        _DFCCheckData  = (__DFCCheckData)resolve(DEF_DFCCheckData);
        _DFCGetClass   = (__DFCGetClass)resolve(DEF_DFCGetClass);

#if defined(_DEBUG) && defined(DATAFOX_STUDIO_IV)
        // ########################################################
        // ########### FUNKTIONEN FUER PRODUKTION #################
        // ########################################################
        _DFPReadHardwareInfo  = (__DFPReadHardwareInfo)resolve(DEF_DFPReadHardwareInfo);
        _DFPWriteHardwareInfo = (__DFPWriteHardwareInfo)resolve(DEF_DFPWriteHardwareInfo);
        _DllGetProtectionKey  = (__DllGetProtectionKey)resolve(DEF_DllGetProtectionKey);
        _DllUnlockProtection  = (__DllUnlockProtection)resolve(DEF_DllUnlockProtection);
#endif
    }
}

int DFComDLL::Validate(int version)
{
    int res = 0;
    QString functionName;
    m_errorText.clear();

    if (isLoaded()) {
        // ########################################################
        // ############ FUNKTIONEN FUER ALLGEMEIN #################
        // ########################################################
        if (version >= 0x00000000 && _DFCComInit == NULL) { functionName = DEF_DFCComInit; goto label_error; }
        if (version >= 0x02000300 && _DFCComOpenSerial == NULL) { functionName = DEF_DFCComOpenSerial; goto label_error; }
        if (version >= 0x00000000 && _DFCComOpen == NULL) { functionName = DEF_DFCComOpen; goto label_error; }
        if (version >= 0x02000300 && _DFCComOpenSocket == NULL) { functionName = DEF_DFCComOpenSocket; goto label_error; }
        if (version >= 0x02001000 && _DFCComOpenIV == NULL) { functionName = DEF_DFCComOpenIV; goto label_error; }
        if (version >= 0x00000000 && _DFCComClose == NULL) { functionName = DEF_DFCComClose; goto label_error; }
        if (version >= 0x00000000 && _DFCCheckAE == NULL) { functionName = DEF_DFCCheckAE; goto label_error; }
        if (version >= 0x02000300 && _DFCCheckDevice == NULL) { functionName = DEF_DFCCheckDevice; goto label_error; }
        if (version >= 0x00000000 && _DFCComSetTime == NULL) { functionName = DEF_DFCComSetTime; goto label_error; }
        if (version >= 0x00000000 && _DFCComGetTime == NULL) { functionName = DEF_DFCComGetTime; goto label_error; }
        if (version >= 0x00000000 && _DFCComSendMessage == NULL) { functionName = DEF_DFCComSendMessage; goto label_error; }
        if (version >= 0x00000000 && _DFCComSendInfotext == NULL) { functionName = DEF_DFCComSendInfotext; goto label_error; }
        if (version >= 0x00000000 && _DFCGetSeriennummer == NULL) { functionName = DEF_DFCGetSeriennummer; goto label_error; }
        if (version >= 0x02000300 && _DFCSetLogOn == NULL) { functionName = DEF_DFCSetLogOn; goto label_error; }
        if (version >= 0x02000300 && _DFCSetLogOff == NULL) { functionName = DEF_DFCSetLogOff; goto label_error; }
        if (version >= 0x02000000 && _DFCSetCallBack == NULL) { functionName = DEF_DFCSetCallBack; goto label_error; }
        if (version >= 0x02000300 && _DFCSetLogFileName == NULL) { functionName = DEF_DFCSetLogFileName; goto label_error; }
        if (version >= 0x02000300 && _DFCGetErrorText == NULL) { functionName = DEF_DFCGetErrorText; goto label_error; }
        if (version >= 0x02000400 && _DFCSetGlobVar == NULL) { functionName = DEF_DFCSetGlobVar; goto label_error; }
        if (version >= 0x02000400 && _DFCGetGlobVar == NULL) { functionName = DEF_DFCGetGlobVar; goto label_error; }
        if (version >= 0x02000500 && _DFCCloseRelay == NULL) { functionName = DEF_DFCCloseRelay; goto label_error; }
        if (version >= 0x02000500 && _DFCGetRelayState == NULL) { functionName = DEF_DFCGetRelayState; goto label_error; }
        if (version >= 0x02000500 && _DFCOpenRelay == NULL) { functionName = DEF_DFCOpenRelay; goto label_error; }
        if (version >= 0x02000500 && _DFCGetDevicePollRetry == NULL) { functionName = DEF_DFCGetDevicePollRetry; goto label_error; }
        if (version >= 0x00000000 && _DFCGetComPort == NULL) { functionName = DEF_DFCGetComPort; goto label_error; }
        if (version >= 0x00000000 && _DFCSetComPort == NULL) { functionName = DEF_DFCSetComPort; goto label_error; }
        if (version >= 0x02001800 && _DFCWrite == NULL) { functionName = DEF_DFCWrite; goto label_error; }
        if (version >= 0x02001800 && _DFCRead == NULL) { functionName = DEF_DFCRead; goto label_error; }
        if (version >= 0x02001800 && _DFCUpload == NULL) { functionName = DEF_DFCUpload; goto label_error; }
        if (version >= 0x02001800 && _DFCGetVersionFirmware == NULL) { functionName = DEF_DFCGetVersionFirmware; goto label_error; }
        if (version >= 0x02002100 && _DFCGetVersionFirmwareFromFile == NULL) { functionName = DEF_DFCGetVersionFirmwareFromFile; goto label_error; }
        if (version >= 0x04010400 && _DFCGetInfo == NULL) { functionName = DEF_DFCGetInfo; goto label_error; }
        if (version >= 0x04010400 && _DFCOpenComServerMode == NULL) { functionName = DEF_DFCOpenComServerMode; goto label_error; }
        if (version >= 0x04010400 && _DFCCloseComServerMode == NULL) { functionName = DEF_DFCCloseComServerMode; goto label_error; }
        if (version >= 0x04010500 && _DFCUploadModule == NULL) { functionName = DEF_DFCUploadModule; goto label_error; }
        if (version >= 0x04010500 && _DFCIsChannelOpen == NULL) { functionName = DEF_DFCIsChannelOpen; goto label_error; }
        if (version >= 0x04010600 && _DFCSetOptionFirmware == NULL) { functionName = DEF_DFCSetOptionFirmware; goto label_error; }
        if (version >= 0x04010600 && _DFCGetOptionFirmware == NULL) { functionName = DEF_DFCGetOptionFirmware; goto label_error; }
        if (version >= 0x04010700 && _DFCSystemReadRecord == NULL) { functionName = DEF_DFCSystemReadRecord; goto label_error; }
        if (version >= 0x04010700 && _DFCSystemQuitRecord == NULL) { functionName = DEF_DFCSystemQuitRecord; goto label_error; }
        if (version >= 0x04010700 && _DFCSystemRestoreRecords == NULL) { functionName = DEF_DFCSystemRestoreRecords; goto label_error; }
        if (version >= 0x04010700 && _DFCReset == NULL) { functionName = DEF_DFCReset; goto label_error; }
        if (version >= 0x04010700 && _DFCSetFontType == NULL) { functionName = DEF_DFCSetFontType; goto label_error; }
        if (version >= 0x04010700 && _DFCSetPassword == NULL) { functionName = DEF_DFCSetPassword; goto label_error; }
        if (version >= 0x04010700 && _DFCGetPasswordKey == NULL) { functionName = DEF_DFCGetPasswordKey; goto label_error; }
        if (version >= 0x04010700 && _DFCPressVirtualKey == NULL) { functionName = DEF_DFCPressVirtualKey; goto label_error; }
        if (version >= 0x04010700 && _DFCGetFlashStatus == NULL) { functionName = DEF_DFCGetFlashStatus; goto label_error; }
        if (version >= 0x04020300 && _DFCSetCommunicationPassword == NULL) { functionName = DEF_DFCSetCommunicationPassword; goto label_error; }
        if (version >= 0x04020300 && _DFCReadHardwareInfo == NULL) { functionName = DEF_DFCReadHardwareInfo; goto label_error; }
        if (version >= 0x04020302 && _DFCFileUpload == NULL) { functionName = DEF_DFCFileUpload; goto label_error; }
        if (version >= 0x04020302 && _DFCFileDownload == NULL) { functionName = DEF_DFCFileDownload; goto label_error; }
        if (version >= 0x04020302 && _DFCRecordVolume == NULL) { functionName = DEF_DFCRecordVolume; goto label_error; }
        if (version >= 0x04030001 && _DFCVersionLibrary == NULL) { functionName = DEF_DFCVersionLibrary; goto label_error; }
		if (version >= 0x04030600 && _DFCCustomCommand == NULL) { functionName = DEF_DFCCustomCommand; goto label_error; }
        if (version >= 0x04030800 && _DFCGetLastErrorNumber == NULL) { functionName = DEF_DFCGetLastErrorNumber; goto label_error; }
        if (version >= 0x04030800 && _DFCBlockTransferSetDuration == NULL) { functionName = DEF_DFCBlockTransferSetDuration; goto label_error; }
        if (version >= 0x04030800 && _DFCBlockTransferResume == NULL) { functionName = DEF_DFCBlockTransferResume; goto label_error; }
        if (version >= 0x04030800 && _DFCBlockTransferGetState == NULL) { functionName = DEF_DFCBlockTransferGetState; goto label_error; }
        if (version >= 0x04030800 && _DFCBlockTransferDiscard == NULL) { functionName = DEF_DFCBlockTransferDiscard; goto label_error; }

        // ########################################################
        // ############## FUNKTIONEN FUER SETUP ###################
        // ########################################################
        if (version >= 0x00000000 && _DFCSetupLaden == NULL) { functionName = DEF_DFCSetupLaden; goto label_error; }
        if (version >= 0x02000000 && _DFCDownload == NULL) { functionName = DEF_DFCDownload; goto label_error; }
        if (version >= 0x04010500 && _DFCModifyStudioFile == NULL) { functionName = DEF_DFCModifyStudioFile; goto label_error; }
        // ########################################################
        // FUNKTIONEN FUER LISTEN, SOWIE ZUTRITTSLISTEN (VERSION 2)
        // ########################################################
        if (version >= 0x04010100 && _DFCOpenTable == NULL) { functionName = DEF_DFCOpenTable; goto label_error; }
        if (version >= 0x04010100 && _DFCCloseTable == NULL) { functionName = DEF_DFCCloseTable; goto label_error; }
        if (version >= 0x04010100 && _DFCSetFilter == NULL) { functionName = DEF_DFCSetFilter; goto label_error; }
        if (version >= 0x04010100 && _DFCGetFilter == NULL) { functionName = DEF_DFCGetFilter; goto label_error; }
        if (version >= 0x04010100 && _DFCClearFilter == NULL) { functionName = DEF_DFCClearFilter; goto label_error; }
        if (version >= 0x04010100 && _DFCSkip == NULL) { functionName = DEF_DFCSkip; goto label_error; }
        if (version >= 0x04010100 && _DFCSetField == NULL) { functionName = DEF_DFCSetField; goto label_error; }
        if (version >= 0x04010100 && _DFCGetField == NULL) { functionName = DEF_DFCGetField; goto label_error; }
        if (version >= 0x04030800 && _DFCTableOpen == NULL) { functionName =  DEF_DFCTableOpen; goto label_error; }
        if (version >= 0x04030800 && _DFCTableClose == NULL) { functionName =  DEF_DFCTableClose; goto label_error; }
        if (version >= 0x04030800 && _DFCTableSetFilter == NULL) { functionName =  DEF_DFCTableSetFilter; goto label_error; }
        if (version >= 0x04030800 && _DFCTableGetFilter == NULL) { functionName =  DEF_DFCTableGetFilter; goto label_error; }
        if (version >= 0x04030800 && _DFCTableRemoveFilter == NULL) { functionName =  DEF_DFCTableRemoveFilter; goto label_error; }
        if (version >= 0x04030800 && _DFCTableGetRowCount == NULL) { functionName =  DEF_DFCTableGetRowCount; goto label_error; }
        if (version >= 0x04030800 && _DFCTableGetCurrentRow == NULL) { functionName =  DEF_DFCTableGetCurrentRow; goto label_error; }
        if (version >= 0x04030800 && _DFCTableSetCurrentRow == NULL) { functionName =  DEF_DFCTableSetCurrentRow; goto label_error; }
        if (version >= 0x04030800 && _DFCTableSetCurrentRowData == NULL) { functionName =  DEF_DFCTableSetCurrentRowData; goto label_error; }
        if (version >= 0x04030800 && _DFCTableSetCurrentColumnData == NULL) { functionName =  DEF_DFCTableSetCurrentColumnData; goto label_error; }
        if (version >= 0x04030800 && _DFCTableSetAllRowsToColumnData == NULL) { functionName =  DEF_DFCTableSetAllRowsToColumnData; goto label_error; }
        if (version >= 0x04030800 && _DFCTableGetCurrentRowData == NULL) { functionName =  DEF_DFCTableGetCurrentRowData; goto label_error; }
        if (version >= 0x04030800 && _DFCTableGetCurrentColumnData == NULL) { functionName =  DEF_DFCTableGetCurrentColumnData; goto label_error; }
        if (version >= 0x04030800 && _DFCTableAppendRowData == NULL) { functionName =  DEF_DFCTableAppendRowData; goto label_error; }
        if (version >= 0x04030800 && _DFCTableDeleteCurrentRow == NULL) { functionName =  DEF_DFCTableDeleteCurrentRow; goto label_error; }
        if (version >= 0x04030800 && _DFCTableDeleteAvailableRows == NULL) { functionName =  DEF_DFCTableDeleteAvailableRows; goto label_error; }

        // ########################################################
        // ############## FUNKTIONEN FUER DATEN ###################
        // ########################################################
        if (version >= 0x00000000 && _DFCComClearData == NULL) { functionName = DEF_DFCComClearData; goto label_error; }
        if (version >= 0x00000000 && _DFCComCollectData == NULL) { functionName = DEF_DFCComCollectData; goto label_error; }
        if (version >= 0x00000000 && _DFCComGetDatensatz == NULL) { functionName = DEF_DFCComGetDatensatz; goto label_error; }
        if (version >= 0x00000000 && _DFCLoadDatensatzbeschreibung == NULL) { functionName = DEF_DFCLoadDatensatzbeschreibung; goto label_error; }
        if (version >= 0x00000000 && _DFCDatBCnt == NULL) { functionName = DEF_DFCDatBCnt; goto label_error; }
        if (version >= 0x00000000 && _DFCDatBDatensatz == NULL) { functionName = DEF_DFCDatBDatensatz; goto label_error; }
        if (version >= 0x00000000 && _DFCDatBFeld == NULL) { functionName = DEF_DFCDatBFeld; goto label_error; }
        if (version >= 0x02002300 && _DFCReadRecord == NULL) { functionName = DEF_DFCReadRecord; goto label_error; }
        if (version >= 0x02002300 && _DFCQuitRecord == NULL) { functionName = DEF_DFCQuitRecord; goto label_error; }
        if (version >= 0x04010600 && _DFCRestoreRecords == NULL) { functionName = DEF_DFCRestoreRecords; goto label_error; }
        // ########################################################
        // ############## FUNKTIONEN FUER LISTEN ###################
        // ########################################################
        if (version >= 0x00000000 && _DFCMakeListe == NULL) { functionName = DEF_DFCMakeListe; goto label_error; }
        if (version >= 0x00000000 && _DFCLoadListen == NULL) { functionName = DEF_DFCLoadListen; goto label_error; }
        if (version >= 0x00000000 && _DFCClrListenBuffer == NULL) { functionName = DEF_DFCClrListenBuffer; goto label_error; }
        if (version >= 0x00000000 && _DFCLoadListenbeschreibung == NULL) { functionName = DEF_DFCLoadListenbeschreibung; goto label_error; }
        if (version >= 0x00000000 && _DFCListBCnt == NULL) { functionName = DEF_DFCListBCnt; goto label_error; }
        if (version >= 0x00000000 && _DFCListBDatensatz == NULL) { functionName = DEF_DFCListBDatensatz; goto label_error; }
        if (version >= 0x00000000 && _DFCListBFeld == NULL) { functionName = DEF_DFCListBFeld; goto label_error; }
        // ########################################################
        // ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN #########
        // ########################################################
        if (version >= 0x02000500 && _DFCMakeEntranceList == NULL) { functionName = DEF_DFCMakeEntranceList; goto label_error; }
        if (version >= 0x02000500 && _DFCLoadEntranceList == NULL) { functionName = DEF_DFCLoadEntranceList; goto label_error; }
        if (version >= 0x02000500 && _DFCClearEntranceListBuffer == NULL) { functionName = DEF_DFCClearEntranceListBuffer; goto label_error; }
        // ########################################################
        // ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN 2 #######
        // ########################################################
        if (version >= 0x03020000 && _DFCMakeEntrance2List == NULL) { functionName = DEF_DFCMakeEntrance2List; goto label_error; }
        if (version >= 0x03020000 && _DFCLoadEntrance2List == NULL) { functionName = DEF_DFCLoadEntrance2List; goto label_error; }
        if (version >= 0x03020000 && _DFCClearEntrance2ListBuffer == NULL) { functionName = DEF_DFCClearEntrance2ListBuffer; goto label_error; }
        if (version >= 0x04010400 && _DFCEntrance2Identification == NULL) { functionName = DEF_DFCEntrance2Identification; goto label_error; }
        if (version >= 0x04010500 && _DFCEntrance2OnlineAction == NULL) { functionName = DEF_DFCEntrance2OnlineAction; goto label_error; }
        if (version >= 0x04030403 && _DFCAccessControlIdentification == NULL) { functionName = DEF_DFCAccessControlIdentification; goto label_error; }
        if (version >= 0x04030403 && _DFCAccessControlOnlineAction == NULL) { functionName = DEF_DFCAccessControlOnlineAction; goto label_error; }
        if (version >= 0x04030500 && _DFCAccessControlKnobCommand == NULL) { functionName = DEF_DFCAccessControlKnobCommand; goto label_error; }
        // ########################################################
        // ####### FUNKTIONEN FUER FINGERPRINT ####################
        // ########################################################
        if (version >= 0x04010400 && _DFCFingerprintAppendRecord == NULL) { functionName = DEF_DFCFingerprintAppendRecord; goto label_error; }
        if (version >= 0x04010400 && _DFCFingerprintGetRecord == NULL) { functionName = DEF_DFCFingerprintGetRecord; goto label_error; }
        if (version >= 0x04010400 && _DFCFingerprintDeleteRecord == NULL) { functionName = DEF_DFCFingerprintDeleteRecord; goto label_error; }
        if (version >= 0x04010700 && _DFCFingerprintBackup == NULL) { functionName = DEF_DFCFingerprintBackup; goto label_error; }
        if (version >= 0x04010700 && _DFCFingerprintRestore == NULL) { functionName = DEF_DFCFingerprintRestore; goto label_error; }
        // ########################################################
        // ####### FUNKTIONEN FUER TIMEBOYLISTEN ##################
        // ########################################################
        if (version >= 0x04010400 && _DFCMakeTimeboyList == NULL) { functionName = DEF_DFCMakeTimeboyList; goto label_error; }
        if (version >= 0x04010400 && _DFCLoadTimeboyList == NULL) { functionName = DEF_DFCLoadTimeboyList; goto label_error; }
        if (version >= 0x04010400 && _DFCClearTimeboyListBuffer == NULL) { functionName = DEF_DFCClearTimeboyListBuffer; goto label_error; }
        // ########################################################
        // ######### FUNKTIONEN FUER AKTIVE VERBINDUNG ############
        // ########################################################
        if (version >= 0x04010500 && _DFCStartActiveConnection == NULL) { functionName = DEF_DFCStartActiveConnection; goto label_error; }
        if (version >= 0x04010500 && _DFCStopActiveConnection == NULL) { functionName = DEF_DFCStopActiveConnection; goto label_error; }
        if (version >= 0x04010500 && _DFCGetFirstActiveChannelID == NULL) { functionName = DEF_DFCGetFirstActiveChannelID; goto label_error; }
        if (version >= 0x04010500 && _DFCGetNextActiveChannelID == NULL) { functionName = DEF_DFCGetNextActiveChannelID; goto label_error; }
        if (version >= 0x04010500 && _DFCGetInfoActiveChannel == NULL) { functionName = DEF_DFCGetInfoActiveChannel; goto label_error; }
        if (version >= 0x04010500 && _DFCSetRecordAvailable == NULL) { functionName = DEF_DFCSetRecordAvailable; goto label_error; }
        if (version >= 0x04010500 && _DFCRecordAvailable == NULL) { functionName = DEF_DFCRecordAvailable; goto label_error; }
        if (version >= 0x04010500 && _DFCBindDeviceToChannel == NULL) { functionName = DEF_DFCBindDeviceToChannel; goto label_error; }
        // ########################################################
        // ############# ** PRIVATE FUNKTIONEN ** #################
        // ########################################################
#ifdef WITH_DLLGETVERSION
        if (version >= 0x02000000 && _DllGetVersion == NULL) { functionName = DEF_DllGetVersion; goto label_error; }
#endif
        if (version >= 0x02000200 && _DFCComVersion == NULL) { functionName = DEF_DFCComVersion; goto label_error; }
        if (version >= 0x02000400 && _DFCCheckData == NULL) { functionName = DEF_DFCCheckData; goto label_error; }
        if (version >= 0x03020000 && _DFCGetClass == NULL) { functionName = DEF_DFCGetClass; goto label_error; }

#if defined(_DEBUG) && defined(DATAFOX_STUDIO_IV)
        // ########################################################
        // ########### FUNKTIONEN FUER PRODUKTION #################
        // ########################################################
        if (version >= 0x04020500 && _DFPReadHardwareInfo == NULL) { functionName = DEF_DFPReadHardwareInfo; goto label_error; }
        if (version >= 0x04020500 && _DFPWriteHardwareInfo == NULL) { functionName = DEF_DFPWriteHardwareInfo; goto label_error; }
        if (version >= 0x04020500 && _DllGetProtectionKey == NULL) { functionName = DEF_DllGetProtectionKey; goto label_error; }
        if (version >= 0x04020500 && _DllUnlockProtection == NULL) { functionName = DEF_DllUnlockProtection; goto label_error; }
#endif

        res = 1;
    } else {

        m_errorText = QString::fromUtf8("Die DLL - Datei [ %1 ] wurde nicht gefunden.").arg(QString::fromUtf8(DFCOMDLL_NAME));
    }

label_error:
    if (functionName.length()) {

        m_errorText = QString::fromUtf8("In der DLL - Datei [ %1 ] trat folgender Fehler ein:\n\nFehlender Einsprungspunkt: %2").arg(QString::fromUtf8(DFCOMDLL_NAME)).arg(functionName);
    }

    return res;
}

// Lesen der Versionsinformation aus der Library
int DFComDLL::GetVersion( SoftwarebuildVersion &version )
{
    char ver[64];
    memset( ver, 0, sizeof(ver) );

    if ( !DFCVersionLibrary( ver, sizeof(ver) ) )
        return 0;

    // Ermittelte Version aus der Resource in Versionsobjekt wandeln.
    version.reset();
    return version.parse( ver );
}
