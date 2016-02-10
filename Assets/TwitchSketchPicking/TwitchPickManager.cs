using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine.UI;


public class TwitchPickManager : MonoBehaviour {
    public class VoteOption
    {
        public string username;
        public string option;
        public int votes;

        public VoteOption(string username, string option)
        {
            this.username = username;
            this.option = option;
            this.votes = 0;
        }
    }
    
    public TwitchIRC IRC;
    
    public Dictionary<string,List<string>> objectsUserDictionary = new Dictionary<string, List<string>>();
    public Dictionary<int, VoteOption> optionToIndex = new Dictionary<int, VoteOption>();
    public HashSet<string> usernamesVotedAlready = new HashSet<string>(); 
    public Button StartStopButton;
    public Button PickSketchButton;
    public Button ResetButton;
    public Button ExportList;
    public Button BackToMenu;
    public TextBasedOnSlider countDownSlider;
    public Text titleText;
    public Text StartStopText;
    public Text pickRandomText;
    public Text PickText;

    public bool pickInstant = false;
    private bool collectingIdeas = false;
    public bool acceptMultiple = false;
    public bool removeAfterPick = false;
    public bool chromakey = false;

    private bool doingVote;

    public float waitForTime = 0;

    private int lastCount;

    public int type;
    public string objStr;
    public string objsStr;
    public string cmdStr;
    public string titleStr;
    public string startWithOptions;

    void Awake()
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

    // Use this for initialization
    void Start()
    {
        type = PlayerPrefs.GetInt("type", 0);
        switch (type)
        {
            case 0:
                titleStr = PlayerPrefs.GetString("picksugtitle", "");
                cmdStr = PlayerPrefs.GetString("picksuggestioncmd", "");
                objStr = PlayerPrefs.GetString("sugobj", "");
                objsStr = PlayerPrefs.GetString("sugobjs", "");
                acceptMultiple = PlayerPrefs.GetInt("sugallowmultiple", 0) == 1 ? true : false;
                pickRandomText.text = "Pick random " + objStr;
                break;
            case 1:
                titleStr = PlayerPrefs.GetString("votesugtitle", "");
                cmdStr = PlayerPrefs.GetString("votesuggestioncmd", "");
                objStr = PlayerPrefs.GetString("votesugobj", "");
                objsStr = PlayerPrefs.GetString("votesugobjs", "");
                startWithOptions = PlayerPrefs.GetString("startingoptions", "");
                acceptMultiple = PlayerPrefs.GetInt("voteallowmultiple", 0) == 1 ? true : false;
                pickRandomText.text = "Vote on " + objsStr;
                break;
            case 2:
                titleStr = PlayerPrefs.GetString("pickusertitle", "");
                cmdStr = PlayerPrefs.GetString("pickusercmd", "");
                acceptMultiple = false;
                pickRandomText.text = "Pick random user ";
                objStr = PlayerPrefs.GetString("pickuserterm", "");
                objsStr = PlayerPrefs.GetString("pickusersterm", "");
                break;
        }
        removeAfterPick = PlayerPrefs.GetInt("removeafter", 0) == 1 ? true : false;
        pickInstant = PlayerPrefs.GetInt("animatebefore", 0) == 0 ? true : false;
        chromakey = PlayerPrefs.GetInt("chromakey", 0) == 1 ? true : false;
        waitForTime = PlayerPrefs.GetFloat("stoptime", 0);
        countDownSlider.gameObject.SetActive(false);
        titleText.text = titleStr;
        if (chromakey)
        {
            Camera.main.backgroundColor = Color.green;
        }
        IRC.messageRecievedEvent.AddListener(OnChatMsgRecieved);
        StartStopButton.onClick.AddListener(StartStopClicked);
        PickSketchButton.onClick.AddListener(PickRandomObject);
        ResetButton.onClick.AddListener(ResetDictionary);
        ExportList.onClick.AddListener(DoExportList);
        BackToMenu.onClick.AddListener(ReturnToMenu);
        UpdateButtons();
    }
    
    public void ReturnToMenu()
    {
        Application.LoadLevel(0);
    }

    public void ShakeScale(Transform t)
    {
        t.DOKill();
        t.localScale = Vector3.one;
        t.DOShakeScale(0.15f, 0.4f);
    }

    public void StartStopClicked()
    {
        ShakeScale(StartStopButton.transform);
        PickText.transform.DOKill();
        if(collectingIdeas || doingVote) StopPicking();
        else StartPicking();
    }

    public void StartPicking()
    {
        collectingIdeas = true;
        UpdateObjsText();
        if (waitForTime > 0) StartCoroutine("CountDownToStop");

        if (type == 1 && startWithOptions.Length > 0)
        {
            StartWithVotesAdd();
        }
    }

    public void StartWithVotesAdd()
    {
        string[] words = startWithOptions.Split(',');
        objectsUserDictionary.Add(IRC.channelName, new List<string>());
        foreach (string _word in words)
        {
            objectsUserDictionary[IRC.channelName].Add(_word);
        }
        UpdateButtons();
        UpdateObjsText();
    }

