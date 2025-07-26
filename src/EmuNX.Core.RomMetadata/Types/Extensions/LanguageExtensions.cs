namespace EmuNX.Core.RomMetadata.Types.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Language"/> enum to retrieve ROM icons and names easily.
/// </summary>
public static class LanguageExtensions
{
    #region Icon
    private static readonly Dictionary<Language, string> IconPaths = new()
    {
        { Language.AmericanEnglish     , "/icon_AmericanEnglish.dat"     },
        { Language.BritishEnglish      , "/icon_BritishEnglish.dat"      },
        { Language.Japanese            , "/icon_Japanese.dat"            },
        { Language.French              , "/icon_French.dat"              },
        { Language.German              , "/icon_German.dat"              },
        { Language.LatinAmericanSpanish, "/icon_LatinAmericanSpanish.dat"},
        { Language.Spanish             , "/icon_Spanish.dat"             },
        { Language.Italian             , "/icon_Italian.dat"             },
        { Language.Dutch               , "/icon_Dutch.dat"               },
        { Language.CanadianFrench      , "/icon_CanadianFrench.dat"      },
        { Language.Portuguese          , "/icon_Portuguese.dat"          },
        { Language.BrazilianPortuguese , "/icon_BrazilianPortuguese.dat" },
        { Language.Russian             , "/icon_Russian.dat"             },
        { Language.Korean              , "/icon_Korean.dat"              },
        { Language.TraditionalChinese  , "/icon_TraditionalChinese.dat"  },
        { Language.Taiwanese           , "/icon_TraditionalChinese.dat"  },
        { Language.SimplifiedChinese   , "/icon_SimplifiedChinese.dat"   },
    };

    /// <summary>
    /// Gets the file path of the language-specific icon associated with the specified language.
    /// </summary>
    /// <param name="language">The language for which to get the icon path.</param>
    /// <returns>
    /// A string representing the path to the corresponding language icon file.
    /// For example, <c>"/icon_AmericanEnglish.dat"</c>.
    /// </returns>
    /// <remarks>
    /// This path typically points to a pre-defined language-specific icon asset inside Control NCA.
    /// All known language paths follow the naming convention <c>"/icon_{Language}.dat"</c>.
    /// Note that <see cref="Language.Taiwanese"/> is an alias of <see cref="Language.TraditionalChinese"/> and shares
    /// the same icon file.
    /// </remarks>
    public static string GetIconPath(this Language language)
    {
        return IconPaths[language];
    }
    #endregion

    #region Indeces
    /// <summary>
    /// Returns the index of the localized title entry for the given language, as stored in the NACP file of a ROM.
    /// </summary>
    /// <param name="language">The language for which to obtain the title entry index.</param>
    /// <returns>
    /// An integer representing the index of the language entry (e.g., <c>0</c> for American English, <c>13</c> for
    /// Traditional Chinese).
    /// </returns>
    /// <remarks>
    /// The NACP stores up to 16 title entries in 0x300-byte blocks, each containing localized application name and
    /// publisher strings.
    /// This index maps directly to those language-specific blocks, starting from offset <c>0x0</c>.
    /// </remarks>
    public static int GetNameTranslationIndex(this Language language)
    {
        return (int) language;
    }
    #endregion
}