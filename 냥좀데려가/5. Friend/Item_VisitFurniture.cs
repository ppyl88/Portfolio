using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_VisitFurniture : MonoBehaviour
{
    public FurnitureData furniture;
    public GameObject catObject = null;
    [SerializeField] private Item_VisitCat catItem = null;
    [SerializeField] private BoxCollider2D boxTrigger = null;
    [SerializeField] private SpriteRenderer spriteRenderer = null;
    [SerializeField] private GameObject messSprite = null;
    [SerializeField] private SpriteRenderer srMessSprite = null;
    [SerializeField] private GameObject[] markers = null;

    public void SetData(FurnitureData furnitureData)
    {
        furniture = furnitureData;
        spriteRenderer.sprite = furnitureData.Sprite;
        srMessSprite.size = furnitureData.Sprite.rect.size * 0.01f;
        boxTrigger.size = furniture.Sprite.rect.size * 0.01f;
        switch (furniture.AreaType)
        {
            case AreaType.Floor:
                gameObject.tag = "VisitFloorFurniture";
                break;
            case AreaType.Mat:
                spriteRenderer.sortingLayerName = "VisitBackground";
                break;
        }
        if (furniture.state == FurnitureState.Dirty)
        {
            messSprite.SetActive(true);
        }
        else if (furniture.state == FurnitureState.UsingItem)
        {
            spriteRenderer.sprite = furniture.SubSprite;
        }
        for (int i = 0; i < furniture.AreaCat.Length; i++)
        {
            markers[i].SetActive(true);
            markers[i].GetComponent<Transform>().localPosition = furniture.AreaCat[i].pos;
        }
    }

    public void ResetData()
    {
        furniture = null;
        messSprite.SetActive(false);
        ChangeOrderLayer(0);
        spriteRenderer.sortingLayerName = "Visit";
        gameObject.tag = "Untagged";
        catObject.SetActive(false);
    }

    public void ArrangeCat(CatData catData, int i_marker)
    {
        catObject.SetActive(true);
        catItem.SetData(catData, furniture.AreaCat[i_marker]);
    }

    public void ChangeOrderLayer(int _order)
    {
        spriteRenderer.sortingOrder = _order;
        srMessSprite.sortingOrder = _order + 1;
        catItem.SetOrder(_order + 1);
    }
}
