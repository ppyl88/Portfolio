using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoSingleton<DataManager>
{
    #region MemberData Management
    private string token;
    public UserData userData = new UserData();

    public void SetToken(string _token)
    {
        token = _token;
    }

    public void UpdateMemberData(UserData newUser, Action _Success)
    {
        Action sucess = _Success;
        sucess += new Action(() => userData = newUser);
        StartCoroutine(DBManager.CoUpdateMember(token, newUser, sucess, (key, code) =>
        {
            string errorcode = (code < 10) ? "M0" + code : "M" + code;
            if (key == null) UIPopupManager.Inst.ShowError(errorcode);
            else UIPopupManager.Inst.Show(key);
        }));
    }

    public void UpdateMemberPassowrd(string first_password, string second_password, Action _Success)
    {
        StartCoroutine(DBManager.CoChangePW(token, first_password, second_password, userData, _Success, (key, code) =>
        {
            string errorcode = (code < 10) ? "M0" + code : "M" + code;
            if (key == null) UIPopupManager.Inst.ShowError(errorcode);
            else UIPopupManager.Inst.Show(key);
        }));
    }
    #endregion

    #region For Language Management
    //Language
    public enum LanguageType
    {
        Korean,
        English,
        Japanese,
        Taiwanese
    }
    public LanguageType languageType;

    public Dictionary<string, Dictionary<string, object>> LanguagePackage = new Dictionary<string, Dictionary<string, object>>();
    public Dictionary<string, LocalizeText> localizeTexts = new Dictionary<string, LocalizeText>();


    protected override void Init()
    {
        languageType = (LanguageType)PlayerPrefs.GetInt("LanguageType", 0);
        LanguagePackage = TSVReader.SetData("TSV/" + languageType.ToString());
    }

    public void ChanageLang(int _key)
    {
        PlayerPrefs.SetInt("LanguageType", _key);
        languageType = (LanguageType)_key;
        LanguagePackage = null;
        LanguagePackage = TSVReader.SetData("TSV/" + languageType.ToString());
        foreach (var localizeText in localizeTexts)
        {
            if (localizeText.Value != null)
            {
                localizeText.Value.SetText(localizeText.Key);
            }
        }
    }

    #endregion

    #region Darkmode관련
    public bool darkmode
    {
        get
        {
            return PlayerPrefs.GetInt("darkmode") == 1 ? true : false;
        }
    }
    public void DarkMode(bool sw)
    {
        if (sw)
        {
            foreach (var localizeText in localizeTexts)
            {
                if (localizeText.Value != null) localizeText.Value.SetDarkmode();
            }
        }
        else
        {
            foreach (var localizeText in localizeTexts)
            {
                if (localizeText.Value != null) localizeText.Value.RemoveDarmode();
            }
        }
    }
    #endregion


    #region For Local Alarm Data

    public List<AlarmData> alarmDatas = new List<AlarmData>();

    public int AlarmCount
    {
        get { return PlayerPrefs.GetInt("AlarmCount", 0); }
    }


    public void AddAlarm(int _hour, int _min, string _AMPM, string _title)
    {
        DateTime date = Convert.ToDateTime(_hour + ":" + _min + ":00" + " " + _AMPM);
        if (date < DateTime.Now) date.AddDays(1);
        //유저 index 추가
        int index = DataManager.Inst.AlarmCount;
        PlayerPrefs.SetString("Alarm" + index, date.ToString() +  "/" + _title + "/1");
        alarmDatas.Add(new AlarmData(index, date.ToString(),  _title, 1));
        PlayerPrefs.SetInt("AlarmCount", AlarmCount + 1);
        LocalNotifyUtils.SetTimeNoSeeNotification(date, "어서오세요.", "00:00 알람입니다.", new Color32(225, 255, 255, 255));
    }

    public void EditAlarm(AlarmData _alarmData)
    {
        alarmDatas[_alarmData.index] = _alarmData;
        PlayerPrefs.SetString("Alarm" + _alarmData.index, _alarmData.time + "/" + _alarmData.title+ "/" + _alarmData.on);
        ResetAlarm();
    }

    public void RemoveAlarm(AlarmData _alarmData)
    {
        alarmDatas.Remove(_alarmData);
        PlayerPrefs.SetInt("AlarmCount", AlarmCount - 1);
        ResetAlarm();
    }


    public List<AlarmData> GetAlarmData()
    {
        alarmDatas.Clear();
        for (int i = 0; i < AlarmCount; i++)
        {
            string[] temp = PlayerPrefs.GetString("Alarm" + i).Split('/');
            alarmDatas.Add(new AlarmData(i, temp[0],  temp[1], int.Parse(temp[2])));
        }
        return alarmDatas;
    }
    public void ResetAlarm()
    {
        LocalNotifyUtils.ClearNotification();
        GetAlarmData();
    }

    #endregion

}
