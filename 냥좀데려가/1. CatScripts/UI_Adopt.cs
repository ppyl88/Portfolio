using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FirebaseNetwork;

public class UI_Adopt : MonoBehaviour
{
    [SerializeField] private UI_AdoptableCat adoptableCatUI= null;  // 입양 고양이 선택 UI
    [SerializeField] private UI_AdoptReward adoptRewardUI= null;    // 입양 보상 UI

    [Header("UI Setting")]
    [SerializeField] private GameObject adoptListPrefab= null;
    [SerializeField] private RectTransform contentAdoptList= null;
    [SerializeField] private CanvasGroup imgNoAdoptList= null;
    [SerializeField] private CanvasGroup[] btnTags= null;
    [SerializeField] private Image[] imgTags = null;
    [SerializeField] private Button btnLeft= null;
    [SerializeField] private Button btnRight= null;
    [SerializeField] private CanvasGroup imgAdoptDisable= null;

    public Adopt_List curAdopt;
    public int selectedCatIndex;
    public int curIndex;
    private List<ListItem_Adopt> listAdopts = new List<ListItem_Adopt>();

    private void Start()
    {
        CreateAdoptList();
    }

    // 입양 UI 클릭 함수 (입양 신청서들 출력)
    public void InitAdoptUI()
    {
        bool isChange = StackNewAdoptList();
        if(isChange) CreateAdoptList();
        else if(Authentication.Inst.userData.adopt.adoptIndex.Count > 0) ResectCatData(curIndex);
        ExistCatMask();
    }

    public void ExistCatMask()
    {
        if(Authentication.Inst.userData.adopt.adoptIndex.Count != 0)
        {
            listAdopts[curIndex].ActiveNoCat(!adoptableCatUI.existCat);
        }
    }

    // 입양 버튼 클릭 함수
    public void OnClickAdopt()
    {
        UserDataManager.Inst.CheckAdvertiseReset();
        int adoptIndex = Authentication.Inst.userData.adopt.adoptIndex[curIndex];
        curAdopt = TableDataManager.Instance.table_Adopt_List[adoptIndex];
        adoptRewardUI.Init(adoptIndex, selectedCatIndex);
        UIManager.instance.OpenPopUpChildControl(-2);
        SoundManager.Instance.PlayEffect(SoundType.ByeCat);
    }

    // 입양 완료 함수
    private void EndAdopt()
    {
        CatData cat = Authentication.Inst.userData.cats[selectedCatIndex];
        MissionManager.Instance.MissionCountUp(MissionBehavior.AdoptCat, cat.catTableIndex, cat.TypeIndex, cat.SubTypeIndex);
        CatManager.Instance.RemoveCat(selectedCatIndex);
        RemoveAdopt();
    }

    // 일반 입양 보상 클릭 함수
    public void OnClickAdoptReward()
    {
        GainReward();
        EndAdopt();
        SoundManager.Instance.PlayEffect(SoundType.Buff);
        UIManager.instance.OpenPopUpChildControl(0);
    }

    // 광고 입양 보상 클릭 함수
    public void OnClickAdoptAdvertise()
    {
        AdMobManager.Inst.ShowAd(() => {

            GainReward(curAdopt.Advertisement);
            EndAdopt();
            SoundManager.Instance.PlayEffect(SoundType.Buff);
            UIManager.instance.ui_Advertise.ShowPopUP(curAdopt.Advertisement, curAdopt.Rewards, () => {
                UIManager.instance.OpenPopUpChildControl(0);
            });
        });
    }

    // 입양 신청서 삭제 함수
    public void RemoveAdopt()
    {
        if(Authentication.Inst.userData.adopt.adoptIndex.Count >= TableDataManager.Instance.table_Adopt[Authentication.Inst.userData.level].Max_Adopt_List)
        {
            SetLastCreationTime(TimeUtils.GetCurrentTime());
        }
        StackNewAdoptList();
        RemoveAdoptIndex(curIndex);
        CreateAdoptList();
    }

