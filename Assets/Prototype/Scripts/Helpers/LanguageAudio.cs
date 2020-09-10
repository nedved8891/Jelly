using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class LanguageAudio :MonoBehaviour
{
	//	static public LanguageAudio Instance;

	private static Dictionary<string, AudioClip> nonlanguage_sounds;
	//private static Dictionary<string, Dictionary<string, AudioClip>> multilanguage_sounds;
	private static Dictionary<string, AudioClip> language_sounds;

	private static bool debug = false;

	private static string multilanguage_path = "Sounds/MultiLanguage/";
	private static string nonlanguage_path = "Sounds/NonLanguage/";

	//	private bool NonLang = false;

	//	public LodingTextController
	//		loader;


	//	void Awake ()
	//	{
	//		Instance = this;
	//	}
	//
	public static void Init ()
	{
		nonlanguage_sounds = new Dictionary<string, AudioClip> ( );
		//multilanguage_sounds = new Dictionary<string, Dictionary<string, AudioClip>>();
		language_sounds = new Dictionary<string, AudioClip> ( );
		//		NonLang = false;
		LoadSounds ( false, false );
	}

	public static void ReleaseLanguageSounds ()
	{
		language_sounds.Clear ( );
	}

	public static AudioClip GetSoundByName (string name, bool multilanguage = true)
	{
		AudioClip sound = null;
		//		if (multilanguage_sounds.ContainsKey (name)) {
		//			Dictionary<string, AudioClip> sounds = multilanguage_sounds[name];
		//			sound = sounds[Languages.Instance.GetLanguageName ()];

		name = name.ToLower ( );

		if (language_sounds != null && language_sounds.ContainsKey ( name ))
		{
			sound = language_sounds [name];
		}
		else if (nonlanguage_sounds != null && nonlanguage_sounds.ContainsKey ( name ))
		{
			sound = nonlanguage_sounds [name];
		}
		return sound;
	}

	private static string GetPath (string root, string name, string folder)
	{
		string path = root;
		if (folder != "")
		{
			path += folder + "/";
		}
		path += name;
		return path;
	}

	public static void LoadSounds (bool multilanguage = true, bool virt = true)
	{
		if (virt)
		{
			DOVirtual.DelayedCall ( 0, () => LoadSounds ( multilanguage, false ) );
			return;
		}
		//		loader.Init ();
		if (multilanguage)
		{
			ReleaseLanguageSounds ( );
			string lang = Languages.GetLanguageName ( );
			string path = multilanguage_path + lang + "/";
			//			Debug.LogWarning ("MESSAGE: Loading MultiLanguage from " + path);
			Object [] res = Resources.LoadAll ( path );
			for (int i = 0; i < res.Length; i++)
			{
				AudioClip sound = res [i] as AudioClip;
				StoreSound ( language_sounds, res [i].name, sound );
				if (debug && sound == null)
				{
					print ( path + "\t\t" + lang + "\t\t" + (sound) );
				}
			}
			//			if (!NonLang)
			//				LoadSounds (false);
		}
		else
		{
			Object [] res = Resources.LoadAll ( nonlanguage_path );
			for (int i = 0; i < res.Length; i++)
			{
				AudioClip sound = res [i] as AudioClip;
				StoreSound ( nonlanguage_sounds, res [i].name, sound );
				if (debug && sound == null)
				{
					print ( nonlanguage_path + "\t\t" + (sound) );
				}
			}
			//			NonLang = true;
		}
		//		loader.Release ();
	}


	public static void LoadSoundByName (string name, bool multilanguage = true, string folder = "")
	{
		if (multilanguage)
		{
			//			Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip> ();
			//			foreach (string lang in Languages.Instance.Localization) {
			//				string path = GetPath (multilanguage_path + lang + "/", name, folder);
			//				AudioClip sound = Resources.Load (path) as AudioClip;
			//				sounds.Add (lang, sound);
			//
			//				if (debug && sound == null) {
			//					print (path + "\t\t" + lang + "\t\t" + (sound));
			//				}
			//			}
			//			multilanguage_sounds.Add (name, sounds);

			string lang = Languages.GetLanguageName ( );
			string path = GetPath ( multilanguage_path + lang + "/", name, folder );
			AudioClip sound = Resources.Load ( path ) as AudioClip;
			StoreSound ( language_sounds, name, sound );

			if (debug && sound == null)
			{
				print ( path + "\t\t" + lang + "\t\t" + (sound) );
			}
		}
		else
		{
			string path = GetPath ( nonlanguage_path, name, folder );
			AudioClip sound = Resources.Load ( path ) as AudioClip;
			StoreSound ( nonlanguage_sounds, name, sound );

			if (debug && sound == null)
			{
				print ( name + "\t\t" + (sound != null) );
			}
		}
	}


	private static void StoreSound (Dictionary<string, AudioClip> dic, string name, AudioClip sound)
	{
		dic.Add ( name.ToLower ( ), sound );
	}
}
