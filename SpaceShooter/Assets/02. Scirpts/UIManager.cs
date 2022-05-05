using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Button startButton;
    public Button optionButton;
    public Button shopButton;

    private UnityAction action;

    private void Start()
    {
        action = () =>OnButtonClick(startButton.name);
        startButton.onClick.AddListener(action);

        optionButton.onClick.AddListener(delegate { OnButtonClick(optionButton.name); });

        shopButton.onClick.AddListener(() => OnButtonClick(shopButton.name));
    }
    public void OnButtonClick(string msg)
    {
        Debug.Log($"Click Button: {msg}");
    }
}
