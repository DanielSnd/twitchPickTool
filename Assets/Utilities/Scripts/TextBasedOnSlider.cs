using UnityEngine;
using System.Collections;
using System.ComponentModel;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextBasedOnSlider : MonoBehaviour
{
    public Slider sld;
    public Text sldText;
    public string zerovalue = "";
    public string prevalue = "";
    public string posvalue = "";

	// Use this for initialization
	void Start ()
	{
	    sld.onValueChanged.AddListener(ChangedValue);
	    ChangedValue(0);
	}

    public void ChangedValue(float _v)
    {
        sldText.text = (Mathf.FloorToInt(sld.value) == 0) ? zerovalue : prevalue + (Mathf.FloorToInt(sld.value)).ToString() + posvalue;
    }
}
