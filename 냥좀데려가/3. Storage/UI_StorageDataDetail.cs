using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_StorageDataDetail : MonoBehaviour
{
    [Header("Data")]
    public bool isItem;
    public ItemData itemData;
    public FurnitureData furnitureData;

    [Header("Detail Contents")]
    [SerializeField] private CanvasGroup canvasStorage = null;
    [SerializeField] private Image imgStorage = null;
    [SerializeField] private CanvasGroup canvasInterior = null;
    [SerializeField] private Image imgInterior = null;
    [SerializeField] private CanvasGroup canvasHairBall = null;
    [SerializeField] private Image imgHairBall = null;

    [SerializeField] private CanvasGroup groupLevel = null;
    [SerializeField] private TextMeshProUGUI txtLevel = null;
    [SerializeField] private CanvasGroup groupCount = null;
    [SerializeField] private TextMeshProUGUI txtCount = null;

    [SerializeField] private TextMeshProUGUI txtDesc = null;

    [SerializeField] private TextMeshProUGUI txtName = null;
    [SerializeField] private TextMeshProUGUI txtType = null;

    [SerializeField] private GameObject groupState = null;
    [SerializeField] private TextMeshProUGUI txtState = null;
    [SerializeField] private GameObject groupCondition = null;
    [SerializeField] private TextMeshProUGUI txtCondition = null;
    [SerializeField] private GameObject groupEffectShort = null;
    [SerializeField] private TextMeshProUGUI txtEffectShort = null;
    [SerializeField] private GameObject groupEffectLong = null;
    [SerializeField] private TextMeshProUGUI txtEffectLong1 = null;
    [SerializeField] private TextMeshProUGUI txtEffectLong2 = null;
    [SerializeField] private GameObject groupUseTime = null;
    [SerializeField] private TextMeshProUGUI txtUseTime = null;
    [SerializeField] private GameObject groupBuffTime = null;
    [SerializeField] private TextMeshProUGUI txtBuffTime = null;

    [Header("Buttons")]
    [SerializeField] private GameObject btnUse = null;
    [SerializeField] private GameObject groupArrange = null;
    [SerializeField] private CanvasGroup btnArrange = null;
    [SerializeField] private CanvasGroup btnStore = null;
    [SerializeField] private GameObject btnUpgrade = null;
    [SerializeField] private TextMeshProUGUI txtUpgrade = null;

    private string strDefault = "Storage/UI/";

    // 아이템 상세 정보를 클릭했을 때 나타낼 내용 변경 (어떤 버튼을 클릭했느냐에 따라 내용 변경)
    public void SetStorageDetail(ItemData item)
    {
        isItem = true;
        itemData = item;
        ResetComponents();
        if (item.Type == ItemType.HairBall)
        {
            canvasHairBall.alpha = 1;
            imgHairBall.sprite = item.Sprite;
        }
        else
        {
            canvasStorage.alpha = 1;
            imgStorage.sprite = item.Sprite;
        }

        groupCount.alpha = 1;
        txtCount.text = item.count.ToString();

        txtDesc.text = item.Desc;
        txtName.text = item.Name;
        txtType.text = item.TypeName;

        if(item.Type == ItemType.Snack)
        {
            groupEffectShort.SetActive(true);
            string effect = TableDataManager.Instance.table_String[strDefault + "ItemEffectShort"].Contents[(int)UI_Setting.language];
            effect = effect.Replace("Stat", item.StatName);
            effect = effect.Replace("0", item.BuffValue.ToString());
            txtEffectShort.text = effect;

            groupBuffTime.SetActive(true);
            string time = TableDataManager.Instance.table_String[strDefault + "Time"].Contents[(int)UI_Setting.language];
            time = time.Replace("0", item.BuffTime.ToString());
            txtBuffTime.text = time;

            btnUse.SetActive(true);
        }
    }

    // 가구 상세 정보를 클릭했을 때 나타낼 내용 변경 (어떤 버튼을 클릭했느냐에 따라 내용 변경)
    public void SetStorageDetail(FurnitureData furniture)
    {
        isItem = false;
        furnitureData = furniture;
        ResetComponents();
        if (furniture.Type == FurnitureType.Interior)
        {
            canvasInterior.alpha = 1;
            imgInterior.sprite = furniture.Sprite;
        }
        else
        {
            canvasStorage.alpha = 1;
            imgStorage.sprite = furniture.Sprite;
        }

        txtDesc.text = furniture.Desc;
        txtName.text = furniture.Name;
        txtType.text = furniture.SubTypeName;

        if (furniture.Type == FurnitureType.FunctionFurniture)
        {
            groupLevel.alpha = 1;
            txtLevel.text = furniture.level.ToString();
            txtType.text = furniture.SubTypeName;

            groupCondition.SetActive(true);
            string condition = TableDataManager.Instance.table_String[strDefault + "ItemConditionContent"].Contents[(int)UI_Setting.language];
            condition = condition.Replace("0", furniture.MinCatLevel.ToString());
            txtCondition.text = condition;

            groupEffectLong.SetActive(true);
            string effect1 = TableDataManager.Instance.table_String[strDefault + "ItemEffectLong1"].Contents[(int)UI_Setting.language];
            effect1 = effect1.Replace("0", furniture.ExpValue.ToString());
            string effect2 = TableDataManager.Instance.table_String[strDefault + "ItemEffectLong3"].Contents[(int)UI_Setting.language];
            effect2 = effect2.Replace("Stat", furniture.StatName);
            effect2 = effect2.Replace("YY", (furniture.StatValue * furniture.UseTime / furniture.StatPerMinute).ToString());
            txtEffectLong1.text = effect1;
            txtEffectLong2.text = effect2;

            groupUseTime.SetActive(true);
            string time = TableDataManager.Instance.table_String[strDefault + "Time"].Contents[(int)UI_Setting.language];
            time = time.Replace("0", furniture.UseTime.ToString());
            txtUseTime.text = time;
            btnUpgrade.SetActive(true);

            string nextLevel = furniture.LevelIndex + "/" + (furniture.level + 1);
            if (TableDataManager.Instance.table_Item_Level.ContainsKey(nextLevel))
            {
                txtUpgrade.text = TableDataManager.Instance.table_String[strDefault + "BtnUpgrade"].Contents[(int)UI_Setting.language]; ;
                btnUpgrade.GetComponent<Button>().interactable = true;
            }
            else
            {
                txtUpgrade.text = TableDataManager.Instance.table_String[strDefault + "BtnMaxLevel"].Contents[(int)UI_Setting.language]; ;
                btnUpgrade.GetComponent<Button>().interactable = false;
            }
        }

        groupState.SetActive(true);
        groupArrange.SetActive(true);
        ChangeState();
    }

    private void ResetComponents()
    {
        canvasStorage.alpha = 0;
        canvasInterior.alpha = 0;
        canvasHairBall.alpha = 0;
        groupLevel.alpha = 0;
        groupCount.alpha = 0;
        groupState.SetActive(false);
        groupCondition.SetActive(false);
        groupEffectShort.SetActive(false);
        groupEffectLong.SetActive(false);
        groupUseTime.SetActive(false);
        groupBuffTime.SetActive(false);
        btnUse.SetActive(false);
        groupArrange.SetActive(false);
        btnUpgrade.SetActive(false);
    }

    // 가구 레벨 변경
    public void ChangeLevel()
    {
        txtLevel.text = furnitureData.level.ToString();
        string effect2 = TableDataManager.Instance.table_String[strDefault + "ItemEffectLong2"].Contents[(int)UI_Setting.language];
        effect2 = effect2.Replace("Stat", furnitureData.StatName);
        effect2 = effect2.Replace("XX", furnitureData.StatPerMinute.ToString());
        effect2 = effect2.Replace("YY", furnitureData.StatValue.ToString());
        txtEffectLong2.text = effect2;
        string nextLevel = furnitureData.LevelIndex + "/" + (furnitureData.level + 1);
        if (!TableDataManager.Instance.table_Item_Level.ContainsKey(nextLevel))
        {
            txtUpgrade.text = TableDataManager.Instance.table_String[strDefault + "BtnMaxLevel"].Contents[(int)UI_Setting.language];
            btnUpgrade.GetComponent<Button>().interactable = false;
        }
    }

    // 아이템 개수 변경
    public void ChangeCount()
    {
        txtCount.text = itemData.count.ToString();
    }

    // 가구 배치 유무에 따른 버튼 변경
    public void ChangeState()
    {
        FurnitureState state = furnitureData.state;
        btnArrange.alpha = 0;
        btnArrange.blocksRaycasts = false;
        btnStore.alpha = 0;
        btnStore.blocksRaycasts = false;

        string str = TableDataManager.Instance.table_String["Furniture/State/" + (int)state].Contents[(int)UI_Setting.language];
        if (state == FurnitureState.Stored)
        {
            btnArrange.alpha = 1;
            btnArrange.blocksRaycasts = true;
        }
        else
        {
            btnStore.alpha = 1;
            btnStore.blocksRaycasts = true;
            if (state == FurnitureState.UsingItem)
            {
                string itemName = TableDataManager.Instance.table_Item[furnitureData.arrangeIndex].Name;
                str = str.Replace(TableDataManager.Instance.table_String["Common/UI/Item"].Contents[(int)UI_Setting.language], itemName);
            }
            else if (state == FurnitureState.UsingCat)
            {
                int index = Authentication.Inst.userData.cats.FindIndex(cat => cat.idx == furnitureData.arrangeIndex);
                string catName = Authentication.Inst.userData.cats[index].name;
                str = str.Replace(TableDataManager.Instance.table_String["Common/UI/Cat"].Contents[(int)UI_Setting.language], catName);
            }
        }
        txtState.text = str;
    }
}
