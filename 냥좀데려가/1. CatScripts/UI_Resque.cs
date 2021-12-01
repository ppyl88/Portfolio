using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class UI_Resque : MonoBehaviour
{
    [SerializeField] public UI_ResqueReward resqueRewardUI = null;  // 고양이 구조대 보상 UI

    [Header("EventResque")]
    [SerializeField] private GameObject eventResquePrefab = null;
    [SerializeField] private RectTransform contentEventResque = null;
    [SerializeField] private CanvasGroup imgUnderRank = null;
    [SerializeField] private TextMeshProUGUI txtUnderRank = null;
    [SerializeField] private CanvasGroup imgNoEventResque = null;
    [SerializeField] private TextMeshProUGUI txtNoEventResque = null;
    [SerializeField] private Button btnLeftEventResqueList = null;
    [SerializeField] private Button btnRightEventResqueList = null;
    private float speedLeftRight = 5000f;   // 이벤트 구조대 좌우 스크롤 속도
    private int curIndexEventResque;        // 이벤트 구조대 현재 보여지는 리스트 인덱스
    private List<int> eventResqueIndexes = new List<int>();
    private List<ListItem_EventResque> listEventResques = new List<ListItem_EventResque>();

    [Header("NormalResque")]
    [SerializeField] private Button btnNormalResque = null;
    [SerializeField] private TextMeshProUGUI txtNormalName = null;
    [SerializeField] private TextMeshProUGUI txtNormalDesc = null;
    [SerializeField] private Image imgIconNormal = null;
    [SerializeField] private TextMeshProUGUI txtNormalCost = null;
    private int normalResqueIndex;

    [Header("SpecialResque")]
    [SerializeField] private Button btnSpecialResque = null;
    [SerializeField] private TextMeshProUGUI txtSpecialName = null;
    [SerializeField] private TextMeshProUGUI txtSpecialDesc = null;
    [SerializeField] private Image imgIconSpecial = null;
    [SerializeField] public TextMeshProUGUI txtSpecialCost = null;
    public int specialResqueIndex;

    // 구조대 버튼 이벤트 (구조대 UI 화면 세팅)
    public void InitResqueUI()
    {
        SetNormalResque();
        SetSpecialResque();
        SetEventResque();
        InitNormalResque();
        InitSpecialResque();
        InitEventResque();
    }

    // 이벤트 구조 버튼 클릭 함수
    public void OnClickEventResque()
    {
        Dispatch(eventResqueIndexes[curIndexEventResque]);
    }

    // 노말 구조 버튼 클릭 함수
    public void OnClickNormalResque()
    {
        Dispatch(normalResqueIndex);
    }

    // 스페셜 구조 버튼 클릭 함수
    public void OnClickSpecialResque()
    {
        Dispatch(specialResqueIndex);
    }


    // 이벤트 구조대 좌측 화살표 버튼 이벤트
    public void OnClickbtnLeft()
    {
        OffRankMask();
        curIndexEventResque--;

        StartCoroutine(LeftRightAnimation(true));
    }

    // 이벤트 구조대 우측 화살표 버튼 이벤트
    public void OnClickbtnRight()
    {
        OffRankMask();
        curIndexEventResque++;

        StartCoroutine(LeftRightAnimation(false));
    }

    // 구조대 리스트 중 파견 가능한 일반 구조대 선별 (1개) - 여러개일 경우 Index가 높은 걸 채택
    private void SetNormalResque()
    {
        int curNormalResqueIndex = 0;
        foreach (int key in TableDataManager.Instance.table_Resque_Area.Keys)
        {
            if (key / 100 == 1 && Authentication.Inst.userData.level >= TableDataManager.Instance.table_Resque_Area[key].Level)
            {
                curNormalResqueIndex = key;
            }
        }
        normalResqueIndex = curNormalResqueIndex;
    }

    // 구조대 리스트 중 파견 가능한 특수 구조대 선별 (1개) - 여러개일 경우 Index가 높은 걸 채택
    private void SetSpecialResque()
    {
        int curSpecialResqueIndex = 0;
        foreach (int key in TableDataManager.Instance.table_Resque_Area.Keys)
        {
            if (key / 100 == 2 && Authentication.Inst.userData.level >= TableDataManager.Instance.table_Resque_Area[key].Level)
            {
                curSpecialResqueIndex = key;
            }
        }
        specialResqueIndex = curSpecialResqueIndex;
    }

    // 구조대 리스트 중 파견 가능한 이벤트 구조대 선별 (여러개 가능)
    private void SetEventResque()
    {
        string currentTime = TimeUtils.GetCurrentTimetoStr();
        List<int> curEventResqueIndexes = new List<int>();
        foreach (int key in TableDataManager.Instance.table_Resque_Area.Keys)
        {
            if (key / 100 == 3 && CompareTime(currentTime, TableDataManager.Instance.table_Resque_Area[key].Time_Start, TableDataManager.Instance.table_Resque_Area[key].Time_End))
            {
                curEventResqueIndexes.Add(key);
            }
        }
        eventResqueIndexes = curEventResqueIndexes;
    }

    // 일반 구조대 UI 세팅
    private void InitNormalResque()
    {
        Resque_Area resque = TableDataManager.Instance.table_Resque_Area[normalResqueIndex];
        txtNormalName.text = resque.ResqueName;
        txtNormalDesc.text = resque.ResqueDesc;
        imgIconNormal.sprite = resque.ResqueIcon;
        txtNormalCost.text = string.Format("{0:#,###}", resque.Price);
    }

    // 특수 구조대 UI 세팅
    private void InitSpecialResque()
    {
        Resque_Area resque = TableDataManager.Instance.table_Resque_Area[specialResqueIndex];
        txtSpecialName.text = resque.ResqueName;
        txtSpecialDesc.text = resque.ResqueDesc;
        imgIconSpecial.sprite = resque.ResqueIcon;
        txtSpecialCost.text = string.Format("{0:#,###}", resque.Price);
    }

    // 이벤트 구조대 UI 리스트 세팅
    private void InitEventResque()
    {
        // 기존 이벤트 구조대 UI 리스트 삭제 및 초기화
        for (int i = 0; i < listEventResques.Count; i++)
        {
            Destroy(listEventResques[i].gameObject);
        }
        listEventResques.Clear();
        Vector3 pos = contentEventResque.localPosition;
        pos.x += curIndexEventResque * contentEventResque.rect.width;
        contentEventResque.localPosition = pos;
        curIndexEventResque = 0;

        // 가능한 이벤트 구조대가 없을 경우
        if (eventResqueIndexes.Count == 0)
        {
            txtNoEventResque.text = TableDataManager.Instance.table_String["Resque/UI/NoEventResque"].Contents[(int)UI_Setting.language];
            imgNoEventResque.alpha = 1;
            imgNoEventResque.blocksRaycasts = true;
        }
        // 가능한 이벤트 구조대가 있을 경우
        else
        {
            imgNoEventResque.alpha = 0;
            imgNoEventResque.blocksRaycasts = false;
            btnLeftEventResqueList.interactable = false;
            // 이벤트 구조대 개수에 따른 좌우 버튼 세팅
            if (eventResqueIndexes.Count > 1)
            {
                btnRightEventResqueList.interactable = true;
            }
            else
            {
                btnRightEventResqueList.interactable = false;
            }
            // 이벤트 구조대 UI 리스트 생성
            for (int i = 0; i < eventResqueIndexes.Count; i++)
            {
                ListItem_EventResque resqueEventListItemGo = Instantiate(eventResquePrefab, contentEventResque).GetComponent<ListItem_EventResque>();
                Resque_Area resque = TableDataManager.Instance.table_Resque_Area[eventResqueIndexes[i]];

                // 관리용 목록에 추가
                listEventResques.Add(resqueEventListItemGo);
                resqueEventListItemGo.Init(eventResqueIndexes[i]);

                resqueEventListItemGo.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnClickEventResque();
                    SoundManager.Instance.PlayClick();
                });
            }
        }
    }
    private void CheckRank()
    {
        int rank = TableDataManager.Instance.table_Resque_Area[eventResqueIndexes[curIndexEventResque]].Level;
        // 레벨 부족 시 레벨 관련 마스크 생성
        if (Authentication.Inst.userData.level < rank)
        {
            string UnderRank = TableDataManager.Instance.table_String["Resque/UI/UnderRank"].Contents[(int)UI_Setting.language];
            UnderRank = UnderRank.Replace("0", rank.ToString());
            txtUnderRank.text = UnderRank;
            imgUnderRank.alpha = 1;
            imgUnderRank.blocksRaycasts = true;
        }
    }
    private void OffRankMask()
    {
        imgUnderRank.alpha = 0;
        imgUnderRank.blocksRaycasts = false;
    }

    // 구조대 파견용 재화 소모 함수
    private bool CostPrice(FeeType priceType, int price)
    {
        switch (priceType)
        {
            case FeeType.Gold:
                if (Authentication.Inst.userData.gold < price)
                {
                    string warning = TableDataManager.Instance.table_String["Resque/Warning/NoGold"].Contents[(int)UI_Setting.language];
                    UIManager.instance.uI_Warning.ShowPopUP(warning);
                    return false;
                }
                else
                {
                    UserDataManager.Inst.AddGold(-price);
                    return true;
                }
            case FeeType.Coin:
                if (Authentication.Inst.userData.coin < price)
                {
                    string warning = TableDataManager.Instance.table_String["Resque/Warning/NoCoin"].Contents[(int)UI_Setting.language];
                    UIManager.instance.uI_Warning.ShowPopUP(warning);
                    return false;
                }
                else
                {
                    UserDataManager.Inst.AddCoin(-price);
                    return true;
                }
            default:
                return false;
        }
    }

    // 구조대 파견용 재화 소모 함수 (오류 시 재화 복원을 위한 장치)
    private void RefundPrice(FeeType priceType, int price)
    {
        switch (priceType)
        {
            case FeeType.Gold:
                UserDataManager.Inst.AddGold(price);
                break;
            case FeeType.Coin:
                UserDataManager.Inst.AddCoin(price);
                break;
        }
    }

    // 구조대 파견 보상 중 확률에 따라 1개의 보상 선택
    public int CastResqueReward(int resqueIndex)
    {
        List<Resque_Reward> possibleResqueRewards = new List<Resque_Reward>();
        int rateSum = 0;
        int selectedCatTableIndex = -1;
        bool findmatch = false;
        foreach (string key in TableDataManager.Instance.table_Resque_Reward.Keys)
        {
            if(key.StartsWith(resqueIndex.ToString()))
            {
                Resque_Reward reward = TableDataManager.Instance.table_Resque_Reward[key];
                possibleResqueRewards.Add(reward);
                rateSum += reward.Rate;
                findmatch = true;
            }
            else if (findmatch)
            {
                break;
            }
        }
        // 보상 확률의 총량이 10000일 경우에만 보상 선택 진행
        if (rateSum == 10000)
        {
            int randomvalue = Random.Range(0, rateSum);
            int rateStack = 0;
            for (int i = 0; i < possibleResqueRewards.Count; i++)
            {
                rateStack += possibleResqueRewards[i].Rate;
                if (randomvalue < rateStack)
                {
                    selectedCatTableIndex = possibleResqueRewards[i].Cat_Index;
                    break;
                }
            }
        }
        // 보상 확률의 총량이 10000이 아닐 경우 오류를 출력
        else
        {
            string warning = TableDataManager.Instance.table_String["Resque/Warning/FailResque"].Contents[(int)UI_Setting.language];
            UIManager.instance.uI_Warning.ShowPopUP(warning);
        }
        return selectedCatTableIndex;
    }

    // 구조대 파견 버튼 이벤트 (가능 유무 파악 및 재화 소모)
    private void Dispatch(int resqueIndex)
    {
        Resque_Area resque = TableDataManager.Instance.table_Resque_Area[resqueIndex];
        string currentTime = TimeUtils.GetCurrentTimetoStr();
        if (resqueIndex / 100 == 3 && !CompareTime(currentTime, resque.Time_Start, resque.Time_End))
        {
            string warning = TableDataManager.Instance.table_String["Resque/Warning/EndEvent"].Contents[(int)UI_Setting.language];
            UIManager.instance.uI_Warning.ShowPopUP(warning);
        }
        else
        {
            if (CostPrice(resque.Price_Type, resque.Price))
            {
                int selectedCatTableIndex = CastResqueReward(resqueIndex);
                // 보상 선택이 제대로 이루어졌을 경우, 구조대 보상 UI로 넘어감
                if (selectedCatTableIndex != -1)
                {
                    CatTable catTable = TableDataManager.Instance.table_CatTable[selectedCatTableIndex];
                    MissionManager.Instance.MissionCountUp(MissionBehavior.ResqueCat, selectedCatTableIndex, catTable.TypeIndex, catTable.SubTypeIndex);
                    CatManager.Instance.AddCat(CatManager.Instance.CreateNewCat(selectedCatTableIndex));
                    resqueRewardUI.SetResqueReward();
                }
                // 보상 선택이 제대로 이루어 지지 않았을 경우, 재화 복원
                else
                {
                    RefundPrice(resque.Price_Type, resque.Price);
                }
            }
        }
    }

    // 구조대 시간 비교
    private bool CompareTime(string current, string start, string end)
    {
        DateTime currentTime = new DateTime(int.Parse(current.Substring(0, 4)), int.Parse(current.Substring(4, 2)), int.Parse(current.Substring(6, 2)), int.Parse(current.Substring(8, 2)), int.Parse(current.Substring(10, 2)), 0);

        double startTimediff = (currentTime - new DateTime(int.Parse(start.Substring(0, 4)), int.Parse(start.Substring(4, 2)), int.Parse(start.Substring(6, 2)), int.Parse(start.Substring(8, 2)), int.Parse(start.Substring(10, 2)), 0)).TotalMinutes;

        double endTimediff = -1f;
        if (end != "UnLimit")
        {
            endTimediff = (currentTime - new DateTime(int.Parse(end.Substring(0, 4)), int.Parse(end.Substring(4, 2)), int.Parse(end.Substring(6, 2)), int.Parse(end.Substring(8, 2)), int.Parse(end.Substring(10, 2)), 0)).TotalMinutes;
        }

        if (startTimediff >= 0 && endTimediff <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 이벤트 구조대 리스트 이동 애니메이션
    private IEnumerator LeftRightAnimation(bool isLeft)
    {
        bool isAnimation = true;
        Vector3 pos = contentEventResque.localPosition;
        float goal;
        btnLeftEventResqueList.interactable = false;
        btnRightEventResqueList.interactable = false;
        if (isLeft)
        {
            goal = pos.x + contentEventResque.rect.width;
            while (isAnimation)
            {
                pos.x += Time.deltaTime * speedLeftRight;
                contentEventResque.localPosition = pos;
                if (contentEventResque.localPosition.x >= goal) isAnimation = false;
                yield return null;
            }
        }
        else
        {
            goal = pos.x - contentEventResque.rect.width;
            while (isAnimation)
            {
                pos.x -= Time.deltaTime * speedLeftRight;
                contentEventResque.localPosition = pos;
                if (contentEventResque.localPosition.x <= goal) isAnimation = false;
                yield return null;
            }
        }

        pos.x = goal;
        contentEventResque.localPosition = pos;
        btnLeftEventResqueList.interactable = true;
        btnRightEventResqueList.interactable = true;
        if (curIndexEventResque == 0)
        {
            btnLeftEventResqueList.interactable = false;
        }
        else if (curIndexEventResque == listEventResques.Count - 1)
        {
            btnRightEventResqueList.interactable = false;
        }

        CheckRank();

        yield return null;
    }
}
