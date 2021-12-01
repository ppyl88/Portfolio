using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_StorageSell : MonoBehaviour
{
    [SerializeField] private Image imgProduct = null;
    [SerializeField] private TextMeshProUGUI txtName = null;
    [SerializeField] private TextMeshProUGUI txtCount = null;
    [SerializeField] private TextMeshProUGUI txtPrice = null;
    [SerializeField] private CanvasGroup panelCount = null;
    [SerializeField] private Button btnCountDown = null;
    [SerializeField] private Button btnCountUp = null;

    public int finalprice;
    public int count;
    private int price;
    private int maxcount;

    public void SetSellUI(FurnitureData furniture)
    {
        count = 1;
        price = furniture.SellingPrice;
        finalprice = count * price;

        imgProduct.sprite = furniture.Sprite;
        txtName.text = furniture.Name;
        txtCount.text = count.ToString();
        txtPrice.text = finalprice.ToString();
        panelCount.alpha = 0;
        panelCount.blocksRaycasts = false;
    }

    public void SetSellUI(ItemData item)
    {
        count = 1;
        price = item.SellingPrice;
        finalprice = count * price;
        maxcount = item.count;

        imgProduct.sprite = item.Sprite;
        txtName.text = item.Name;
        txtCount.text = count.ToString();
        txtPrice.text = finalprice.ToString();
        panelCount.alpha = 1;
        panelCount.blocksRaycasts = true;
        //SetCountUI();
    }

    public void OnClickCountUp()
    {
        count++;

        if(count > maxcount)
        {
            count -= maxcount;
        }

        finalprice = count * price;
        txtCount.text = count.ToString();
        txtPrice.text = finalprice.ToString();
        //SetCountUI();
    }

    public void OnClickCountDown()
    {
        count--;

        if (count < 1)
        {
            count = maxcount;
        }

        finalprice = count * price;
        txtCount.text = count.ToString();
        txtPrice.text = finalprice.ToString();
        //SetCountUI();
    }

    private void SetCountUI()
    {
        btnCountDown.interactable = true;
        btnCountUp.interactable = true;

        if (count == 1)
        {
            btnCountDown.interactable = false;
        }
        if (count == maxcount)
        {
            btnCountUp.interactable = false;
        }
    }
}
