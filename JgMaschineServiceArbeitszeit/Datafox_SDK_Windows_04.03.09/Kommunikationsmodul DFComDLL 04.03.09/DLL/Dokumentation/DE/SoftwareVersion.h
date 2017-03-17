#ifndef _SOFTWAREVERSION_
#define _SOFTWAREVERSION_

#ifdef _MSC_VER
#pragma warning(disable : 4996)
#endif

#if !defined(_MSC_VER) && !defined(__MINGW32__)
#define _T(x) x
#endif

#include <string>
#include <unordered_map>

#define VF_DEVICE_NA                   0xFF
#define VF_DEVICE_PZE                  0
#define VF_DEVICE_ZK                   1
#define VF_DEVICE_BDE                  2
#define VF_DEVICE_MOBIL                3
#define VF_DEVICE_MDE                  4
#define VF_DEVICE_AE                   5
#define VF_DEVICE_TIMEBOY              6
#define VF_DEVICE_EXKLUSIV             7
#define VF_DEVICE_FLEX                 8
#define VF_DEVICE_TIMEBOY_MOBIL_PZE    9
#define VF_DEVICE_EVO_2_8             10
#define VF_DEVICE_EVO_4_3             11
#define VF_DEVICE_IPC_EXTENSION_A     12
#define VF_DEVICE_IPC_EXTENSION_B     13
#define VF_DEVICE_IPC_EXTENSION_C     14
#define VF_DEVICE_IO_BOX              15
#define VF_DEVICE_ZK_BOX              16
#define VF_DEVICE_MOBIL_BOX           17
#define VF_DEVICE_TIMEBOY_TBDOCK      18
#define VF_DEVICE_MOBIL_TBDOCK        19
#define VF_DEVICE_ZK_KNOTEN           20
#define VF_DEVICE_ZK_TERMINAL         21
#define VF_DEVICE_SAMS_OCL            22
#define VF_DEVICE_EVO_3_5             23	
#define VF_DEVICE_ZK_READER_PLUS      24

#define IV_DEVICE_NAME_NA                "NA"
#define VF_DEVICE_NAME_PZE               "PZE"
#define VF_DEVICE_NAME_ZK                "ZK"
#define VF_DEVICE_NAME_BDE               "BDE"
#define VF_DEVICE_NAME_MOBIL             "MOBIL"
#define VF_DEVICE_NAME_MDE               "MDE"
#define VF_DEVICE_NAME_AE                "AE"
#define VF_DEVICE_NAME_TIMEBOY           "TIMEBOY"
#define VF_DEVICE_NAME_EXKLUSIV          "EXKLUSIV"
#define VF_DEVICE_NAME_FLEX              "Flex"
#define VF_DEVICE_NAME_TIMEBOY_MOBIL_PZE "TB-MOBIL-PZE"
#define VF_DEVICE_NAME_EVO_2_8           "Evo28"
#define VF_DEVICE_NAME_EVO_4_3           "Evo43"
#define VF_DEVICE_NAME_IPC_EXTENSION_A   "VARIO"
#define VF_DEVICE_NAME_IPC_EXTENSION_B   "IPC"
#define VF_DEVICE_NAME_IPC_EXTENSION_C   "Q7"
#define VF_DEVICE_NAME_IO_BOX            "IOBOX"
#define VF_DEVICE_NAME_ZK_BOX            "ZKBOX"
#define VF_DEVICE_NAME_MOBIL_BOX         "MOBILBOX"
#define VF_DEVICE_NAME_TIMEBOY_TBDOCK    "DockingV2"
#define VF_DEVICE_NAME_MOBIL_TBDOCK      "LoggerV2"
#define VF_DEVICE_NAME_ZK_KNOTEN         "ZKKNOTEN"
#define VF_DEVICE_NAME_ZK_TERMINAL       "ZKTERMINAL"
#define VF_DEVICE_NAME_SAMS_OCL          "SAMS-OCL"
#define VF_DEVICE_NAME_EVO_3_5           "EVO35"
#define VF_DEVICE_NAME_ZK_READER_PLUS    "ZKREADERPLUS"