    // 고양이 추가 버튼 함수
    public void OnClickPlusCat()
    {
        // 고양이 추가 화면 활성화
        UIManager.instance.OpenPopUpChildControl(-1);
    }

    // 고양이 추가 화면 선택 취소 버튼 함수
    public void OnClickCancelSelectCat()
    {
        // 고양이 추가 취소 시퀸스
        adoptableCatUI.Cancel();

        // 고양이 추가 화면 비활성화
        UIManager.instance.OpenPopUpChildControl(0);
    }

    // 고양이 추가 화면 선택 적용 버튼 함수
    public void OnClickSelectButton()
    {
        // 선택된 고양이 데이터 세팅
        selectedCatIndex = adoptableCatUI.Apply();
        listAdopts[curIndex].SetCatData(Authentication.Inst.userData.cats[selectedCatIndex]);
        imgAdoptDisable.alpha = 0;
        imgAdoptDisable.blocksRaycasts = false;

        // 고양이 추가 화면 비활성화
        UIManager.instance.OpenPopUpChildControl(0);
    }

    // 왼쪽 화살표 버튼 함수
    public void OnClickBtnLeft()
    {
        OnClickBtnTag(curIndex - 1);
    }

    // 오른쪽 화살표 버튼 함수
    public void OnClickBtnRight()
    {
        OnClickBtnTag(curIndex + 1);
    }

    // 고양이 데이터 리셋 함수
    private void ResectCatData(int index)
    {
        // 고양이 데이터 리셋
        listAdopts[curIndex].ResetCatData();
        adoptableCatUI.ResetCatList();
        adoptableCatUI.Init(listAdopts[index].adoptIndex);
        imgAdoptDisable.alpha = 1;
        imgAdoptDisable.blocksRaycasts = true;
    }

    // 태그 버튼 함수
    public void OnClickBtnTag(int index)
    {
        ResectCatData(index);

        // 기존 태그 활성화
        btnTags[curIndex].alpha = 1;
        btnTags[curIndex].blocksRaycasts = true;

        // 화면 전환
        Vector3 pos = contentAdoptList.localPosition;
        pos.x += contentAdoptList.rect.width * (curIndex - index);
        contentAdoptList.localPosition = pos;

        curIndex = index;

        // 신규 태그 비활성화
        btnTags[curIndex].alpha = 0;
        btnTags[curIndex].blocksRaycasts = false;

        // 좌우 버튼 활성/비활성화
        if (curIndex == 0)
        {
            btnLeft.interactable = false;
            btnRight.interactable = true;
        }
        else if (curIndex == listAdopts.Count - 1)
        {
            btnLeft.interactable = true;
            btnRight.interactable = false;
        }
        else
        {
            btnLeft.interactable = true;
            btnRight.interactable = true;
        }

        ExistCatMask();
    }

    // 입양 신청서 마지막 생성시간 변경
    private void SetLastCreationTime(int lastCreationTime)
    {
        Authentication.Inst.userData.adopt.lastCreationTime = lastCreationTime;
        NetworkMethod.SetAdoptLastCreationTime(lastCreationTime);
    }

    // 입양 신청서 쿨타임 변경
    private void SetCoolDownTime(int coolDownTime, bool upload)
    {
        Authentication.Inst.userData.adopt.coolDownTime = coolDownTime;
        if (upload)
        {
            NetworkMethod.SetAdoptCoolDownTime(coolDownTime);
        }
    }

    // 입양 신청서 인덱스 제거
    private void RemoveAdoptIndex(int removedIndex)
    {
        Authentication.Inst.userData.adopt.adoptIndex.RemoveAt(removedIndex);
        NetworkMethod.SetAdoptList(Authentication.Inst.userData.adopt.adoptIndex);
    }

