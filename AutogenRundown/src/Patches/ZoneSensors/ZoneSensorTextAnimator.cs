using Il2CppInterop.Runtime.Injection;
using TMPro;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// MonoBehaviour that animates sensor text with an encrypted hex effect.
/// Cycles through hex characters with periodic reveals of actual text.
/// </summary>
public class ZoneSensorTextAnimator : MonoBehaviour
{
    private string actualText = "";
    private Color encryptedColor;
    private Color normalColor;

    private const float HEX_CYCLE_INTERVAL = 0.1f;   // Hex scramble rate
    private const float REVEAL_CYCLE_TIME = 2.5f;    // Full cycle duration
    private const float REVEAL_DURATION = 0.3f;      // Actual text flash duration

    private TextMeshPro textComponent;
    private float hexTimer = 0f;
    private float cycleTimer = 0f;
    private bool isRevealing = false;
    private bool initialized = false;

    private static readonly char[] HexChars = "0123456789ABCDEF".ToCharArray();

    public void Initialize(string text, Color normalTextColor, Color encryptedTextColor, TextMeshPro tmp)
    {
        actualText = text ?? "";
        normalColor = normalTextColor;
        encryptedColor = encryptedTextColor;
        textComponent = tmp;

        if (textComponent == null) return;

        SetEncryptedText();
        initialized = true;
    }

    void Update()
    {
        if (!initialized || textComponent == null) return;

        cycleTimer += Time.deltaTime;
        float cyclePosition = cycleTimer % REVEAL_CYCLE_TIME;
        bool shouldReveal = cyclePosition < REVEAL_DURATION;

        if (shouldReveal != isRevealing)
        {
            isRevealing = shouldReveal;
            if (isRevealing) SetRevealedText();
            else SetEncryptedText();
        }
        else if (!isRevealing)
        {
            hexTimer += Time.deltaTime;
            if (hexTimer >= HEX_CYCLE_INTERVAL)
            {
                hexTimer = 0f;
                UpdateHexScramble();
            }
        }
    }

    private void SetRevealedText()
    {
        textComponent.SetText(actualText);
        textComponent.m_fontColor = textComponent.m_fontColor32 = normalColor;
    }

    private void SetEncryptedText()
    {
        textComponent.m_fontColor = textComponent.m_fontColor32 = encryptedColor;
        UpdateHexScramble();
    }

    private void UpdateHexScramble()
    {
        if (actualText.Length == 0) { textComponent.SetText(""); return; }

        char[] scrambled = new char[actualText.Length];
        for (int i = 0; i < actualText.Length; i++)
        {
            scrambled[i] = actualText[i] == ' ' ? ' ' : HexChars[UnityEngine.Random.Range(0, HexChars.Length)];
        }
        textComponent.SetText(new string(scrambled));
    }

    static ZoneSensorTextAnimator()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorTextAnimator>();
    }
}
