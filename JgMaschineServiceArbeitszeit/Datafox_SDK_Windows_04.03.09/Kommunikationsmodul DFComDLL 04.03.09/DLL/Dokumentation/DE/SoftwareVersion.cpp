#include "SoftwareVersion.h"
#include <locale>
#include <boost/format.hpp>
#include <boost/lexical_cast.hpp>

TypeDeviceNameValue::TypeDeviceNameValue()
{
    // Inhalt der Map ist: map<lower(Name), pair<Wert, Name> >
    auto makeDeviceNameValue = [this](const std::string &name, int val) -> std::pair<std::string, std::pair<std::string, int> > 
    {
        return std::make_pair(toLower(name), std::make_pair(name, val));
    };
    // Liste der Geraetebezeichnung zum Vergleich mittels Kleinbuchstaben gemappt auf die Geraetetypnummer und abgesprochene Namensdarstellung.
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_PZE, VF_DEVICE_PZE));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_ZK, VF_DEVICE_ZK));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_BDE, VF_DEVICE_BDE));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_MOBIL, VF_DEVICE_MOBIL));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_MDE, VF_DEVICE_MDE));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_AE, VF_DEVICE_AE));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_TIMEBOY, VF_DEVICE_TIMEBOY));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_EXKLUSIV, VF_DEVICE_EXKLUSIV));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_FLEX, VF_DEVICE_FLEX));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_TIMEBOY_MOBIL_PZE, VF_DEVICE_TIMEBOY_MOBIL_PZE));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_EVO_2_8,  VF_DEVICE_EVO_2_8));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_EVO_4_3, VF_DEVICE_EVO_4_3));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_IPC_EXTENSION_A, VF_DEVICE_IPC_EXTENSION_A));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_IPC_EXTENSION_B, VF_DEVICE_IPC_EXTENSION_B));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_IPC_EXTENSION_C, VF_DEVICE_IPC_EXTENSION_C));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_IO_BOX, VF_DEVICE_IO_BOX));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_ZK_BOX, VF_DEVICE_ZK_BOX));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_MOBIL_BOX, VF_DEVICE_MOBIL_BOX));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_TIMEBOY_TBDOCK, VF_DEVICE_TIMEBOY_TBDOCK));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_MOBIL_TBDOCK, VF_DEVICE_MOBIL_TBDOCK));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_ZK_KNOTEN, VF_DEVICE_ZK_KNOTEN));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_ZK_TERMINAL, VF_DEVICE_ZK_TERMINAL));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_SAMS_OCL, VF_DEVICE_SAMS_OCL));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_EVO_3_5, VF_DEVICE_EVO_3_5));
    typeDeviceNameValue_.insert(makeDeviceNameValue(VF_DEVICE_NAME_ZK_READER_PLUS, VF_DEVICE_ZK_READER_PLUS));
}

std::string TypeDeviceNameValue::deviceName(int type)
{
    std::string res;
    for(auto iter = typeDeviceNameValue_.begin(); iter != typeDeviceNameValue_.end(); ++iter)
    {
        if (type == iter->second.second)
        {
            res = iter->second.first;
            break;
        }
    }

    return res;
}

int TypeDeviceNameValue::deviceValue(const std::string &str)
{
    int res = VF_DEVICE_NA;
    if (typeDeviceNameValue_.count(toLower(str)))
        res = typeDeviceNameValue_[toLower(str)].second;

    return res;
}

std::string TypeDeviceNameValue::toLower(const std::string &str)
{
    std::string res(str);
    for_each(res.begin(), res.end(), [](char &c){ c = std::tolower(c, std::locale()); });
    return res;
}

// Static
TypeDeviceNameValue SoftwareVersion::typeDeviceNameValue_;

SoftwareVersion::SoftwareVersion()
{
	reset();
}

SoftwareVersion::SoftwareVersion(const std::string &version)
{
    reset();
    if (parse(version) == false)
        reset();
}

SoftwareVersion::SoftwareVersion(const char *version)
{
	reset();
	if (parse(version) == false)
		reset();
}

