using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Facebook.Unity;
using System.Collections.Generic;

public class LoginManager : MonoBehaviour
{
    public UIManager uiManager = null;

    [SerializeField] InputField infJoinEmail = null;
    [SerializeField] InputField infJoinNickname = null;
    [SerializeField] InputField infJoinPW = null;
    [SerializeField] InputField infJoinPWConfirm = null;
    [SerializeField] CanvasGroup cgDuplEmail = null;
    [SerializeField] CanvasGroup cgDuplNickname = null;
    [SerializeField] CanvasGroup imgDuplEmail = null;
    [SerializeField] CanvasGroup imgDuplNickname = null;
    [SerializeField] Text txtPasswordRule = null;
    [SerializeField] CanvasGroup cgPasswordConfirm = null;
    [SerializeField] Button btnJoin = null;

    [SerializeField] InputField infLoginEmail = null;
    [SerializeField] InputField infLoginPW = null;

    [SerializeField] UIManager UIManager;

    private class SocailUser
    {
        public string id;
        public string email;
        public string nickname;
    }

    private SocailUser suser = new SocailUser();

    #region EmailJoin
    // 회원가입 패널 초기화
    public void ResetJoinPanel()
    {
        infJoinEmail.text = null;
        infJoinNickname.text = null;
        infJoinPW.text = null;
        infJoinPWConfirm.text = null;
        SetBtnDuplEmail(true);
        SetBtnDuplNickname(true);
        SetPasswordRuleColor(true);
        SetPasswordConfirm(false);
    }

    // 가입 버튼 클릭 함수
    public void OnClickEmailJoin()
    {
        if (infJoinPW.text.Trim() == "")
        {
            UIPopupManager.Inst.Show("popup_blankfirst_input_password");
        }
        else if (!IsValidPW(infJoinPW.text))
        {
            SetPasswordRuleColor(false);
        }
        else if (infJoinPW.text != infJoinPWConfirm.text)
        {
            SetPasswordConfirm(true);
        }
        else
        {
            StartCoroutine(DBManager.CoJoin(infJoinEmail.text, infJoinNickname.text, infJoinPW.text, infJoinPWConfirm.text, () => {
                UIPopupManager.Inst.Show("popup_joinsuccess", () =>
                {
                    uiManager.ShowUI(1);
                    ResetEmailLoginPanel();
                    infLoginEmail.text = infJoinEmail.text;

                });
            }, (key, code) => ErrorPopup(key, code)));
        }
    }

    // 이메일 중복확인 클릭 버튼
    public void OnClickCheckDuplEmail()
    {
        if (infJoinEmail.text.Trim() == "")
        {
            UIPopupManager.Inst.Show("popup_blankemail");
        }
        else if (!IsValidEmail(infJoinEmail.text))
        {
            UIPopupManager.Inst.Show("popup_joinemailcheck");
        }
        else
        {
            StartCoroutine(DBManager.CoCheckDuplEmail(infJoinEmail.text, (dupl) => { SetBtnDuplEmail(dupl); cgDuplEmail.alpha = dupl ? 1 : 0; if (!dupl) UIPopupManager.Inst.Show("popup_uniqueemailnickname"); }, (key, code) => ErrorPopup(key, code)));

        }
    }

    // 닉네임 중복확인 클릭 버튼
    public void OnClickCheckDuplNickname()
    {
        if (infJoinNickname.text.Trim() == "")
        {
            UIPopupManager.Inst.Show("popup_blanknickname");
        }
        else
        {
            StartCoroutine(DBManager.CoCheckDuplNickname(infJoinNickname.text, (dupl) => { SetBtnDuplNickname(dupl); cgDuplNickname.alpha = dupl ? 1 : 0; if (!dupl) UIPopupManager.Inst.Show("popup_uniqueemailnickname"); }, (key, code) => ErrorPopup(key, code)));

        }
    }

