using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_CatDataDetail : MonoBehaviour
{
    public int catIndex;

    [Header("Detail Contents")]
    [SerializeField] private TextMeshProUGUI txtName = null;
    [SerializeField] private TextMeshProUGUI txtExp = null;
    [SerializeField] private TextMeshProUGUI txtType = null;
    [SerializeField] private TextMeshProUGUI txtState = null;
    [SerializeField] private TextMeshProUGUI txtlevel = null;
    [SerializeField] private TextMeshProUGUI[] txtStat = null;
    [SerializeField] private Image imgCat = null;

    [Header("Place Cat")]
    [SerializeField] private CanvasGroup[] panelBtnState = null;
    [SerializeField] private TextMeshProUGUI txtFuniture = null;
    [SerializeField] private TextMeshProUGUI txtOut = null;
    [SerializeField] private Button btnFuniture = null;

    [Header("Change Cat Name")]
    [SerializeField] private TMP_InputField inputNewName = null;

    // 고양이 상세 정보를 클릭했을 때 나타낼 내용 변경 (어떤 버튼을 클릭했느냐에 따라 내용 변경)
    public void SetCatDetail(CatData catData)
    {
        string strDefault = "Cat/UI/";
        catIndex = Authentication.Inst.userData.cats.IndexOf(catData);
        txtName.text = catData.name;
        txtType.text = catData.TypeName;
        txtlevel.text = catData.level.ToString();
        for (int i = 0; i < catData.stat.Length; i++)
        {
            txtStat[i].text = catData.stat[i].ToString();
        }
        ChangeBTNPlace();
        if (catData.IconSprite == null)
        {
            imgCat.sprite = catData.SitSprite;
        }
        else
        {
            imgCat.sprite = catData.IconSprite;
        }

        // 고양이 레벨이 맥스 레벨 미만일 경우, exp 표시 및 가구 배치 유무에 따라 아이콘 변경
        if (catData.level < TableDataManager.Instance.table_Setting["Cat_Max_Level"].Value)
        {
            int maxExp = catData.MaxExp;
            txtExp.text = catData.exp + "/" + maxExp + "(" + (catData.exp * 100) / maxExp + "%)";
            txtFuniture.text = TableDataManager.Instance.table_String[strDefault + "BtnFurniture"].Contents[(int)UI_Setting.language];
            btnFuniture.interactable = true;
        }
        // 고양이 레벨이 맥스레벨 이상일 경우 exp 표시 변경 및 가구 배치 아이콘 변경
        else
        {
            txtExp.text = "MAX";
            txtFuniture.text = TableDataManager.Instance.table_String[strDefault + "LvMax"].Contents[(int)UI_Setting.language];
            btnFuniture.interactable = false;
        }
    }

    // 고양이 스탯 변경
    public void UpgradeCat()
    {
        string strDefault = "Cat/UI/";
        CatData catData = Authentication.Inst.userData.cats[catIndex];

        txtlevel.text = catData.level.ToString();
        for (int i = 0; i < catData.stat.Length; i++)
        {
            txtStat[i].text = catData.stat[i].ToString();
        }

        // 고양이 레벨이 맥스 레벨 미만일 경우, exp 표시 및 가구 배치 유무에 따라 아이콘 변경
        if (catData.level < TableDataManager.Instance.table_Setting["Cat_Max_Level"].Value)
        {
            txtExp.text = catData.exp + "/" + catData.MaxExp + "(" + (catData.exp * 100) / catData.MaxExp + "%)";
            txtFuniture.text = TableDataManager.Instance.table_String[strDefault + "BtnFurniture"].Contents[(int)UI_Setting.language];
            btnFuniture.interactable = true;
        }
        // 고양이 레벨이 맥스레벨 이상일 경우 exp 표시 변경 및 가구 배치 아이콘 변경
        else
        {
            txtExp.text = "MAX";
            txtFuniture.text = TableDataManager.Instance.table_String[strDefault + "LvMax"].Contents[(int)UI_Setting.language];
            btnFuniture.interactable = false;
        }
    }

    // 가구 배치 유무에 따른 버튼 변경
    public void ChangeBTNPlace()
    {
        CatState state = Authentication.Inst.userData.cats[catIndex].state;
        string strDefault = "Cat/";
        for (int i = 0; i < panelBtnState.Length; i++)
        {
            panelBtnState[i].alpha = 0;
            panelBtnState[i].blocksRaycasts = false;
        }
        txtState.text = TableDataManager.Instance.table_String[strDefault + "State/" + (int)state].Contents[(int)UI_Setting.language];
        switch (state)
        {
            case CatState.Free:
                panelBtnState[0].alpha = 1;
                panelBtnState[0].blocksRaycasts = true;
                break;
            case CatState.InInterior:
            case CatState.UsingFurniture:
                panelBtnState[1].alpha = 1;
                panelBtnState[1].blocksRaycasts = true;
                txtOut.text = TableDataManager.Instance.table_String[strDefault + "UI/BtnOut"].Contents[(int)UI_Setting.language];
                break;
            case CatState.EndFurniture:
                panelBtnState[1].alpha = 1;
                panelBtnState[1].blocksRaycasts = true;
                txtOut.text = TableDataManager.Instance.table_String[strDefault + "UI/BtnEnd"].Contents[(int)UI_Setting.language];
                break;
        }
    }

    // 고양이 이름 변경 클릭 시 기존 이름 세팅
    public void CatNameSetting()
    {
        inputNewName.text = txtName.text;
    }

    public void CatNameChange(string name)
    {
        txtName.text = name;
    }

    // 고양이 이름 변경 완료 버튼 클릭 시 동작
    public string GetNewCatName()
    {
        string newName;

        // 공백일 경우 기존 이름 사용
        if (inputNewName.text == "")
        {
            newName = txtName.text;
        }
        // 공백이 아닐 경우 새로 입력된 값 사용
        else
        {
            newName = inputNewName.text;
        }

        return newName;
    }
}
