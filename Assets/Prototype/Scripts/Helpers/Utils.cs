using UnityEngine;
using System.Collections.Generic;
using Spine.Unity;

namespace BSH_Prototype
{

	static public class Utils
	{

		public static bool IsMobilePlatform ()
		{
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_WP8_1 || UNITY_WSA
			return true;
#else
			return false;
#endif
		}


		public static Languages.Language GetSystemLanguage ()
		{
			Languages.Language res = Languages.Language.English;
#if UNITY_ANDROID
			res = (Languages.Language) System.Enum.Parse ( typeof ( Languages.Language ), GetAndroidDisplayLanguage ( ), true );
#else
			res = (Languages.Language) System.Enum.Parse ( typeof ( Languages.Language ), Application.systemLanguage.ToString ( ), true );
#endif
			return res;

		}


		private static List<string>
			muslim_locales = new List<string> ( )
			{
			"Arabic",
			"Albanian",
			"Bengali",
			"Indonesian",
			"Kyrgyz",
			"Kazakh",
			"Sinhala",
			"Turkish",
			};



		public static bool IsMuslim ()
		{
			return muslim_locales.IndexOf ( GetAndroidDisplayLanguage ( ) ) >= 0;
		}





		public static string GetAndroidDisplayLanguage ()
		{
#if UNITY_EDITOR
			return Application.systemLanguage.ToString ( );
#elif UNITY_ANDROID
        AndroidJavaClass localeClass = new AndroidJavaClass ( "java/util/Locale" );
        AndroidJavaObject defaultLocale = localeClass.CallStatic<AndroidJavaObject> ( "getDefault" );
        AndroidJavaObject usLocale = localeClass.GetStatic<AndroidJavaObject> ( "US" );
        string systemLanguage = defaultLocale.Call<string> ( "getDisplayLanguage", usLocale );
        Debug.Log ( "Android language is " + systemLanguage + " detected as " + systemLanguage );
        return systemLanguage;
#else
        return "";
#endif
		}


		public static string ConvertToString (this string [] arr)
		{
			string res = "Length = " + arr.Length + "\n";
			for (int i = 0; i < arr.Length; i++)
			{
				res += i + ":\t" + arr [i] + "\n";
			}
			return res;
		}

		public static string ConvertToString<T> (this T [] arr)
		{
			string res = "";
			for (int i = 0; i < arr.Length; i++)
			{
				res += i + ":\t" + arr [i].ToString ( ) + "\n";
			}
			return res;
		}

		public static string ConvertToString<T> (this Dictionary<T, string> dic)
		{
			string res = "count = " + dic.Count + "\n";

			foreach (var item in dic)
			{
				res += item.Key.ToString ( ) + ":\t\t" + item.Value.ToString ( ) + "\n";
			}
			return res;
		}

		public static Vector2 ConvertPoint (Vector2 poin, Camera Cam)
		{
			return Cam.ScreenToWorldPoint ( Camera.main.WorldToScreenPoint ( poin ) );
		}


		public static T ToEnum<T> (this string value)
		{
			return (T) System.Enum.Parse ( typeof ( T ), value, true );
		}


		static public T GetRandomEnum<T> ()
		{
			System.Array A = System.Enum.GetValues ( typeof ( T ) );
			T V = (T) A.GetValue ( UnityEngine.Random.Range ( 0, A.Length ) );
			return V;
		}

		static public bool RandomBool ()
		{
			return Random.value > 0.5f;
		}

		static public List<T> Shuffle<T> (this List<T> list)
		{
			int n = list.Count;
			List<T> new_list = list.CopyList ( );
			while (n > 0)
			{
				int k = Random.Range ( 0, n );
				T item = new_list [n - 1];
				new_list [n - 1] = new_list [k];
				new_list [k] = item;
				n--;
			}
			return new_list;
		}

		static public T [] Shuffle<T> (this T [] array)
		{
			int n = array.Length;
			T [] new_array = array.CopyArray ( );
			while (n > 0)
			{
				int k = Random.Range ( 0, n );
				T item = new_array [n - 1];
				new_array [n - 1] = new_array [k];
				new_array [k] = item;
				n--;
			}
			return new_array;
		}

