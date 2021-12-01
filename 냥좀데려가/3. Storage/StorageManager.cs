using System.Collections.Generic;
using UnityEngine;
using FirebaseNetwork;
using System.Collections;

public class StorageManager : MonoBehaviour
{
    public UI_StorageDataList storageDataListUI= null;
    public UI_StorageDataDetail storageDataDetailUI= null;
    [SerializeField] private UI_FurnitureUpgrade furnitureUpgradeUI= null;
    [SerializeField] private UI_StorageSell storageSellUI= null;
    [SerializeField] private UI_Buff buffUI= null;

    [SerializeField] private GameObject Wall= null;
    [SerializeField] private GameObject Floor= null;

    private Item_Table curBuffItem;
    private FurnitureData curBuffBowl;
    private FurnitureData curWallPaper;
    private FurnitureData curFloorMaterial;
    private float buffRemainTime = 0;

    private string strWarning = "Storage/Warning/";
    private string strYesorNo = "Storage/YesorNo/";

    public static StorageManager Instance = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        curBuffBowl = Authentication.Inst.userData.furnitures.Find((furniture) => furniture.idx == Authentication.Inst.userData.curBuffBowl);
        curWallPaper = Authentication.Inst.userData.furnitures.Find((furniture) => furniture.idx == Authentication.Inst.userData.curWallPaper);
        curFloorMaterial = Authentication.Inst.userData.furnitures.Find((furniture) => furniture.idx == Authentication.Inst.userData.curFloorMaterial);

        if (curWallPaper != null) PositionBackGround(curWallPaper.SubType);
        if (curFloorMaterial != null) PositionBackGround(curFloorMaterial.SubType);

        storageDataListUI.CreateStorageItem();
        ResetBuff();

        FurniturePlacement.Instance.InitFurniturePlacement();