    // 누적 입양 신청서 생성
    private bool StackNewAdoptList()
    {
        Adopt adopt = TableDataManager.Instance.table_Adopt[Authentication.Inst.userData.level];
        int remainList = adopt.Max_Adopt_List - Authentication.Inst.userData.adopt.adoptIndex.Count;
        bool changList = false;
        if(remainList <= 0)
        {
            changList = false;
        }
        else
        {
            int currentTime = TimeUtils.GetCurrentTime();
            int remainTime = currentTime - Authentication.Inst.userData.adopt.lastCreationTime;
            for (int i = 0; i < remainList; i++)
            {
                if (remainTime >= Authentication.Inst.userData.adopt.coolDownTime)
                {
                    remainTime -= Authentication.Inst.userData.adopt.coolDownTime;
                    changList = true;
                    CreateNewAdopt();
                }
                else
                {
                    break;
                }
            }
            if (adopt.Max_Adopt_List == Authentication.Inst.userData.adopt.adoptIndex.Count)
            {
                remainTime = 0;
            }
            if (changList)
            {
                SetLastCreationTime(currentTime - remainTime);
                SetCoolDownTime(Authentication.Inst.userData.adopt.coolDownTime, true);
                ReArrangeAdoptList();
            }
        }

        return changList;
    }

