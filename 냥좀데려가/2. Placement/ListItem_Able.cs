using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListItem_Able : MonoBehaviour, IContentUIControl<AbleData>
{
    [Header("Contents")]
    [SerializeField] private Image imgIcon = null;
    [SerializeField] private CanvasGroup groupLevel = null;
    [SerializeField] private TextMeshProUGUI txtLevel = null;
    [SerializeField] private CanvasGroup groupName = null;
    [SerializeField] private TextMeshProUGUI txtName = null;

    [Header("Data")]
    public AbleData ableData = null;

    public void InitData()
    {

    }

    public void SetData(AbleData able)
    {
        ableData = able;
        if(able.isFurniture)
        {
            imgIcon.sprite = able.furniture.Sprite;
            if (able.furniture.Type == FurnitureType.FunctionFurniture)
            {
                groupLevel.alpha = 1;
                txtLevel.text = able.furniture.level.ToString();
            }
        }
        else
        {
            imgIcon.sprite = able.cat.SitSprite;
            groupName.alpha = 1;
            txtName.text = able.cat.name.ToString();
            groupLevel.alpha = 1;
            txtLevel.text = able.cat.level.ToString();
        }
    }

    public void OnDragEvent()
    {
        if(ableData.isFurniture)
        {
            StorageManager.Instance.ArrangeFurniture(ableData.furniture, "List");
        }
        else
        {
            StorageManager.Instance.ArrangeCatInFurniture(ableData.cat, true);
        }
    }
}
