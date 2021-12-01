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
    // ȸ������ �г� �ʱ�ȭ
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

    // ���� ��ư Ŭ�� �Լ�
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

    // �̸��� �ߺ�Ȯ�� Ŭ�� ��ư
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

    // �г��� �ߺ�Ȯ�� Ŭ�� ��ư
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

    // �̸��� �ߺ�Ȯ�� ��ư ����
    public void SetBtnDuplEmail(bool reset)
    {
        imgDuplEmail.alpha = reset ? 0 : 1;
        imgDuplEmail.blocksRaycasts = !reset;
        cgDuplEmail.alpha = 0;
        btnJoin.interactable = imgDuplEmail.blocksRaycasts && imgDuplNickname.blocksRaycasts;
    }

    // �г��� �ߺ�Ȯ�� ��ư ����
    public void SetBtnDuplNickname(bool reset)
    {
        imgDuplNickname.alpha = reset ? 0 : 1;
        imgDuplNickname.blocksRaycasts = !reset;
        cgDuplNickname.alpha = 0;
        btnJoin.interactable = imgDuplEmail.blocksRaycasts && imgDuplNickname.blocksRaycasts;
    }

    // ��й�ȣ ��ȿ�� üũ �� ��� ����
    public void SetPasswordRuleColor(bool normal)
    {
        if (normal) txtPasswordRule.color = new Color(50f / 255f, 50f / 255f, 50f / 255f);
        else txtPasswordRule.color = new Color(225f / 255f, 93f / 255f, 93f / 255f);
    }

    // ��й�ȣ Ȯ��
    public void SetPasswordConfirm(bool error)
    {
        cgPasswordConfirm.alpha = error ? 1 : 0;
    }

    // �̸��� ��ȿ�� üũ
    private bool IsValidEmail(string email)
    {
        bool valid = Regex.IsMatch(email, @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");
        return valid;
    }

    // ��й�ȣ ��ȿ�� üũ
    private bool IsValidPW(string pw)
    {
        //bool valid = Regex.IsMatch(pw, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[\W]).{8,20}$");
        bool valid = Regex.IsMatch(pw, @"^(?=.*[a-z])(?=.*[0-9]).{6,20}$");
        return valid;
    }
    #endregion

    #region EmailLogin
    // �̸��� �α��� �г� �ʱ�ȭ
    public void ResetEmailLoginPanel()
    {
        infLoginEmail.text = null;
        infLoginPW.text = null;
    }

    // �̸��� �α��� Ŭ�� �Լ�
    public void OnClickEmailLogin()
    {
        // �̸��� �� ��ȣ ��ĭ ���� üũ
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
            // DB �����Ͽ� �̸��� �α��� ��û
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
                    .RequestEmail()         // �̸��� ���� ��û
                    .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        if (!Social.localUser.authenticated)
        {
            DebugTemp.Inst.ShowText("���� �α��� �õ�");
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
                        DebugTemp.Inst.ShowText("�ߺ����� ���̵�\nȸ������ �õ�\n" + nickname);
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
            DebugTemp.Inst.ShowText("�̹� ���� �α��� ��");
            suser.id = Social.localUser.id;
            suser.email = ((PlayGamesLocalUser)Social.localUser).Email;
            suser.nickname = Social.localUser.userName;
            StartCoroutine(DBManager.CoSocialLogin("google", suser.id, (token, result) =>
            {
                Login(token, result);
            },
            () => UIPopupManager.Inst.ShowNicknamePanel(suser.nickname, (nickname) =>
            {
                DebugTemp.Inst.ShowText("�ߺ����� ���̵�\nȸ������ �õ�\n" + nickname);
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
            // �ʱ�ȭ�� ���� �ʾҴٸ� �ʱ�ȭ�� ����(�ݹ�,�ݹ�)
            FB.Init(() =>
            {
                // �ʱ�ȭ�� ���� ��
                if (FB.IsInitialized)
                {
                    // �������������� ��Ȱ��ȭ
                    FB.ActivateApp();
                    FBLogin();
                }
                else UIPopupManager.Inst.ShowError("S02");
            }, OnHideUnity);
        }
        else
        {
            // �̹� �ʱ�ȭ�� ����ƴٸ� �������������� ���� Ȱ��ȭ
            FB.ActivateApp();
            FBLogin();
        }
    }

    private void FBLogin()
    {
        var Perms = new List<string>() { "email", "public_profile" };

        DebugTemp.Inst.ShowText("���̽��� �α��� �õ�");

        //�α��� �ƴ��� Ȯ���ϴ� �Լ�
        if (!FB.IsLoggedIn)
        {
            //�α��� �ȵƴٸ� �α����ϴ� �Լ�(����,�ݹ�)
            FB.LogInWithReadPermissions(Perms, (result) =>
            {
                if (result.Cancelled)
                {
                    UIPopupManager.Inst.ShowError("S03");
                }
                else
                {
                    DebugTemp.Inst.ShowText("���̽��� �α��� ����");
                    AfterFBLogin();
                }
            });
        }
        else
        {
            DebugTemp.Inst.ShowText("�̹� ���̽��� �α��ε�");
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
                DebugTemp.Inst.ShowText("�ߺ����� ���̵�\nȸ������ �õ�\n"+nickname);
                
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
