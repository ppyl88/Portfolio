using System;

public class FriendListData
{
    public FriendData friend;               // 친구 인덱스
    public string inform;                   // UI에 보여줄 내용
    public Action clickProfile;             // 프로필 클릭 액션
    public string[] btns;                   // 버튼 이름
    public Action[] acts;                   // 버튼 이벤트
    public bool[] btnActives;               // 버튼 활성화 여부

    public FriendListData(FriendData friend, string inform, Action clickProfile, string[] btns, Action[] acts, bool[] btnActives)
    {
        this.friend = friend;
        this.inform = inform;
        this.clickProfile = clickProfile;
        this.btns = btns;
        this.acts = acts;
        this.btnActives = btnActives;
    }
}
