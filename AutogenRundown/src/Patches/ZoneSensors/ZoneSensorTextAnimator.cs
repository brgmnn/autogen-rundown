using Il2CppInterop.Runtime.Injection;
using System.Text;
using TMPro;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// MonoBehaviour that animates sensor text with a two-stage encryption effect.
/// Cycles: Reveal → Partial Encrypted → Fully Encrypted → Partial Encrypted → repeat
/// </summary>
public class ZoneSensorTextAnimator : MonoBehaviour
{
    private string actualText = "";
    private Color encryptedColor;
    private Color normalColor;

    // Timing constants (using existing tuned values)
    private const float HEX_CYCLE_INTERVAL = 0.6f;
    private const float FULL_CYCLE_TIME = 9.5f;

    // Phase durations within the cycle (total 9.5s)
    private const float REVEAL_DURATION = 1.2f;              // Phase 0: actual text
    private const float PARTIAL1_DURATION = 2.15f;           // Phase 1: partial encrypted
    private const float FULL_ENCRYPTED_DURATION = 4.0f;      // Phase 2: full encrypted
    private const float PARTIAL2_DURATION = 2.15f;           // Phase 3: partial encrypted

    private TextMeshPro textComponent;
    private float hexTimer = 0f;
    private float cycleTimer = 0f;
    private int currentPhase = 0;  // 0=reveal, 1=partial, 2=full encrypted, 3=partial
    private bool initialized = false;

    private static readonly char[] HexChars = "0123456789ABCDEF".ToCharArray();

    public void Initialize(string text, Color normalTextColor, Color encryptedTextColor, TextMeshPro tmp)
    {
        actualText = text ?? "";
        normalColor = normalTextColor;
        encryptedColor = encryptedTextColor;
        textComponent = tmp;

        if (textComponent == null) return;

        currentPhase = 0;
        UpdateDisplay();
        initialized = true;
    }

    void Update()
    {
        if (!initialized || textComponent == null) return;

        cycleTimer += Time.deltaTime;
        float cyclePosition = cycleTimer % FULL_CYCLE_TIME;

        // Determine which phase we're in
        int newPhase;
        if (cyclePosition < REVEAL_DURATION)
            newPhase = 0;  // Reveal
        else if (cyclePosition < REVEAL_DURATION + PARTIAL1_DURATION)
            newPhase = 1;  // Partial encrypted
        else if (cyclePosition < REVEAL_DURATION + PARTIAL1_DURATION + FULL_ENCRYPTED_DURATION)
            newPhase = 2;  // Full encrypted
        else
            newPhase = 3;  // Partial encrypted again

        // Phase changed
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            UpdateDisplay();
        }
        // Update hex scramble during encrypted phases (1, 2, 3)
        else if (currentPhase > 0)
        {
            hexTimer += Time.deltaTime;
            if (hexTimer >= HEX_CYCLE_INTERVAL)
            {
                hexTimer = 0f;
                UpdateDisplay();
            }
        }
    }

    private void UpdateDisplay()
    {
        switch (currentPhase)
        {
            case 0:
                SetRevealedText();
                break;
            case 1:
            case 3:
                SetPartiallyDecryptedText();
                break;
            case 2:
                SetFullyEncryptedText();
                break;
        }
    }

    private void SetRevealedText()
    {
        textComponent.SetText(actualText);
        textComponent.m_fontColor = textComponent.m_fontColor32 = normalColor;
    }

    private void SetPartiallyDecryptedText()
    {
        textComponent.m_fontColor = textComponent.m_fontColor32 = encryptedColor;

        if (actualText.Length == 0) { textComponent.SetText(""); return; }

        // Hex chars preserving spaces (original behavior)
        char[] scrambled = new char[actualText.Length];
        for (int i = 0; i < actualText.Length; i++)
        {
            scrambled[i] = actualText[i] == ' ' ? ' ' : HexChars[UnityEngine.Random.Range(0, HexChars.Length)];
        }
        textComponent.SetText(new string(scrambled));
    }

    private void SetFullyEncryptedText()
    {
        textComponent.m_fontColor = textComponent.m_fontColor32 = encryptedColor;

        // Fixed 14-char format: "XX-XX-XX-XX-XX"
        var sb = new StringBuilder(14);
        for (int i = 0; i < 5; i++)
        {
            if (i > 0) sb.Append('-');
            sb.Append(HexChars[UnityEngine.Random.Range(0, HexChars.Length)]);
            sb.Append(HexChars[UnityEngine.Random.Range(0, HexChars.Length)]);
        }
        textComponent.SetText(sb.ToString());
    }

    static ZoneSensorTextAnimator()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorTextAnimator>();
    }
}
