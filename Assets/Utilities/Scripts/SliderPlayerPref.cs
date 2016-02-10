using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderPlayerPref : MonoBehaviour {
    public float defaultValue;
    public string valueName;
    public Slider sld;

    public UnityEvent onChangeEvent;

    public void Awake()
    {
        if (sld == null)
            sld = GetComponent<Slider>();
        if (sld == null)
            sld = GetComponentInChildren<Slider>();
        if (!string.IsNullOrEmpty(valueName))
        {
            float myvalue = PlayerPrefs.GetFloat(valueName, defaultValue);
            sld.value = myvalue;
        }
        sld.onValueChanged.AddListener(changedValue);
        changedValue(0);
    }

    public void changedValue(float _s)
    {
        PlayerPrefs.SetFloat(valueName, sld.value);
        PlayerPrefs.Save();
        onChangeEvent.Invoke();
    }
}
