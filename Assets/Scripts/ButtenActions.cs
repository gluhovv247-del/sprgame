using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class ButtenActions : MonoBehaviour
{
    public int index;
    private Button _button;
    private Dabgues _dialogues;
    private UnityAction _clickAction;

    void Start()
    {
        _button = GetComponent<Button>();
        _dialogues = FindObjectOfType<Dabgues>();
        _clickAction = new UnityAction(()=>_dialogues.ChoiceButtonAction(index));
        _button.onClick.AddListener(_clickAction);
    }

}
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.UI;

//public class ButtenActions : MonoBehaviour
//{
//    public int index;
//    private Button _button;
//    private Dabgues _dialogues; 
//    private UnityAction _clickAction;

//    void Start()
//    {
//        _button = GetComponent<Button>(); 
//        _dialogues = FindObjectOfType<Dabgues>(); 
//        _clickAction = () => _dialogues.ChoiceButtonAction(index); 
//        _button.onClick.AddListener(_clickAction);
//    }
//}