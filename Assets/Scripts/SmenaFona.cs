using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmenaFona : MonoBehaviour
{
    public List<Sprite> smena;
    public void changeImage(int currentImage)
    {
        GetComponent<Image>().sprite = smena[currentImage];
    }
}
