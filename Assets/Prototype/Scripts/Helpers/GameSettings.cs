using UnityEngine;
using System.Collections;

public static class GameSettings
{
    private static bool actual = false;

    private static bool vibro_on = true;
    private static bool music_on = true;
    private static bool sounds_on = true;
    private static float music_vol = 1;
    private static float sounds_vol = 1;
    private static int current_lang = -1;

    public static void UpdateSettings ()
    {
        if (!actual)
        {
            vibro_on = PlayerPrefs.GetInt ( "vibro_on", 1 ) == 0 ? false : true;
            music_on = PlayerPrefs.GetInt ( "music_on", 1 ) == 0 ? false : true;
            sounds_on = PlayerPrefs.GetInt ( "sounds_on", 1 ) == 0 ? false : true;
            music_vol = PlayerPrefs.GetFloat ( "music_vol", 1 );
            sounds_vol = PlayerPrefs.GetFloat ( "sounds_vol", 1 );
            current_lang = PlayerPrefs.GetInt ( "CurrentLanguage", -1 );
            actual = true;
        }
    }


    public static bool IsVibroEnabled ()
    {
        return vibro_on;
    }

    public static void EnableVibro (bool param)
    {
        vibro_on = param;
        PlayerPrefs.SetInt ( "vibro_on", vibro_on == false ? 0 : 1 );
        SaveSettings ( );
    }


    public static bool IsMusicEnabled ()
    {
        return music_on;
    }

    public static void EnableMusic (bool param)
    {
        music_on = param;
        AudioController.EnableMusic ( music_on );
        PlayerPrefs.SetInt ( "music_on", (music_on == false ? 0 : 1) );
        SaveSettings ( );
    }


    public static bool IsSoundsEnabled ()
    {
        return sounds_on;
    }

    public static void EnableSounds (bool param)
    {
        sounds_on = param;
        AudioController.EnableSounds ( sounds_on );
        PlayerPrefs.SetInt ( "sounds_on", (sounds_on == false ? 0 : 1) );
        SaveSettings ( );
    }

    public static float GetMusicVol ()
    {
        return music_vol;
    }

    public static void SetMusicVol (float param)
    {
        music_vol = param;
        AudioController.SetMusicVolume ( music_vol );
        PlayerPrefs.SetFloat ( "music_vol", music_vol );
        SaveSettings ( );
    }


    public static float GetSoundsVol ()
    {
        return sounds_vol;
    }

    public static void SetSoundsVol (float param)
    {
        sounds_vol = param;
        AudioController.SetSoundsVolume ( sounds_vol );
        PlayerPrefs.SetFloat ( "sounds_vol", sounds_vol );
        SaveSettings ( );
    }


    public static int GetCurrentLang ()
    {
        return current_lang;
    }


    public static void SetCurrentLang (int lang)
    {
        current_lang = lang;
        PlayerPrefs.SetInt ( "CurrentLanguage", current_lang );
        SaveSettings ( );
    }


    public static void SaveSettings ()
    {
        PlayerPrefs.Save ( );
    }
}