    // 이메일 중복확인 버튼 세팅
    public void SetBtnDuplEmail(bool reset)
    {
        imgDuplEmail.alpha = reset ? 0 : 1;
        imgDuplEmail.blocksRaycasts = !reset;
        cgDuplEmail.alpha = 0;
        btnJoin.interactable = imgDuplEmail.blocksRaycasts && imgDuplNickname.blocksRaycasts;
    }

    // 닉네임 중복확인 버튼 세팅
    public void SetBtnDuplNickname(bool reset)
    {
        imgDuplNickname.alpha = reset ? 0 : 1;
        imgDuplNickname.blocksRaycasts = !reset;
        cgDuplNickname.alpha = 0;
        btnJoin.interactable = imgDuplEmail.blocksRaycasts && imgDuplNickname.blocksRaycasts;
    }

    // 비밀번호 유효성 체크 후 경고 세팅
    public void SetPasswordRuleColor(bool normal)
    {
        if (normal) txtPasswordRule.color = new Color(50f / 255f, 50f / 255f, 50f / 255f);
        else txtPasswordRule.color = new Color(225f / 255f, 93f / 255f, 93f / 255f);
    }

    // 비밀번호 확정
    public void SetPasswordConfirm(bool error)
    {
        cgPasswordConfirm.alpha = error ? 1 : 0;
    }

    // 이메일 유효성 체크
    private bool IsValidEmail(string email)
    {
        bool valid = Regex.IsMatch(email, @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");
        return valid;
    }

    // 비밀번호 유효성 체크
    private bool IsValidPW(string pw)
    {
        //bool valid = Regex.IsMatch(pw, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[\W]).{8,20}$");
        bool valid = Regex.IsMatch(pw, @"^(?=.*[a-z])(?=.*[0-9]).{6,20}$");
        return valid;
    }
    #endregion

    #region EmailLogin
    // 이메일 로그인 패널 초기화
    public void ResetEmailLoginPanel()
    {
        infLoginEmail.text = null;
        infLoginPW.text = null;
    }

    // 이메일 로그인 클릭 함수
    public void OnClickEmailLogin()
    {
        // 이메일 및 암호 빈칸 여부 체크
        if (infLoginEmail.text.Trim() == "")
        {
            UIPopupManager.Inst.Show("popup_blankemail");
        }
        else if (infLoginPW.text.Trim() == "")
        {
            UIPopupManager.Inst.Show("popup_blankpassword");
        }
        else
        {
            // DB 접근하여 이메일 로그인 요청
            StartCoroutine(DBManager.CoEmailLogin(infLoginEmail.text, infLoginPW.text,
            (token, result) =>
            {
                Login(token, result);
            }, (key, code) => ErrorPopup(key,code)));
        }
    }
    #endregion

    #region GoogleLogin
    public void OnClickGoogleLogin()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                    .RequestEmail()         // 이메일 정보 요청
                    .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        if (!Social.localUser.authenticated)
        {
            DebugTemp.Inst.ShowText("구글 로그인 시도");
            Social.localUser.Authenticate((success) =>
            {
                if (success)
                {
                    suser.id = Social.localUser.id;
                    suser.email = ((PlayGamesLocalUser)Social.localUser).Email;
                    suser.nickname = Social.localUser.userName;
                    StartCoroutine(DBManager.CoSocialLogin("google", suser.id, (token, result) =>
                    {
                        Login(token, result);
                    },
                    () => UIPopupManager.Inst.ShowNicknamePanel(suser.nickname, (nickname) =>
                    {
                        DebugTemp.Inst.ShowText("중복없는 아이디\n회원가입 시도\n" + nickname);
                        StartCoroutine(DBManager.CoSocialJoin("google", suser.id, suser.email, nickname,
                        (token, result) =>
                        {
                            Login(token, result);
                        }, (key, code) => ErrorPopup(key, code)));
                    }), (key, code) => ErrorPopup(key, code)));
                }
                else UIPopupManager.Inst.ShowError("S01");
            });
        }
        else
        {
            DebugTemp.Inst.ShowText("이미 구글 로그인 됨");
            suser.id = Social.localUser.id;
            suser.email = ((PlayGamesLocalUser)Social.localUser).Email;
            suser.nickname = Social.localUser.userName;
            StartCoroutine(DBManager.CoSocialLogin("google", suser.id, (token, result) =>
            {
                Login(token, result);
            },
            () => UIPopupManager.Inst.ShowNicknamePanel(suser.nickname, (nickname) =>
            {
                DebugTemp.Inst.ShowText("중복없는 아이디\n회원가입 시도\n" + nickname);
                StartCoroutine(DBManager.CoSocialJoin("google", suser.id, suser.email, nickname,
                (token, result) =>
                {
                    Login(token, result);
                }, (key, code) => ErrorPopup(key, code)));
            }), (key, code) => ErrorPopup(key, code)));
        }

    }
    #endregion

