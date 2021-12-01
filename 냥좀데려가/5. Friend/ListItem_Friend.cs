using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListItem_Friend : MonoBehaviour, IContentUIControl<FriendListData>
{
    [SerializeField] private Sprite defaultProfileImg = null;
    [SerializeField] private TextMeshProUGUI txtLv = null;
    [SerializeField] private TextMeshProUGUI txtNick = null;
    [SerializeField] private TextMeshProUGUI txtInform = null;
    [SerializeField] private RawImage imgProfile = null;
    [SerializeField] private Button btnProfile = null;
    [SerializeField] private GameObject[] btn = null;

    public FriendListData friendListData;

    public void InitData()
    {
        btnProfile.onClick.RemoveAllListeners();
        btnProfile.onClick.AddListener(() => SoundManager.Instance.PlayClick());
        for (int i = 0; i < btn.Length; i++)
        {
            btn[i].SetActive(false);
            btn[i].GetComponent<Button>().onClick.RemoveAllListeners();
            btn[i].GetComponent<Button>().onClick.AddListener(() => SoundManager.Instance.PlayClick());
        }
    }

    public void SetData(FriendListData friendList)
    {
        friendListData = friendList;
        txtLv.text = friendList.friend.level.ToString();
        txtNick.text = friendList.friend.nickName;
        if (friendList.friend.profileTexture != null)
        {
            imgProfile.material.mainTexture = friendList.friend.profileTexture;
            imgProfile.texture = friendList.friend.profileTexture;
        }
        else
        {
            imgProfile.texture = defaultProfileImg.texture;
        }
        txtInform.text = friendList.inform;
        btnProfile.onClick.AddListener(() => friendList.clickProfile());
        for (int i = 0; i < friendList.btns.Length; i++)
        {
            int _i = i;
            btn[i].GetComponentInChildren<TextMeshProUGUI>().text = friendList.btns[i];
            btn[i].SetActive(true);
            btn[i].GetComponent<Button>().onClick.AddListener(delegate { friendList.acts[_i](); });
            btn[i].GetComponent<Button>().interactable = friendList.btnActives[i];
        }
    }
}