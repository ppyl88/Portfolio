using UnityEngine;
using UnityEngine.Rendering;

public class Item_Gift : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        SpriteRenderer sorting = transform.GetComponent<SpriteRenderer>();
        float offset_y = transform.GetComponent<BoxCollider2D>().offset.y * 0.5f;
        float size_y = transform.GetComponent<BoxCollider2D>().size.y * 0.5f;
        if (other.tag == "VisitFloorFurniture")
        {

            int sorting_other = other.transform.GetComponent<SpriteRenderer>().sortingOrder;
            FurnitureData furniture = other.transform.GetComponent<Item_VisitFurniture>().furniture;
            if (other.transform.position.y + furniture.AreaFurniture.offset.y > transform.position.y + offset_y - size_y/4)
            {
                if (sorting.sortingOrder < sorting_other + 3) sorting.sortingOrder = sorting_other + 3;
            }
            else
            {
                if (sorting.sortingOrder > sorting_other - 3) sorting.sortingOrder = sorting_other - 3;
            }
        }
        else if (other.tag == "VisitCat")
        {
            if (other.transform.GetComponent<Item_VisitCat>().currentCat.state != CatState.InInterior) return;
            int sorting_other = other.transform.GetComponent<SortingGroup>().sortingOrder;
            if (other.transform.position.y > transform.position.y + offset_y - size_y/4)
            {
                if (sorting.sortingOrder < sorting_other + 1) sorting.sortingOrder = sorting_other + 1;
            }
            else
            {
                if (sorting.sortingOrder > sorting_other - 1) sorting.sortingOrder = sorting_other - 1;
            }
        }
    }
}