    #region FacebookLogin
    public void OnClickFacebookLogin()
    {
        if (!FB.IsInitialized)
        {
            // 초기화가 되지 않았다면 초기화를 진행(콜백,콜백)
            FB.Init(() =>
            {
                // 초기화가 진행 후
                if (FB.IsInitialized)
                {
                    // 개발자페이지의 앱활성화
                    FB.ActivateApp();
                    FBLogin();
                }
                else UIPopupManager.Inst.ShowError("S02");
            }, OnHideUnity);
        }
        else
        {
            // 이미 초기화가 진행됐다면 개발자페이지의 앱을 활성화
            FB.ActivateApp();
            FBLogin();
        }
    }

    private void FBLogin()
    {
        var Perms = new List<string>() { "email", "public_profile" };

        DebugTemp.Inst.ShowText("페이스북 로그인 시도");

        //로그인 됐는지 확인하는 함수
        if (!FB.IsLoggedIn)
        {
            //로그인 안됐다면 로그인하는 함수(권한,콜백)
            FB.LogInWithReadPermissions(Perms, (result) =>
            {
                if (result.Cancelled)
                {
                    UIPopupManager.Inst.ShowError("S03");
                }
                else
                {
                    DebugTemp.Inst.ShowText("페이스북 로그인 성공");
                    AfterFBLogin();
                }
            });
        }
        else
        {
            DebugTemp.Inst.ShowText("이미 페이스북 로그인됨");
            AfterFBLogin();
        }
    }

    private void AfterFBLogin()
    {
        var aToken = AccessToken.CurrentAccessToken;
        FB.API("/me?fields=id,name,email", HttpMethod.GET, GetFacebookInfo);
    }

    private void GetFacebookInfo(IResult result)
    {
        if (result.Error == null)
        {
            suser.id = result.ResultDictionary["id"].ToString();
            suser.email = result.ResultDictionary["email"].ToString();
            suser.nickname = result.ResultDictionary["name"].ToString();

            StartCoroutine(DBManager.CoSocialLogin("facebook", suser.id, (token, result) =>
            {
                Login(token, result);
            },
            () => UIPopupManager.Inst.ShowNicknamePanel(suser.nickname, (nickname) =>
            {
                DebugTemp.Inst.ShowText("중복없는 아이디\n회원가입 시도\n"+nickname);
                
                StartCoroutine(DBManager.CoSocialJoin("facebook", suser.id, suser.email, nickname,
                (token, result) =>
                {
                    Login(token, result);
                }, (key, code) => ErrorPopup(key, code)));
                
            }), (key, code) => ErrorPopup(key, code)));
        }
        else
        {
            UIPopupManager.Inst.ShowError("S04");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
            Time.timeScale = 0.0f;
        else
            Time.timeScale = 1.0f;

    }
    #endregion

    #region ETC
    public void OnClickRule()
    {
        Application.OpenURL("https://blog.naver.com/ppyl88/222434247989");
    }

    public void ErrorPopup(string key, int code)
    {
        string errorcode = (code < 10) ? "M0" + code : "M" + code;
        if (key == null) UIPopupManager.Inst.ShowError(errorcode);
        else UIPopupManager.Inst.Show(key);
    }

    public void Login(string _token, UserData _userdata)
    {
        DataManager.Inst.SetToken(_token);
        DataManager.Inst.userData = _userdata;
        SceneManager.LoadScene(1);
    }
    #endregion
}
