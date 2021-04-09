using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Akaal.Consoled.Editor
{
    public class ConsoledWindow : EditorWindow
    {
        #region Window

        [MenuItem("Window/Tools/Consoled")]
        public static void ShowExample()
        {
            ConsoledWindow wnd = CreateWindow<ConsoledWindow>();
            wnd.titleContent = new GUIContent("Consoled", ConsoledResources.LogoTexture());
        }

        #region Window Callbacks

        private void OnEnable()
        {
            CreateGUI();
        }

        public void CreateGUI()
        {
            Create();
        }

        private void OnDisable()
        {
            OnDestroy();
        }

        private void OnDestroy()
        {
            Destroy();
        }

        #endregion

        #endregion

        private bool             _didCreate;
        private ConsoledInstance _instance = new ConsoledInstance(); //todo -- serialize this!

        private void Create()
        {
            if (_didCreate) return;
            _didCreate = true;

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            root.AddToClassList("root");

            // Import UXML
            var visualTree = ConsoledResources.WindowAsset();
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = ConsoledResources.StyleSheet();
            root.styleSheets.Add(styleSheet);

            SetupInstance();
            AttachInput();
            AttachOutput();
            HandleStatus();
        }

        private void SetupInstance()
        {
            if (_instance != null)
            {
                _instance = new ConsoledInstance();
            }
        }

        private void Destroy()
        {
            if (!_didCreate) return;
            _didCreate = false;
            DetachOutput();
        }

        #region Fields

        private int           _commandOffset = 0;
        private string        _tempCommand;
        private VisualElement _memoryPanel;
        private Label         _memoryLabel;
        private ScrollView    _consoleScrollView;
        private TextField     _input;
        private Button        _submit;
        private Label         _status;
        private bool          _readyState = true;

        #endregion

        private void HandleStatus()
        {
            _status = rootVisualElement.Q<Label>(name: "status-text");
            rootVisualElement.schedule.Execute(() =>
                              {
                                  if (!_instance.IsReady && _readyState)
                                  {
                                      _readyState = false;
                                      SetStatusText("Reloading commands...");
                                      _input.SetEnabled(false);
                                      _submit.SetEnabled(false);
                                  }
                                  else if (_instance.IsReady && !_readyState)
                                  {
                                      _readyState = true;
                                      SetStatusText("Ready.");
                                      _input.SetEnabled(true);
                                      UpdateTypedCommand();
                                  }
                              }).
                              Every(200);
        }

        private void AttachOutput()
        {
            var outputText = rootVisualElement.Q<Label>("console-text");
            foreach (string s in _instance.OutputHistory)
            {
                outputText.text += s;
            }

            _instance.Context.OnMemoryUpdated += MemoryUpdated;
            _instance.SetColorTranslator((text, color) =>
            {
                return text;
                //todo -- solve rich text
                /*
                if (!color.HasValue) return text;
                string hex = ColorUtility.ToHtmlStringRGB(color.Value);
                return $"<color=#{hex}>{text}</color>";*/
            });
            _instance.SetClearHandler(() => { outputText.text      =  string.Empty; });
            _instance.SetOutputHandler((text) => { outputText.text += text; });
        }

        private void AttachInput()
        {
            _input                    =  rootVisualElement.Q<TextField>("console-input");
            _submit                   =  rootVisualElement.Q<Button>("console-submit");
            _submit.clickable.clicked += SubmitCommand;
            _input.RegisterCallback<KeyUpEvent>(TryKeyboardShortcuts);
            _input.RegisterValueChangedCallback(InputValueChanged);
        }

        #region Methods

        private void SetStatusText(string statusMessage)
        {
            _status.text = statusMessage;
        }

        private void SetStatusTextIfEmpty(string statusMessage)
        {
            if (string.IsNullOrEmpty(_status.text))
            {
                _status.text = statusMessage;
            }
        }

        private void InputValueChanged(ChangeEvent<string> evt)
        {
            _submit.SetEnabled(!string.IsNullOrEmpty(_input.value));
        }

        private void TryKeyboardShortcuts(KeyUpEvent evt)
        {
            if (evt.target != _input || _input.focusController.focusedElement != _input) return;

            switch (evt.keyCode)
            {
                case KeyCode.Return:
                    SubmitCommand();
                    break;
                case KeyCode.UpArrow:
                    PreviousCommand();
                    break;
                case KeyCode.DownArrow:
                    NextCommand();
                    break;
            }
        }

        private void NextCommand()
        {
            TrySaveTempCommand();
            _commandOffset--;
            if (_commandOffset < -1) _commandOffset = -1;
            UpdateTypedCommand();
        }

        private void PreviousCommand()
        {
            TrySaveTempCommand();
            _commandOffset++;
            UpdateTypedCommand();
        }

        private void Validate()
        {
            int cmdHistorySize = _instance.CommandHistory.Size;
            _commandOffset = Mathf.Clamp(_commandOffset, -1, cmdHistorySize - 1);
        }

        private void UpdateTypedCommand()
        {
            Validate();
            if (_commandOffset == -1)
            {
                _input.value = _tempCommand;
            }
            else
            {
                _input.value = _instance.CommandHistory[_commandOffset];
            }
        }

        private void SubmitCommand()
        {
            _instance.SubmitCommand(_input.value);
            _tempCommand   = string.Empty;
            _commandOffset = -1;
            UpdateTypedCommand();
            _input.SelectAll();
            if (_consoleScrollView == null)
                _consoleScrollView = rootVisualElement.Q<ScrollView>("console-text-scrollview");
            _consoleScrollView.scrollOffset = Vector2.up * _consoleScrollView.contentRect.height;
        }

        private void TrySaveTempCommand()
        {
            if (_commandOffset == -1) _tempCommand = _input.value;
        }

        #endregion

        private void DetachOutput()
        {
            _instance.Context.OnMemoryUpdated -= MemoryUpdated;
            _instance.SetColorTranslator(null);
            _instance.SetOutputHandler(Debug.Log);
        }

        #region Memory

        private void MemoryUpdated()
        {
            if (_memoryLabel == null)
                _memoryLabel = rootVisualElement.Q<Label>("memory-text");
            if (_memoryPanel == null)
                _memoryPanel = rootVisualElement.Q("memory");

            _memoryLabel.text = _instance.Context.GetUserFriendlyMemoryRepresentation();
            if (string.IsNullOrEmpty(_memoryLabel.text))
            {
                _memoryPanel.visible = false;
            }
        }

        #endregion
    }
}