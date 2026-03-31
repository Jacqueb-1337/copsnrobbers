using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

// Token: 0x02000257 RID: 599
public class MainMenuDirector : MonoBehaviour
{
	// Token: 0x0600109B RID: 4251 RVA: 0x00089448 File Offset: 0x00087648
	public MainMenuDirector()
	{
	}

	// Token: 0x14000014 RID: 20
	// (add) Token: 0x0600109C RID: 4252 RVA: 0x00089494 File Offset: 0x00087694
	// (remove) Token: 0x0600109D RID: 4253 RVA: 0x000894AC File Offset: 0x000876AC
	public static event MainMenuDirector.FacebookEventHandler OnFacebook;

	// Token: 0x14000015 RID: 21
	// (add) Token: 0x0600109E RID: 4254 RVA: 0x000894C4 File Offset: 0x000876C4
	// (remove) Token: 0x0600109F RID: 4255 RVA: 0x000894DC File Offset: 0x000876DC
	public static event MainMenuDirector.TwitterEventHandler OnTwitter;

	// Token: 0x060010A0 RID: 4256 RVA: 0x000894F4 File Offset: 0x000876F4
	public void GetGiftPackBtnPressed()
	{
		AutoGiftInfo autoGiftInfo = GrowthManagerKit.RecevieOneGift();
		this.giftNumLabel.text = "x " + autoGiftInfo.num.ToString();
		this.giftSprite.spriteName = autoGiftInfo.spriteName;
		this.giftSprite.MarkAsChanged();
		this.rootPanel.SetActive(false);
		this.GiftPackEmptyObject.SetActive(true);
		TweenScale.Begin(this.GiftPackEmptyObject, 0.001f, new Vector3(0.01f, 0.01f, 0.01f));
		base.StartCoroutine(this.ShowGiftPackPanel(0.01f));
		this.backgroundAudio.GetComponent<AudioSource>().Pause();
		base.audio.PlayOneShot(this.slotAudio[2]);
	}

	// Token: 0x060010A1 RID: 4257 RVA: 0x000895B8 File Offset: 0x000877B8
	private IEnumerator ShowGiftPackPanel(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		TweenScale.Begin(this.GiftPackEmptyObject, 0.2f, new Vector3(1f, 1f, 1f));
		yield break;
	}

	// Token: 0x060010A2 RID: 4258 RVA: 0x000895E4 File Offset: 0x000877E4
	public void CloseGiftPackPanelBtnPressed()
	{
		this.rootGiftNumLabel.text = "x " + GrowthManagerKit.GetCurGiftBoxTotal().ToString();
		this.GiftPackEmptyObject.SetActive(false);
		this.rootPanel.SetActive(true);
		base.audio.Stop();
		this.backgroundAudio.GetComponent<AudioSource>().Play();
	}

	// Token: 0x060010A3 RID: 4259 RVA: 0x00089648 File Offset: 0x00087848
	public void LotteryBtnPressed()
	{
		this.lotteryPanel.SetActive(true);
		this.rootPanel.SetActive(false);
		this.backgroundAudio.GetComponent<AudioSource>().Pause();
		this.giftNumShowLabel.text = "x " + GrowthManagerKit.GetCurGiftBoxTotal().ToString();
		this.gemNumShowLabel.text = "x " + GrowthManagerKit.GetGems().ToString();
		this.coinNumShowLabel.text = "x " + GrowthManagerKit.GetCoins().ToString();
		if (GrowthManagerKit.GetCurGiftBoxTotal() > 0)
		{
			this.startBtn.GetComponent<UIImageButton>().isEnabled = true;
		}
		else
		{
			this.startBtn.GetComponent<UIImageButton>().isEnabled = false;
		}
		this.facebookBtn.GetComponent<UIImageButton>().isEnabled = false;
		this.twitterBtn.GetComponent<UIImageButton>().isEnabled = false;
	}