    IEnumerator CountDownToStop()
    {
        countDownSlider.sldText.transform.DOKill();
        countDownSlider.sldText.transform.localScale = Vector3.one;
        countDownSlider.sldText.transform.DOScale(Vector3.one * 1.07f, 0.6f).SetLoops(-1, LoopType.Yoyo);

        float timeElapsed = waitForTime;
        while (timeElapsed>0)
        {
            if (!countDownSlider.gameObject.activeInHierarchy) countDownSlider.gameObject.SetActive(true);

            countDownSlider.sld.value = ((timeElapsed / waitForTime));
            countDownSlider.ChangedValue(1);
            //countDownSlider.sldText.text = "current = " + currentTimeElapsed.ToString("0.0") + " / timeremaining " + timeRemaining.ToString("0.0");
            countDownSlider.sldText.text = "<color=#D43115>"+(waitForTime-Mathf.CeilToInt(waitForTime - timeElapsed))+"</color>" + countDownSlider.posvalue;
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
            collectingIdeas = false;
            UpdateObjsText();
        }
        countDownSlider.gameObject.SetActive(false);
    }

    public void ResetDictionary()
    {
        ShakeScale(ResetButton.transform);
        PickText.transform.DOKill();
        objectsUserDictionary.Clear();
        UpdateObjsText();
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
                AlertManager.Alert(username, AlertManager.Type.Middle, 1.7f, Random.Range(0.35f, 1), Random.Range(0.35f, 1),
                Random.Range(0.35f, 1), 0.75f);
                objectsUserDictionary[username].Add(idea);
                Debug.Log("Added to list of "+username+" / idea: "+idea);
            }
            else
            {
                objectsUserDictionary[username][0] = idea;
                Debug.Log("Added to dictionary " + username + " / idea: " + idea);
            }
        }
        UpdateObjsText();
    }

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

        if (collectingIdeas)
        {
            int totalCount = CountAll();
            if (type < 2)
            {
                //PICK SELECTION
                PickText.text = (totalCount > 0
                    ? ("<color=#00ffff>" + totalCount + "</color> " +( totalCount>1 ? objsStr : objStr )+ " so far!")
                    : "") + "\nTo submit your " + objStr + " type: <color=#00ffff>" + cmdStr + " " + objStr + "</color>";
            }
            else
            {
                //PICK USER
                PickText.text = (totalCount > 0
                    ? ("<color=#00ffff>" + totalCount + "</color> " + (totalCount > 1 ? objsStr : objStr) + " so far!")
                    : "") + "\nTo enter type: <color=#00ffff>" + cmdStr + "</color>";
            }
        }
        else
        {
            int amountReceived = CountAll();
            if (type < 2)
            {
                //PCK SELECTION
                PickText.text = amountReceived > 0
                    ? ("<color=#00ffff>" + amountReceived + "</color> " +
                       (amountReceived > 1 ? objStr : objsStr) + " received! Thanks everyone!")
                    : "";
            }
            else
            {
                //PICK USER!
                PickText.text = amountReceived > 0
                    ? ("<color=#00ffff>" + amountReceived + "</color> " +
                       (amountReceived > 1 ? objStr : objsStr) + " entered! Thanks everyone!")
                    : "";
            }
        }
        UpdateButtons();
    }
    
    public void UpdateButtons()
    {
        int totalObjs = CountAll();
            PickSketchButton.gameObject.SetActive(totalObjs > 0 && !doingVote && !collectingIdeas);
            ResetButton.gameObject.SetActive(totalObjs > 0);
        if (doingVote)
        {
            StartStopText.text = "Stop voting!";
            StartStopButton.colors = ResetButton.colors;
        }
        else
        {
            StartStopText.text = collectingIdeas
                ? "Stop receiving " + objsStr + "!"
                : "Start receiving " + objsStr + "!";
            StartStopButton.colors = collectingIdeas ? ResetButton.colors : PickSketchButton.colors;
        }
            ExportList.gameObject.SetActive(totalObjs > 0 && type == 2);
            
    }

    public void DoExportList()
    {
        string usernamestowrite = "";
        foreach (var _obj in objectsUserDictionary.Keys)
        {
            usernamestowrite = (string.IsNullOrEmpty(usernamestowrite) ? _obj : usernamestowrite + " " + _obj);
        }

        System.IO.File.WriteAllText(Application.dataPath + "/pickUsersList.txt", usernamestowrite);
        OpenFolder.OpenInFileBrowser(Application.dataPath);
    }

    public int CountAll()
    {
        int total = 0;
        foreach (List<string> _objlist in objectsUserDictionary.Values)
        {
            total += _objlist.Count;
        }
        return total;
    }

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

    void StartVoting()
    {
        doingVote = true;
        UpdateButtons();
        if (waitForTime == 0) waitForTime = 60;
        
        optionToIndex.Clear();
        usernamesVotedAlready.Clear();
        int optionIndex = 0;
        foreach (var _userlist in objectsUserDictionary)
        {
            foreach (string _s in _userlist.Value)
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
        string voteOptions =
            "<size=24>Type <color=#00FFFF>#VOTE</color> <color=#D43115>number</color> to vote!</size>\n<size=30>";
        int optionIndex = 0;
        foreach (var _option in optionToIndex)
        {
            voteOptions += "     <color=#D43115>[" + _option.Key + "]</color> <color=#c0c0c0ff><i>" + _option.Value.option +
                           "</i></color>     ";
            optionIndex++;
        }
        voteOptions += "</size>";
        PickText.text = voteOptions;
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
    
    public void CalculateVoteResults()
    {
        doingVote = false;
        StopCoroutine("CountDownToStop");

        VoteOption winnerOption = new VoteOption("","");

        int maxVotes = -1;
        Debug.Log("Checking votes, amount of options = "+optionToIndex.Count);
        foreach (var _option in optionToIndex.Values)
        {
            if (_option.votes > maxVotes)
            {
                maxVotes = _option.votes;
                Debug.Log("Found new winning option "+_option.option+" / vote amount"+_option.votes);
                winnerOption = new VoteOption(_option.username, _option.option);
            }
        }

        PickText.transform.DOKill();
        PickText.transform.localScale = Vector3.one;
        PickText.transform.DOScale(Vector3.one * 1.07f, 0.6f).SetLoops(-1, LoopType.Yoyo);
        Debug.Log("Winner is "+winnerOption.option);
        PickText.text = "<color=#00ffff>" + winnerOption.username + "</color>: " + winnerOption.option;
        UpdateButtons();
    }

    public void SetObjectText(string username, bool forReals = true)
    {
        if (!objectsUserDictionary.ContainsKey(username)) return;
        if (forReals)
        {
            PickText.transform.DOKill();
            PickText.transform.localScale = Vector3.one;
            PickText.transform.DOScale(Vector3.one*1.07f, 0.6f).SetLoops(-1, LoopType.Yoyo);
            int pickedIdeaIndex = Random.Range(0, objectsUserDictionary[username].Count);
            if (type < 2)
            {
                //PICKING SUGGESTION
                PickText.text = "<color=#00ffff>" + username + "</color>: " + objectsUserDictionary[username][pickedIdeaIndex];
            }
            else
            {
                //PICK USER
                PickText.text = "<color=#00ffff>" + username + "</color>";
            }
            if (removeAfterPick)
            {
                if (objectsUserDictionary[username].Count > 1)
                {
                    objectsUserDictionary[username].RemoveAt(pickedIdeaIndex);
                }
                else
                {
                    objectsUserDictionary.Remove(username);
                }
            }
        }
        else
        {
            PickText.transform.DOKill();
            PickText.transform.localScale = Vector3.one*0.75f;
            PickText.transform.DOShakeScale(0.1f, 0.4f);
            if (type == 2)
            {
                PickText.text = "<color=#4d4d4d>"+ username +"</color>";
            }
            else
            {
                PickText.text = "<color=#4d4d4d>"+ username + ": " +
                                objectsUserDictionary[username][Random.Range(0, objectsUserDictionary[username].Count)] +
                                "</color>";
            }
        }
        UpdateButtons();
    }

    IEnumerator DOPickRandom()
    {
        float totalDuration = 15;
        float duration = totalDuration;
        while (duration > 0)
        {
            SetObjectText(objectsUserDictionary.ElementAt(Random.Range(0, objectsUserDictionary.Count)).Key,false);
            duration--;
            yield return new WaitForSeconds((totalDuration-duration)*0.015f);
        }
        
        SetObjectText(objectsUserDictionary.ElementAt(Random.Range(0, objectsUserDictionary.Count)).Key,true);

        yield return null;

        if (objectsUserDictionary.Count > 0)
        {
            PickSketchButton.gameObject.SetActive(true);
        }
    }
    
    void OnChatMsgRecieved(string msg)
    {
        //parse from buffer.
        int msgIndex = msg.IndexOf("PRIVMSG #");
        string msgString = msg.Substring(msgIndex + IRC.channelName.Length + 11);
        msgString = msgString.ToLower();
        string user = msg.Substring(1, msg.IndexOf('!') - 1);
        if (collectingIdeas && (msgString.ToLower().StartsWith(cmdStr)))
        {
            ObjectReceived(user, type<2 ? msgString.Substring(cmdStr.Length) : "");
        }
        if (doingVote && (msgString.ToLower().StartsWith("#vote")))
        {
            int tryParseVote = 0;
            string[] words = msgString.Split(' ');
            if (words.Length > 1)
            {
                bool parsedInt = int.TryParse(words[1], out tryParseVote);
                if (parsedInt)
                {
                    VoteReceived(user, tryParseVote);
                }
            }
        }
        if (doingVote && user.ToLower() == IRC.channelName.ToLower() && (msgString.ToLower().StartsWith("#removevote")))
        {
            int tryParseVote = 0;
            string[] words = msgString.Split(' ');
            if (words.Length > 1)
            {
                bool parsedInt = int.TryParse(words[1], out tryParseVote);
                if (parsedInt)
                {
                    optionToIndex.Remove(tryParseVote);
                    UpdateVotesText();
                    if (optionToIndex.Count == 0)
                    {
                        StopPicking();
                    }
                }
            }
        }
    }
}
