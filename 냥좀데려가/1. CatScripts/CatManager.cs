using UnityEngine;
using FirebaseNetwork;

public class CatManager : MonoBehaviour
{
    public UI_CatDataList catListUI = null;        // 보유 고양이 정보 UI
    public UI_CatDataDetail catDetailUI = null;    // 보유 고양이별 상세 정보 UI    
    [SerializeField] private ArrangeCats arrangeCats = null;

    public static CatManager Instance = null;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        catListUI.CreateCatDataItem();
    }

    // 새로운 고양이 데이터 생성 (catTableIndex를 인자로 받음)
    public CatData CreateNewCat(int catTableIndex)
    {
        int idx = Authentication.Inst.userData.catCount;
        CatTable catTableData = TableDataManager.Instance.table_CatTable[catTableIndex];
        // 고양이 이름 랜덤 생성
        string name = TableDataManager.Instance.table_Cat_Name[Random.Range(0, TableDataManager.Instance.table_Cat_Name.Count)].Name;
        // 고양이 CatTable로부터 catlevelIndex 정보 받아옴
        int levelIndex = catTableData.Cat_Level_Index;
        // 고양이 레벨, 경험치, 가구 사용 유무 초기화
        int level = 1;
        int exp = 0;
        CatState state = CatState.Free;

        // 고양이 레벨 테이블에서 경험치 최대치, 총 스탯 최대치, 스탯별 최대치 정보 받아옴
        string keyLv = levelIndex.ToString() + "/" + level.ToString();
        Cat_Level catLevelData = TableDataManager.Instance.table_Cat_Level[keyLv];
        int maxCatStat = catLevelData.Cat_Stat_Per_Level;
        int[] catStatMax = new int[3] { catLevelData.Health_Max, catLevelData.Charm_Max, catLevelData.Sociability_Max };

        // 총 스탯 최대치와 스탯별 최대치에 따라 각 스탯 랜덤 배정
        int[] catStat = new int[3] { 0, 0, 0 };
        int[] catStatOrder = new int[3] { 0, 1, 2 };
        while (maxCatStat > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                int rn = Random.Range(i, 3);
                int temp = catStatOrder[i];
                catStatOrder[i] = catStatOrder[rn];
                catStatOrder[rn] = temp;
            }
            for (int i = 0; i < 3; i++)
            {
                int rdStat = Random.Range(1, catStatMax[catStatOrder[i]] + 1);
                catStat[catStatOrder[i]] += rdStat;
                maxCatStat -= rdStat;
                if (maxCatStat <= 0)
                {
                    catStat[catStatOrder[i]] += maxCatStat;
                    break;
                }
            }
        }

        return new CatData(idx, name, catTableIndex, level, exp, catStat, state);
    }

    // 보유 고양이 데이터 추가
    public void AddCat(CatData cat)
    {
        Authentication.Inst.userData.cats.Add(cat);
        MissionManager.Instance.MissionCountUpdate(MissionBehavior.KeepCat);
        NetworkMethod.SetCatData(cat);
        catListUI.AddCatDataItem();
    }

    // 보유 고양이 데이터 제거
    public void RemoveCat(int index)
    {
        catListUI.RemoveCatItem(index);
        Authentication.Inst.userData.cats.RemoveAt(index);
        MissionManager.Instance.MissionCountUpdate(MissionBehavior.KeepCat);
        MissionManager.Instance.MissionCountUpdate(MissionBehavior.CatLevel);
        NetworkMethod.EditCatData();
    }

    // 고양이 스탯 증가
    public void CatAddStat(int index, int[] addStat)
    {
        CatData cat = Authentication.Inst.userData.cats[index];
        for(int i=0; i<cat.stat.Length; i++)
        {
            cat.stat[i] += addStat[i];
        }

        NetworkMethod.SetCatStat(index, cat.stat);
    }

    // 고양이 경험치 증가
    public void CatAddExp(int index, int addExp)
    {
        CatData cat = Authentication.Inst.userData.cats[index];
        cat.exp += addExp;
        int maxExp = cat.MaxExp;
        while(cat.level < TableDataManager.Instance.table_Setting["Cat_Max_Level"].Value && cat.exp >= maxExp)
        {
            cat.exp -= cat.MaxExp;
            CatLevelUp(index);
            maxExp = cat.MaxExp;
        }
        NetworkMethod.SetCatExp(index, cat.exp);
    }

    // 고양이 레벨 증가
    public void CatLevelUp(int index)
    {
        CatData cat = Authentication.Inst.userData.cats[index];

        // 총 스탯 최대치와 스탯별 최대치에 따라 각 스탯 랜덤 배정
        int maxCatStat = cat.LevelStat;
        int[] catStat = new int[3] { 0, 0, 0 };
        int[] catStatOrder = new int[3] { 0, 1, 2 };
        while (maxCatStat > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                int rn = Random.Range(i, 3);
                int temp = catStatOrder[i];
                catStatOrder[i] = catStatOrder[rn];
                catStatOrder[rn] = temp;
            }
            for (int i = 0; i < 3; i++)
            {
                int rdStat = Random.Range(1, cat.StatMax[catStatOrder[i]] + 1);
                catStat[catStatOrder[i]] += rdStat;
                maxCatStat -= rdStat;
                if (maxCatStat <= 0)
                {
                    catStat[catStatOrder[i]] += maxCatStat;
                    break;
                }
            }
        }
        CatAddStat(index, catStat);
        cat.level++;
        MissionManager.Instance.MissionCountUpdate(MissionBehavior.CatLevel);
        NetworkMethod.SetCatLevel(index, cat.level);
        catListUI.ChangeLevel(index, cat.level);
    }

    // 고양이 상태 변경
    public void CatStateChange(int index, CatState state)
    {
        CatData cat = Authentication.Inst.userData.cats[index];
        CatState beforeState = cat.state;
        cat.state = state;
        if (Authentication.Inst.userData.endTutorial)
        {
            switch(state)
            {
                case CatState.Free:
                    if (beforeState == CatState.EndFurniture) MissionManager.Instance.MissionCountUp(MissionBehavior.EndFurnitureCat, cat.catTableIndex, cat.TypeIndex, cat.SubTypeIndex);
                    break;
                case CatState.InInterior:
                    MissionManager.Instance.MissionCountUp(MissionBehavior.ArrangeCatInterior, cat.catTableIndex, cat.TypeIndex, cat.SubTypeIndex);
                    break;
                case CatState.UsingFurniture:
                    MissionManager.Instance.MissionCountUp(MissionBehavior.ArrangeCatFurniture, cat.catTableIndex, cat.TypeIndex, cat.SubTypeIndex);
                    break;
            }
            NetworkMethod.SetCatState(index, cat.state);
        }
        catListUI.ChangeState(index, cat.state);
        catDetailUI.ChangeBTNPlace();
    }

    // 시설 배치 고양이수 변경
    public void CatInInteriorChange(int count)
    {
        Authentication.Inst.userData.catInInterior += count;
        NetworkMethod.SetCatInInterior(Authentication.Inst.userData.catInInterior);
        UserDataManager.Inst.SetCatCountUI();
    }

    // 고양이 시설에 배치하기
    public void ArrangeInInterior(int index)
    {
        CatData cat = Authentication.Inst.userData.cats[index];
        CatStateChange(index, CatState.InInterior);
        CatInInteriorChange(1);
        SoundManager.Instance.PlayEffect(SoundType.ArrangeCat);
        arrangeCats.SetData(cat);
    }
    // 고양이 가구에 배치하기
    private void ArrangeInFurniture(int index)
    {
        CatData cat = Authentication.Inst.userData.cats[index];
        StorageManager.Instance.ArrangeCatInFurniture(cat);
    }
    // 고양이 배치 취소하기
    public void StoreCat(int index, bool serverConnect = true)
    {
        bool success = false;
        CatData cat = Authentication.Inst.userData.cats[index];
        int[] addData;
        switch (cat.state)
        {
            case CatState.InInterior:
                arrangeCats.CancelData(cat);
                CatInInteriorChange(-1);
                success = true;
                break;
            case CatState.UsingFurniture:
                success = StorageManager.Instance.CancelArrangeCatInFurniture(cat, serverConnect);
                break;
            case CatState.EndFurniture:
                addData = StorageManager.Instance.EndArrangeCat(cat, serverConnect);
                if(addData[0] >= 0)
                {
                    int[] addStat = new int[3] { addData[0], addData[1], addData[2] };
                    int addExp = addData[3];
                    CatAddStat(index, addStat);
                    CatAddExp(index, addExp);
                    catDetailUI.UpgradeCat();
                    success = true;
                    SoundManager.Instance.PlayEffect(SoundType.Buff);
                }
                break;
        }
        if (success)
        {
            CatStateChange(index, CatState.Free);
        }
    }

    // 시설에 배치하기 버튼 클릭 시 동작
    public void OnClickInterior()
    {
        if(Authentication.Inst.userData.catInInterior < TableDataManager.Instance.table_Setting["Cat_Place_Max"].Value)
        {
            CatData cat = Authentication.Inst.userData.cats[catDetailUI.catIndex];
            ArrangeInInterior(catDetailUI.catIndex);
        }
        else
        {
            string str = TableDataManager.Instance.table_String["Cat/Warning/InteriorCount"].Contents[(int)UI_Setting.language];
            str = str.Replace("0", TableDataManager.Instance.table_Setting["Cat_Place_Max"].Value.ToString());
            UIManager.instance.uI_Warning.ShowPopUP(str);
        }
    }

    // 가구에 배치하기 버튼 클릭 시 동작
    public void OnClickFuniture()
    {
        CatData cat = Authentication.Inst.userData.cats[catDetailUI.catIndex];

        // 고양이 배치가 가능한 가구가 있는지 체크
        int possibleIndex = Authentication.Inst.userData.furnitures.FindIndex((furniture) => (furniture.Type == FurnitureType.FunctionFurniture && furniture.state == FurnitureState.Arranged && cat.level >= furniture.Table.Item_Condition_Level_Min));
        // 고양이 배치가 가능한 가구가 있는 경우 고양이 배치 진행
        if(possibleIndex != -1)
        {
            ArrangeInFurniture(catDetailUI.catIndex);
        }
        // 고양이 배치가 가능한 가구가 없는 경우 알림
        else
        {
            UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String["Cat/Warning/NoFurniture"].Contents[(int)UI_Setting.language]);
        }
    }

    // 시설/가구에 배치취소/완료 버튼 클릭 시 동작
    public void OnClickOut()
    {
        StoreCat(catDetailUI.catIndex);
    }

    // 고양이 이름 변경 UI 함수
    public void OnClickChangeCatNameUI()
    {
        catDetailUI.CatNameSetting();
        UIManager.instance.OpenPopUpChildControl(-2);
    }

    // 고양이 이름 변경 OK 버튼 함수
    public void OnClickChangeCatNameOK()
    {
        CatData cat = Authentication.Inst.userData.cats[catDetailUI.catIndex];
        cat.name = catDetailUI.GetNewCatName(); ;
        NetworkMethod.SetCatName(catDetailUI.catIndex, cat.name);
        catListUI.ChangeName(catDetailUI.catIndex, cat.name);
        catDetailUI.CatNameChange(cat.name);
        UIManager.instance.OpenPopUpChildControl(0);
        UIManager.instance.OpenPopUpChildControl(-1);
    }
}
