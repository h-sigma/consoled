using Akaal.Consoled;
using UnityEngine;


public class ConsoledIMGUIWindow : MonoBehaviour
{
    public bool isDisplayedBasedOnGameObject = true;
    public int  outputFontSize               = 20;
    public int  inputFontSize                = 20;

    private Rect          drawRect;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.pivot = Vector2.zero;
    }

    private Vector3[] corners = new Vector3[4];

    private void OnRectTransformDimensionsChange()
    {
        if (rectTransform == null) return;
        //rectTransform.GetWorldCorners(corners);
        drawRect          =  rectTransform.rect;
        drawRect.height -= rectTransform.anchoredPosition.y;
        drawRect.x += rectTransform.anchoredPosition.x;
    }

    private void OnEnable()
    {
        if (isDisplayedBasedOnGameObject)
            Show(true);
    }

    private void OnDisable()
    {
        if (isDisplayedBasedOnGameObject)
            Show(false);
    }

    #region Styles

    public GUIStyle headerStyle        { get; private set; }
    public GUIStyle consoleOutputStyle { get; private set; }
    public GUIStyle inputFieldStyle    { get; private set; }

    private void SetupStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 40
            };
        }

        if (consoleOutputStyle == null)
        {
            consoleOutputStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize  = outputFontSize,
                wordWrap  = true,
                font      = ConsoledResources.Font()
            };
        }

        if (inputFieldStyle == null)
        {
            inputFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = inputFontSize,
                wordWrap = false,
                font     = ConsoledResources.Font(),
                name     = nameof(inputFieldStyle)
            };
        }
    }

    #endregion

    #region GUI

    private bool isShown = false;

    private Vector2 consoleScrollPos;

    private Vector2 memoryScrollPos;

    private string command;

    private GUIContent outputText = new GUIContent();

    private string memoryText;

    private float lastTextRectWidth;

    void OnGUI()
    {
        if (!isShown) return;
        Validate();
        GUILayout.BeginHorizontal();
        if (rectTransform == null) drawRect = new Rect(0, 0, Screen.width, Screen.height / 2f);

        //console drawing
        Rect consoleRect = new Rect(drawRect.x, drawRect.y, drawRect.width - 100, drawRect.height);
        GUI.Box(consoleRect, "Consoled");

        consoleRect.height -= 25f;
        lastTextRectWidth  =  consoleRect.width - 20;
        var textRect = new Rect(0, 0, lastTextRectWidth, GetOutputTextHeight());

        consoleScrollPos = GUI.BeginScrollView(consoleRect, consoleScrollPos,
            textRect);

        GUI.Label(textRect, outputText, consoleOutputStyle);
        GUI.EndScrollView();

        consoleRect.y      += consoleRect.height;
        consoleRect.height =  25;
        consoleRect.width  -= 50;
        command            =  GUI.TextField(consoleRect, command, inputFieldStyle);
        consoleRect.x      += consoleRect.width;
        consoleRect.width  =  50;

        bool guiEnabled = GUI.enabled;
        GUI.enabled = command?.Length > 0;
        if (GUI.Button(consoleRect, "Run") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
        {
            SubmitCommand();
        }

        GUI.enabled = guiEnabled;

        Rect memoryRect = new Rect(drawRect.x + drawRect.width - 100, drawRect.y, 100, drawRect.height);
        GUI.Box(memoryRect, "Memory");
    }

    private float GetOutputTextHeight()
    {
        return consoleOutputStyle.CalcHeight(outputText, lastTextRectWidth);
    }

    #endregion

    private ConsoledInstance _consoled;

    private void Validate()
    {
        if (_consoled == null)
        {
            _consoled = new ConsoledInstance();
            AttachToConsole();
        }

        SetupStyles();
    }

    public void Show(bool show)
    {
        isShown = show;
    }

    public bool IsShown => isShown;

    private void SubmitCommand()
    {
        if (string.IsNullOrEmpty(command)) return;
        _consoled.SubmitCommand(command);
        command = string.Empty;
    }

    private void AttachToConsole()
    {
        _consoled.SetClearHandler(() => outputText.text = string.Empty);
        _consoled.SetOutputHandler((str) =>
        {
            outputText.text    += str;
            consoleScrollPos.y =  GetOutputTextHeight();
        });
    }
}