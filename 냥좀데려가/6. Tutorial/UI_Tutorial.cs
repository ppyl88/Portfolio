using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;
using FirebaseNetwork;

public class UI_Tutorial : MonoBehaviour
{
    // 튜토리얼 변수
    public int cnt_tutorial = 0;
    private bool currentMain;
    private bool waiting;
    private string strDefault = "Tutorial/";
    private int end_cnt = 88;

    private CatData newCat = new CatData(-1, "만렙냥", 21, 10, 0, new int[3] { 100, 100, 100 }, CatState.Free);
    private int tutorialAdopt = 200000;
    private int jump_cnt = 72;
    private int waring_cnt = 1000;

    [SerializeField] private UI_Script scriptUI = null;
    [SerializeField] private UI_Resque resqueUI = null;
    [SerializeField] private UI_Adopt adoptUI = null;
    [SerializeField] private ShopUIManager shopUI = null;

    [SerializeField] private CanvasGroup canvasMaskSpots = null;
    [SerializeField] private CanvasGroup[] maskSpots = null;
    [SerializeField] private RectTransform[] rectMaskSpots = null;
    [SerializeField] private CanvasGroup canvasBtnSpot = null;
    [SerializeField] private RectTransform rectBtnSpot = null;
    [SerializeField] private Button btnSpot = null;
    [SerializeField] private CanvasGroup canvasBG = null;

    [SerializeField] private UnityEvent[] events = null;
    [SerializeField] private CanvasGroup[] canvasEndBtns = null;
    [SerializeField] private Button[] endBtns = null;
    private Dictionary<int, Action> actions = new Dictionary<int, Action>();
    private Button curBtn = null;


    // 헤어볼 튜토리얼 관련 변수
    private Item_Furniture itemFurniture;
    private HairBallSpawner hairBallSpawner;
    private bool makingHairBall = true;

    private void Awake()
    {
        if (Authentication.Inst.userData.endTutorial) gameObject.SetActive(false);
        else
        {
            InitAction();
        }
    }

    private void Start()
    {
        UIManager.instance.ui_YesNo.ShowPopUP(TableDataManager.Instance.table_String[strDefault + "UI/Ment"].Contents[(int)UI_Setting.language], () =>
        {
            cnt_tutorial = 0;
            curBtn = null;
            makingHairBall = false;
            TutorialNext();
        }, () =>
        {
            Authentication.Inst.userData.endTutorial = true;
            NetworkMethod.SetUserEndTutorial();
            MissionManager.Instance.CheckEnableChapter();
        }
        );
    }

    #region Main Function
    public void TutorialNext()
    {
        waiting = false;
        if (curBtn != null)
        {
            curBtn.onClick.RemoveListener(TutorialNext);
            curBtn = null;
        }
        cnt_tutorial++;
        if (cnt_tutorial != end_cnt + 1)
        {
            Tutorial_table tutorial = TableDataManager.Instance.table_Tutorial[cnt_tutorial];
            switch (tutorial.Type_Tutorial)
            {
                case TutorialType.Script:
                    ActiveBackground(false);
                    scriptUI.scriptEnded = false;
                    string[] typing_text = new string[tutorial.Script_Count];
                    for (int i = 0; i < typing_text.Length; i++)
                    {
                        typing_text[i] = TableDataManager.Instance.table_String[strDefault + tutorial.Index_Tutorial + "/" + (i+1)].Contents[(int)UI_Setting.language];
                    }
                    scriptUI.StartScript(typing_text, 10);
                    StartCoroutine(CoScriptWait());
                    break;
                case TutorialType.Highlight:
                    ActiveBackground(true);
                    SetMask(true, tutorial.isSpotCircle, tutorial.Spot_Size, tutorial.Spot_Position, tutorial.Spot_AnchorMin, tutorial.Spot_AnchorMax);
                    break;
                case TutorialType.Event:
                    ActiveBackground(true);
                    SetMask(false, tutorial.isSpotCircle, tutorial.Spot_Size, tutorial.Spot_Position, tutorial.Spot_AnchorMin, tutorial.Spot_AnchorMax);
                    SetButton(tutorial.Spot_Size, tutorial.Spot_Position, tutorial.Spot_AnchorMin, tutorial.Spot_AnchorMax, () => events[tutorial.Event_Tutorial].Invoke());
                    break;
                case TutorialType.Action:
                    actions[tutorial.Action_Tutorial]();
                    TutorialNext();
                    break;
                case TutorialType.Wait:
                    ActiveBackground(false);
                    waiting = true;
                    StartCoroutine(coWait(tutorial.Wait_Tutorial));
                    break;
            }
        }
        else
        {
            Authentication.Inst.userData.endTutorial = true;
            NetworkMethod.SetUserEndTutorial();
            NetworkMethod.UpdateUserDateAfterTutorial();
            MissionManager.Instance.CheckEnableChapter();
            gameObject.SetActive(false);
        }

    }

