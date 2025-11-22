using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Добавлено для работы с UI
using Zenject;

public class Dabgues : MonoBehaviour
{
    private Story _currentStory;
    [SerializeField] private TextAsset _inkJson; // Добавлен [SerializeField]

    private GameObject _dialoguePanel;
    private TextMeshProUGUI _dialogueText;
    private TextMeshProUGUI _nameText;

    private GameObject _choiceButtonsPanel;
    private GameObject _choiceButton;
    private List<TextMeshProUGUI> _choiceText = new(); // Исправлено имя переменной

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
        _currentStory = new Story(_inkJson.text); // Исправлено: Story с заглавной S
    }

    void Start()
    {
        StartDialogue();
    }

    public void StartDialogue()
    {
        DialogPlay = true;
        _dialoguePanel.SetActive(true);
        ContinueStory();
    }

    public void ContinueStory(bool choiceBefore = false)
    {
        if (_currentStory.canContinue)
        {
            ShowDialogue();
            ShowChoiceButtons();
        }
        else if (!choiceBefore)
        {
            ExitDialogue(); // Исправлено имя метода
        }
    }

    private void ShowDialogue()
    {
        _dialogueText.text = _currentStory.Continue();
        _nameText.text = (string)_currentStory.variablesState["characterName"]; // Исправлено: variablesState
    }

    private void ShowChoiceButtons()
    {
        List<Choice> currentChoices = _currentStory.currentChoices; // Исправлено: Choice
        _choiceButtonsPanel.SetActive(currentChoices.Count != 0);
        if (currentChoices.Count <= 0) { return; }

        // Очистка предыдущих кнопок
        foreach (Transform child in _choiceButtonsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < currentChoices.Count; i++)
        {
            GameObject choice = Instantiate(_choiceButton);
            choice.GetComponent<ButtenActions>().index = i;
            choice.transform.SetParent(_choiceButtonsPanel.transform);

            TextMeshProUGUI choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
            choiceText.text = currentChoices[i].text;
            // Убрана строка: choiceText.text = choiceText; - это вызывало ошибку
        }
    }

    public void ChoiceButtonAction(int choiceIndex)
    {
        _currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory(true); // Добавлен параметр
    }

    private void ExitDialogue() // Исправлено имя метода 
    {
        DialogPlay = false;
        _dialoguePanel.SetActive(false);
    }
}