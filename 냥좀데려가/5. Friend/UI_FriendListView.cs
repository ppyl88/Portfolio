using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using FirebaseNetwork;

public class UI_FriendListView : MonoBehaviour
{
    [SerializeField] private CanvasGroup emptyFriend = null;
    [SerializeField] private ProfileImgNetwork profileImgNetwork = null;
    [SerializeField] private FriendListScrollView friendListScrollView = null;
    [SerializeField] private UI_FriendDetail friendDetailUI = null;
    [SerializeField] private UI_CheckFriend addFriendUI = null;
    [SerializeField] private UI_CheckFriend delFriendUI = null;

    [SerializeField] private CanvasGroup[] panelCategorys = null;
    [SerializeField] private TextMeshProUGUI txtReceivedNum = null;

    [SerializeField] private VisitManager managerVisit = null;


    public DateTime currentTime;
    public Dictionary<int, FriendData> users = new Dictionary<int, FriendData>();
    
    private int curCategory = 0;
    private bool resetEnd = false;
    private string strDefault = "Friend/";

    private List<FriendListData> myFriendLists = new List<FriendListData>();
    private List<FriendListData> recommendFriendLists = new List<FriendListData>();
    private List<FriendListData> receivedFriendLists = new List<FriendListData>();


    #region Basic Setting
    private void Start()
    {
        MissionManager.Instance.MissionCountUpdate(MissionBehavior.KeepFriend);
        emptyFriend.alpha = 0;
        friendListScrollView.Init();
    }

    public void SetFriendUI()
    {
        resetEnd = false;
        users.Clear();
        UIManager.instance.Loading(true);
        currentTime = TimeUtils.GetDateTime().AddHours(9);
        CheckFriendDataTime();
        StartCoroutine(GetCurrentUserData(() => {
            UIManager.instance.Loading(false);
            MakeFriendLists();
            txtReceivedNum.text = Authentication.Inst.userData.receivedFriends.Count.ToString();
            OnClickFriendList();
        }));

    }

    private void MakeFriendLists()
    {
        myFriendLists.Clear();
        recommendFriendLists.Clear();
        receivedFriendLists.Clear();

        for (int i=0; i< Authentication.Inst.userData.myFriends.Count; i++)
        {
            AddMyFriendList(Authentication.Inst.userData.myFriends[i]);
        }
        for (int i = 0; i < Authentication.Inst.userData.recommendFriends.Count; i++)
        {
            AddRecommendFriendList(Authentication.Inst.userData.recommendFriends[i]);
        }
        for (int i = 0; i < Authentication.Inst.userData.receivedFriends.Count; i++)
        {
            AddReceivedFriendList(Authentication.Inst.userData.receivedFriends[i].index, Authentication.Inst.userData.receivedFriends[i].message);
        }
    }

    private void CheckFriendDataTime()
    {
        string curDate = currentTime.Year.ToString("D4") + currentTime.Month.ToString("D2") + currentTime.Day.ToString("D2");
        if (curDate != Authentication.Inst.userData.lastRecommendDate)
        {
            Authentication.Inst.userData.lastRecommendDate = curDate;
            NetworkMethod.SetLastRecommendDate();
            ResetFriendsData();
            ResetRecommendFriend();
        }
        else
        {
            resetEnd = true;
        }
    }
    #endregion

    #region Category Select
    public void OnClickFriendList()
    {
        emptyFriend.alpha = 0;
        if (Authentication.Inst.userData.myFriends.Count == 0)
        {
            emptyFriend.alpha = 1;
        }
        else
        {
            ReArrangeFriendList(myFriendLists);
        }
        curCategory = 0;
        BtnPanelOn(curCategory);
        friendListScrollView.SetData(myFriendLists);
    }

    public void OnClickRecommendFriendsList()
    {
        emptyFriend.alpha = 0;
        if (Authentication.Inst.userData.recommendFriends.Count == 0)
        {
            emptyFriend.alpha = 1;
        }
        else
        {
            ReArrangeFriendList(recommendFriendLists);
        }
        curCategory = 1;
        BtnPanelOn(curCategory);
        friendListScrollView.SetData(recommendFriendLists);
    }