		static public List<T> CopyList<T> (this List<T> list)
		{
			int n = list.Count;
			List<T> new_list = new List<T> ( );
			;
			for (int i = 0; i < n; i++)
			{
				new_list.Add ( list [i] );
			}
			return new_list;
		}

		static public T [] CopyArray<T> (this T [] array)
		{
			int n = array.Length;
			T [] new_array = new T [n];
			for (int i = 0; i < n; i++)
			{
				new_array [i] = array [i];
			}
			return new_array;
		}

		static public T GetRandomElement<T> (this List<T> list)
		{
			return list [Random.Range ( 0, list.Count )];
		}


		static public T GetRandomElement<T> (this T [] array)
		{
			return array [Random.Range ( 0, array.Length )];
		}

        static public List<T> GetRandomElements<T>(this List<T> list, int count)
        {
            int n = list.Count;
            List<T> new_list = new List<T>(count);
            while (count > 0)
            {
                int k = Random.Range(0, n);
                T item = new_list[n - 1];
                new_list[n - 1] = new_list[k];
                new_list[k] = item;
                n--;
                count--;
            }

            return new_list;
        }

        static public T[] GetRandomElements<T>(this T[] array, int count)
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < array.Length; i++)
                temp.Add(i);

            T[] newArray = new T[count];
            int random;
            for (int i = 0; i < count; i++)
            {
                random = Random.Range(0, temp.Count);
                newArray[i] = array[temp[random]];
                temp.RemoveAt(random);
            }

            return newArray;
        }

        static public string GetUID ()
		{
			string UID = System.Guid.NewGuid ( ).ToString ( );
			if (PlayerPrefs.HasKey ( "UID" ))
			{
				UID = PlayerPrefs.GetString ( "UID" );
			}
			else
			{
				PlayerPrefs.SetString ( "UID", UID );
				PlayerPrefs.Save ( );
			}
			return UID;
		}

		static public void SetSpineAnimation (this SkeletonAnimation anim, string clip1, string clip2 = "", bool delay = false, float from = 0, float to = 0)
		{
			if (anim != null)
			{
				if (anim.state == null)
				{
					anim.Initialize ( false );
				}
				if (delay)
				{
					DG.Tweening.DOVirtual.DelayedCall ( Random.Range ( from, to ), () => SetSpineAnimation ( anim, clip1, clip2, false ) );
				}
				else
				{
					if (clip1 != "")
					{
						anim.state.SetAnimation ( 0, clip1, clip2 == "" );
						if (clip2 != "")
						{
							anim.state.AddAnimation ( 0, clip2, true, 0 );
						}
					}
				}
			}
			else
			{
				Debug.Log ( "SetAnimation: Anim is null" );
			}
		}
	}

	public static class WeightedRandom
	{
		public static float [] CalcLookups (float [] weights)
		{
			float total_weight = 0;
			for (int i = 0; i < weights.Length; i++)
			{
				total_weight += weights [i];
			}
			float [] lookups = new float [weights.Length];
			for (int i = 0; i < weights.Length; i++)
			{
				lookups [i] = (weights [i] / total_weight) + (i == 0 ? 0 : lookups [i - 1]);
			}
			return lookups;
		}

		private static int binary_search (float needle, float [] lookups)
		{
			int high = lookups.Length - 1;
			int low = 0;
			int probe = 0;
			if (lookups.Length < 2)
			{
				return 0;
			}
			while (low < high)
			{
				probe = (int) ((high + low) / 2);

				if (lookups [probe] < needle)
				{
					low = probe + 1;
				}
				else if (lookups [probe] > needle)
				{
					high = probe - 1;
				}
				else
				{
					return probe;
				}
			}

			if (low != high)
			{
				return probe;
			}
			else
			{
				return (lookups [low] >= needle) ? low : low + 1;
			}
		}


		public static int RandomW (float [] weights)
		{
			float [] lookups = CalcLookups ( weights );

			return PrecalculatedRandomW ( lookups );
		}


		public static int PrecalculatedRandomW (float [] lookups) //used for getting random on the same set to optimise time by excludinc CalcLookups from each cycle of getting random
		{
			if (lookups.Length > 0)
				return binary_search ( Random.value, lookups );
			else
				return -1;
		}

	}
}