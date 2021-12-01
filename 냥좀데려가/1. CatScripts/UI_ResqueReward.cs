using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ResqueReward : MonoBehaviour
{
    [Header("SuccessResquePanel")]
    [SerializeField] private CanvasGroup panelSuccessResque = null;
    [SerializeField] private RectTransform rectCatBag = null;
    [SerializeField] private CanvasGroup imgCatBagNormal = null;
    [SerializeField] private CanvasGroup imgCatBagSpecial = null;

    // 고양이 가방 흔들림 변수들
    private float shakeWaitSec = 0.05f;  // 고양이 가방 세부 흔들림 시간간격
    private float shakeAmountX = 25f;    // 고양이 가방 좌우 흔들림 최대치
    private float shakeAmountY = 5f;     // 고양이 가방 상하 흔들림 최대치
    private int shakeTime = 15;          // 고양이 가방 흔들림 횟수     
    private float shakeTerm = 2f;        // 고양이 가방 흔들림 시간 간격

    [Header("WashCatPanel")]
    [SerializeField] private Camera camera_main = null;         // 메인카메라
    [SerializeField] private CanvasGroup panelWashCat = null;
    [SerializeField] private Image imgCatWashed = null;
    [SerializeField] private Image imgCatShadow = null;
    [SerializeField] private GameObject maskCat = null;
    [SerializeField] private GameObject imgBrush = null;         // 고양이 브러쉬 이미지
    [SerializeField] private ParticleSystem bubbleParticle = null; // 거품 파티클
    private int scratchRadius = 50;     // 고양이 브러쉬 사이즈

    // 고양이 얼룩제거용 변수들
    Texture2D maskTexture;
    RectTransform rtMaskCat;
    private int maskWidth, maskHeight;
    private int washCount = 0;
    private int completePercent = 72;    // 얼룩제거 완료 기준 퍼센트

    [Header("NewCatInfoPanel")]
    [SerializeField] private CanvasGroup panelNewCatInfo = null;
    [SerializeField] private Image imgCat = null;
    [SerializeField] private Image imgCatBG = null;
    [SerializeField] private TextMeshProUGUI txtName = null;
    [SerializeField] private TextMeshProUGUI txtType = null;
    [SerializeField] private TextMeshProUGUI[] txtStat = null;

    // 고양이 후광 변수들
    private float rotationDegree = 10f;      // 새 고양이 후광 변경 각도
    private float rotationWaitSec = 0.2f;    // 새 고양이 후광 시간 간격

    // 보상 고양이 정보
    private CatData catData;

    // 현재 수행중인 코루틴 제어용 변수
    private bool doCoroutine;

    private void Start()
    {
        // 고양이 얼룩제거 관련 변수 초기화
        rtMaskCat = maskCat.GetComponent<RectTransform>();
        maskWidth = (int)rtMaskCat.sizeDelta.x;
        maskHeight = (int)rtMaskCat.sizeDelta.y;

        maskTexture = new Texture2D(maskWidth, maskHeight);
        maskCat.GetComponent<Image>().material.mainTexture = maskTexture;
        bubbleParticle.Stop();
    }

    // 구조 보상 UI 세팅 (고양이 가방 단계)
    public void SetResqueReward()
    {
        catData = Authentication.Inst.userData.cats[Authentication.Inst.userData.cats.Count - 1];

        // 고양이 등급에 따른 고양이 가방 변경
        if (catData.LevelIndex == 1)
        {
            imgCatBagNormal.alpha = 1;
            imgCatBagSpecial.alpha = 0;
        }
        else if (catData.LevelIndex == 2)
        {
            imgCatBagNormal.alpha = 0;
            imgCatBagSpecial.alpha = 1;
        }

        // 고양이 가방 흔들림 관련 변수 초기화
        doCoroutine = true;
        // 고양이 가방 흔들림 코루틴 실행
        StartCoroutine(ShakeCatBag(rectCatBag));



        // 패널 설정
        UIManager.instance.OpenPopUpChildControl(-1);

        panelSuccessResque.alpha = 1;
        panelSuccessResque.blocksRaycasts = true;
        panelWashCat.alpha = 0;
        panelWashCat.blocksRaycasts = false;
        panelNewCatInfo.alpha = 0;
        panelNewCatInfo.blocksRaycasts = false;
    }

    // 고양이 가방 화면 클릭 이벤트 함수
    public void OnClickCatBag()
    {
        doCoroutine = false;
        OpenCatBag();
    }

    // 새 고양이 화면 클릭 이벤트 함수
    public void OnClickNewCat()
    {
        doCoroutine = false;
        UIManager.instance.OpenPopUpChildControl(0);
    }

    // 고양이 가방 오픈 후 얼룩제거 단계 세팅
    private void OpenCatBag()
    {
        SoundManager.Instance.StopEffect();
        SoundManager.Instance.SetEffect(SoundType.BrushCat, true);
        imgCatWashed.sprite = catData.StandSprite;
        imgCatShadow.sprite = catData.StandSprite;

        // 얼룩 제거 관련 변수 초기화
        washCount = 0;
        Color32[] cols = maskTexture.GetPixels32();
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i] = new Color32(255, 0, 0, 255);
        }
        maskTexture.SetPixels32(cols);
        maskTexture.Apply(false);
        // 얼룩 제거 코루틴 실행
        StartCoroutine(WashCatbyScretch());

        // 패널 설정
        panelSuccessResque.alpha = 0;
        panelSuccessResque.blocksRaycasts = false;
        panelWashCat.alpha = 1;
        panelWashCat.blocksRaycasts = true;
    }

    private void ShowNewCat()
    {
        SoundManager.Instance.PlayEffect(SoundType.Fanfare);
        imgCat.sprite = catData.StandSprite;

        // 고양이 등급에 따른 후광 색깔 지정
        Color color = new Color(255, 255, 255);
        if (catData.LevelIndex == 1)
        {
            color = new Color(255, 255, 0, 255);
        }
        else if (catData.LevelIndex == 2)
        {
            color = new Color(255, 0, 0, 255);
        }
        imgCatBG.color = color;

        // 후광 회전 관련 변수 초기화
        doCoroutine = true;
        Transform transNewCatBG = imgCatBG.gameObject.transform;
        // 후광 회전 코루틴 실행
        StartCoroutine(NewCatBGEffect(transNewCatBG));

        // 고양이 상세 정보 세팅
        txtName.text = catData.name;
        txtType.text = catData.TypeName;
        for (int i = 0; i < catData.stat.Length; i++)
        {
            txtStat[i].text = catData.stat[i].ToString();
        }

        // 패널 설정
        panelWashCat.alpha = 0;
        panelWashCat.blocksRaycasts = false;
        panelNewCatInfo.alpha = 1;
        panelNewCatInfo.blocksRaycasts = true;
    }

    // 화면 스크래치 함수
    private void Scratch(int xCenter, int yCenter)
    {
        int xOffset, yOffset, xPos, yPos, yRange;
        Color32[] tempArray = maskTexture.GetPixels32();
        bool hasChanged = false;

        for (xOffset = -scratchRadius; xOffset <= scratchRadius; xOffset++)
        {
            yRange = (int)Mathf.Ceil(Mathf.Sqrt(scratchRadius * scratchRadius - xOffset * xOffset));
            for (yOffset = -yRange; yOffset <= yRange; yOffset++)
            {
                xPos = xCenter + xOffset;
                yPos = yCenter + yOffset;
                hasChanged = TryScratchPixel(xPos, yPos, ref tempArray) || hasChanged;
            }
        }
        if (hasChanged)
        {
            maskTexture.SetPixels32(tempArray);
            maskTexture.Apply(false);
        }
    }

    // 스크래치된 픽셀 투명화 유무 체크 함수
    private bool TryScratchPixel(int xPos, int yPos, ref Color32[] pixels)
    {
        if (xPos >= 0 && xPos < maskWidth && yPos >= 0 && yPos < maskHeight)
        {
            int index = yPos * maskWidth + xPos;
            if (pixels[index].a != 0)
            {
                pixels[index].a = 0;
                washCount++; // 진행도 파악을 위한 카운팅
                return true;
            }
        }
        return false;
    }

    // 고양이 얼룩 제거용 브러쉬 생성 함수
    private void ChaseMouse(GameObject gameObject)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, camera_main, out Vector2 localPosInRect);
        gameObject.transform.localPosition = localPosInRect;
    }

    // 고양이 가방 흔들림 코루틴
    private IEnumerator ShakeCatBag(Transform transform)
    {
        Vector3 initialPosition = transform.localPosition;
        while (doCoroutine)
        {
            for (int i = 0; i < shakeTime; ++i)
            {
                float ShakePosX = Random.Range(-shakeAmountX, shakeAmountX);
                float ShakePosY = Random.Range(-shakeAmountY, shakeAmountY);
                transform.localPosition = new Vector3(ShakePosX, ShakePosY) + initialPosition;
                
                yield return YieldInstructionCache.WaitForSeconds(shakeWaitSec);
            }
            transform.localPosition = initialPosition;
            SoundManager.Instance.PlayEffect(SoundType.CatinBag);
            yield return YieldInstructionCache.WaitForSeconds(shakeTerm);
        }
    }

    // 고양이 얼룩 제거 코루틴
    private IEnumerator WashCatbyScretch()
    {
        CanvasGroup imgBrushCanvasGroup = imgBrush.GetComponent<CanvasGroup>();
        while ((float)washCount / (float)(maskWidth * maskHeight) * 100 < completePercent)
        {
            if (Input.GetMouseButtonDown(0))
            {
                imgBrushCanvasGroup.alpha = 1;
                SoundManager.Instance.PlayEffect();
                ParticleControl(true);
            }
            if (Input.GetMouseButton(0))
            {
                ChaseMouse(imgBrush);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rtMaskCat, Input.mousePosition, camera_main, out Vector2 localPosInRect);
                Scratch((int)localPosInRect.x, (int)localPosInRect.y);
            }
            if (Input.GetMouseButtonUp(0))
            {
                imgBrushCanvasGroup.alpha = 0;
                SoundManager.Instance.PauseEffect();
                ParticleControl(false);
            }
            yield return null;
        }
        imgBrushCanvasGroup.alpha = 0;
        SoundManager.Instance.StopEffect();
        ParticleControl(false);
        ShowNewCat();
    }

    // 새 고양이 후광 효과 코루틴
    private IEnumerator NewCatBGEffect(Transform transform)
    {
        while (doCoroutine)
        {
            transform.Rotate(0, 0, rotationDegree);
            yield return YieldInstructionCache.WaitForSeconds(rotationWaitSec);

        }
        transform.Rotate(0, 0, 0);
    }


    // 파티클 효과
    private void ParticleControl(bool _sw)
    {
        if (_sw)
        {
            if(bubbleParticle.isPlaying == true)
            {
                bubbleParticle.Stop();
                bubbleParticle.Clear();
                bubbleParticle.Play();
            }
            else
            {
                bubbleParticle.Play();
            }
        }
        else
        {
            if (bubbleParticle.isPlaying == true)
            {
                StartCoroutine(CoStopPaticle());
            }
        }
    }

    WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);
    private IEnumerator CoStopPaticle()
    {
        yield return waitForSeconds;
        bubbleParticle.Stop();
        bubbleParticle.Clear();
        
    }
}
