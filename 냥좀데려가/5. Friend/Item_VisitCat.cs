using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Item_VisitCat : CatFSMManager
{
    [SerializeField] private GameObject Go_NomalCat, Go_LittlePrinceCat, Go_RedhoodCat;
    private GameObject Go_CurrentCat;

    public void SetData(CatData catData, CatArea _catArea)
    {
        currentCat = catData;
        randomAnimation = new List<EAnimation>();

        CatSelector(currentCat.ResourceIndex);
        StatAnalysis(currentCat.stat);
        transform.localPosition = new Vector3(_catArea.pos.x, _catArea.pos.y, 0);
        transform.localScale = _catArea.arrow == 1 ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        eCurrentAnimation = EAnimation.None;
        CatFSM();
    }

    public void SetData(CatData catData)
    {
        currentCat = catData;
        CatSelector(currentCat.ResourceIndex);
        StatAnalysis(currentCat.stat);
        eCurrentAnimation = EAnimation.None;
        CatFSM();
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
        SortingGroup sorting = transform.GetComponent<SortingGroup>();
        int sorting_other = 0;
        switch (other.tag)
        {
            case "CatBlockTop":
                arrowV = 2;
                arrowH = Random.Range(0, 2) == 0 ? false : true;
                break;
            case "CatBlockBottom":
                arrowV = 1;
                arrowH = Random.Range(0, 2) == 0 ? false : true;
                break;
            case "CatBlockLeft":
                arrowH = false;
                arrowV = Random.Range(0, 3);
                break;
            case "CatBlockRight":
                arrowH = true;
                arrowV = Random.Range(0, 3);
                break;
            case "VisitCat":
                if (other.transform.GetComponent<Item_VisitCat>().currentCat.state != CatState.InInterior) return;
                sorting_other = other.transform.GetComponent<SortingGroup>().sortingOrder;
                if (other.transform.position.y > transform.position.y)
                {
                    if (sorting.sortingOrder < sorting_other + 1) sorting.sortingOrder = sorting_other + 1;
                }
                else
                {
                    if (sorting.sortingOrder > sorting_other - 1) sorting.sortingOrder = sorting_other - 1;
                }
                if (eCurrentAnimation == EAnimation.Walk)
                {
                    arrowV = Random.Range(0, 3);
                    arrowH = !arrowH;
                }
                break;
            case "VisitFloorFurniture":
                sorting_other = other.transform.GetComponent<SpriteRenderer>().sortingOrder;
                FurnitureData furniture = other.transform.GetComponent<Item_VisitFurniture>().furniture;
                if (other.transform.position.y + furniture.AreaFurniture.offset.y > transform.position.y)
                {
                    if (sorting.sortingOrder < sorting_other + 4) sorting.sortingOrder = sorting_other + 4;
                }
                else
                {
                    if (sorting.sortingOrder > sorting_other - 2) sorting.sortingOrder = sorting_other - 2;
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
            case "VisitFloorFurniture":
                if (eCurrentAnimation != EAnimation.Walk)
                {
                    eCurrentAnimation = EAnimation.Walk;
                }
                break;
        }
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
            SetAnimation(eAnimation);
            eCurrentAnimation = eAnimation;
        }
    }
    #endregion
}
