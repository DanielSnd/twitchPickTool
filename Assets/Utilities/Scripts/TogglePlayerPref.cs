using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class TogglePlayerPref : MonoBehaviour {
    public int defaultValue;
    public string valueName;
    public Toggle tog;

    public UnityEvent onChangeEvent;

    public void Awake()
    {
        if (tog == null)
            tog = GetComponent<Toggle>();
        if (tog == null)
            tog = GetComponentInChildren<Toggle>();
        if (!string.IsNullOrEmpty(valueName))
        {
            bool myvalue = PlayerPrefs.GetInt(valueName, defaultValue) == 1;
            tog.isOn = myvalue;
        }
        tog.onValueChanged.AddListener(changedValue);
        changedValue(true);
    }

    public void changedValue(bool _s)
    {
        PlayerPrefs.SetInt(valueName, tog.isOn ? 1 : 0);
        PlayerPrefs.Save();
        onChangeEvent.Invoke();
    }
}
