using EmuNX.Core.Common.Monad;

namespace EmuNX.Core.Configuration.TitleExecutionPetition.IO;

/// <summary>
/// Defines a contract for classes responsible for persisting and retrieving <see cref="TepConfig"/> 
/// instances from a given storage medium (e.g., file system, database).
/// </summary>
public interface ITepConfigStorage
{
    /// <summary>
    /// The version accepted by <see cref="Load"/> (based on <see cref="Version.IsCompatibleWith"/>) and stored by
    /// <see cref="Save"/>.
    /// </summary>
    Version VersionRequired { get; }

    /// <summary>
    /// Loads a <see cref="TepConfig"/> instance from the storage medium.
    /// </summary>
    /// <returns>
    /// A <see cref="Result{TSuccess, TError}"/> where 
    /// <typeparamref name="TSuccess"/> is <see cref="TepConfig"/> when the operation succeeds,
    /// and <typeparamref name="TError"/> is <see cref="TepConfigStorageError"/> when it fails.
    /// </returns>
    Result<TepConfig, TepConfigStorageError> Load();

    /// <summary>
    /// Saves the given <see cref="TepConfig"/> instance to the storage medium.
    /// </summary>
    /// <param name="config">The configuration object to persist.</param>
    /// <returns>
    /// A <see cref="TepConfigStorageError"/> if the save operation fails; 
    /// otherwise <c>null</c> when the operation succeeds.
    /// </returns>
    TepConfigStorageError? Save(TepConfig config);
}