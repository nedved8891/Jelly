using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Linq;

/* ChangeLog:
 * fixed pause method to take only sources playing right now
 * will clear list of paused sources on unPause (can override by parameter)
 * added OnPause event to support interation with custom controllers
 * refactored to avoid null sources
 * can auto-initialize
 * now all Play methods return audio length
 * ReleaseStreams >> ReleaseTweens
 * Pausing tweens
 * Fixed pause methods to collect paused sources every time pause is set to true and clear them when game unpaused (will help not loose paused sources after second pause true method will be called)
 * Complete refactor and logic change: now all streams are divided into groups and can be managed separately
 * can override volume level for each group
 */


public static class AudioController
{
	public static Action<bool> OnPause;

	/// <summary>
	/// List here all necessary stream groups to manage them separately. By default should have MUSIC and FX. The rest items can be optionaly added.
	/// </summary>
	public enum StreamGroup
	{
		FX,
		VOICE,
		MUSIC,
		//AMBIENT,
	}

	/// <summary>
	/// List here groups which should play one sound at a time (no simultaneous sounds will be played)
	/// </summary>
	private static List<StreamGroup> single_stream = new List<StreamGroup> {
		StreamGroup.VOICE,
		StreamGroup.MUSIC,
		//StreamGroup.AMBIENT,
	};

	/// <summary>
	/// List here streams, that will be muted with music and will use volume of music group
	/// </summary>
	private static List<StreamGroup> music_like_streams = new List<StreamGroup> ( )
	{
		StreamGroup.MUSIC,
		//StreamGroup.AMBIENT,
	};

	/// <summary>
	/// List here groups and corresponding volume levels for each of them (main_volume * group_volume). Used to simulate AudioMixer functionality.
	/// </summary>
	private static Dictionary<StreamGroup, float> group_volume = new Dictionary<StreamGroup, float> ( )
	{
		{ StreamGroup.FX, 0.8f },
		{ StreamGroup.VOICE, 1f },
		{ StreamGroup.MUSIC, 0.8f },
		//{ StreamGroup.AMBIENT, 0.8f },
	};

	//stream containers for groups
	private static Dictionary<StreamGroup, List<AudioSource>>
		streams = new Dictionary<StreamGroup, List<AudioSource>> ( ), //AudioSource pool to play sond without specifying any AudioSource
		user_streams = new Dictionary<StreamGroup, List<AudioSource>> ( ), //streams that user specifyied when played a sound
		paused_streams = new Dictionary<StreamGroup, List<AudioSource>> ( ); //streams that were paused

	//for playing Non/Language audio by name in a sequence
	private const string
		PAUSE = "||";

	//AudioSource pool holder
	private static GameObject
		SrcParrent;

	//tween containers for StreamGroup
	private static Dictionary<StreamGroup, List<Sequence>>
		tweens = new Dictionary<StreamGroup, List<Sequence>> ( ),
		paused_tweens = new Dictionary<StreamGroup, List<Sequence>> ( );

	//streams that are used to play a sequence of sounds
	private static List<AudioSource>
		locked_streams = new List<AudioSource> ( );

	//holder for available enum items
	private static StreamGroup []
		_available_groups = null;

	//field to get available enum items typeof(StreamGroup)
	public static StreamGroup []
		available_groups
	{
		get
		{
			if (_available_groups == null)
			{
				System.Array A = System.Enum.GetValues ( typeof ( StreamGroup ) );
				_available_groups = new StreamGroup [A.Length];
				//create at least one source per group
				for (int i = 0; i < A.Length; i++)
				{
					_available_groups [i] = (StreamGroup) A.GetValue ( i );
				}
			}
			return _available_groups;
		}
	}


	#region Methods to play sound by AudioClip.name (LanguageAudio is used to load sounds from Resources)

	/// <summary>
	/// Will stop all streams playing clip with name == snd_name
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	public static void StopSound (string snd_name)
	{
		List<AudioSource> container = GetStreams ( snd_name );
		StopStreams ( container );
	}

	/// <summary>
	/// Will stop all streams playing clip with name == snd_name
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	public static void StopSound (string snd_name, StreamGroup group)
	{
		List<AudioSource> container = GetStreams ( snd_name, group );
		StopStreams ( container );
	}

	/// <summary>
	/// Will tell true if at least one stream plays the sound
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <returns></returns>
	public static bool IsSoundPlaying (string snd_name)
	{
		List<AudioSource> lst = GetStreams ( snd_name );
		return IsSoundPlaying ( snd_name, lst );
	}

	/// <summary>
	/// Will tell true if at least one stream plays the sound
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <returns></returns>
	public static bool IsSoundPlaying (string snd_name, StreamGroup group)
	{
		List<AudioSource> lst = GetStreams ( snd_name, group );
		return IsSoundPlaying ( snd_name, lst );
	}


