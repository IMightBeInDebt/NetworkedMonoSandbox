using UnityEngine;
using UnityEngine.UI;

namespace MonoSandbox.Behaviours.UI
{
    public class SandboxMenu : MonoBehaviour
    {
        public GameObject _menu, _text, _objParent, _toolParent, _modsParent, _utilsParent, _weaponsParent, _settingsParent, _sideBtnParent, _sender;
        public int _currentPage;

        // --------------------------------------------- page one ----------------------------------------\\
        public bool[] objectButtons = new bool[12], weaponButtons = new bool[12], toolButtons = new bool[8], utilButtons = new bool[8], settingsButtons = new bool[12], modsButtons = new bool[4], nextOrPreviousPageButtons = new bool[0], sideBtnButtons = new bool[4];
        public string[] objectNames = new string[12] { "Box", "Sphere", "Bean", "Crate", "Barrel", "Wheel", "Couch", "Plane", "Entity", "Gorilla", "Bathtub", "Soft Sphere" };
        public string[] weaponNames = new string[12] { "Revolver", "Shotgun", "Rifle", "Sniper", "Melon Cannon", "C4", "Airstrike", "Laser Gun", "Banana Gun", "Mine", "Grenade", "Hammer" };
        public string[] toolNames = new string[8] { "Weld", "Thruster", "Spring", "Gravity Gun", "Colourize", "Freeze", "Toggle Gravity", "Balloon" };
        public string[] utilNames = new string[8] { "Remove All", "Remove Thrusters", "Remove Springs", "Remove Balloons", "Enable Rain", "Disable Rain", "Enable Snow", "Disable Snow" };
        public string[] settingsNames = new string[12] { "Default Theme", "Red Theme", "Purple Theme", "Green Theme", "Speedboost Strength", "Small Boost", "Medium Boost", "Large Boost", "Fly Speeds", "Slow Fly", "Medium Fly", "Fast Fly", };
        public string[] modsNames = new string[4] { "Fly", "Iron Monke", "Platforms", "Speedboost" };

        private Canvas _canvas;
        private AudioSource _audioSource;

        public void Start()
        {
            _menu = transform.GetChild(1).gameObject;
            _canvas = _menu.transform.GetChild(0).gameObject.GetComponent<Canvas>();
            _objParent = new GameObject();
            _weaponsParent = new GameObject();
            _toolParent = new GameObject();
            _utilsParent = new GameObject();
            _settingsParent = new GameObject();
            _modsParent = new GameObject();
            _sideBtnParent = new GameObject();
            _sideBtnParent.name = "SideButtons";
            _sideBtnParent.transform.SetParent(_menu.transform, false);

            AddPage(objectNames, PageManager.objectsName, 4, _objParent, 0);
            AddPage(weaponNames, PageManager.weaponsName, 4, _weaponsParent, 1);
            AddPage(toolNames, PageManager.toolsName, 4, _toolParent, 2);
            AddPage(utilNames, PageManager.utilsName, 4, _utilsParent, 3);
            AddPage(settingsNames, PageManager.settingsNames, 4, _settingsParent, 4);
            AddPage(modsNames, PageManager.modsNames, 4, _modsParent, 5);

            transform.SetParent(RefCache.LHand.transform, false);
            transform.localPosition = new Vector3(0, 0.14f, 0.075f);
            transform.localScale = Vector3.one * 0.5f;
            transform.localEulerAngles = new Vector3(0f, 90f, -5f);

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.volume = 0.4f;
            _audioSource.spatialBlend = 1f;
            _audioSource.clip = RefCache.PageSelection;
            _audioSource.Play();
        }

        public void Update()
        {
            if (_objParent == null || _weaponsParent == null || _utilsParent == null || _toolParent == null || _settingsParent == null || _modsParent == null) return;
            
                if (_currentPage == 0 && !_objParent.activeSelf)
                {
                    _objParent.SetActive(true);
                    _canvas.transform.GetChild(0).gameObject.SetActive(true);
                }
                else if (_currentPage != 0 && _objParent.activeSelf)
                {
                    _objParent.SetActive(false);
                    _canvas.transform.GetChild(0).gameObject.SetActive(false);
                }

                if (_currentPage == 1 && !_weaponsParent.activeSelf)
                {
                    _weaponsParent.SetActive(true);
                    _canvas.transform.GetChild(2).gameObject.SetActive(true);
                }
                else if (_currentPage != 1 && _weaponsParent.activeSelf)
                {
                    _weaponsParent.SetActive(false);
                    _canvas.transform.GetChild(2).gameObject.SetActive(false);
                }

                if (_currentPage == 2 && !_toolParent.activeSelf)
                {
                    _toolParent.SetActive(true);
                    _canvas.transform.GetChild(4).gameObject.SetActive(true);
                }
                else if (_currentPage != 2 && _toolParent.activeSelf)
                {
                    _toolParent.SetActive(false);
                    _canvas.transform.GetChild(4).gameObject.SetActive(false);
                }

                if (_currentPage == 3 && !_utilsParent.activeSelf)
                {
                    _utilsParent.SetActive(true);
                    _canvas.transform.GetChild(6).gameObject.SetActive(true);
                }
                else if (_currentPage != 3 && _utilsParent.activeSelf)
                {
                    _utilsParent.SetActive(false);
                    _canvas.transform.GetChild(6).gameObject.SetActive(false);
                }

                if (_currentPage == 4 && !_settingsParent.activeSelf)
                {
                    _settingsParent.SetActive(true);
                    _canvas.transform.GetChild(8).gameObject.SetActive(true);
                }
                else if (_currentPage != 4 && _settingsParent.activeSelf)
                {
                    _settingsParent.SetActive(false);
                    _canvas.transform.GetChild(8).gameObject.SetActive(false);
                }

                if (_currentPage == 5 && !_modsParent.activeSelf)
                {
                    _modsParent.SetActive(true);
                    _canvas.transform.GetChild(10).gameObject.SetActive(true);
                }
                else if (_currentPage != 5 && _modsParent.activeSelf)
                {
                    _modsParent.SetActive(false);
                    _canvas.transform.GetChild(10).gameObject.SetActive(false);
                }
        }