class TypeDeviceNameValue
{
public:
    TypeDeviceNameValue();
    std::string deviceName(int type);
    int deviceValue(const std::string &str);

protected:
    std::string toLower(const std::string &str);

private:
    std::unordered_map<std::string, std::pair<std::string, int> > typeDeviceNameValue_;
};

class SoftwareVersion
{
public:
    SoftwareVersion();
    SoftwareVersion(const std::string &version);
    SoftwareVersion(const char *version);
    SoftwareVersion(unsigned int version);
    SoftwareVersion(unsigned char master, char major, unsigned char minor, unsigned char build, unsigned char typeDevice);
    virtual ~SoftwareVersion();
    friend class SoftwarebuildVersion;

private:
    static TypeDeviceNameValue typeDeviceNameValue_;
    std::string version_;
    unsigned char master_;
    unsigned char major_;
    unsigned char minor_;
    unsigned char build_;
    unsigned char typeDevice_;

public:
    void reset();
    bool parse(const std::string &version);
    bool parse(unsigned int version);

    inline unsigned char master()
    {
        return master_;
    }
    inline unsigned char major()
    {
        return major_;
    }
    inline unsigned char minor()
    {
        return minor_;
    }
    inline unsigned char build()
    {
        return build_;
    }
    inline unsigned char typeDevice()
    {
        return typeDevice_;
    }

public: // Zuweisungsoperator
    SoftwareVersion& operator= (const char *version)
    {
        if (this->parse(version) == false)
            this->reset();

        return *this;
    }
    SoftwareVersion& operator= (unsigned int version)
    {
        if (this->parse(version) == false)
            this->reset();

        return *this;
    }


public: // Vergleichsoperatoren
    // Gleichheit
    bool operator== (SoftwareVersion &version)
    {
        if (this->master_ == version.master_ &&
            this->major_ == version.major_ &&
            this->minor_ == version.minor_)
        {
            return true;
        }

        return false;
    }
    bool operator== (const char *version)
    {
        SoftwareVersion lVersion(version);
        return (*this == lVersion);
    }
    // Ungleichheit
    bool operator!= (SoftwareVersion &version)
    {
        if (this->master_ != version.master_ ||
            this->major_ != version.major_ ||
            this->minor_ != version.minor_)
        {
            return true;
        }

        return false;
    }
    bool operator!= (const char *version)
    {
        SoftwareVersion lVersion(version);
        return (*this != lVersion);
    }
    // Groesser
    bool operator> (SoftwareVersion &version)
    {
        if (this->master_ > version.master_ ||
            (this->master_ == version.master_ && this->major_ > version.major_) ||
            (this->master_ == version.master_ && this->major_ == version.major_ && this->minor_ > version.minor_))
        {
            return true;
        }

        return false;
    }
    bool operator> (const char *version)
    {
        SoftwareVersion lVersion(version);
        return (*this > lVersion);
    }
    // Groesser gleich
    bool operator>= (SoftwareVersion &version)
    {
        if (*this > version || *this == version)
        {
            return true;
        }

        return false;
    }
    bool operator>= (const char *version)
    {
        SoftwareVersion lVersion(version);
        return (*this >= lVersion);
    }
    // Kleiner
    bool operator< (SoftwareVersion &version)
    {
        if (this->master_ < version.master_ ||
            (this->master_ == version.master_ && this->major_ < version.major_) ||
            (this->master_ == version.master_ && this->major_ == version.major_ && this->minor_ < version.minor_))
        {
            return true;
        }

        return false;
    }
    bool operator< (const char *version)
    {
        SoftwareVersion lVersion(version);
        return (*this < lVersion);
    }
    // Kleiner gleich
    bool operator<= (SoftwareVersion &version)
    {
        if (*this < version || *this == version)
        {
            return true;
        }

        return false;
    }
    bool operator<= (const char *version)
    {
        SoftwareVersion lVersion(version);
        return (*this <= lVersion);
    }

public: // Cast operatoren
    operator const char* ()
    {
        return version_.c_str();
    }
    operator unsigned int ()
    {
        return static_cast<unsigned int>( (master_ << 24) | (major_ << 16) | (minor_ << 8) | build_ );
    }
};

