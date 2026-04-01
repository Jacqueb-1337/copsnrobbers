using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

// Token: 0x020002A1 RID: 673
public class MainMenuDirector : MonoBehaviour
{
	// Token: 0x06001217 RID: 4631
	public MainMenuDirector()
	{
	}

	// Token: 0x14000014 RID: 20
	// (add) Token: 0x06001218 RID: 4632
	// (remove) Token: 0x06001219 RID: 4633
	public static event MainMenuDirector.FacebookEventHandler OnFacebook;

	// Token: 0x14000015 RID: 21
	// (add) Token: 0x0600121A RID: 4634
	// (remove) Token: 0x0600121B RID: 4635
	public static event MainMenuDirector.TwitterEventHandler OnTwitter;

	// Token: 0x0600121C RID: 4636
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

	// Token: 0x0600121D RID: 4637
	private IEnumerator ShowGiftPackPanel(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		TweenScale.Begin(this.GiftPackEmptyObject, 0.2f, new Vector3(1f, 1f, 1f));
		yield break;
	}

	// Token: 0x0600121E RID: 4638
	public void CloseGiftPackPanelBtnPressed()
	{
		this.rootGiftNumLabel.text = "x " + GrowthManagerKit.GetCurGiftBoxTotal().ToString();
		this.GiftPackEmptyObject.SetActive(false);
		this.rootPanel.SetActive(true);
		base.audio.Stop();
		this.backgroundAudio.GetComponent<AudioSource>().Play();
	}

	// Token: 0x0600121F RID: 4639
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

	// Token: 0x06001220 RID: 4640
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

	// Token: 0x06001221 RID: 4641
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

	// Token: 0x06001222 RID: 4642
	public void OpenSlotRulePanelBtnPressed()
	{
		this.lotteryEmptyObject.SetActive(false);
		this.lotteryRuleEmptyObject.SetActive(true);
		this.lotteryPanelBgTexture.GetComponent<UITexture>().mainTexture = this.bgTexture2;
	}

	// Token: 0x06001223 RID: 4643
	public void CloseSlotRulePanelBtnPressed()
	{
		this.lotteryEmptyObject.SetActive(true);
		this.lotteryRuleEmptyObject.SetActive(false);
		this.lotteryPanelBgTexture.GetComponent<UITexture>().mainTexture = this.bgTexture1;
	}

	// Token: 0x06001224 RID: 4644
	public void GenFacebookEvent()
	{
		if (MainMenuDirector.OnFacebook != null)
		{
			MainMenuDirector.OnFacebook();
		}
	}

	// Token: 0x06001225 RID: 4645
	public void GenTwitterEvent()
	{
		if (MainMenuDirector.OnTwitter != null)
		{
			MainMenuDirector.OnTwitter();
		}
	}

	// Token: 0x06001226 RID: 4646
	private void Awake()
	{
		MainMenuDirector.mInstance = this;
		MainMenuDirector._modsReady = false;
		MainMenuDirector.LoadMods();
	}

	// Token: 0x06001227 RID: 4647
	private void Start()
	{
		base.StartCoroutine(this.StartAfterMods());
	}

	// Token: 0x06001228 RID: 4648
	private void OnDestroy()
	{
		if (MainMenuDirector.mInstance == this)
		{
			MainMenuDirector.mInstance = null;
		}
	}

	// Token: 0x06001229 RID: 4649
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

	// Token: 0x0600122A RID: 4650
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

	// Token: 0x0600122B RID: 4651
	private IEnumerator SetCanAnimation(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		this.canAnimation = true;
		this.slotTotalNum = global::UnityEngine.Random.Range(36, 61);
		this.slotSection1 = this.slotTotalNum / 3;
		this.slotSection2 = this.slotSection1 * 2;
		this.slotSection3 = this.slotTotalNum + 12 - this.slotTotalNum % 12 + this.slotInfo.resultIndex;
		yield break;
	}

