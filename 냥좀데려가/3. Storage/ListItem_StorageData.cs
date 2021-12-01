using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListItem_StorageData : MonoBehaviour
{
    [Header("Contents")]
    [SerializeField] private Image imgMask = null;
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
    [SerializeField] private CanvasGroup imgInArrange = null;
    [SerializeField] private TextMeshProUGUI txtInArrange = null;

    [Header("Button")]
    public Button btnStorageData = null;

    [Header("Data")]
    public FurnitureData furnitureData = null;
    public ItemData itemData = null;
    public bool isItem;

    // 아이템 리스트 아이템 초기화
    public void Init(ItemData item)
    {
        imgMask.color = new Color(1, 1, 1, 150f / 255f);
        itemData = item;
        furnitureData = null;
        isItem = true;
        if(item.Type == ItemType.HairBall)
        {
            canvasHairBall.alpha = 1;
            imgHairBall.sprite = item.Sprite;
        }
        else
        {
            canvasStorage.alpha = 1;
            imgStorage.sprite = item.Sprite;
        }
        imgStorage.sprite = item.Sprite;
        btnStorageData.gameObject.GetComponent<CanvasGroup>().alpha = 1;
        btnStorageData.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        groupCount.alpha = 1;
        txtCount.text = item.count.ToString();
    }

    // 가구 리스트 아이템 초기화
    public void Init(FurnitureData furniture)
    {
        imgMask.color = new Color(1, 1, 1, 150f / 255f);
        furnitureData = furniture;
        itemData = null;
        isItem = false;
        if(furniture.Type == FurnitureType.Interior)
        {
            canvasInterior.alpha = 1;
            imgInterior.sprite = furniture.Sprite;
        }
        else
        {
            canvasStorage.alpha = 1;
            imgStorage.sprite = furniture.Sprite;
        }
        imgStorage.sprite = furniture.Sprite;
        btnStorageData.gameObject.GetComponent<CanvasGroup>().alpha = 1;
        btnStorageData.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        if(furniture.Type == FurnitureType.FunctionFurniture)
        {
            groupLevel.alpha = 1;
            txtLevel.text = furniture.level.ToString();
        }
        ChangeInOutFuniture(furniture.state);
    }

    // 가구 레벨 변경 함수
    public void ChangeLevel(int level)
    {
        txtLevel.text = level.ToString();
    }

    // 개수 변경 함수
    public void ChangeCount(int count)
    {
        txtCount.text = count.ToString();
    }

    // 가구 배치 유무에 따른 마스크 함수
    public void ChangeInOutFuniture(FurnitureState state)
    {
        if(state == FurnitureState.Stored)
        {
            imgInArrange.alpha = 0;
        }
        else
        {
            txtInArrange.text = TableDataManager.Instance.table_String["Furniture/State/" + (int)state].Contents[(int)UI_Setting.language];
            imgInArrange.alpha = 1;
        }
    }
}
