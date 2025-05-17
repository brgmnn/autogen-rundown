using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using LevelGeneration;
using Localization;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public static class TerminalUplink
{
	public static LocalizedText StringToLocalized(string stringConvert)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		return new LocalizedText
		{
			Id = 0u,
			UntranslatedText = stringConvert
		};
	}

	[HarmonyPatch(typeof(TerminalUplinkPuzzle), "GetCodesString")]
	[HarmonyPrefix]
	public static bool ReturnNewString(ref string __result, TerminalUplinkPuzzleRound round, bool newLine = false)
	{
		string text = "\n";
		for (int i = 0; i < ((Il2CppArrayBase<string>)(object)round.Codes).Length; i++)
		{
			text = text + "<color=orange>" + ((Il2CppArrayBase<string>)(object)round.Prefixes)[i] + "</color>:" + ((Il2CppArrayBase<string>)(object)round.Codes)[i];
			if (i < ((Il2CppArrayBase<string>)(object)round.Codes).Length - 1)
			{
				text = (((i + 1) % 3 == 0) ? (text += "\n") : (text += " | "));
			}
		}
		__result = text;
		return false;
	}

	[HarmonyPatch(typeof(SerialGenerator), "Setup")]
	[HarmonyPrefix]
    public static void OverrideSerialGenerator()
	{
		SerialGenerator.m_codeWordPrefixes = new string[84]
		{
			"A-01", "A-02", "A-03", "A-04", "A-05", "A-06", "A-07", "A-08", "A-09", "A-10",
			"A-11", "A-12", "B-01", "B-02", "B-03", "B-04", "B-05", "B-06", "B-07", "B-08",
			"B-09", "B-10", "B-11", "B-12", "C-01", "C-02", "C-03", "C-04", "C-05", "C-06",
			"C-07", "C-08", "C-09", "C-10", "C-11", "C-12", "W-01", "W-02", "W-03", "W-04",
			"W-05", "W-06", "W-07", "W-08", "W-09", "W-10", "W-11", "W-12", "X-01", "X-02",
			"X-03", "X-04", "X-05", "X-06", "X-07", "X-08", "X-09", "X-10", "X-11", "X-12",
			"Y-01", "Y-02", "Y-03", "Y-04", "Y-05", "Y-06", "Y-07", "Y-08", "Y-09", "Y-10",
			"Y-11", "Y-12", "Z-01", "Z-02", "Z-03", "Z-04", "Z-05", "Z-06", "Z-07", "Z-08",
			"Z-09", "Z-10", "Z-11", "Z-12"
		};
		SerialGenerator.m_codeWords = new string[240]
		{
			"abacus", "abject", "ablaze", "abroad", "abrupt", "absorb", "abused", "accept", "access", "accuse",
			"admire", "agreed", "alarms", "albeit", "alerts", "aligns", "beacon", "beaker", "beamed", "beaten",
			"beauty", "bedlam", "before", "behind", "belfry", "bended", "berate", "beside", "bestir", "betide",
			"bigger", "binged", "binned", "bisect", "bitten", "bladed", "blamed", "blazed", "blonde", "boated",
			"bodily", "boiled", "bolted", "borrow", "bouncy", "choice", "choose", "chosen", "church", "circle",
			"client", "closed", "closer", "coffee", "column", "combat", "coming", "common", "desert", "design",
			"desire", "detail", "detect", "device", "differ", "dinner", "direct", "eighth", "either", "eleven",
			"emerge", "empire", "employ", "enable", "ending", "energy", "engage", "engine", "enough", "ensure",
			"entire", "entity", "finger", "finish", "fiscal", "flight", "flying", "follow", "forced", "forest",
			"forget", "formal", "format", "former", "foster", "fought", "glowed", "gluten", "goggle", "gossip",
			"graded", "grains", "grassy", "grater", "gravel", "grease", "greedy", "groove", "grudge", "guided",
			"guitar", "gunner", "gurgle", "gabber", "harass", "hardly", "harmed", "hazard", "heated", "heaven",
			"herbal", "hereby", "heroes", "herpes", "hinder", "hinged", "hissed", "hoarse", "instep", "inward",
			"indent", "indium", "iodize", "infant", "influx", "jersey", "jetsam", "jigsaw", "jingle", "jogged",
			"joiner", "joking", "jumble", "jumble", "jumped", "junker", "leader", "league", "leaves", "legacy",
			"length", "lesson", "letter", "lights", "likely", "linked", "liquid", "listen", "little", "medium",
			"member", "memory", "mental", "merely", "merger", "method", "middle", "miller", "mining", "minute",
			"mirror", "mobile", "narrow", "nation", "native", "nature", "nearby", "nearly", "nights", "nobody",
			"normal", "notice", "notion", "number", "packed", "palace", "parent", "partly", "patent", "people",
			"period", "permit", "person", "phrase", "picked", "planet", "player", "raised", "random", "rarely",
			"rather", "rating", "reader", "really", "reason", "recall", "recent", "sexual", "should", "signal",
			"signed", "silent", "silver", "simple", "simply", "single", "sister", "slight", "smooth", "theory",
			"thirty", "though", "threat", "thrown", "ticket", "timely", "timing", "tissue", "toward", "unique",
			"useful", "vacant", "vacuum", "vigour", "walker", "wealth", "weekly", "weight", "winner", "winter"
		};

        Plugin.Logger.LogWarning($"Setup codes for longer codes");
	}

	[HarmonyPatch(typeof(SerialGenerator), "Setup")]
	[HarmonyPostfix]
    public static void IPV6Uplinks()
	{
		SerialGenerator.m_ips = new string[99];
		for (int i = 0; i < ((Il2CppArrayBase<string>)(object)SerialGenerator.m_ips).Length; i++)
		{
			int num = 2001;
			string text = ReturnRandomChar().ToString() + ReturnRandomChar() + Builder.SessionSeedRandom.Range(1, 9, "NO_TAG");
			string text2 = Builder.SessionSeedRandom.Range(10, 99, "NO_TAG").ToString() + ReturnRandomChar() + Builder.SessionSeedRandom.Range(1, 9, "NO_TAG");
			string text3 = Builder.SessionSeedRandom.Range(10, 99, "NO_TAG").ToString() + ReturnRandomChar() + ReturnRandomChar();
			string text4 = ReturnRandomChar().ToString() + Builder.SessionSeedRandom.Range(10, 99, "NO_TAG");
			((Il2CppArrayBase<string>)(object)SerialGenerator.m_ips)[i] = num + ":0:" + text + ":" + text2 + ":" + text3 + ":" + text4;
		}
	}

	[HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), "TerminalCorruptedUplinkConnect")]
	[HarmonyPrefix]
    public static bool NoUpperCase(LG_ComputerTerminalCommandInterpreter __instance, string param1, string param2)
	{
		if ((object)__instance.m_terminal.CorruptedUplinkReceiver == null)
		{
			Plugin.Logger.LogDebug("TerminalCorruptedUplinkConnect() critical failure because terminal does not have a CorruptedUplinkReceiver.");
			return false;
		}
		if (LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId != 0 && LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId != __instance.m_terminal.SyncID)
		{
			__instance.AddOngoingUplinkOutput();
			return false;
		}
		LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId = __instance.m_terminal.SyncID;
		// Plugin.Logger.LogDebug(Object.op_Implicit("TerminalCorruptedUplinkConnect, param1: " + param1 + " TerminalUplink: " + ((Object)__instance.m_terminal.UplinkPuzzle).ToString()));
		if (param1 == __instance.m_terminal.UplinkPuzzle.TerminalUplinkIP)
		{
			if (__instance.m_terminal.CorruptedUplinkReceiver.m_command.HasRegisteredCommand((TERM_Command)27))
			{
				__instance.AddUplinkCorruptedOutput();
			}
			else
			{
				__instance.AddUplinkCorruptedOutput();
				__instance.AddOutput("", true);
				__instance.AddOutput((TerminalLineType)4, "Sending connection request to " + __instance.m_terminal.CorruptedUplinkReceiver.PublicName, 3f, (TerminalSoundType)0, (TerminalSoundType)0);
				__instance.AddOutput((TerminalLineType)0, "Connection request sent. Waiting for confirmation.", 0.6f, (TerminalSoundType)0, (TerminalSoundType)0);
				__instance.AddOutput("", true);
				__instance.AddOutput((TerminalLineType)0, "Please <color=green>'CONFIRM'</color> connection on " + __instance.m_terminal.CorruptedUplinkReceiver.PublicName, 0.8f, (TerminalSoundType)0, (TerminalSoundType)0);
				__instance.m_terminal.CorruptedUplinkReceiver.m_command.AddCommand((TERM_Command)27, "CONFIRM", StringToLocalized("Confirm an established Uplink connection with this terminal and another."), (TERM_CommandRule)2);
				__instance.m_terminal.CorruptedUplinkReceiver.m_command.AddOutput((TerminalLineType)0, "Connection request from " + __instance.m_terminal.PublicName + ". Please type <color=green>CONFIRM</color> to continue.", 0f, (TerminalSoundType)0, (TerminalSoundType)0);
			}
		}
		else
		{
			__instance.AddUplinkWrongAddressError(param1);
		}
		return false;
	}

    public static char ReturnRandomChar()
	{
		return System.Convert.ToChar(Builder.SessionSeedRandom.Range(97, 122, "NO_TAG"));
	}
}
