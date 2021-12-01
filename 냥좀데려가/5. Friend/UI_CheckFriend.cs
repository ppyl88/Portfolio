using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_CheckFriend : MonoBehaviour
{
    [SerializeField] private Sprite defaultProfileImg = null;
    [SerializeField] private TextMeshProUGUI txtLv = null;
    [SerializeField] private TextMeshProUGUI txtNick = null;
    [SerializeField] private TextMeshProUGUI txtInform = null;
    [SerializeField] private RawImage imgProfile = null;
    [SerializeField] private TMP_InputField inputMsg = null;
    [SerializeField] private Button btnYes = null;
    [SerializeField] private Button btnNo = null;

    public void SetUI(FriendData friend, Action actYes, Action actNo)
    {
        txtLv.text = friend.level.ToString();
        txtNick.text = friend.nickName;
        txtInform.text = friend.msgStatus;
        if (friend.profileTexture != null)
        {
            imgProfile.material.mainTexture = friend.profileTexture;
            imgProfile.texture = friend.profileTexture;
        }
        else
        {
            imgProfile.texture = defaultProfileImg.texture;
        }
        btnYes.onClick.RemoveAllListeners();
        btnYes.onClick.AddListener(() => actYes());
        btnNo.onClick.RemoveAllListeners();
        btnNo.onClick.AddListener(() => actNo());
    }

    public string GetRequestMsg()
    {
        return inputMsg.text;
    }
}
