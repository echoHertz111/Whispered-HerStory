using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BallController : MonoBehaviour
{
    public GameObject[] balls; // 公共数组，用于在Unity的Inspector中拖入球体
    public AudioClip[] audioClips; // 不同的音频剪辑
    public Button resetButton; // 按钮用于重置已点击的球
    public Sprite clickedSprite; // 点击后的小球图像
    public Sprite hoverSprite; // 悬停时的小球图像
    public Sprite defaultSprite; // 默认的小球图像

    private Dictionary<GameObject, int> ballNumbers = new Dictionary<GameObject, int>(); // 存储每个球体的编号
    private HashSet<GameObject> clickedBalls = new HashSet<GameObject>(); // 用于存储点击的小球
    private int currentStep = 0; // 当前步骤
    private List<GameObject> leftGroup = new List<GameObject>(); // 左组
    private List<GameObject> rightGroup = new List<GameObject>(); // 右组
    private List<Line> lines = new List<Line>(); // 用于存储连线

    void Start()
    {
        // 分配球体编号并分组
        for (int i = 0; i < balls.Length; i++)
        {
            ballNumbers[balls[i]] = i + 1;
            if (i < 5)
            {
                leftGroup.Add(balls[i]);
            }
            else
            {
                rightGroup.Add(balls[i]);
            }
            balls[i].AddComponent<AudioSource>().clip = audioClips[i]; // 添加音频源并设置音频剪辑

            // 添加点击事件
            int index = i; // 捕获当前循环的索引
            Button button = balls[i].GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnBallClicked(balls[index]));
            }

            // 添加悬停和离开悬停区域的事件
            EventTrigger trigger = balls[i].AddComponent<EventTrigger>();

            // 悬停事件
            EventTrigger.Entry entryHover = new EventTrigger.Entry();
            entryHover.eventID = EventTriggerType.PointerEnter;
            entryHover.callback.AddListener((eventData) => OnHoverEnter(balls[index]));
            trigger.triggers.Add(entryHover);

            // 离开悬停区域事件
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((eventData) => OnHoverExit(balls[index]));
            trigger.triggers.Add(entryExit);


        }

        // 绑定按钮点击事件
        resetButton.onClick.AddListener(ResetClickedBalls);

      
    }

    public void OnBallClicked(GameObject clickedBall)
    {
        if (currentStep >= 5) return; // 已完成所有步骤，禁止进一步操作

        // 播放音频
        AudioSource audioSource = clickedBall.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // 更改图像
        Image image = clickedBall.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = clickedSprite; // 将球体图像变为点击后的图像
        }

        clickedBalls.Add(clickedBall);

        // 检查是否满足条件
        StepSuccess(currentStep);
    }

    private void OnHoverEnter(GameObject ball)
    {
        if (clickedBalls.Contains(ball))
        {
            return;
        }
        // 更改图像为悬停状态
        Image image = ball.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = hoverSprite;
        }
    }

    private void OnHoverExit(GameObject ball)
    {
        // 如果该小球已被点击，则不恢复为默认图像
        if (clickedBalls.Contains(ball))
        {
            return;
        }

        // 恢复为默认图像
        Image image = ball.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = defaultSprite;
        }
    }


    private void ConnectBalls()
    {
        List<GameObject> clickedLeftBalls = new List<GameObject>();
        List<GameObject> clickedRightBalls = new List<GameObject>();

        // 找出被点击的左组小球
        foreach (GameObject ball in clickedBalls)
        {
            if (leftGroup.Contains(ball))
            {
                clickedLeftBalls.Add(ball);
            }

            if (rightGroup.Contains(ball))
            {
                clickedRightBalls.Add(ball);
            }
        }

        // 将被点击的左组小球与右组的小球两两相连
        foreach (GameObject leftBall in clickedLeftBalls)
        {
            foreach (GameObject rightBall in clickedRightBalls)
            {
                Line line = CreateLineBetweenBalls(leftBall, rightBall);
                lines.Add(line);
            }
        }
    }

    private Line CreateLineBetweenBalls(GameObject ball1, GameObject ball2)
    {
        GameObject lineObject = new GameObject("Line");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, ball1.transform.position);
        lineRenderer.SetPosition(1, ball2.transform.position);
        lineRenderer.startWidth = 0.0005f;
        lineRenderer.endWidth = 0.0005f;

        Line line = new Line();
        line.GameObject = lineObject;
        line.LineRenderer = lineRenderer;
        return line;
    }

    private void ClearClickedBallsAndLines()
    {
        foreach (GameObject ball in clickedBalls)
        {
            Image image = ball.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = defaultSprite; // 将球体图像重置为默认图像
            }
        }

        foreach (Line line in lines)
        {
            Destroy(line.GameObject);
        }

        clickedBalls.Clear();
        lines.Clear();
    }

    private IEnumerator ClearClickedBallsAndLinesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject ball in clickedBalls)
        {
            Image image = ball.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = defaultSprite; // 将球体图像重置为默认图像
            }
        }

        foreach (Line line in lines)
        {
            Destroy(line.GameObject);
        }

        clickedBalls.Clear();
        lines.Clear();
    }

    public void ResetClickedBalls()
    {
        ClearClickedBallsAndLines();
    }

    private class Line
    {
        public GameObject GameObject;
        public LineRenderer LineRenderer;
    }

    private void StepSuccess(int step)
    {
        switch (step)
        {
            case 0:
                if (clickedBalls.Contains(balls[1]) && clickedBalls.Contains(balls[2]) && clickedBalls.Contains(balls[3])
                    && clickedBalls.Contains(balls[5]) && clickedBalls.Contains(balls[6]) && clickedBalls.Contains(balls[7]) && clickedBalls.Contains(balls[8])
                    && clickedBalls.Count==7)
                {
                    currentStep++;
                    ConnectBalls();
                    if (Level0Manager.Instance != null)
                    {
                        Level0Manager.Instance.Step1Success(currentStep);
                        Debug.Log("ConnectCalledTwo");
                    }
                    StartCoroutine(ClearClickedBallsAndLinesAfterDelay(2f));
                    Debug.Log("01FINISH");
                }
                break;
            case 1:
                if (clickedBalls.Contains(balls[1]) && clickedBalls.Contains(balls[2]) 
                    && clickedBalls.Contains(balls[7]) && clickedBalls.Contains(balls[8]) && clickedBalls.Contains(balls[9])
                    && clickedBalls.Count == 5)
                {
                    currentStep++;
                    ConnectBalls();
                    if (Level0Manager.Instance != null)
                    {
                        Level0Manager.Instance.Step1Success(currentStep);
                        Debug.Log("ConnectCalledTwo");
                    }
                    StartCoroutine(ClearClickedBallsAndLinesAfterDelay(2f));
                    Debug.Log("02FINISH");
                }
                break;
            // 为每个步骤添加相应的条件
            // ...
            case 2:
                if (clickedBalls.Contains(balls[1]) && clickedBalls.Contains(balls[3]) && clickedBalls.Contains(balls[4])
                    && clickedBalls.Contains(balls[5]) && clickedBalls.Contains(balls[7])
                    && clickedBalls.Count == 5)
                {
                    currentStep++;
                    ConnectBalls();
                    if (Level0Manager.Instance != null)
                    {
                        Level0Manager.Instance.Step1Success(currentStep);
                        Debug.Log("ConnectCalledTwo");
                    }
                    StartCoroutine(ClearClickedBallsAndLinesAfterDelay(2f));
                    Debug.Log("02FINISH");
                }
                break;
            case 3:
                if (clickedBalls.Contains(balls[1]) && clickedBalls.Contains(balls[2]) && clickedBalls.Contains(balls[4])
                    && clickedBalls.Contains(balls[6]) && clickedBalls.Contains(balls[7]) && clickedBalls.Contains(balls[8])
                    && clickedBalls.Count == 6)
                {
                    currentStep++;
                    ConnectBalls();
                    if (Level0Manager.Instance != null)
                    {
                        Level0Manager.Instance.Step1Success(currentStep);
                        Debug.Log("ConnectCalledTwo");
                    }
                    StartCoroutine(ClearClickedBallsAndLinesAfterDelay(2f));
                    Debug.Log("03FINISH");
                }
                break;
            case 4:
                if (clickedBalls.Contains(balls[0]) && clickedBalls.Contains(balls[1]) && clickedBalls.Contains(balls[3])
                    && clickedBalls.Contains(balls[5]) && clickedBalls.Contains(balls[6]) && clickedBalls.Contains(balls[9])
                    && clickedBalls.Count == 6)
                {
                    currentStep++;
                    ConnectBalls();
                    if (Level0Manager.Instance != null)
                    {
                        Level0Manager.Instance.Step1Success(currentStep);
                        Debug.Log("ConnectCalledTwo");
                    }
                    StartCoroutine(ClearClickedBallsAndLinesAfterDelay(2f));
                    Debug.Log("04FINISH");
                }
                break;
        






        }
    }

}