	/// <summary>
	/// Will tell true if at least one stream plays the sound	/// 
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="lst">List of streams to check</param>
	/// <returns></returns>
	private static bool IsSoundPlaying (string snd_name, List<AudioSource> lst)
	{
		bool res = false;
		for (int i = 0; i < lst.Count; i++)
		{
			if (lst [i].IsPlaying ( ))
			{
				res = true;
				break;
			}
		}
		return res;
	}

	/// <summary>
	/// Plays sound by name and returns it's length
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <param name="loop">This will loop the sound</param>
	/// <param name="pitch">Pitch value for sound</param>
	/// <returns></returns>
	public static float PlaySound (string snd_name, StreamGroup group = StreamGroup.FX, float volume = 1, bool loop = false, float pitch = 1)
	{
		AudioSource src = null;
		return PlaySound ( snd_name, ref src, group, volume, loop, pitch );
	}

	/// <summary>
	/// Plays sound by name and returns it's length and AoudioSource that will play it
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="_src">AudioSource from a pool that will play the clip. If unlocked - will be recycled</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <param name="loop">This will loop the sound</param>
	/// <param name="pitch">Pitch value for sound</param>
	/// <returns></returns>
	public static float PlaySound (string snd_name, ref AudioSource _src, StreamGroup group = StreamGroup.FX, float volume = -1, bool loop = false, float pitch = 1)
	{
		if (snd_name == "hp_sg-144")
			Debug.Log ("szuka");
		float res = 0;
		if (snd_name != "")
		{
			AudioClip clip = LanguageAudio.GetSoundByName ( snd_name );
			res = PlaySound ( clip, ref _src, group, volume, loop, pitch );
		}
		else
		{
			Debug.Log ( "Play sound called with null argument" );
		}

		return res;
	}


	/// <summary>
	/// Plays a set of sounds placed in a queue. Used for dialogues compbined with separate phrases. Returns stack length.
	/// </summary>
	/// <param name="snd_name">AudioClip name, value "||2.2" gives extra pause between phrases</param>
	/// <param name="interval">Pause before next AudioClip will be played</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <returns></returns>
	public static float PlaySound (string [] snd_name, float interval = 0, StreamGroup group = StreamGroup.FX, float volume = 1)
	{
		Sequence stack = null;
		AudioSource _src = null;
		return PlaySound ( snd_name, ref stack, ref _src, interval, group, volume );
	}


	/// <summary>
	/// Plays a set of sounds placed in a queue. Used for dialogues compbined with separate phrases. Returns stack length and stack itself
	/// </summary>
	/// <param name="snd_name">AudioClip name, value "||2.2" gives extra pause between phrases</param>
	/// <param name="_stack">Sequence that is managing playback of set of AudioClip</param>
	/// <param name="_src">AudioSource from a pool that will play the set of AudioClip. If unlocked - will be recycled</param>
	/// <param name="interval">Pause before next AudioClip will be played</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <returns></returns>
	public static float PlaySound (string [] snd_name, ref Sequence _stack, ref AudioSource _src, float interval = 0, StreamGroup group = StreamGroup.FX, float volume = 1)
	{
		AudioClip [] sounds = new AudioClip [snd_name.Length];
		Sequence stack = DOTween.Sequence ( );
		_stack = stack;
		List<Sequence> container = GetTweenContainer ( group );
		container.Add ( stack );
		float t = 0;
		AudioSource src = GetStream ( group );
		_src = src;
		LockStream ( src, true );
		stack.OnComplete ( () =>
		{
			container.Remove ( stack );
			LockStream ( src, false );
		} );
		for (int i = 0; i < snd_name.Length; i++)
		{
			if (snd_name [i].Substring ( 0, 2 ).Equals ( PAUSE ))
			{
				float delay;
				if (float.TryParse ( snd_name [i].Substring ( 2 ), out delay ))
				{
					t += delay;
				}
			}
			else
			{
				sounds [i] = LanguageAudio.GetSoundByName ( snd_name [i] );
				AudioClip clip = sounds [i];
				stack.InsertCallback ( t, () =>
				{
					SetAndPlayStream ( src, clip, group );
				} );
				if (sounds [i] == null)
				{
					Debug.Log ( "Sound " + snd_name [i] + " is null" );
				}
				else
				{
					t += sounds [i].length + interval;
				}
			}
		}
		return Mathf.Clamp ( t - interval, 0, float.MaxValue );
	}

	/// <summary>
	/// Switches flawlessly from one music clip to another
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="duration">fade duration</param>
	/// <param name="src">AudioSource that will be used to play effect on (optional)</param>
	public static void CrossFadeMusic (string snd_name, float duration = 0.5f, AudioSource src = null)
	{
		StreamGroup group = StreamGroup.MUSIC;
		AudioSource fade_src = src == null ? GetStream ( group ) : src;
		AudioClip clip = LanguageAudio.GetSoundByName ( snd_name );
		Sequence fader = DOTween.Sequence ( );
		List<Sequence> container = GetTweenContainer ( group );
		container.Add ( fader );
		float vol = fade_src.volume;
		fader.Append ( fade_src.DOFade ( 0, duration ) );
		fader.AppendCallback ( () =>
		{
			fade_src.clip = clip;
			fade_src.Play ( );
		} );
		fader.Append ( fade_src.DOFade ( vol, duration ) );
		fader.AppendCallback ( () => container.Remove ( fader ) );
	}


