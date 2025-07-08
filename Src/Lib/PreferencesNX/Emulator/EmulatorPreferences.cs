using System.Collections.Generic;
using System.IO;
using EmuNX.Lib.EmulatorManagerNX;

namespace EmuNX.Lib.PreferencesNX.Emulator;

public class EmulatorPreferences(EmulatorFamilies family, EmulatorProfilePreferences defaultProfile, List<EmulatorProfilePreferences> profiles)
{
    public EmulatorFamilies Family = family;
    public EmulatorProfilePreferences DefaultProfile = defaultProfile;
    public List<EmulatorProfilePreferences> Profiles = profiles;
}

public class EmulatorProfilePreferences (string name, FileInfo cliExecutablePath, FileInfo guiExecutablePath)
{
    public string Name = name;
    public FileInfo CliExecutablePath = cliExecutablePath;
    public FileInfo GuiExecutablePath = guiExecutablePath;
}