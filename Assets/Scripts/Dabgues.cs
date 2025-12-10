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
    private Coroutine displayLineCoroutine;
    public bool DialogPlay { get; private set; }
    private bool canContinueToNextLine = false;
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
    private IEnumerator Dysplayline(string line)
    {

        _dialogueText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed / 5);
        }
    }
    private void ShowDialogue()
    {
        // Если нужно читать несколько строк подряд, можно заменить на цикл while (story.canContinue) { ... }
        if (displayLineCoroutine != null)
        {
            StopCoroutine(displayLineCoroutine);
        }
        /*StopCoroutine(displayLineCoroutine);*/
        displayLineCoroutine = StartCoroutine(Dysplayline(_currentStory.Continue()));


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
}