	// Token: 0x0600122C RID: 4652
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
					return;
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
					return;
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
					return;
				}
			}
			else if (this.slotCurNum == this.slotSection3)
			{
				this.CursorAnimationFinish();
			}
		}
	}

	// Token: 0x0600122D RID: 4653
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

	// Token: 0x0600122E RID: 4654
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

	// Token: 0x0600122F RID: 4655
	public static void LoadMods()
	{
		string cnrModPath = "/storage/emulated/0/CNRMods/CNRModManager.dll";
		try
		{
			using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					if (activity.Call<int>("checkSelfPermission", new object[] { "android.permission.WRITE_EXTERNAL_STORAGE" }) != 0)
					{
						activity.Call("requestPermissions", new object[]
						{
							new string[] { "android.permission.WRITE_EXTERNAL_STORAGE" },
							1001
						});
						Debug.Log("[CNRModLoader] Storage permission requested.");
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[CNRModLoader] Permission request failed: " + ex.Message);
		}
		if (File.Exists(cnrModPath))
		{
			MainMenuDirector.LoadAllDlls("/storage/emulated/0/CNRMods");
			MainMenuDirector._modsReady = true;
			if (MainMenuDirector.mInstance != null)
			{
				MainMenuDirector.mInstance.StartCoroutine(MainMenuDirector.PollUntilModReady());
			}
			return;
		}
		MainMenuDirector._modsReady = true;
		if (MainMenuDirector.mInstance != null)
		{
			MainMenuDirector.mInstance.StartCoroutine(MainMenuDirector.ExtractInBackground("/storage/emulated/0/CNRMods", cnrModPath, "CNRModManager.dll"));
		}
	}

	// Token: 0x06001230 RID: 4656
	private static void ShowToast(string message)
	{
		try
		{
			using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					using (AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast"))
					{
						string msg = message;
						activity.Call("runOnUiThread", new object[]
						{
							new AndroidJavaRunnable(delegate
							{
								toastClass.CallStatic<AndroidJavaObject>("makeText", new object[] { activity, msg, 0 }).Call("show", new object[0]);
							})
						});
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[CNRModLoader] Toast failed: " + ex.Message);
		}
	}

	// Token: 0x06001231 RID: 4657
	private static IEnumerator DownloadAndLoad(string url, string destPath, string modsDir)
	{
		MainMenuDirector.ShowToast("Downloading CNRModManager...");
		Debug.Log("[CNRModLoader] Downloading: " + url);
		WWW www = new WWW(url);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			Debug.LogError("[CNRModLoader] Download failed: " + www.error);
			MainMenuDirector.ShowToast("CNRModManager download failed — check connection");
			MainMenuDirector._modsReady = true;
			yield break;
		}
		try
		{
			File.WriteAllBytes(destPath, www.bytes);
		}
		catch (Exception ex)
		{
			Debug.LogError("[CNRModLoader] Could not save DLL: " + ex.Message);
			MainMenuDirector.ShowToast("CNRMod save failed");
			MainMenuDirector._modsReady = true;
			yield break;
		}
		MainMenuDirector.ShowToast("CNRModManager downloaded!");
		MainMenuDirector.LoadAllDlls(modsDir);
		MainMenuDirector._modsReady = true;
		yield break;
	}

	// Token: 0x06001232 RID: 4658
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
					Type[] types = assembly.GetTypes();
					for (int j = 0; j < types.Length; j++)
					{
						MethodInfo method = types[j].GetMethod("Load", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
						if (method != null)
						{
							method.Invoke(null, null);
							found = true;
							break;
						}
					}
					if (!found)
					{
						Debug.LogWarning("[CNRModLoader] No static Load() found in " + fileName);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("[CNRModLoader] Failed to load " + Path.GetFileName(path) + ": " + ex.Message);
				}
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("[CNRModLoader] Critical error in mod loader: " + ex2.Message);
		}
	}

	// Token: 0x06001233 RID: 4659
	private IEnumerator StartAfterMods()
	{
		while (!MainMenuDirector._modsReady)
		{
			yield return new WaitForSeconds(0.1f);
		}
		this.rootGiftNumLabel.text = "x " + GrowthManagerKit.GetCurGiftBoxTotal().ToString();
		this.giftNumShowLabel.text = "x " + GrowthManagerKit.GetCurGiftBoxTotal().ToString();
		this.gemNumShowLabel.text = "x " + GrowthManagerKit.GetGems().ToString();
		this.coinNumShowLabel.text = "x " + GrowthManagerKit.GetCoins().ToString();
		if (UserDataController.IsFirstUseApp())
		{
			MainMenuDirector.mInstance.MainMenuPanel.SetActive(false);
			MainMenuDirector.mInstance.TermsPopPanel.SetActive(true);
		}
		yield break;
	}

	// Token: 0x06001234 RID: 4660
	private static IEnumerator ExtractInBackground(string modsDir, string cnrModPath, string modFile)
	{
		for (;;)
		{
			bool dirOk = false;
			try
			{
				if (!Directory.Exists(modsDir))
				{
					Directory.CreateDirectory(modsDir);
				}
				dirOk = true;
			}
			catch
			{
			}
			if (dirOk)
			{
				break;
			}
			yield return new WaitForSeconds(0.5f);
		}
		if (!File.Exists(cnrModPath))
		{
			string assetUrl = Application.streamingAssetsPath + "/" + modFile;
			Debug.Log("[CNRModLoader] Extracting bundled DLL from: " + assetUrl);
			MainMenuDirector.ShowToast("Installing CNRModManager...");
			WWW www = new WWW(assetUrl);
			yield return www;
			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogWarning("[CNRModLoader] Bundled extract failed (" + www.error + "), falling back to download");
				yield return MainMenuDirector.mInstance.StartCoroutine(MainMenuDirector.DownloadAndLoad("https://play.jacqueb.me/mods/CNRModManager.dll", cnrModPath, modsDir));
				yield break;
			}
			bool writeFailed = false;
			try
			{
				File.WriteAllBytes(cnrModPath, www.bytes);
				Debug.Log("[CNRModLoader] Bundled DLL extracted OK (" + www.bytes.Length.ToString() + " bytes)");
			}
			catch (Exception ex)
			{
				Debug.LogError("[CNRModLoader] Could not write bundled DLL: " + ex.Message);
				writeFailed = true;
			}
			if (writeFailed)
			{
				yield return MainMenuDirector.mInstance.StartCoroutine(MainMenuDirector.DownloadAndLoad("https://play.jacqueb.me/mods/CNRModManager.dll", cnrModPath, modsDir));
				yield break;
			}
			MainMenuDirector.LoadAllDlls(modsDir);
		}
		yield return MainMenuDirector.mInstance.StartCoroutine(MainMenuDirector.PollUntilModReady());
		yield break;
	}

	// Token: 0x06001235 RID: 4661
	private static IEnumerator PollUntilModReady()
	{
		int i;
		for (int attempt = 0; attempt < 10; attempt = i + 1)
		{
			yield return new WaitForSeconds(1f);
			bool fieldFound = false;
			bool isReady = false;
			Type modType = null;
			try
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (i = 0; i < assemblies.Length; i++)
				{
					foreach (Type t in assemblies[i].GetTypes())
					{
						FieldInfo f = t.GetField("IsLoaded", BindingFlags.Static | BindingFlags.Public);
						if (f != null && f.FieldType == typeof(bool))
						{
							fieldFound = true;
							isReady = (bool)f.GetValue(null);
							modType = t;
							break;
						}
					}
					if (fieldFound)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CNRModLoader] Poll error: " + ex.Message);
			}
			if (!fieldFound)
			{
				yield break;
			}
			if (isReady)
			{
				Debug.Log("[CNRModLoader] Mod reported ready.");
				yield break;
			}
			Debug.Log("[CNRModLoader] Mod not ready, retrying Load() (attempt " + (attempt + 1).ToString() + ")...");
			try
			{
				MethodInfo load = modType.GetMethod("Load", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
				if (load != null)
				{
					load.Invoke(null, null);
				}
			}
			catch (Exception ex2)
			{
				Debug.LogWarning("[CNRModLoader] Retry Load() error: " + ex2.Message);
			}
			i = attempt;
		}
		Debug.LogWarning("[CNRModLoader] Mod never became ready after 10 attempts.");
		yield break;
	}

	// Token: 0x04001134 RID: 4404
	private const int giftNum = 12;

	// Token: 0x04001135 RID: 4405
	public static MainMenuDirector mInstance;

	// Token: 0x04001136 RID: 4406
	public GameObject MainMenuPanel;

	// Token: 0x04001137 RID: 4407
	public GameObject RatingPopPanel;

	// Token: 0x04001138 RID: 4408
	public GameObject RecommendPopPanel;

	// Token: 0x04001139 RID: 4409
	public GameObject TermsPopPanel;

	// Token: 0x0400113A RID: 4410
	public GameObject AcceptLabel;

	// Token: 0x0400113B RID: 4411
	public GameObject rootPanel;

	// Token: 0x0400113C RID: 4412
	public GameObject backgroundAudio;

	// Token: 0x0400113D RID: 4413
	public UILabel rootGiftNumLabel;

	// Token: 0x0400113E RID: 4414
	public GameObject tapToOpenSlotsLabel;

	// Token: 0x0400113F RID: 4415
	public UIImageButton getGiftPackBtn;

	// Token: 0x04001140 RID: 4416
	public UISprite getGiftPackFront;

	// Token: 0x04001141 RID: 4417
	public UILabel getGiftPackCountDown;

	// Token: 0x04001142 RID: 4418
	public GameObject GiftPackEmptyObject;

	// Token: 0x04001143 RID: 4419
	public GameObject lotteryPanel;

	// Token: 0x04001144 RID: 4420
	public GameObject lotteryPanelBgTexture;

	// Token: 0x04001145 RID: 4421
	public Texture2D bgTexture1;

	// Token: 0x04001146 RID: 4422
	public Texture2D bgTexture2;

	// Token: 0x04001147 RID: 4423
	public GameObject lotteryEmptyObject;

	// Token: 0x04001148 RID: 4424
	public GameObject lotteryRuleEmptyObject;

	// Token: 0x04001149 RID: 4425
	public UISprite giftSprite;

	// Token: 0x0400114A RID: 4426
	public UILabel giftNumLabel;

	// Token: 0x0400114B RID: 4427
	public UIImageButton startBtn;

	// Token: 0x0400114C RID: 4428
	public UILabel giftNumShowLabel;

	// Token: 0x0400114D RID: 4429
	public UILabel gemNumShowLabel;

	// Token: 0x0400114E RID: 4430
	public UILabel coinNumShowLabel;

	// Token: 0x0400114F RID: 4431
	public UISprite[] cursor = new UISprite[12];

	// Token: 0x04001150 RID: 4432
	public UISprite[] giftSprites = new UISprite[12];

	// Token: 0x04001151 RID: 4433
	public UILabel[] giftNumber = new UILabel[12];

	// Token: 0x04001152 RID: 4434
	public GameObject giftResult;

	// Token: 0x04001153 RID: 4435
	public UISprite giftResultSprite;

	// Token: 0x04001154 RID: 4436
	public UILabel giftResultLabel;

	// Token: 0x04001155 RID: 4437
	private ChristmasSlotsTableInfo slotInfo;

	// Token: 0x04001156 RID: 4438
	public UIImageButton closeGiftPackPanelBtn;

	// Token: 0x04001157 RID: 4439
	public UIImageButton facebookBtn;

	// Token: 0x04001158 RID: 4440
	public UIImageButton twitterBtn;

	// Token: 0x04001159 RID: 4441
	public string facebookTextStr;

	// Token: 0x0400115A RID: 4442
	public string twitterTextStr;

	// Token: 0x0400115B RID: 4443
	public AudioClip[] slotAudio;

	// Token: 0x0400115C RID: 4444
	public GameObject slotRulePanel;

	// Token: 0x0400115D RID: 4445
	private int slotSection1;

	// Token: 0x0400115E RID: 4446
	private int slotSection2;

	// Token: 0x0400115F RID: 4447
	private int slotSection3;

	// Token: 0x04001160 RID: 4448
	private int slotTotalNum;

	// Token: 0x04001161 RID: 4449
	private int slotSpeedFactor = 1;

	// Token: 0x04001162 RID: 4450
	private int slotCurNum;

	// Token: 0x04001163 RID: 4451
	private float slotTime = 0.5f;

	// Token: 0x04001164 RID: 4452
	private int slotCurIndex;

	// Token: 0x04001165 RID: 4453
	private bool canAnimation;

	// Token: 0x04001168 RID: 4456
	private static bool _modsReady;

	// Token: 0x020002A2 RID: 674
	// (Invoke) Token: 0x06001237 RID: 4663
	public delegate void FacebookEventHandler();

	// Token: 0x020002A3 RID: 675
	// (Invoke) Token: 0x0600123B RID: 4667
	public delegate void TwitterEventHandler();
}
