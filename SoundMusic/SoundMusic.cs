using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMusic : MainRefs
{
	public static SoundMusic shared;
	public List<SoundTypeClip> mainThemeSoundsList;
	public List<SoundTypeClip> soundTypeClipsList;
	public List<LoopSoundTypeClip> loopSoundTypeClipsList;
	public AudioSource mainThemeAudioSource;
	public GameObject loopSoundPrefab;
	private AudioSource audioSource;
	private int mainThemeIndex;
	private Timer mainThemeTimer = new Timer(TimerMode.counterFixedUpdate);
	private bool temporaryMuteMusic;
	private Dictionary<Object, AudioSource> loopSoundPair = new Dictionary<Object, AudioSource>();
	protected GeneralSavingData GeneralSavingData => GetRef<AbstractSavingManager>().GetSavingData<GeneralSavingData>(SavingDataType.General);

	private void Awake()
	{
		shared = this;
		FindObjectOfType<Initializer>().AddReadySharedObject(this);
#if UNITY_EDITOR
		var arr = FindObjectsOfType<SoundMusic>();
		if (arr.Length > 1) Debug.LogError("Singleton already exist!!!");
#endif
	}

	protected override void Start()
	{
		audioSource = GetComponent<AudioSource>();
		mainThemeTimer.OnTimerReached = PlayMainThemeSound;
		SetSoundEnable(GeneralSavingData.isSoundOn);
		SetMusicEnable(GeneralSavingData.isMusicOn);
	}

	private void FixedUpdate()
	{
		mainThemeTimer.TimerUpdate();
	}

	private void PlayMainThemeSound()
	{
		if (mainThemeSoundsList.Count == 0) return;
		var v = mainThemeSoundsList[mainThemeIndex];
		mainThemeAudioSource.clip = v.audioClip;
		mainThemeAudioSource.volume = v.volume;
		mainThemeAudioSource.Play();
		if (!mainThemeAudioSource.loop) mainThemeTimer.StartTimer(v.audioClip.length);
		mainThemeIndex++;
		if (mainThemeIndex > mainThemeSoundsList.Count - 1) mainThemeIndex = 0;
	}

	public void PlaySound(SoundType type)
	{
		var v = soundTypeClipsList.Find(a => a.type == type);
		if (v != null) audioSource.PlayOneShot(v.audioClip, v.volume);
	}

	public void PlayStopLoopSound(SoundType type, bool play, Object obj)
	{
		var v = loopSoundTypeClipsList.Find(a => a.type == type);

		if (v != null)
		{
			if (!loopSoundPair.ContainsKey(obj) && play)
			{
				var audioSource = Instantiate(loopSoundPrefab, transform).GetComponent<AudioSource>();
				loopSoundPair.Add(obj, audioSource);
				audioSource.clip = v.audioClip;
				audioSource.volume = v.volume;
				audioSource.loop = true;
				audioSource.Play();
			}

			if (loopSoundPair.ContainsKey(obj) && !play)
			{
				loopSoundPair[obj].Stop();
				Destroy(loopSoundPair[obj].gameObject, 0.1f);
				loopSoundPair.Remove(obj);
			}
		}
	}

	public void SetSoundEnable(bool b)
	{
		var arr = FindObjectsOfType<AudioSource>();

		foreach (var item in arr)
		{
			if (item.name == "MainTheme") continue;
			item.mute = !b;
		}
	}

	public void SetMusicEnable(bool b)
	{
		StartCoroutine(MusicEnableCoroutine(b));
	}

	private IEnumerator MusicEnableCoroutine(bool b)
	{
		yield return new WaitForSeconds(0.5f);
		if (temporaryMuteMusic == true)
			yield return new WaitUntil(() => temporaryMuteMusic == false);
		var item = transform.Find("MainTheme").GetComponent<AudioSource>();
		item.mute = !b;
		if (!item.isPlaying) PlayMainThemeSound();
		yield return null;
	}

	public void TemporaryMuteSound(bool b)
	{
		if (!GeneralSavingData.isSoundOn) return;
		var arr = FindObjectsOfType<AudioSource>();

		foreach (var item in arr)
		{
			if (item.name == "MainTheme") continue;
			item.mute = b;
		}
	}

	public void TemporaryMuteMusic(bool b)
	{
		if (!GeneralSavingData.isMusicOn) return;
		var item = transform.Find("MainTheme").GetComponent<AudioSource>();
		item.mute = b;
		temporaryMuteMusic = b;
	}

	public void PlayLoopAudioSource(SoundType type, AudioSource audioSource)
	{
		var v = loopSoundTypeClipsList.Find(a => a.type == type);

		if (v != null && !audioSource.isPlaying)
		{
			audioSource.clip = v.audioClip;
			audioSource.volume = v.volume;
			audioSource.loop = true;
			audioSource.Play();
		}
	}

	[System.Serializable]
	public class SoundTypeClip
	{
		public SoundType type;
		public AudioClip audioClip;
		public float volume = 1;
	}

	[System.Serializable]
	public class LoopSoundTypeClip
	{
		public SoundType type;
		public AudioClip audioClip;
		public float volume = 1;
	}
}

public enum SoundType
{
	none = 0,
	pickUp = 3,
	showQuestPopup = 4,
	absentTool = 5,
	closeWindow = 6,
	clickBtn = 7,
	craftStart = 8,
	craftFinished = 9,
	showGhostBuilding = 10,
	clearBackpack = 11,
	showCraftPanel = 12,
	switchCraftPanelTab = 13,
	showCardPanel = 14,
	heapOpen = 15,
	heapOpenFinished = 16,
	showBuildNeedResourcePopup = 17,
	mapFound = 18,
	chestOpen = 19,
	chestOpenFinished = 20,
	mechanismOpen = 21,
	mechanismOpenFinished = 22,
	openTrap = 23,
	openTrapFinished = 24,
	showEndLocationPopup = 25,
	goToNewLocationButton = 26,
	showSellerPopup = 27,
	sellerBuyProduct = 28,
	cartLaunch = 29,
	rockImpact = 30,
	chopWood = 31,
	chopWoodFinished = 32,
	chopStone = 33,
	chopStoneFinished = 34,
	chopVine = 35,
	chopVineFinished = 36,
	chopBranch = 37,
	chopBranchFinished = 38,
	houseOpen = 39,
	houseOpenFinished = 40,
	carOpen = 41,
	carOpenFinished = 42,
	exchangeItemFinished = 43,
	kickZombie = 44,
	zombieAttack = 45,
	zombieKickPlayer = 46,
	zombieDeath = 47,
	playerDeath = 48,
	showPlayerDeathPopup = 49,
	shotgunShot = 50,
	getDiamonds = 51,
	getCoins = 52,
	buyCloth = 53,
	changeCloth = 54,
	buyChest = 55,
}