	/// <summary>
	/// Plays sound in Music group (deprecated)
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <returns></returns>
	public static float PlayMusic (string snd_name, float volume = -1)
	{
		return PlaySound ( snd_name, StreamGroup.MUSIC, volume, true );
	}



	/// <summary>
	/// Plays sound in Voice group (deprecated)
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <param name="loop">Loop value</param>
	/// <param name="pitch">Pitch value</param>
	/// <returns></returns>
	public static float PlayVoice (string snd_name, float volume = -1, bool loop = false, float pitch = 1)
	{
		return PlaySound ( snd_name, StreamGroup.VOICE, volume, loop, pitch );
	}

	#endregion


	#region Methods to play sound as AudioClip

	/// <summary>
	/// Plays sound by AudioClip and returns it's length and AoudioSource that will play it
	/// </summary>
	/// <param name="clip">AudioClip</param>
	/// <param name="_src">AudioSource from a pool that will play the clip. If unlocked - will be recycled</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <param name="loop">This will loop the sound</param>
	/// <param name="pitch">Pitch value for sound</param>
	/// <returns></returns>
	public static float PlaySound (AudioClip clip, StreamGroup group = StreamGroup.FX, float volume = -1, bool loop = false, float pitch = 1)
	{
		AudioSource src = null;
		return PlaySound ( clip, ref src, group, volume, loop, pitch );
	}


	/// <summary>
	/// Plays sound by AudioClip and returns it's length and AoudioSource that will play it
	/// </summary>
	/// <param name="clip">AudioClip</param>
	/// <param name="_src">AudioSource from a pool that will play the clip. If unlocked - will be recycled</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <param name="loop">This will loop the sound</param>
	/// <param name="pitch">Pitch value for sound</param>
	/// <returns></returns>
	public static float PlaySound (AudioClip clip, ref AudioSource _src, StreamGroup group = StreamGroup.FX, float volume = -1, bool loop = false, float pitch = 1)
	{
		float res = 0;
		_src = GetStream ( group );
		res = SetAndPlayStream ( _src, clip, group, volume, loop, pitch );
		return res;
	}

	#endregion


	#region Tweens Management

	private static List<Sequence> GetTweenContainer (StreamGroup group, bool paused = false)
	{
		return GetContainer ( group, paused ? paused_tweens : tweens );
	}

	/// <summary>
	///Stops all tweens.
	/// </summary>
	public static void ReleaseTweens ()
	{
		for (int i = 0; i < available_groups.Length; i++)
		{
			ReleaseTweens ( available_groups [i] );
		}
	}

	/// <summary>
	/// Stops and clears tweens related to the group.
	/// </summary>
	/// <param name="group"></param>
	public static void ReleaseTweens (StreamGroup group)
	{
		List<Sequence> container = GetTweenContainer ( group );
		for (int i = 0; i < container.Count; i++)
		{
			container [i].Kill ( true );
		}
		container.Clear ( );
	}

	/// <summary>
	/// Pauses all active tweens.
	/// </summary>
	/// <param name="pause"></param>
	public static void PauseTweens (bool pause)
	{
		for (int i = 0; i < available_groups.Length; i++)
		{
			PauseTweens ( pause, available_groups [i] );
		}
		//paused_tweens.Clear ( ); //will clear grpoups themselves
	}


	/// <summary>
	/// Pauses active tweens related to the group.
	/// </summary>
	/// <param name="pause"></param>
	/// <param name="group"></param>
	public static void PauseTweens (bool pause, StreamGroup group)
	{
		List<Sequence> container = GetTweenContainer ( group, true );

		if (pause)
		{
			container.AddRange ( GetPlayingTweens ( group ).ToArray ( ) );
		}
		for (int i = 0; i < container.Count; i++)
		{
			if (pause)
			{
				container [i].Pause ( );
			}
			else
			{
				container [i].Play ( );
			}
		}
		if (!pause)
		{
			container.Clear ( );
		}
	}

	/// <summary>
	/// Returns Active tweens related to the group.
	/// </summary>
	/// <param name="group"></param>
	/// <returns></returns>
	private static List<Sequence> GetPlayingTweens (StreamGroup group)
	{
		List<Sequence> res = new List<Sequence> ( );
		List<Sequence> container = GetTweenContainer ( group );
		for (int i = 0; i < container.Count; i++)
		{
			if (container [i].IsPlaying ( ))
			{
				res.Add ( container [i] );
			}
		}
		return res;
	}

	#endregion


	#region General methods

