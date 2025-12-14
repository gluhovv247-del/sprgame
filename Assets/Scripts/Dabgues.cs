using Ink.Runtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using System.Collections;

public class Dabgues : MonoBehaviour
{
    private Story _currentStory;
    private bool isDisplaying = false;
    public float typingSpeed = 0.04f;
    [SerializeField] private TextAsset _inkJson;
    private string fulltext;
    private GameObject _dialoguePanel;
    private TextMeshProUGUI _dialogueText;
    private TextMeshProUGUI _nameText;
    private bool isTyping;
    [HideInInspector] public GameObject _choiceButtonsPanel;
    private GameObject _choiceButton;
    private List<TextMeshProUGUI> _choiceText = new();
    private SmenaFona _smenafona;
    private bool isCorroutineStopped = false;
    public GameObject nextButton;
    public GameObject backButton;
    private Coroutine displayLineCoroutine;
    public bool DialogPlay { get; private set; }
    private bool canContinueToNextLine = false;

    // История для возврата назад
    private List<string> _historyStates = new List<string>();
    private List<string> _historyTexts = new List<string>(); // Храним текст отдельно
    private int _currentHistoryIndex = -1;

    [Inject]
    public void Construct(DialoguesInstaller dialoguesInstaller)
    {
        _inkJson = dialoguesInstaller.inkJson;
        _dialoguePanel = dialoguesInstaller.dialoguePanel;
        _dialogueText = dialoguesInstaller.dialogueText;
        _nameText = dialoguesInstaller.nameText;
        _choiceButtonsPanel = dialoguesInstaller.choiceButtonsPanel;
        _choiceButton = dialoguesInstaller.choiceButton;
        backButton = dialoguesInstaller.backButton;
    }

    private void Awake()
    {
        _currentStory = new Story(_inkJson.text);
    }

