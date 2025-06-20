namespace EmuNX.Lib.MetadataParserNX.Parser;

using System;
using System.IO;
using System.Linq;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;

/// <summary>
/// Provides functionality to parse metadata from ROM files (XCI/NSP),
/// this includes TitleID, Name, and Icon, regardless of language.
/// This class is designed for efficient ROM parsing by avoiding redundant
/// operations. For example, decryption keys are loaded only once and then
/// reused to analyze multiple ROMs without the need to reload them again.
/// </summary>
public class RomMetadataParser
{
    // Keys: prod.keys
    private KeySet _keyset;
    // RootFileSystem: Stores meta.cnmt.nca, control.nca, etc...
    private IFileSystem _rootFs;
    private LocalStorage _rootLocalStorage;

    #region Parsing process
    /// <summary>
    /// Loads the encryption keys from prod.keys file.
    /// These are required to read the metadata.
    /// The next thing you can execute is <see cref="LoadRootFsFromRom(string)"/>.
    /// </summary>
    /// <param name="prodKeysPath">Path to the prod.keys file</param>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadKeys(string prodKeysPath)
    {
        DisposeKeys();

        // Check file existence
        if (!File.Exists(prodKeysPath)) return RomMetadataParserError.KeysProdNotFound;

        // Load keys
        _keyset = new KeySet();
        ExternalKeyReader.ReadKeyFile(_keyset, prodKeysPath, null, null, null);

        // Validate keys
        if (_keyset.HeaderKey.IsZeros()) return RomMetadataParserError.KeysProdInvalid;
        
        return null;
    }

    /// <summary>
    /// Tries to load the Root FileSystem from any rom file.
    /// The rom can be any XCI or NSP, and the Root FileSystem will be loaded
    /// with the adequate method.
    /// This Root FileSystem contains multiple NCAs like CNMT, CONTROL, etc...
    /// The next thing you can execute is <see cref="LoadCnmt()"/>.
    /// </summary>
    /// <param name="romPath">Path to the xci|nsp file</param>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadRootFsFromRom(string romPath)
    {
        var romExtension = romPath.Split(".").Last();

        return romExtension switch
        {
            "xci" => LoadRootFsFromXci(romPath),
            "nsp" => LoadRootFsFromNsp(romPath),
            _ => RomMetadataParserError.RomUnknownFormat
        };
    }
    
    #region Specific RootFs loading
    /// <summary>
    /// Tries to load the Root FileSystem from any XCI file.
    /// This Root FileSystem contains multiple NCAs like CNMT, CONTROL, etc...
    /// The next thing you can execute is <see cref="LoadCnmt()"/>.
    /// </summary>
    /// <param name="xciPath">Path to the xci file</param>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    /// <returns></returns>
    private RomMetadataParserError? LoadRootFsFromXci(string xciPath)
    {
        DisposeRootFs();

        _rootLocalStorage = OpenFileAsLocalStorage(xciPath);

        try
        {
            var xci = new Xci(_keyset, _rootLocalStorage);
            _rootFs = xci.OpenPartition(XciPartitionType.Secure);
            return null;
        }
        catch (Exception)
        {
            _rootFs = null;
            return RomMetadataParserError.XciLoadRootFsError;
        }
    }
    
    /// <summary>
    /// Tries to load the Root FileSystem from any NSP file.
    /// This Root FileSystem contains multiple NCAs like CNMT, CONTROL, etc...
    /// The next thing you can execute is <see cref="LoadCnmt()"/>.
    /// </summary>
    /// <param name="nspPath">Path to the nsp file</param>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    private RomMetadataParserError? LoadRootFsFromNsp(string nspPath)
    {
        DisposeRootFs();

        _rootLocalStorage = OpenFileAsLocalStorage(nspPath);

        using var pfs = new UniqueRef<PartitionFileSystem>();
        pfs.Reset(new PartitionFileSystem());
        var result = pfs.Get.Initialize(_rootLocalStorage);

        var success = result.IsSuccess();

        if (success)
        {
            _rootFs = pfs.Release();
            return null;
        }

        return RomMetadataParserError.NspLoadRootFsError;
    }
    #endregion

    /// <summary>
    /// Loads the cnmt file inside RootFs, also known as, PackagedContentMeta.
    /// This file stores the TitleID and NCA IDs. These NCA IDs are used as
    /// filename. 
    /// </summary>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadCnmt()
    {
        return RomMetadataParserError.Unknown;
    }
    #endregion

    #region Memory management
    private void DisposeKeys()
    {
        // Dispose nested elements
        DisposeRootFs();
        // Clear references
        _keyset = null;
    }
    
    private void DisposeRootFs()
    {
        // Free memory
        _rootFs?.Dispose();
        _rootLocalStorage?.Dispose();
        // Clear references
        _rootFs = null;
        _rootLocalStorage = null;
    }
    #endregion
    
    #region Utils
    private static LocalStorage OpenFileAsLocalStorage(string path)
    {
        try
        {
            return new LocalStorage(path, FileAccess.Read);
        }
        catch (Exception)
        {
            return null;
        }
    }
    #endregion
}