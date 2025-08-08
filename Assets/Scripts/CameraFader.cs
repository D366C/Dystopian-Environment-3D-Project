using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Written by Mike Yeates for FIT1033

public class CameraFader : MonoBehaviour
{
    public Texture2D black;

    public const float DEFAULT_FADE_DURATION = 0.75f;

    private static float fadeSpeed;
    private static bool isFadingToBlack = false;
    private static bool isFadingToClear = false;
    private static float currentAlpha;

    // Event triggered when screen reaches full black
    public delegate void OnScreenBlackDelegate();
    public static OnScreenBlackDelegate screenBlackDelegate;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    
    void Update()
    {
        if(isFadingToBlack)
        {
            currentAlpha += Time.unscaledDeltaTime * fadeSpeed;

            if(currentAlpha >= 1.0f)
            {
                if (screenBlackDelegate != null)
                    screenBlackDelegate();

                currentAlpha = 1.0f;
                isFadingToBlack = false;
            }
        }
        else if(isFadingToClear)
        {
            currentAlpha -= Time.unscaledDeltaTime * fadeSpeed;

            if(currentAlpha <= 0.0f)
            {
                currentAlpha = 0.0f;
                isFadingToClear = false;
            }
        }
    }

    void OnGUI()
	{
		if (currentAlpha >= 0.0f)
		{
			GUI.color = new Color(1.0f, 1.0f, 1.0f, currentAlpha);
			GUI.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height), black);
			GUI.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		}
	}

    public static void FadeToClear(float duration = DEFAULT_FADE_DURATION)
    {
        fadeSpeed = 1.0f / duration;
        isFadingToBlack = false;
        isFadingToClear = true;
    }

    public static void FadeToBlack(float duration = DEFAULT_FADE_DURATION)
    {
        fadeSpeed = 1.0f / duration;
        isFadingToClear = false;
        isFadingToBlack = true;
    }

    public static void JumpToClear()
    {
        currentAlpha = 0.0f;
    }

    public static void JumpToBlack()
    {
        currentAlpha = 1.0f;
    }
}
