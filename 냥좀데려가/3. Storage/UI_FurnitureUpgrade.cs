using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_FurnitureUpgrade : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtDesc = null;
    [SerializeField] private TextMeshProUGUI txtCurrent = null;
    [SerializeField] private TextMeshProUGUI txtNext = null;
    [SerializeField] private Button btnUpgrade = null;

    public void SetUpgradeUI(FurnitureData furniture)
    {
        string index_next = furniture.LevelIndex + "/" + (furniture.level + 1);
        string strDefault = "Storage/UI/";
        Item_Level_Table levelTable_next = TableDataManager.Instance.table_Item_Level[index_next];

        string desc = TableDataManager.Instance.table_String[strDefault + "UpgradeDesc"].Contents[(int)UI_Setting.language];
        desc = desc.Replace("Fee", levelTable_next.FeeName);
        desc = desc.Replace("0", furniture.UpgradeFee.ToString());
        desc = desc.Replace("Name", furniture.Name);
        txtDesc.text = desc;
        
        string current = TableDataManager.Instance.table_String[strDefault + "ItemEffectLong2"].Contents[(int)UI_Setting.language];
        current = current.Replace("Stat", furniture.StatName);
        current = current.Replace("XX", furniture.StatPerMinute.ToString());
        current = current.Replace("YY", furniture.StatValue.ToString());
        txtCurrent.text = current;

        string next = TableDataManager.Instance.table_String[strDefault + "ItemEffectLong2"].Contents[(int)UI_Setting.language];
        next = next.Replace("Stat", levelTable_next.StatName);
        next = next.Replace("XX", levelTable_next.Increase_Stat_Per_Minute.ToString());
        next = next.Replace("YY", levelTable_next.Stat_Parameter_Value.ToString());
        txtNext.text = next;

        if(Authentication.Inst.userData.gold >= furniture.UpgradeFee)
        {
            btnUpgrade.interactable = true;
        }
        else
        {
            btnUpgrade.interactable = false;
        }
    }
}
