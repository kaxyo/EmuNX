namespace EmuNX.Lib.MetadataParserNX.Parser;

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ns;
using LibHac.Settings;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.Ncm;
using LibHac.Util;
using ContentType = LibHac.Ncm.ContentType;

/// <summary>
/// <para>
/// Provides functionality to parse metadata from ROM files (XCI/NSP),
/// this includes TitleID, Name, and Icon, regardless of language.
/// </para>
/// <para>
/// This class is designed for efficient ROM parsing by avoiding redundant
/// operations. For example, decryption keys are loaded only once and then
/// reused to analyze multiple ROMs without the need to reload them again.
/// </para>
/// <example>
/// The parsing process goes like this: Load everything you need and/or
/// configure some parameters, then you Read the fields. Each Read methods
/// returns the value and updates the <see cref="RomMetadata"/> object.
/// <code>
/// var romMetadataParser = new RomMetadataParser();
/// // Minimum requirements
/// romMetadataParser.LoadKeys("~/switch/prod.keys");
/// romMetadataParser.LoadRootFsFromRom("~/switch/rom.xci"); // Overwrites this.RomMetadata
/// var rom = romMetadataParser.RomMetadata; // If you need this reference ALWAYS pick it here
/// // Read TitleID
/// romMetadataParser.LoadCnmt();
/// romMetadataParser.ReadId();
/// // Needed for both TitleName and TitleIcon
/// romMetadataParser.LoadControlNca();
/// // Read Name
/// romMetadataParser.LoadNacp();
/// romMetadataParser.ReadName();
/// // Read Icon
/// romMetadataParser.ReadIcon();
/// // Configure languages and overwrite
/// romMetadataParser.ConfigLanguageName = Language.Japanese;
/// romMetadataParser.ConfigLanguageIcon = Language.Japanese;
/// romMetadataParser.ReadName();
/// romMetadataParser.ReadIcon();
/// </code>
/// </example>
/// </summary>
public class RomMetadataParser
{
    // Result
    public RomMetadata RomMetadata { get; private set; } = new RomMetadata();
    // Configuration
    public Language ConfigLanguageName = Language.AmericanEnglish;
    public Language ConfigLanguageIcon = Language.AmericanEnglish;
    // Keys: prod.keys
    private KeySet _keyset;
    // RootFileSystem: Stores meta.cnmt.nca, control.nca, etc...
    private IFileSystem _rootFs;
    private LocalStorage _rootLocalStorage;
    // Cnmt: Metadata that is mostly IDs
    private Cnmt _cnmt;
    // Control: Name and icons
    private string _controlNcaIdHex = null;
    private IFileSystem _controlNcaFs = null;
    private UniqueRef<IFile> _controlNcaFile = new UniqueRef<IFile>();
    // Nacp: Name
    private ApplicationControlProperty _nacp;

    /// <summary>
    /// Reads all metadata from a ROM.
    /// </summary>
    /// <param name="romPath"></param>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadAndReadEverythingFromRom(string romPath)
    {
        RomMetadataParserError? error;

        if ((error = LoadRootFsFromRom(romPath)) != null) return error;
        if ((error = LoadCnmt()                ) != null) return error;
        if ((error = LoadControlNca()          ) != null) return error;
        if ((error = LoadNacp()                ) != null) return error;

        ReadId();
        ReadName();
        ReadIcon();

        return null;
    }

    #region Loading
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
        if (!CanLoadRootFsFromRom()) return RomMetadataParserError.RomReadDependenciesNotComplete;
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
        if (!CanLoadRootFsFromRom()) return RomMetadataParserError.RomReadDependenciesNotComplete;
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
    /// The next thing you can execute is <see cref="LoadControlNca()"/>.
    /// </summary>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadCnmt()
    {
        if (!CanLoadCnmt()) return RomMetadataParserError.CnmtReadDependenciesNotComplete;
        DisposeCnmt();

        // Find unique NCA with a CNMT file inside
        var cnmtNcaEntry = _rootFs
            .EnumerateEntries("*.cnmt.nca", SearchOptions.Default)
            .FirstOrDefault();
        if (cnmtNcaEntry == null) return RomMetadataParserError.CnmtNcaNotFound;

        // Read NCA stream
        using var cnmtNcaFile = new UniqueRef<IFile>();
        var result = _rootFs.OpenFile(ref cnmtNcaFile.Ref, (U8Span)cnmtNcaEntry.FullPath, OpenMode.All);
        if (result.IsFailure()) return RomMetadataParserError.CnmtNcaReadError;

        // Open NCA FileSystem
        var cnmtNca = new Nca(_keyset, cnmtNcaFile.Get.AsStorage());
        using var cnmtNcaFs = cnmtNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

        // Find the name of the CNMT file
        var cnmtEntry = cnmtNcaFs
            .EnumerateEntries("/", "*.cnmt")
            .FirstOrDefault();
        if (cnmtEntry == null) return RomMetadataParserError.CnmtNotFound;

        var cnmtPath = (U8Span)cnmtEntry.FullPath;
    
        // Read CNMT file stream
        using var cnmtFile = new UniqueRef<IFile>();
        result = cnmtNcaFs.OpenFile(ref cnmtFile.Ref, cnmtPath, OpenMode.Read);
        if (result.IsFailure()) return RomMetadataParserError.CnmtReadError;
        
        // Parse CNMT
        _cnmt = new Cnmt(cnmtFile.Get.AsStream());

        // Read control data
        CnmtContentEntry control = _cnmt.ContentEntries.FirstOrDefault(e => e.Type == ContentType.Control);
        _controlNcaIdHex = control?.NcaId.ToHexString().ToLower();

        return null;
    }

