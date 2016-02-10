using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class AlertManager : MonoBehaviour {
    private static AlertManager _instance;
    public alertMessage prefab;
    public RectTransform mCanvasRect;
    public List<alertMessage> messagesMiddle = new List<alertMessage>();
    public List<alertMessage> messagesTop = new List<alertMessage>();
    public List<alertMessage> messagesRandom = new List<alertMessage>();
    public List<alertMessage> messagesWorld = new List<alertMessage>();

    public enum Type
    {
        Middle,
        Top,
        Random,
        TopSmall,
        World,
        Dialogue
    }

    public static AlertManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<AlertManager>();

                if (!_instance)
                {
                    GameObject singleton = (GameObject)Instantiate(Resources.Load("UI/AlertManager"));
                    singleton.name = "AlertManager";
                    _instance = singleton.GetComponent<AlertManager>();
                }
                //Tell unity not to destroy this object when loading a new scene!
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != _instance)
                Destroy(this.gameObject);
        }
        mCanvasRect = GetComponent<RectTransform>();

        prefab.CreatePool();
    }


    public static void Alert(string text, Type type = Type.Top, float duration = 1.4f, float r = 1, float g = 1, float b = 1, float scale=1 )
    {
        alertMessage msg = instance.prefab.Spawn();
        msg.transform.SetParent(instance.transform);
        switch (type)
        {
            case Type.Middle:
                if (instance.messagesMiddle.Count > 0)
                    for (int i = 0; i < instance.messagesMiddle.Count; i++)
                        instance.messagesMiddle[i].transform.DOLocalMoveY(instance.messagesMiddle[i].transform.localPosition.y + 80, 0.2f);
                instance.messagesMiddle.Add(msg);

                msg.transform.localPosition = Vector3.zero;

                msg.transform.localScale = Vector3.one * scale;
                break;
            case Type.Top:
                if (instance.messagesTop.Count > 0)
                    for (int i = 0; i < instance.messagesTop.Count; i++)
                        instance.messagesTop[i].transform.DOLocalMoveY(instance.messagesTop[i].transform.localPosition.y + 60, 0.2f);
                instance.messagesTop.Add(msg);

                Vector3 desiredLocalPosition = Vector3.zero;
                desiredLocalPosition.y = 260;
                msg.transform.localPosition = desiredLocalPosition;
                msg.transform.localScale = Vector3.one * 0.85f * scale;
                break;
            case Type.Random:
                instance.messagesRandom.Add(msg);

                Vector3 desiredRandomLocalPosition = Vector3.zero;
                desiredRandomLocalPosition.y = Random.value < 0.5 ? Random.Range(-300, -140) : Random.Range(140, 300);
                desiredRandomLocalPosition.x = Random.value < 0.5 ? Random.Range(-450, -250) : Random.Range(250, 450);
                msg.transform.localPosition = desiredRandomLocalPosition;
                msg.transform.localScale = Vector3.one * 0.85f * scale;
                break;
            case Type.TopSmall:
                if (instance.messagesTop.Count > 0)
                    for (int i = 0; i < instance.messagesTop.Count; i++)
                        instance.messagesTop[i].transform.DOLocalMoveY(instance.messagesTop[i].transform.localPosition.y + 60, 0.2f);
                instance.messagesTop.Add(msg);

                Vector3 desiredLocalPosition2 = Vector3.zero;
                desiredLocalPosition2.y = 260;
                msg.transform.localPosition = desiredLocalPosition2;
                msg.transform.localScale = Vector3.one * 0.45f * scale;
                break;
        }
        msg.type = type;
        msg.SetText(text, duration, r, g, b);
    }

    public static void AlertWorld(string text,Vector3 worldPos,Color messageColor, float duration = 1.4f, float scale=1)
    {
        alertMessage msg = instance.prefab.Spawn();
        msg.transform.SetParent(instance.transform);

        Vector3 desiredLocalPosition2 = Vector3.zero;
        desiredLocalPosition2.y = 260;

        msg.transform.localScale = Vector3.one * 0.45f * scale;

        msg.SetUpWorldFollow(instance.mCanvasRect,worldPos);

        instance.messagesWorld.Add(msg);
        msg.type = Type.World;
        msg.SetText(text, duration, messageColor.r, messageColor.g, messageColor.b);
    }

    public static void AlertWorld(string text, Transform posFollower, Color messageColor, float duration = 1.4f, float scale = 1)
    {
        alertMessage msg = instance.prefab.Spawn();
        msg.transform.SetParent(instance.transform);

        Vector3 desiredLocalPosition2 = Vector3.zero;
        desiredLocalPosition2.y = 260;
        //        msg.transform.localPosition = desiredLocalPosition2;
        msg.transform.localScale = Vector3.one * 0.3f * scale;

        msg.SetText(text, duration, messageColor.r, messageColor.g, messageColor.b);
        msg.SetUpWorldFollow(instance.mCanvasRect, posFollower);
        instance.messagesWorld.Add(msg);
        msg.type = Type.World;
    }
    
    public static void ClearAll()
    {
        instance.doClearAll();
    }
    public void doClearAll()
    {
        foreach (var _alertMessage in messagesMiddle)
        {
            _alertMessage.Recycle();
        }
        foreach (alertMessage _alertMessage in messagesTop)
        {
            _alertMessage.Recycle();
        }
        foreach (alertMessage _alertMessage in messagesWorld)
        {
            _alertMessage.Recycle();
        }
        messagesWorld.Clear();
        messagesMiddle.Clear();
        messagesTop.Clear();
    }

    public void removeFromList(alertMessage theMessage, Type type)
    {
        switch (type)
        {
            case Type.Middle:
                messagesMiddle.Remove(theMessage);
                break;
            case Type.Top:
                messagesTop.Remove(theMessage);
                break;
            case Type.TopSmall:
                messagesTop.Remove(theMessage);
                break;
            case Type.Random:
                messagesRandom.Remove(theMessage);
                break;
        }
    }
    
}
