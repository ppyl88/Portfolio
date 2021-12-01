using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurniturePlacement : MonoBehaviour
{
    [SerializeField] private Camera camera_main = null;                             // 메인 카메라
    [SerializeField] private ShopUIManager shopUIManger = null;                     // Shop UI 매니저
    [SerializeField] private AbleListScrollView ableListView = null;                // 배치가능 가구/고양이 리스트
    [SerializeField] private GameObject prefabItemFurniture = null;                 // 배치 가구 프리팹
    [SerializeField] private GameObject canvas_Placement = null;                    // 배치화면 캔버스
    [SerializeField] private CanvasGroup panelAble = null;                          // 배치가능 가구/고양이 패널
    [SerializeField] private GameObject catPointer = null;                          // 고양이 포인터
    [SerializeField] private List<SpriteRenderer> placeAreas = null;                // 고양이 포인터

    [SerializeField] private CanvasGroup btnLeftRight = null;                       // 좌우 이동 버튼
    [SerializeField] private CanvasGroup areaLeftRight = null;                      // 좌우 이동 영역
    [SerializeField] private CanvasGroup btnEndArrange = null;                      // 배치 종료 버튼
    [SerializeField] public CanvasGroup btnStoreCat = null;                         // 고양이 보관 버튼
    [SerializeField] public CanvasGroup btnStoreFurniture = null;                   // 가구 보관 버튼

    [SerializeField] private GameObject prefabHairBall = null;                      // 헤어볼 프리팹
    public Transform hairBallPlacement = null;                                      // 헤어볼 트랜스폼
    public List<HairBall> hairBalls = new List<HairBall>();                         // 헤어볼 리스트
    public List<Item_Table> itemHairBalls = new List<Item_Table>();                 // 헤어볼 아이템 리스트
    public int lifeTimeHairBall;                                                    // 헤어볼 라이프 타임
    public int maxHairBall;                                                         // 헤어볼 최대 개수

    private List<Item_Furniture> itemFurnitures = new List<Item_Furniture>();       // 가구 리스트
    private List<Item_Furniture> floorFurnitures = new List<Item_Furniture>();      // 바닥 가구 리스트
    private List<AbleData> ables = new List<AbleData>();                            // 배치 가능 가구/고양이 리스트

    public float xlim;                                                              // x축 한계점
    public float ylim;                                                              // y축 한계점
    public int selectedIndex = -1;                                                  // 선택된 가구/고양이 인덱스
    public int selectedMarker = -1;                                                 // 선택된 가구 마커
    public CatData selectedCat = null;                                              // 선택된 고양이
    public bool toStore;                                                            // 가구/고양이 보관 여부
    public bool movingFurniture;                                                    // 가구 포인터 이동중 여부
    public bool movingCat;                                                          // 고양이 포인터 이동중 여부
    public bool pause;                                                              //
    private string restoreUI;                                                       // 
    private bool currentMain;                                                       // 메인 화면에서 진입 여부

    // 기본 스트링 테이블 변수
    private string strUI = "Placement/UI/";
    private string strYesorNo = "Placement/YesorNo/";
    private string strWarning = "Placement/Warning/";

    // 싱글톤 변수
    public static FurniturePlacement Instance = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        foreach (int key in TableDataManager.Instance.table_Item.Keys)
        {
            Item_Table itemTable = TableDataManager.Instance.table_Item[key];
            if (itemTable.Item_Type == ItemType.HairBall)
            {
                itemHairBalls.Add(itemTable);
            }
        }
        lifeTimeHairBall = TableDataManager.Instance.table_Setting["HairBall_Lifetime"].Value;
        maxHairBall = TableDataManager.Instance.table_Setting["HairBall_Max"].Value;
    }

    #region Init
    // 가구 배치 초기화 함수
    public void InitFurniturePlacement()
    {
        xlim = 1080f * 3 * 0.005f;
        ylim = 1920f * 0.005f;

        selectedIndex = -1;
        selectedCat = null;
        toStore = false;
        
        // 최대 가구 배치수에 맞춰 가구 프리팹 생성 및 초기화
        for (int i = 0; i < TableDataManager.Instance.table_Setting["Furniture_Limited"].Value; i++)
        {
            Item_Furniture item = Instantiate(prefabItemFurniture, transform).GetComponent<Item_Furniture>();
            item.index = i;
            item.gameObject.SetActive(false);
            itemFurnitures.Add(item);
        }

        // 최대 헤어볼 생성 가능수에 맞춰 헤어볼 프리팹 생성
        for (int i = 0; i< maxHairBall; i++)
        {
            HairBall hairBall = Instantiate(prefabHairBall, hairBallPlacement).GetComponent<HairBall>();
            hairBall.gameObject.SetActive(false);
            hairBalls.Add(hairBall);
        }

        // 배치 가구 정보에 따른 가구 프리팹 세팅
        for (int i = 0; i < Authentication.Inst.userData.furnitures.Count; i++)
        {
            FurnitureData furniture = Authentication.Inst.userData.furnitures[i];
            if (furniture.SubType != FurnitureSubType.WallPaper && furniture.SubType != FurnitureSubType.FloorMaterial)
            {
                if (furniture.state != FurnitureState.Stored)
                {
                    Item_Furniture fur = ArrangeFurniture(furniture, null);
                    if (furniture.state == FurnitureState.UsingCat)
                    {
                        CatData cat = Authentication.Inst.userData.cats.Find(temp => temp.idx == furniture.arrangeIndex);
                        fur.ArrangeCat(cat, furniture.arrangeMarker);
                    }
                    if (furniture.state == FurnitureState.UsingItem)
                    {
                        ChangeFurnitureImage(furniture, true);
                    }
                }
            }
        }

        // 배치 가능 가구/고양이 리스트 초기화
        ableListView.Init();
    }
    #endregion

    #region Canvas Setting
    // 가구 배치 화면으로 이동하는 함수
    public void SetIntoArrageCanvas(bool into)
    {
        if (into)
        {
            currentMain = UIManager.instance.isMain;
            if (!currentMain) UIManager.instance.isMain = true;
        }
        else UIManager.instance.isMain = currentMain;

        canvas_Placement.SetActive(into);
        hairBallPlacement.gameObject.SetActive(!into);
        pause = false;
        for (int i = 0; i < UserDataManager.Inst.cntArrFurniture; i++)
        {
            itemFurnitures[i].IntoArrageCanvas(into);
            itemFurnitures[i].ActiveFurnitureBtn(false);
        }
        if (into)
        {
            UIManager.instance.HideAllUI();
        }
        else
        {
            if (selectedIndex != -1)
            {
                itemFurnitures.Find(temp => temp.index == selectedIndex).ActiveArrangeMenu(false);
            }
            switch (restoreUI)
            {
                case "Storage":
                    UIManager.instance.OpenMainUI();
                    UIManager.instance.OpenPopUpUIByIndex(5);
                    UIManager.instance.OpenPopUpChildControl(-1);
                    break;
                case "Shop":
                    UIManager.instance.OpenMainUI();
                    UIManager.instance.OpenPopUpUIByIndex(7);
                    shopUIManger.OnClickToStore();
                    break;
                case "CatInfo":
                    UIManager.instance.OpenMainUI();
                    UIManager.instance.OpenPopUpUIByIndex(2);
                    UIManager.instance.OpenPopUpChildControl(-1);
                    break;
                case "Main":
                    UIManager.instance.OpenMainUI();
                    break;
            }
        }
    }

    // 모든 가구 버튼 활성화 함수
    private void ActiveBtnFurnitureAll(bool active)
    {
        for (int i = 0; i < UserDataManager.Inst.cntArrFurniture; i++)
        {
            itemFurnitures[i].ActiveFurnitureBtn(active);
        }
    }

    // 가구 보관 세팅
    public void SetToStore(bool active)
    {
        toStore = active;
    }
    #endregion

    #region Furniture Arrangement
    // 가구 배치 함수 (시작 시점에 따라 구분)
    public Item_Furniture ArrangeFurniture(FurnitureData furniture, string UI)
    {
        Item_Furniture newFur = itemFurnitures[UserDataManager.Inst.cntArrFurniture];
        newFur.gameObject.SetActive(true);
        newFur.SetData(furniture);
        if (newFur.furniture.AreaType == AreaType.Floor)
        {
            newFur.curOrder = -1;
            floorFurnitures.Add(newFur);
        }
        UserDataManager.Inst.cntArrFurniture++;
        // 시작 시 가구 배치 세팅
        if (UI == null)
        {
            newFur.transform.position = furniture.position;
            if (newFur.furniture.AreaType == AreaType.Floor) SortingFurniture(newFur);
            else if (newFur.furniture.Type == FurnitureType.FunctionFurniture) newFur.ChangeGroundPosition();
        }
        // 배치가능 가구 리스트에서 바로 배치 시
        else if (UI == "List")
        {
            newFur.IntoArrageCanvas(true);
            selectedIndex = newFur.index;
            ViewAble(false);
            StartCoroutine(FurnitureMoving(true));
        }
        // 그 외 UI에서 접근 시
        else
        {
            selectedIndex = newFur.index;
            restoreUI = UI;
            SetIntoArrageCanvas(true);
            StartCoroutine(FurnitureMoving());
        }
        return newFur;
    }

    // 메인 화면에서 가구 배치 함수
    public void ArrangeCurrentFurniture(int index)
    {
        string yesno = TableDataManager.Instance.table_String[strYesorNo + "ToArrange"].Contents[(int)UI_Setting.language];
        UIManager.instance.ui_YesNo.ShowPopUP(yesno, () =>
        {
            selectedIndex = index;
            restoreUI = "Main";
            SetIntoArrageCanvas(true);
            StartCoroutine(FurnitureMoving());
        });
    }

    // 가구 보관 함수
    public void StoreFurniture(FurnitureData furniture)
    {
        Item_Furniture fur = itemFurnitures.Find(temp => temp.furniture == furniture);
        if (fur.furniture.AreaType == AreaType.Floor) floorFurnitures.Remove(fur);
        fur.ResetData();
        fur.gameObject.SetActive(false);
        itemFurnitures.Add(fur);
        itemFurnitures.Remove(fur);
        UserDataManager.Inst.cntArrFurniture--;
    }

    // 가구 상태에 따른 이미지 변경 함수 (간식그릇류)
    public void ChangeFurnitureImage(FurnitureData furniture, bool isSub)
    {
        Item_Furniture fur = itemFurnitures.Find(temp => temp.furniture == furniture);
        fur.ChangeSprite(isSub);
    }

    // 가구 배치화면에서 가구 선택 함수
    public void SelectFurniture(int index)
    {
        if (selectedIndex != -1)
        {
            itemFurnitures.Find(temp => temp.index == selectedIndex).ActiveArrangeMenu(false);
        }
        selectedIndex = index;
    }

    // 가구 배치 순서 정렬
    private void SortingFurniture(Item_Furniture fur)
    {
        int curOrder = fur.curOrder;
        int curIndex = floorFurnitures.IndexOf(fur);
        floorFurnitures.Remove(fur);
        float posy = fur.transform.position.y - fur.furniture.Sprite.rect.height * 0.005f;
        int index = -1;
        if (floorFurnitures.Count == 0) index = 0;
        for (int i = 0; i < floorFurnitures.Count; i++)
        {
            float posy_cmp = floorFurnitures[i].transform.position.y - floorFurnitures[i].furniture.Sprite.rect.height * 0.005f;
            if (posy > posy_cmp)
            {
                index = i;
                break;
            }
        }
        if (index == -1) index = floorFurnitures.Count;

        floorFurnitures.Insert(index, fur);
        if (curOrder != index * 6)
        {
            if (index < curIndex)
            {
                for (int i = index; i <= curIndex; i++)
                {
                    floorFurnitures[i].ChangeOrderLayer(i * 6);
                }
            }
            else
            {
                for (int i = curIndex; i <= index; i++)
                {
                    floorFurnitures[i].ChangeOrderLayer(i * 6);
                }
            }
        }
    }
    #endregion

    #region Cat Arrangement
    // 고양이 배치 함수
    public void ArrangeCat(CatData cat, bool direct)
    {
        selectedCat = cat;
        if (!direct)
        {
            SetIntoArrageCanvas(true);
            restoreUI = "CatInfo";
        }
        StartCoroutine(CatArranging(direct));
    }

    // 고양이 배치 종료 함수
    public void RemoveCat(FurnitureData furniture)
    {
        Item_Furniture furnitureObject = itemFurnitures.Find(temp => temp.furniture == furniture);
        furnitureObject.RemoveCat();
    }
    #endregion

    #region HairBall Function
    // 헤어볼 추가 함수
    public void AddHairBall(int index, int order, Vector2 _parentPosition, GameObject _parentObject, GameObject _groundObject)
    {
        HairBall newHairBall = hairBalls[UserDataManager.Inst.cntHairBall];
        newHairBall.transform.position = _parentPosition;
        newHairBall.SetHairBall(itemHairBalls[index], order, UserDataManager.Inst.cntHairBall+1, _parentObject, _groundObject);
        UserDataManager.Inst.cntHairBall++;
    }

    // 헤어볼 제거 함수
    public void RemoveHairBall(HairBall hairBall)
    {
        hairBall.gameObject.SetActive(false);
        hairBalls.Add(hairBall);
        hairBalls.Remove(hairBall);
        UserDataManager.Inst.cntHairBall--;
    }
    #endregion

    #region Able List
    // 배치가능 가구/고양이 리스트 활성화 함수
    public void ViewAble(bool active)
    {
        panelAble.alpha = active ? 1 : 0;
        panelAble.blocksRaycasts = active;
        if (active) ableListView.SetData(ables);
    }

    // 배치가능 가구 리스트업 함수
    public void AbleFurnitures()
    {
        ables.Clear();
        for (int i = 0; i < Authentication.Inst.userData.furnitures.Count; i++)
        {
            FurnitureData furniture = Authentication.Inst.userData.furnitures[i];
            if (furniture.SubType != FurnitureSubType.WallPaper && furniture.SubType != FurnitureSubType.FloorMaterial && furniture.state == FurnitureState.Stored)
            {
                if (furniture.Type == FurnitureType.BuffBowl)
                {
                    if (Authentication.Inst.userData.curBuffBowl == -1)
                    {
                        ables.Add(new AbleData(furniture));
                    }
                }
                else
                {
                    ables.Add(new AbleData(furniture));
                }
            }
        }
        ableListView.MaskMessage("");
        if (ables.Count == 0)
        {
            string ui = TableDataManager.Instance.table_String[strUI + "NoFurniture"].Contents[(int)UI_Setting.language];
            ableListView.MaskMessage(ui);
        }
        else if (UserDataManager.Inst.cntArrFurniture >= TableDataManager.Instance.table_Setting["Furniture_Limited"].Value)
        {
            string ui = TableDataManager.Instance.table_String[strUI + "MaxFurniture"].Contents[(int)UI_Setting.language];
            ableListView.MaskMessage(ui);
        }
        ViewAble(true);
    }

    // 배치가능 고양이 리스트업 함수
    public void AbleCats()
    {
        ables.Clear();
        int cntFur = 0;
        int minLv = 10;
        for (int i = 0; i < Authentication.Inst.userData.cats.Count; i++)
        {
            CatData cat = Authentication.Inst.userData.cats[i];
            if (cat.state == CatState.Free && cat.level < TableDataManager.Instance.table_Setting["Cat_Max_Level"].Value)
            {
                ables.Add(new AbleData(cat));
            }
        }
        ableListView.MaskMessage("");
        if (ables.Count == 0)
        {
            string ui = TableDataManager.Instance.table_String[strUI + "NoCat"].Contents[(int)UI_Setting.language];
            ableListView.MaskMessage(ui);
        }
        else
        {
            for (int i = 0; i < Authentication.Inst.userData.furnitures.Count; i++)
            {
                FurnitureData furniture = Authentication.Inst.userData.furnitures[i];
                if (furniture.Type == FurnitureType.FunctionFurniture && furniture.state == FurnitureState.Arranged)
                {
                    cntFur++;
                    if (furniture.MinCatLevel < minLv)
                    {
                        minLv = furniture.MinCatLevel;
                    }
                }
            }
            if (cntFur == 0)
            {
                string ui = TableDataManager.Instance.table_String[strUI + "NoCatFurniture"].Contents[(int)UI_Setting.language];
                ableListView.MaskMessage(ui);
            }
            else
            {
                List<AbleData> lvCats = new List<AbleData>();
                for (int i = 0; i < ables.Count; i++)
                {
                    if (ables[i].cat.level >= minLv)
                    {
                        lvCats.Add(ables[i]);
                    }
                }
                if (lvCats.Count == 0)
                {
                    string ui = TableDataManager.Instance.table_String[strUI + "LowLevelCat"].Contents[(int)UI_Setting.language];
                    ableListView.MaskMessage(ui);
                }
                else
                {
                    ables = lvCats;
                }
            }
        }

        ViewAble(true);
    }
    #endregion

    #region State Setting
    // 배치 가구 상태 전체 변경
    private void ChangeArrangeStateAll(FurniturePlacementState state)
    {
        for (int i = 0; i < UserDataManager.Inst.cntArrFurniture; i++)
        {
            itemFurnitures[i].ChangeState(state);
        }
    }

    // 고양이 배치 가능 가구 상태 전체 변경
    private void ChangeArrangeStateCatFurnitures(CatData cat)
    {
        for (int i = 0; i < UserDataManager.Inst.cntArrFurniture; i++)
        {
            FurnitureData furniture = itemFurnitures[i].furniture;
            if (furniture.Type == FurnitureType.FunctionFurniture && furniture.state == FurnitureState.Arranged && cat.level >= furniture.Table.Item_Condition_Level_Min)
            {
                itemFurnitures[i].ChangeState(FurniturePlacementState.Arrangable);
            }
            else
            {
                itemFurnitures[i].ChangeState(FurniturePlacementState.Default);
            }
        }
    }
    #endregion

    #region Moving and Arranging
    // 가구/고양이 이동 배치 화면 세팅
    private void MoveorArrange(bool isStart)
    {
        btnEndArrange.alpha = isStart ? 0 : 1;
        btnEndArrange.blocksRaycasts = !isStart;
        if(UI_Setting.screenMove == 0)
        {
            btnLeftRight.alpha = isStart ? 0 : 1;
            btnLeftRight.blocksRaycasts = !isStart;
        }
        areaLeftRight.alpha = isStart ? 1 : 0;
        areaLeftRight.blocksRaycasts = isStart;
        ViewAble(!isStart);
    }

    // 가구 이동 시작 세팅 함수
    private Item_Furniture StartFurnitureMove()
    {
        movingFurniture = true;
        toStore = false;
        btnStoreFurniture.alpha = 1;
        btnStoreFurniture.blocksRaycasts = true;
        ActiveBtnFurnitureAll(false);
        MoveorArrange(true);
        Item_Furniture fur = itemFurnitures.Find(temp => temp.index == selectedIndex);
        fur.ChangeState(FurniturePlacementState.Moving);
        fur.ChangeLayerPointer(true);
        if (fur.furniture.AreaType == AreaType.Floor || fur.furniture.AreaType == AreaType.Mat)
        {
            ViewPlaceArea(true, "Floor");
        }
        else
        {
            ViewPlaceArea(true, fur.furniture.AreaType.ToString());
        }
        for (int i = 0; i < UserDataManager.Inst.cntArrFurniture; i++)
        {
            itemFurnitures[i].ActiveBtnMoving(false);
        }
        return fur;
    }

    // 가구 이동 종료 세팅 함수
    private void EndFurnitureMove(Item_Furniture fur)
    {
        selectedIndex = -1;
        movingFurniture = false;
        toStore = false;
        btnStoreFurniture.alpha = 0;
        btnStoreFurniture.blocksRaycasts = false;
        ActiveBtnFurnitureAll(true);
        MoveorArrange(false);
        AbleFurnitures();
        fur.ChangeLayerPointer(false);
        ViewPlaceArea(false);
        fur.ChangeState(FurniturePlacementState.Default);
        for (int i = 0; i < UserDataManager.Inst.cntArrFurniture; i++)
        {
            itemFurnitures[i].ActiveBtnMoving(true);
        }
    }

    // 가구 배치 가능 구역 표시 함수
    private void ViewPlaceArea(bool active, string _tag = null)
    {
        if (active) placeAreas.Find(temp => temp.tag == _tag).color = new Color(1, 1, 0, 0.39f);
        else
        {
            for (int i = 0; i < placeAreas.Count; i++)
            {
                placeAreas[i].color = new Color(1, 1, 0, 0);
            }
        }
    }

    // 고양이 배치 시작 세팅 함수
    private void StartCatArrange()
    {
        selectedIndex = -1;
        movingCat = true;
        toStore = false;
        btnStoreCat.alpha = 1;
        btnStoreCat.blocksRaycasts = true;
        MoveorArrange(true);
        catPointer.GetComponent<catPointer>().SetData(selectedCat);
        ChangeArrangeStateCatFurnitures(selectedCat);
    }

    // 고양이 배치 종료 세팅 함수
    private void EndCatArrange()
    {
        selectedIndex = -1;
        movingCat = false;
        selectedCat = null;
        toStore = false;
        btnStoreCat.alpha = 0;
        btnStoreCat.blocksRaycasts = false;
        MoveorArrange(false);
        AbleCats();
        ChangeArrangeStateAll(FurniturePlacementState.Default);
        catPointer.SetActive(false);
        selectedIndex = -1;
        selectedCat = null;
    }

    // 가구 배치 포인터 제어
    public IEnumerator FurnitureMoving(bool isStart = false)
    {
        Item_Furniture fur = StartFurnitureMove();
        bool moving = true;
        bool chasing = true;
        bool curStore = false;
        ChaseMouse(fur.gameObject);
        while (moving)
        {
            if (chasing && Input.GetMouseButtonDown(0))
            {
                isStart = true;
            }
            if (isStart && chasing && Input.GetMouseButton(0))
            {
                ChaseMouse(fur.gameObject);
                if (curStore != toStore)
                {
                    if (toStore)
                    {
                        fur.ChangeState(FurniturePlacementState.StoreFurniture);
                    }
                    else
                    {
                        fur.ExitStore();
                    }
                    curStore = toStore;
                }
            }
            else if (isStart && chasing && Input.GetMouseButtonUp(0))
            {
                chasing = false;
                if (fur.state == FurniturePlacementState.Moving)
                {
                    switch (fur.furniture.AreaType)
                    {
                        case AreaType.Ceiling:
                            fur.transform.position = new Vector3(fur.transform.position.x, ylim - fur.furniture.Sprite.rect.height * 0.005f, 0);
                            break;
                        case AreaType.Door:
                            fur.transform.position = new Vector3(fur.transform.position.x, 3.3f + fur.furniture.Sprite.rect.height * 0.005f, 0);
                            break;
                        case AreaType.Stair:
                            fur.transform.position = new Vector3(-xlim + fur.furniture.Sprite.rect.width * 0.005f, ylim - fur.furniture.Sprite.rect.height * 0.005f, 0);
                            break;
                    }
                    string yesno = TableDataManager.Instance.table_String[strYesorNo + "ArrangeFurniture"].Contents[(int)UI_Setting.language];
                    UIManager.instance.ui_YesNo.ShowPopUP(yesno, () =>
                    {
                        if (fur.furniture.state == FurnitureState.Stored)
                        {
                            if (fur.furniture.Type == FurnitureType.BuffBowl) StorageManager.Instance.ChangeBuffBowl(fur.furniture);
                            if (fur.furniture.mess > 0)
                            {
                                StorageManager.Instance.ChangeFurnitureState(fur.furniture, FurnitureState.Dirty, true);
                                fur.ActiveClean(true);
                            }
                            else StorageManager.Instance.ChangeFurnitureState(fur.furniture, FurnitureState.Arranged);
                        }
                        StorageManager.Instance.ChangeFuniturePosition(fur.furniture, fur.transform.position);
                        if (fur.furniture.AreaType == AreaType.Floor) SortingFurniture(fur);
                        else if (fur.furniture.Type == FurnitureType.FunctionFurniture) fur.ChangeGroundPosition();
                        EndFurnitureMove(fur);
                        moving = false;
                    }, () => { chasing = true; isStart = false; });
                }
                else if (fur.state == FurniturePlacementState.CantPlace)
                {
                    fur.transform.position = fur.furniture.position;
                    chasing = true;
                }
                else if (fur.state == FurniturePlacementState.StoreFurniture)
                {
                    if (fur.furniture.state == FurnitureState.Stored)
                    {
                        StoreFurniture(fur.furniture);
                        EndFurnitureMove(fur);
                        moving = false;
                    }
                    else if (fur.furniture.state == FurnitureState.Arranged || fur.furniture.state == FurnitureState.Dirty)
                    {
                        StorageManager.Instance.StoreFurniture(fur.furniture);
                        EndFurnitureMove(fur);
                        moving = false;
                    }
                    else
                    {
                        string warning = TableDataManager.Instance.table_String[strWarning + "CantStore"].Contents[(int)UI_Setting.language];
                        UIManager.instance.uI_Warning.ShowPopUP(warning, () => { chasing = true; isStart = false; });
                    }
                }
            }
            yield return null;
        }
    }

    // 고양이 배치 포인터 제어
    public IEnumerator CatArranging(bool isStart)
    {
        StartCatArrange();
        bool selected = false;
        bool chasing = true;
        ChaseMouse(catPointer);
        catPointer _catPointer = catPointer.GetComponent<catPointer>();
        while (!selected)
        {
            if (chasing && Input.GetMouseButtonDown(0))
            {
                isStart = true;
            }
            else if (isStart && chasing && Input.GetMouseButton(0))
            {
                ChaseMouse(catPointer);
                if (toStore)
                {
                    _catPointer.ChangeCatPointerState(CatPointerState.StoreCat);
                }
                else if (_catPointer.state == CatPointerState.StoreCat)
                {
                    _catPointer.ChangeCatPointerState(CatPointerState.OutStore);
                }
            }
            else if (isStart && chasing && Input.GetMouseButtonUp(0))
            {
                chasing = false;
                if (_catPointer.state == CatPointerState.StoreCat)
                {
                    EndCatArrange();
                    selected = true;
                }
                else if (_catPointer.state == CatPointerState.Selected)
                {
                    Item_Furniture fur = itemFurnitures.Find(temp => temp.index == selectedIndex);
                    int i_marker = selectedMarker;
                    string yesno = TableDataManager.Instance.table_String[strYesorNo + "ArrangeCat"].Contents[(int)UI_Setting.language];
                    UIManager.instance.ui_YesNo.ShowPopUP(yesno, () =>
                    {
                        fur.ArrangeCat(selectedCat, i_marker);
                        SoundManager.Instance.PlayEffect(SoundType.ArrangeCat);
                        StorageManager.Instance.ChangeFurnitureInner(fur.furniture, selectedCat.idx, TimeUtils.GetCurrentTime(), i_marker);
                        StorageManager.Instance.ChangeFurnitureState(fur.furniture, FurnitureState.UsingCat);
                        CatManager.Instance.CatStateChange(Authentication.Inst.userData.cats.IndexOf(selectedCat), CatState.UsingFurniture);
                        EndCatArrange();
                        selected = true;
                    }, () => { chasing = true; isStart = false; });
                }
                else
                {
                    chasing = true;
                }
            }
            yield return null;
        }
    }

    // 마우스 제어
    private void ChaseMouse(GameObject gameObject)
    {
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        Vector3 objPosition = camera_main.ScreenToWorldPoint(mousePosition);
        float x_r = gameObject.GetComponent<SpriteRenderer>().sprite.rect.width * 0.005f;
        float x_l = x_r;
        float y_u = gameObject.GetComponent<SpriteRenderer>().sprite.rect.height * 0.005f;
        float y_d = y_u + 1.1f;
        if (gameObject.GetComponent<Item_Furniture>() != null)
        {
            FurnitureArea areaFurniture = gameObject.GetComponent<Item_Furniture>().furniture.AreaFurniture;
            float colx_r = areaFurniture.size.x * 0.5f + areaFurniture.offset.x;
            float colx_l = areaFurniture.size.x * 0.5f - areaFurniture.offset.x;
            float coly_u = areaFurniture.size.y * 0.5f + areaFurniture.offset.y;
            float coly_d = areaFurniture.size.y * 0.5f - areaFurniture.offset.y;
            x_r = x_r >= colx_r ? x_r : colx_r;
            x_l = x_l >= colx_l ? x_l : colx_l;
            y_u = y_u >= coly_u ? y_u : coly_u;
            y_d = y_d >= coly_d ? y_d : coly_d;
        }
        if (objPosition.x > (xlim - x_r))
        {
            objPosition = new Vector3(xlim - x_r, objPosition.y, objPosition.z);
        }
        else if (objPosition.x < -(xlim - x_l))
        {
            objPosition = new Vector3(-(xlim - x_l), objPosition.y, objPosition.z);
        }
        if (objPosition.y > (ylim - y_u))
        {
            objPosition = new Vector3(objPosition.x, ylim - y_u, objPosition.z);
        }
        else if (objPosition.y < -(ylim - y_d))
        {
            objPosition = new Vector3(objPosition.x, -(ylim - y_d), objPosition.z);
        }
        gameObject.transform.position = objPosition;
    }
    #endregion

    #region Tutorial
    public void TutorialArrangeFurniture(FurnitureData furniture)
    {
        Item_Furniture newFur = itemFurnitures[UserDataManager.Inst.cntArrFurniture];
        // Debug.Log(UserDataManager.Inst.cntArrFurniture);
        newFur.gameObject.SetActive(true);
        newFur.SetData(furniture);
        floorFurnitures.Add(newFur);
        selectedIndex = newFur.index;
        restoreUI = "Shop";
        UserDataManager.Inst.cntArrFurniture++;
        SetIntoArrageCanvas(true);
        StartCoroutine(TutorialFurnitureMoving());
    }

    // 가구 배치 포인터
    public IEnumerator TutorialFurnitureMoving()
    {
        Item_Furniture fur = StartFurnitureMove();
        bool moving = true;
        bool chasing = true;
        bool isStart = false;
        ChaseMouse(fur.gameObject);
        while (moving)
        {
            if (!pause)
            {
                if (chasing && Input.GetMouseButtonDown(0))
                {
                    isStart = true;
                }
                if (isStart && chasing && Input.GetMouseButton(0))
                {
                    ChaseMouse(fur.gameObject);
                }
                else if (isStart && chasing && Input.GetMouseButtonUp(0))
                {
                    chasing = false;
                    if (fur.state == FurniturePlacementState.Moving)
                    {
                        string yesno = TableDataManager.Instance.table_String[strYesorNo + "ArrangeFurniture"].Contents[(int)UI_Setting.language];
                        UIManager.instance.ui_YesNo.ShowPopUP(yesno, () =>
                        {
                            StorageManager.Instance.ChangeFurnitureState(fur.furniture, FurnitureState.Arranged, false);
                            fur.furniture.position = fur.transform.position;
                            SortingFurniture(fur);
                            EndFurnitureMove(fur);
                            moving = false;
                        }, () => { chasing = true; isStart = false; });
                    }
                    else if (fur.state == FurniturePlacementState.CantPlace)
                    {
                        fur.transform.position = fur.furniture.position;
                        chasing = true;
                    }
                }
            }
            yield return null;
        }
    }

    public void TutorialArrangeCat(CatData cat)
    {
        selectedCat = cat;
        SetIntoArrageCanvas(true);
        restoreUI = "CatInfo";
        StartCoroutine(TutorialCatArranging());
    }

    public IEnumerator TutorialCatArranging()
    {
        StartCatArrange();
        bool selected = false;
        bool chasing = true;
        bool isStart = false;
        ChaseMouse(catPointer);
        catPointer _catPointer = catPointer.GetComponent<catPointer>();
        while (!selected)
        {
            if (!pause)
            {
                if (chasing && Input.GetMouseButtonDown(0))
                {
                    isStart = true;
                }
                else if (isStart && chasing && Input.GetMouseButton(0))
                {
                    ChaseMouse(catPointer);
                }
                else if (isStart && chasing && Input.GetMouseButtonUp(0))
                {
                    chasing = false;
                    if (_catPointer.state == CatPointerState.Selected)
                    {
                        Item_Furniture fur = itemFurnitures.Find(temp => temp.index == selectedIndex);
                        int i_marker = selectedMarker;
                        string yesno = TableDataManager.Instance.table_String[strYesorNo + "ArrangeCat"].Contents[(int)UI_Setting.language];
                        UIManager.instance.ui_YesNo.ShowPopUP(yesno, () =>
                        {
                            fur.ArrangeCat(selectedCat, i_marker);
                            SoundManager.Instance.PlayEffect(SoundType.ArrangeCat);
                            StorageManager.Instance.ChangeFurnitureInner(fur.furniture, selectedCat.idx, TimeUtils.GetCurrentTime(), i_marker, false);
                            StorageManager.Instance.ChangeFurnitureState(fur.furniture, FurnitureState.UsingCat, false);
                            CatManager.Instance.CatStateChange(0, CatState.UsingFurniture);
                            EndCatArrange();
                            selected = true;
                        }, () => { chasing = true; isStart = false; });
                    }
                    else
                    {
                        chasing = true;
                    }
                }
            }
            yield return null;
        }
    }
    #endregion
}