    void Start()
    {
        foreach (var fon in FindObjectsOfType<SmenaFona>())
            _smenafona = fon;

        // Сохраняем начальное состояние перед началом диалога
        SaveCurrentState("", true);

        StartDialogue();

        // Инициализируем кнопку "Назад"
        if (backButton != null)
        {
            var backBtn = backButton.GetComponent<Button>();
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(GoBack);
            }
            UpdateBackButton();
        }
    }

    public void StartDialogue()
    {
        DialogPlay = true;
        _dialoguePanel.SetActive(true);
        ContinueStory();
    }

    // Сохраняем текущее состояние в историю
    private void SaveCurrentState(string currentText, bool isInitial = false)
    {
        if (_currentStory != null)
        {
            // Сохраняем JSON состояние истории
            string jsonState = _currentStory.state.ToJson();

            // Если это не начальное состояние, удаляем все состояния после текущего
            if (!isInitial && _currentHistoryIndex < _historyStates.Count - 1)
            {
                int itemsToRemove = _historyStates.Count - (_currentHistoryIndex + 1);
                _historyStates.RemoveRange(_currentHistoryIndex + 1, itemsToRemove);
                _historyTexts.RemoveRange(_currentHistoryIndex + 1, itemsToRemove);
            }

            // Добавляем новое состояние
            _historyStates.Add(jsonState);
            _historyTexts.Add(currentText);
            _currentHistoryIndex = _historyStates.Count - 1;

            UpdateBackButton();
        }
    }

    // Возврат к предыдущему состоянию
    public void GoBack()
    {
        // Нельзя вернуться назад, если мы в самом начале
        if (_currentHistoryIndex <= 0) return;

        // Переходим к предыдущему состоянию
        _currentHistoryIndex--;

        // Восстанавливаем состояние из истории
        LoadStateFromHistory(_currentHistoryIndex);

        UpdateBackButton();
    }

    // Загружаем состояние из истории
    private void LoadStateFromHistory(int historyIndex)
    {
        if (historyIndex < 0 || historyIndex >= _historyStates.Count) return;

        // Создаем новую историю с сохраненным состоянием
        _currentStory = new Story(_inkJson.text);
        _currentStory.state.LoadJson(_historyStates[historyIndex]);

        // Обновляем UI с сохраненным текстом
        RefreshUI(historyIndex);
    }

    private void RefreshUI(int historyIndex)
    {
        // Показываем сохраненный текст
        _dialogueText.text = _historyTexts[historyIndex];

        // Обновляем имя персонажа
        UpdateNameAndBackground();

        // Показываем кнопки выбора, если они есть
        ShowChoiceButtons();
    }

    private void UpdateNameAndBackground()
    {
        _nameText.text = _currentStory.variablesState["characterName"]?.ToString() ?? "";

        if (_smenafona != null && _currentStory.variablesState["fon"] != null)
        {
            try
            {
                _smenafona.changeImage((int)_currentStory.variablesState["fon"]);
            }
            catch
            {
                // Игнорируем ошибки при смене фона
            }
        }
    }

    private void UpdateBackButton()
    {
        bool canGoBack = _currentHistoryIndex > 0;

        if (backButton != null)
        {
            backButton.SetActive(canGoBack);
            var btn = backButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = canGoBack;
            }
        }
    }

    private void CheckTagsAndHandle()
    {
        var tags = _currentStory.currentTags;
        if (tags != null)
        {
            if (tags.Contains("GAME_OVER"))
            {
                SceneManager.LoadScene("GameOver");
            }
            if (tags.Contains("THE78"))
            {
                SceneManager.LoadScene("The 78");
            }
        }
    }

    public void ContinueStory(bool choiceBefore = false)
    {
        if (_currentStory == null) return;

        if (_currentStory.canContinue)
        {
            // Получаем текст ДО сохранения состояния
            string nextLine = _currentStory.Continue();

            // Сохраняем состояние С текстом текущей строки
            SaveCurrentState(nextLine);

            // Показываем диалог
            _dialogueText.text = nextLine;
            UpdateNameAndBackground();
            ShowChoiceButtons();
        }
        else if (!choiceBefore)
        {
            ExitDialogue();
        }

        CheckTagsAndHandle();
    }

    private void ShowDialogue()
    {
        if (_currentStory.canContinue)
        {
            string nextLine = _currentStory.Continue();
            _dialogueText.text = nextLine;

            // Сохраняем состояние
            SaveCurrentState(nextLine);
        }

        UpdateNameAndBackground();
    }

    private void ShowChoiceButtons()
    {
        List<Choice> currentChoices = _currentStory.currentChoices;
        _choiceButtonsPanel.SetActive(currentChoices.Count != 0);

        if (currentChoices.Count <= 0) { return; }

        // Очистка предыдущих кнопок
        for (int i = _choiceButtonsPanel.transform.childCount - 1; i >= 0; i--)
            Destroy(_choiceButtonsPanel.transform.GetChild(i).gameObject);

        _choiceText.Clear();

        for (int i = 0; i < currentChoices.Count; i++)
        {
            int index = i;
            GameObject choice = Instantiate(_choiceButton, _choiceButtonsPanel.transform, false);

            TextMeshProUGUI choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
            if (choiceText != null) choiceText.text = currentChoices[i].text;

            var btnComp = choice.GetComponent<ButtenActions>();
            if (btnComp != null) btnComp.index = index;

            var btn = choice.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    // Сохраняем состояние перед выбором (с текущим текстом)
                    SaveCurrentState(_dialogueText.text);

                    DisableAllChoiceButtons();
                    ChoiceButtonAction(index);
                });
            }
        }
    }

    private void DisableAllChoiceButtons()
    {
        foreach (Transform t in _choiceButtonsPanel.transform)
        {
            var b = t.GetComponent<Button>();
            if (b != null) b.interactable = false;
        }
    }

    public void ChoiceButtonAction(int choiceIndex)
    {
        int count = _currentStory.currentChoices.Count;
        if (choiceIndex < 0 || choiceIndex >= count)
        {
            Debug.LogWarning($"Choice index {choiceIndex} out of range (0..{count - 1}). Ignoring.");
            return;
        }

        _currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory(true);
    }

    public void ExitDialogue()
    {
        DialogPlay = false;
        _dialoguePanel.SetActive(false);
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex <= SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    // Очистка истории при загрузке новой сцены
    void OnDestroy()
    {
        _historyStates.Clear();
        _historyTexts.Clear();
        _currentHistoryIndex = -1;
    }
}
//using Ink.Runtime;
//using System.Collections.Generic;
//using System.Linq;
//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
//using Zenject;
//using System.Collections;

//public class Dabgues : MonoBehaviour
//{
//    private Story _currentStory;
//    private bool isDisplaying = false;
//    public float typingSpeed = 0.04f;
//    [SerializeField] private TextAsset _inkJson;
//    private string fulltext;
//    private GameObject _dialoguePanel;
//    private TextMeshProUGUI _dialogueText;
//    private TextMeshProUGUI _nameText;
//    private bool isTyping;
//    [HideInInspector] public GameObject _choiceButtonsPanel;
//    private GameObject _choiceButton;
//    private List<TextMeshProUGUI> _choiceText = new();
//    private SmenaFona _smenafona;
//    private bool isCorroutineStopped = false;
//    public GameObject nextButton;
//    private Coroutine displayLineCoroutine;
//    public bool DialogPlay { get; private set; }
//    private bool canContinueToNextLine = false;
//    [Inject]
//    public void Construct(DialoguesInstaller dialoguesInstaller)
//    {
//        _inkJson = dialoguesInstaller.inkJson;
//        _dialoguePanel = dialoguesInstaller.dialoguePanel;
//        _dialogueText = dialoguesInstaller.dialogueText;
//        _nameText = dialoguesInstaller.nameText;
//        _choiceButtonsPanel = dialoguesInstaller.choiceButtonsPanel;
//        _choiceButton = dialoguesInstaller.choiceButton;
//    }

//    private void Awake()
//    {
//        _currentStory = new Story(_inkJson.text);
//    }

//    void Start()
//    {
//        foreach (var fon in FindObjectsOfType<SmenaFona>())
//            _smenafona = fon;