	// Token: 0x060010A4 RID: 4260 RVA: 0x00089738 File Offset: 0x00087938
	public void StartToLotteryBtnPressed()
	{
		this.startBtn.GetComponent<UIImageButton>().isEnabled = false;
		this.facebookBtn.GetComponent<UIImageButton>().isEnabled = false;
		this.twitterBtn.GetComponent<UIImageButton>().isEnabled = false;
		this.closeGiftPackPanelBtn.GetComponent<UIImageButton>().isEnabled = false;
		this.giftNumShowLabel.text = "x " + (GrowthManagerKit.GetCurGiftBoxTotal() - 1).ToString();
		if (this.slotInfo != null)
		{
			this.cursor[this.slotInfo.resultIndex].gameObject.SetActive(false);
		}
		this.slotInfo = GrowthManagerKit.GetSlotsResultInfo();
		for (int i = 0; i < this.slotInfo.itemList.Count; i++)
		{
			this.giftSprites[i].spriteName = "Null_SlotLogo";
			this.giftSprites[i].MarkAsChanged();
			this.giftNumber[i].text = string.Empty;
		}
		this.giftResult.SetActive(false);
		base.StartCoroutine(this.ShowDefaultGift(1f));
	}

	// Token: 0x060010A5 RID: 4261 RVA: 0x00089850 File Offset: 0x00087A50
	public void CloseLotteryPanelBtnPressed()
	{
		this.lotteryPanel.SetActive(false);
		this.rootPanel.SetActive(true);
		this.backgroundAudio.GetComponent<AudioSource>().Play();
		this.giftResult.SetActive(false);
		if (this.slotInfo != null)
		{
			this.cursor[this.slotInfo.resultIndex].gameObject.SetActive(false);
			for (int i = 0; i < this.slotInfo.itemList.Count; i++)
			{
				this.giftSprites[i].spriteName = "Null_SlotLogo";
				this.giftSprites[i].MarkAsChanged();
				this.giftNumber[i].text = string.Empty;
			}
		}
	}

	// Token: 0x060010A6 RID: 4262 RVA: 0x0008990C File Offset: 0x00087B0C
	public void OpenSlotRulePanelBtnPressed()
	{
		this.lotteryEmptyObject.SetActive(false);
		this.lotteryRuleEmptyObject.SetActive(true);
		this.lotteryPanelBgTexture.GetComponent<UITexture>().mainTexture = this.bgTexture2;
	}

	// Token: 0x060010A7 RID: 4263 RVA: 0x00089948 File Offset: 0x00087B48
	public void CloseSlotRulePanelBtnPressed()
	{
		this.lotteryEmptyObject.SetActive(true);
		this.lotteryRuleEmptyObject.SetActive(false);
		this.lotteryPanelBgTexture.GetComponent<UITexture>().mainTexture = this.bgTexture1;
	}

	// Token: 0x060010A8 RID: 4264 RVA: 0x00089984 File Offset: 0x00087B84
	public void GenFacebookEvent()
	{
		if (MainMenuDirector.OnFacebook != null)
		{
			MainMenuDirector.OnFacebook();
		}
	}

	// Token: 0x060010A9 RID: 4265 RVA: 0x0008999C File Offset: 0x00087B9C
	public void GenTwitterEvent()
	{
		if (MainMenuDirector.OnTwitter != null)
		{
			MainMenuDirector.OnTwitter();
		}
	}

	private static bool _modsReady = false;

	// Token: 0x060010AA RID: 4266 RVA: 0x000899B4 File Offset: 0x00087BB4
	private void Awake()
	{
		MainMenuDirector.mInstance = this;
		_modsReady = false;
		MainMenuDirector.LoadMods();
	}

	// Token: 0x060010AB RID: 4267 RVA: 0x000899BC File Offset: 0x00087BBC
	private void Start()
	{
		base.StartCoroutine(StartAfterMods());
	}

	private IEnumerator StartAfterMods()
	{
		while (!_modsReady)
			yield return new WaitForSeconds(0.1f);

		this.rootGiftNumLabel.text = "x " + GrowthManagerKit.GetCurGiftBoxTotal().ToString();
		this.giftNumShowLabel.text = "x " + GrowthManagerKit.GetCurGiftBoxTotal().ToString();
		this.gemNumShowLabel.text = "x " + GrowthManagerKit.GetGems().ToString();
		this.coinNumShowLabel.text = "x " + GrowthManagerKit.GetCoins().ToString();
		if (UnityEngine.Random.Range(1, 11) == 1 || UserDataController.GetLoginCount() % 3 != 1 || UserDataController.HasDownloadRecommend())
		{
			MainMenuDirector.mInstance.RecommendPopPanel.SetActive(false);
			if (UnityEngine.Random.Range(1, 5) == 1 && UserDataController.GetLoginCount() >= 10 && !UserDataController.HasRatedInAppstore())
			{
				MainMenuDirector.mInstance.MainMenuPanel.SetActive(false);
				MainMenuDirector.mInstance.RatingPopPanel.SetActive(true);
			}
			else
			{
				MainMenuDirector.mInstance.MainMenuPanel.SetActive(true);
				MainMenuDirector.mInstance.RatingPopPanel.SetActive(false);
			}
		}
		if (UserDataController.IsFirstUseApp())
		{
			MainMenuDirector.mInstance.MainMenuPanel.SetActive(false);
			MainMenuDirector.mInstance.TermsPopPanel.SetActive(true);
		}
	}

