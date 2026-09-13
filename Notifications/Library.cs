using System;
using System.Linq;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace MonoSandbox.Notifications
{
    [BepInPlugin("org.gorillatag.lars.notifications2", "NotificationLibrary", "1.0.5")]
    public class NotifiLib : BaseUnityPlugin
    {
        public static Font currentFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        private void Awake()
        {
            base.Logger.LogInfo("Plugin NotificationLibrary is loaded!");
        }

        private bool Init()
        {
            MainCamera = GameObject.Find("Main Camera");

            if (MainCamera == null)
            {
                Logger.LogWarning("NotificationLibrary: Main Camera not found.");
                return false;
            }

            Camera camera = MainCamera.GetComponent<Camera>();

            if (camera == null)
            {
                Logger.LogWarning("NotificationLibrary: Main Camera has no Camera component.");
                return false;
            }

            HUDObj = new GameObject("NOTIFICATIONLIB_HUD_OBJ");
            HUDObj2 = new GameObject("NOTIFICATIONLIB_HUD_OBJ_ROOT");

            Canvas canvas = HUDObj.AddComponent<Canvas>();
            HUDObj.AddComponent<CanvasScaler>();
            HUDObj.AddComponent<GraphicRaycaster>();

            canvas.enabled = true;
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            this.HUDObj.GetComponent<RectTransform>().sizeDelta = new Vector2(5f, 5f);
            this.HUDObj.GetComponent<RectTransform>().position = new Vector3(this.MainCamera.transform.position.x, this.MainCamera.transform.position.y, this.MainCamera.transform.position.z);
            this.HUDObj2.transform.position = new Vector3(this.MainCamera.transform.position.x, this.MainCamera.transform.position.y, this.MainCamera.transform.position.z - 4.6f);
            this.HUDObj.transform.parent = this.HUDObj2.transform;
            this.HUDObj.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 1.6f);
            Vector3 eulerAngles = this.HUDObj.GetComponent<RectTransform>().rotation.eulerAngles;
            eulerAngles.y = -270f;
            this.HUDObj.transform.localScale = new Vector3(1f, 1f, 1f);
            this.HUDObj.GetComponent<RectTransform>().rotation = Quaternion.Euler(eulerAngles);
            this.Testtext = new GameObject
            {
                transform =
                {
                    parent = this.HUDObj.transform
                }
            }.AddComponent<Text>();
            this.Testtext.text = "";
            this.Testtext.fontSize = 30;
            this.Testtext.font = currentFont;
            this.Testtext.rectTransform.sizeDelta = new Vector2(450f, 210f);
            this.Testtext.alignment = TextAnchor.LowerLeft;
            this.Testtext.rectTransform.localScale = new Vector3(0.00333333333f, 0.00333333333f, 0.33333333f);
            this.Testtext.rectTransform.localPosition = new Vector3(-1f, -1f, -0.5f);
            this.Testtext.material = this.AlertText;
            NotifiLib.NotifiText = this.Testtext;
            
            return true;
        }

        private void FixedUpdate()
        {
            if (!HasInit)
            {
                MainCamera = GameObject.Find("Main Camera");

                if (MainCamera == null)
                    return;

                Init();

                if (HUDObj2 == null || Testtext == null)
                    return;

                HasInit = true;
            }

            if (MainCamera == null || HUDObj2 == null || Testtext == null)
                return;

            HUDObj2.transform.position = MainCamera.transform.position;
            HUDObj2.transform.rotation = MainCamera.transform.rotation;

            if (!string.IsNullOrEmpty(Testtext.text))
            {
                NotificationDecayTimeCounter++;

                if (NotificationDecayTimeCounter > NotificationDecayTime)
                {
                    Notifilines = null;
                    newtext = "";
                    NotificationDecayTimeCounter = 0;

                    Notifilines = Testtext.text
                        .Split(Environment.NewLine.ToCharArray())
                        .Skip(1)
                        .ToArray();

                    foreach (string text in Notifilines)
                    {
                        if (!string.IsNullOrEmpty(text))
                            newtext += text + "\n";
                    }

                    Testtext.text = newtext;
                }
            }
            else
            {
                NotificationDecayTimeCounter = 0;
            }
        }

        public static void SendNotification(string NotificationText)
        {
                try
                {
                    if (NotifiLib.IsEnabled && NotifiLib.PreviousNotifi != NotificationText)
                    {
                        if (!NotificationText.Contains(Environment.NewLine))
                        {
                            NotificationText += Environment.NewLine;
                        }
                        NotifiLib.NotifiText.text = NotifiLib.NotifiText.text + NotificationText;
                        NotifiLib.NotifiText.supportRichText = true;
                        NotifiLib.PreviousNotifi = NotificationText;
                    }
                }
                catch
                {
                    UnityEngine.Debug.LogError("Notification failed, object probably nil due to third person ; " + NotificationText);
                }
        }

        public static void ClearAllNotifications()
        {
            //NotifiLib.NotifiText.text = "<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> <color=white>Notifications cleared.</color>" + Environment.NewLine;
            NotifiLib.NotifiText.text = "";
        }

        public static void ClearPastNotifications(int amount)
        {
            string text = "";
            foreach (string text2 in Enumerable.ToArray<string>(Enumerable.Skip<string>(NotifiLib.NotifiText.text.Split(Environment.NewLine.ToCharArray()), amount)))
            {
                if (text2 != "")
                {
                    text = text + text2 + "\n";
                }
            }
            NotifiLib.NotifiText.text = text;
        }

        private GameObject HUDObj;

        private GameObject HUDObj2;

        private GameObject MainCamera;

        private Text Testtext;

        private Material AlertText = new Material(Shader.Find("GUI/Text Shader"));

        private int NotificationDecayTime = 144;

        private int NotificationDecayTimeCounter;

        public static int NoticationThreshold = 30;

        private string[] Notifilines;

        private string newtext;

        public static string PreviousNotifi;

        private bool HasInit;

        private static Text NotifiText;

        public static bool IsEnabled = true;
    }
}