//        StartDialogue();
//    }

//    public void StartDialogue()
//    {

//        DialogPlay = true;
//        _dialoguePanel.SetActive(true);
//        ContinueStory();

//    }
//    private void CheckTagsAndHandle()
//    {
//        var tags = _currentStory.currentTags;
//        if (tags != null && tags.Contains("GAME_OVER"))
//        {
//            // например: перейти на сцену
//            SceneManager.LoadScene("GameOver");
//            // или вызвать ваш менеджер перехода:
//            // GameOverManager.Instance.ShowGameOver();
//        }
//        if (tags != null && tags.Contains("THE78"))
//        {
//            // например: перейти на сцену
//            SceneManager.LoadScene("The 78");
//            // или вызвать ваш менеджер перехода:
//            // GameOverManager.Instance.ShowGameOver();
//        }

//    }
//    public void ContinueStory(bool choiceBefore = false)
//    {
//        if (_currentStory == null) return;

//        if (_currentStory.canContinue)
//        {
//            ShowDialogue();
//            ShowChoiceButtons();
//        }
//        else if (!choiceBefore)
//        {
//            ExitDialogue();
//        }
//        CheckTagsAndHandle();
//    }
//    //private IEnumerator Dysplayline(string line)
//    //{

//    //    _dialogueText.text = "";
//    //    foreach (char letter in line.ToCharArray())
//    //    {
//    //        _dialogueText.text += letter;
//    //        yield return new WaitForSeconds(typingSpeed / 5);
//    //    }
//    //    isDisplaying = false;
//    //}
//    private void ShowDialogue()
//    {
//        //для печати текста комментируем 111 строку и раскомменчиваем  все что сейчас закомментировано в строках: 97-107, 113-121

//        _dialogueText.text = _currentStory.Continue();
//        //if (_currentStory.canContinue)
//        //{
//        //    isDisplaying = true;
//        //    if (displayLineCoroutine != null)
//        //    {
//        //        StopCoroutine(displayLineCoroutine);
//        //    }
//        //    displayLineCoroutine = StartCoroutine(Dysplayline(_currentStory.Continue()));
//        //}
//        _nameText.text = _currentStory.variablesState["characterName"]?.ToString() ?? "";
//        _smenafona?.changeImage((int)_currentStory.variablesState["fon"]);
//    }
//    private void ShowChoiceButtons()
//    {
//        List<Choice> currentChoices = _currentStory.currentChoices;
//        _choiceButtonsPanel.SetActive(currentChoices.Count != 0);
//        if (currentChoices.Count <= 0) { return; }

//        // Очистка предыдущих кнопок
//        for (int i = _choiceButtonsPanel.transform.childCount - 1; i >= 0; i--)
//            Destroy(_choiceButtonsPanel.transform.GetChild(i).gameObject);

//        _choiceText.Clear();

//        for (int i = 0; i < currentChoices.Count; i++)
//        {
//            int index = i; // локальная копия для корректного захвата в лямбде
//            GameObject choice = Instantiate(_choiceButton, _choiceButtonsPanel.transform, false);

//            // Устанавливаем текст
//            TextMeshProUGUI choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
//            if (choiceText != null) choiceText.text = currentChoices[i].text;

//            // Сохраняем индекс в компоненте, если он нужен где-то ещё
//            var btnComp = choice.GetComponent<ButtenActions>();
//            if (btnComp != null) btnComp.index = index;

//            // Назначаем слушатель прямо здесь — надежнее, чем искать объект внутри Start()
//            var btn = choice.GetComponent<Button>();
//            if (btn != null)
//            {
//                btn.onClick.RemoveAllListeners();
//                btn.onClick.AddListener(() =>
//                {
//                    DisableAllChoiceButtons();
//                    ChoiceButtonAction(index);
//                });
//            }
//        }
//    }

//    private void DisableAllChoiceButtons()
//    {
//        foreach (Transform t in _choiceButtonsPanel.transform)
//        {
//            var b = t.GetComponent<Button>();
//            if (b != null) b.interactable = false;
//        }
//    }
//    public void ChoiceButtonAction(int choiceIndex)
//    {
//        // Безопасная проверка индекса — предотвращает исключение
//        int count = _currentStory.currentChoices.Count;
//        if (choiceIndex < 0 || choiceIndex >= count)
//        {
//            Debug.LogWarning($"Choice index {choiceIndex} out of range (0..{count - 1}). Ignoring.");
//            Debug.Log($"Available choices: {count}");
//            for (int j = 0; j < count; j++)
//                Debug.Log($"Choice {j}: {_currentStory.currentChoices[j].text}");
//            return;
//        }

//        _currentStory.ChooseChoiceIndex(choiceIndex);
//        ContinueStory(true);
//    }

//    public void ExitDialogue()
//    {
//        DialogPlay = false;
//        _dialoguePanel.SetActive(false);
//        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
//        if (nextSceneIndex <= SceneManager.sceneCountInBuildSettings)
//        {
//            SceneManager.LoadScene(nextSceneIndex);
//        }
//    }
//}