using TMPro;
using UnityEngine;
using Zenject;

public class DialoguesInstaller : MonoInstaller
{
    [SerializeField] public TextAsset inkJson;
    [SerializeField] public GameObject dialoguePanel;
    [SerializeField] public TextMeshProUGUI dialogueText;
    [SerializeField] public TextMeshProUGUI nameText;
    [SerializeField] public GameObject choiceButtonsPanel;
    [SerializeField] public GameObject choiceButton;
    [SerializeField] public GameObject backButton; // Добавьте это поле

    public override void InstallBindings()
    {
        Container.BindInstance(inkJson);
        Container.BindInstance(dialoguePanel);
        Container.BindInstance(dialogueText);
        Container.BindInstance(nameText);
        Container.BindInstance(choiceButtonsPanel);
        Container.BindInstance(choiceButton);
        Container.BindInstance(backButton); // И это
    }
}