        // 버프 세팅
        if (curBuffBowl != null && curBuffBowl.state == FurnitureState.UsingItem)
        {
            curBuffItem = TableDataManager.Instance.table_Item[curBuffBowl.arrangeIndex];
            buffRemainTime = curBuffBowl.arrangeTime + curBuffItem.Buff_Effect_Time * 60 - TimeUtils.GetCurrentTime();

            if (buffRemainTime >= 0)
            {
                buffUI.StartBuff(curBuffItem.Sprite);
                StartCoroutine(BuffUI());
            }
            else
            {
                EndBuff();
            }
        }
        else
        {
            curBuffItem = null;
        }
    }

    // 가구 추가 함수
    public void AddFuniture(int furnitureIndex)
    {
        FurnitureData furniture = new FurnitureData(furnitureIndex);
        Authentication.Inst.userData.furnitures.Add(furniture);
        NetworkMethod.SetFurnitureData(furniture);
        MissionManager.Instance.MissionCountUpdate(MissionBehavior.KeepFurniture);
        storageDataListUI.AddFunitureStorage(furniture);
    }

    // 가구 삭제 함수
    public void RemoveFuniture(FurnitureData furniture)
    {
        if (furniture.state == 0)
        {
            storageDataListUI.RemoveFunitureStorage(furniture);
            Authentication.Inst.userData.furnitures.Remove(furniture);
            NetworkMethod.EditFurnitureData();
            MissionManager.Instance.MissionCountUpdate(MissionBehavior.KeepFurniture);
            UIManager.instance.OpenPopUpChildControl(0);
        }
        else
        {
            string warning = TableDataManager.Instance.table_String[strWarning + "FailRemove"].Contents[(int)UI_Setting.language];
            UIManager.instance.uI_Warning.ShowPopUP(warning);
        }
    }

    // 가구 레벨업 함수
    private void UpgradeFuniture(FurnitureData furniture)
    {
        UserDataManager.Inst.AddGold(-furniture.UpgradeFee);
        furniture.level++;
        MissionManager.Instance.MissionCountUp(MissionBehavior.UpgradeFurniture, furniture.arrangeIndex, (int)furniture.Type, (int)furniture.SubType);
        NetworkMethod.SetFurnitureLevel(Authentication.Inst.userData.furnitures.IndexOf(furniture), furniture.level);
        storageDataListUI.ChangeLevel(furniture);
        storageDataDetailUI.ChangeLevel();
        SoundManager.Instance.PlayEffect(SoundType.Buff);
    }

    // 가구 상태 변경 함수
    public void ChangeFurnitureState(FurnitureData furniture, FurnitureState state, bool serverConnect = true)
    {
        FurnitureState beforeState = furniture.state;
        furniture.state = state;
        storageDataListUI.ChangeState(furniture);
        storageDataDetailUI.ChangeState();
        if (serverConnect)
        {
            switch(state)
            {
                case FurnitureState.Arranged:
                case FurnitureState.Dirty:
                    if (beforeState == FurnitureState.Stored) MissionManager.Instance.MissionCountUp(MissionBehavior.ArrangeFurniture, furniture.tableIndex, (int)furniture.Type, (int)furniture.SubType);
                    break;
                case FurnitureState.UsingItem:
                case FurnitureState.UsingCat:
                    MissionManager.Instance.MissionCountUp(MissionBehavior.UseFurniture, furniture.tableIndex, (int)furniture.Type, (int)furniture.SubType);
                    break;
                case FurnitureState.Cleaning:
                    MissionManager.Instance.MissionCountUp(MissionBehavior.CleanFurniture, furniture.tableIndex, (int)furniture.Type, (int)furniture.SubType);
                    break;
            }
            NetworkMethod.SetFurnitureState(Authentication.Inst.userData.furnitures.IndexOf(furniture), furniture.state);
        }
    }

    // 가구 아이템/고양이 배치 함수
    public void ChangeFurnitureInner(FurnitureData furniture, int arrangeIndex, int arrangeTime, int arrangeMarker, bool serverConnect = true)
    {
        furniture.arrangeIndex = arrangeIndex;
        furniture.arrangeTime = arrangeTime;
        furniture.arrangeMarker = arrangeMarker;
        if (serverConnect) NetworkMethod.SetFurnitureArrangeThing(Authentication.Inst.userData.furnitures.IndexOf(furniture), arrangeIndex, arrangeTime, arrangeMarker);
    }

    // 가구 위치 변경 함수
    /// 필요한 곳에서 불러와서 사용
    public void ChangeFuniturePosition(FurnitureData furniture, Vector2 position)
    {
        furniture.position = position;
        NetworkMethod.SetFurniturePos(Authentication.Inst.userData.furnitures.IndexOf(furniture), position);
    }

    // 가구 지저분함 추가 함수
    private void PlusFurnitureMess(FurnitureData furniture, bool serverConnect = true)
    {
        furniture.mess++;
        if(serverConnect)
        {
            NetworkMethod.SetFurnitureMess(Authentication.Inst.userData.furnitures.IndexOf(furniture), furniture.mess);
            if (furniture.mess > 0)
            {
                ChangeFurnitureState(furniture, FurnitureState.Dirty, serverConnect);
            }
        }
    }

    // 가구 지저분함 추가 함수
    public void ChangeFurnitureCleanTime(FurnitureData furniture, int cleanTime)
    {
        furniture.cleanTime = cleanTime;
        NetworkMethod.SetFurnitureCleanTime(Authentication.Inst.userData.furnitures.IndexOf(furniture), furniture.cleanTime);
    }

    // 가구 청소시작 함수
    public void CleanFurnitureStart(FurnitureData furniture)
    {
        ChangeFurnitureCleanTime(furniture, TimeUtils.GetCurrentTime());
        ChangeFurnitureState(furniture, FurnitureState.Cleaning, true);
    }
    
    // 가구 청소완료 함수
    public void CleanFurnitureEnd(FurnitureData furniture)
    {
        furniture.mess = 0;
        NetworkMethod.SetFurnitureMess(Authentication.Inst.userData.furnitures.IndexOf(furniture), 0);
        ChangeFurnitureCleanTime(furniture, -1);
        ChangeFurnitureState(furniture, FurnitureState.Arranged, true);
    }

    // 간식 그릇 배치 함수
    public void ChangeBuffBowl(FurnitureData furniture)
    {
        curBuffBowl = furniture;
        Authentication.Inst.userData.curBuffBowl = furniture.idx;
        NetworkMethod.SetCurBuffBowl(furniture.idx);
    }

    // 가구 배치 함수
    public void ArrangeFurniture(FurnitureData furniture, string UI)
    {
        FurniturePlacement.Instance.ArrangeFurniture(furniture, UI);
    }

    // 가구 보관 함수
    public void StoreFurniture(FurnitureData furniture, bool serverConnect = true)
    {
        switch (furniture.SubType)
        {
            case FurnitureSubType.BuffBowl:
                curBuffBowl = null;
                Authentication.Inst.userData.curBuffBowl = -1;
                NetworkMethod.SetCurBuffBowl(-1);
                break;
            default:
                break;
        }
        FurniturePlacement.Instance.StoreFurniture(furniture);
        ChangeFurnitureState(furniture, 0, serverConnect);
    }

    // 배경 배치 함수
    private void ArrangeBackGround(FurnitureData furniture)
    {
        switch (furniture.SubType)
        {
            case FurnitureSubType.WallPaper:
                curWallPaper = furniture;
                Authentication.Inst.userData.curWallPaper = furniture.idx;
                NetworkMethod.SetCurWallPaper(furniture.idx);
                ChangeFuniturePosition(furniture, new Vector2(0, (float)(1920 - furniture.SubSprite.rect.height / furniture.SubSprite.rect.width * 1080 * 3) / 200f));
                PositionBackGround(furniture.SubType);
                break;
            case FurnitureSubType.FloorMaterial:
                curFloorMaterial = furniture;
                Authentication.Inst.userData.curFloorMaterial = furniture.idx;
                NetworkMethod.SetCurFloorMaterial(furniture.idx);
                ChangeFuniturePosition(furniture, new Vector2(0, - (float)(1920 - furniture.SubSprite.rect.height / furniture.SubSprite.rect.width * 1080 * 3) / 200f));
                PositionBackGround(furniture.SubType);
                break;
        }
        ChangeFurnitureState(furniture, FurnitureState.Arranged, true);
    }

    // 배경 제거 함수
    private void StoreBackGround(FurnitureData furniture, bool serverConnect = true)
    {
        switch (furniture.SubType)
        {
            case FurnitureSubType.WallPaper:
                curWallPaper = null;
                Authentication.Inst.userData.curWallPaper = -1;
                NetworkMethod.SetCurWallPaper(-1);
                Wall.GetComponent<SpriteRenderer>().sprite = null;
                break;
            case FurnitureSubType.FloorMaterial:
                curFloorMaterial = null;
                Authentication.Inst.userData.curFloorMaterial = -1;
                NetworkMethod.SetCurFloorMaterial(-1);
                Floor.GetComponent<SpriteRenderer>().sprite = null;
                break;
        }
        ChangeFurnitureState(furniture, 0, serverConnect);
    }

    // 배경 포지션 함수
    private void PositionBackGround(FurnitureSubType type)
    {
        switch(type)
        {
            case FurnitureSubType.WallPaper:
                Wall.GetComponent<SpriteRenderer>().sprite = curWallPaper.SubSprite;
                Wall.GetComponent<Transform>().position = curWallPaper.position;
                break;
            case FurnitureSubType.FloorMaterial:
                Floor.GetComponent<SpriteRenderer>().sprite = curFloorMaterial.SubSprite;
                Floor.GetComponent<Transform>().position = curFloorMaterial.position;
                break;
        }
    }

    // 가구 배치 버튼 클릭 함수
    public void OnClickArrangeButton()
    {
        FurnitureData furniture = storageDataDetailUI.furnitureData;
        switch (furniture.SubType)
        {
            case FurnitureSubType.BuffBowl:
                if (curBuffBowl == null)
                {
                    if (UserDataManager.Inst.cntArrFurniture < TableDataManager.Instance.table_Setting["Furniture_Limited"].Value)
                    {
                        ArrangeFurniture(furniture, "Storage");
                    }
                    else
                    {
                        string warning = TableDataManager.Instance.table_String[strWarning + "ManyFurniture"].Contents[(int)UI_Setting.language];
                        warning = warning.Replace("0", TableDataManager.Instance.table_Setting["Furniture_Limited"].Value.ToString());
                        UIManager.instance.uI_Warning.ShowPopUP(warning);
                    }
                }
                else if (curBuffBowl.state == FurnitureState.Arranged)
                {
                    string yesno = TableDataManager.Instance.table_String[strYesorNo + "OnlyBuffBowl"].Contents[(int)UI_Setting.language];
                    yesno += "\n" + TableDataManager.Instance.table_String[strYesorNo + "ChangeBuffBowl"].Contents[(int)UI_Setting.language];
                    UIManager.instance.ui_YesNo.ShowPopUP(yesno, () => { StoreFurniture(curBuffBowl); ArrangeFurniture(furniture, "Storage"); });
                }
                else
                {
                    string yesno = TableDataManager.Instance.table_String[strYesorNo + "CancelBuff"].Contents[(int)UI_Setting.language];
                    yesno += "\n" + TableDataManager.Instance.table_String[strYesorNo + "ChangeBuffBowl"].Contents[(int)UI_Setting.language];
                    UIManager.instance.ui_YesNo.ShowPopUP(yesno, () => { EndBuff(); StoreFurniture(curBuffBowl); ArrangeFurniture(furniture, "Storage"); });
                }
                break;

            case FurnitureSubType.WallPaper:
                if (curWallPaper == null)
                {
                    ArrangeBackGround(furniture);
                }
                else
                {
                    string yesno = TableDataManager.Instance.table_String[strYesorNo + "ChangeInterior"].Contents[(int)UI_Setting.language];
                    yesno = yesno.Replace("0", furniture.SubTypeName);
                    UIManager.instance.ui_YesNo.ShowPopUP(yesno, () => { StoreBackGround(curWallPaper); ArrangeBackGround(furniture); });
                }
                break;
            case FurnitureSubType.FloorMaterial:
                if (curFloorMaterial == null)
                {
                    ArrangeBackGround(furniture);
                }
                else
                {
                    string yesno = TableDataManager.Instance.table_String[strYesorNo + "ChangeInterior"].Contents[(int)UI_Setting.language];
                    yesno = yesno.Replace("0", furniture.SubTypeName);
                    UIManager.instance.ui_YesNo.ShowPopUP(yesno, () => { StoreBackGround(curFloorMaterial); ArrangeBackGround(furniture); });
                }
                break;
            default:
                if (UserDataManager.Inst.cntArrFurniture < TableDataManager.Instance.table_Setting["Furniture_Limited"].Value)
                {
                    ArrangeFurniture(furniture, "Storage");
                }
                else
                {
                    string warning = TableDataManager.Instance.table_String[strWarning + "ManyFurniture"].Contents[(int)UI_Setting.language];
                    warning = warning.Replace("0", TableDataManager.Instance.table_Setting["Furniture_Limited"].Value.ToString());
                    UIManager.instance.uI_Warning.ShowPopUP(warning);
                }
                break;
        }
    }

    // 가구 즉시 설치 버튼 클릭 함수
    public void OnClickArrangeNowButton(FurnitureData furniture)
    {
        switch (furniture.SubType)
        {
            case FurnitureSubType.BuffBowl:
                if (curBuffBowl == null)
                {
                    if (UserDataManager.Inst.cntArrFurniture < TableDataManager.Instance.table_Setting["Furniture_Limited"].Value)
                    {
                        ArrangeFurniture(furniture, "Shop");
                    }
                    else
                    {
                        string warning = TableDataManager.Instance.table_String[strWarning + "ManyFurniture"].Contents[(int)UI_Setting.language];
                        warning = warning.Replace("0", TableDataManager.Instance.table_Setting["Furniture_Limited"].Value.ToString());
                        UIManager.instance.uI_Warning.ShowPopUP(warning);
                    }
                }
                else if (curBuffBowl.state == FurnitureState.Arranged)
                {
                    string yesno = TableDataManager.Instance.table_String[strYesorNo + "OnlyBuffBowl"].Contents[(int)UI_Setting.language];
                    yesno += "\n" + TableDataManager.Instance.table_String[strYesorNo + "ChangeBuffBowl"].Contents[(int)UI_Setting.language];
                    UIManager.instance.ui_YesNo.ShowPopUP(yesno, () => { StoreFurniture(curBuffBowl); ArrangeFurniture(furniture, "Shop"); });
                }
                else
                {
                    string yesno = TableDataManager.Instance.table_String[strYesorNo + "CancelBuff"].Contents[(int)UI_Setting.language];
                    yesno += "\n" + TableDataManager.Instance.table_String[strYesorNo + "ChangeBuffBowl"].Contents[(int)UI_Setting.language];
                    UIManager.instance.ui_YesNo.ShowPopUP(yesno, () => { EndBuff(); StoreFurniture(curBuffBowl); ArrangeFurniture(furniture, "Shop"); });
                }
                break;

            case FurnitureSubType.WallPaper:
                if (curWallPaper == null)
                {
                    ArrangeBackGround(furniture);
                    UIManager.instance.ClosePopUpUIExceptMainUI();
                }
                else
                {
                    string yesno = TableDataManager.Instance.table_String[strYesorNo + "ChangeInterior"].Contents[(int)UI_Setting.language];
                    yesno = yesno.Replace("0", furniture.SubTypeName);
                    UIManager.instance.ui_YesNo.ShowPopUP(yesno, () => { StoreBackGround(curWallPaper); ArrangeBackGround(furniture); UIManager.instance.ClosePopUpUIExceptMainUI(); });
                }
                break;
            case FurnitureSubType.FloorMaterial:
                if (curFloorMaterial == null)
                {
                    ArrangeBackGround(furniture);
                    UIManager.instance.ClosePopUpUIExceptMainUI();
                }
                else
                {
                    string yesno = TableDataManager.Instance.table_String[strYesorNo + "ChangeInterior"].Contents[(int)UI_Setting.language];
                    yesno = yesno.Replace("0", furniture.SubTypeName);
                    UIManager.instance.ui_YesNo.ShowPopUP(yesno, () => { StoreBackGround(curFloorMaterial); ArrangeBackGround(furniture); UIManager.instance.ClosePopUpUIExceptMainUI(); });
                }
                break;
            default:
                if (UserDataManager.Inst.cntArrFurniture < TableDataManager.Instance.table_Setting["Furniture_Limited"].Value)
                {
                    ArrangeFurniture(furniture, "Shop");
                }
                else
                {
                    string warning = TableDataManager.Instance.table_String[strWarning + "ManyFurniture"].Contents[(int)UI_Setting.language];
                    warning = warning.Replace("0", TableDataManager.Instance.table_Setting["Furniture_Limited"].Value.ToString());
                    UIManager.instance.uI_Warning.ShowPopUP(warning);
                }
                break;
        }
    }

    // 가구 보관 버튼 클릭 함수
    public void OnClickStoreButton()
    {
        FurnitureData furniture = storageDataDetailUI.furnitureData;
        string ment;
        switch (furniture.state)
        {
            case FurnitureState.UsingItem:
                ment = TableDataManager.Instance.table_String[strYesorNo + "AlreadyBuff"].Contents[(int)UI_Setting.language];
                ment += "\n" + TableDataManager.Instance.table_String[strYesorNo + "CancelArrange"].Contents[(int)UI_Setting.language];
                UIManager.instance.ui_YesNo.ShowPopUP(ment, () => { EndBuff(); StoreFurniture(furniture); });
                break;
            case FurnitureState.UsingCat:
                int catIndex = Authentication.Inst.userData.cats.FindIndex((cat) => cat.idx == furniture.arrangeIndex);
                ment = TableDataManager.Instance.table_String[strYesorNo + "AlreadyCat"].Contents[(int)UI_Setting.language];
                ment += "\n" + TableDataManager.Instance.table_String[strYesorNo + "CancelArrange"].Contents[(int)UI_Setting.language];
                UIManager.instance.ui_YesNo.ShowPopUP(ment, () => { CatManager.Instance.StoreCat(catIndex); StoreFurniture(furniture); });
                break;
            case FurnitureState.Cleaning:
                ment = TableDataManager.Instance.table_String[strWarning + "CantStoreCleaning"].Contents[(int)UI_Setting.language];
                UIManager.instance.uI_Warning.ShowPopUP(ment);
                break;
            default:
                if(furniture.SubType == FurnitureSubType.WallPaper || furniture.SubType == FurnitureSubType.FloorMaterial)
                {
                    StoreBackGround(furniture);
                }
                else
                {
                    StoreFurniture(furniture);
                }
                break;
        }
    }

    // 고양이 배치 함수
    /// 필요한 곳에서 불러와서 사용
    public void ArrangeCatInFurniture(CatData cat, bool isDirect = false)
    {
        // 고양이배치
        FurniturePlacement.Instance.ArrangeCat(cat, isDirect);
    }

    // 고양이 배치 취소 함수
    public bool CancelArrangeCatInFurniture(CatData cat, bool serverConnect)
    {
        bool success = false;
        FurnitureData furniture = Authentication.Inst.userData.furnitures.Find((fur) => (fur.state == FurnitureState.UsingCat && fur.arrangeIndex == cat.idx));
        if (furniture != null)
        {
            ChangeFurnitureInner(furniture, -1, -1, -1, serverConnect);
            ChangeFurnitureState(furniture, FurnitureState.Arranged, serverConnect);
            PlusFurnitureMess(furniture, serverConnect);
            FurniturePlacement.Instance.RemoveCat(furniture);
            success = true;
        }
        else
        {
            string warning = TableDataManager.Instance.table_String[strWarning + "FailCancelCat"].Contents[(int)UI_Setting.language];
            UIManager.instance.uI_Warning.ShowPopUP(warning);
        }
        return success;
    }

    // 고양이 배치 완료 함수
    public int[] EndArrangeCat(CatData cat, bool serverConnect)
    {
        int[] addData = new int[4] { 0, 0, 0, 0 };
        FurnitureData furniture = Authentication.Inst.userData.furnitures.Find((fur) => (fur.state == FurnitureState.UsingCat && fur.arrangeIndex == cat.idx));
        if (furniture != null)
        {
            // 기본 스탯 증가량
            int originalStat = Mathf.RoundToInt((float)furniture.UseTime / (float)furniture.StatPerMinute * (float)furniture.StatValue);
            // 보너스 스탯 증가량
            int bonusStat = 0;
            // 누적 버프 효과 적용
            for (int i = 0; i < Authentication.Inst.userData.stackBuffs.Count; i++)
            {
                BuffData buff = Authentication.Inst.userData.stackBuffs[i];
                Item_Table buffItem = TableDataManager.Instance.table_Item[buff.itemIndex];
                if (buffItem.SnackType == furniture.StatType && buff.startTime < furniture.EndTime && buff.endTime > furniture.StartTime)
                {
                    int bonusStartTime = buff.startTime;
                    int bonusEndTime = buff.endTime;
                    if(buff.startTime < furniture.StartTime)
                    {
                        bonusStartTime = furniture.StartTime;
                    }
                    if(buff.endTime > furniture.EndTime)
                    {
                        bonusEndTime = furniture.EndTime;
                    }
                    float bonusTime = (float)(bonusEndTime - bonusStartTime) / 60f / (float)furniture.StatPerMinute;
                    int bonus = Mathf.RoundToInt(bonusTime * (float)furniture.StatValue * (buffItem.Buff_Effect_Value - 1));
                    bonusStat += bonus;
                }
            }
            if(curBuffItem != null)
            {
                int buffEndTime = TimeUtils.GetCurrentTime();
                if (curBuffItem.SnackType == furniture.StatType && curBuffBowl.StartTime < furniture.EndTime && buffEndTime > furniture.StartTime)
                {
                    int bonusStartTime = curBuffBowl.StartTime;
                    int bonusEndTime = buffEndTime;
                    if (curBuffBowl.StartTime < furniture.StartTime)
                    {
                        bonusStartTime = furniture.StartTime;
                    }
                    if (buffEndTime > furniture.EndTime)
                    {
                        bonusEndTime = furniture.EndTime;
                    }
                    float bonusTime = (float)(bonusEndTime - bonusStartTime) / 60f / (float)furniture.StatPerMinute;
                    int bonus = Mathf.RoundToInt(bonusTime * (float)furniture.StatValue * (curBuffItem.Buff_Effect_Value - 1));
                    bonusStat += bonus;
                }
            }
            addData[(int)furniture.StatType - 1] = originalStat + bonusStat;
            addData[3] = furniture.ExpValue;
            if (CancelArrangeCatInFurniture(cat, serverConnect))
            {
                // Debug.Log("기본 스탯 증가 : " + furniture.StatType + originalStat);
                // Debug.Log("보너스 스탯 증가 : " + furniture.StatType + bonusStat);
                return addData;
            }
            else
            {
                return new int[1] { -1 };
            }
        }
        else
        {
            string warning = TableDataManager.Instance.table_String[strWarning + "FailCancelCat"].Contents[(int)UI_Setting.language];
            UIManager.instance.uI_Warning.ShowPopUP(warning);
            return new int[1] { -1 };
        }
    }

    // 아이템 추가 함수
    public void AddItem(int itemIndex, int count = 1, bool connect = true)
    {
        ItemData item = Authentication.Inst.userData.items.Find((it) => it.index == itemIndex);
        if (item != null)
        {
            item.count += count;
            if(connect) NetworkMethod.SetItemCount(Authentication.Inst.userData.items.IndexOf(item), item.count);
            storageDataListUI.ChangeCount(item);
        }
        else
        {
            item = new ItemData(itemIndex, count);
            Authentication.Inst.userData.items.Add(item);
            if(connect) NetworkMethod.SetItemData(item);
            storageDataListUI.AddItemStorage(item);
        }
        if (connect)
        {
            MissionManager.Instance.MissionCountUp(MissionBehavior.GetItem, item.index, (int)item.Type, 0, count);
            MissionManager.Instance.MissionCountUpdate(MissionBehavior.KeepItem);
        }
    }

    // 아이템 삭제 함수
    private void RemoveItem(ItemData item, int count = 1)
    {
        item.count -= count;
        if(item.count > 0)
        {
            NetworkMethod.SetItemCount(Authentication.Inst.userData.items.IndexOf(item), item.count);
            storageDataListUI.ChangeCount(item);
            storageDataDetailUI.ChangeCount();
        }
        else
        {
            storageDataListUI.RemoveItemStorage(item);
            Authentication.Inst.userData.items.Remove(item);
            NetworkMethod.EditItemData();
            UIManager.instance.OpenPopUpChildControl(0);
        }
        MissionManager.Instance.MissionCountUpdate(MissionBehavior.KeepItem);
    }

    // 아이템 사용 버튼 함수
    public void OnClickUseButton()
    {
        ItemData item = storageDataDetailUI.itemData;
        if (curBuffBowl == null)
        {
            string warning = TableDataManager.Instance.table_String[strWarning + "NoBuffBowl"].Contents[(int)UI_Setting.language];
            UIManager.instance.uI_Warning.ShowPopUP(warning);
        }
        else if (curBuffBowl.state == FurnitureState.Arranged)
        {
            RemoveItem(item);
            StartBuff(item.index);
        }
        else
        {
            string yesno = TableDataManager.Instance.table_String[strYesorNo + "AlreadyBuff"].Contents[(int)UI_Setting.language];
            yesno += "\n" + TableDataManager.Instance.table_String[strYesorNo + "CancelBuff"].Contents[(int)UI_Setting.language];
            yesno += "\n" + TableDataManager.Instance.table_String[strYesorNo + "UseNewItem"].Contents[(int)UI_Setting.language];
            UIManager.instance.ui_YesNo.ShowPopUP(yesno,
            () =>
            {
                RemoveItem(item);
                EndBuff();
                StartBuff(item.index);
            });
        }
    }

    // 아이템 버프 시작 함수
    private void StartBuff(int itemIndex)
    {
        FurniturePlacement.Instance.ChangeFurnitureImage(curBuffBowl, true);
        curBuffItem = TableDataManager.Instance.table_Item[itemIndex];

        SoundManager.Instance.PlayEffect(SoundType.FeedCat);
        ChangeFurnitureInner(curBuffBowl, itemIndex, TimeUtils.GetCurrentTime(), -1);
        ChangeFurnitureState(curBuffBowl, FurnitureState.UsingItem);
        buffRemainTime = curBuffItem.Buff_Effect_Time * 60;
        buffUI.StartBuff(curBuffItem.Sprite);
        MissionManager.Instance.MissionCountUp(MissionBehavior.UseItem, itemIndex, (int) curBuffItem.Item_Type, 0);
        StartCoroutine(BuffUI());
    }

    // 아이템 버프 시간 종료 함수
    private void EndBuff(bool serverConnect = true)
    {
        FurniturePlacement.Instance.ChangeFurnitureImage(curBuffBowl, false);
        int endTime = curBuffBowl.arrangeTime + (int)(curBuffItem.Buff_Effect_Time * 60);
        int currentTime = TimeUtils.GetCurrentTime();
        if (currentTime < endTime)
        {
            StackBuff(curBuffBowl.arrangeTime, currentTime, curBuffBowl.arrangeIndex);
        }
        else
        {
            StackBuff(curBuffBowl.arrangeTime, endTime, curBuffBowl.arrangeIndex);
        }
        ChangeFurnitureState(curBuffBowl, FurnitureState.Arranged, serverConnect);
        ChangeFurnitureInner(curBuffBowl, -1, -1, -1, serverConnect);
        curBuffItem = null;
        buffUI.EndBuff();
    }

    // 버프 누적
    private void StackBuff(int startTime, int endTime, int itemIndex)
    {
        BuffData buff = new BuffData(startTime, endTime, itemIndex);
        NetworkMethod.SetBuffData(buff);
        Authentication.Inst.userData.stackBuffs.Add(buff);
    }

    // 누적 버프 리셋
    private void ResetBuff()
    {
        List<BuffData> buffs = Authentication.Inst.userData.stackBuffs;
        Authentication.Inst.userData.stackBuffs = new List<BuffData>();
        NetworkMethod.ResetBuffData();
        int startBuffIndex = buffs.Count;
        int tmpBuffIndex = startBuffIndex;
        for (int i = 0; i < Authentication.Inst.userData.furnitures.Count; i++)
        {
            FurnitureData furniture = Authentication.Inst.userData.furnitures[i];
            if (furniture.state == FurnitureState.UsingCat)
            {
                tmpBuffIndex = buffs.FindIndex((buff) => furniture.arrangeTime < buff.startTime + TableDataManager.Instance.table_Item[buff.itemIndex].Buff_Effect_Time);
                if(tmpBuffIndex != -1 && tmpBuffIndex < startBuffIndex)
                {
                    startBuffIndex = tmpBuffIndex;
                }
            }
        }
        for(int i = startBuffIndex; i<buffs.Count; i++)
        {
            BuffData buff = buffs[i];
            Authentication.Inst.userData.stackBuffs.Add(buff);
            NetworkMethod.SetBuffData(buff);
        }
    }

    // 서브 패널 닫기 함수 (보관함 화면 및 보관함 디테일 화면 유지)
    public void CloseSubPanel()
    {
        UIManager.instance.OpenPopUpChildControl(0);
        UIManager.instance.OpenPopUpChildControl(-1);
    }
    
    // 판매하기 버튼 클릭 함수
    public void OnClickSellUI()
    {
        if (storageDataDetailUI.isItem)
        {
            ItemData item = storageDataDetailUI.itemData;
            storageSellUI.SetSellUI(item);
            UIManager.instance.OpenPopUpChildControl(-3);
        }
        else
        {
            FurnitureData furniture = storageDataDetailUI.furnitureData;
            string ment;
            // 판매 여부 재확인
            switch (furniture.state)
            {
                case FurnitureState.Stored:
                    ment = TableDataManager.Instance.table_String[strYesorNo + "Sell"].Contents[(int)UI_Setting.language];
                    ment = ment.Replace("0", furniture.SellingPrice.ToString());
                    UIManager.instance.ui_YesNo.ShowPopUP(ment, () => { SellFurniture(furniture); });
                    break;
                case FurnitureState.UsingItem:
                    ment = TableDataManager.Instance.table_String[strYesorNo + "AlreadyBuff"].Contents[(int)UI_Setting.language];
                    ment += "\n" + TableDataManager.Instance.table_String[strYesorNo + "CanelArrange&"].Contents[(int)UI_Setting.language];
                    ment += "\n" + TableDataManager.Instance.table_String[strYesorNo + "Sell"].Contents[(int)UI_Setting.language];
                    ment = ment.Replace("0", furniture.SellingPrice.ToString());
                    UIManager.instance.ui_YesNo.ShowPopUP(ment, () => { EndBuff(false); StoreFurniture(furniture, false); SellFurniture(furniture); });
                    break;
                case FurnitureState.UsingCat:
                    int catIndex = Authentication.Inst.userData.cats.FindIndex((cat) => cat.idx == storageDataDetailUI.furnitureData.arrangeIndex);
                    ment = TableDataManager.Instance.table_String[strYesorNo + "AlreadyCat"].Contents[(int)UI_Setting.language];
                    ment += "\n" + TableDataManager.Instance.table_String[strYesorNo + "CanelArrange&"].Contents[(int)UI_Setting.language];
                    ment += "\n" + TableDataManager.Instance.table_String[strYesorNo + "Sell"].Contents[(int)UI_Setting.language];
                    ment = ment.Replace("0", furniture.SellingPrice.ToString());
                    UIManager.instance.ui_YesNo.ShowPopUP(ment, () => { CatManager.Instance.StoreCat(catIndex, false); StoreFurniture(furniture, false); SellFurniture(furniture); });
                    break;
                case FurnitureState.Cleaning:
                    ment = TableDataManager.Instance.table_String[strWarning + "CantSellCleaning"].Contents[(int)UI_Setting.language];
                    UIManager.instance.uI_Warning.ShowPopUP(ment);
                    break;
                default:
                    ment = TableDataManager.Instance.table_String[strYesorNo + "AlreadyArrange"].Contents[(int)UI_Setting.language];
                    ment += "\n" + TableDataManager.Instance.table_String[strYesorNo + "CanelArrange&"].Contents[(int)UI_Setting.language];
                    ment += "\n" + TableDataManager.Instance.table_String[strYesorNo + "Sell"].Contents[(int)UI_Setting.language];
                    ment = ment.Replace("0", furniture.SellingPrice.ToString());
                    if (furniture.SubType == FurnitureSubType.WallPaper || furniture.SubType == FurnitureSubType.FloorMaterial)
                    {
                        UIManager.instance.ui_YesNo.ShowPopUP(ment, () => { StoreBackGround(furniture, false); SellFurniture(furniture); });
                    }
                    else
                    {
                        UIManager.instance.ui_YesNo.ShowPopUP(ment, () => { StoreFurniture(furniture, false); SellFurniture(furniture); });
                    }
                    break;
            }
        }
    }

    // 가구 판매 함수
    public void SellFurniture(FurnitureData furniture)
    {
        SoundManager.Instance.PlayEffect(SoundType.GainMoney);
        RemoveFuniture(furniture);
        UserDataManager.Inst.AddGold(furniture.SellingPrice);
    }

    // 판매 금액 버튼 클릭 함수
    public void OnClickSellItem()
    {
        CloseSubPanel();
        UserDataManager.Inst.AddGold(storageSellUI.finalprice);
        ItemData item = storageDataDetailUI.itemData;
        MissionManager.Instance.MissionCountUp(MissionBehavior.SellItem, item.index, (int)item.Type, 0, storageSellUI.count);
        RemoveItem(storageDataDetailUI.itemData, storageSellUI.count);
    }

    // 강화 패널 열기 함수
    public void OpenUpgradeUI()
    {
        furnitureUpgradeUI.SetUpgradeUI(storageDataDetailUI.furnitureData);
        UIManager.instance.OpenPopUpChildControl(-2);
    }

    // 강화하기 함수
    public void OnClickUpgrade()
    {
        FurnitureData furniture = storageDataDetailUI.furnitureData;
        UpgradeFuniture(furniture);
        CloseSubPanel();
    }


    // 버프 UI 코루틴
    private IEnumerator BuffUI()
    {
        while (true)
        {
            if (curBuffItem == null) break;
            float progress = buffRemainTime / (float)(curBuffItem.Buff_Effect_Time * 60);
            buffUI.SetProgress(progress);
            if (progress <= 0)
            {
                EndBuff();
                yield break;
            }
            buffRemainTime -= Time.deltaTime;
            yield return null;
        }
    }

    // 어플을 내렸다 올렸을 때 버프 시간 재연동
    private void OnApplicationPause(bool pause)
    {
        if(!pause)
        {
            // 버프 세팅
            if (curBuffBowl != null && curBuffBowl.state == FurnitureState.UsingItem)
            {
                buffRemainTime = curBuffBowl.arrangeTime + curBuffItem.Buff_Effect_Time * 60 - TimeUtils.GetCurrentTime();
                if (buffRemainTime <= 0)
                {
                    EndBuff();
                }
            }
        }
    }
}