	// Token: 0x060010AC RID: 4268 RVA: 0x00089B20 File Offset: 0x00087D20
	private void OnDestroy()
	{
		if (MainMenuDirector.mInstance == this)
		{
			MainMenuDirector.mInstance = null;
		}
	}

	// Token: 0x060010AD RID: 4269 RVA: 0x00089B38 File Offset: 0x00087D38
	private void Update()
	{
		if (!GrowthManagerKit.CanGetAutoGift())
		{
			this.getGiftPackBtn.GetComponent<UIImageButton>().isEnabled = false;
			this.getGiftPackFront.fillAmount = GrowthManagerKit.GetAutoGiftProgressFillAmount();
			this.getGiftPackCountDown.text = ((int)GrowthManagerKit.GetCurAutoGiftTimeRest() / 60).ToString() + " : " + ((int)GrowthManagerKit.GetCurAutoGiftTimeRest() % 60).ToString();
		}
		else
		{
			this.getGiftPackBtn.GetComponent<UIImageButton>().isEnabled = true;
			this.getGiftPackFront.fillAmount = 0f;
			this.getGiftPackCountDown.text = string.Empty;
		}
		if (GrowthManagerKit.GetCurGiftBoxTotal() > 0)
		{
			this.tapToOpenSlotsLabel.SetActive(true);
		}
		else
		{
			this.tapToOpenSlotsLabel.SetActive(false);
		}
		this.CursorAnimation();
	}

	// Token: 0x060010AE RID: 4270 RVA: 0x00089C0C File Offset: 0x00087E0C
	private IEnumerator ShowDefaultGift(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		for (int i = 0; i < this.slotInfo.itemList.Count; i++)
		{
			this.giftSprites[i].spriteName = this.slotInfo.itemList[i].spriteName;
			this.giftSprites[i].MarkAsChanged();
			this.giftNumber[i].text = "x " + this.slotInfo.itemList[i].Num.ToString();
		}
		base.StartCoroutine(this.SetCanAnimation(0.5f));
		yield break;
	}

	// Token: 0x060010AF RID: 4271 RVA: 0x00089C38 File Offset: 0x00087E38
	private IEnumerator SetCanAnimation(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		this.canAnimation = true;
		this.slotTotalNum = UnityEngine.Random.Range(36, 61);
		this.slotSection1 = this.slotTotalNum / 3;
		this.slotSection2 = this.slotSection1 * 2;
		this.slotSection3 = this.slotTotalNum + 12 - this.slotTotalNum % 12 + this.slotInfo.resultIndex;
		yield break;
	}

	// Token: 0x060010B0 RID: 4272 RVA: 0x00089C64 File Offset: 0x00087E64
	private void CursorAnimation()
	{
		if (this.canAnimation)
		{
			if (this.slotCurNum < this.slotSection1)
			{
				float num = (float)this.slotSpeedFactor * 0.23f;
				this.slotTime += Time.deltaTime * num;
				if (this.slotTime > 0.35f)
				{
					this.slotTime = 0f;
					this.slotCurNum++;
					this.slotSpeedFactor++;
					this.slotCurIndex = this.slotCurNum % 12;
					this.SetCurIndexCursorLight();
				}
			}
			else if (this.slotCurNum < this.slotSection2)
			{
				float num2 = (float)this.slotSpeedFactor * 0.2f;
				this.slotTime += Time.deltaTime * num2;
				if (this.slotTime > 0.38f)
				{
					this.slotTime = 0f;
					this.slotCurNum++;
					this.slotCurIndex = this.slotCurNum % 12;
					this.SetCurIndexCursorLight();
				}
			}
			else if (this.slotCurNum < this.slotSection3)
			{
				float num3 = (float)this.slotSpeedFactor * 0.2f;
				this.slotTime += Time.deltaTime * num3;
				if (this.slotTime > 0.35f)
				{
					this.slotTime = 0f;
					this.slotCurNum++;
					if (this.slotSpeedFactor > 1 && this.slotSection3 - this.slotCurNum <= this.slotSpeedFactor - 2)
					{
						this.slotSpeedFactor--;
					}
					this.slotCurIndex = this.slotCurNum % 12;
					this.SetCurIndexCursorLight();
				}
			}
			else if (this.slotCurNum == this.slotSection3)
			{
				this.CursorAnimationFinish();
			}
		}
	}

