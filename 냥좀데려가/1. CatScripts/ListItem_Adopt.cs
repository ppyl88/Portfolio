using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListItem_Adopt : MonoBehaviour
{
    public int adoptIndex;

    [Header("Adopt Description")]
    [SerializeField] private TextMeshProUGUI AdoptList = null;
    [SerializeField] private CanvasGroup imgSpecial = null;
    [SerializeField] private TextMeshProUGUI Special = null;
    [SerializeField] private Image imgNPC = null;
    [SerializeField] private TextMeshProUGUI txtDesc = null;

    [Header("Adopt Tag")]
    [SerializeField] private RectTransform transTag = null;
    [SerializeField] private TextMeshProUGUI textTag = null;
    [SerializeField] private Image imgTag = null;

    [Header("Adopt Condtions")]
    [SerializeField] private TextMeshProUGUI Condition = null;
    [SerializeField] private GameObject[] conditions = null;

    [Header("Adopt Rewards")]
    [SerializeField] private TextMeshProUGUI Reward = null;
    [SerializeField] private GameObject[] rewards = null;

    [Header("Adopt Cat Detail")]
    public Button btnSelectCat = null;
    [SerializeField] private TextMeshProUGUI CatSelect = null;
    [SerializeField] private TextMeshProUGUI WarnigNoCat = null;
    [SerializeField] private TextMeshProUGUI CatName = null;
    [SerializeField] private TextMeshProUGUI CatType = null;
    [SerializeField] private TextMeshProUGUI[] CatStat = null;

    [SerializeField] private CanvasGroup panelNoCat = null;
    [SerializeField] private CanvasGroup panelCat = null;
    [SerializeField] private Image imgCat = null;
    [SerializeField] private TextMeshProUGUI txtCatName = null;
    [SerializeField] private TextMeshProUGUI txtCatType = null;
    [SerializeField] private TextMeshProUGUI[] txtStat = null;

    // 입양 신청서 세팅
    public void Init(int index, int adoptIndex)
    {
        // 기본 UI 세팅
        string strDefault = "Adopt/UI/";
        AdoptList.text = TableDataManager.Instance.table_String[strDefault + "AdoptList"].Contents[(int)UI_Setting.language];
        Special.text = TableDataManager.Instance.table_String[strDefault + "Special"].Contents[(int)UI_Setting.language];
        Condition.text = TableDataManager.Instance.table_String[strDefault + "Condition"].Contents[(int)UI_Setting.language];
        Reward.text = TableDataManager.Instance.table_String[strDefault + "Reward"].Contents[(int)UI_Setting.language];
        CatSelect.text = TableDataManager.Instance.table_String[strDefault + "CatSelect"].Contents[(int)UI_Setting.language];
        WarnigNoCat.text = TableDataManager.Instance.table_String["Adopt/Warning/NoCat"].Contents[(int)UI_Setting.language];
        CatName.text = TableDataManager.Instance.table_String["Cat/UI/" + "CatName"].Contents[(int)UI_Setting.language];
        CatType.text = TableDataManager.Instance.table_String["Cat/UI/" + "CatType"].Contents[(int)UI_Setting.language];
        for (int i = 0; i < CatStat.Length; i++)
        {
            CatStat[i].text = TableDataManager.Instance.table_String["Cat/Stat/" + (i + 1)].Contents[(int)UI_Setting.language];
        }

        // 입양 신청서 내용 세팅
        this.adoptIndex = adoptIndex;
        Adopt_List adopt = TableDataManager.Instance.table_Adopt_List[adoptIndex];
        imgNPC.sprite = adopt.NPC;
        txtDesc.text = adopt.AdoptDesc;
        textTag.text = (index + 1).ToString();
        Vector3 pos = transTag.localPosition;
        pos.x += 58 * index;
        transTag.localPosition = pos;
        if(adopt.Type == AdoptType.Normal)
        {
            imgTag.color = Color.white;
            imgSpecial.alpha = 0;
        }
        else
        {
            imgTag.color = new Color(163f/255, 0, 233f/255);
            imgSpecial.alpha = 1;
        }
        // 입양 조건 UI 리스트 생성
        for (int i = 0; i < 3; i++)
        {
            AdoptApplication adoptApp = adopt.AdoptApplications[i];
            if (adoptApp.Condition != AdoptCondition.None)
            {
                int idx = (int)adoptApp.Condition - 1;
                conditions[idx].SetActive(true);
                string value;
                string condition;
                switch (adoptApp.Condition)
                {
                    case AdoptCondition.MoreThanStat1:
                        value = TableDataManager.Instance.table_String["Cat/Stat/" + 1].Contents[(int)UI_Setting.language];
                        condition = TableDataManager.Instance.table_String["Adopt/Condition/1"].Contents[(int)UI_Setting.language];
                        condition = condition.Replace("0", adoptApp.Value.ToString());
                        conditions[idx].GetComponentInChildren<TextMeshProUGUI>().text = value;
                        conditions[idx].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = condition;
                        break;
                    case AdoptCondition.MoreThanStat2:
                        value = TableDataManager.Instance.table_String["Cat/Stat/" + 2].Contents[(int)UI_Setting.language];
                        condition = TableDataManager.Instance.table_String["Adopt/Condition/1"].Contents[(int)UI_Setting.language];
                        condition = condition.Replace("0", adoptApp.Value.ToString());
                        conditions[idx].GetComponentInChildren<TextMeshProUGUI>().text = value;
                        conditions[idx].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = condition;
                        break;
                    case AdoptCondition.MoreThanStat3:
                        value = TableDataManager.Instance.table_String["Cat/Stat/" + 3].Contents[(int)UI_Setting.language];
                        condition = TableDataManager.Instance.table_String["Adopt/Condition/1"].Contents[(int)UI_Setting.language];
                        condition = condition.Replace("0", adoptApp.Value.ToString());
                        conditions[idx].GetComponentInChildren<TextMeshProUGUI>().text = value;
                        conditions[idx].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = condition;
                        break;
                    case AdoptCondition.CatType:
                        value = TableDataManager.Instance.table_String["Cat/Type/" + adoptApp.Value].Contents[(int)UI_Setting.language];
                        conditions[idx].GetComponentInChildren<TextMeshProUGUI>().text = value;
                        break;
                    case AdoptCondition.CatSubType:
                        value = TableDataManager.Instance.table_String["Cat/SubType/" + adoptApp.Value].Contents[(int)UI_Setting.language];
                        condition = TableDataManager.Instance.table_String["Common/UI/Cat"].Contents[(int)UI_Setting.language];
                        conditions[idx].GetComponentInChildren<TextMeshProUGUI>().text = value;
                        conditions[idx].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = condition;
                        break;
                }
            }
        }

        // 입양 보상 UI 리스트 생성
        for (int i = 0; i < adopt.Rewards.Length; i++)
        {
            RewardData reward = adopt.Rewards[i];
            if (reward.Type != RewardType.None)
            {
                int idx = (int)reward.Type - 1;
                rewards[idx].SetActive(true);
                rewards[idx].GetComponentInChildren<TextMeshProUGUI>().text = reward.Value.ToString();
                if (reward.Type == RewardType.Furniture)
                    rewards[idx].GetComponentInChildren<Image>().sprite = TableDataManager.Instance.table_Furniture[reward.Index].Sprite;
                else if (reward.Type == RewardType.Item)
                    rewards[idx].GetComponentInChildren<Image>().sprite = TableDataManager.Instance.table_Item[reward.Index].Sprite;
            }
        }

        ResetCatData();
    }

    // 고양이 데이터 리셋
    public void ResetCatData()
    {
        panelCat.alpha = 0;
        txtCatName.text = "";
        txtCatType.text = "";
        for (int i = 0; i < txtStat.Length; i++)
        {
            txtStat[i].text = "";
        }
    }

    // 고양이 데이터 세팅
    public void SetCatData(CatData cat)
    {
        panelCat.alpha = 1;
        if (cat.IconSprite == null)
        {
            imgCat.sprite = cat.SitSprite;
        }
        else
        {
            imgCat.sprite = cat.IconSprite;
        }
        txtCatName.text = cat.name;
        txtCatType.text = cat.TypeName;
        for (int i = 0; i < cat.stat.Length; i++)
        {
            txtStat[i].text = cat.stat[i].ToString();
        }
    }

    // 가능 고양이 없음 패널 활성화
    public void ActiveNoCat(bool active)
    {
        panelNoCat.alpha = active? 1:0;
        panelNoCat.blocksRaycasts = active;
    }
}
