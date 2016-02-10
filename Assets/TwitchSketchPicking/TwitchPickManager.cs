using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TwitchPickManager : MonoBehaviour
{
    public bool acceptMultiple;
    public Button BackToMenu;
    public bool chromakey;
    public string cmdStr;
    private bool collectingSuggestions;
    public TextBasedOnSlider countDownSlider;

    private bool doingVote;
    public Button ExportList;

    public TwitchIRC IRC;

    private int lastCount;

    public Color textColor1 = Color.cyan;
    public Color textColor2 = Color.red;

    public Dictionary<string, List<string>> objectsUserDictionary = new Dictionary<string, List<string>>();
    public string objsStr;
    public string objStr;
    public Dictionary<int, VoteOption> optionToIndex = new Dictionary<int, VoteOption>();

    public bool pickInstant;
    public Text pickRandomText;
    public Button PickSketchButton;
    public Text PickText;
    public bool removeAfterPick;
    public Button ResetButton;
    public Button StartStopButton;
    public Text StartStopText;
    public string startWithOptions;
    public string titleStr;
    public Text titleText;

    public int type;
    public const int pickSuggestionType = 0;
    public const int voteSuggestionType = 1;
    public const int pickUserType = 2;

    public HashSet<string> usernamesVotedAlready = new HashSet<string>();

    public float waitForTime;

    #region Initialization
    private void Awake()
    {
        InitializeIRC();
    }

    // Use this for initialization
    private void Start()
    {
        type = PlayerPrefs.GetInt("type", 0);
        switch (type)
        {
            case pickSuggestionType:
                InitializePickingSuggestion();
                break;
            case voteSuggestionType:
                InitializeVotingSuggestion();
                break;
            case pickUserType:
                InitializePickUser();
                break;
        }
        InitializeCommon();
        
        IRC.messageRecievedEvent.AddListener(OnChatMsgRecieved);
        StartStopButton.onClick.AddListener(StartStopClicked);
        PickSketchButton.onClick.AddListener(PickRandomObject);
        ResetButton.onClick.AddListener(ResetDictionary);
        ExportList.onClick.AddListener(DoExportList);
        BackToMenu.onClick.AddListener(ReturnToMenu);
        UpdateButtons();
    }

    private void InitializeCommon()
    {
        removeAfterPick = PlayerPrefs.GetInt("removeafter", 0) == 1 ? true : false;
        pickInstant = PlayerPrefs.GetInt("animatebefore", 0) == 0 ? true : false;
        chromakey = PlayerPrefs.GetInt("chromakey", 0) == 1 ? true : false;
        waitForTime = PlayerPrefs.GetFloat("stoptime", 0);

        titleText.text = titleStr;
        countDownSlider.gameObject.SetActive(false);
        if (chromakey) { Camera.main.backgroundColor = Color.green; }
    }

    private void InitializePickUser()
    {
        titleStr = PlayerPrefs.GetString("pickusertitle", "");
        cmdStr = PlayerPrefs.GetString("pickusercmd", "");
        acceptMultiple = false;
        pickRandomText.text = "Pick random user ";
        objStr = PlayerPrefs.GetString("pickuserterm", "");
        objsStr = PlayerPrefs.GetString("pickusersterm", "");
    }

    private void InitializeVotingSuggestion()
    {
        titleStr = PlayerPrefs.GetString("votesugtitle", "");
        cmdStr = PlayerPrefs.GetString("votesuggestioncmd", "");
        objStr = PlayerPrefs.GetString("votesugobj", "");
        objsStr = PlayerPrefs.GetString("votesugobjs", "");
        startWithOptions = PlayerPrefs.GetString("startingoptions", "");
        acceptMultiple = PlayerPrefs.GetInt("voteallowmultiple", 0) == 1 ? true : false;
        pickRandomText.text = "Vote on " + objsStr;
    }

    private void InitializePickingSuggestion()
    {
        titleStr = PlayerPrefs.GetString("picksugtitle", "");
        cmdStr = PlayerPrefs.GetString("picksuggestioncmd", "");
        objStr = PlayerPrefs.GetString("sugobj", "");
        objsStr = PlayerPrefs.GetString("sugobjs", "");
        acceptMultiple = PlayerPrefs.GetInt("sugallowmultiple", 0) == 1 ? true : false;
        pickRandomText.text = "Pick random " + objStr;
    }

    private void InitializeIRC()
    {
        if (IRC == null)
        {
            IRC = GetComponent<TwitchIRC>();
            if (IRC == null)
            {
                IRC = gameObject.AddComponent<TwitchIRC>();
            }
        }
        IRC.channelName = PlayerPrefs.GetString("channelname", "");
        IRC.StartIRC();
    }
    #endregion

    #region Start/Stop
    public void StartStopClicked()
    {
        ShakeScale(StartStopButton.transform);
        PickText.transform.DOKill();
        if (collectingSuggestions || doingVote) StopPicking();
        else StartPicking();
    }

    public void StartPicking()
    {
        collectingSuggestions = true;
        UpdateObjsText();
        if (waitForTime > 0) StartCoroutine("CountDownToStop");

        if (type == 1 && startWithOptions.Length > 0)
        {
            StartWithVotesAdd();
        }
    }

    private IEnumerator CountDownToStop()
    {
        countDownSlider.sldText.transform.DOKill();
        countDownSlider.sldText.transform.localScale = Vector3.one;
        countDownSlider.sldText.transform.DOScale(Vector3.one*1.07f, 0.6f).SetLoops(-1, LoopType.Yoyo);

        var timeElapsed = waitForTime;
        while (timeElapsed > 0)
        {
            if (!countDownSlider.gameObject.activeInHierarchy) countDownSlider.gameObject.SetActive(true);

            countDownSlider.sld.value = timeElapsed/waitForTime;
            countDownSlider.ChangedValue(1);
            countDownSlider.sldText.text = "<color="+HexConverter(textColor2)+">" +
                                           (waitForTime - Mathf.CeilToInt(waitForTime - timeElapsed)) + "</color>" +
                                           countDownSlider.posvalue;
            timeElapsed -= Time.deltaTime;
            yield return null;
        }
        countDownSlider.gameObject.SetActive(false);
        StopPicking();
    }

    public void StopPicking()
    {
        StopCoroutine("CountDownToStop");
        if (doingVote)
        {
            CalculateVoteResults();
        }
        else
        {
            collectingSuggestions = false;
            UpdateObjsText();
        }
        countDownSlider.gameObject.SetActive(false);
    }
    #endregion

    #region HandlingChatMessages
    private void OnChatMsgRecieved(string msg)
    {
        //parse from buffer.
        var msgIndex = msg.IndexOf("PRIVMSG #");
        var msgString = msg.Substring(msgIndex + IRC.channelName.Length + 11);
        msgString = msgString.ToLower();
        var user = msg.Substring(1, msg.IndexOf('!') - 1);

        var words = msgString.Split(' ');

        if (collectingSuggestions && msgString.ToLower().StartsWith(cmdStr))
        {
            ObjectReceived(user, (type == pickUserType) ? "" : msgString.Substring(cmdStr.Length));
        }

        if (doingVote && msgString.ToLower().StartsWith("#vote") && words.Length > 1)
        {
            var tryParseVote = 0;
            var successParseInt = int.TryParse(words[1], out tryParseVote);
            if (successParseInt)
            {
                VoteReceived(user, tryParseVote);
            }
        }

        if (doingVote && user.ToLower() == IRC.channelName.ToLower() && msgString.ToLower().StartsWith("#removevote") && words.Length > 1)
        {
            var tryParseVote = 0;
            var successParseInt = int.TryParse(words[1], out tryParseVote);
            if (successParseInt)
            {
                optionToIndex.Remove(tryParseVote);
                UpdateVotesText();
                if (optionToIndex.Count == 0) { StopPicking(); }
            }
        }
    }

    public void ObjectReceived(string username, string idea)
    {
        if (!objectsUserDictionary.ContainsKey(username))
        {
            AlertManager.Alert(username, AlertManager.Type.Middle, 1.7f, Random.Range(0.35f, 1), Random.Range(0.35f, 1),
                Random.Range(0.35f, 1), 0.75f);
            objectsUserDictionary[username] = new List<string>();
            objectsUserDictionary[username].Add(idea);
            Debug.Log("Added to dictionary for the first time " + username + " / idea: " + idea);
        }
        else
        {
            if (acceptMultiple)
            {
                AlertManager.Alert(username, AlertManager.Type.Middle, 1.7f, Random.Range(0.35f, 1),
                    Random.Range(0.35f, 1),
                    Random.Range(0.35f, 1), 0.75f);
                objectsUserDictionary[username].Add(idea);
                Debug.Log("Added to list of " + username + " / idea: " + idea);
            }
            else
            {
                objectsUserDictionary[username][0] = idea;
                Debug.Log("Added to dictionary " + username + " / idea: " + idea);
            }
        }
        UpdateObjsText();
    }

    public void VoteReceived(string _username, int _voteindex)
    {
        if (!usernamesVotedAlready.Contains(_username.ToLower()) && optionToIndex.ContainsKey(_voteindex))
        {
            usernamesVotedAlready.Add(_username.ToLower());
            optionToIndex[_voteindex].votes++;
            AlertManager.Alert(_username, AlertManager.Type.Middle, 1.5f, Random.Range(0.35f, 1),
                Random.Range(0.35f, 1), Random.Range(0.35f, 1), 0.75f);
        }
    }

    #endregion

    #region Text/ButtonHandling
    public void UpdateObjsText()
    {
        if (lastCount != objectsUserDictionary.Count)
        {
            ShakeScale(PickText.transform);
            lastCount = objectsUserDictionary.Count;
        }

        if (doingVote)
        {
            UpdateButtons();
            return;
        }

        if (collectingSuggestions)
        {
            var totalCount = CountAll();
            if (type < 2)
            {
                //PICK SELECTION
                PickText.text = (totalCount > 0
                    ? "<color=" + HexConverter(textColor1) + ">" + totalCount + "</color> " + (totalCount > 1 ? objsStr : objStr) + " so far!"
                    : "") + "\nTo submit your " + objStr + " type: <color=" + HexConverter(textColor1) + ">" + cmdStr + " " + objStr + "</color>";
            }
            else
            {
                //PICK USER
                PickText.text = (totalCount > 0
                    ? "<color=" + HexConverter(textColor1) + ">" + totalCount + "</color> " + (totalCount > 1 ? objsStr : objStr) + " so far!"
                    : "") + "\nTo enter type: <color=" + HexConverter(textColor1) + ">" + cmdStr + "</color>";
            }
        }
        else
        {
            var amountReceived = CountAll();
            if (type < 2)
            {
                //PCK SELECTION
                PickText.text = amountReceived > 0
                    ? "<color=" + HexConverter(textColor1) + ">" + amountReceived + "</color> " +
                      (amountReceived > 1 ? objStr : objsStr) + " received! Thanks everyone!"
                    : "";
            }
            else
            {
                //PICK USER!
                PickText.text = amountReceived > 0
                    ? "<color=" + HexConverter(textColor1) + ">" + amountReceived + "</color> " +
                      (amountReceived > 1 ? objStr : objsStr) + " entered! Thanks everyone!"
                    : "";
            }
        }
        UpdateButtons();
    }

    public void UpdateButtons()
    {
        var totalObjs = CountAll();
        PickSketchButton.gameObject.SetActive(totalObjs > 0 && !doingVote && !collectingSuggestions);
        ResetButton.gameObject.SetActive(totalObjs > 0);
        if (doingVote)
        {
            StartStopText.text = "Stop voting!";
            StartStopButton.colors = ResetButton.colors;
        }
        else
        {
            StartStopText.text = collectingSuggestions
                ? "Stop receiving " + objsStr + "!"
                : "Start receiving " + objsStr + "!";
            StartStopButton.colors = collectingSuggestions ? ResetButton.colors : PickSketchButton.colors;
        }
        ExportList.gameObject.SetActive(totalObjs > 0 && type == 2);
    }

    public void SetObjectText(string username, bool forReals = true)
    {
        if (!objectsUserDictionary.ContainsKey(username)) return;
        if (forReals)
        {
            PickText.transform.DOKill();
            PickText.transform.localScale = Vector3.one;
            PickText.transform.DOScale(Vector3.one*1.07f, 0.6f).SetLoops(-1, LoopType.Yoyo);
            var pickedIdeaIndex = Random.Range(0, objectsUserDictionary[username].Count);
            if (type == pickUserType)
            {
                //PICK USER
                PickText.text = "<color=" + HexConverter(textColor1) + ">" + username + "</color>";
            }
            else
            {
                //PICKING SUGGESTION
                PickText.text = "<color=" + HexConverter(textColor1) + ">" + username + "</color>: " +
                                objectsUserDictionary[username][pickedIdeaIndex];
            }
            if (removeAfterPick)
            {
                if (objectsUserDictionary[username].Count > 1) { objectsUserDictionary[username].RemoveAt(pickedIdeaIndex); }
                else { objectsUserDictionary.Remove(username); }
            }
        }
        else
        {
            ShakeScale(PickText.transform, 0.75f, 0.1f);
            if (type == pickUserType)
            {
                PickText.text = "<color=#4d4d4d>" + username + "</color>";
            }
            else
            {
                PickText.text = "<color=#4d4d4d>" + username + ": " + objectsUserDictionary[username][Random.Range(0, objectsUserDictionary[username].Count)] + "</color>";
            }
        }
        UpdateButtons();
    }

    public void ShakeScale(Transform t, float scale=1, float duration=0.15f)
    {
        t.DOKill();
        t.localScale = Vector3.one * scale;
        t.DOShakeScale(duration, 0.4f);
    }
#endregion

    #region HandleVoting
    public void StartWithVotesAdd()
    {
        var words = startWithOptions.Split(',');
        objectsUserDictionary.Add(IRC.channelName, new List<string>());
        foreach (var _word in words)
        {
            objectsUserDictionary[IRC.channelName].Add(_word);
        }
        UpdateButtons();
        UpdateObjsText();
    }

    private void StartVoting()
    {
        doingVote = true;
        UpdateButtons();
        if (waitForTime == 0) waitForTime = 60;

        optionToIndex.Clear();
        usernamesVotedAlready.Clear();
        var optionIndex = 0;
        foreach (var _userlist in objectsUserDictionary)
        {
            foreach (var _s in _userlist.Value)
            {
                optionToIndex.Add(optionIndex, new VoteOption(_userlist.Key, _s));
                optionIndex++;
            }
        }
        UpdateVotesText();
        StartCoroutine("CountDownToStop");
    }

    private void UpdateVotesText()
    {
        var voteOptions =
            "<size=24>Type <color=" + HexConverter(textColor1) + ">#VOTE</color> <color=#D43115>number</color> to vote!</size>\n<size=30>";
        var optionIndex = 0;
        foreach (var _option in optionToIndex)
        {
            voteOptions += "     <color=" + HexConverter(textColor2) + ">[" + _option.Key + "]</color> <color=#c0c0c0ff><i>" +
                           _option.Value.option +
                           "</i></color>     ";
            optionIndex++;
        }
        voteOptions += "</size>";
        PickText.text = voteOptions;
    }

    public void CalculateVoteResults()
    {
        doingVote = false;
        StopCoroutine("CountDownToStop");

        var winnerOption = new VoteOption("", "");

        var maxVotes = -1;
        Debug.Log("Checking votes, amount of options = " + optionToIndex.Count);
        foreach (var _option in optionToIndex.Values)
        {
            if (_option.votes > maxVotes)
            {
                maxVotes = _option.votes;
                Debug.Log("Found new winning option " + _option.option + " / vote amount" + _option.votes);
                winnerOption = new VoteOption(_option.username, _option.option);
            }
        }

        PickText.transform.DOKill();
        PickText.transform.localScale = Vector3.one;
        PickText.transform.DOScale(Vector3.one*1.07f, 0.6f).SetLoops(-1, LoopType.Yoyo);
        Debug.Log("Winner is " + winnerOption.option);
        PickText.text = "<color=" + HexConverter(textColor1) + ">" + winnerOption.username + "</color>: " + winnerOption.option;
        UpdateButtons();
    }
    #endregion

 #region PickingObject
    public void PickRandomObject()
    {
        if (type == 0 || type == 2)
        {
            StopPicking();
            ShakeScale(PickSketchButton.transform);
            if (pickInstant)
            {
                SetObjectText(objectsUserDictionary.ElementAt(Random.Range(0, objectsUserDictionary.Count)).Key, true);
            }
            else
            {
                PickSketchButton.gameObject.SetActive(false);
                StopCoroutine("DOPickRandom");
                StartCoroutine("DOPickRandom");
            }
        }
        else
        {
            StartVoting();
        }
    }

    private IEnumerator DOPickRandom()
    {
        float totalDuration = 15;
        var duration = totalDuration;
        while (duration > 0)
        {
            SetObjectText(objectsUserDictionary.ElementAt(Random.Range(0, objectsUserDictionary.Count)).Key, false);
            duration--;
            yield return new WaitForSeconds((totalDuration - duration)*0.015f);
        }

        SetObjectText(objectsUserDictionary.ElementAt(Random.Range(0, objectsUserDictionary.Count)).Key, true);

        yield return null;

        if (objectsUserDictionary.Count > 0)
        {
            PickSketchButton.gameObject.SetActive(true);
        }
    }
    #endregion

    public void ResetDictionary()
    {
        ShakeScale(ResetButton.transform);
        PickText.transform.DOKill();
        objectsUserDictionary.Clear();
        UpdateObjsText();
    }

    public void DoExportList()
    {
        var usernamestowrite = "";
        foreach (var _obj in objectsUserDictionary.Keys)
        {
            usernamestowrite = string.IsNullOrEmpty(usernamestowrite) ? _obj : usernamestowrite + " " + _obj;
        }

        File.WriteAllText(Application.dataPath + "/pickUsersList.txt", usernamestowrite);
        OpenFolder.OpenInFileBrowser(Application.dataPath);
    }

    public void ReturnToMenu()
    {
        Application.LoadLevel(0);
    }

    public int CountAll()
    {
        var total = 0;
        foreach (var _objlist in objectsUserDictionary.Values)
        {
            total += _objlist.Count;
        }
        return total;
    }

    private string HexConverter(Color32 c)
    {
        return "#" + c.r.ToString("X2") + c.g.ToString("X2") + c.b.ToString("X2");
    }

    public class VoteOption
    {
        public string option;
        public string username;
        public int votes;

        public VoteOption(string username, string option)
        {
            this.username = username;
            this.option = option;
            votes = 0;
        }
    }
}