    public void TutorialGo(int index)
    {
        waiting = false;
        if (curBtn != null)
        {
            curBtn.onClick.RemoveListener(TutorialNext);
            curBtn = null;
        }
        cnt_tutorial = index;
        Tutorial_table tutorial = TableDataManager.Instance.table_Tutorial[cnt_tutorial];
        switch (tutorial.Type_Tutorial)
        {
            case TutorialType.Script:
                ActiveBackground(false);
                scriptUI.scriptEnded = false;
                string[] typing_text = new string[tutorial.Script_Count];
                for (int i = 0; i < typing_text.Length; i++)
                {
                    typing_text[i] = TableDataManager.Instance.table_String[strDefault + (tutorial.Script_Start + i).ToString()].Contents[(int)UI_Setting.language];
                }
                scriptUI.StartScript(typing_text, 10);
                StartCoroutine(CoScriptWait());
                break;
            case TutorialType.Highlight:
                ActiveBackground(true);
                SetMask(true, tutorial.isSpotCircle, tutorial.Spot_Size, tutorial.Spot_Position, tutorial.Spot_AnchorMin, tutorial.Spot_AnchorMax);
                break;
            case TutorialType.Event:
                ActiveBackground(true);
                SetMask(false, tutorial.isSpotCircle, tutorial.Spot_Size, tutorial.Spot_Position, tutorial.Spot_AnchorMin, tutorial.Spot_AnchorMax);
                SetButton(tutorial.Spot_Size, tutorial.Spot_Position, tutorial.Spot_AnchorMin, tutorial.Spot_AnchorMax, () => events[tutorial.Event_Tutorial].Invoke());
                break;
            case TutorialType.Action:
                actions[tutorial.Action_Tutorial]();
                TutorialNext();
                break;
            case TutorialType.Wait:
                ActiveBackground(false);
                waiting = true;
                StartCoroutine(coWait(tutorial.Wait_Tutorial));
                break;
        }
    }
    #endregion

    #region ActiveCanvas

    private void ActiveBackground(bool active)
    {
        canvasBG.alpha = active ? 1 : 0;
        canvasBG.blocksRaycasts = active;
    }

    private void ActiveMask(bool active)
    {
        canvasMaskSpots.alpha = active ? 1 : 0;
        if (active)
        {
            currentMain = UIManager.instance.isMain;
            if (currentMain) UIManager.instance.isMain = false;
        }
        else
        {
            UIManager.instance.isMain = currentMain;
        }
    }

    private void ActiveButton(bool active)
    {
        canvasBtnSpot.alpha = active ? 1 : 0;
        canvasBtnSpot.blocksRaycasts = active;
    }
    #endregion

    #region Script Function
    // 스크립트 Wait 함수
    private IEnumerator CoScriptWait()
    {
        while(true)
        {
            if(scriptUI.scriptEnded)
            {
                TutorialNext();
                yield break;
            }
            yield return null;
        }
    }
    #endregion

    #region Mask Function
    private void SetMask(bool onlyHighlight, bool isCircle, Vector2 size, Vector2 position, Vector2 anchorMin, Vector2 anchorMax)
    {
        ActiveMask(true);
        maskSpots[0].alpha = 0;
        maskSpots[1].alpha = 0;

        int index_spot = 1;

        if (isCircle)
        {
            index_spot = 0;
        }

        maskSpots[index_spot].alpha = 1;
        rectMaskSpots[index_spot].sizeDelta = size;
        rectMaskSpots[index_spot].anchoredPosition = position;
        rectMaskSpots[index_spot].anchorMin = anchorMin;
        rectMaskSpots[index_spot].anchorMax = anchorMax;

        StartCoroutine(coHighLight(index_spot, size, onlyHighlight));
    }