        public void AddPage(string[] buttonNames, string pageName, int perline, GameObject buttonParent, int pIndex)
        {
            int currentSpot = 0;
            int currentLine = 0;
            buttonParent.transform.SetParent(_menu.transform, false);
            buttonParent.name = pageName;
            GameObject textParent = new GameObject
            {
                name = pageName
            };
            textParent.transform.parent = _canvas.transform;
            GameObject sideButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideButton.layer = 18;
            sideButton.GetComponent<BoxCollider>().isTrigger = true;
            sideButton.transform.SetParent(_sideBtnParent.transform, false);
            sideButton.transform.localScale = new Vector3(0.02f, 0.07f, 0.225f);
            sideButton.transform.localPosition = new Vector3(0, 0.085f - pIndex * 0.1f, 0.745f);
            sideButton.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");

            GameObject pageLabel = Instantiate(_text);
            pageLabel.transform.SetParent(_canvas.transform, false);
            pageLabel.name = pageName;
            pageLabel.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 90, 0);
            pageLabel.transform.position = sideButton.transform.position + new Vector3(-0.015f, 0, 0);
            pageLabel.GetComponent<Text>().text = pageName.ToUpper();
            pageLabel.GetComponent<Text>().color = Color.black;

            PageButton pageBtn = sideButton.AddComponent<PageButton>();
            pageBtn._pageIndex = pIndex;
            pageBtn._text = pageLabel;
            pageBtn._list = this;

            for (int i = 0; i < buttonNames.Length; i++)
            {
                GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
                button.layer = 18;
                button.GetComponent<BoxCollider>().isTrigger = true;

                button.transform.localScale = new Vector3(0.025f, 0.145f, 0.145f);
                button.transform.SetParent(buttonParent.transform, false);

                currentLine = (int)Mathf.Floor(i / perline);
                button.transform.localPosition = new Vector3(0.02f, -currentLine * (button.transform.localScale.y + 0.03f), (perline - 1 - currentSpot) * (button.transform.localScale.z + 0.02f));
                currentSpot++;
                if (currentSpot == perline) currentSpot = 0;

                GameObject objLabel = Instantiate(_text);
                objLabel.transform.SetParent(textParent.transform, false);
                objLabel.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 90, 0);
                objLabel.transform.position = button.transform.position + new Vector3(-0.015f, 0, 0);
                objLabel.GetComponent<Text>().text = buttonNames[i].ToUpper();
                objLabel.GetComponent<Text>().color = Color.black;

                Button buttonScript = button.AddComponent<Button>();
                buttonScript._buttonIndex = i;
                buttonScript._text = objLabel;
                buttonScript._list = this;

                button.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
            }
        }

        public void ClearMain()
        {
            objectButtons = new bool[12];
            weaponButtons = new bool[12];
            toolButtons = new bool[8];
            utilButtons = new bool[8];
            settingsButtons = new bool[12];
            modsButtons = new bool[4];
        }
        public void PlayAudio(bool item) => _audioSource.PlayOneShot(item ? RefCache.ItemSelection : RefCache.PageSelection);

        public bool[] GetArray() => _currentPage switch
        {
            0 => objectButtons,
            1 => weaponButtons,
            2 => toolButtons,
            3 => utilButtons,
            4 => settingsButtons,
            5 => modsButtons,
            _ => throw new System.IndexOutOfRangeException()
        };

        public void SetArray(bool[] array)
        {
            switch (_currentPage)
            {
                case 0:
                    objectButtons = array;
                    break;
                case 1:
                    weaponButtons = array;
                    break;
                case 2:
                    toolButtons = array;
                    break;
                case 3:
                    utilButtons = array;
                    break;
                case 4:
                    settingsButtons = array;
                    break;
                case 5:
                    modsButtons = array;
                    break;
            }
        }
    }
}