using AutogenRundown.DataBlocks;
using Vector3 = UnityEngine.Vector3;

namespace AutogenRundown.Utils;

/// <summary>
/// Central entry point for posting Wwise sound events from the mod.
///
/// Three layers are available from the game:
///   * <see cref="CellSound"/>        — static, fire-and-forget, no cleanup required.
///   * <see cref="CellSoundPlayer"/>  — instance emitter; required when the sound must move,
///                                     be stopped, or have RTPCs/switches driven after posting.
///                                     Caller MUST invoke <c>Recycle()</c> when done or the
///                                     underlying native Wwise game object leaks.
///   * <see cref="AkSoundEngine"/>    — raw binding; only needed for bank loading and
///                                     string-to-id resolution.
///
/// New events should be added to <see cref="Sound"/> so call sites stay readable and the
/// curated catalog stays central.
/// </summary>
public static class SoundPlayer
{
    /// <summary>
    /// Post a 3D-positioned one-shot. The Wwise listener is the local player, so
    /// <paramref name="position"/> determines volume attenuation and stereo panning.
    /// </summary>
    public static void Play(Sound sound, Vector3 position)
    {
        if (sound == Sound.None)
            return;

        CellSound.Post((uint)sound, position);
    }

    public static void Play(uint eventId, Vector3 position)
    {
        if (eventId == 0)
            return;

        CellSound.Post(eventId, position);
    }

    /// <summary>
    /// Post a non-positioned (2D) event — use for UI cues or global alarms.
    /// </summary>
    public static void Play(Sound sound)
    {
        if (sound == Sound.None)
            return;

        CellSound.Post((uint)sound);
    }

    /// <summary>
    /// Schedule a positional post <paramref name="delaySeconds"/> from now.
    /// Wraps <see cref="CellSound.PostDelayed"/>.
    /// </summary>
    public static void PlayDelayed(Sound sound, Vector3 position, float delaySeconds)
    {
        if (sound == Sound.None)
            return;

        CellSound.PostDelayed((uint)sound, position, delaySeconds);
    }

    /// <summary>
    /// Create a persistent emitter for sounds that need to move (e.g. projectile trails)
    /// or be controlled after posting. Caller owns the lifetime — call <c>Recycle()</c>
    /// when done, or the native Wwise game object leaks across level transitions.
    /// </summary>
    public static CellSoundPlayer CreateEmitter(Vector3 position) => new(position);

    /// <summary>
    /// Resolve a Wwise event name (e.g. from a custom soundbank) to its uint id.
    /// Returns 0 if the event is not in any currently loaded bank.
    /// </summary>
    public static uint ResolveEvent(string name) => AkSoundEngine.GetIDFromString(name);
}
