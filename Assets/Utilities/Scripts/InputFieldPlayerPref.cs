using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputFieldPlayerPref : MonoBehaviour
{
    public string defaultValue;
    public string valueName;
    public InputField iField;

    public UnityEvent onChangeEvent;

    public void Awake()
    {
        if (iField == null)
            iField = GetComponent<InputField>();
        if (iField == null)
            iField = GetComponentInChildren<InputField>();
        if (!string.IsNullOrEmpty(valueName))
        {
            string myvalue = PlayerPrefs.GetString(valueName, defaultValue);
            iField.text = myvalue;
        }
        iField.onValueChange.AddListener(changedValue);
        changedValue("");
    }

    public void changedValue(string _s)
    {
        PlayerPrefs.SetString(valueName,iField.text);
        PlayerPrefs.Save();
        onChangeEvent.Invoke();
    }
}