    // 입양 신청서 생성
    private void CreateNewAdopt()
    {
        // 입양 신청서 타입 결정
        Adopt adopt = TableDataManager.Instance.table_Adopt[Authentication.Inst.userData.level];
        if (adopt.Rate_Nor + adopt.Rate_Rare == 10000)
        {
            AdoptType type;
            int rd_type = Random.Range(0, 10000);
            if (rd_type < adopt.Rate_Nor)
            {
                type = AdoptType.Normal;
            }
            else
            {
                type = AdoptType.Special;
            }
            // 입양 신청서 후보 리스트 생성 (레벨 조건 및 타입 체크)
            List<int> adoptListTables = new List<int>();
            foreach (int key in TableDataManager.Instance.table_Adopt_List.Keys)
            {
                Adopt_List adoptList = TableDataManager.Instance.table_Adopt_List[key];
                if (Authentication.Inst.userData.level >= adoptList.Rank && type == adoptList.Type)
                {
                    adoptListTables.Add(key);
                }
            }

            // 입양 신청서 후보 중 랜덤 택 1
            int rd_index = Random.Range(0, adoptListTables.Count);
            int selectedIndex = adoptListTables[rd_index];

            // 입양 신청서 추가
            Authentication.Inst.userData.adopt.adoptIndex.Add(selectedIndex);

            // 생성 대기 시간 지정
            SetCoolDownTime(Random.Range(adopt.Min_Cool_Time, adopt.Max_Cool_Time + 1), false);
        }
        else
        {
            UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String["Adopt/Warning/FailList"].Contents[(int)UI_Setting.language]);
        }
    }

    // 입양 신청서 리스트 재배열 (특수 > 일반, 경험치 보상 큰 순, 골드 보상 큰 순으로 나열)
    private void ReArrangeAdoptList()
    {
        List<int> adoptList = Authentication.Inst.userData.adopt.adoptIndex;
        for (int i = 0; i < Authentication.Inst.userData.adopt.adoptIndex.Count - 1; i++)
        {
            int tmp = i;
            for (int j = i + 1; j < Authentication.Inst.userData.adopt.adoptIndex.Count; j++)
            {
                Adopt_List adopt_tmp = TableDataManager.Instance.table_Adopt_List[adoptList[tmp]];
                Adopt_List adopt_j = TableDataManager.Instance.table_Adopt_List[adoptList[j]];
                if ((int)adopt_tmp.Type < (int)adopt_j.Type)
                {
                    tmp = j;
                }
                else if ((int)adopt_tmp.Type == (int)adopt_j.Type)
                {
                    int exp_tmp = 0;
                    int gold_tmp = 0;
                    int exp_j = 0;
                    int gold_j = 0;

                    for (int k = 0; k < adopt_j.Rewards.Length; k++)
                    {
                        if (adopt_j.Rewards[k].Type == RewardType.Exp)
                        {
                            exp_j = adopt_j.Rewards[k].Value;
                        }
                        if (adopt_j.Rewards[k].Type == RewardType.Gold)
                        {
                            gold_j = adopt_j.Rewards[k].Value;
                        }
                    }
                    for (int k = 0; k < adopt_tmp.Rewards.Length; k++)
                    {
                        if (adopt_tmp.Rewards[k].Type == RewardType.Exp)
                        {
                            exp_tmp = adopt_tmp.Rewards[k].Value;
                        }
                        if (adopt_tmp.Rewards[k].Type == RewardType.Gold)
                        {
                            gold_tmp = adopt_tmp.Rewards[k].Value;
                        }
                    }

                    if (exp_tmp < exp_j)
                    {
                        tmp = j;
                    }
                    else if (exp_tmp == exp_j)
                    {
                        if (gold_tmp < gold_j)
                        {
                            tmp = j;
                        }
                    }
                }
            }
            int val_tmp = adoptList[i];
            adoptList[i] = adoptList[tmp];
            adoptList[tmp] = val_tmp;
        }
        NetworkMethod.SetAdoptList(adoptList);
    }

    // 입양 신청서 리스트 출력
    public void CreateAdoptList()
    {
        List<int> adoptLists = Authentication.Inst.userData.adopt.adoptIndex;
        // 기존 입양 신청서 UI 리스트 삭제 및 초기화
        for (int i = 0; i < listAdopts.Count; i++)
        {
            Destroy(listAdopts[i].gameObject);
        }
        listAdopts.Clear();
        Vector3 pos = contentAdoptList.localPosition;
        pos.x += curIndex * contentAdoptList.rect.width;
        contentAdoptList.localPosition = pos;
        curIndex = 0;
        selectedCatIndex = -1;
        imgAdoptDisable.alpha = 1;
        imgAdoptDisable.blocksRaycasts = true;
        for (int i = 0; i < btnTags.Length; i++)
        {
            btnTags[i].alpha = 0;
            btnTags[i].blocksRaycasts = false;
        }

        // 가능한 입양 신청서가 없을 경우
        if (adoptLists.Count == 0)
        {
            imgNoAdoptList.alpha = 1;
            imgNoAdoptList.blocksRaycasts = true;
            btnLeft.interactable = false;
            btnRight.interactable = false;
        }
        // 가능한 입양 신청서가 있을 경우
        else
        {
            imgNoAdoptList.alpha = 0;
            imgNoAdoptList.blocksRaycasts = false;
            btnLeft.interactable = false;
            // 입양 신청서 개수에 따른 좌우 버튼 세팅
            if (adoptLists.Count == 1)
            {
                btnRight.interactable = false;
            }
            else
            {
                btnRight.interactable = true;
            }
            // 입양 신청서 UI 리스트 생성
            for (int i = 0; i < adoptLists.Count; i++)
            {
                btnTags[i].alpha = 1;
                btnTags[i].blocksRaycasts = true;
                if(TableDataManager.Instance.table_Adopt_List[adoptLists[i]].Type == AdoptType.Normal)
                {
                    imgTags[i].color = new Color(255f/255, 255f/255, 255f/255);
                }
                else
                {
                    imgTags[i].color = new Color(200f/255, 102f/255, 255f/255);
                }
                ListItem_Adopt adoptListItemGo = Instantiate(adoptListPrefab, contentAdoptList).GetComponent<ListItem_Adopt>();

                // 관리용 목록에 추가
                listAdopts.Add(adoptListItemGo);

                btnTags[0].alpha = 0;
                btnTags[0].blocksRaycasts = false;
                adoptListItemGo.Init(i, adoptLists[i]);
                adoptListItemGo.btnSelectCat.onClick.AddListener(() => {
                    OnClickPlusCat();
                    SoundManager.Instance.PlayClick();
                });
            }

            adoptableCatUI.Init(adoptLists[curIndex]);
            ExistCatMask();
        }
    }

    // 입양 보상 획득
    private void GainReward(int multi = 1)
    {
        RewardData[] adoptRewards = curAdopt.Rewards;
        for (int i = 0; i < adoptRewards.Length; i++)
        {
            RewardData reward = new RewardData(adoptRewards[i].Type, adoptRewards[i].Index, adoptRewards[i].Value * multi);
            UserDataManager.Inst.GetReward(reward);
        }
    }
}
