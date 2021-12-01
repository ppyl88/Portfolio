using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Item_Cat : CatFSMManager
{
    // 0 ~ 23 / 1 / 1
    [SerializeField] private GameObject Go_NomalCat, Go_LittlePrinceCat, Go_RedhoodCat;
    [SerializeField] private Sprite[] Emotions;
    [SerializeField] private Sprite[] MessageBox;
    [SerializeField] private Image Image_Emotion, Image_MessageBox;
    [SerializeField] private Button Button_Emotion;
    [SerializeField] private CanvasGroup canvasBtnEnd = null;
    [SerializeField] private GameObject[] EmotionEffect;
    private GameObject Go_CurrentCat;
    // First Setting
    public void SetData(CatData catData, CatArea _catArea)
    {
        currentCat = catData;
        randomAnimation = new List<EAnimation>();

        ActiveBtnEnd(false);
        CatSelector(currentCat.ResourceIndex);
        StatAnalysis(currentCat.stat);
        transform.localPosition = new Vector3(_catArea.pos.x, _catArea.pos.y, 0);
        transform.localScale = _catArea.arrow == 1 ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        eCurrentAnimation = EAnimation.None;
        CatFSM();
    }

    public void OnClickEnd()
    {
        CatManager.Instance.StoreCat(Authentication.Inst.userData.cats.IndexOf(currentCat));
    }

    public void ActiveBtnEnd(bool active)
    {
        canvasBtnEnd.alpha = active ? 1 : 0;
        canvasBtnEnd.blocksRaycasts = active;
    }

    public void SetOrder(int _order)
    {
        sortingGroup.sortingOrder = _order;
    }

    public void SetLayer(string _layer)
    {
        sortingGroup.sortingLayerName = _layer;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentCat.state != CatState.InInterior) return;

        switch (other.tag)
        {
            case "CatBlockTop":
                arrowV = 2;
                arrowH = UnityEngine.Random.Range(0, 2) == 0 ? false : true;
                break;
            case "CatBlockBottom":
                arrowV = 1;
                arrowH = UnityEngine.Random.Range(0, 2) == 0 ? false : true;
                break;
            case "CatBlockLeft":
                arrowH = false;
                arrowV = UnityEngine.Random.Range(0, 3);
                break;
            case "CatBlockRight":
                arrowH = true;
                arrowV = UnityEngine.Random.Range(0, 3);
                break;
            case "Cat":
                if (other.transform.transform.GetComponent<Item_Cat>().currentCat.state == CatState.UsingFurniture) return;
                if (other.transform.position.y > transform.position.y)
                {
                    transform.GetComponent<SortingGroup>().sortingOrder = other.transform.GetComponent<SortingGroup>().sortingOrder + 5;
                }
                else
                {
                    transform.GetComponent<SortingGroup>().sortingOrder = other.transform.GetComponent<SortingGroup>().sortingOrder - 5;
                }
                if (eCurrentAnimation == EAnimation.Walk)
                {
                    arrowV = UnityEngine.Random.Range(0, 3);
                    arrowH = !arrowH;
                }
                break;
            case "FloorFurniture":
                if ((int)other.transform.GetComponent<Item_Furniture>().catObject.GetComponent<Item_Cat>().currentCat.state > 1)
                {
                    if (other.transform.position.y > transform.position.y)
                    {
                        transform.GetComponent<SortingGroup>().sortingOrder = other.transform.GetComponent<Item_Furniture>().catObject.GetComponent<SortingGroup>().sortingOrder + 5;
                    }
                    else
                    {
                        transform.GetComponent<SortingGroup>().sortingOrder = other.transform.GetComponent<SpriteRenderer>().sortingOrder - 5;
                    }
                }
                else
                {
                    if (other.transform.position.y > transform.position.y)
                    {
                        transform.GetComponent<SortingGroup>().sortingOrder = other.transform.GetComponent<SpriteRenderer>().sortingOrder + 5;
                    }
                    else
                    {
                        transform.GetComponent<SortingGroup>().sortingOrder = other.transform.GetComponent<SpriteRenderer>().sortingOrder - 5;
                    }
                }

                arrowV = 0;
                eCurrentAnimation = EAnimation.Walk;
                break;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "FloorFurniture":
                if (eCurrentAnimation != EAnimation.Walk)
                {
                    eCurrentAnimation = EAnimation.Walk;
                }
                break;
        }
    }

    public void SetData(CatData catData)
    {
        currentCat = catData;
        CatSelector(currentCat.ResourceIndex);
        StatAnalysis(currentCat.stat);
        eCurrentAnimation = EAnimation.None;
        CatFSM();
    }

    #region Cat Selector

    private void CatSelector(int _index)
    {
        if (_index < 24)
        {
            InitCatSelector();
            Go_CurrentCat = Go_NomalCat;
            Go_CurrentCat.SetActive(true);
            Go_CurrentCat.GetComponent<CatAnimation>().SetCurrentSkin(_index);
        }
        else
        {
            switch (_index)
            {
                case 24:
                    InitCatSelector();
                    Go_CurrentCat = Go_LittlePrinceCat;
                    Go_CurrentCat.SetActive(true);
                    Go_CurrentCat.GetComponent<CatAnimation>().SetCurrentSkin(0);
                    break;
                case 25:
                    InitCatSelector();
                    Go_CurrentCat = Go_RedhoodCat;
                    Go_CurrentCat.SetActive(true);
                    Go_CurrentCat.GetComponent<CatAnimation>().SetCurrentSkin(0);
                    break;
            }
        }
    }

    private void InitCatSelector()
    {
        Go_NomalCat.SetActive(false);
        Go_LittlePrinceCat.SetActive(false);
        Go_RedhoodCat.SetActive(false);
    }

    #endregion

    #region Cat Emotion
    public void SetEmotion(int _index)
    {
        Button_Emotion.onClick.RemoveAllListeners();
        foreach (var i in EmotionEffect) i.SetActive(false);
        Image_Emotion.sprite = Emotions[_index];
        int[] addStat = new int[3] { 0, 0, 0 };
        int MessageBoxIndex = 0;
        int addExp = 0;

        switch (_index)
        {
            case 0:
                addExp = 5;
                addStat[2] = 10;
                MessageBoxIndex = 1;
                break;

            case 1:
                addExp = 5;
                addStat[0] = 10;
                MessageBoxIndex = 0;
                break;

            case 2:
                addExp = 20;
                MessageBoxIndex = 1;
                break;

            case 3:
                addExp = 20;
                MessageBoxIndex = 1;
                break;

            case 4:
                addExp = 5;
                addStat[1] = 10;
                MessageBoxIndex = 0;
                break;

            case 5:
                addExp = 5;
                addStat[1] = 10;
                MessageBoxIndex = 1;
                break;

            case 6:
                addExp = 5;
                addStat[2] = 10;
                MessageBoxIndex = 0;
                break;

            case 7:
                addExp = 5;
                addStat[1] = 10;
                MessageBoxIndex = 1;
                break;

            case 8:
                addExp = 5;
                addStat[0] = 10;
                MessageBoxIndex = 0;
                break;

            case 9:
                addExp = 5;
                addStat[0] = 10;
                MessageBoxIndex = 1;
                break;

            case 10:
                addExp = 5;
                addStat[1] = 10;
                MessageBoxIndex = 0;
                break;

            case 11:
                addExp = 5;
                addStat[1] = 10;
                MessageBoxIndex = 1;
                break;

            case 12:
                addExp = 5;
                addStat[0] = 10;
                MessageBoxIndex = 0;
                break;

            case 13:
                addExp = 5;
                addStat[2] = 10;
                MessageBoxIndex = 1;
                break;

        }
        Button_Emotion.onClick.AddListener(() => OnClickEmotion(addStat, addExp));
        Image_MessageBox.sprite = MessageBox[MessageBoxIndex];
        Image_MessageBox.GetComponent<CanvasGroup>().alpha = 1;
        Image_MessageBox.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void OnClickEmotion(int[] _stat, int _exp)
    {
        CatManager.Instance.CatAddStat(Authentication.Inst.userData.cats.IndexOf(currentCat), _stat);
        CatManager.Instance.CatAddExp(Authentication.Inst.userData.cats.IndexOf(currentCat), _exp);
        UserDataManager.Inst.AddCoin(UnityEngine.Random.Range(1, 3));
        try
        {
            EmotionEffect[Array.FindIndex(_stat, i => i == 10)].SetActive(true);
        }
        catch
        {
            EmotionEffect[3].SetActive(true);
        }
        Image_MessageBox.GetComponent<CanvasGroup>().alpha = 0;
        Image_MessageBox.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    #endregion

    #region  Cat Animation
    public void SetAnimation(EAnimation _animation)
    {
        Go_CurrentCat.GetComponent<CatAnimation>().SetCurrentAnimation(_animation);
    }

    [SerializeField] EAnimation eCurrentAnimation;

    private void FixedUpdate()
    {
        if (eAnimation == EAnimation.Walk)
        {
            transform.position = transform.position + Vector3.left * (arrowH ? 1 : -1) * 0.01f + Vector3.up * (arrowV == 2 ? -1 * 0.5f : arrowV * 0.5f) * 0.01f;
        }
        eAnimation = eAnimation == EAnimation.None ? EAnimation.Sit : eAnimation;
        if (!eAnimation.Equals(eCurrentAnimation))
        {
            if (currentCat.state == CatState.InInterior && eCurrentAnimation == EAnimation.Walk)
            {
                SetEmotion(UnityEngine.Random.Range(0, Emotions.Length));
            }
            else
            {
                Image_MessageBox.GetComponent<CanvasGroup>().alpha = 0;
                Image_MessageBox.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
            SetAnimation(eAnimation);
            eCurrentAnimation = eAnimation;
        }
    }
    #endregion
}