    /// <summary>
    /// Loads the Control NCA which stores "control.nacp" and icon.
    /// </summary>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadControlNca()
    {
        if (!CanLoadControlNca()) return RomMetadataParserError.ControlReadDependenciesNotComplete;
        DisposeControlNca();

        // Find Control NCA entry
        var controlNcaEntry = _rootFs
            .EnumerateEntries($"{_controlNcaIdHex}.nca", SearchOptions.Default)
            .FirstOrDefault();
        if (controlNcaEntry == null) return RomMetadataParserError.ControlNcaNotFound;

        // Read NCA stream
        _controlNcaFile = new UniqueRef<IFile>();
        var result = _rootFs.OpenFile(ref _controlNcaFile.Ref, (U8Span)controlNcaEntry.FullPath, OpenMode.All);
        if (result.IsFailure()) return RomMetadataParserError.ControlNcaReadError;

        // Open NCA FileSystem
        var controlNca = new Nca(_keyset, _controlNcaFile.Get.AsStorage());
        _controlNcaFs = controlNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

        return null;
    }

    /// <summary>
    /// Loads "control.nacp" which stores the ROM name and other data.
    /// </summary>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadNacp()
    {
        if (!CanLoadNacp()) return RomMetadataParserError.NacpReadDependenciesNotComplete;
        DisposeNacp();

        // Search for NACP file
        using var nacpFile = new UniqueRef<IFile>();
        var result = _controlNcaFs.OpenFile(ref nacpFile.Ref, (U8Span)"/control.nacp", OpenMode.Read);
        if (!result.IsSuccess()) return RomMetadataParserError.NacpNotFound;

        // Load NACP and transform it into object
        const int nacpSize = 0x4000;
        byte[] nacpBuffer = new byte[nacpSize];
        using (var nacpStream = nacpFile.Get.AsStream())
        {
            // Load NACP stream into buffer
            int bytesRead = nacpStream.Read(nacpBuffer, 0, nacpSize);
            if (bytesRead != nacpSize) return RomMetadataParserError.NacpReadError;

            // Transform NACP buffer into easy-to-read object with Marshall
            GCHandle handle = GCHandle.Alloc(nacpBuffer, GCHandleType.Pinned);
            try
            {
                _nacp = Marshal.PtrToStructure<ApplicationControlProperty>(handle.AddrOfPinnedObject());
            }
            catch (Exception)
            {
                return RomMetadataParserError.NacpParseError;
            }
            finally
            {
                handle.Free();
            }
        }

        return null;
    }
    #endregion

    #region Reading
    public ulong ReadId()
    {
        RomMetadata.Id = _cnmt.ApplicationTitleId;
        return RomMetadata.Id;
    }

    public string ReadName()
    {
        RomMetadata.Name = _nacp.Title.Items[ConfigLanguageName.GetEntryIndex()].NameString.ToString();
        return RomMetadata.Name;
    }
    
    public Stream ReadIcon()
    {
        // Avoid null pointer
        if (_controlNcaFs == null)
        {
            RomMetadata.Icon = null;
            return null;
        }

        // Load file
        using var iconFile = new UniqueRef<IFile>();
        var result = _controlNcaFs.OpenFile(ref iconFile.Ref, (U8Span)ConfigLanguageIcon.GetIconPath(), OpenMode.Read);

        // Copy Icon stream
        RomMetadata.Icon = result.IsSuccess() ? iconFile.Get.AsStream() : null;
        return RomMetadata.Icon;
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
        // Dispose nested elements
        DisposeCnmt();
        // New metadata
        RomMetadata = new RomMetadata();
        // Free memory
        _rootFs?.Dispose();
        _rootLocalStorage?.Dispose();
        // Clear references
        _rootFs = null;
        _rootLocalStorage = null;
    }

    private void DisposeCnmt()
    {
        // Dispose nested elements
        DisposeControlNca();
        // Clear references
        _cnmt = null;
        _controlNcaIdHex = null;
    }
    
    private void DisposeControlNca()
    {
        // Dispose nested elements
        DisposeNacp();
        // Free memory
        _controlNcaFs?.Dispose();
        _controlNcaFile.Destroy();
        // Clear references
        _controlNcaFs = null;
        _controlNcaFile = new UniqueRef<IFile>();
    }
    
    private void DisposeNacp()
    {
        // Clear references
        _nacp = new ApplicationControlProperty();
    }
    #endregion

    #region Dependency checks
    public bool CanLoadRootFsFromRom()
    {
        return _keyset != null;
    }
    
    public bool CanLoadCnmt()
    {
        return CanLoadRootFsFromRom() && _rootFs != null;
    }
        
    public bool CanLoadControlNca()
    {
        return CanLoadCnmt() && _controlNcaIdHex != null;
    }
            
    public bool CanLoadNacp()
    {
        return CanLoadControlNca() && _controlNcaFs != null;
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