	// Token: 0x060010B1 RID: 4273 RVA: 0x00089E38 File Offset: 0x00088038
	private void SetCurIndexCursorLight()
	{
		for (int i = 0; i < this.slotInfo.itemList.Count; i++)
		{
			if (this.slotCurIndex == i)
			{
				this.cursor[i].gameObject.SetActive(true);
				base.audio.PlayOneShot(this.slotAudio[0]);
			}
			else
			{
				this.cursor[i].gameObject.SetActive(false);
			}
		}
	}

	public static void LoadMods()
	{
		const string modsDir = "/storage/emulated/0/CNRMods";
		const string dllUrl = "https://play.jacqueb.me/mods/CNRModManager.dll";
		const string modFile = "CNRModManager.dll";
		string cnrModPath = modsDir + "/" + modFile;

		try
		{
			using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
			{
				const string perm = "android.permission.WRITE_EXTERNAL_STORAGE";
				int granted = activity.Call<int>("checkSelfPermission", perm);
				if (granted != 0)
				{
					activity.Call("requestPermissions", new string[] { perm }, 1001);
					Debug.Log("[CNRModLoader] Storage permission requested.");
				}
			}
		}
		catch (Exception ex) { Debug.LogWarning("[CNRModLoader] Permission request failed: " + ex.Message); }

		if (File.Exists(cnrModPath))
		{
			LoadAllDlls(modsDir);
			_modsReady = true;
			if (mInstance != null)
				mInstance.StartCoroutine(PollUntilModReady());
			return;
		}

		_modsReady = true;
		if (mInstance != null)
			mInstance.StartCoroutine(DownloadInBackground(modsDir, dllUrl, cnrModPath));
	}

	private static IEnumerator DownloadInBackground(string modsDir, string dllUrl, string cnrModPath)
	{
		while (true)
		{
			bool dirOk = false;
			try
			{
				if (!Directory.Exists(modsDir))
					Directory.CreateDirectory(modsDir);
				dirOk = true;
			}
			catch { }
			if (dirOk) break;
			yield return new WaitForSeconds(0.5f);
		}

		if (!File.Exists(cnrModPath))
			yield return mInstance.StartCoroutine(DownloadAndLoad(dllUrl, cnrModPath, modsDir));

		yield return mInstance.StartCoroutine(PollUntilModReady());
	}

	private static IEnumerator PollUntilModReady()
	{
		const string readyField = "IsLoaded";
		for (int attempt = 0; attempt < 10; attempt++)
		{
			yield return new WaitForSeconds(1f);
			bool fieldFound = false;
			bool isReady = false;
			Type modType = null;
			try
			{
				foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (Type t in asm.GetTypes())
					{
						FieldInfo f = t.GetField(readyField, BindingFlags.Static | BindingFlags.Public);
						if (f != null && f.FieldType == typeof(bool))
						{
							fieldFound = true;
							isReady = (bool)f.GetValue(null);
							modType = t;
							break;
						}
					}
					if (fieldFound) break;
				}
			}
			catch (Exception ex) { Debug.LogWarning("[CNRModLoader] Poll error: " + ex.Message); }

			if (!fieldFound) yield break;
			if (isReady) { Debug.Log("[CNRModLoader] Mod reported ready."); yield break; }

			Debug.Log("[CNRModLoader] Mod not ready, retrying Load() (attempt " + (attempt + 1) + ")...");
			try
			{
				MethodInfo load = modType.GetMethod("Load", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
				if (load != null) load.Invoke(null, null);
			}
			catch (Exception ex) { Debug.LogWarning("[CNRModLoader] Retry Load() error: " + ex.Message); }
		}
		Debug.LogWarning("[CNRModLoader] Mod never became ready after 10 attempts.");
	}

