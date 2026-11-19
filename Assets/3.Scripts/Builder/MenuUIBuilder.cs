#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Bird.AI.UIBuilder
{
    public class MenuUIBuilder : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private TMP_FontAsset font;

        private const string RootName = "Menu UI";
        private readonly Vector2 ReferenceResolution = new Vector2(1440, 3088);

        [ContextMenu("1. Build UI")]
        public void BuildUI()
        {
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                Debug.LogWarning($"[UIBuilder] '{RootName}'이(가) 이미 존재합니다. 겹쳐서 만들지 않고 중단합니다.");
                return;
            }

            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogWarning("[UIBuilder] 씬에 EventSystem이 없습니다. 씬에 EventSystem을 추가해주세요.");
            }

            // 1. Root & Canvas 설정
            GameObject rootObj = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(rootObj, "Build UI");

            Canvas canvas = rootObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = rootObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            rootObj.AddComponent<GraphicRaycaster>();

            // 2. 영역별 UI 생성
            CreateMainLobby(rootObj.transform);
            CreateLobbyPopup(rootObj.transform);      // 방 생성 팝업
            CreateLobbyListPopup(rootObj.transform);  // 방 목록 팝업
            CreatePasswordPopup(rootObj.transform);   // 비밀번호 입력 팝업

            // 변경사항 저장
            EditorSceneManager.MarkSceneDirty(rootObj.scene);
            Debug.Log($"[UIBuilder] '{RootName}' 통합 로비 UI 생성 완료.");
        }

        [ContextMenu("2. Clear UI")]
        public void ClearUI()
        {
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot);
                Debug.Log($"[UIBuilder] '{RootName}'을(를) 삭제했습니다.");
            }
            else
            {
                Debug.Log($"[UIBuilder] 삭제할 '{RootName}'을(를) 찾을 수 없습니다.");
            }
        }

        #region Create UI Areas

        private void CreateMainLobby(Transform canvas)
        {
            GameObject bg = NewPanel("Panel_Background", canvas, Vector2.zero);
            Stretch(bg.GetComponent<RectTransform>(), 0f);
            bg.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.22f);

            GameObject centerMenu = NewUI("CenterMenu", canvas);
            SetRect(centerMenu.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 600));

            Button createBtn = NewButton("Button_CreateRoom", centerMenu.transform, "방 생성 (Create)", out TMP_Text cTxt);
            SetRect(createBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(700, 220));
            createBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.8f);
            cTxt.color = Color.white;
            cTxt.fontSize = 55f;
            cTxt.fontStyle = FontStyles.Bold;

            Button joinBtn = NewButton("Button_JoinRoom", centerMenu.transform, "방 참가 (Join)", out TMP_Text jTxt);
            SetRect(joinBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(700, 220));
            joinBtn.GetComponent<Image>().color = new Color(0.25f, 0.75f, 0.4f);
            jTxt.color = Color.white;
            jTxt.fontSize = 55f;
            jTxt.fontStyle = FontStyles.Bold;
        }

        private void CreateLobbyPopup(Transform canvas)
        {
            GameObject popup = NewUI("Panel_CreateLobby", canvas);
            Stretch(popup.GetComponent<RectTransform>(), 0f);

            Button dimBtn = NewButton("Button_DimBg", popup.transform, "", out TMP_Text dimTxt);
            Stretch(dimBtn.GetComponent<RectTransform>(), 0f);
            dimBtn.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            dimTxt.gameObject.SetActive(false);

            GameObject window = NewPanel("Panel_Window", popup.transform, new Vector2(1100, 1600));
            SetRect(window.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 1600));
            window.GetComponent<Image>().color = new Color(0.92f, 0.94f, 0.96f);

            TMP_Text title = NewText("Text_Title", window.transform, "새로운 방 생성", 65f, TextAlignmentOptions.Center);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(800, 150));
            title.color = Color.black;
            title.fontStyle = FontStyles.Bold;

            GameObject inputGroup = NewUI("Group_Inputs", window.transform);
            SetRect(inputGroup.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(900, 1000));

            // 방 이름
            TMP_Text nameLabel = NewText("Label_RoomName", inputGroup.transform, "방 이름", 45f, TextAlignmentOptions.Left);
            SetRect(nameLabel.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -50), new Vector2(300, 80));
            nameLabel.color = Color.black;
            GameObject nameInput = CreateInputField("InputField_RoomName", inputGroup.transform, "방 이름을 입력하세요");
            SetRect(nameInput.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(0, 120));
            
            // 비밀번호
            TMP_Text pwLabel = NewText("Label_Password", inputGroup.transform, "비밀번호 (선택)", 45f, TextAlignmentOptions.Left);
            SetRect(pwLabel.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -320), new Vector2(400, 80));
            pwLabel.color = Color.black;
            GameObject pwInput = CreateInputField("InputField_Password", inputGroup.transform, "비밀번호를 입력하세요");
            SetRect(pwInput.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -410), new Vector2(0, 120));
            pwInput.GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.Password;

            // 난이도
            TMP_Text diffLabel = NewText("Label_Difficulty", inputGroup.transform, "게임 난이도", 45f, TextAlignmentOptions.Left);
            SetRect(diffLabel.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -590), new Vector2(300, 80));
            diffLabel.color = Color.black;
            GameObject diffDropdown = CreateDropdown("Dropdown_Difficulty", inputGroup.transform);
            SetRect(diffDropdown.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -680), new Vector2(0, 120));

            // 확인 버튼
            Button confirmBtn = NewButton("Button_ConfirmCreate", window.transform, "방 생성 확인", out TMP_Text cfTxt);
            SetRect(confirmBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 100), new Vector2(600, 180));
            confirmBtn.GetComponent<Image>().color = new Color(0.9f, 0.4f, 0.2f);
            cfTxt.color = Color.white;
            cfTxt.fontStyle = FontStyles.Bold;

            popup.SetActive(false);
        }

        private void CreateLobbyListPopup(Transform canvas)
        {
            GameObject popup = NewUI("Panel_LobbyList", canvas);
            Stretch(popup.GetComponent<RectTransform>(), 0f);

            Button dimBtn = NewButton("Button_DimBg", popup.transform, "", out TMP_Text dimTxt);
            Stretch(dimBtn.GetComponent<RectTransform>(), 0f);
            dimBtn.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
            dimTxt.gameObject.SetActive(false);

            GameObject window = NewPanel("Panel_Window", popup.transform, new Vector2(1300, 2400));
            SetRect(window.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300, 2400));
            window.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.28f);

            TMP_Text title = NewText("Text_Title", window.transform, "방 목록", 70f, TextAlignmentOptions.Center);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -100), new Vector2(600, 120));
            title.color = Color.white;
            title.fontStyle = FontStyles.Bold;

            Button closeBtn = NewButton("Button_Close", window.transform, "X", out TMP_Text clsTxt);
            SetRect(closeBtn.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-50, -50), new Vector2(120, 120));
            closeBtn.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);
            clsTxt.color = Color.white;

            // 스크롤 뷰 생성
            GameObject scrollView = NewUI("ScrollView_Rooms", window.transform);
            RectTransform svRt = scrollView.GetComponent<RectTransform>();
            SetRect(svRt, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            svRt.offsetMin = new Vector2(50, 100);
            svRt.offsetMax = new Vector2(-50, -250); // 상단 타이틀 여백

            GameObject viewport = NewUI("Viewport", scrollView.transform);
            Stretch(viewport.GetComponent<RectTransform>(), 0f);
            viewport.AddComponent<Image>().color = new Color(0,0,0, 0.1f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = NewUI("Content", viewport.transform);
            RectTransform ctRt = content.GetComponent<RectTransform>();
            SetRect(ctRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 0));
            
            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20f;
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false; // 높이는 각 아이템의 크기를 따름
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.content = ctRt;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // 방 아이템 목업 생성 (RoomItem)
            CreateRoomItem(content.transform, "RoomItem_1", "모두 환영합니다!", "Normal", "1 / 4", false);
            CreateRoomItem(content.transform, "RoomItem_2", "지인 전용 빡겜방", "Hard", "2 / 4", true);
            CreateRoomItem(content.transform, "RoomItem_3", "초보만 오세요", "Easy", "3 / 4", false);

            popup.SetActive(false);
        }

        private void CreatePasswordPopup(Transform canvas)
        {
            GameObject popup = NewUI("Panel_PasswordPopup", canvas);
            Stretch(popup.GetComponent<RectTransform>(), 0f);

            Button dimBtn = NewButton("Button_DimBg", popup.transform, "", out TMP_Text dimTxt);
            Stretch(dimBtn.GetComponent<RectTransform>(), 0f);
            dimBtn.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
            dimTxt.gameObject.SetActive(false);

            GameObject window = NewPanel("Panel_Window", popup.transform, new Vector2(1000, 800));
            SetRect(window.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 800));
            window.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f);

            TMP_Text title = NewText("Text_Title", window.transform, "비밀번호 입력", 60f, TextAlignmentOptions.Center);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(600, 100));
            title.color = Color.black;
            title.fontStyle = FontStyles.Bold;

            GameObject pwInput = CreateInputField("InputField_Password", window.transform, "비밀번호");
            SetRect(pwInput.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(700, 150));
            pwInput.GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.Password;

            Button confirmBtn = NewButton("Button_Confirm", window.transform, "입장", out TMP_Text cfTxt);
            SetRect(confirmBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(400, 150));
            confirmBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.8f);
            cfTxt.color = Color.white;
            cfTxt.fontStyle = FontStyles.Bold;

            popup.SetActive(false);
        }

        #endregion

        #region Custom Elements Generation Helpers

        private void CreateRoomItem(Transform parent, string objName, string roomName, string difficulty, string playerCount, bool isLocked)
        {
            GameObject item = NewPanel(objName, parent, new Vector2(0, 160)); // Width는 LayoutGroup이 결정
            item.GetComponent<Image>().color = new Color(0.25f, 0.3f, 0.38f);

            // 방 이름 (RoomNameText)
            TMP_Text nameTxt = NewText("Text_RoomName", item.transform, roomName, 45f, TextAlignmentOptions.Left);
            SetRect(nameTxt.rectTransform, new Vector2(0, 0), new Vector2(0.5f, 1), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(-40, 0));
            nameTxt.color = Color.white;
            nameTxt.fontStyle = FontStyles.Bold;

            // 난이도 (DifficultyText)
            TMP_Text diffTxt = NewText("Text_Difficulty", item.transform, difficulty, 40f, TextAlignmentOptions.Center);
            SetRect(diffTxt.rectTransform, new Vector2(0.5f, 0), new Vector2(0.65f, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            diffTxt.color = difficulty == "Hard" ? new Color(1f, 0.4f, 0.4f) : new Color(0.6f, 0.9f, 0.6f);

            // 인원수 (PlayerCountText)
            TMP_Text countTxt = NewText("Text_PlayerCount", item.transform, playerCount, 40f, TextAlignmentOptions.Center);
            SetRect(countTxt.rectTransform, new Vector2(0.65f, 0), new Vector2(0.8f, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            countTxt.color = Color.white;

            // 입장 버튼 (JoinButton)
            Button joinBtn = NewButton("Button_Join", item.transform, "입장", out TMP_Text jTxt);
            SetRect(joinBtn.GetComponent<RectTransform>(), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(160, 100));
            joinBtn.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.3f);
            jTxt.color = Color.white;

            // 자물쇠 아이콘 (LockIcon) - 입장 버튼 좌측에 배치
            GameObject lockIcon = NewPanel("Image_LockIcon", item.transform, new Vector2(70, 70));
            SetRect(lockIcon.GetComponent<RectTransform>(), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-220, 0), new Vector2(70, 70));
            lockIcon.GetComponent<Image>().color = new Color(0.9f, 0.8f, 0.2f); // 자물쇠를 나타내는 노란색 목업
            lockIcon.SetActive(isLocked);
        }

        private GameObject CreateInputField(string name, Transform parent, string placeholderText)
        {
            GameObject inputObj = NewPanel(name, parent, Vector2.zero);
            inputObj.GetComponent<Image>().color = Color.white;
            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();

            TMP_Text textComp = NewText("Text", inputObj.transform, "", 45f, TextAlignmentOptions.Left);
            Stretch(textComp.rectTransform, 20f);
            textComp.color = Color.black;
            textComp.richText = false;

            TMP_Text placeholderComp = NewText("Placeholder", inputObj.transform, placeholderText, 45f, TextAlignmentOptions.Left);
            Stretch(placeholderComp.rectTransform, 20f);
            placeholderComp.color = new Color(0.6f, 0.6f, 0.6f);
            placeholderComp.fontStyle = FontStyles.Italic;

            inputField.textComponent = textComp;
            inputField.placeholder = placeholderComp;
            inputField.textViewport = inputObj.GetComponent<RectTransform>();

            return inputObj;
        }

        private GameObject CreateDropdown(string name, Transform parent)
        {
            GameObject dropObj = NewPanel(name, parent, Vector2.zero);
            dropObj.GetComponent<Image>().color = Color.white;
            TMP_Dropdown dropdown = dropObj.AddComponent<TMP_Dropdown>();

            dropdown.options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("Easy"),
                new TMP_Dropdown.OptionData("Normal"),
                new TMP_Dropdown.OptionData("Hard")
            };

            TMP_Text label = NewText("Label", dropObj.transform, "Easy", 45f, TextAlignmentOptions.Left);
            Stretch(label.rectTransform, 20f);
            label.color = Color.black;
            dropdown.captionText = label;

            return dropObj;
        }

        private void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
        }

        #endregion

        #region Standard Helper Methods

        private GameObject NewUI(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            return go;
        }

        private GameObject NewPanel(string name, Transform parent, Vector2 size)
        {
            GameObject go = NewUI(name, parent);
            go.AddComponent<Image>();
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            return go;
        }

        private TMP_Text NewText(string name, Transform parent, string text, float size, TextAlignmentOptions align)
        {
            GameObject go = NewUI(name, parent);
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            if (font != null)
            {
                tmp.font = font;
            }
            return tmp;
        }

        private Button NewButton(string name, Transform parent, string label, out TMP_Text labelText)
        {
            GameObject go = NewPanel(name, parent, new Vector2(200, 100)); 
            Button btn = go.AddComponent<Button>();
            
            labelText = NewText("Text_Label", go.transform, label, 40f, TextAlignmentOptions.Center);
            Stretch(labelText.rectTransform, 0f);
            labelText.color = Color.black;

            return btn;
        }

        private void Stretch(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        #endregion
    }
}
#endif