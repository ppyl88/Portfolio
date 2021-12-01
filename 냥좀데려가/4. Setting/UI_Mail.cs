using UnityEngine;
using System.Net.Mail;
using TMPro;

public class UI_Mail : MonoBehaviour
{
    public TMP_InputField inputFieldContent;
    public TMP_InputField inputFieldMail;

    // private string contact = "snsgameteam@gmail.com";
    // private int cntRetry = 0;
    // private int maxRetry = 5;
    private string strDefault = "Setting/Warning/";

    private void Start()
    {
        if (Authentication.Inst.user == null)
        {
            if (PlayerPrefs.HasKey("replyMail")) inputFieldMail.text = PlayerPrefs.GetString("replyMail");
        }
        else
        {
            if (Authentication.Inst.user.Email.Contains("fb") || Authentication.Inst.user.Email.Contains("google"))
            {
                PlayerPrefs.SetString("replyMail", "");
                inputFieldMail.text = "";
            }
            else
            {
                PlayerPrefs.SetString("replyMail", Authentication.Inst.user.Email);
                inputFieldMail.text = Authentication.Inst.user.Email;
            }
        }
    }

    public void OnClickSendMail()
    {
        if (inputFieldContent.text.Trim() != "")
        {
            if (LoginSceneUIManager.Instance != null) LoginSceneUIManager.Instance.LoadingSimple(true);
            else UIManager.instance.Loading(true);
            SendMail();
        }
        else
        {
            inputFieldContent.text = "";
            string warning = TableDataManager.Instance.table_String[strDefault + "NoContent"].Contents[(int)UI_Setting.language];
            if (LoginSceneUIManager.Instance != null) LoginSceneUIManager.Instance.warningPanel.ShowPopUP(warning);
            else UIManager.instance.uI_Warning.ShowPopUP(warning);
        }
        if (inputFieldMail.text != "") PlayerPrefs.SetString("replyMail", inputFieldMail.text);
    }

    private void SendMail()
    {
        string warning_success = TableDataManager.Instance.table_String[strDefault + "SuccessMail"].Contents[(int)UI_Setting.language];

        if (Authentication.Inst.user == null)
        {
            GetComponent<GoogleSheetMessageManager>().SendMessage(
                $"User Name : {inputFieldMail.text}",
                $"Reply Mail : {inputFieldMail.text}\n"
                + $"Device Model : {SystemInfo.deviceModel}\n"
                + $"Device OS : {SystemInfo.operatingSystem}\n"
                + inputFieldContent.text);
            LoginSceneUIManager.Instance.warningPanel.ShowPopUP(warning_success);
            LoginSceneUIManager.Instance.LoadingSimple(false);
            UIManager.instance.Loading(false);
        }
        else
        {
            GetComponent<GoogleSheetMessageManager>().SendMessage(
                $"User Name : {Authentication.Inst.userData.nickName}\n"
                + $"User Code : {Authentication.Inst.userData.index}",
                $"Reply Mail : {inputFieldMail.text}\n"
                + $"Device Model : {SystemInfo.deviceModel}\n"
                + $"Device OS : {SystemInfo.operatingSystem}\n"
                + inputFieldContent.text);
            inputFieldContent.text = "";
            UIManager.instance.OpenPopUpChildControl(0);
            UIManager.instance.OpenPopUpChildControl(-3);
            UIManager.instance.uI_Warning.ShowPopUP(warning_success);
            UIManager.instance.Loading(false);
        }
        inputFieldContent.text = "";
    }

