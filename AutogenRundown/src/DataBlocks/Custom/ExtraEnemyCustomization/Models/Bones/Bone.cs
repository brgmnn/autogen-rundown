using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Bones;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public record Bone
{
    /// <summary>
    /// This directly references the unity docs for human bones.
    /// https://docs.unity3d.com/ScriptReference/HumanBodyBones.html
    ///
    /// Possible values:
    ///     * Hips                        	This is the Hips bone.
    ///     * LeftUpperLeg                  This is the Left Upper Leg bone.
    ///     * RightUpperLeg                 This is the Right Upper Leg bone.
    ///     * LeftLowerLeg                  This is the Left Knee bone.
    ///     * RightLowerLeg                 This is the Right Knee bone.
    ///     * LeftFoot                      This is the Left Ankle bone.
    ///     * RightFoot                     This is the Right Ankle bone.
    ///     * Spine                       	This is the first Spine bone.
    ///     * Chest                       	This is the Chest bone.
    ///     * UpperChest                    This is the Upper Chest bone.
    ///     * Neck                        	This is the Neck bone.
    ///     * Head                        	This is the Head bone.
    ///     * LeftShoulder                  This is the Left Shoulder bone.
    ///     * RightShoulder                 This is the Right Shoulder bone.
    ///     * LeftUpperArm                  This is the Left Upper Arm bone.
    ///     * RightUpperArm                 This is the Right Upper Arm bone.
    ///     * LeftLowerArm                  This is the Left Elbow bone.
    ///     * RightLowerArm                 This is the Right Elbow bone.
    ///     * LeftHand                      This is the Left Wrist bone.
    ///     * RightHand                     This is the Right Wrist bone.
    ///     * LeftToes                      This is the Left Toes bone.
    ///     * RightToes                     This is the Right Toes bone.
    ///     * LeftEye                     	This is the Left Eye bone.
    ///     * RightEye                      This is the Right Eye bone.
    ///     * Jaw                     	    This is the Jaw bone.
    ///     * LeftThumbProximal             This is the left thumb 1st phalange.
    ///     * LeftThumbIntermediate         This is the left thumb 2nd phalange.
    ///     * LeftThumbDistal               This is the left thumb 3rd phalange.
    ///     * LeftIndexProximal             This is the left index 1st phalange.
    ///     * LeftIndexIntermediate         This is the left index 2nd phalange.
    ///     * LeftIndexDistal               This is the left index 3rd phalange.
    ///     * LeftMiddleProximal            This is the left middle 1st phalange.
    ///     * LeftMiddleIntermediate        This is the left middle 2nd phalange.
    ///     * LeftMiddleDistal              This is the left middle 3rd phalange.
    ///     * LeftRingProximal              This is the left ring 1st phalange.
    ///     * LeftRingIntermediate          This is the left ring 2nd phalange.
    ///     * LeftRingDistal                This is the left ring 3rd phalange.
    ///     * LeftLittleProximal            This is the left little 1st phalange.
    ///     * LeftLittleIntermediate        This is the left little 2nd phalange.
    ///     * LeftLittleDistal              This is the left little 3rd phalange.
    ///     * RightThumbProximal            This is the right thumb 1st phalange.
    ///     * RightThumbIntermediate        This is the right thumb 2nd phalange.
    ///     * RightThumbDistal              This is the right thumb 3rd phalange.
    ///     * RightIndexProximal            This is the right index 1st phalange.
    ///     * RightIndexIntermediate        This is the right index 2nd phalange.
    ///     * RightIndexDistal              This is the right index 3rd phalange.
    ///     * RightMiddleProximal           This is the right middle 1st phalange.
    ///     * RightMiddleIntermediate       This is the right middle 2nd phalange.
    ///     * RightMiddleDistal             This is the right middle 3rd phalange.
    ///     * RightRingProximal             This is the right ring 1st phalange.
    ///     * RightRingIntermediate         This is the right ring 2nd phalange.
    ///     * RightRingDistal               This is the right ring 3rd phalange.
    ///     * RightLittleProximal           This is the right little 1st phalange.
    ///     * RightLittleIntermediate       This is the right little 2nd phalange.
    ///     * RightLittleDistal             This is the right little 3rd phalange.
    ///     * LastBone                      This is the Last bone index delimiter.
    /// </summary>
    [JsonProperty("Bone")]
    public string Name { get; set; } = "Head";

    public bool UsingRelativeScale { get; set; } = true;

    // public Vector3 Scale { get; set; } = new() { X = 1.0, Y = 1.0, Z = 1.0 };
    public JObject Scale { get; set; } = new() { ["x"] = 1.0, ["y"] = 1.0, ["z"] = 1.0 };

    // public Vector3 Offset { get; set; } = new() { X = 1.0, Y = 1.0, Z = 1.0 };
    public JObject Offset { get; set; } = new() { ["x"] = 1.0, ["y"] = 1.0, ["z"] = 1.0 };

    // public Vector3 RotationOffset { get; set; } = new() { X = 1.0, Y = 1.0, Z = 1.0 };
    public JObject RotationOffset { get; set; } = new() { ["x"] = 1.0, ["y"] = 1.0, ["z"] = 1.0 };
}
