using UnityEngine;
using System;
using System.Collections.Generic;

public class catPointer : MonoBehaviour
{
    public CatData cat = null;                                              // 포인터 고양이 데이터
    [HideInInspector] public CatPointerState state;                         // 고양이 포인터 상태
    [SerializeField] private SpriteRenderer spriteRenderer = null;          // 스프라이트 랜더러
    [SerializeField] private FurniturePlacement furniturePlacement = null;

    // 고양이에 닿은 가구 정보(가구 및 마커 인덱스) 저장 클래스
    class TriggerFur
    {
        public Item_Furniture fur;
        public int i_marker;
        public TriggerFur(Item_Furniture fur, int i_marker)
        {
            this.fur = fur;
            this.i_marker = i_marker;
        }
    }

    // 고양이에 닿은 가구  리스트
    private List<TriggerFur> triggerFur;

    // 고양이 포인터 데이터 설정
    public void SetData(CatData catData)
    {
        cat = catData;
        spriteRenderer.sprite = cat.SitSprite;
        triggerFur = new List<TriggerFur>();
        ChangeCatPointerState(CatPointerState.Default);
        gameObject.SetActive(true);
    }

    // 고양이 포인터 상태 변경 함수
    public void ChangeCatPointerState(CatPointerState _state)
    {
        state = _state;
        switch(state)
        {
            case CatPointerState.Default:
            case CatPointerState.Selected:
                spriteRenderer.color = Color.white;
                break;
            case CatPointerState.StoreCat:
                spriteRenderer.color = Color.gray;
                break;
            case CatPointerState.OutStore:
                if(triggerFur.Count==0)
                {
                    ChangeCatPointerState(CatPointerState.Default);
                }
                else
                {
                    ChangeCatPointerState(CatPointerState.Selected);
                }
                break;
        }
    }

    // 고양이 포인터 충돌 Enter 상태에 따라 상태 변경
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Marker")
        {
            Item_Furniture fur = collision.transform.parent.GetComponentInParent<Item_Furniture>();
            int i_marker = Int32.Parse(collision.gameObject.name);
            triggerFur.Add(new TriggerFur(fur, i_marker));
            if (!FurniturePlacement.Instance.toStore)
            {
                if (triggerFur.Count > 1)
                {
                    triggerFur[triggerFur.Count-2].fur.ChangeState(FurniturePlacementState.CancelArranging, triggerFur[triggerFur.Count - 2].i_marker);
                }
                FurniturePlacement.Instance.selectedIndex = fur.index;
                FurniturePlacement.Instance.selectedMarker = i_marker;
                ChangeCatPointerState(CatPointerState.Selected);
                fur.ChangeState(FurniturePlacementState.Arranging, FurniturePlacement.Instance.selectedMarker);
            }
        }
    }

    // 고양이 포인터 충돌 Exit 상태에 따라 상태 변경
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Marker")
        {
            Item_Furniture fur = collision.transform.parent.GetComponentInParent<Item_Furniture>();
            int i_marker = Int32.Parse(collision.gameObject.name);
            bool iscurrent = false;
            int index = triggerFur.FindIndex(temp => temp.fur == fur && temp.i_marker == i_marker);
            if (index == triggerFur.Count - 1) iscurrent = true;
            triggerFur.RemoveAt(index);
            if (!FurniturePlacement.Instance.toStore)
            {
                if (iscurrent)
                {
                    fur.ChangeState(FurniturePlacementState.CancelArranging, FurniturePlacement.Instance.selectedMarker);
                    if(triggerFur.Count == 0)
                    {
                        ChangeCatPointerState(CatPointerState.Default);
                        FurniturePlacement.Instance.selectedIndex = -1;
                        FurniturePlacement.Instance.selectedMarker = -1;
                    }
                    else
                    {
                        FurniturePlacement.Instance.selectedIndex = triggerFur[triggerFur.Count-1].fur.index;
                        FurniturePlacement.Instance.selectedMarker = triggerFur[triggerFur.Count - 1].i_marker;
                        triggerFur[triggerFur.Count - 1].fur.ChangeState(FurniturePlacementState.Arranging, FurniturePlacement.Instance.selectedMarker);
                    }
                }
            }
        }
    }
}