    public void OnClickReceivedFriendsList()
    {
        emptyFriend.alpha = 0;
        if (Authentication.Inst.userData.receivedFriends.Count == 0)
        {
            emptyFriend.alpha = 1;
        }
        curCategory = 2;
        BtnPanelOn(curCategory);
        friendListScrollView.SetData(receivedFriendLists);
    }

    private void BtnPanelOn(int index)
    {
        for (int i = 0; i < panelCategorys.Length; i++)
        {
            panelCategorys[i].alpha = 0;
            panelCategorys[i].blocksRaycasts = false;
        }
        panelCategorys[index].alpha = 1;
        panelCategorys[index].blocksRaycasts = true;
    }
    #endregion

    #region Button Event
    public void OnClickFindFriendUI()
    {
        UIManager.instance.OpenPopUpChildControl(-1);
    }

    public void OnClickFriendDetailUI(FriendData friend, string str_btn, Action act_btn, Action act_close)
    {
        friendDetailUI.SetData(friend, str_btn, act_btn, act_close);
        UIManager.instance.OpenPopUpChildControl(-2);
    }

    private void VisitFriend(int index)
    {
        managerVisit.VisitSetting(index, users[index].profileTexture);
    }

    public void OpenAddFriendUIinFind(int index)
    {
        if(Authentication.Inst.userData.myFriends.Count >= TableDataManager.Instance.table_Setting["Friend_MAX_Count"].Value)
        {
            UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String[strDefault + "Warning/TooManyFriend"].Contents[(int)UI_Setting.language], () => RemoveMyFriend(index));
        }
        else
        {
            addFriendUI.SetUI(users[index], () => { RequestFriend(index, addFriendUI.GetRequestMsg()); UIManager.instance.OpenPopUpChildControl(0); UIManager.instance.OpenPopUpChildControl(-1); }, () => { UIManager.instance.OpenPopUpChildControl(0); UIManager.instance.OpenPopUpChildControl(-1); });
            UIManager.instance.OpenPopUpChildControl(-3);
        }
    }

    private void OpenAddFriendUI(int index)
    {
        if (Authentication.Inst.userData.myFriends.Count >= TableDataManager.Instance.table_Setting["Friend_MAX_Count"].Value)
        {
            UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String[strDefault + "Warning/TooManyFriend"].Contents[(int)UI_Setting.language], () => RemoveMyFriend(index));
        }
        else
        {
            addFriendUI.SetUI(users[index], () => { RequestFriend(index, addFriendUI.GetRequestMsg()); UIManager.instance.OpenPopUpChildControl(0); }, () => { UIManager.instance.OpenPopUpChildControl(0); });
            UIManager.instance.OpenPopUpChildControl(-3);
        }
    }

    private void OpenDelFriendUI(int index)
    {
        delFriendUI.SetUI(users[index], () => { RemoveMyFriend(index); UIManager.instance.OpenPopUpChildControl(0); }, () => { UIManager.instance.OpenPopUpChildControl(0); });
        UIManager.instance.OpenPopUpChildControl(-4);
    }

    public void OnClickLikeVisitFriend()
    {
        LikeFriend(managerVisit.friendIndex);
    }

    private void LikeFriend(int index)
    {
        int myIndex = Authentication.Inst.userIndex;
        MyFriendData myFriend = Authentication.Inst.userData.myFriends.Find(friend => friend.index == index);
        myFriend.like = true;
        // 내 서버 친구 좋아요 데이터 수정
        StartCoroutine(NetworkMethod.CoGetMyFriendData(myIndex, result =>
        {
            int idx = -1;
            if (result != null)
            {
                idx = result.FindIndex(data => data.index == index);
            }
            if (idx != -1)
            {
                // 친구 서버 좋아요 데이터 수정
                StartCoroutine(NetworkMethod.CoGetUserLikeCount(index, _likeCount =>
                {
                    NetworkMethod.SetFriendLike(idx, myFriend.like);
                    int likeCount = _likeCount;
                    likeCount++;
                    NetworkMethod.SetUserLike(index, likeCount);
                    users[index].cnt_like = likeCount;
                    MissionManager.Instance.MissionCountUp(MissionBehavior.LikeFriend);
                }
                ));
            }
            else
            {
                UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String[strDefault + "Warning/NotFriend"].Contents[(int)UI_Setting.language], () => RemoveMyFriend(index));
            }
        }));
        bool[] btnActives = new bool[2] { true, !myFriend.like };
        FriendListData friendList = myFriendLists.Find(data => data.friend.index == index);
        friendList.btnActives = btnActives;
        friendListScrollView.ResetListData(friendList);
    }

    public void AcceptFriend(int index)
    {
        AddMyFriend(index);
    }

    private void RejectFriend(int index)
    {
        RemoveReceivedFriend(index);
    }
    #endregion

    #region FriendData Control
    bool alreadymyFriend = false;
    bool alreadyfriendFriend = false;
    int mycount = -1;
    int friendcount = -1;
    private void AddMyFriend(int index)
    {
        mycount = -1;
        friendcount = -1;
        UIManager.instance.Loading(true);
        int myIndex = Authentication.Inst.userIndex;
        // 내 서버에 데이터 가져오기
        StartCoroutine(NetworkMethod.CoGetMyFriendData(myIndex, result =>
        {
            if (result != null)
            {
                mycount = result.Count;
                alreadymyFriend = result.Exists(data => data.index == index);
            }
            else
            {
                mycount = 0;
            }
        }));
        // 상대 서버 데이터 가져오기
        StartCoroutine(NetworkMethod.CoGetMyFriendData(index, result =>
        {
            if (result != null)
            {
                friendcount = result.Count;
                alreadyfriendFriend = result.Exists(data => data.index == myIndex);
            }
            else
            {
                friendcount = 0;
            }
        }));

        StartCoroutine(WaitForConnectingNetwork(index));
    }

    private IEnumerator WaitForConnectingNetwork(int index)
    {
        int myIndex = Authentication.Inst.userIndex;
        MyFriendData myFriend = new MyFriendData(index);
        int mycount_new = 0;
        bool addTry = false;
        bool countTry = false;
        bool countMyFriend = false;
        while (true)
        {
            if (countMyFriend)
            {
                if (mycount_new > mycount)
                {
                    UIManager.instance.Loading(false);
                    yield break;
                }
                else
                {
                    countMyFriend = false;
                    countTry = false;
                }
            }
            else if (countTry) { }
            else if (addTry)
            {
                // 내 서버에 데이터 가져오기
                StartCoroutine(NetworkMethod.CoGetMyFriendData(myIndex, result =>
                {
                    if (result != null)
                    {
                        mycount_new = result.Count;
                    }
                    else
                    {
                        mycount_new = 0;
                    }
                    countMyFriend = true;
                }));
                countTry = true;
            }
            else if (mycount >=0 && friendcount >= 0)
            {
                if (mycount >= TableDataManager.Instance.table_Setting["Friend_MAX_Count"].Value)
                {
                    string str = TableDataManager.Instance.table_String[strDefault + "YesNo/MyFriendMax"].Contents[(int)UI_Setting.language];
                    UIManager.instance.ui_YesNo.ShowPopUP(str.Replace("0", TableDataManager.Instance.table_Setting["Friend_MAX_Count"].Value.ToString()), () => RemoveReceivedFriend(index));
                    yield break;
                }
                else if (friendcount >= TableDataManager.Instance.table_Setting["Friend_MAX_Count"].Value)
                {
                    string str = TableDataManager.Instance.table_String[strDefault + "YesNo/FriendFriendMax"].Contents[(int)UI_Setting.language];
                    UIManager.instance.ui_YesNo.ShowPopUP(str.Replace("0", TableDataManager.Instance.table_Setting["Friend_MAX_Count"].Value.ToString()), () => RemoveReceivedFriend(index));
                    yield break;
                }
                else
                {
                    addTry = true;
                    Authentication.Inst.userData.myFriends.Add(myFriend);
                    AddMyFriendList(myFriend);
                    RemoveReceivedFriend(index);
                    if (Authentication.Inst.userData.recommendFriends.Contains(index))
                    {
                        bool[] btnActives = new bool[1] { false };
                        FriendListData friendList = recommendFriendLists.Find(data => data.friend.index == index);
                        friendList.btnActives = btnActives;
                        if (curCategory == 1) friendListScrollView.ResetListData(friendList);
                    }
                    if (alreadymyFriend && alreadyfriendFriend)
                    {
                        yield break;
                    }
                    else
                    {
                        if (!alreadyfriendFriend)
                        {
                            NetworkMethod.AddMyFriend(index, new MyFriendData(myIndex), friendcount);
                        }
                        if (!alreadymyFriend)
                        {
                            //StartCoroutine(NetworkMethod.CoGetDataInfo(myFriend.index, (result) => GetFriendData(result)));
                            NetworkMethod.AddMyFriend(myIndex, myFriend, mycount);
                        }
                        else
                        {
                            yield break;
                        }
                    }
                }
            }
            yield return null; ;
        }
    }

    private void RemoveMyFriend(int index)
    {
        int myIndex = Authentication.Inst.userIndex;
        RemoveMyFriendList(index);
        MyFriendData myFriend = Authentication.Inst.userData.myFriends.Find(friend => friend.index == index);
        Authentication.Inst.userData.myFriends.Remove(myFriend);
        // 내 서버 친구 제거
        StartCoroutine(NetworkMethod.CoGetMyFriendData(myIndex, result =>
        {
            int idx = -1;
            if (result != null)
            {
                idx = result.FindIndex(data => data.index == index);
            }
            if (idx != -1)
            {
                NetworkMethod.DelMyFriend(myIndex, idx, result);
            }
        }));
        // 상대 서버 친구(나) 제거
        StartCoroutine(NetworkMethod.CoGetMyFriendData(index, result =>
        {
            int idx = -1;
            if (result != null)
            {
                idx = result.FindIndex(data => data.index == myIndex);
            }
            if (idx != -1)
            {
                NetworkMethod.DelMyFriend(index, idx, result);
            }
        }));
    }

    private void ResetFriendsData()
    {
        for(int i=0; i<Authentication.Inst.userData.myFriends.Count; i++)
        {
            Authentication.Inst.userData.myFriends[i].like = false;
            Authentication.Inst.userData.myFriends[i].gift = false;
            NetworkMethod.SetFriendLike(i, Authentication.Inst.userData.myFriends[i].like);
            NetworkMethod.SetFriendGift(i, Authentication.Inst.userData.myFriends[i].gift);
        }
    }

    private void ResetRecommendFriend()
    {
        Authentication.Inst.userData.recommendFriends.Clear();
        int before = -10;
        string date = currentTime.AddDays(before).Year.ToString("D4") + currentTime.AddDays(before).Month.ToString("D2") + currentTime.AddDays(before).Day.ToString("D2") + "0000";
        StartCoroutine(NetworkMethod.CoSearchConnectDate(date, (result) =>
        {
            List<int> recommendable = new List<int>();
            foreach (UserData recommendFriend in result)
            {
                int index = recommendFriend.index;
                if (index != Authentication.Inst.userData.index && !CheckAlreadyMyFriend(index) && !CheckAlreadyReceivedFriend(index) && !CheckAlreadyRequestFriend(index))
                {
                    recommendable.Add(index);
                }
            }
            if (recommendable.Count <= TableDataManager.Instance.table_Setting["Friend_Recommend_Count"].Value)
            {
                Authentication.Inst.userData.recommendFriends = recommendable;
            }
            else
            {
                while (Authentication.Inst.userData.recommendFriends.Count < TableDataManager.Instance.table_Setting["Friend_Recommend_Count"].Value)
                {
                    int rnd = UnityEngine.Random.Range(0, recommendable.Count);
                    Authentication.Inst.userData.recommendFriends.Add(recommendable[rnd]);
                    recommendable.RemoveAt(rnd);
                }
            }
            // 내 서버 추천 친구 리스트 리셋
            NetworkMethod.SetRecommendFriends(Authentication.Inst.userData.recommendFriends);
            resetEnd = true;
        }
        ));
    }

    private void RequestFriend(int index, string msg)
    {
        int myIndex = Authentication.Inst.userIndex;
        Authentication.Inst.userData.requestFriends.Add(index);
        // 내 서버 보낸 요청 친구 추가
        StartCoroutine(NetworkMethod.CoGetRequestFriendData(myIndex, result =>
        {
            int count = 0;
            if (result != null) count = result.Count;
            NetworkMethod.AddRequestFriend(index, count);
        }));
        // 상대 서버 받은 요청 친구 추가 
        StartCoroutine(NetworkMethod.CoGetReceivedFriendData(index, result =>
        {
            int count = 0;
            if (result != null) count = result.Count;
            NetworkMethod.AddReceivedFriend(index, new FriendRequestData(myIndex,msg), count);
        }));

        if (Authentication.Inst.userData.recommendFriends.Contains(index))
        {
            bool[] btnActives = new bool[1] { false };
            FriendListData friendList = recommendFriendLists.Find(data => data.friend.index == index);
            friendList.btnActives = btnActives;
            if (curCategory == 1) friendListScrollView.ResetListData(friendList);
        }
    }

    private void RemoveReceivedFriend(int index)
    {
        int myIndex = Authentication.Inst.userIndex;
        RemoveReceivedFriendList(index);
        FriendRequestData friendRequest = Authentication.Inst.userData.receivedFriends.Find(friend => friend.index == index);
        Authentication.Inst.userData.receivedFriends.Remove(friendRequest);
        txtReceivedNum.text = Authentication.Inst.userData.receivedFriends.Count.ToString();
        // 내 서버 받은 요청 친구 제거
        StartCoroutine(NetworkMethod.CoGetReceivedFriendData(myIndex, result =>
        {
            int idx = -1;
            if (result != null)
            {
                idx = result.FindIndex(data => data.index == index);
            }
            if (idx != -1)
            {
                NetworkMethod.DelReceivedFriend(idx, result);
            }
        }));
        // 상대 서버 보낸 요청 친구 제거
        StartCoroutine(NetworkMethod.CoGetRequestFriendData(index, result =>
        {
            int idx = -1;
            if (result != null)
            {
                idx = result.FindIndex(data => data == myIndex);
            }
            if (idx != -1)
            {
                NetworkMethod.DelRequestFriend(index, idx, result);
            }
        }));
    }
    #endregion

    #region FriendList Control
    private void AddMyFriendList(MyFriendData myFriend)
    {
        string[] btns = new string[2] { TableDataManager.Instance.table_String[strDefault + "UI/BtnVisit"].Contents[(int)UI_Setting.language], TableDataManager.Instance.table_String[strDefault + "UI/BtnLike"].Contents[(int)UI_Setting.language] };
        Action[] acts = new Action[2] { () => { VisitFriend(myFriend.index); }, () => { LikeFriend(myFriend.index); } };
        bool[] btnActives = new bool[2] { true, !myFriend.like };
        FriendListData friendList = new FriendListData(users[myFriend.index], users[myFriend.index].inform, () => OnClickFriendDetailUI(users[myFriend.index], "친구삭제", () => OpenDelFriendUI(myFriend.index), () => { UIManager.instance.OpenPopUpChildControl(0); }), btns, acts, btnActives);
        myFriendLists.Add(friendList);
        MissionManager.Instance.MissionCountUpdate(MissionBehavior.KeepFriend);
    }

    private void RemoveMyFriendList(int index)
    {
        FriendListData friendList = myFriendLists.Find(list => list.friend.index == index);
        myFriendLists.Remove(friendList);
        MissionManager.Instance.MissionCountUpdate(MissionBehavior.KeepFriend);
        if (curCategory==0) OnClickFriendList();
    }

    private string CalculateLastConnect(string lastconnect)
    {
        if(lastconnect == "ing")
        {
            return TableDataManager.Instance.table_String[strDefault + "UI/ConnectIng"].Contents[(int)UI_Setting.language];
        }
        else if(lastconnect == null || lastconnect == "")
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

    // 친구 리스트 재배열 (접속시간 순으로 나열)
    private void ReArrangeFriendList(List<FriendListData> friendLists)
    {
        currentTime = TimeUtils.GetDateTime().AddHours(9);

        for (int i = 0; i < friendLists.Count - 1; i++)
        {
            int tmp = i;
            for (int j = i + 1; j < friendLists.Count; j++)
            {
                FriendData friend_tmp = friendLists[tmp].friend;
                FriendData friend_j = friendLists[j].friend;
                long lastconnect_tmp = 0;
                long lastconnect_j = 0;
                if (friend_tmp.lastConnect == "ing")
                {
                    lastconnect_tmp = 999999999999;
                }
                else if (friend_tmp.lastConnect.Length > 5)
                {
                    lastconnect_tmp = long.Parse(friend_tmp.lastConnect);
                }
                if (friend_j.lastConnect == "ing")
                {
                    lastconnect_j = 999999999999;
                }
                else if (friend_j.lastConnect.Length > 5)
                {
                    lastconnect_j = long.Parse(friend_j.lastConnect);
                }
                if (lastconnect_j > lastconnect_tmp)
                {
                    tmp = j;
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

    private void AddRecommendFriendList(int index)
    {
        string[] btns = new string[1] { TableDataManager.Instance.table_String[strDefault + "UI/BtnAdd"].Contents[(int)UI_Setting.language] };
        Action[] acts = new Action[1] { () => {
            if (CheckAlreadyReceivedFriend(index))
            {
                UIManager.instance.ui_YesNo.ShowPopUP(TableDataManager.Instance.table_String[strDefault + "YesNo/AlreadyRecieved"].Contents[(int)UI_Setting.language], () => AcceptFriend(index));
            }
            else
            {
                OpenAddFriendUI(index);
            }
        } };
        bool[] btnActives = new bool[1] { !CheckAlreadyMyFriend(index) && !CheckAlreadyRequestFriend(index) };
        FriendData friend = users[index];
        FriendListData friendList = new FriendListData(friend, friend.inform, () => OnClickFriendDetailUI(friend, "친구추가", () => OpenAddFriendUI(friend.index), () => { UIManager.instance.OpenPopUpChildControl(0); }), btns, acts, btnActives);
        recommendFriendLists.Add(friendList);
    }

    private void AddReceivedFriendList(int index, string message)
    {
        string[] btns = new string[2] { TableDataManager.Instance.table_String[strDefault + "UI/BtnAccept"].Contents[(int)UI_Setting.language], TableDataManager.Instance.table_String[strDefault + "UI/BtnReject"].Contents[(int)UI_Setting.language] };
        Action[] acts = new Action[2] { () => { AcceptFriend(index); }, () => { RejectFriend(index); } };
        bool[] btnActives = new bool[2] { true, true };
        FriendListData friendList = new FriendListData(users[index], message, () => OnClickFriendDetailUI(users[index], "친구수락", () => AcceptFriend(index), () => { UIManager.instance.OpenPopUpChildControl(0); }), btns, acts, btnActives);
        receivedFriendLists.Add(friendList);
    }

    private void RemoveReceivedFriendList(int index)
    {
        FriendListData friendList = receivedFriendLists.Find(list => list.friend.index == index);
        receivedFriendLists.Remove(friendList);
        if (curCategory == 2) OnClickReceivedFriendsList();
    }
    #endregion

    #region CheckData
    public bool CheckAlreadyMyFriend(int index)
    {
        if(Authentication.Inst.userData.myFriends.Exists(friend => friend.index == index))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckAlreadyRequestFriend(int index)
    {
        if(Authentication.Inst.userData.requestFriends.Contains(index))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CheckAlreadyReceivedFriend(int index)
    {
        if (Authentication.Inst.userData.receivedFriends.Exists(friend => friend.index == index))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region UserData Making
    private IEnumerator GetCurrentUserData(Action _onEnd)
    {
        while(true)
        {
            if(resetEnd)
            {
                StartCoroutine(NetworkMethod.CoGetDataInfo(Authentication.Inst.userData.index, (result) => StartCoroutine(UpdateUserData(result, _onEnd))));
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator UpdateUserData(UserData user, Action _onEnd)
    {
        List<int> friendUpdated = new List<int>();
        List<int> recommendUpdated = new List<int>();
        List<int> receivedUpdated = new List<int>();
        Authentication.Inst.userData.myFriends = user.myFriends;
        Authentication.Inst.userData.requestFriends = user.requestFriends;
        Authentication.Inst.userData.receivedFriends = user.receivedFriends;
        user.recommendFriends = Authentication.Inst.userData.recommendFriends;
        for (int i = 0; i < user.myFriends.Count; i++)
        {
            friendUpdated.Add(Authentication.Inst.userData.myFriends[i].index);
            StartCoroutine(NetworkMethod.CoGetDataInfo(user.myFriends[i].index, (result) => AddFriendData(result)));
        }
        for (int i = 0; i < user.recommendFriends.Count; i++)
        {
            recommendUpdated.Add(Authentication.Inst.userData.recommendFriends[i]);
            StartCoroutine(NetworkMethod.CoGetDataInfo(user.recommendFriends[i], (result) => AddFriendData(result)));
        }
        for (int i = 0; i < user.receivedFriends.Count; i++)
        {
            receivedUpdated.Add(Authentication.Inst.userData.receivedFriends[i].index);
            StartCoroutine(NetworkMethod.CoGetDataInfo(user.receivedFriends[i].index, (result) => AddFriendData(result)));
        }
        MissionManager.Instance.MissionCountUpdate(MissionBehavior.KeepFriend);

        while(true)
        {
            if(friendUpdated.Count == 0 && recommendUpdated.Count == 0 && receivedUpdated.Count == 0)
            {
                _onEnd?.Invoke();
                yield break;
            }
            else
            {
                for(int i=0; i<friendUpdated.Count; i++)
                {
                    if (users.ContainsKey(friendUpdated[i])) friendUpdated.RemoveAt(i--);
                }
                for(int i=0; i<recommendUpdated.Count; i++)
                {
                    if (users.ContainsKey(recommendUpdated[i])) recommendUpdated.RemoveAt(i--);
                }
                for (int i=0; i<receivedUpdated.Count; i++)
                {
                    if (users.ContainsKey(receivedUpdated[i])) receivedUpdated.RemoveAt(i--);
                }
            }
            yield return null;
        }
    }

    private void AddFriendData(UserData user)
    {
        if (user.profileImgURL == "")
        {
            AddUserList(user, null);
        }
        else
        {
            StartCoroutine(profileImgNetwork.CoLoadImage(user.profileImgURL, (result) => AddUserList(user, result)));
        }
    }
    
    private void AddUserList(UserData user, Texture2D profile)
    {
        if (!users.ContainsKey(user.index))
        {
            int catCount = (user.cats == null) ? 0 : user.cats.Count;
            int friendCount = (user.myFriends == null) ? 0 : user.myFriends.Count;
            FriendData friend = new FriendData(user.index, user.level, user.nickName, profile, user.lastConnect, CalculateLastConnect(user.lastConnect), user.status, catCount, user.like, friendCount);
            users.Add(user.index, friend);
        }
    }
    #endregion
}
