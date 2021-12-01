using UnityEngine;
using UnityEngine.UI;

public class UI_Buff : MonoBehaviour
{
    [SerializeField] private Image imgProgress = null;
    [SerializeField] private Image imgBuff = null;

    public void StartBuff(Sprite spriteBuff)
    {
        imgBuff.sprite = spriteBuff;
        SetProgress(1f);
        GetComponent<CanvasGroup>().alpha = 1;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    public void EndBuff()
    {
        GetComponent<CanvasGroup>().alpha = 0;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    public void SetProgress(float progress)
    {
        progress = progress < 0 ? 0 : progress;
        imgProgress.fillAmount = progress;
    }
}