SoftwareVersion::SoftwareVersion(unsigned int version)
{
	reset();
	if (parse(version) == false)
		reset();
}

SoftwareVersion::SoftwareVersion(unsigned char master, char major, unsigned char minor, unsigned char build, unsigned char typeDevice)
{
	std::string typeDeviceText;
	reset();

	if (master > 99)
        master = 99;

	master_ = master;
	
    if (major > 99)
        major = 99;

	major_ = major;
	
    if (minor > 99)
        minor = 99;
	
    minor_ = minor;
	
    if (build > 99)
        build = 99;
	
    build_ = build;
    
    if (typeDevice != VF_DEVICE_NA)
        typeDeviceText = typeDeviceNameValue_.deviceName(typeDevice);
    
    typeDevice_ = typeDevice;
    if (typeDevice == VF_DEVICE_NA)
        version_ = boost::str(boost::format("%02d.%02d.%02d.%02d") % static_cast<int>(master_) % static_cast<int>(major_) % static_cast<int>(minor_) % static_cast<int>(build_));
    else
        version_ = boost::str(boost::format("%02d.%02d.%02d.%02d.%s") % static_cast<int>(master_) % static_cast<int>(major_) % static_cast<int>(minor_) % static_cast<int>(build_) % typeDeviceText);
}

SoftwareVersion::~SoftwareVersion()
{
}

void SoftwareVersion::reset()
{
    master_ = major_ = minor_ = build_ = 0;
    typeDevice_ = 0;
    version_ = boost::str(boost::format("%02d.%02d.%02d.%02d") % static_cast<int>(master_) % static_cast<int>(major_) % static_cast<int>(minor_) % static_cast<int>(build_));
}

bool SoftwareVersion::parse(const std::string &version)
{
	// Formatvorlage kurz: XX.XX.XX???
	// Formatvorlage lang: XX.XX.XX.XX.YYYYYYY
    auto notDigit = [](char c){ return (std::isdigit(c, std::locale()) == false); };
	if (version.size() < 8 
        || notDigit(version[0]) || notDigit(version[1]) || version[2] != '.' 
        || notDigit(version[3]) || notDigit(version[4]) || version[5] != '.'
        || notDigit(version[6]) || notDigit(version[7]) 
        || (version.size() > 8 && (version[8] != '.' || notDigit(version[9]) || notDigit(version[10]))) )
    {
		// Formatfehler
		return false;
	}

	// Kopie des Versionsstrings halten.
	version_ = version;

	int i;
	// Alle Werte übernehmen
	i = 0;
	master_ = (version[i] - '0') * 10 + (version[i + 1] - '0');

	i += 3;
	major_  = (version[i] - '0') * 10 + (version[i + 1] - '0');

	i += 3;
	minor_  = (version[i] - '0') * 10 + (version[i + 1] - '0');

	build_ = 0;
	if (version.size() > 8)
	{	
		i += 3;
		build_  = (version[i] - '0') * 10 + (version[i + 1] - '0');
	}

	typeDevice_ = VF_DEVICE_NA;
	// Ermittlung des Geraetetyps?
	if (version.size() > 12)
        typeDevice_ = typeDeviceNameValue_.deviceValue(&version[12]);

	return true;
}

bool SoftwareVersion::parse(unsigned int version)
{
	if ((((version >> 24) & 0xFF) > 99)
        || (((version >> 16) & 0xFF) > 99)
        || (((version >>  8) & 0xFF) > 99)
        || ((version & 0xFF) > 99))
	{
		// Formatfehler
		return false;
	}

	// Wert in Member uebernehmen
	master_ = (version >> 24) & 0xFF;
	major_  = (version >> 16) & 0xFF;
	minor_  = (version >>  8) & 0xFF;
	build_  = version & 0xFF;
	typeDevice_ = VF_DEVICE_NA;

	version_ = boost::str(boost::format("%02d.%02d.%02d.%02d") % static_cast<int>(master_) % static_cast<int>(major_) % static_cast<int>(minor_) % static_cast<int>(build_));

	return true;
}
