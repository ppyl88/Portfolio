using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListItem_AdoptableCat : MonoBehaviour
{
    public int catIndex;
    public Button btnSelectCat;
    [SerializeField] private Image imgCat= null;
    [SerializeField] private TextMeshProUGUI txtCatName= null;
    [SerializeField] private CanvasGroup imgChecked= null;
    [SerializeField] private TextMeshProUGUI txtSelected= null;

    // 고양이 이미지 세팅
    public void Init(int i, CatData cat)
    {
        catIndex = i;
        if (cat.IconSprite == null)
        {
            imgCat.sprite = cat.SitSprite;
        }
        else
        {
            imgCat.sprite = cat.IconSprite;
        }
        txtCatName.text = cat.name;
        UnCheck();
    }

    // 선택
    public void Check()
    {
        txtSelected.text = TableDataManager.Instance.table_String["Adopt/UI/Selected"].Contents[(int)UI_Setting.language];
        imgChecked.alpha = 1;
        imgChecked.blocksRaycasts = true;
    }

    // 선택 해제
    public void UnCheck()
    {
        imgChecked.alpha = 0;
        imgChecked.blocksRaycasts = false;
    }
}
