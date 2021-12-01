using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListItem_CatData : MonoBehaviour
{
    public CatData catData;
    public Button btnCatData;
    [SerializeField] private Image imgMask = null;
    [SerializeField] private Image imgCat = null;
    [SerializeField] private TextMeshProUGUI txtName = null;
    [SerializeField] private TextMeshProUGUI txtLevel = null;
    [SerializeField] private CanvasGroup imgInFuniture = null;
    [SerializeField] private CanvasGroup imgInInterior = null;

    // 고양이 리스트 아이템 초기화
    public void Init(CatData cat)
    {
        imgMask.color = new Color(1, 1, 1, 150f / 255f);
        catData = cat;
        btnCatData.gameObject.GetComponent<CanvasGroup>().alpha = 1;
        btnCatData.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        if(cat.IconSprite == null)
        {
            imgCat.sprite = cat.SitSprite;
        }
        else
        {
            imgCat.sprite = cat.IconSprite;
        }
        txtName.text = cat.name;
        txtLevel.text = cat.level.ToString();
        ChangeInOutFuniture(cat.state);
    }

    // 고양이 이름 변경 함수
    public void ChangeName(string name)
    {
        txtName.text = name;
    }

    // 고양이 레벨 변경 함수
    public void ChangeLevel(int level)
    {
        txtLevel.text = level.ToString();
    }

    // 고양이 가구 배치 유무에 따른 마스크 함수
    public void ChangeInOutFuniture(CatState state)
    {
        if (state == CatState.Free)
        {
            imgInFuniture.alpha = 0;
            imgInInterior.alpha = 0;
        }
        else if (state == CatState.InInterior)
        {
            imgInInterior.alpha = 1;
        }
        else
        {
            imgInFuniture.alpha = 1;
        }
    }
}
