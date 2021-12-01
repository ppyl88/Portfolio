using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_StorageDataList : MonoBehaviour
{
    [SerializeField] private UI_StorageDataDetail storageDataDetailUI = null;
    [SerializeField] private RectTransform contents = null;
    [SerializeField] private GameObject storageDataListPrefab = null;
    [SerializeField] private TMP_Dropdown drdCategory = null;
    private List<ListItem_StorageData> listStorageItem = new List<ListItem_StorageData>();
    private List<ListItem_StorageData> listStorageItemReal = new List<ListItem_StorageData>();
    private List<GameObject> listBlank = new List<GameObject>();

    private int maxBlank = 12;
    private int currentCategory;
    private int currentBlank;

    // 보유 가구/아이템 패널 생성
    public void CreateStorageItem()
    {
        // 빈 패널 생성
        for (int i=0; i<maxBlank; i++)
        {
            GameObject blankListItemGo = Instantiate(storageDataListPrefab, contents);
            blankListItemGo.SetActive(false);
            listBlank.Add(blankListItemGo);
        }

        // 패널에 가구/아이템 데이터 삽입
        for (int i = 0; i < Authentication.Inst.userData.furnitures.Count; i++)
        {
            FurnitureData furnitureData = Authentication.Inst.userData.furnitures[i];
            AddFunitureStorage(furnitureData);
        }
        for (int i = 0; i < Authentication.Inst.userData.items.Count; i++)
        {
            ItemData itemData = Authentication.Inst.userData.items[i];
            AddItemStorage(itemData);
        }
        ReArrangeStorageList();
        ResetCategory();

        // 드롭다운 필터 세팅
        List<string> categorys = new List<string>();
        for (int i = 0; i < System.Enum.GetValues(typeof(StorageCategory)).Length; i++)
        {
            categorys.Add(TableDataManager.Instance.table_String["Storage/Category/" + i].Contents[(int)UI_Setting.language]);
        }
        currentCategory = 0;
        drdCategory.AddOptions(categorys);
    }

    public void OnClickCategory()
    {
        if (currentCategory != drdCategory.value)
        {
            ResetCategory();
        }
    }

    private void ResetCategory()
    {
        int cntPanel = 0;
        currentCategory = drdCategory.value;
        switch ((StorageCategory)currentCategory)
        {
            case StorageCategory.All:
                for (int i = 0; i < listStorageItemReal.Count; i++)
                {
                    if (!listStorageItemReal[i].gameObject.activeSelf)
                    {
                        listStorageItemReal[i].gameObject.SetActive(true);
                    }
                    cntPanel++;
                }
                break;
            case StorageCategory.Item:
                for (int i = 0; i < listStorageItemReal.Count; i++)
                {
                    if (listStorageItemReal[i].isItem)
                    {
                        listStorageItemReal[i].gameObject.SetActive(true);
                        cntPanel++;
                    }
                    else listStorageItemReal[i].gameObject.SetActive(false);
                }
                break;
            case StorageCategory.Rest:
                for (int i = 0; i < listStorageItemReal.Count; i++)
                {
                    if (!listStorageItemReal[i].isItem && listStorageItemReal[i].furnitureData.SubType == FurnitureSubType.Rest)
                    {
                        listStorageItemReal[i].gameObject.SetActive(true);
                        cntPanel++;
                    }
                    else listStorageItemReal[i].gameObject.SetActive(false);
                }
                break;
            case StorageCategory.Mobile:
                for (int i = 0; i < listStorageItemReal.Count; i++)
                {
                    if (!listStorageItemReal[i].isItem && listStorageItemReal[i].furnitureData.SubType == FurnitureSubType.Mobile)
                    {
                        listStorageItemReal[i].gameObject.SetActive(true);
                        cntPanel++;
                    }
                    else listStorageItemReal[i].gameObject.SetActive(false);
                }
                break;
            case StorageCategory.Scratcher:
                for (int i = 0; i < listStorageItemReal.Count; i++)
                {
                    if (!listStorageItemReal[i].isItem && listStorageItemReal[i].furnitureData.SubType == FurnitureSubType.Scratcher)
                    {
                        listStorageItemReal[i].gameObject.SetActive(true);
                        cntPanel++;
                    }
                    else listStorageItemReal[i].gameObject.SetActive(false);
                }
                break;
            case StorageCategory.BuffBowl:
                for (int i = 0; i < listStorageItemReal.Count; i++)
                {
                    if (!listStorageItemReal[i].isItem && listStorageItemReal[i].furnitureData.SubType == FurnitureSubType.BuffBowl)
                    {
                        listStorageItemReal[i].gameObject.SetActive(true);
                        cntPanel++;
                    }
                    else listStorageItemReal[i].gameObject.SetActive(false);
                }
                break;
            case StorageCategory.InteriorFurniture:
                for (int i = 0; i < listStorageItemReal.Count; i++)
                {
                    if (!listStorageItemReal[i].isItem && (listStorageItemReal[i].furnitureData.SubType == FurnitureSubType.Wall || listStorageItemReal[i].furnitureData.SubType == FurnitureSubType.Floor))
                    {
                        listStorageItemReal[i].gameObject.SetActive(true);
                        cntPanel++;
                    }
                    else listStorageItemReal[i].gameObject.SetActive(false);
                }
                break;
            case StorageCategory.WallFloor:
                for (int i = 0; i < listStorageItemReal.Count; i++)
                {
                    if (!listStorageItemReal[i].isItem && (listStorageItemReal[i].furnitureData.SubType == FurnitureSubType.WallPaper || listStorageItemReal[i].furnitureData.SubType == FurnitureSubType.FloorMaterial))
                    {
                        listStorageItemReal[i].gameObject.SetActive(true);
                        cntPanel++;
                    }
                    else listStorageItemReal[i].gameObject.SetActive(false);
                }
                break;
        }

        // 생성할 빈 패널 수 계산
        int blankPanel = (cntPanel > maxBlank) ? ((cntPanel - 1) / 3 + 1) * 3 : maxBlank;
        int cntBlank = blankPanel - cntPanel;

        for(int i=0; i<cntBlank; i++)
        {
            if (!listBlank[i].activeSelf) listBlank[i].SetActive(true);
        }
        for(int i=cntBlank; i<maxBlank; i++)
        {
            if(listBlank[i].activeSelf) listBlank[i].SetActive(false);
        }
    }

    public void OnClickCloseStorageDetail()
    {
        UIManager.instance.OpenPopUpChildControl(0);
    }

    // 가구 추가 함수
    public void AddFunitureStorage(FurnitureData furnitureData)
    {
        ListItem_StorageData storageDataListItemGo = Instantiate(storageDataListPrefab, contents).GetComponent<ListItem_StorageData>();
        // 관리용 목록에 추가
        listStorageItem.Add(storageDataListItemGo);
        listStorageItemReal.Add(storageDataListItemGo);
        storageDataListItemGo.Init(furnitureData);
        // 가구 버튼을 눌렀을때 실행되는 함수 추가 
        storageDataListItemGo.btnStorageData.onClick.AddListener(() => {
            storageDataDetailUI.SetStorageDetail(furnitureData);
            UIManager.instance.OpenPopUpChildControl(-1);
            SoundManager.Instance.PlayClick();
        });

        ReArrangeStorageList();
        ResetCategory();
    }

    // 아이템 추가 함수
    public void AddItemStorage(ItemData itemData)
    {
        ListItem_StorageData storageDataListItemGo = Instantiate(storageDataListPrefab, contents).GetComponent<ListItem_StorageData>();
        // 관리용 목록에 추가
        listStorageItem.Add(storageDataListItemGo);
        listStorageItemReal.Add(storageDataListItemGo);
        storageDataListItemGo.Init(itemData);
        // 아이템 버튼을 눌렀을때 실행되는 함수 추가 
        storageDataListItemGo.btnStorageData.onClick.AddListener(() => {
            storageDataDetailUI.SetStorageDetail(itemData);
            UIManager.instance.OpenPopUpChildControl(-1);
            SoundManager.Instance.PlayClick();
        });

        ReArrangeStorageList();
        ResetCategory();
    }

    // 가구 제거 함수
    public void RemoveFunitureStorage(FurnitureData furniture)
    {
        // 인덱스 찾기
        int index = listStorageItem.FindIndex(storage => storage.furnitureData == furniture);
        int _index = listStorageItemReal.IndexOf(listStorageItem[index]);

        // 선택된 가구 오프젝트 제거 및 리스트에서 삭제
        Destroy(listStorageItem[index].gameObject);
        listStorageItem.RemoveAt(index);
        listStorageItemReal.RemoveAt(_index);

        ResetCategory();
    }

    // 아이템 제거 함수
    public void RemoveItemStorage(ItemData item)
    {
        int index = listStorageItem.FindIndex(storage => storage.itemData == item);
        int _index = listStorageItemReal.IndexOf(listStorageItem[index]);

        // 선택된 아이템 오프젝트 제거 및 리스트에서 삭제
        Destroy(listStorageItem[index].gameObject);
        listStorageItem.RemoveAt(index);
        listStorageItemReal.RemoveAt(_index);

        ResetCategory();
    }

    // 가구 레벨 변경
    public void ChangeLevel(FurnitureData furniture)
    {
        int index = listStorageItem.FindIndex(storage => storage.furnitureData == furniture);

        listStorageItem[index].ChangeLevel(furniture.level);
        ReArrangeStorageList();
    }

    // 아이템 개수 변경
    public void ChangeCount(ItemData item)
    {
        int index = listStorageItem.FindIndex(storage => storage.itemData == item);

        listStorageItem[index].ChangeCount(item.count);
    }

    // 가구 배치 유무에 따른 패널 마스크 생성
    public void ChangeState(FurnitureData furniture)
    {
        int index = listStorageItem.FindIndex(storage => storage.furnitureData == furniture);

        listStorageItem[index].ChangeInOutFuniture(furniture.state);
        ReArrangeStorageList();
    }

    // 보유 가구/아이템 리스트 재배열 (간식-가구 순, 상태 자유, 휴식가구-놀이기구-인테리어 순, 레벨 높은 순, 가구 인덱스가 높은 순, 획득한 순으로 나열)
    private void ReArrangeStorageList()
    {
        for (int i = 0; i < listStorageItemReal.Count - 1; i++)
        {
            int tmp = i;
            for (int j = i + 1; j < listStorageItemReal.Count; j++)
            {
                bool isItem_tmp = listStorageItemReal[tmp].isItem;
                bool isItem_j = listStorageItemReal[j].isItem;
                ItemData item_tmp = listStorageItemReal[tmp].itemData;
                ItemData item_j = listStorageItemReal[j].itemData;
                FurnitureData furniture_tmp = listStorageItemReal[tmp].furnitureData;
                FurnitureData furniture_j = listStorageItemReal[j].furnitureData;
                int index_tmp = listStorageItem.IndexOf(listStorageItemReal[tmp]);
                int index_j = listStorageItem.IndexOf(listStorageItemReal[j]);
                if (!isItem_tmp && isItem_j)
                {
                    tmp = j;
                }
                else if(isItem_tmp && isItem_j)
                {
                    if ((int)item_tmp.Type > (int)item_j.Type)
                    {
                        tmp = j;
                    }
                    else if (item_tmp.Type == item_j.Type)
                    {
                        if (item_tmp.index < item_j.index)
                        {
                            tmp = j;
                        }
                    }
                }
                else if (!isItem_tmp && !isItem_j)
                {
                    if (furniture_tmp.state != FurnitureState.Stored && furniture_j.state == FurnitureState.Stored)
                    {
                        tmp = j;
                    }
                    else if ((furniture_tmp.state != FurnitureState.Stored && furniture_j.state != FurnitureState.Stored) || (furniture_tmp.state == FurnitureState.Stored && furniture_j.state == FurnitureState.Stored))
                    {
                        if ((int) furniture_tmp.Type > (int) furniture_j.Type)
                        {
                            tmp = j;
                        }
                        else if (furniture_tmp.Type == furniture_j.Type)
                        {
                            if ((int) furniture_tmp.SubType > (int) furniture_j.SubType)
                            {
                                tmp = j;
                            }
                            else if (furniture_tmp.SubType == furniture_j.SubType)
                            {
                                if (furniture_tmp.level < furniture_j.level)
                                {
                                    tmp = j;
                                }
                                else if (furniture_tmp.level == furniture_j.level)
                                {
                                    if (furniture_tmp.tableIndex < furniture_j.tableIndex)
                                    {
                                        tmp = j;
                                    }
                                    else if (furniture_tmp.tableIndex == furniture_j.tableIndex)
                                    {
                                        if (index_tmp > index_j)
                                        {
                                            tmp = j;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            listStorageItemReal[tmp].gameObject.transform.SetSiblingIndex(i);
            ListItem_StorageData temp = listStorageItemReal[i];
            listStorageItemReal[i] = listStorageItemReal[tmp];
            listStorageItemReal[tmp] = temp;
        }
        if(listStorageItemReal.Count-1 >= 0) listStorageItemReal[listStorageItemReal.Count-1].gameObject.transform.SetSiblingIndex(listStorageItemReal.Count - 1);
    }
}
