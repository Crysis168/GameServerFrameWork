using UnityEngine;
using System.Collections;

namespace Engine
{
	[AddComponentMenu("")]
	public class SoundManager : SingletonMonoBehaviour<SoundManager> 
	{
		private GameObject musicGO;
		private AudioSource musicSource;
		private AudioSource soundSource;
		private AudioClip   musicClip = null;

		private string musicName;
		private bool musicLoop = true;
		private float fadeDuration = 10.0f;
		private bool fadeInStart = true;

		private float _soundVolume = 1.0f;
		private float _musicVolume = 1.0f;
		private bool  _mute = false;
		private bool  _muteSound = false;
		private bool  _muteMusic = false;

		protected override void Awake()
		{
			base.Awake ();
			gameObject.AddComponent<AudioListener> ();
			soundSource = gameObject.AddComponent<AudioSource> ();
			CreateMusicGameObject ();
		}

		private void CreateMusicGameObject()
		{
			if(musicGO != null)
			{
				GameObject.Destroy(musicGO);
			}
			musicGO = new GameObject ("BackMusic");
			musicGO.transform.parent = gameObject.transform;
			musicSource = musicGO.AddComponent<AudioSource> ();
			musicSource.playOnAwake = false;
		}

		public void PlayMusic(string strMusic)
		{
			if(_mute && _musicVolume == 0)
				return;
			if (musicName == strMusic)
				return;
			
			AudioClip clip = Resources.Load ("musics/"+strMusic) as AudioClip;
			if (clip)
			{
				musicName = strMusic;
				musicClip = clip;
				FadeInStart();
			}
		}

		void Update()
		{
			if(musicSource != null && musicClip != null && !musicSource.isPlaying && musicLoop)
			{
				FadeInStart();
			}
		}

		public void PlayOneShot(string soundName)
		{
			if(_mute && SoundVolume == 0)
			{
				return;
			}

			AudioClip clip = Resources.Load ("sounds/"+soundName) as AudioClip;
			if(clip)
				soundSource.PlayOneShot(clip, SoundVolume);
		}

		private void FadeInStart()
		{
			if (musicSource == null)
				return;
			if(musicSource.isPlaying)
				musicSource.Stop ();
			musicSource.clip = musicClip;
			musicSource.Play ();
			if(fadeInStart)
			{
				musicSource.volume = 0.01f;
				SUGUI.TweenVolume.Begin(musicGO,fadeDuration,MusicVolume);
			}
			else
			{
				musicSource.volume = MusicVolume;
			}
		}

		public float SoundVolume
		{
			get
			{
				return _soundVolume;
			}
			set
			{
				if(_soundVolume != value)
				{
					_soundVolume = value;
					soundSource.volume = SoundVolume;
				}
			}
		}

		public float MusicVolume
		{
			get
			{
				return _musicVolume;
			}
			set
			{
				if(_musicVolume != value)
				{
					_musicVolume = value;
					musicSource.volume = MusicVolume;
				}
			}
		}

		public bool Mute
		{
			get
			{
				return _mute;
			}
			set
			{
				if(_mute != value)
				{
					_mute = value;
					musicSource.mute = _mute;
					soundSource.mute = _mute;
				}
			}
		}

		public bool MuteSound
		{
			get
			{
				return _mute;
			}
			set
			{
				if(_muteSound != value)
				{
					_muteSound = value;
					soundSource.mute = _muteSound;
				}
			}
		}

		public bool MuteMusic
		{
			get
			{
				return _muteMusic;
			}
			set
			{
				if(_muteMusic != value)
				{
					_muteSound = value;
					musicSource.mute = _muteMusic;
				}
			}
		}
	}
}
