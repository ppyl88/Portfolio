using System;
using UnityEngine;

public class FriendData
{
    public int index;                       // 인덱스
    public int level;                       // 레벨
    public string nickName;                 // 닉네임
    public Texture2D profileTexture;        // 프로필 텍스쳐
    public string lastConnect;              // 최종 접속 시간
    public string inform;                   // 최종 접속 시간 (얼마 지났나 표시)
    public string msgStatus;                // 상태 메세지
    public int cnt_cat;                     // 보유 고양이수
    public int cnt_like;                    // 좋아요 수
    public int cnt_friend;                  // 보유 친구수

    public FriendData(int index, int level, string nickName, Texture2D profileTexture, string lastConnect, string inform, string msgStatus, int cnt_cat, int cnt_like, int cnt_friend)
    {
        this.index = index;
        this.level = level;
        this.nickName = nickName;
        this.profileTexture = profileTexture;
        this.lastConnect = lastConnect;
        this.inform = inform;
        this.msgStatus = msgStatus;
        this.cnt_cat = cnt_cat;
        this.cnt_like = cnt_like;
        this.cnt_friend = cnt_friend;
    }
}
