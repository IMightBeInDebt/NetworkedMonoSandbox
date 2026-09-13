using BepInEx;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MonoSandbox.Behaviours;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;
using Object = UnityEngine.Object;
using Plugin = Ragdoll.Main;

#nullable disable
namespace Ragdoll;

[BepInPlugin(MonoSandbox.PluginInfo.GUIDRagdoll, MonoSandbox.PluginInfo.NameRagdoll, MonoSandbox.PluginInfo.VersionRagdoll)]
public class Main : BaseUnityPlugin
{
  public static Main instance;
  private static AssetBundle assetBundle;
  public static Dictionary<string, AudioClip> audioPool = new Dictionary<string, AudioClip>();
  private static List<GameObject> portedCosmetics = new List<GameObject>();
  private Queue<Vector3> posHistory = new Queue<Vector3>();
  private Queue<float> posTimes = new Queue<float>();
  public bool hasInit;
  public bool IsSteam;
  public float endDeathSoundTime = -1f;
  public bool lastLeftHeld;
  public GameObject ui;
  public Coroutine uiCoroutine;
  public static Vector3 startForward;
  public static bool isDead;
  public static GameObject Ragdoll;
  public static bool showGui;
  public static bool showHintText = true;
  public static bool fbtEnabled = true;
  public static bool freeMoveEnabled = false;
  public static bool ragdollVelocityEnabled = true;
  
  public static GameObject LoadAsset(string assetName)
  {
    GameObject gameObject = (GameObject) null;
    Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NetworkedMonoSandbox.Assets.ragdollbundle");
    if (manifestResourceStream != null)
    {
      if ((Object) Plugin.assetBundle == (Object) null)
        Plugin.assetBundle = AssetBundle.LoadFromStream(manifestResourceStream);
      gameObject = Object.Instantiate<GameObject>(Plugin.assetBundle.LoadAsset<GameObject>(assetName));
    }
    else
      Debug.LogError((object) ("Failed to load asset from resource: " + assetName));
    return gameObject;
  }
  
  private void Awake()
  {
    instance = this;
  }

  public static AudioClip LoadSoundFromResource(string resourcePath)
  {
    AudioClip audioClip = (AudioClip) null;
    if (!Plugin.audioPool.ContainsKey(resourcePath))
    {
      Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NetworkedMonoSandbox.Assets.ragdollbundle");
      if (manifestResourceStream != null)
      {
        if ((Object) Plugin.assetBundle == (Object) null)
          Plugin.assetBundle = AssetBundle.LoadFromStream(manifestResourceStream);
        audioClip = Plugin.assetBundle.LoadAsset(resourcePath) as AudioClip;
        Plugin.audioPool.Add(resourcePath, audioClip);
      }
      else
        Debug.LogError((object) ("Failed to load sound from resource: " + resourcePath));
    }
    else
      audioClip = Plugin.audioPool[resourcePath];
    return audioClip;
  }

  public static void DisableCosmetics()
  {
    try
    {
      VRRig.LocalRig.transform.Find("rig/body_pivot/TransferrableItemLeftShoulder").gameObject.SetActive(false);
      VRRig.LocalRig.transform.Find("rig/body_pivot/TransferrableItemRightShoulder").gameObject.SetActive(false);
      VRRig.LocalRig.transform.Find("rig/head/gorillaface").gameObject.layer = LayerMask.NameToLayer("Default");
      foreach (GameObject cosmetic in VRRig.LocalRig.cosmetics)
      {
        if (cosmetic.activeSelf && (Object) cosmetic.transform.parent == (Object) VRRig.LocalRig.mainCamera.transform.Find("HeadCosmetics"))
        {
          Plugin.portedCosmetics.Add(cosmetic);
          cosmetic.transform.SetParent(VRRig.LocalRig.headMesh.transform, false);
          cosmetic.transform.localPosition += new Vector3(0.0f, 0.1333f, 0.1f);
        }
      }
    }
    catch
    {
    }
  }

  public static void EnableCosmetics()
  {
    VRRig.LocalRig.transform.Find("rig/body_pivot/TransferrableItemLeftShoulder").gameObject.SetActive(true);
    VRRig.LocalRig.transform.Find("rig/body_pivot/TransferrableItemRightShoulder").gameObject.SetActive(true);
    VRRig.LocalRig.transform.Find("rig/head/gorillaface").gameObject.layer = LayerMask.NameToLayer("MirrorOnly");
    foreach (GameObject portedCosmetic in Plugin.portedCosmetics)
    {
      portedCosmetic.transform.SetParent(VRRig.LocalRig.mainCamera.transform.Find("HeadCosmetics"), false);
      portedCosmetic.transform.localPosition -= new Vector3(0.0f, 0.1333f, 0.1f);
    }
    Plugin.portedCosmetics.Clear();
  }

