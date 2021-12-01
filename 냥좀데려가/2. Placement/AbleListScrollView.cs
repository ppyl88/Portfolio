using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AbleListScrollView : ScrollViewControl_Horizontal<ListItem_Able, AbleData>
{
    [SerializeField] private CanvasGroup panelBtnShow = null;       // 리스트창 숨겼을 때 패널
    [SerializeField] private CanvasGroup panelList = null;          // 리스트창 패널
    [SerializeField] private CanvasGroup panelMessage = null;       // 리스트 메시지창 패널
    [SerializeField] private ScrollRect scrollRect = null;          // 리스트 ScrollRect
    [SerializeField] private Button btnLeft = null;                 // 리스트 좌버튼
    [SerializeField] private Button btnRight = null;                // 리스트 우버튼
    [SerializeField] private TextMeshProUGUI txtMessage = null;     // 메세지 TextMesh

    private float speed = 1000f;
    private float move;
    private bool left, right;

    // 배치가능 고양이/가구 리스트 표시
    public void VisibleList(bool show)
    {
        panelBtnShow.alpha = show ? 0 : 1;
        panelBtnShow.blocksRaycasts = !show;
        panelList.alpha = show ? 1 : 0;
        panelList.blocksRaycasts = show;
    }

    // 표시 버튼 세팅
    public void ButtonSetting()
    {
        if (scrollRect.content.rect.width > scrollRect.viewport.rect.width)
        {
            if (scrollRect.horizontalNormalizedPosition < 0.001f)
            {
                btnLeft.interactable = false;
                btnRight.interactable = true;
            }
            else if (scrollRect.horizontalNormalizedPosition > 0.999f)
            {
                btnLeft.interactable = true;
                btnRight.interactable = false;
            }
            else
            {
                btnLeft.interactable = true;
                btnRight.interactable = true;
            }
        }
        else
        {
            btnLeft.interactable = false;
            btnRight.interactable = false;
        }
    }

    // 좌 버튼 커서 인 감지
    public void InBtnLeft()
    {
        left = true;
        move = speed * Time.deltaTime / scrollRect.content.rect.width;
        StartCoroutine(CoLeft());
    }

    // 좌 버튼 커서 아웃 감지
    public void OutBtnLeft()
    {
        left = false;
    }

    // 우 버튼 커서 인 감지
    public void InBtnRight()
    {
        right = true;
        move = speed * Time.deltaTime / scrollRect.content.rect.width;
        StartCoroutine(CoRight());
    }

    // 우 버튼 커서 아웃 감지
    public void OutBtnRight()
    {
        right = false;
    }

    // 리스트 좌 이동 코루틴
    private IEnumerator CoLeft()
    {
        while (btnLeft.interactable && left)
        {
            scrollRect.horizontalNormalizedPosition -= move;
            yield return null;
        }
    }

    // 리스트 우 이동 코루틴
    private IEnumerator CoRight()
    {
        while (btnRight.interactable && right)
        {
            scrollRect.horizontalNormalizedPosition += move;
            yield return null;
        }
    }

    // 리스트 이용 불가 시 띄우는 메세지창 함수
    public void MaskMessage(string msg)
    {
        if(msg == "")
        {
            panelMessage.alpha = 0;
            panelMessage.blocksRaycasts = false;
        }
        else
        {
            txtMessage.text = msg;
            panelMessage.alpha = 1;
            panelMessage.blocksRaycasts = true;
        }
    }
}
