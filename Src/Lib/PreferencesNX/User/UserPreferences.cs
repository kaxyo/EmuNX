using System;
using System.Collections.Generic;
using EmuNX.Lib.EmulatorManagerNX;

namespace EmuNX.Lib.PreferencesNX.User;

/// <summary>
/// Stores some information about the virtual users.
/// Unlike normal users, i.e. those stored inside your emulator, virtual users are EmuNx's own and work as shortcut to
/// the normal ones. This improves user management across different emulator profiles.
/// </summary>
public class UserPreferences
{
    public Guid Id;
    public string Name;
    public Dictionary<EmulatorFamilies, Dictionary<string, string>> Relations; // i.e. Ryujinx { "profile": "user_id", ... }
}