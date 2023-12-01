using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FadeInMultipleRawImages : MonoBehaviour
{   public List<RawImage> rawImages; // Assign raw images in the inspector
    public List<Button> buttons; // Assign buttons in the inspector
    public float fadeInTime = 2.0f; // Time in seconds for each item to fully appear
    public float delayBetweenEach = 1.0f; // Delay between each item appearing

    void Start()
    {
        StartCoroutine(FadeInSequence());
    }

    IEnumerator FadeInSequence()
    {
        foreach (var image in rawImages)
        {
            if (image != null)
            {
                yield return StartCoroutine(FadeInGraphic(image));
            }
            yield return new WaitForSeconds(delayBetweenEach);
        }

        foreach (var button in buttons)
        {
            if (button != null)
            {
                // Ensure the button and its components are active but fully transparent
                button.gameObject.SetActive(true);
                SetButtonComponentsTransparency(button, 0f);
                yield return StartCoroutine(FadeInButton(button));
            }
            yield return new WaitForSeconds(delayBetweenEach);
        }
    }

    IEnumerator FadeInGraphic(Graphic graphic)
    {
        float elapsedTime = 0f;
        Color color = graphic.color;

        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeInTime);
            graphic.color = color;
            yield return null;
        }
    }

    IEnumerator FadeInButton(Button button)
    {
        // Fade in the image component of the button, if it exists
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            yield return StartCoroutine(FadeInGraphic(buttonImage));
        }

        // Fade in the text component of the button, if it exists
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            yield return StartCoroutine(FadeInGraphic(buttonText));
        }
    }

    void SetButtonComponentsTransparency(Button button, float alpha)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color imageColor = buttonImage.color;
            imageColor.a = alpha;
            buttonImage.color = imageColor;
        }

        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            Color textColor = buttonText.color;
            textColor.a = alpha;
            buttonText.color = textColor;
        }
    }
}