	/// <summary>
	/// Creates a pool of AudioSource - one per group.
	/// </summary>
	/// <param name="src_parrent">AudioSource pool holed (optional)</param>
	public static void InitStreams (GameObject src_parrent = null)
	{
		SrcParrent = src_parrent;
		if (SrcParrent == null)
		{
			SrcParrent = new GameObject ( "AudioSources" );
			GameObject.DontDestroyOnLoad ( SrcParrent );
		}
		//create at least one source per group
		for (int i = 0; i < available_groups.Length; i++)
		{
			GetStream ( available_groups [i] );
		}
	}



	/// <summary>
	/// Returns a generic List assigned to the group
	/// </summary>
	/// <typeparam name="T">Value type</typeparam>
	/// <param name="group">StreamGroup to select</param>
	/// <param name="dic">Container holder</param>
	/// <returns></returns>
	private static List<T> GetContainer<T> (StreamGroup group, Dictionary<StreamGroup, List<T>> dic)
	{
		List<T> res = null;
		if (!dic.TryGetValue ( group, out res ))
		{
			res = new List<T> ( );
			dic.Add ( group, res );
		}
		return res;
	}


	/// <summary>
	/// Will return all streams, that are playing sound named as clip_name.
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <returns></returns>
	public static List<AudioSource> GetStreams (string snd_name)
	{
		List<AudioSource> res = new List<AudioSource> ( );
		for (int i = 0; i < available_groups.Length; i++)
		{
			res.AddRange ( GetStreams ( snd_name, available_groups [i] ) );
		}
		return res;
	}


	/// <summary>
	/// Will return all streams, that are playing sound named as clip_name related to the group only.
	/// </summary>
	/// <param name="clip_name">AudioClip name</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <returns></returns>
	public static List<AudioSource> GetStreams (string clip_name, StreamGroup group)
	{
		List<AudioSource> res = new List<AudioSource> ( );
		res.AddRange ( GetStreams ( clip_name, GetStreamContainer ( (StreamGroup) group ) ) );
		res.AddRange ( GetStreams ( clip_name, GetUserStreamContainer ( (StreamGroup) group ) ) ); //deprecated
		return res;
	}


	/// <summary>
	/// Will stop all sequences, fades, own streams and user streams.
	/// </summary>
	/// <param name="leave_music">WIll prevent music streams from being stopped</param>
	static public void Release (bool leave_music = false)
	{
		//stop all dialofues and crossfadings
		ReleaseTweens ( );
		//release sources in all containers (music will be left playing if leave_music == true)
		for (int i = 0; i < available_groups.Length; i++)
		{
			StreamGroup group = available_groups [i];
			if (!leave_music || group != StreamGroup.MUSIC)
			{
				ReleaseSources ( group );
			}
		}
		//unlock all streams, previously locked in dialogues
		locked_streams.Clear ( );
	}


	/// <summary>
	/// Will unmute/mute sources. Set of groups will depend on is_music param (music_like_streams list defines wether group will be managed as music or as sound)
	/// </summary>
	/// <param name="param">true - unmute, false - mute</param>
	/// <param name="is_music"></param>
	private static void EnableStreams (bool param, bool is_music)
	{
		for (int i = 0; i < available_groups.Length; i++)
		{
			StreamGroup group = available_groups [i];
			if (music_like_streams.Contains ( group ) == is_music)
			{
				EnableStreams ( GetStreamContainer ( group ), param );
				EnableStreams ( GetUserStreamContainer ( group ), param ); //deprecated
			}
		}
	}

	/// <summary>
	/// Will unmute/mute sources, not listed in music_like_streams list.
	/// </summary>
	/// <param name="param">true - unmute, false - mute</param>
	public static void EnableSounds (bool param)
	{
		EnableStreams ( param, false );
	}
	/// <summary>
	/// Will unmute/mute sources, listed in music_like_streams list.
	/// </summary>
	/// <param name="param">true - unmute, false - mute</param>
	public static void EnableMusic (bool param)
	{
		EnableStreams ( param, true );
	}


	/// <summary>
	/// Will change own and user sources volume (individual group values are counted. Set of groups will depend on is_music param (music_like_streams list defines wether group will be managed as music or as sound)
	/// </summary>
	/// <param name="volume">This will override volume from GameSettings for all groups</param>
	/// <param name="is_music"></param>
	private static void SetStreamsVolume (float volume, bool is_music)
	{
		for (int i = 0; i < available_groups.Length; i++)
		{
			StreamGroup group = available_groups [i];
			if (music_like_streams.Contains ( group ) == is_music)
			{
				SetStreamsVolume ( volume, group );
			}
		}
	}


	/// <summary>
	/// Will change volume on own and user sources, not listed in music_like_streams list.
	/// </summary>
	/// <param name="volume">This will override volume from GameSettings</param>
	public static void SetSoundsVolume (float volume)
	{
		SetStreamsVolume ( volume, false );
	}


