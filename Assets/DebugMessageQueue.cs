using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugMessageQueue : MonoBehaviour
{
    private TMP_Text messageText;
    private float messageDuration = 5.0f;
    private AnimationCurve fadeCurve;

    private Queue<string> messageQueue = new Queue<string>();
    private Coroutine currentMessageCoroutine;

    private void Start()
    {
        messageText = GetComponent<TextMeshProUGUI>();
        fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    public void EnqueueMessage(string message)
    {
        messageQueue.Enqueue(message);
        DisplayNextMessage();
    }

    private void DisplayNextMessage()
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }

        if (messageQueue.Count > 0)
        {
            currentMessageCoroutine = StartCoroutine(FadeInMessage(messageQueue.Dequeue()));
        }
    }

    private IEnumerator FadeInMessage(string message)
    {
        messageText.text = message;
        messageText.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        float elapsedTime = 0.0f;
        while (elapsedTime < messageDuration)
        {
            float alpha = fadeCurve.Evaluate(elapsedTime / messageDuration);
            messageText.color = new Color(1.0f, 1.0f, 1.0f, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentMessageCoroutine = StartCoroutine(FadeOutMessage());
    }

    private IEnumerator FadeOutMessage()
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < messageDuration)
        {
            float alpha = fadeCurve.Evaluate(1.0f - elapsedTime / messageDuration);
            messageText.color = new Color(1.0f, 1.0f, 1.0f, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        DisplayNextMessage();
    }
}

