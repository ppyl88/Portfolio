using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListItem_EventResque : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI EventResque = null;

    [SerializeField] private CanvasGroup imgEventPriceGold = null;
    [SerializeField] private CanvasGroup imgEventPriceCoin = null;
    [SerializeField] private TextMeshProUGUI txtEventName = null;
    [SerializeField] private TextMeshProUGUI txtEventTime = null;
    [SerializeField] private TextMeshProUGUI txtEventDesc = null;
    [SerializeField] private Image imgIconEvent = null;
    [SerializeField] private TextMeshProUGUI txtEventCost = null;

    // 이벤트 구조대 UI 세팅
    public void Init(int resqueIndex)
    {
        EventResque.text = TableDataManager.Instance.table_String["Resque/UI/EventResque"].Contents[(int)UI_Setting.language];

        Resque_Area resque = TableDataManager.Instance.table_Resque_Area[resqueIndex];

        txtEventName.text = resque.ResqueName;
        string eventTime;
        if (resque.Time_End == "UnLimit")
        {
            eventTime = TableDataManager.Instance.table_String["Resque/UI/EventUnLimit"].Contents[(int)UI_Setting.language];
        }
        else
        {
            eventTime = TableDataManager.Instance.table_String["Resque/UI/EventTime"].Contents[(int)UI_Setting.language];
            eventTime = eventTime.Replace("month", resque.Time_End.Substring(4, 2));
            eventTime = eventTime.Replace("day", resque.Time_End.Substring(6, 2));
            eventTime = eventTime.Replace("hh", resque.Time_End.Substring(8, 2));
            eventTime = eventTime.Replace("mm", resque.Time_End.Substring(10, 2));
        }
        txtEventTime.text = eventTime;
        txtEventDesc.text = resque.ResqueDesc;
        imgIconEvent.sprite = resque.ResqueIcon;
        switch (resque.Price_Type)
        {
            case FeeType.Gold:
                imgEventPriceGold.alpha = 1;
                imgEventPriceCoin.alpha = 0;
                break;
            case FeeType.Coin:
                imgEventPriceGold.alpha = 0;
                imgEventPriceCoin.alpha = 1;
                break;
        }
        txtEventCost.text = string.Format("{0:#,###}", resque.Price);
    }
}