	/// <summary>
	/// Will change volume on own and user sources, listed in music_like_streams list.
	/// </summary>
	/// <param name="volume">This will override volume from GameSettings</param>
	public static void SetMusicVolume (float volume)
	{
		SetStreamsVolume ( volume, true );
	}


	/// <summary>
	/// Will pause/unpause all tweens and streams
	/// </summary>
	/// <param name="pause">Pause</param>
	public static void Pause (bool pause)
	{
		PauseTweens ( pause );
		//pause all groups
		for (int i = 0; i < available_groups.Length; i++)
		{
			PauseStreams ( pause, available_groups [i] );
		}
		if (OnPause != null)
		{
			OnPause ( pause );
		}
	}

	/// <summary>
	/// Tells if AudioSource is currently playing sound. Time check is implemented due to inability to use property AudioSource.isPlaying when ApplicationPause event fired (it will always return false when application loses focus or minimized).
	/// </summary>
	/// <param name="src">Stream to check</param>
	/// <returns></returns>
	private static bool IsPlaying (this AudioSource src)
	{
		return src.isPlaying || src.time != 0;
	}

	#endregion


	#region StreamGroup management

	/// <summary>
	/// Tells if group is not muted in GameSettings.
	/// </summary>
	/// <param name="group">StreamGroup to select</param>
	/// <returns></returns>
	public static bool IsGroupEnabled (StreamGroup group)
	{
		bool res = false;
		if (music_like_streams.Contains ( group ))
		{
			res = GameSettings.IsMusicEnabled ( );
		}
		else
		{
			res = GameSettings.IsSoundsEnabled ( );
		}
		return res;
	}


	/// <summary>
	/// Returns volume for the group multiplied by volume_override. If override == -1 - value, defined for the group will be used
	/// </summary>
	/// <param name="group">StreamGroup to select</param>
	/// <param name="volume_override">Will override settings from group_volume dictionary. -1 means that override value will be taken from that dictionary, otherwise param will be used</param>
	/// <returns></returns>
	public static float GetStreamsVolume (StreamGroup group, float volume_override = -1)
	{
		float res = 0;
		if (music_like_streams.Contains ( group ))
		{
			res = GameSettings.GetMusicVol ( );
		}
		else
		{
			res = GameSettings.GetSoundsVol ( );
		}
		//if volume_override == -1 we use value, defined in group_volume
		if (volume_override >= 0 || group_volume.TryGetValue ( group, out volume_override ))
		{
			res *= volume_override;
		}
		return res;
	}


	/// <summary>
	/// Will search for playing sources among own and user containers.
	/// </summary>
	/// <param name="group">StreamGroup to select</param>
	/// <param name="receiver">Streams, that are playing now</param>
	private static void GetPlayingSources (StreamGroup group, ref List<AudioSource> receiver)
	{
		List<AudioSource> src = new List<AudioSource> ( );
		GetPlayingStreams ( GetStreamContainer ( group ), ref receiver );
		GetPlayingStreams ( GetUserStreamContainer ( group ), ref receiver ); //deprecated
	}


	/// <summary>
	/// Returns a List of AudioSource in pool, assigned to the group.
	/// </summary>
	/// <param name="group">StreamGroup to select</param>
	/// <param name="paused">Tellls wether to take container from paused streams list or from own streams pool</param>
	/// <returns></returns>
	private static List<AudioSource> GetStreamContainer (StreamGroup group, bool paused = false)
	{
		return GetContainer ( group, paused ? paused_streams : streams );
	}


	/// <summary>
	/// Will return maximum remaining time for streams playing now, except looped streams
	/// </summary>
	/// <param name="group">StreamGroup to select</param>
	/// <returns></returns>
	public static float GetSoundsEndTime (StreamGroup group)
	{
		float res = 0;
		List<AudioSource> container = GetStreamContainer ( group );
		if (container != null && container.Count > 0)
		{
			res = container.FindAll ( x => x.clip != null && x.IsPlaying ( ) && !x.loop ).Max ( x => x.clip.length - x.time );
			Mathf.Clamp ( res, 0, float.MaxValue );
		}
		return res;
	}


	/// <summary>
	/// Release sources in pool and user sources. Will leave them available for further usage.
	/// </summary>
	/// <param name="group">StreamGroup to select</param>
	public static void ReleaseSources (StreamGroup group)
	{
		ReleaseOwnSources ( group ); // streams will be stopped only
		ReleaseUserStreams ( group ); // will stop and unregister streams (deprecated)
	}



	/// <summary>
	/// Release sources in pool. Will leave them available for further usage.
	/// </summary>
	/// <param name="group">StreamGroup to select</param>
	private static void ReleaseOwnSources (StreamGroup group)
	{
		List<AudioSource> container = GetStreamContainer ( group );
		for (int i = 0; i < container.Count; i++)
		{
			StopStream ( container [i] );
		}
	}




