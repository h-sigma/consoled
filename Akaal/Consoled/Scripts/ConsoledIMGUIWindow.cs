using Akaal.Consoled;
using UnityEngine;


public class ConsoledIMGUIWindow : MonoBehaviour
{
    public  bool  isDisplayedBasedOnGameObject = true;
    public  int   outputFontSize               = 30;
    public  int   headerFontSize               = 30;
    public  int   inputFontSize                = 30;
    public  float top;
    public  float left;
    public  float right;
    public  float bottom;
    public  float heightScreenNormalized;
    public  float widthScreenNormalized;
    public  float consoleWidthNormalized = 0.8f;
    private Rect  drawRect;

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
    public GUIStyle memoryOutputStyle  { get; private set; }
    public GUIStyle inputFieldStyle    { get; private set; }

    private void SetupStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = headerFontSize
            };
        }

        if (consoleOutputStyle == null)
        {
            consoleOutputStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize  = outputFontSize,
                wordWrap  = true,
                font      = ConsoledResources.Font(),
                richText  = true
            };
        }

        if (memoryOutputStyle == null)
        {
            memoryOutputStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize  = outputFontSize,
                wordWrap  = true,
                font      = ConsoledResources.Font(),
                richText  = true
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

    private GUIContent memoryText = new GUIContent();

    private float lastTextRectWidth;

    void OnGUI()
    {
        if (!isShown) return;
        Validate();
        GUILayout.BeginHorizontal();

        drawRect = GetDrawRect();

        DrawConsole();

        DrawMemory();
    }

    public bool ShowMemoryPanel => memoryText.text?.Length > 0;

    private void DrawMemory()
    {
        if (!ShowMemoryPanel) return;
        //memory drawing
        Rect memoryRect = new Rect(drawRect.x + drawRect.width * consoleWidthNormalized, drawRect.y,
            drawRect.width * (1 - consoleWidthNormalized), drawRect.height);
        GUI.Box(memoryRect, "Memory");

        memoryRect.width -= 20;
        float textHeight = consoleOutputStyle.CalcHeight(memoryText, memoryRect.width);
        var   textRect   = new Rect(0, 0, memoryRect.width, textHeight);
        memoryScrollPos = GUI.BeginScrollView(memoryRect, memoryScrollPos,
            textRect);

        GUI.Label(textRect, memoryText, memoryOutputStyle);
        GUI.EndScrollView();
    }

    private void DrawConsole()
    {
        //console drawing
        Rect consoleRect = new Rect(drawRect.x, drawRect.y, drawRect.width * (ShowMemoryPanel ? consoleWidthNormalized : 1f), drawRect.height);
        GUI.Box(consoleRect, "Consoled");

        consoleRect.height -= inputFontSize     + 10;
        lastTextRectWidth  =  consoleRect.width - 20;
        var textRect = new Rect(0, 0, lastTextRectWidth, GetOutputTextHeight());

        consoleScrollPos = GUI.BeginScrollView(consoleRect, consoleScrollPos,
            textRect);

        GUI.Label(textRect, outputText, consoleOutputStyle);
        GUI.EndScrollView();

        consoleRect.y      += consoleRect.height;
        consoleRect.height =  inputFontSize + 10;
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
    }

    private Rect GetDrawRect()
    {
        return new Rect(left, top, Screen.width * widthScreenNormalized  - left - right,
            Screen.height                       * heightScreenNormalized - top  - bottom);
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
        _consoled.SetColorTranslator((text, color) =>
        {
            if (!color.HasValue) return text;
            string hex = ColorUtility.ToHtmlStringRGB(color.Value);
            return $"<color=#{hex}>{text}</color>";
        });
        _consoled.Context.OnMemoryUpdated += () =>
        {
            memoryText.text = _consoled.Context.GetUserFriendlyMemoryRepresentation();
        };
    }
}