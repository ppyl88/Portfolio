using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_FriendDetail : MonoBehaviour
{
    [SerializeField] private Sprite defaultProfileImg = null;
    [SerializeField] private RawImage imgProfile = null;
    [SerializeField] private TextMeshProUGUI txtLv = null;
    [SerializeField] private TextMeshProUGUI txtNick = null;
    [SerializeField] private TextMeshProUGUI txtStatus = null;
    [SerializeField] private TextMeshProUGUI txtInform = null;

    [SerializeField] private TextMeshProUGUI txtCat = null;
    [SerializeField] private TextMeshProUGUI txtLike = null;
    [SerializeField] private TextMeshProUGUI txtFriend = null;

    [SerializeField] private GameObject btn = null;
    [SerializeField] private Button btn_close = null;

    public FriendListData friendListData;

    public void SetData(FriendData friend, string str_btn, Action act_btn, Action act_close)
    {
        txtLv.text = friend.level.ToString();
        txtNick.text = friend.nickName;
        if (friend.profileTexture != null)
        {
            imgProfile.material.mainTexture = friend.profileTexture;
            imgProfile.texture = friend.profileTexture;
        }
        else
        {
            imgProfile.texture = defaultProfileImg.texture;
        }
        txtStatus.text = friend.msgStatus;
        txtInform.text = friend.inform;

        txtCat.text = friend.cnt_cat.ToString();
        txtLike.text = friend.cnt_like.ToString();
        txtFriend.text = friend.cnt_friend.ToString();

        btn.GetComponentInChildren<TextMeshProUGUI>().text = str_btn;
        btn.GetComponent<Button>().onClick.RemoveAllListeners();
        btn.GetComponent<Button>().onClick.AddListener(() => act_btn());
        btn_close.onClick.RemoveAllListeners();
        btn_close.onClick.AddListener(() => act_close());
    }

}
