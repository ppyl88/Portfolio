using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FirebaseNetwork;

public class AutoPickManager : MonoBehaviour
{
    [Header("UI_Pick")]
    [SerializeField] private TextMeshProUGUI txtTry = null;         // 일일가능횟수 텍스트
    [SerializeField] private CanvasGroup[] canvasFeeIcons = null;   // 가격 아이콘 어레이
    [SerializeField] private TextMeshProUGUI txtFeeAmount = null;   // 가격 금액 텍스트
    [SerializeField] private GameObject objBtnAdvertise = null;     // 광고버튼 오브젝트
    [SerializeField] private GameObject objBtnPay = null;           // 지불버튼 오브젝트
    [SerializeField] private GameObject objImgTryEnd = null;        // 일일가능횟수 초과 이미지 오브젝트
    private Button btnPay = null;

    [Header("UI_AutoPickIng")]
    [SerializeField] private CanvasGroup canvasAutoPickIng = null;  // 자동 줍기 진행중 UI 캔버스
    [SerializeField] private TextMeshProUGUI txtAutoPickIng = null; // 자동 줍기 진행중 텍스트

    [Header("UI_PickResult")]
    [SerializeField] private TextMeshProUGUI txtResult = null;      // 자동 줍기 결과 텍스트

    [Header("AutoPick")]
    [SerializeField] private ScreenMove screenMove = null;          // 카메라 이동 스크립트
    [SerializeField] private GameObject cleanerAutoPick = null;     // 자동 줍기 클리너 오브젝트
    [SerializeField] private Animator animAutoPick = null;          // 자동 줍기 애니메이션 오브젝트
    private Transform trCleanerAutoPick = null;                     // 자동 줍기 클리너 오브젝트 트랜스폼

    private FeeData[] feeAutoPicks;                                 // 자동 줍기 가격 어레이
    private FeeData feeAutoPick;                                    // 자동 줍기 현재 가격
    private string ingAutoPick;                                     // 자동 줍기 진행중 문자열
    private bool isSkip;                                            // 자동 줍기 애니메이션 스킵
    private Dictionary<int, int> dicPickedHairBalls = new Dictionary<int, int>();   // 주운 헤어볼 딕셔너리 <헤어볼 인덱스, 수량>

    public void Start()
    {
        btnPay = objBtnPay.GetComponent<Button>();
        trCleanerAutoPick = cleanerAutoPick.GetComponent<Transform>();
        ingAutoPick = TableDataManager.Instance.table_String["AutoPick/UI/IngPick"].Contents[(int)UI_Setting.language];
        foreach (int key in TableDataManager.Instance.table_Item.Keys)
        {
            if(TableDataManager.Instance.table_Item[key].Item_Type == ItemType.AutoPick || TableDataManager.Instance.table_Item[key].Item_Type == ItemType.None)
            {
                feeAutoPicks = TableDataManager.Instance.table_Item[key].AutoPick_Fee.ToArray();
                break;
            }
        }

    }

    // 자동줍기 초기화
    public void InitPick()
    {
        UserDataManager.Inst.CheckAdvertiseReset();
        UserDataManager.Inst.CheckAutoPickReset();
        string tryCount = TableDataManager.Instance.table_String["AutoPick/UI/PickTry"].Contents[(int)UI_Setting.language];
        tryCount = tryCount.Replace("a", Authentication.Inst.userData.todayAutoPick.ToString());
        tryCount = tryCount.Replace("b", feeAutoPicks.Length.ToString());
        txtTry.text = tryCount;
        if (Authentication.Inst.userData.todayAutoPick < feeAutoPicks.Length)
        {
            objBtnPay.SetActive(true);
            objImgTryEnd.SetActive(false);
            if (Authentication.Inst.userData.todayAdvertise < TableDataManager.Instance.table_Setting["Advertisement_Watched_Count"].Value) objBtnAdvertise.SetActive(true);
            else objBtnAdvertise.SetActive(false);

            feeAutoPick = feeAutoPicks[Authentication.Inst.userData.todayAutoPick];
            if (feeAutoPick.Type == FeeType.Gold)
            {
                canvasFeeIcons[0].alpha = 1;
                canvasFeeIcons[1].alpha = 0;
                if (Authentication.Inst.userData.gold < feeAutoPick.Value) btnPay.interactable = false;
                else btnPay.interactable = true;
            }
            else if (feeAutoPick.Type == FeeType.Coin)
            {
                canvasFeeIcons[0].alpha = 0;
                canvasFeeIcons[1].alpha = 1;
                if (Authentication.Inst.userData.coin < feeAutoPick.Value) btnPay.interactable = false;
                else btnPay.interactable = true;
            }
            txtFeeAmount.text = feeAutoPick.Value.ToString();

        }
        else
        {
            objBtnAdvertise.SetActive(false);
            objBtnPay.SetActive(false);
            objImgTryEnd.SetActive(true);
        }
    }

