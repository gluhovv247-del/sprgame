using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InputReader : MonoBehaviour, Controls.IDialogueActions
{
    Controls _inputActions;
    Dabgues _dialogues;
    private void OnEnable()
    {
        _dialogues = FindObjectOfType<Dabgues>();
        if (_inputActions != null)
        {
            return;
        }
        _inputActions = new Controls();
        _inputActions.Dialogue.SetCallbacks(this);
        _inputActions.Dialogue.Enable();
    }
    private void OnDisable()
    {
        _inputActions.Dialogue.Disable();
    }
    public void OnNextPhrase(InputAction.CallbackContext context)
    {
        if (context.started && _dialogues.DialogPlay)
        {
            _dialogues.ContinueStory(_dialogues._choiceButtonsPanel.activeInHierarchy);
        }
    }
    public Text uiText;
    public float typingSpeed = 0.1f;
    public GameObject nextButton;

    private string fulltext;
    private bool isTyping;
    private bool isCorroutineStopped = false;
}

 