class SoftwarebuildVersion : public SoftwareVersion
{
public:
    SoftwarebuildVersion() : SoftwareVersion() {

    }
    SoftwarebuildVersion(const std::string &version) : SoftwareVersion(version) {

    }
    SoftwarebuildVersion(const char *version) : SoftwareVersion(version) {

    }
    SoftwarebuildVersion(unsigned int version) : SoftwareVersion(version) {

    }
    SoftwarebuildVersion(unsigned char master, char major, unsigned char minor, unsigned char build, unsigned char typeDevice) : SoftwareVersion(master, major, minor, build, typeDevice) {
    }

public: // Vergleichsoperatoren
    // Gleichheit
    bool operator== (SoftwarebuildVersion &version)
    {
        if (this->master_ == version.master_ &&
            this->major_ == version.major_ &&
            this->minor_ == version.minor_ &&
            this->build_ == version.build_) {
                return true;
        }

        return false;
    }
    bool operator== (const char *version)
    {
        SoftwarebuildVersion lVersion(version);
        return (*this == lVersion);
    }
    // Ungleichheit
    bool operator!= (SoftwarebuildVersion &version)
    {
        if (this->master_ != version.master_ ||
            this->major_ != version.major_ ||
            this->minor_ != version.minor_ ||
            this->build_ != version.build_) {
                return true;
        }

        return false;
    }
    bool operator!= (const char *version)
    {
        SoftwarebuildVersion lVersion(version);
        return (*this != lVersion);
    }
    // Groesser
    bool operator> (SoftwarebuildVersion &version)
    {
        if (this->master_ > version.master_ ||
            (this->master_ == version.master_ && this->major_ > version.major_) ||
            (this->master_ == version.master_ && this->major_ == version.major_ && this->minor_ > version.minor_) ||
            (this->master_ == version.master_ && this->major_ == version.major_ && this->minor_ == version.minor_ && this->build_ > version.build_)) {
                return true;
        }

        return false;
    }
    bool operator> (const char *version)
    {
        SoftwarebuildVersion lVersion(version);
        return (*this > lVersion);
    }
    // Groesser gleich
    bool operator>= (SoftwarebuildVersion &version)
    {
        if (*this > version || *this == version)
        {
            return true;
        }

        return false;
    }
    bool operator>= (const char *version)
    {
        SoftwarebuildVersion lVersion(version);
        return (*this >= lVersion);
    }
    // Kleiner
    bool operator< (SoftwarebuildVersion &version)
    {
        if (this->master_ < version.master_ ||
            (this->master_ == version.master_ && this->major_ < version.major_) ||
            (this->master_ == version.master_ && this->major_ == version.major_ && this->minor_ < version.minor_) ||
            (this->master_ == version.master_ && this->major_ == version.major_ && this->minor_ == version.minor_ && this->build_ < version.build_)) {
                return true;
        }

        return false;
    }
    bool operator< (const char *version)
    {
        SoftwarebuildVersion lVersion(version);
        return (*this < lVersion);
    }
    // Kleiner gleich
    bool operator<= (SoftwarebuildVersion &version)
    {
        if (*this < version || *this == version)
        {
            return true;
        }

        return false;
    }
    bool operator<= (const char *version)
    {
        SoftwarebuildVersion lVersion(version);
        return (*this <= lVersion);
    }

};
#endif
