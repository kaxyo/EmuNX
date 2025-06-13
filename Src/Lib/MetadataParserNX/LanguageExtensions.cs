using System.Collections.Generic;
using Language = LibHac.Settings.Language;

/// <summary>
/// Provides extension methods for the <see cref="Language"/> enum that helps to retrieve rom icon and title.
/// </summary>
public static class LanguageExtensions
{
    #region Icon
    private static readonly Dictionary<Language, string> iconPaths = new()
    {
        { Language.AmericanEnglish     , "/icon_AmericanEnglish.dat"     },
        { Language.BrazilianPortuguese , "/icon_BrazilianPortuguese.dat" },
        { Language.BritishEnglish      , "/icon_BritishEnglish.dat"      },
        { Language.CanadianFrench      , "/icon_CanadianFrench.dat"      },
        { Language.Dutch               , "/icon_Dutch.dat"               },
        { Language.French              , "/icon_French.dat"              },
        { Language.German              , "/icon_German.dat"              },
        { Language.Italian             , "/icon_Italian.dat"             },
        { Language.Japanese            , "/icon_Japanese.dat"            },
        { Language.Korean              , "/icon_Korean.dat"              },
        { Language.LatinAmericanSpanish, "/icon_LatinAmericanSpanish.dat"},
        { Language.Portuguese          , "/icon_Portuguese.dat"          },
        { Language.Russian             , "/icon_Russian.dat"             },
        { Language.SimplifiedChinese   , "/icon_SimplifiedChinese.dat"   },
        { Language.Spanish             , "/icon_Spanish.dat"             },
        { Language.TraditionalChinese  , "/icon_TraditionalChinese.dat"  },
        { Language.Taiwanese           , "/icon_TraditionalChinese.dat"  }
    };

    /// <summary>
    /// Gets the icon path associated with the specified language.
    /// </summary>
    /// <param name="language">The language for which to get the icon path.</param>
    /// <returns>The icon path as a string.</returns>
    public static string GetIconPath(this Language language)
    {
        return iconPaths[language];
    }
    #endregion

    #region Indeces
    private static readonly Dictionary<Language, int> entryIndeces = new()
    {
        { Language.AmericanEnglish     , 0  },
        { Language.BritishEnglish      , 1  },
        { Language.Japanese            , 2  },
        { Language.French              , 3  },
        { Language.German              , 4  },
        { Language.LatinAmericanSpanish, 5  },
        { Language.Spanish             , 6  },
        { Language.Italian             , 7  },
        { Language.Dutch               , 8  },
        { Language.CanadianFrench      , 9  },
        { Language.Portuguese          , 10 },
        { Language.BrazilianPortuguese , 10 },
        { Language.Russian             , 11 },
        { Language.Korean              , 12 },
        { Language.TraditionalChinese  , 13 },
        { Language.Taiwanese           , 13 },
        { Language.SimplifiedChinese   , 14 }
    };

    /// <summary>
    /// Gets the entry index associated with the specified language.
    /// This index is used to access ApplicationControlProperty::Title
    /// </summary>
    /// <param name="language">The language for which to get the entry index.</param>
    /// <returns>The entry index as an integer.</returns>
    public static string GetEntryIndex(this Language language)
    {
        return iconPaths[language];
    }
    #endregion
}