using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Button pickSuggestionButton;
    public Button voteSuggestionButton;
    public Button pickUserButton;
    public Button startButton;
    public Text titleText;

    public GameObject[] pickSuggestionObjects;
    public GameObject[] voteSuggestionObjects;
    public GameObject[] pickUserObjects;

    // Use this for initialization
    void Start () {
	    pickSuggestionButton.onClick.AddListener(pickSuggestion);
        voteSuggestionButton.onClick.AddListener(voteSuggestion);
        pickUserButton.onClick.AddListener(pickUser);
        startButton.onClick.AddListener(PressStart);
        int type = PlayerPrefs.GetInt("type", 0);
        if (type == 0)
        {
            pickSuggestion();
        } else if (type == 1)
        {
            voteSuggestion();
        }
        else
        {
            pickUser();
        }
        UpdateStartButton();
    }

    public void UpdateStartButton()
    {
        startButton.gameObject.SetActive(!string.IsNullOrEmpty(PlayerPrefs.GetString("channelname","")));
    }

    public void pickSuggestion()
    {
        titleText.text = "Pick Suggestion";
        pickSuggestionButton.interactable = false;
        voteSuggestionButton.interactable = true;
        pickUserButton.interactable = true;
        UpdateOptions();
    }

    public void voteSuggestion()
    {
        titleText.text = "Vote Suggestion";
        pickSuggestionButton.interactable = true;
        voteSuggestionButton.interactable = false;
        pickUserButton.interactable = true;
        UpdateOptions();
    }

    public void pickUser()
    {
        titleText.text = "Pick User";
        pickSuggestionButton.interactable = true;
        voteSuggestionButton.interactable = true;
        pickUserButton.interactable = false;
        UpdateOptions();
    }

    public void UpdateOptions()
    {
        foreach (GameObject _o in pickSuggestionObjects)
        {
            _o.SetActive(!pickSuggestionButton.interactable);
        }
        foreach (GameObject _o in voteSuggestionObjects)
        {
            _o.SetActive(!voteSuggestionButton.interactable);
        }
        foreach (GameObject _o in pickUserObjects)
        {
            _o.SetActive(!pickUserButton.interactable);
        }
    }

    public void PressStart()
    {
        if (!pickSuggestionButton.interactable)
        {
            PlayerPrefs.SetInt("type", 0);
        }
        if (!voteSuggestionButton.interactable)
        {
            PlayerPrefs.SetInt("type", 1);
        }
        if (!pickUserButton.interactable)
        {
            PlayerPrefs.SetInt("type", 2);
        }
        PlayerPrefs.Save();
        Application.LoadLevel(1);
    }
}
