using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class texttyping : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;
    private int index;
    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }
    private string fulltext;
    private bool isTyping;
    private bool isCorroutineStopped = false;
    private void Start()
    {
        textComponent.text = "";
        StartDialogue();
    }
    private IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed/5);
        }
    }
}


