using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Item_Furniture : MonoBehaviour
{
    public int index = -1;
    [HideInInspector] public int curOrder = 0;
    [HideInInspector] public FurnitureData furniture;
    [HideInInspector] public CatData cat;
    [HideInInspector] public FurniturePlacementState state;

    [SerializeField] private HairBallSpawner hairBallSpawner = null;
    [SerializeField] private Transform hairBallGround = null;

    [SerializeField] private SpriteRenderer spriteRenderer = null;
    [SerializeField] private CapsuleCollider2D capsCollider = null;
    [SerializeField] private BoxCollider2D boxCollider = null;
    [SerializeField] private BoxCollider2D boxTrigger = null;
    [SerializeField] private GameObject messSprite = null;
    [SerializeField] private SpriteRenderer srMessSprite = null;
    [SerializeField] private GameObject cleaningObject = null;
    [SerializeField] private SpriteRenderer srCleaningObject = null;
    [SerializeField] private GameObject outLine = null;
    [SerializeField] private GameObject groupMarkers = null;
    [SerializeField] private GameObject[] markers = null;
    [SerializeField] private GameObject groupEtc = null;
    public GameObject catObject = null;
    [SerializeField] private Item_Cat catItem = null;
    [SerializeField] private RectTransform HUDrect = null;
    [SerializeField] private Canvas HUDcanvas = null;

    [SerializeField] private CanvasGroup panelDefault = null;
    [SerializeField] private CanvasGroup canvasBtnClean = null;
    [SerializeField] private CanvasGroup canvasProgressBar = null;
    [SerializeField] private CanvasGroup[] iconProgress = null;
    [SerializeField] private Image imgProgressBar = null;
    [SerializeField] private RectTransform rectProgressBar = null;

    [SerializeField] private CanvasGroup panelBtnFurniture = null;
    [SerializeField] private CanvasGroup panelBtns = null;
    
    [SerializeField] private Button btnMoving = null;
    [SerializeField] private Button btnStore = null;

    [SerializeField] private TextMeshProUGUI timer = null;

    [SerializeField] private ParticleSystem arrangeParticle = null;

    [SerializeField] private CanvasGroup canvasBtnFurniture = null;
    [SerializeField] private CanvasGroup canvasBtnCat = null;


    private List<GameObject> conflicts = new List<GameObject>();
    private string layer = null;
    private bool areaIn = false;
    private float touchTime = -1;
    private float useTime;
    private float remainUsingTime;
    private int defaultCleaningTime;
    private float remainCleaningTime;

    private void Update()
    {
        // 메인화면 가구 터치 인식
        if (touchTime > -0.5)
        {
            touchTime -= Time.deltaTime;
            if (touchTime <= 0)
            {
                FurniturePlacement.Instance.ArrangeCurrentFurniture(index);
                touchTime = -1;
            }
        }
    }

    #region Setting
    public void SetData(FurnitureData furnitureData)
    {
        furniture = furnitureData;
        curOrder = -1;
        areaIn = true;
        spriteRenderer.sprite = furniture.Sprite;
        srMessSprite.size = furniture.Sprite.rect.size * 0.01f;
        HUDrect.sizeDelta = furniture.Sprite.rect.size;
        boxTrigger.size = furniture.Sprite.rect.size * 0.01f;
        if (furniture.AreaFurniture.isBox)
        {
            boxCollider.size = furniture.AreaFurniture.size;
            boxCollider.offset = furniture.AreaFurniture.offset;
        }
        else
        {
            capsCollider.size = furniture.AreaFurniture.size;
            capsCollider.offset = furniture.AreaFurniture.offset;
            if (capsCollider.size.x >= capsCollider.size.y) capsCollider.direction = CapsuleDirection2D.Horizontal;
            else capsCollider.direction = CapsuleDirection2D.Vertical;
        }
        if(furniture.Type == FurnitureType.FunctionFurniture) hairBallGround.gameObject.SetActive(true);
        else hairBallGround.gameObject.SetActive(false);
        switch (furniture.AreaType)
        {
            case AreaType.Floor:
            case AreaType.Mat:
                layer = furniture.AreaType.ToString();
                hairBallGround.localPosition = new Vector3(0, -(furniture.Sprite.rect.height + 100f) * 0.005f, 0);
                break;
            default:
                layer = "Wall";
                hairBallGround.localPosition =  new Vector3(0, 2.4f, 0) - transform.position;
                break;
        }
        ChangeLayerName(layer);
        ChangeOrderLayer(0);
        gameObject.layer = LayerMask.NameToLayer(layer + "Furniture");
        gameObject.tag = layer + "Furniture";
        outLine.GetComponent<SpriteMask>().sprite = furniture.Sprite;
        outLine.GetComponent<SpriteRenderer>().size = furniture.Sprite.rect.size * 0.01f;
        if (furniture.state == FurnitureState.Dirty)
        {
            ActiveClean(true);
        }
        else if (furniture.state == FurnitureState.Cleaning)
        {
            StartCleaning();
        }
        rectProgressBar.anchorMin = new Vector2(0.5f, 0);
        rectProgressBar.anchorMax = new Vector2(0.5f, 0);
        rectProgressBar.pivot = new Vector2(0.5f, 0);
        rectProgressBar.anchoredPosition = new Vector2(0, -60);
        for (int i = 0; i < furniture.AreaCat.Length; i++)
        {
            markers[i].SetActive(true);
            markers[i].GetComponent<Transform>().localPosition = furniture.AreaCat[i].pos;
            if(furniture.AreaCat[i].pos.y <= - (furniture.Sprite.rect.height * 0.005f))
            {
                rectProgressBar.anchorMin = new Vector2(0.5f, 1);
                rectProgressBar.anchorMax = new Vector2(0.5f, 1);
                rectProgressBar.pivot = new Vector2(0.5f, 1);
                rectProgressBar.anchoredPosition = new Vector2(0, 60);
            }
        }
    }

    public void ResetData()
    {
        for (int i = 0; i < furniture.AreaCat.Length; i++)
        {
            markers[i].SetActive(false);
        }
        ActiveClean(false);
        ActiveProgressBar(false);
        ActiveArrangeMenu(false);
        IntoArrageCanvas(false);
        furniture = null;
        cat = null;
    }
    #endregion

    #region Furniture Function
    public void StoreFurniture()
    {
        FurniturePlacement.Instance.SelectFurniture(-1);
        StorageManager.Instance.StoreFurniture(furniture);
        FurniturePlacement.Instance.AbleFurnitures();
    }

    public void ChangeSprite(bool isSub)
    {
        if (isSub)
        {
            spriteRenderer.sprite = furniture.SubSprite;
        }
        else
        {
            spriteRenderer.sprite = furniture.Sprite;
        }
    }

    public void ChangeState(FurniturePlacementState _state, int _marker = 0)
    {
        state = _state;
        switch (state)
        {
            case FurniturePlacementState.Default:
                outLine.SetActive(false);
                spriteRenderer.color = Color.white;
                groupMarkers.SetActive(false);
                break;
            case FurniturePlacementState.Moving:
                outLine.SetActive(true);
                outLine.GetComponent<SpriteRenderer>().color = Color.green;
                spriteRenderer.color = Color.white;
                break;
            case FurniturePlacementState.CantPlace:
                outLine.SetActive(true);
                outLine.GetComponent<SpriteRenderer>().color = Color.red;
                spriteRenderer.color = Color.white;
                break;
            case FurniturePlacementState.StoreFurniture:
                outLine.SetActive(false);
                spriteRenderer.color = Color.gray;
                break;
            case FurniturePlacementState.Arrangable:
                groupMarkers.SetActive(true);
                for (int i = 0; i < furniture.AreaCat.Length; i++)
                {
                    markers[i].GetComponent<SpriteRenderer>().color = Color.green;
                }
                break;
            case FurniturePlacementState.Arranging:
                markers[_marker].GetComponent<SpriteRenderer>().color = Color.yellow;
                arrangeParticle.Play();
                break;
            case FurniturePlacementState.CancelArranging:
                markers[_marker].GetComponent<SpriteRenderer>().color = Color.green;
                break;
        }
    }

    public void ChangeLayerPointer(bool isPointer)
    {
        spriteRenderer.sortingLayerName = isPointer ? "Pointer" : layer;
    }

    public void ChangeLayerName(string _layer)
    {
        layer = _layer;
        spriteRenderer.sortingLayerName = _layer;
        catItem.SetLayer(_layer);
        srMessSprite.sortingLayerName = _layer;
        srCleaningObject.sortingLayerName = _layer;
        HUDcanvas.sortingLayerName = _layer;
    }

    public void ChangeOrderLayer(int _order)
    {
        curOrder = _order;
        spriteRenderer.sortingOrder = _order;
        srMessSprite.sortingOrder = _order + 1;
        srCleaningObject.sortingOrder = _order + 2;
        HUDcanvas.sortingOrder = _order + 3;
        catItem.SetOrder(_order + 4);
    }

    public void ChangeGroundPosition()
    {
        hairBallGround.localPosition = new Vector3(0, 2.4f, 0) - transform.position;
    }

    public void StartCleaning()
    {
        SetProgressBarImg(1);
        defaultCleaningTime = TableDataManager.Instance.table_Setting["Cleaning_Time_Sec"].Value;
        if (furniture.cleanTime == -1)
        {
            StorageManager.Instance.CleanFurnitureStart(furniture);
            remainCleaningTime = defaultCleaningTime;
        }
        else
        {
            remainCleaningTime = furniture.cleanTime + defaultCleaningTime - TimeUtils.GetCurrentTime();
        }

        if (remainCleaningTime > 0)
        {
            canvasProgressBar.alpha = 1;
            StartCoroutine(Cleaning());
            cleaningObject.SetActive(true);
        }
        else
        {
            EndCleaning();
        }
    }

    private void EndCleaning()
    {
        canvasProgressBar.alpha = 0;
        cleaningObject.SetActive(false);
        messSprite.SetActive(false);
        StorageManager.Instance.CleanFurnitureEnd(furniture);
    }
    #endregion

    #region Cat Function
    public void ArrangeCat(CatData catData, int i_marker)
    {
        cat = catData;
        catObject.SetActive(true);
        catItem.SetData(catData, furniture.AreaCat[i_marker]);
        if (cat.state != CatState.EndFurniture)
        {
            StartProgress();
        }
        else
        {
            catItem.ActiveBtnEnd(true);
        }
    }

    public void RemoveCat()
    {
        catObject.SetActive(false);
        ActiveProgressBar(false);
        cat = null;
        if (furniture.mess + 1 > 0)
        {
            ActiveClean(true);
        }
    }

    private void StartProgress()
    {
        SetProgressBarImg(0);
        useTime = furniture.UseTime * 60;
        if (furniture.arrangeTime == -1)
        {
            remainUsingTime = useTime;
        }
        else
        {
            UpdateRemainProgress();
        }

        if (remainUsingTime > 0)
        {
            StartCoroutine(ProgressUI());
            ActiveProgressBar(true);
        }
        else
        {
            EndProgress();
        }
    }

    private void SetProgress(float progress)
    {
        progress = progress < 0 ? 0 : progress;
        imgProgressBar.fillAmount = progress;
    }


    // 프로그래스 종료
    private void EndProgress()
    {
        CatManager.Instance.CatStateChange(Authentication.Inst.userData.cats.IndexOf(cat), CatState.EndFurniture);
        catItem.ActiveBtnEnd(true);
        ActiveProgressBar(false);
    }

    public void UpdateRemainProgress()
    {
        remainUsingTime = furniture.arrangeTime + useTime - TimeUtils.GetCurrentTime();
    }
    #endregion

    #region Furniture Button Control
    public void CancelArrangeMenu()
    {
        FurniturePlacement.Instance.SelectFurniture(-1);
    }

    public void MoveFurniture()
    {
        ActiveArrangeMenu(false);
        StartCoroutine(FurniturePlacement.Instance.FurnitureMoving());
    }

    public void OnClickFurniture()
    {
        StorageManager.Instance.storageDataDetailUI.SetStorageDetail(furniture);
        UIManager.instance.OpenPopUpUIByIndex(5);
        UIManager.instance.OpenPopUpChildControl(-1);
    }
    
    // 가구 터치 인식
    public void DetectOnOff(bool on)
    {
        if (on)
        {
            touchTime = 2f;
        }
        else
        {
            touchTime = -1f;
        }
    }
    #endregion

    #region UI Control
    public void ActiveFurnitureBtn(bool active)
    {
        panelBtnFurniture.alpha = active ? 1 : 0;
        panelBtnFurniture.blocksRaycasts = active;
    }

    public void ActiveArrangeMenu(bool active)
    {
        panelBtns.alpha = active ? 1 : 0;
        panelBtns.blocksRaycasts = active;
        HUDcanvas.sortingLayerName = active ? "HUD" : layer;
        btnMoving.enabled = !active;
        RectTransform rectPanelBtns = panelBtns.gameObject.GetComponent<RectTransform>();
        if (active)
        {
            FurniturePlacement.Instance.SelectFurniture(index);
            if (furniture.state == FurnitureState.Arranged)
            {
                btnStore.interactable = true;
            }
            else
            {
                btnStore.interactable = false;
            }
            if (transform.position.y > FurniturePlacement.Instance.ylim - 1.2f - furniture.Sprite.rect.height * 0.005f)
            {
                rectPanelBtns.anchorMin = new Vector2(0.5f, 0f);
                rectPanelBtns.anchorMax = new Vector2(0.5f, 0f);
                rectPanelBtns.pivot = new Vector2(0.5f, 0f);
                rectPanelBtns.anchoredPosition = new Vector2(0, -120);
            }
            else
            {
                rectPanelBtns.anchorMin = new Vector2(0.5f, 1f);
                rectPanelBtns.anchorMax = new Vector2(0.5f, 1f);
                rectPanelBtns.pivot = new Vector2(0.5f, 1f);
                rectPanelBtns.anchoredPosition = new Vector2(0, 120);
            }
            if (furniture.Sprite.rect.width < 400)
            {
                if (transform.position.x > FurniturePlacement.Instance.xlim - 2)
                {
                    rectPanelBtns.anchoredPosition = new Vector2((FurniturePlacement.Instance.xlim - transform.position.x) * 100 - 200, rectPanelBtns.anchoredPosition.y);
                }
                else if (transform.position.x < -FurniturePlacement.Instance.xlim + 2)
                {
                    rectPanelBtns.anchoredPosition = new Vector2(200 - (FurniturePlacement.Instance.xlim + transform.position.x) * 100, rectPanelBtns.anchoredPosition.y);
                }
            }
        }
    }

    public void ActiveBtnMoving(bool active)
    {
        btnMoving.enabled = active;
    }

    public void IntoArrageCanvas(bool into)
    {
        panelDefault.alpha = into ? 0 : 1;
        panelDefault.blocksRaycasts = !into;
        groupEtc.SetActive(!into);
        if (furniture.AreaFurniture.isBox) boxCollider.enabled = into;
        else capsCollider.enabled = into;
    }

    public void ActiveClean(bool active)
    {
        canvasBtnClean.alpha = active ? 1 : 0;
        canvasBtnClean.blocksRaycasts = active;
        if (active) messSprite.SetActive(true);
    }

    public void ActiveProgressBar(bool active)
    {
        hairBallSpawner.enabled = active;
        canvasProgressBar.alpha = active ? 1 : 0;
    }

    private void SetProgressBarImg(int index)
    {
        for (int i = 0; i < iconProgress.Length; i++)
        {
            iconProgress[i].alpha = 0;
        }
        iconProgress[index].alpha = 1;
        if (index == 0)
        {
            imgProgressBar.color = new Color(0.66f, 0.53f, 0.74f);
        }
        else if (index == 1)
        {
            imgProgressBar.color = new Color(0.54f, 0.63f, 0.74f);
        }
    }

    public void AbleBtns(bool able)
    {
        canvasBtnFurniture.blocksRaycasts = able;
        canvasBtnCat.blocksRaycasts = able;
    }
    #endregion

    #region Coroutine
    // 프로그래스바 코루틴
    private IEnumerator ProgressUI()
    {
        while (true)
        {
            float progress = remainUsingTime / useTime;
            timer.text = $"{(Math.Truncate(remainUsingTime / 60)).ToString("00")} : {(remainUsingTime % 60).ToString("00")} / {(Math.Truncate(useTime / 60)).ToString("00")} : {(useTime % 60).ToString("00")}";

            SetProgress(progress);
            if (progress <= 0)
            {
                EndProgress();
                timer.text = null;
                yield break;
            }
            else if (cat == null)
            {
                timer.text = null;
                yield break;
            }
            remainUsingTime -= Time.deltaTime;
            yield return null;
        }
    }

    // 프로그래스바 코루틴
    private IEnumerator Cleaning()
    {
        cleaningObject.SetActive(true);
        SoundManager.Instance.StopEffect();
        SoundManager.Instance.PlayEffect(SoundType.Broom);
        while (true)
        {
            float progress = remainCleaningTime / defaultCleaningTime;
            timer.text = $"00 : {(remainCleaningTime % 60).ToString("00")} / 00 : {(defaultCleaningTime % 60).ToString("00")}";
            SetProgress(progress);
            if (remainCleaningTime <= 0)
            {
                EndCleaning();
                timer.text = null;
                yield break;
            }
            remainCleaningTime -= Time.deltaTime;
            yield return null;
        }
    }
    #endregion

    #region Collider

    public void ExitStore()
    {
        if (conflicts.Count == 0) ChangeState(FurniturePlacementState.Moving);
        else ChangeState(FurniturePlacementState.CantPlace);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (FurniturePlacement.Instance.movingFurniture && FurniturePlacement.Instance.selectedIndex == index && !FurniturePlacement.Instance.toStore)
        {
            if (furniture.AreaType == AreaType.Ceiling || furniture.AreaType == AreaType.Door || furniture.AreaType == AreaType.Stair)
            {
                if (collision.tag == furniture.AreaType.ToString())
                {
                    areaIn = true;
                    if (conflicts.Count == 0 && state == FurniturePlacementState.CantPlace)
                    {
                        ChangeState(FurniturePlacementState.Moving);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (FurniturePlacement.Instance.movingFurniture && FurniturePlacement.Instance.selectedIndex == index && !FurniturePlacement.Instance.toStore)
        {
            if (furniture.AreaType == AreaType.Ceiling || furniture.AreaType == AreaType.Door || furniture.AreaType == AreaType.Stair)
            {
                if (collision.tag == furniture.AreaType.ToString() && state == FurniturePlacementState.Moving)
                {
                    areaIn = false;
                    ChangeState(FurniturePlacementState.CantPlace);
                }
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (FurniturePlacement.Instance.movingFurniture && FurniturePlacement.Instance.selectedIndex == index && !FurniturePlacement.Instance.toStore)
        {
            conflicts.Add(collision.gameObject);
            if (!FurniturePlacement.Instance.toStore) ChangeState(FurniturePlacementState.CantPlace);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (FurniturePlacement.Instance.movingFurniture && FurniturePlacement.Instance.selectedIndex == index && !FurniturePlacement.Instance.toStore)
        {
            conflicts.Remove(collision.gameObject);
            if (!FurniturePlacement.Instance.toStore && areaIn && conflicts.Count == 0) ChangeState(FurniturePlacementState.Moving);
        }
    }
    #endregion

    #region Application Control
    // 어플을 내렸다 올렸을 때 버프 시간 재연동
    private void OnApplicationPause(bool pause)
    {

        if (!pause)
        {
            LocalNotifyUtils.ClearNotification();
            if (cat == null) return;
            // 프로그래스바 세팅
            if (cat.state == CatState.UsingFurniture)
            {
                UpdateRemainProgress();
            }
            // 버프 세팅
            if (furniture.state == FurnitureState.Cleaning)
            {
                remainCleaningTime = furniture.cleanTime + defaultCleaningTime - TimeUtils.GetCurrentTime();
            }
        }
        else
        {
            if (remainUsingTime > 0 && Authentication.Inst.userData.endTutorial) LocalNotifyUtils.SpanTimeNotification(remainUsingTime, "가구 사용 완료", $"[{furniture.Name}] 해당 가구 사용이 완료되었어요!", Color.white);
        }
    }
    #endregion
}