    private IEnumerator coHighLight(int spot, Vector2 finalSize, bool onlyHighlight)
    {
        float multi = 2;
        while (multi > 1)
        {
            rectMaskSpots[spot].sizeDelta = finalSize * multi;
            multi -= Time.deltaTime * 2;
            yield return null;
        }
        rectMaskSpots[spot].sizeDelta = finalSize;
        if (onlyHighlight)
        {
            yield return YieldInstructionCache.WaitForSeconds(0.8f);
            ActiveMask(false);
            TutorialNext();
        }
        yield return null;
    }

    #endregion

    #region Button Function
    private void SetButton(Vector2 size, Vector2 position, Vector2 anchorMin, Vector2 anchorMax, Action action)
    {
        ActiveButton(true);
        rectBtnSpot.sizeDelta = size;
        rectBtnSpot.anchoredPosition = position;
        rectBtnSpot.anchorMin = anchorMin;
        rectBtnSpot.anchorMax = anchorMax;
        btnSpot.onClick.RemoveAllListeners();
        btnSpot.onClick.AddListener(() =>
        {
            ActiveMask(false);
            ActiveButton(false);
            action();
            TutorialNext();
        });
    }

    public void TutorialResque()
    {
        int selectedCatTableIndex = resqueUI.CastResqueReward(resqueUI.specialResqueIndex);
        CatData cat = CatManager.Instance.CreateNewCat(selectedCatTableIndex);
        Authentication.Inst.userData.cats.Add(cat);
        CatManager.Instance.catListUI.AddCatDataItem();
        resqueUI.resqueRewardUI.SetResqueReward();
    }

    public void TutorialBuyFurniture()
    {
        SoundManager.Instance.PlayEffect(SoundType.UseMoney);
        UserDataManager.Inst.AddGold(-shopUI.total, false);
        FurnitureData furniture = new FurnitureData(shopUI.index);
        Authentication.Inst.userData.furnitures.Add(furniture);
        StorageManager.Instance.storageDataListUI.AddFunitureStorage(furniture);
        UIManager.instance.OpenPopUpChildControl(-3);
    }

    public void TutorialFurnitureArrange()
    {
        FurnitureData furniture = Authentication.Inst.userData.furnitures[Authentication.Inst.userData.furnitures.Count - 1];
        FurniturePlacement.Instance.TutorialArrangeFurniture(furniture);
    }

    public void TutorialCatSelect()
    {
        CatManager.Instance.catListUI.OnClickCat(Authentication.Inst.userData.cats[0]);
    }

    public void TutorialCatArrange()
    {
        CatData cat = Authentication.Inst.userData.cats[Authentication.Inst.userData.cats.Count - 1];
        FurniturePlacement.Instance.TutorialArrangeCat(cat);
    }

    public void TutorialItemDetail()
    {
        ItemData item = Authentication.Inst.userData.items[0];
        StorageManager.Instance.storageDataDetailUI.SetStorageDetail(item);
        UIManager.instance.OpenPopUpChildControl(-1);
    }

    public void TutorialSellItem()
    {
        ItemData item = Authentication.Inst.userData.items[0];
        UserDataManager.Inst.AddGold(item.SellingPrice, false);
        StorageManager.Instance.storageDataListUI.RemoveItemStorage(item);
        Authentication.Inst.userData.items.Remove(item);
        UIManager.instance.OpenPopUpChildControl(0);
    }

    public void TutorialAdoptReward()
    {
        RewardData[] adoptRewards = adoptUI.curAdopt.Rewards;
        for (int i = 0; i < adoptRewards.Length; i++)
        {
            UserDataManager.Inst.GetReward(adoptRewards[i], false);
        }
        Authentication.Inst.userData.cats.RemoveAt(adoptUI.selectedCatIndex);
        Authentication.Inst.userData.adopt.adoptIndex.RemoveAt(0);
        SoundManager.Instance.PlayEffect(SoundType.Buff);
        UIManager.instance.ClosePopUpUIExceptMainUI();
    }