	/// <summary>
	/// Sets and saves volume for the group.
	/// </summary>
	/// <param name="volume">Will overwrite setting in group_volume dictionary.</param>
	/// <param name="group">StreamGroup to select</param>
	public static void SetGroupVolume (float volume, StreamGroup group)
	{
		if (!group_volume.ContainsKey ( group ))
		{
			group_volume.Add ( group, volume );
		}
		else
		{
			group_volume [group] = volume;
		}
		SetStreamsVolume ( volume, group );
	}


	/// <summary>
	/// Sets volume for own and user streams in accordance to GameSettings and group_volume overrides/volume param.
	/// </summary>
	/// <param name="volume">Will override settings from group_volume dictionary. -1 means that override value will be taken from that dictionary, otherwise param will be used</param>
	/// <param name="group">StreamGroup to select</param>
	public static void SetStreamsVolume (float volume, StreamGroup group)
	{
		volume = GetStreamsVolume ( group, volume );
		SetVolume ( GetStreamContainer ( group ), volume );
		SetVolume ( GetUserStreamContainer ( group ), volume ); //deprecated
	}


	/// <summary>
	/// Will pause/unpause streams related to the group.
	/// </summary>
	/// <param name="pause">Pause param</param>
	/// <param name="group">StreamGroup to select</param>
	public static void PauseStreams (bool pause, StreamGroup group)
	{
		List<AudioSource> container = GetStreamContainer ( group, true );
		//collect playing streams (will add newly found to existing list, in case pause was set twice in a row)
		if (pause)
		{
			GetPlayingSources ( group, ref container );
		}
		PauseStreams ( container, pause );
		//clear all streams that were UnPaused to avoid playing stopped streams
		if (!pause)
		{
			container.Clear ( );
		}
	}



	#endregion


	#region AudioSource management

	/// <summary>
	/// Creates AudioSource as a component of SrcParrent gameObject
	/// </summary>
	/// <returns></returns>
	private static AudioSource CreateStream ()
	{
		if (SrcParrent == null)
		{
			InitStreams ( );
		}
		AudioSource src = SrcParrent.AddComponent<AudioSource> ( );
		src.playOnAwake = false;
		return src;
	}


	/// <summary>
	/// Returns AudioSource for the group. If Group has no available AudioSource it will be created and attached to the group automaticaly.
	/// </summary>
	/// <param name="group">StreamGroup to select</param>
	/// <returns></returns>
	private static AudioSource GetStream (StreamGroup group)
	{
		AudioSource res = null;
		List<AudioSource> container = GetStreamContainer ( group );
		bool single = single_stream.Contains ( group );
		for (int i = 0; container != null && i < container.Count; i++)
		{
			AudioSource src = container [i];
			//get stream that is not playing, not paused and not locked right now
			if (!src.IsLocked ( ) && (single || (!src.IsPlaying ( ) && !src.IsPaused ( group ))))
			{
				res = src;
				break;
			}
		}
		if (res == null && (!single || container.Count == 0))
		{
			res = CreateStream ( );
			container.Add ( res );
		}
		if (res == null)
		{
			Debug.LogError ( "Couldn't play sound on group " + group + " (is_single = " + single + "), but streams were locked or paused" );
		}
		return res;
	}


	/// <summary>
	/// Returns all streams playing sound, named as clip_name
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="container">Streams list to serach for sound</param>
	/// <returns></returns>
	private static List<AudioSource> GetStreams (string snd_name, List<AudioSource> container)
	{
		return container.FindAll ( X => X.clip != null && X.clip.name == snd_name );
	}

	/// <summary>
	/// Sets all necessary params to AudioSource according to its group and overrides.
	/// </summary>
	/// <param name="src">AudioSource to play</param>
	/// <param name="clip">AudioClip to play</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <param name="loop">This will loop the sound</param>
	/// <param name="pitch">Pitch value for sound</param>
	/// <returns></returns>
	private static float SetAndPlayStream (AudioSource src, AudioClip clip, StreamGroup group, float volume = -1, bool loop = false, float pitch = 1)
	{
		float res = 0;
		if (src != null)
		{
			if (clip != null)
			{
				src.clip = clip;
				res = clip.length;
				src.loop = loop;
				src.mute = !IsGroupEnabled ( group );
				src.volume = GetStreamsVolume ( group, volume );
				src.pitch = pitch;
				src.Play ( );
			}
		}
		return res;
	}



	/// <summary>
	/// Will return all sources from List that are playing now.
	/// </summary>
	/// <param name="container">Streams list to search for streams playing now</param>
	/// <param name="receiver">List where playng streams will be stored</param>
	private static void GetPlayingStreams (List<AudioSource> container, ref List<AudioSource> receiver)
	{
		for (int i = 0; container != null && i < container.Count; i++)
		{
			if (container [i] != null)
			{
				if (container [i].IsPlaying ( ))
				{
					receiver.Add ( container [i] );
				}
			}
		}
	}


