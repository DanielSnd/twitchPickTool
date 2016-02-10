using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;

public class alertMessage : MonoBehaviour {
    public Text txt;
    public AlertManager.Type type;
    public Vector3 worldPos;
    public RectTransform mRectTransform;
    public Transform followTransform;
    public RectTransform ownerRectTransform;
    private CanvasGroup cg;
    public float yOffset = 0;
    public bool permanent = false;
    public static Dictionary<Transform,List<alertMessage>> followTransformDictionary = new Dictionary<Transform, List<alertMessage>>();

    // Use this for initialization
    void Awake()
    {
        txt = GetComponentInChildren<Text>();
        mRectTransform = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        Color tempColor = txt.color;
        txt.color = tempColor;
    }

    void OnEnable()
    {
        txt.transform.localPosition = Vector3.zero;
        yOffset = 0;
        //txt.transform.parent.localPosition = Vector3.zero;
    }

    public void SetText(string myText, float disappearIn = 2.5f, float r = 1, float g = 1, float b = 1)
    {
        Debug.Log("Set text ["+myText+"] duration "+disappearIn);
        transform.DOKill();
        txt.color = new Color(r, g, b, 1);
        cg.alpha = 0;
        cg.DOFade(1, 0.3f);
        txt.text = myText;
        txt.transform.parent.DOKill();
        txt.transform.parent.DOShakeScale(0.3f, 0.3f);
        if (disappearIn != 0)
        {
            permanent = false;
            StartCoroutine(Disappear(disappearIn));
        }
        else
        {
            permanent = true;
        }
    }

    public void SetUpWorldFollow(RectTransform _alertMessageCanvasRect, Vector3 _desiredPos)
    {
        ownerRectTransform = _alertMessageCanvasRect;
        followTransform = null;
        worldPos = _desiredPos;
        StartCoroutine(DOFollowWorld());
    }

    public void SetUpWorldFollow(RectTransform _alertMessageCanvasRect, Transform _desiredTransform)
    {
        ownerRectTransform = _alertMessageCanvasRect;
        followTransform = _desiredTransform;
        StartCoroutine(DOFollowTransform());
    }

    public IEnumerator DOFollowWorld()
    {
        while (gameObject.activeInHierarchy)
        {
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(worldPos);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * ownerRectTransform.sizeDelta.x) - (ownerRectTransform.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * ownerRectTransform.sizeDelta.y) - (ownerRectTransform.sizeDelta.y * 0.5f)));

            mRectTransform.anchoredPosition = WorldObject_ScreenPosition;
            yield return null;
        }
        yield return null;
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Disappear(0.3f));
        }
    }

    public IEnumerator DOFollowTransform()
    {
        NudgeOlderMessagesOnTransform();

        while (gameObject.activeInHierarchy && followTransform != null && followTransform.gameObject.activeInHierarchy)
        {
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(followTransform.position+(Vector3.up*0.9f));
            Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * ownerRectTransform.sizeDelta.x) - (ownerRectTransform.sizeDelta.x * 0.5f)),
            (yOffset + (ViewportPosition.y * ownerRectTransform.sizeDelta.y) - (ownerRectTransform.sizeDelta.y * 0.5f)));

            mRectTransform.anchoredPosition = WorldObject_ScreenPosition;
            yield return null;
        }
        yield return null;
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Disappear(0.3f));
        }
    }

    public IEnumerator Disappear(float timeToDisappear)
    {
        yield return new WaitForSeconds(timeToDisappear);
        cg.DOFade(0, 0.1f);
        yield return new WaitForSeconds(0.12f);
        AlertManager.instance.removeFromList(this, type);
        yield return null;
        
        this.Recycle();
    }

    public void NudgeOlderMessagesOnTransform()
    {
        if (!followTransform) return;

        AddTransformToDictionary();

        foreach (alertMessage _message in followTransformDictionary[followTransform])
        {
            _message.yOffset += 24;
        }

        followTransformDictionary[followTransform].Add(this);
    }

    void OnDisable()
    {
        if (followTransform && followTransformDictionary.ContainsKey(followTransform) && followTransformDictionary[followTransform].Contains(this))
            followTransformDictionary[followTransform].Remove(this);

        if (followTransform && followTransformDictionary.ContainsKey(followTransform) && followTransformDictionary[followTransform].Count == 0) followTransformDictionary.Remove(followTransform);
    }

    public void AddTransformToDictionary()
    {
        if (followTransform != null && !followTransformDictionary.ContainsKey(followTransform))
        {
            var list = new List<alertMessage>();
            followTransformDictionary.Add(followTransform, list);
        }
    }

    public static bool hasPermanentMessage(Transform obj)
    {
        if (!obj || !followTransformDictionary.ContainsKey(obj)) return false;

        for (int i = followTransformDictionary[obj].Count; i-- > 0;)
            if (followTransformDictionary[obj][i].permanent)
                return true;

        return false;
    }
}
