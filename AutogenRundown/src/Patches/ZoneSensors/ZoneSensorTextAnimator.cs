using Il2CppInterop.Runtime.Injection;
using System.Text;
using TMPro;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// MonoBehaviour that animates sensor text with a two-stage encryption effect.
/// Cycles: Fully Encrypted (hex pairs) → Partially Decrypted (hex over text) → Reveal → repeat
/// </summary>
public class ZoneSensorTextAnimator : MonoBehaviour
{
    private string actualText = "";
    private Color encryptedColor;
    private Color normalColor;

    // Timing constants (using existing tuned values)
    private const float HEX_CYCLE_INTERVAL = 0.6f;
    private const float FULL_CYCLE_TIME = 9.5f;

    // Phase durations within the cycle
    private const float FULLY_ENCRYPTED_DURATION = 4.0f;    // Hex pairs with dashes
    private const float PARTIAL_DECRYPT_DURATION = 4.3f;    // Hex over original structure
    private const float REVEAL_DURATION = 1.2f;             // Show actual text

    private TextMeshPro textComponent;
    private float hexTimer = 0f;
    private float cycleTimer = 0f;
    private int currentPhase = 0;  // 0=fully encrypted, 1=partial, 2=reveal
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
        if (cyclePosition < FULLY_ENCRYPTED_DURATION)
            newPhase = 0;  // Fully encrypted (hex pairs with dashes)
        else if (cyclePosition < FULLY_ENCRYPTED_DURATION + PARTIAL_DECRYPT_DURATION)
            newPhase = 1;  // Partially decrypted (hex over original structure)
        else
            newPhase = 2;  // Reveal actual text

        // Phase changed
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            UpdateDisplay();
        }
        // Update hex scramble during encrypted phases
        else if (currentPhase < 2)
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
                SetFullyEncryptedText();
                break;
            case 1:
                SetPartiallyDecryptedText();
                break;
            case 2:
                SetRevealedText();
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

        if (actualText.Length == 0) { textComponent.SetText(""); return; }

        // Generate hex pairs with dashes: "AB-95-F7-02"
        // Use ~half the original length for pair count to keep similar visual width
        int pairCount = Mathf.Max(1, (actualText.Length + 1) / 2);
        var sb = new StringBuilder(pairCount * 3 - 1);

        for (int i = 0; i < pairCount; i++)
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