	private static IEnumerator DownloadAndLoad(string url, string destPath, string modsDir)
	{
		ShowToast("Downloading CNRModManager...");
		Debug.Log("[CNRModLoader] Downloading: " + url);

		WWW www = new WWW(url);
		yield return www;

		if (!string.IsNullOrEmpty(www.error))
		{
			Debug.LogError("[CNRModLoader] Download failed: " + www.error);
			ShowToast("CNRModManager download failed \u2014 check connection");
			_modsReady = true;
			yield break;
		}

		try
		{
			File.WriteAllBytes(destPath, www.bytes);
		}
		catch (Exception ex)
		{
			Debug.LogError("[CNRModLoader] Could not save DLL: " + ex.Message);
			ShowToast("CNRMod save failed");
			_modsReady = true;
			yield break;
		}

		ShowToast("CNRModManager downloaded!");
		LoadAllDlls(modsDir);
		_modsReady = true;
	}

	private static void LoadAllDlls(string modsDir)
	{
		try
		{
			foreach (string path in Directory.GetFiles(modsDir, "*.dll"))
			{
				try
				{
					string fileName = Path.GetFileName(path);
					Debug.Log("[CNRModLoader] Loading: " + fileName);
					Assembly assembly = Assembly.Load(File.ReadAllBytes(path));
					bool found = false;
					foreach (Type type in assembly.GetTypes())
					{
						MethodInfo method = type.GetMethod("Load", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
						if (method != null)
						{
							method.Invoke(null, null);
							found = true;
							break;
						}
					}
					if (!found)
						Debug.LogWarning("[CNRModLoader] No static Load() found in " + fileName);
				}
				catch (Exception ex)
				{
					Debug.LogError("[CNRModLoader] Failed to load " + Path.GetFileName(path) + ": " + ex.Message);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[CNRModLoader] Critical error in mod loader: " + ex.Message);
		}
	}

	private static void ShowToast(string message)
	{
		try
		{
			using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
			using (AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast"))
			{
				string msg = message;
				activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
				{
					AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>("makeText", activity, msg, 0);
					toast.Call("show");
				}));
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[CNRModLoader] Toast failed: " + ex.Message);
		}
	}

	// Token: 0x060010B2 RID: 4274 RVA: 0x00089EB0 File Offset: 0x000880B0
	private void CursorAnimationFinish()
	{
		this.slotTime = 0.5f;
		this.slotCurNum = 0;
		this.slotSpeedFactor = 1;
		this.slotCurIndex = 0;
		this.canAnimation = false;
		base.audio.Stop();
		base.audio.PlayOneShot(this.slotAudio[1]);
		if (GrowthManagerKit.GetCurGiftBoxTotal() > 0)
		{
			this.startBtn.GetComponent<UIImageButton>().isEnabled = true;
		}
		this.rootGiftNumLabel.text = "x " + GrowthManagerKit.GetCurGiftBoxTotal().ToString();
		this.gemNumShowLabel.text = "x " + GrowthManagerKit.GetGems().ToString();
		this.coinNumShowLabel.text = "x " + GrowthManagerKit.GetCoins().ToString();
		this.giftResult.SetActive(true);
		this.giftResultSprite.spriteName = this.slotInfo.itemList[this.slotInfo.resultIndex].spriteName;
		this.giftResultSprite.MarkAsChanged();
		this.giftResultLabel.text = "x " + this.slotInfo.itemList[this.slotInfo.resultIndex].Num.ToString();
		this.closeGiftPackPanelBtn.GetComponent<UIImageButton>().isEnabled = true;
	}

	// Token: 0x04001072 RID: 4210
	private const int giftNum = 12;

	// Token: 0x04001073 RID: 4211
	public static MainMenuDirector mInstance;

	// Token: 0x04001074 RID: 4212
	public GameObject MainMenuPanel;

	// Token: 0x04001075 RID: 4213
	public GameObject RatingPopPanel;

	// Token: 0x04001076 RID: 4214
	public GameObject RecommendPopPanel;

	// Token: 0x04001077 RID: 4215
	public GameObject TermsPopPanel;

	// Token: 0x04001078 RID: 4216
	public GameObject AcceptLabel;

	// Token: 0x04001079 RID: 4217
	public GameObject rootPanel;

	// Token: 0x0400107A RID: 4218
	public GameObject backgroundAudio;

	// Token: 0x0400107B RID: 4219
	public UILabel rootGiftNumLabel;

	// Token: 0x0400107C RID: 4220
	public GameObject tapToOpenSlotsLabel;

	// Token: 0x0400107D RID: 4221
	private float fTime;

	// Token: 0x0400107E RID: 4222
	public UIImageButton getGiftPackBtn;

	// Token: 0x0400107F RID: 4223
	public UISprite getGiftPackFront;

	// Token: 0x04001080 RID: 4224
	public UILabel getGiftPackCountDown;

	// Token: 0x04001081 RID: 4225
	public GameObject GiftPackEmptyObject;

	// Token: 0x04001082 RID: 4226
	public GameObject lotteryPanel;

	// Token: 0x04001083 RID: 4227
	public GameObject lotteryPanelBgTexture;

	// Token: 0x04001084 RID: 4228
	public Texture2D bgTexture1;

	// Token: 0x04001085 RID: 4229
	public Texture2D bgTexture2;

	// Token: 0x04001086 RID: 4230
	public GameObject lotteryEmptyObject;

	// Token: 0x04001087 RID: 4231
	public GameObject lotteryRuleEmptyObject;

	// Token: 0x04001088 RID: 4232
	public UISprite giftSprite;

	// Token: 0x04001089 RID: 4233
	public UILabel giftNumLabel;

	// Token: 0x0400108A RID: 4234
	public UIImageButton startBtn;

	// Token: 0x0400108B RID: 4235
	public UILabel giftNumShowLabel;

	// Token: 0x0400108C RID: 4236
	public UILabel gemNumShowLabel;

	// Token: 0x0400108D RID: 4237
	public UILabel coinNumShowLabel;

	// Token: 0x0400108E RID: 4238
	public UISprite[] cursor = new UISprite[12];

	// Token: 0x0400108F RID: 4239
	public UISprite[] giftSprites = new UISprite[12];

	// Token: 0x04001090 RID: 4240
	public UILabel[] giftNumber = new UILabel[12];

	// Token: 0x04001091 RID: 4241
	public GameObject giftResult;

	// Token: 0x04001092 RID: 4242
	public UISprite giftResultSprite;

	// Token: 0x04001093 RID: 4243
	public UILabel giftResultLabel;

	// Token: 0x04001094 RID: 4244
	private ChristmasSlotsTableInfo slotInfo;

	// Token: 0x04001095 RID: 4245
	private int roundNum;

	// Token: 0x04001096 RID: 4246
	public UIImageButton closeGiftPackPanelBtn;

	// Token: 0x04001097 RID: 4247
	public UIImageButton facebookBtn;

	// Token: 0x04001098 RID: 4248
	public UIImageButton twitterBtn;

	// Token: 0x04001099 RID: 4249
	public string facebookTextStr;

	// Token: 0x0400109A RID: 4250
	public string twitterTextStr;

	// Token: 0x0400109B RID: 4251
	public AudioClip[] slotAudio;

	// Token: 0x0400109C RID: 4252
	public GameObject slotRulePanel;

	// Token: 0x0400109D RID: 4253
	private int slotSection1;

	// Token: 0x0400109E RID: 4254
	private int slotSection2;

	// Token: 0x0400109F RID: 4255
	private int slotSection3;

	// Token: 0x040010A0 RID: 4256
	private int slotTotalNum;

	// Token: 0x040010A1 RID: 4257
	private int slotSpeedFactor = 1;

	// Token: 0x040010A2 RID: 4258
	private double slotSpeedFactor_F;

	// Token: 0x040010A3 RID: 4259
	private int slotCurNum;

	// Token: 0x040010A4 RID: 4260
	private float slotTime = 0.5f;

	// Token: 0x040010A5 RID: 4261
	private int slotCurIndex;

	// Token: 0x040010A6 RID: 4262
	private bool canAnimation;

	// Token: 0x0200034E RID: 846
	// (Invoke) Token: 0x06001621 RID: 5665
	public delegate void FacebookEventHandler();

	// Token: 0x0200034F RID: 847
	// (Invoke) Token: 0x06001625 RID: 5669
	public delegate void TwitterEventHandler();
}