	/// <summary>
	/// Locks stream. It won't be selected for other sounds playback as a pool item (exceptions are single streamed groups).
	/// </summary>
	/// <param name="src">Stream to lock</param>
	/// <param name="_lock">Lock/Unlock stream</param>
	public static void LockStream (AudioSource src, bool _lock)
	{
		if (_lock)
		{
			if (!locked_streams.Contains ( src ))
			{
				locked_streams.Add ( src );
			}
		}
		else
		{
			locked_streams.Remove ( src );
		}
	}

	/// <summary>
	/// Tells wether AudioSource is locked.
	/// </summary>
	/// <param name="src">Stream to check</param>
	/// <returns></returns>
	public static bool IsLocked (this AudioSource src)
	{
		return locked_streams.Contains ( src );
	}


	/// <summary>
	/// Tells wether AudioSource is paused.
	/// </summary>
	/// <param name="src">Stream to check</param>
	/// <param name="group">StreamGroup the stream is related to</param>
	/// <returns></returns>
	private static bool IsPaused (this AudioSource src, StreamGroup group)
	{
		return paused_streams.ContainsKey ( group ) && paused_streams [group].Contains ( src );
	}


	/// <summary>
	/// Will pause/unpause streams in container.
	/// </summary>
	/// <param name="container">Streams list to pause</param>
	/// <param name="pause">Pause value</param>
	private static void PauseStreams (List<AudioSource> container, bool pause)
	{
		foreach (AudioSource source in container)
		{
			PauseStream ( source, pause );
		}
		if (!pause)
		{
			container.Clear ( );
		}
	}


	/// <summary>
	/// Will set vaolume for streams list
	/// </summary>
	/// <param name="container">Streams list to pause</param>
	/// <param name="volume">Volume value</param>
	private static void SetVolume (List<AudioSource> container, float volume)
	{
		for (int i = 0; container != null && i < container.Count; i++)
		{
			container [i].volume = volume;
		}
	}


	/// <summary>
	/// Will Unmute/Mute streams.
	/// </summary>
	/// <param name="container">Streams list to Unmute/Mute</param>
	/// <param name="param">True - UnMute, False -Mute</param>
	private static void EnableStreams (List<AudioSource> container, bool param)
	{
		foreach (AudioSource src in container)
		{
			if (src != null)
			{
				src.mute = !param;
			}
		}
	}


	/// <summary>
	/// Will Pause/Unpause stream
	/// </summary>
	/// <param name="src">Stream to Pause/Unpause</param>
	/// <param name="pause">Pause value</param>
	private static void PauseStream (AudioSource src, bool pause)
	{
		if (src != null)
		{
			if (pause)
			{
				src.Pause ( );
			}
			else
			{
				src.Play ( );
			}
		}
	}


	/// <summary>
	/// WIll stop streams.
	/// </summary>
	/// <param name="container">Streams list to stop</param>
	private static void StopStreams (List<AudioSource> container)
	{
		for (int i = 0; i < container.Count; i++)
		{
			StopStream ( container [i] );
		}
	}

	/// <summary>
	/// Will stop stream
	/// </summary>
	/// <param name="src">Stream to stop</param>
	private static void StopStream (AudioSource src)
	{
		if (src != null)
		{
			src.Stop ( );
			src.clip = null;
		}
	}


	#endregion


	#region User Sources management (deprecated)

	[Obsolete ( "Not used anymore, use method with ref AudioSource instead to get stream", false )]
	public static float PlayStream (AudioSource src, AudioClip clip = null, bool loop = false)
	{
		return PlaySound(clip, src, StreamGroup.FX, loop: loop);
	}

	[Obsolete ( "Not used anymore, use method with ref AudioSource instead to get stream", false )]
	static public float PlayMusic (AudioSource src, AudioClip clip = null, bool loop = true, float volume = -1)
	{
		return PlayMusic ( clip, src, volume, loop );
	}


	[Obsolete ( "Not used anymore, use method with ref AudioSource instead to get stream", false )]
	/// <summary>
	/// Plays sound by name and returns it's length and AoudioSource that will play it (deprecated)
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="src">AudioSource that will play the clip</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <param name="loop">This will loop the sound</param>
	/// <param name="pitch">Pitch value for sound</param>
	/// <returns></returns>
	private static float PlaySound (string snd_name, AudioSource src, StreamGroup group = StreamGroup.FX, float volume = -1, bool loop = false, float pitch = 1)
	{
		float res = 0;
		if (snd_name != "")
		{
			AudioClip clip = LanguageAudio.GetSoundByName ( snd_name );
			res = PlaySound ( clip, src, group, volume, loop, pitch );
		}
		else
		{
			Debug.Log ( "Play sound called with null argument" );
		}
		return res;
	}