    #endregion

    #region Action Function
    private void InitAction()
    {
        actions.Add(1, () => resqueUI.txtSpecialCost.text = "0");
        actions.Add(2, () => UIManager.instance.ClosePopUpUIExceptMainUI());
        actions.Add(3, () => FurniturePlacement.Instance.pause = true);
        actions.Add(4, () => { FurniturePlacement.Instance.btnStoreFurniture.alpha = 0; FurniturePlacement.Instance.btnStoreFurniture.blocksRaycasts = false; FurniturePlacement.Instance.pause = false; });
        actions.Add(5, () => { FurniturePlacement.Instance.btnStoreCat.alpha = 0; FurniturePlacement.Instance.btnStoreCat.blocksRaycasts = false; FurniturePlacement.Instance.pause = false; });
        actions.Add(6, () => {
            itemFurniture = FurniturePlacement.Instance.transform.GetChild(1).GetComponent<Item_Furniture>();
            hairBallSpawner = FurniturePlacement.Instance.transform.GetChild(1).GetChild(0).GetChild(0).Find("HairBallSpawner").GetComponent<HairBallSpawner>();
            hairBallSpawner.making = false;
            makingHairBall = true;
            StartCoroutine(coMakingHairBall());
        });
        actions.Add(7, () => { if (Authentication.Inst.userData.cats[0].state == CatState.EndFurniture) { makingHairBall = false; hairBallSpawner.making = true; cnt_tutorial = jump_cnt - 1; } });
        actions.Add(8, () => { if (Authentication.Inst.userData.cats[0].state == CatState.EndFurniture) { cnt_tutorial = waring_cnt - 1; } });
        actions.Add(9, () => {
            itemFurniture.AbleBtns(false);
            UIManager.instance.AbleMainUI(false);
        });
        actions.Add(10, () => {
            itemFurniture.AbleBtns(true);
            UIManager.instance.AbleMainUI(true);
            makingHairBall = false;
            hairBallSpawner.making = true;
        });
        actions.Add(11, () => {
            Authentication.Inst.userData.cats.Add(newCat);
            if (Authentication.Inst.userData.adopt.adoptIndex.Count == 0)
            {
                Authentication.Inst.userData.adopt.adoptIndex.Add(tutorialAdopt);
            }
            else
            {
                Authentication.Inst.userData.adopt.adoptIndex[0] = tutorialAdopt;
            }
            adoptUI.CreateAdoptList();
        });
        actions.Add(1000, () => {
            itemFurniture.AbleBtns(true);
            UIManager.instance.AbleMainUI(true);
            makingHairBall = false;
            hairBallSpawner.making = true;
            cnt_tutorial = jump_cnt - 1;
        });
    }
    #endregion

    #region WaitFunction
    private IEnumerator coWait(int index)
    {
        bool canvasOn = false;
        bool alreadyOn = false;
        while (waiting)
        {
            if(index != -1)
            {
                if (!canvasOn && canvasEndBtns[index].blocksRaycasts)
                {
                    canvasOn = true;
                }
                else if (canvasOn && !makingHairBall && !canvasEndBtns[index].blocksRaycasts)
                {
                    canvasOn = false;
                    alreadyOn = false;
                    curBtn.onClick.RemoveListener(TutorialNext);
                    curBtn = null;
                }
                if (canvasOn && !alreadyOn)
                {
                    curBtn = endBtns[index];
                    curBtn.onClick.AddListener(TutorialNext);
                    alreadyOn = true;
                }
            }
            yield return null;
        }
    }
    #endregion

    #region Etc
    private IEnumerator coMakingHairBall()
    {
        while (makingHairBall)
        {
            if (UserDataManager.Inst.cntHairBall == 0)
            {
                if (!hairBallSpawner.enabled && cnt_tutorial == 64)
                {
                    TutorialGo(waring_cnt);
                    yield break;
                }
                else if(hairBallSpawner.enabled)
                {
                    hairBallSpawner.SpawnHairBall(0);
                    FurniturePlacement.Instance.hairBalls[0].gameObject.GetComponentInChildren<Button>().onClick.AddListener(TutorialNext);
                }
            }
            yield return null;
        }
    }
    #endregion
}
