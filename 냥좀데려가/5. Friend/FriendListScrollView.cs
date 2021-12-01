public class FriendListScrollView : ScrollViewControl_Vertical<ListItem_Friend, FriendListData>
{
    public void ResetListData(FriendListData friendList)
    {
        int idx = transformContents.FindIndex(data => data.GetComponent<ListItem_Friend>().friendListData == friendList);
        transformContents[idx].GetComponent<ListItem_Friend>().SetData(friendList);
    }
}
