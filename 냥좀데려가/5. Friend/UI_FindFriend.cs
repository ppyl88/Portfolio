using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using FirebaseNetwork;
using System.Collections;

public class UI_FindFriend : MonoBehaviour
{
    [SerializeField] private FriendListScrollView friendListScrollView = null;
    [SerializeField] private UI_FriendListView friendListUI = null;

    [SerializeField] private TMP_InputField inputNickname;
    [SerializeField] private ProfileImgNetwork profileImgNetwork = null;

    private DateTime currentTime;
    private string strDefault = "Friend/";

    private void Start()
    {
        friendListScrollView.Init();
    }

    public void InitSearch()
    {
        SetList(null);
        currentTime = friendListUI.currentTime;
    }

    public void OnClickSearch()
    {
        string key = inputNickname.text;

        List<FriendListData> matchFriends = new List<FriendListData>();
        if (key.Trim() == "")
        {
            UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String[strDefault + "Warning/EnterNickname"].Contents[(int)UI_Setting.language]);
        }
        else
        {
            UIManager.instance.Loading(true);
            matchFriends.Clear();
            StartCoroutine(SearchFriend(key, (result) =>
            {
                foreach(FriendData friend in result)
                {
                    if (!friendListUI.users.ContainsKey(friend.index)) friendListUI.users.Add(friend.index, friend);
                    string[] btns = new string[1] { TableDataManager.Instance.table_String[strDefault + "UI/BtnAdd"].Contents[(int)UI_Setting.language] };
                    Action[] acts = new Action[1] { () => { CheckFriend(friend.index); } };
                    bool[] btnActives = new bool[1] { true };
                    FriendListData friendList = new FriendListData(friend, friend.inform, () => friendListUI.OnClickFriendDetailUI(friend, "친구추가", () => CheckFriend(friend.index), () => { UIManager.instance.OpenPopUpChildControl(0); UIManager.instance.OpenPopUpChildControl(-1); }), btns, acts, btnActives);
                    matchFriends.Add(friendList);
                }
                if (matchFriends.Count == 0)
                {
                    UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String[strDefault + "Warning/NoMatch"].Contents[(int)UI_Setting.language]);
                }
                else
                {
                    ReArrangeFriendList(matchFriends, key);
                    matchFriends = matchFriends.GetRange(0, (matchFriends.Count < TableDataManager.Instance.table_Setting["Friend_Search_Count"].Value) ? matchFriends.Count : TableDataManager.Instance.table_Setting["Friend_Search_Count"].Value);
                    SetList(matchFriends);
                }
                UIManager.instance.Loading(false);
            }));
        }
    }

    private IEnumerator SearchFriend(string nickName, Action<List<FriendData>> _onSuccess)
    {
        List<FriendData> matchFriends = new List<FriendData>();
        int searchCount = -1;
        StartCoroutine(NetworkMethod.CoSearchFriend(nickName, (result) =>
        {
            searchCount = result.Count;
            foreach (UserData _result in result)
            {
                if (_result.profileImgURL == "")
                {
                    AddMatchFriend(matchFriends, _result);
                }
                else
                {
                    StartCoroutine(profileImgNetwork.CoLoadImage(_result.profileImgURL, (profile) => AddMatchFriend(matchFriends, _result, profile)));
                }
            }
        }));
        while(true)
        {
            if(searchCount != -1 && matchFriends.Count == searchCount)
            {
                _onSuccess(matchFriends);
                yield break;
            }
            yield return null;
        }
    }

    private void AddMatchFriend(List<FriendData> matchFriends, UserData friend, Texture2D profileImg = null)
    {
        int catCount = (friend.cats == null) ? 0 : friend.cats.Count;
        int friendCount = (friend.myFriends == null) ? 0 : friend.myFriends.Count;
        matchFriends.Add(new FriendData(friend.index, friend.level, friend.nickName, profileImg, friend.lastConnect, CalculateLastConnect(friend.lastConnect), friend.status, catCount, friend.like, friendCount));
    }

    private string CalculateLastConnect(string lastconnect)
    {
        if (lastconnect == "ing")
        {
            return TableDataManager.Instance.table_String[strDefault + "UI/ConnectIng"].Contents[(int)UI_Setting.language];
        }
        else if (lastconnect == null || lastconnect == "")
        {
            return TableDataManager.Instance.table_String[strDefault + "UI/ConnectNull"].Contents[(int)UI_Setting.language];
        }
        else
        {
            TimeSpan diffTime = currentTime - new DateTime(int.Parse(lastconnect.Substring(0, 4)), int.Parse(lastconnect.Substring(4, 2)), int.Parse(lastconnect.Substring(6, 2)), int.Parse(lastconnect.Substring(8, 2)), int.Parse(lastconnect.Substring(10, 2)), 0);
            int diffMin = (int)diffTime.TotalMinutes;
            int diffHour = diffMin / 60;
            int diffDay = diffHour / 24;
            string str = TableDataManager.Instance.table_String[strDefault + "UI/ConnectLastTime"].Contents[(int)UI_Setting.language];
            if (diffDay > 0)
            {
                return str.Replace("0", diffDay + TableDataManager.Instance.table_String["Common/UI/Day"].Contents[(int)UI_Setting.language]);
            }
            else if (diffHour > 0)
            {
                return str.Replace("0", diffHour + TableDataManager.Instance.table_String["Common/UI/Hour"].Contents[(int)UI_Setting.language]);
            }
            else
            {
                return str.Replace("0", diffMin + TableDataManager.Instance.table_String["Common/UI/Minute"].Contents[(int)UI_Setting.language]);
            }
        }
    }

    // 친구 리스트 재배열 (이름 매칭 & 접속시간 순으로 나열)
    private void ReArrangeFriendList(List<FriendListData> friendLists, string key)
    {
        currentTime = TimeUtils.GetDateTime().AddHours(9);

        for (int i = 0; i < friendLists.Count - 1; i++)
        {
            int tmp = i;
            string connect_tmp = friendLists[tmp].friend.lastConnect;
            string nick_tmp = friendLists[tmp].friend.nickName;
            long lastconnect_tmp = 0;
            if (connect_tmp == "ing")
            {
                lastconnect_tmp = 999999999999;
            }
            else if (connect_tmp.Length > 5)
            {
                lastconnect_tmp = long.Parse(connect_tmp);
            }
            for (int j = i + 1; j < friendLists.Count; j++)
            {
                string connect_j = friendLists[j].friend.lastConnect;
                string nick_j = friendLists[j].friend.nickName;
                long lastconnect_j = 0;

                if (connect_j == "ing")
                {
                    lastconnect_j = 999999999999;
                }
                else if (connect_j.Length > 5)
                {
                    lastconnect_j = long.Parse(connect_j);
                }

                if(nick_tmp != key && nick_j == key)
                {
                    tmp = j;
                    lastconnect_tmp = lastconnect_j;
                    nick_tmp = nick_j;
                }
                else if ((nick_tmp == key && nick_j == key) || (nick_tmp != key && nick_j != key))
                {
                    if (lastconnect_j > lastconnect_tmp)
                    {
                        tmp = j;
                        lastconnect_tmp = lastconnect_j;
                        nick_tmp = nick_j;
                    }
                }
            }
            FriendListData temp = friendLists[i];
            friendLists[i] = friendLists[tmp];
            friendLists[tmp] = temp;
            friendLists[i].friend.inform = CalculateLastConnect(friendLists[i].friend.lastConnect);
            friendLists[i].inform = friendLists[i].friend.inform;
        }
        friendLists[friendLists.Count - 1].friend.inform = CalculateLastConnect(friendLists[friendLists.Count - 1].friend.lastConnect);
        friendLists[friendLists.Count - 1].inform = friendLists[friendLists.Count - 1].friend.inform;
    }


    private void CheckFriend(int index)
    {
        if(friendListUI.CheckAlreadyMyFriend(index))
        {
            UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String[strDefault + "Warning/AlreadyFriend"].Contents[(int)UI_Setting.language]);
        }
        else if (friendListUI.CheckAlreadyReceivedFriend(index))
        {
            UIManager.instance.ui_YesNo.ShowPopUP(TableDataManager.Instance.table_String[strDefault + "YesNo/AlreadyRecieved"].Contents[(int)UI_Setting.language], () => friendListUI.AcceptFriend(index));
        }
        else if (friendListUI.CheckAlreadyRequestFriend(index))
        {
            UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String[strDefault + "Warning/AlreadyRequest"].Contents[(int)UI_Setting.language]);
        }
        else
        {
            friendListUI.OpenAddFriendUIinFind(index);
        }
    }

    public void SetList(List<FriendListData> friendLists)
    {
        friendListScrollView.SetData(friendLists);
    }
}
