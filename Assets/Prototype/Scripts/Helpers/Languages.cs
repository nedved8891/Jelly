#define BRAND_NEW_PROTOTYPE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BSH_Prototype;

public class Languages :MonoBehaviour
{
    private static int locale_index;
    private static string locale_name;
    private static Language locale_lang;

    public delegate void Callback (Language lang);
    public static event Callback OnLangChange;

    public enum Language
    {
        Arabic = SystemLanguage.Arabic,
        Chinese = SystemLanguage.Chinese,
        Czech = SystemLanguage.Czech,
        Danish = SystemLanguage.Danish,
        Dutch = SystemLanguage.Dutch,
        English = SystemLanguage.English,
        Finnish = SystemLanguage.Finnish,
        French = SystemLanguage.French,
        German = SystemLanguage.German,
        Greek = SystemLanguage.Greek,
        Italian = SystemLanguage.Italian,
        Japanese = SystemLanguage.Japanese,
        Norwegian = SystemLanguage.Norwegian,
        Polish = SystemLanguage.Polish,
        Portuguese = SystemLanguage.Portuguese,
        Romanian = SystemLanguage.Romanian,
        Russian = SystemLanguage.Russian,
        Spanish = SystemLanguage.Spanish,
        Swedish = SystemLanguage.Swedish,
        Turkish = SystemLanguage.Turkish,
        Hindi = 50,
        Hebrew = SystemLanguage.Hebrew,
        Indonesian = SystemLanguage.Indonesian,
        Korean = SystemLanguage.Korean,
        Thai = SystemLanguage.Thai,
        Ukrainian = SystemLanguage.Ukrainian,
        Catalan = SystemLanguage.Catalan,
        Belarusian = SystemLanguage.Belarusian,
    };



    public static readonly Dictionary<Language, Language []> languages_override = new Dictionary<Language, Language []> ( )
    {
        { Language.Russian, new Language[] {Language.Ukrainian, Language.Belarusian } }, //"key" will replace any item from "value"
        { Language.Spanish , new Language[] {Language.Catalan } }, //"key" will replace any item from "value"
    };


    public static readonly Dictionary<Language, string> languages = new Dictionary<Language, string> ( ) {
		//{ Language.Arabic, "Arabic" },
		//{ Language.Chinese, "Chinese" },
		//{ Language.Czech, "Czech" },
		//{ Language.Danish, "Danish" },
		//{ Language.Dutch, "Dutch" },
		{ Language.English, "English" },
		//{ Language.Finnish, "Finnish" },
//		{ Language.French, "French" },
        //{ Language.German, "German" },
		//{ Language.Greek, "Greek" },
		//{ Language.Hebrew, "Hebrew" },
		//{ Language.Hindi, "Hindi" },
		//{ Language.Indonesian, "Indonesian" },
		//{ Language.Italian, "Italian" },
		//{ Language.Japanese, "Japanese" },
		//{ Language.Korean, "Korean" },
		//{ Language.Norwegian, "Norwegian" },
		//{ Language.Polish, "Polish" },
//		{ Language.Portuguese, "Portuguese" },
		//{ Language.Romanian, "Romanian" },
		{ Language.Russian, "Russian" },
//        { Language.Spanish, "Spanish" },
		//{ Language.Swedish, "Swedish" },
		//{ Language.Thai, "Thai" },
		//{ Language.Turkish, "Turkish" },
		//{ Language.Ukrainian, "Ukrainian" },
    };

    public static readonly string [] Localization = {
        "Arabic",
        "Chinese",
        "Czech",
        "Danish",
        "Dutch",
        "English",
        "Finnish",
        "French",
        "German",
        "Greek",
        "Hebrew",
        "Hindi",
        "Indonesian",
        "Italian",
        "Japanese",
        "Korean",
        "Norwegian",
        "Polish",
        "Portuguese",
        "Romanian",
        "Russian",
        "Spanish",
        "Swedish",
        "Thai",
        "Turkish",
        "Ukrainian"
    };


    static void CheckOverrides (ref Language param)
    {
        foreach (KeyValuePair<Language, Language []> rule in languages_override)
        {
            for (int i = 0; i < rule.Value.Length; i++)
            {
                if (rule.Value [i].Equals ( param ))
                {
                    param = rule.Key;
                    break;
                }
            }
        }
    }


    public static void Init ()
    {
        Language lang = Language.English;
#if BRAND_NEW_PROTOTYPE
        LanguageAudio.Init ( );
        if (GameSettings.GetCurrentLang ( ) >= 0)
        {
            lang = (Language) GameSettings.GetCurrentLang ( );
        }
#else
        if (PlayerPrefs.HasKey ("CurrentLanguage")) {
			lang = (Language)PlayerPrefs.GetInt ("CurrentLanguage");
		} 
#endif
        else
        {
            lang = DetectLanguage ( );
        }
        SetLanguage ( lang );
    }






    private static Language DetectLanguage ()
    {
        Language lang = Utils.GetSystemLanguage ( );
        CheckOverrides ( ref lang );
        if (languages.ContainsKey ( lang ))
        {
            return lang;
        }
        else
        {
            return Language.English;
        }
    }

    public static void SetLanguageByName (string lname)
    {
        foreach (KeyValuePair<Language, string> pair in languages)
        {
            if (pair.Value == lname)
            {
                SetLanguage ( pair.Key );
                break;
            }
        }
    }

    public static void SetLanguage (Language lang)
    {
        if (languages.ContainsKey ( lang ))
        {
            string name = languages [lang];
            for (int i = 0; i < Localization.Length; i++)
            {
                if (Localization [i] == name)
                {
                    locale_index = i;
                    locale_name = Localization [i];
                    locale_lang = lang;
                    break;
                }
            }
#if BRAND_NEW_PROTOTYPE
            GameSettings.SetCurrentLang ( (int) locale_lang );
            LanguageAudio.LoadSounds ( true, false );
#else
            PlayerPrefs.SetInt ( "CurrentLanguage", (int) locale_lang );
#endif
            if (OnLangChange != null)
            {
                OnLangChange ( locale_lang );
            }
        }
        else
        {
            Debug.LogError ( "Couldn't set langueage " + lang.ToString ( ) );
        }
    }

    public static void NextLanguage ()
    {
        locale_index++;
        if (locale_index > Localization.Length - 1)
        {
            locale_index = 0;
        }
        locale_name = Localization [locale_index];
    }

    public static string GetLanguageName ()
    {
        return locale_name;
    }

    public static Language GetLanguage ()
    {
        return locale_lang;
    }

}
