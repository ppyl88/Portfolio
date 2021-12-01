using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEditor;

public class UI_Setting : MonoBehaviour
{

    [Header("Sound")]
    [SerializeField] private Slider sldBGM = null;
    [SerializeField] private Slider sldEffect = null;
    [SerializeField] private CanvasGroup btnBGMMute = null;
    [SerializeField] private CanvasGroup btnBGMPlay = null;
    [SerializeField] private CanvasGroup btnEffectMute = null;
    [SerializeField] private CanvasGroup btnEffectPlay = null;

    [Header("Version")]
    [SerializeField] private TextMeshProUGUI txtVersion = null;

    [Header("Language")]
    [SerializeField] private TMP_Dropdown drdLanguage = null;
    public static LanguageType language = LanguageType.한국어;

    [Header("ScreenMove")]
    [SerializeField] private Toggle togScreenButton = null;
    [SerializeField] private Toggle togScreenSwipe = null;
    [SerializeField] private CanvasGroup LeftRightBtnUIMain = null;
    [SerializeField] private CanvasGroup LeftRightBtnUIVisit = null;
    public static ScreenMoveType screenMove = ScreenMoveType.Button;

    [Header("Alarm")]
    [SerializeField] private Toggle togAlarmOn = null;
    [SerializeField] private Toggle togAlarmOff = null;
    public static AlarmTpye alarmType = AlarmTpye.On;

    private void Awake()
    {
        language = (LanguageType)PlayerPrefs.GetInt("language");
        screenMove = (ScreenMoveType) PlayerPrefs.GetInt("screenMove");
        alarmType = (AlarmTpye)PlayerPrefs.GetInt("alarmType");
    }

    private void Start()
    {
        txtVersion.text = Application.version.ToString();
        List<string> options = new List<string>();
        for (int i = 0; i < System.Enum.GetValues(typeof(LanguageType)).Length; i++)
        {
            options.Add(((LanguageType)i).ToString());
        }
        drdLanguage.AddOptions(options);
        drdLanguage.value = (int)language;
        sldBGM.value = SoundManager.Instance.volBGM;
        sldEffect.value = SoundManager.Instance.volEffect;
        SetBGMMute(SoundManager.Instance.muteBGM);
        SetEffectMute(SoundManager.Instance.muteEffect);
        sldBGM.onValueChanged.AddListener(SetBGMMute);
        sldBGM.onValueChanged.AddListener(SoundManager.Instance.ChangeBGMVolume);
        sldEffect.onValueChanged.AddListener(SetEffectMute);
        sldEffect.onValueChanged.AddListener(SoundManager.Instance.ChangeEffectVolume);

        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            if (screenMove == 0)
            {
                togScreenButton.isOn = true;
                EnableScreenButton(true);
            }
            else
            {
                togScreenSwipe.isOn = true;
                EnableScreenButton(false);
            }

            if(alarmType == AlarmTpye.On)
            {
                togAlarmOn.isOn = true;
            }
            else
            {
                togAlarmOff.isOn = true;
            }

        }
    }

    public void ChangeLauge()
    {
        if (language != (LanguageType)drdLanguage.value)
        {
            language = (LanguageType)drdLanguage.value;
            PlayerPrefs.SetInt("language", (int)language);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void SetBGMMute(float _bgmValue)
    {
        if (_bgmValue >= 0.01f)
        {
            SetBGMMute(false);
        }
        else
        {
            SetBGMMute(true);
        }
    }

    private void SetEffectMute(float _effectValue)
    {
        if (_effectValue >= 0.01f)
        {
            SetEffectMute(false);
        }
        else
        {
            SetEffectMute(true);
        }
    }

    private void SetBGMMute(bool isMute)
    {
        btnBGMMute.alpha = isMute? 0 : 1;
        btnBGMMute.blocksRaycasts = !isMute;
        btnBGMPlay.alpha = isMute? 1 : 0;
        btnBGMPlay.blocksRaycasts = isMute;
        SoundManager.Instance.MuteBGM(isMute);
    }

    private void SetEffectMute(bool isMute)
    {
        btnEffectMute.alpha = isMute ? 0 : 1;
        btnEffectMute.blocksRaycasts = !isMute;
        btnEffectPlay.alpha = isMute ? 1 : 0;
        btnEffectPlay.blocksRaycasts = isMute;
        SoundManager.Instance.MuteEffect(isMute);
    }

    public void OnclickMuteBGM()
    {
        bool isMute = SoundManager.Instance.muteBGM;
        SetBGMMute(!isMute);
        sldBGM.value = isMute ? SoundManager.Instance.volBGM : 0;
    }

    public void OnclickMuteEffect()
    {
        bool isMute = SoundManager.Instance.muteEffect;
        SetEffectMute(!isMute);
        sldEffect.value = isMute ? SoundManager.Instance.volEffect : 0;
    }

    public void OnClickLogOut()
    {
        UserDataManager.Inst.UpdateLastConnection(false);
        Authentication.Inst.Logout();
        SceneManager.LoadScene(0);
    }

    public void OnClickAlarmButton()
    {
        if (togAlarmOn.isOn && alarmType != AlarmTpye.On)
        {
            alarmType = AlarmTpye.On;
            PlayerPrefs.SetInt("alarmType", (int)alarmType);
        }
    }

    public void OnClickAlarmOffButton()
    {
        if (togAlarmOff.isOn && alarmType != AlarmTpye.Off)
        {
            alarmType = AlarmTpye.Off;
            PlayerPrefs.SetInt("alarmType", (int)alarmType);
        }
    }

    public void OnClickScreenButton()
    {
        if(togScreenButton.isOn && screenMove != ScreenMoveType.Button)
        {
            screenMove = ScreenMoveType.Button;
            PlayerPrefs.SetInt("screenMove", (int) screenMove);
            EnableScreenButton(true);
        }
    }

    private void EnableScreenButton(bool enable)
    {
        LeftRightBtnUIMain.alpha = enable? 1 : 0;
        LeftRightBtnUIMain.blocksRaycasts = enable;
        LeftRightBtnUIVisit.alpha = enable ? 1 : 0;
        LeftRightBtnUIVisit.blocksRaycasts = enable;
    }

    public void OnClickScreenSwipe()
    {
        if (togScreenSwipe.isOn && screenMove != ScreenMoveType.Swipe)
        {
            screenMove = ScreenMoveType.Swipe;
            PlayerPrefs.SetInt("screenMove", (int) screenMove);
            EnableScreenButton(false);
        }
    }
}
