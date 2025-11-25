using Ink.Runtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class Dabgues : MonoBehaviour
{
    private Story _currentStory;
    [SerializeField] private TextAsset _inkJson;

    private GameObject _dialoguePanel;
    private TextMeshProUGUI _dialogueText;
    private TextMeshProUGUI _nameText;

    [HideInInspector] public GameObject _choiceButtonsPanel;
    private GameObject _choiceButton;
    private List<TextMeshProUGUI> _choiceText = new();
    private SmenaFona _smenafona;

    public bool DialogPlay { get; private set; }

    [Inject]
    public void Construct(DialoguesInstaller dialoguesInstaller)
    {
        _inkJson = dialoguesInstaller.inkJson;
        _dialoguePanel = dialoguesInstaller.dialoguePanel;
        _dialogueText = dialoguesInstaller.dialogueText;
        _nameText = dialoguesInstaller.nameText;
        _choiceButtonsPanel = dialoguesInstaller.choiceButtonsPanel;
        _choiceButton = dialoguesInstaller.choiceButton;
    }

    private void Awake()
    {
        _currentStory = new Story(_inkJson.text);
    }

    void Start()
    {
        foreach (var fon in FindObjectsOfType<SmenaFona>())
            _smenafona = fon;

        StartDialogue();
    }

    public void StartDialogue()
    {
        DialogPlay = true;
        _dialoguePanel.SetActive(true);
        ContinueStory();
    }
    private void CheckTagsAndHandle()
    {
        var tags = _currentStory.currentTags;
        if (tags != null && tags.Contains("GAME_OVER"))
        {
            // например: перейти на сцену
            SceneManager.LoadScene("GameOver");
            // или вызвать ваш менеджер перехода:
            // GameOverManager.Instance.ShowGameOver();
        }
        if (tags != null && tags.Contains("THE78"))
        {
            // например: перейти на сцену
            SceneManager.LoadScene("The 78");
            // или вызвать ваш менеджер перехода:
            // GameOverManager.Instance.ShowGameOver();
        }
        
    }
    public void ContinueStory(bool choiceBefore = false)
    {
        if (_currentStory == null) return;

        if (_currentStory.canContinue)
        {
            ShowDialogue();
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
        // Если нужно читать несколько строк подряд, можно заменить на цикл while (story.canContinue) { ... }
        _dialogueText.text = _currentStory.Continue();
        _nameText.text = _currentStory.variablesState["characterName"]?.ToString() ?? "";
        _smenafona?.changeImage((int)_currentStory.variablesState["fon"]);
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
            int index = i; // локальная копия для корректного захвата в лямбде
            GameObject choice = Instantiate(_choiceButton, _choiceButtonsPanel.transform, false);

            // Устанавливаем текст
            TextMeshProUGUI choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
            if (choiceText != null) choiceText.text = currentChoices[i].text;

            // Сохраняем индекс в компоненте, если он нужен где-то ещё
            var btnComp = choice.GetComponent<ButtenActions>();
            if (btnComp != null) btnComp.index = index;

            // Назначаем слушатель прямо здесь — надежнее, чем искать объект внутри Start()
            var btn = choice.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
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
        // Безопасная проверка индекса — предотвращает исключение
        int count = _currentStory.currentChoices.Count;
        if (choiceIndex < 0 || choiceIndex >= count)
        {
            Debug.LogWarning($"Choice index {choiceIndex} out of range (0..{count - 1}). Ignoring.");
            Debug.Log($"Available choices: {count}");
            for (int j = 0; j < count; j++)
                Debug.Log($"Choice {j}: {_currentStory.currentChoices[j].text}");
            return;
        }

        _currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory(true);
    }

    private void ExitDialogue()
    {
        DialogPlay = false;
        _dialoguePanel.SetActive(false);
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex <= SceneManager.sceneCount)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}




//using Ink.Runtime;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI; // Добавлено для работы с UI
//using Zenject;

//public class Dabgues : MonoBehaviour
//{
//    private Story _currentStory;
//    [SerializeField] private TextAsset _inkJson; // Добавлен [SerializeField]

//    private GameObject _dialoguePanel;
//    private TextMeshProUGUI _dialogueText;
//    private TextMeshProUGUI _nameText;

//    [HideInInspector]public GameObject _choiceButtonsPanel;
//    private GameObject _choiceButton;
//    private List<TextMeshProUGUI> _choiceText = new(); // Исправлено имя переменной
//    private SmenaFona _smenafona;

//    public bool DialogPlay { get; private set; }
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
//        _currentStory = new Story(_inkJson.text); // Исправлено: Story с заглавной S
//    }

//    void Start()
//    {
//        foreach(var fon in FindObjectsOfType<SmenaFona>())
//        {
//            _smenafona = fon;
//        }

//        StartDialogue();
//    }

//    public void StartDialogue()
//    {
//        DialogPlay = true;
//        _dialoguePanel.SetActive(true);
//        ContinueStory();
//    }

//    public void ContinueStory(bool choiceBefore = false)
//    {
//        if (_currentStory.canContinue)
//        {
//            ShowDialogue();
//            ShowChoiceButtons();
//        }
//        else if (!choiceBefore)
//        {
//            ExitDialogue();
//        }

//    }

//    private void ShowDialogue()
//    {
//        _dialogueText.text = _currentStory.Continue();
//        _nameText.text = (string)_currentStory.variablesState["characterName"]; // Исправлено: variablesState

//        _smenafona.changeImage((int)_currentStory.variablesState["fon"]);
//    }

//    private void ShowChoiceButtons()
//    {
//        List<Choice> currentChoices = _currentStory.currentChoices; // Исправлено: Choice
//        _choiceButtonsPanel.SetActive(currentChoices.Count != 0);
//        if (currentChoices.Count <= 0) { return; }

//        // Очистка предыдущих кнопок
//        _choiceButtonsPanel.transform.Cast<Transform>().ToList().ForEach(child => Destroy(child.gameObject));
//        _choiceText.Clear();
//        for (int i = 0; i < currentChoices.Count; i++)
//        {
//            GameObject choice = Instantiate(_choiceButton);
//            choice.GetComponent<ButtenActions>().index = i;
//            choice.transform.SetParent(_choiceButtonsPanel.transform);

//            TextMeshProUGUI choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
//            choiceText.text = currentChoices[i].text;
//            // Убрана строка: choiceText.text = choiceText; - это вызывало ошибку
//        }
//    }

//    public void ChoiceButtonAction(int choiceIndex)
//    {
//        _currentStory.ChooseChoiceIndex(choiceIndex);
//        ContinueStory(true); // Добавлен параметр
//    }

//    private void ExitDialogue() // Исправлено имя метода 
//    {
//        DialogPlay = false;
//        _dialoguePanel.SetActive(false);
//        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
//        if (nextSceneIndex <= SceneManager.sceneCount)
//        {
//            SceneManager.LoadScene(nextSceneIndex);
//        }
//    }
//}