  private void TrackVelocity()
  {
    this.posHistory.Enqueue(GTPlayer.Instance.transform.position);
    this.posTimes.Enqueue(Time.time);
    while (this.posTimes.Count > 0 && (double) Time.time - (double) this.posTimes.Peek() > 0.30000001192092896)
    {
      this.posHistory.Dequeue();
      double num = (double) this.posTimes.Dequeue();
    }
  }

  public Vector3 GetAverageVelocity()
  {
    if (this.posHistory.Count < 2)
      return Vector3.zero;
    float num = this.posTimes.ToArray()[this.posTimes.Count - 1] - this.posTimes.ToArray()[0];
    return (double) num <= 0.0 ? Vector3.zero : (this.posHistory.ToArray()[this.posHistory.Count - 1] - this.posHistory.ToArray()[0]) / num;
  }

  public void Die()
  {
    if ((Object) Plugin.Ragdoll != (Object) null)
      Object.Destroy((Object) Plugin.Ragdoll);
    Plugin.DisableCosmetics();
    this.endDeathSoundTime = Time.time + 5.265f;
    Plugin.Ragdoll = Plugin.LoadAsset("ragdoll");
    Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body").transform.position = VRRig.LocalRig.transform.Find("rig/body_pivot").position;
    Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body").transform.rotation = VRRig.LocalRig.transform.Find("rig/body_pivot").rotation;
    Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").transform.position = VRRig.LocalRig.leftHand.rigTarget.transform.position;
    Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").transform.rotation = VRRig.LocalRig.leftHand.rigTarget.transform.rotation;
    Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").transform.position = VRRig.LocalRig.rightHand.rigTarget.transform.position;
    Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").transform.rotation = VRRig.LocalRig.rightHand.rigTarget.transform.rotation;
    if (Plugin.ragdollVelocityEnabled)
    {
      Vector3 averageVelocity = this.GetAverageVelocity();
      string[] strArray = new string[8]
      {
        "Stand/Gorilla Rig/body",
        "Stand/Gorilla Rig/body/head",
        "Stand/Gorilla Rig/body/shoulder.L",
        "Stand/Gorilla Rig/body/shoulder.R",
        "Stand/Gorilla Rig/body/shoulder.L/upper_arm.L",
        "Stand/Gorilla Rig/body/shoulder.R/upper_arm.R",
        "Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L",
        "Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R"
      };
      foreach (string n in strArray)
        Plugin.Ragdoll.transform.Find(n).GetComponent<Rigidbody>().linearVelocity = averageVelocity;
      Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").GetComponent<Rigidbody>().linearVelocity = GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0.0f);
      Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").GetComponent<Rigidbody>().angularVelocity = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/LeftHand Controller").GetOrAddComponent<GorillaVelocityEstimator>().angularVelocity;
      Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").GetComponent<Rigidbody>().linearVelocity = GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0.0f);
      Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").GetComponent<Rigidbody>().angularVelocity = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/RightHand Controller").GetOrAddComponent<GorillaVelocityEstimator>().angularVelocity;
    }
    Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body/head").transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
    VRRig.LocalRig.head.rigTarget.transform.rotation = Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body/head").transform.rotation;
    Transform transform = Plugin.Ragdoll.transform.Find("Stand/Mesh");
    if ((Object) transform != (Object) null)
      transform.gameObject.SetActive(false);
    foreach (Renderer componentsInChild in Plugin.Ragdoll.GetComponentsInChildren<Renderer>(true))
    {
      foreach (Material material in componentsInChild.materials)
        material.renderQueue = 3000;
    }
    Plugin.startForward = Plugin.Ragdoll.transform.forward;
    if (this.uiCoroutine != null)
    {
      ((MonoBehaviour) this).StopCoroutine(this.uiCoroutine);
      this.uiCoroutine = (Coroutine) null;
    }
    else
      this.uiCoroutine = ((MonoBehaviour) this).StartCoroutine(this.ShowGModUI());
    AudioClip audioClip = Plugin.LoadSoundFromResource("GMOD-Net");
    if (!((Object) GorillaTagger.Instance.myRecorder != (Object) null))
      return;
    GorillaTagger.Instance.myRecorder.SourceType = Recorder.InputSourceType.AudioClip;
    GorillaTagger.Instance.myRecorder.AudioClip = audioClip;
    GorillaTagger.Instance.myRecorder.RestartRecording(true);
  }

  public static Vector3 World2Player(Vector3 world)
  {
    return world - GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.transform.position;
  }

  public bool GetLeftSecondaryDown()
  {
    return InputHandling.LeftSecondary;
  }

  public IEnumerator ShowGModUI()
  {
    this.ui = Plugin.LoadAsset("UI");
    this.ui.transform.parent = GameObject.Find("Main Camera").transform;
    this.ui.transform.localPosition = Vector3.zero;
    this.ui.transform.localRotation = Quaternion.identity;
    this.ui.transform.Find("Cube/Canvas/Name").GetComponent<Text>().text = PhotonNetwork.NickName;
    this.ui.transform.Find("Cube/Canvas/Name/Shadow").GetComponent<Text>().text = PhotonNetwork.NickName;
    float startTime = Time.time + 5f;
    while ((double) Time.time < (double) startTime)
    {
      this.ui.transform.Find("Cube").gameObject.GetComponent<Renderer>().material.color = new Color(0.8980392f, 0.227450982f, 0.129411772f, Mathf.Lerp(0.0f, 0.15f, (float) (((double) startTime - (double) Time.time) / 5.0)));
      yield return (object) null;
    }
    this.ui.transform.Find("Cube").gameObject.GetComponent<Renderer>().material.color = Color.clear;
    yield return (object) new WaitForSeconds(5f);
    Object.Destroy((Object) this.ui);
    Coroutine thisCoroutine = this.uiCoroutine;
    this.uiCoroutine = (Coroutine) null;
    ((MonoBehaviour) this).StopCoroutine(thisCoroutine);
  }

  public Vector2 GetLeftJoystickAxis()
  {
    if (this.IsSteam)
      return SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.GetAxis((SteamVR_Input_Sources) 1);
    Vector2 leftJoystickAxis;
    ControllerInputPoller.instance.leftControllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftJoystickAxis);
    return leftJoystickAxis;
  }

  public void Update()
  {
    if ((Object) GTPlayer.Instance == (Object) null)
      return;
    if (!this.hasInit)
    {
      this.hasInit = true;
      this.IsSteam = Traverse.Create((object) PlayFabAuthenticator.instance).Field("platform").GetValue().ToString().ToLower() == "steam";
    }
    this.TrackVelocity();
    bool flag = this.GetLeftSecondaryDown() || UnityInput.Current.GetKey(KeyCode.B);
    if (flag && MonoSandbox.Plugin.InRoom && !this.lastLeftHeld || flag && MonoSandbox.Plugin.OverrideUtilla && !this.lastLeftHeld)
    {
      Plugin.isDead = !Plugin.isDead;
      if (Plugin.isDead)
        this.Die();
    }
    this.lastLeftHeld = flag;
    if (UnityInput.Current.GetKeyDown(KeyCode.Z))
      Plugin.showGui = !Plugin.showGui;
    if ((double) Time.time > (double) this.endDeathSoundTime && (double) this.endDeathSoundTime > 0.0)
    {
      if ((Object) GorillaTagger.Instance.myRecorder != (Object) null)
      {
        GorillaTagger.Instance.myRecorder.AudioClip = Plugin.LoadSoundFromResource("Silence");
        GorillaTagger.Instance.myRecorder.RestartRecording(true);
      }
      this.endDeathSoundTime = -1f;
    }
    if (Plugin.isDead)
    {
      if (!((Object) Plugin.Ragdoll != (Object) null))
        return;
      this.UpdateRigPos();
    }
    else if ((Object) Plugin.Ragdoll != (Object) null)
    {
      Plugin.EnableCosmetics();
      this.posHistory.Clear();
      this.posTimes.Clear();
      Vector3 position = Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body").position;
      Object.Destroy((Object) Plugin.Ragdoll);
      Plugin.Ragdoll = (GameObject) null;
      if ((Object) GorillaTagger.Instance.myRecorder != (Object) null)
      {
        GorillaTagger.Instance.myRecorder.SourceType = Recorder.InputSourceType.Microphone;
        GorillaTagger.Instance.myRecorder.AudioClip = (AudioClip) null;
        GorillaTagger.Instance.myRecorder.RestartRecording(true);
      }
      if (this.uiCoroutine != null)
      {
        ((MonoBehaviour) this).StopCoroutine(this.uiCoroutine);
        this.uiCoroutine = (Coroutine) null;
      }
      if ((Object) this.ui != (Object) null)
        Object.Destroy((Object) this.ui);
      if ((double) position.y >= -10.0)
        GTPlayer.Instance.TeleportTo(Plugin.World2Player(position), GTPlayer.Instance.transform.rotation);
    }
  }

  public void UpdateRigPos()
  {
    if ((Object) Plugin.Ragdoll == (Object) null)
      return;
    Transform transform1 = Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body");
    if ((Object) transform1 == (Object) null)
      return;
    Vector3 position = transform1.position;
    if ((double) position.y < -10.0)
      return;
    VRRig.LocalRig.transform.position = position;
    VRRig.LocalRig.transform.rotation = transform1.rotation;
    Transform transform2 = transform1.Find("shoulder.L/upper_arm.L/forearm.L/hand.L");
    Transform transform3 = transform1.Find("shoulder.R/upper_arm.R/forearm.R/hand.R");
    Transform transform4 = transform1.Find("head");
    if ((Object) transform2 != (Object) null)
    {
      VRRig.LocalRig.leftHand.rigTarget.transform.position = transform2.position;
      VRRig.LocalRig.leftHand.rigTarget.transform.rotation = transform2.rotation * Quaternion.Euler(0.0f, 0.0f, 75f);
    }
    if ((Object) transform3 != (Object) null)
    {
      VRRig.LocalRig.rightHand.rigTarget.transform.position = transform3.position;
      VRRig.LocalRig.rightHand.rigTarget.transform.rotation = transform3.rotation * Quaternion.Euler(180f, 0.0f, -75f);
    }
    if ((Object) transform4 != (Object) null)
    {
      VRRig.LocalRig.head.rigTarget.transform.position = transform4.position;
      VRRig.LocalRig.head.rigTarget.transform.rotation = transform4.rotation;
    }
    if (Plugin.freeMoveEnabled)
      return;
    GTPlayer.Instance.TeleportTo(Plugin.World2Player(position + Plugin.startForward * 2f + new Vector3(0.0f, 2f, 0.0f)), GTPlayer.Instance.transform.rotation);
    GorillaTagger.Instance.leftHandTransform.position = GorillaTagger.Instance.bodyCollider.transform.position;
    GorillaTagger.Instance.rightHandTransform.position = GorillaTagger.Instance.bodyCollider.transform.position;
    GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
  }

  public static void SyncRigToRagdoll(VRRig rig)
  {
    if ((Object) Plugin.Ragdoll == (Object) null)
      return;
    Transform transform1 = Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body");
    if ((Object) transform1 == (Object) null)
      return;
    Vector3 position = transform1.position;
    if ((double) position.y < -10.0)
      return;
    rig.transform.position = position;
    rig.transform.rotation = transform1.rotation;
    Transform transform2 = transform1.Find("shoulder.L/upper_arm.L/forearm.L/hand.L");
    Transform transform3 = transform1.Find("shoulder.R/upper_arm.R/forearm.R/hand.R");
    Transform transform4 = transform1.Find("head");
    if ((Object) transform2 != (Object) null)
    {
      rig.leftHand.rigTarget.transform.position = transform2.position;
      rig.leftHand.rigTarget.transform.rotation = transform2.rotation * Quaternion.Euler(0.0f, 0.0f, 75f);
    }
    if ((Object) transform3 != (Object) null)
    {
      rig.rightHand.rigTarget.transform.position = transform3.position;
      rig.rightHand.rigTarget.transform.rotation = transform3.rotation * Quaternion.Euler(180f, 0.0f, -75f);
    }
    if (!((Object) transform4 != (Object) null))
      return;
    rig.head.rigTarget.transform.position = transform4.position;
    rig.head.rigTarget.transform.rotation = transform4.rotation;
  }

  public void OnGUI()
  {
    GUI.color = new Color(1f, 1f, 1f, 0.15f);
    GUI.Label(new Rect(0.0f, (float) Screen.height - 20f, (float) Screen.width, 20f), "Ragdoll Settings");
    GUI.color = Color.white;
    if (Plugin.showHintText && !Plugin.showGui)
    {
      GUI.color = new Color(1f, 1f, 1f, 0.3f);
      GUI.Label(new Rect((float) ((double) Screen.width / 2.0 - 150.0), 10f, 300f, 30f), "press z to open ragdoll settings");
      GUI.color = Color.white;
    }
    if (!Plugin.showGui)
      return;
    float width = 300f;
    float height = 200f;
    float x = (float) ((double) Screen.width / 2.0 - (double) width / 2.0);
    float y = (float) ((double) Screen.height / 2.0 - (double) height / 2.0);
    GUI.Box(new Rect(x, y, width, height), "Ragdoll Settings");
    Plugin.ragdollVelocityEnabled = GUI.Toggle(new Rect(x + 20f, y + 30f, width - 40f, 30f), Plugin.ragdollVelocityEnabled, " Ragdoll Velocity");
    Plugin.freeMoveEnabled = GUI.Toggle(new Rect(x + 20f, y + 60f, width - 40f, 30f), Plugin.freeMoveEnabled, " Free Move (walk while ragdolled)");
    Plugin.showHintText = GUI.Toggle(new Rect(x + 20f, y + 90f, width - 40f, 30f), Plugin.showHintText, " Show Hint Text");
    if (!GUI.Button(new Rect(x + 100f, y + 130f, 100f, 25f), "Close (Z)"))
      return;
    Plugin.showGui = false;
  }
}