	[Obsolete ( "Not used anymore, use method with ref AudioSource instead to get stream", false )]
	/// <summary>
	/// Plays sound by AudioClip and returns it's length and AoudioSource that will play it (deprecated)
	/// </summary>
	/// <param name="clip">AudioClip</param>
	/// <param name="src">user AudioSource that will play the clip</param>
	/// <param name="group">StreamGroup the clip is related to</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <param name="loop">This will loop the sound</param>
	/// <param name="pitch">Pitch value for sound</param>
	/// <returns></returns>
	public static float PlaySound (AudioClip clip, AudioSource src, StreamGroup group = StreamGroup.FX, float volume = -1, bool loop = false, float pitch = 1)
	{
		float res = SetAndPlayStream ( src, clip, group, volume, loop, pitch );
		if (res > 0)
		{
			AddUserStream ( src, group );
		}
		return res;
	}

	[Obsolete ( "Not used anymore, use method with ref AudioSource instead to get stream", false )]
	/// <summary>
	/// Plays sound in Music group (deprecated)
	/// </summary>
	/// <param name="snd_name">AudioClip name</param>
	/// <param name="src">AudioSource that will play the clip</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <returns></returns>
	public static float PlayMusic (string snd_name, AudioSource src, float volume = -1)
	{
		return PlaySound ( snd_name, src, StreamGroup.MUSIC, volume, true );
	}


	[Obsolete ( "Not used anymore, use method with ref AudioSource instead to get stream", false )]
	/// <summary>
	/// (deprecated) Plays sound in Music group (deprecated)
	/// </summary>
	/// <param name="clip">AudioClip</param>
	/// <param name="src">user AudioSource that will play the clip</param>
	/// <param name="volume">This will override volume from GameSettings for the group</param>
	/// <param name="loop">This will loop the sound (true by default for music clips)</param>
	/// <returns></returns>
	static public float PlayMusic (AudioClip clip, AudioSource src, float volume = -1, bool loop = true)
	{
		return PlaySound ( clip, src, StreamGroup.MUSIC, volume, loop );
	}


	[Obsolete ( "Not used anymore", false )]
	/// <summary>
	/// Returns a List of user AudioSource assigned to the group (deprecated)
	/// </summary>
	/// <param name="group">StreamGroup to select</param>
	/// <returns></returns>
	private static List<AudioSource> GetUserStreamContainer (StreamGroup group)
	{
		return GetContainer ( group, user_streams );
	}


	[Obsolete ( "Not used anymore", false )]
	/// <summary>
	/// Adds user stream which was set in Play... method, to keep it for managing settings change events and pause (deprecated)
	/// </summary>
	/// <param name="src">User stream to register</param>
	/// <param name="group">StreamGroup to select</param>
	static private void AddUserStream (AudioSource src, StreamGroup group)
	{
		if (!streams.ContainsKey ( group ))
		{
			streams.Add ( group, new List<AudioSource> ( ) );
		}
		List<AudioSource> container = GetContainer ( group, user_streams );
		if (src != null && !container.Contains ( src ))
		{
			container.Add ( src );
		}
	}

	[Obsolete ( "Not used anymore", false )]
	/// <summary>
	/// Stops user stream, used to play sound on given AudioSource and given group (deprecated)
	/// </summary>
	/// <param name="src">User stram to stop and unregister</param>
	/// <param name="group">StreamGroup to select</param>
	static public bool StopUserStream (AudioSource src, StreamGroup group)
	{
		StopStream ( src );
		return RemoveUserStream ( src, GetUserStreamContainer ( group ) );
	}

	[Obsolete ( "Not used anymore", false )]
	/// <summary>
	/// Release sources, registered by user. Will unregister them (deprecated)
	/// </summary>
	/// <param name="group">StreamGroup to select</param>
	private static void ReleaseUserStreams (StreamGroup group)
	{
		List<AudioSource> container = GetUserStreamContainer ( group );
		for (int i = 0; i < container.Count; i++)
		{
			StopStream ( container [i] );
		}
		//clearing user sources to avoid null items after transition
		container.Clear ( );
	}

	[Obsolete ( "Not used anymore", false )]
	/// <summary>
	/// Will unregister user stream if it was registered (deprecated)
	/// </summary>
	/// <param name="src">Stream to unregister</param>
	/// <param name="container">List to remove the stream from</param>
	/// <returns></returns>
	private static bool RemoveUserStream (AudioSource src, List<AudioSource> container)
	{
		bool res = false;
		if (src != null && container.Contains ( src ))
		{
			res = container.Remove ( src );
		}
		return res;
	}


	[Obsolete ( "Not used anymore", false )]
	/// <summary>
	/// Stops user stream, used to play sound on given AudioSource (deprecated)
	/// </summary>
	/// <param name="src">Stream to stop</param>
	static public bool StopUserStream (AudioSource src)
	{
		bool res = false;
		StopStream ( src );
		foreach (var item in user_streams)
		{
			res = StopUserStream ( src, item.Key );
			if (res)
			{
				break;
			}
		}
		return res;
	}

	#endregion
}