    // 자동 줍기 결과창 설정
    private void InitPickResult()
    {
        string result = "";
        string defaultLine = TableDataManager.Instance.table_String["AutoPick/UI/ResultDesc"].Contents[(int)UI_Setting.language];
        foreach (int index in dicPickedHairBalls.Keys)
        {
            string hairBallName = TableDataManager.Instance.table_Item[index].Name;
            string line = defaultLine.Replace("hh", hairBallName);
            line = line.Replace("0", dicPickedHairBalls[index].ToString());
            result += line + "\n";
        }

        txtResult.text = result.Trim();
    }

    // 자동 줍기 가능 여부 확인
    private bool CheckPossibility()
    {
        if(UserDataManager.Inst.cntHairBall == 0)
        {
            UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String["AutoPick/Warning/NoHairBall"].Contents[(int)UI_Setting.language]);
            return false;
        }
        else
        {
            for (int i = 0; i <Authentication.Inst.userData.items.Count; i++)
            {
                ItemData item = Authentication.Inst.userData.items[i];
                if(item.Type == ItemType.HairBall && item.count >= TableDataManager.Instance.table_Setting["HairBall_Store_Max"].Value)
                {
                    UIManager.instance.uI_Warning.ShowPopUP(TableDataManager.Instance.table_String["AutoPick/Warning/MaxHairBall"].Contents[(int)UI_Setting.language]);
                    return false;
                }
            }
        }
        return true;
    }

    // 광고 보기 클릭 동작
    public void OnClickAdvertise()
    {
        if(CheckPossibility())
        {
            AdMobManager.Inst.ShowAd(() => {
                AutoPick();
                Authentication.Inst.userData.todayAdvertise++;
                NetworkMethod.SetTodayAdvertise();
            });
        }
    }

    // 자동 줍기 가격 클릭 동작
    public void OnClickPay()
    {
        if (CheckPossibility())
        {
            AutoPick();
            if (feeAutoPick.Type == FeeType.Gold) UserDataManager.Inst.AddGold(-feeAutoPick.Value);
            else if (feeAutoPick.Type == FeeType.Coin) UserDataManager.Inst.AddCoin(-feeAutoPick.Value);
        }
    }

    // 자동줍기 skip 클릭 동작
    public void OnClickSkip()
    {
        isSkip = true;
    }

    // 자동줍기 함수
    private void AutoPick()
    {
        UIManager.instance.ClosePopUpUIExceptMainUI();
        isSkip = false;
        txtAutoPickIng.text = ingAutoPick;
        canvasAutoPickIng.alpha = 1;
        canvasAutoPickIng.blocksRaycasts = true;
        dicPickedHairBalls.Clear();
        cleanerAutoPick.SetActive(true);
        StartCoroutine(CoWaitAutoPick());
        StartCoroutine(CoAutoPickIngText());
    }

    // 자동줍기 종료 시 함수
    private void EndAutoPick()
    {
        Authentication.Inst.userData.todayAutoPick++;
        NetworkMethod.SetTodayAutoPick();
        cleanerAutoPick.SetActive(false);
        foreach (int index in dicPickedHairBalls.Keys)
        {
            StorageManager.Instance.AddItem(index, dicPickedHairBalls[index]);
        }
        canvasAutoPickIng.alpha = 0;
        canvasAutoPickIng.blocksRaycasts = false;
        InitPickResult();
        UIManager.instance.OpenPopUpUIByIndex(9);
        UIManager.instance.OpenPopUpChildControl(1);
    }

    // 자동줍기 대기 코루틴
    private IEnumerator CoWaitAutoPick()
    {
        while(true)
        {
            screenMove.FollowTransform(trCleanerAutoPick);
            if (animAutoPick.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99)
            {
                EndAutoPick();
                yield break;
            }
            else if(isSkip)
            {
                Time.timeScale = 0;
                for(int i=0; i<UserDataManager.Inst.cntHairBall; i++)
                {
                    PickHairBall(FurniturePlacement.Instance.hairBalls[i].index_HairBall);
                    FurniturePlacement.Instance.hairBalls[i].AutoPickDetected();
                }
                Time.timeScale = 1;
                EndAutoPick();
                yield break;
            }
            yield return null;
        }
    }

    // 자동줍기 진행중 글자 코루틴
    private IEnumerator CoAutoPickIngText()
    {
        while(canvasAutoPickIng.blocksRaycasts)
        {
            if (txtAutoPickIng.text.Length > ingAutoPick.Length + 3)
            {
                txtAutoPickIng.text = ingAutoPick;
                yield return YieldInstructionCache.WaitForSeconds(0.5f);
            }
            else
            {
                txtAutoPickIng.text = txtAutoPickIng.text + ".";
                yield return YieldInstructionCache.WaitForSeconds(0.5f);
            }
        }
        yield return null;
    }

    // 헤어볼 개당 줍기 동작 함수
    public void PickHairBall(int index)
    {
        int pickedIndex = index;
        if (dicPickedHairBalls.ContainsKey(pickedIndex)) dicPickedHairBalls[pickedIndex]++;
        else dicPickedHairBalls.Add(pickedIndex, 1);
    }
}
