using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BallController : MonoBehaviour
{
    public GameObject[] balls; // �������飬������Unity��Inspector����������
    public AudioClip[] audioClips; // ��ͬ����Ƶ����
    public Button resetButton; // ��ť���������ѵ������
    public Sprite clickedSprite; // ������С��ͼ��
    public Sprite hoverSprite; // ��ͣʱ��С��ͼ��
    public Sprite defaultSprite; // Ĭ�ϵ�С��ͼ��

    private Dictionary<GameObject, int> ballNumbers = new Dictionary<GameObject, int>(); // �洢ÿ������ı��
    private HashSet<GameObject> clickedBalls = new HashSet<GameObject>(); // ���ڴ洢�����С��
    private int currentStep = 0; // ��ǰ����
    private List<GameObject> leftGroup = new List<GameObject>(); // ����
    private List<GameObject> rightGroup = new List<GameObject>(); // ����
    private List<Line> lines = new List<Line>(); // ���ڴ洢����

    void Start()
    {
        // ���������Ų�����
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
            balls[i].AddComponent<AudioSource>().clip = audioClips[i]; // �����ƵԴ��������Ƶ����

            // ��ӵ���¼�
            int index = i; // ����ǰѭ��������
            Button button = balls[i].GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnBallClicked(balls[index]));
            }

            // �����ͣ���뿪��ͣ������¼�
            EventTrigger trigger = balls[i].AddComponent<EventTrigger>();

            // ��ͣ�¼�
            EventTrigger.Entry entryHover = new EventTrigger.Entry();
            entryHover.eventID = EventTriggerType.PointerEnter;
            entryHover.callback.AddListener((eventData) => OnHoverEnter(balls[index]));
            trigger.triggers.Add(entryHover);

            // �뿪��ͣ�����¼�
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((eventData) => OnHoverExit(balls[index]));
            trigger.triggers.Add(entryExit);


        }

        // �󶨰�ť����¼�
        resetButton.onClick.AddListener(ResetClickedBalls);

      
    }

    public void OnBallClicked(GameObject clickedBall)
    {
        if (currentStep >= 5) return; // ��������в��裬��ֹ��һ������

        // ������Ƶ
        AudioSource audioSource = clickedBall.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // ����ͼ��
        Image image = clickedBall.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = clickedSprite; // ������ͼ���Ϊ������ͼ��
        }

        clickedBalls.Add(clickedBall);

        // ����Ƿ���������
        StepSuccess(currentStep);
    }

    private void OnHoverEnter(GameObject ball)
    {
        if (clickedBalls.Contains(ball))
        {
            return;
        }
        // ����ͼ��Ϊ��ͣ״̬
        Image image = ball.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = hoverSprite;
        }
    }

    private void OnHoverExit(GameObject ball)
    {
        // �����С���ѱ�������򲻻ָ�ΪĬ��ͼ��
        if (clickedBalls.Contains(ball))
        {
            return;
        }

        // �ָ�ΪĬ��ͼ��
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

        // �ҳ������������С��
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

        // �������������С���������С����������
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
                image.sprite = defaultSprite; // ������ͼ������ΪĬ��ͼ��
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
                image.sprite = defaultSprite; // ������ͼ������ΪĬ��ͼ��
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
            // Ϊÿ�����������Ӧ������
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