    /*
    private void SendMail()
    {
        MailMessage mail = new MailMessage();

        if (Authentication.Inst.user == null)
        {
            // 보내는 사람 메일, 이름, 인코딩(UTF-8)
            mail.From = new MailAddress(contact, "고객님", System.Text.Encoding.UTF8);
            // 본문 내용
            mail.Body =
             "Reply Mail : " + inputFieldMail.text + "\n" +
             "-----------------------------------------------------------------------------\n" +
             "Device Model : " + SystemInfo.deviceModel + "\n" +
             "Device OS : " + SystemInfo.operatingSystem + "\n" +
             "-----------------------------------------------------------------------------\n\n" +
             inputFieldContent.text
             ;
        }
        else
        {
            // 보내는 사람 메일, 이름, 인코딩(UTF-8)
            mail.From = new MailAddress(contact, Authentication.Inst.userData.nickName, System.Text.Encoding.UTF8);
            // 본문 내용
            mail.Body =
             "Reply Mail : " + inputFieldMail.text + "\n" +
             "-----------------------------------------------------------------------------\n" +
             "UserInfo" + "\n" +
             "UserIndex : " + Authentication.Inst.userIndex + "\n" +
             "Email : " + Authentication.Inst.user.Email + "\n" +
             "-----------------------------------------------------------------------------\n" +
             "Device Model : " + SystemInfo.deviceModel + "\n" +
             "Device OS : " + SystemInfo.operatingSystem + "\n" +
             "-----------------------------------------------------------------------------\n\n" +
             inputFieldContent.text
             ;
        }
        // 받는 사람 메일
        mail.To.Add(contact);
        // 메일 제목
        mail.Subject = "문의 메일";

        // 메일 제목과 본문의 인코딩 타입(UTF-8)
        mail.SubjectEncoding = System.Text.Encoding.UTF8;
        mail.BodyEncoding = System.Text.Encoding.UTF8;

        // smtp 서버 주소
        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
        // smtp 포트
        SmtpServer.Port = 587;
        // smtp 인증
        SmtpServer.Credentials = new System.Net.NetworkCredential("snsgameteam", "snsgame1234");
        // SSL 사용 여부
        SmtpServer.EnableSsl = true;
        
        // 동기식 메일 발송
        //SmtpServer.Send(mail);

        // 비동기식 메일 발송 
        object userState = mail;
        SmtpServer.SendCompleted += new SendCompletedEventHandler(CheckSendCompleted);
        SmtpServer.SendAsync(mail, userState);
    }

    private void CheckSendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        string warning_success = TableDataManager.Instance.table_String[strDefault + "SuccessMail"].Contents[(int)UI_Setting.language];
        string warning_fail = TableDataManager.Instance.table_String[strDefault + "FailMail"].Contents[(int)UI_Setting.language];
        warning_fail = warning_fail.Replace("0", contact);
        if (LoginSceneUIManager.Instance != null)
        {
            if (e.Error == null)
            {
                // 발송 성공 시   
                inputFieldContent.text = "";
                LoginSceneUIManager.Instance.CloseUI(1);
                LoginSceneUIManager.Instance.warningPanel.ShowPopUP(warning_success);
                LoginSceneUIManager.Instance.LoadingSimple(false);
                cntRetry = 0;
            }
            else
            {
                // 발송 실패 시
                if (cntRetry < maxRetry)
                {
                    // 메일 발송 재시도
                    SendMail();
                    cntRetry++;
                }
                else
                {
                    // maxRetry 이상 발송 실패 시 발송 시도 정지 후 에러 메세지 출력
                    LoginSceneUIManager.Instance.warningPanel.ShowPopUP(warning_fail);
                    LoginSceneUIManager.Instance.LoadingSimple(false);
                    cntRetry = 0;
                }
            }
        }
        else
        {
            if (e.Error == null)
            {
                // 발송 성공 시   
                inputFieldContent.text = "";
                UIManager.instance.OpenPopUpChildControl(0);
                UIManager.instance.OpenPopUpChildControl(-3);
                UIManager.instance.uI_Warning.ShowPopUP(warning_success);
                UIManager.instance.Loading(false);
                cntRetry = 0;
            }
            else
            {
                // 발송 실패 시
                if (cntRetry < maxRetry)
                {
                    // 메일 발송 재시도
                    SendMail();
                    cntRetry++;
                }
                else
                {
                    // maxRetry 이상 발송 실패 시 발송 시도 정지 후 에러 메세지 출력
                    UIManager.instance.uI_Warning.ShowPopUP(warning_fail);
                    UIManager.instance.Loading(false);
                    cntRetry = 0;
                }
            }
        }
    }
    */
}
