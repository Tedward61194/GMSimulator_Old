using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMButtonHandler : MonoBehaviour {
    public void ButtonPressed(string buttonName) {
        Text txt = transform.Find("Text").GetComponent<Text>();
        txt.text = buttonName;
    }
}
