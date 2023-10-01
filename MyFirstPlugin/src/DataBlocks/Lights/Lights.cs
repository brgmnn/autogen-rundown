using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin.DataBlocks
{
    public static class Lights
    {
        public enum Light : UInt32
        {
            AlmostWhite_1 = 56,
            AlmostWhite_1_R5C3 = 99,
            BlueGreenBrown = 38,
            BlueToGreen_1 = 24,
            BlueToOrange_1 = 1,
            BlueToPink_1 = 68,
            BlueToRed_1 = 31,
            camo_green = 48,
            camo_green_R4E1 = 97,
            ColdWhite_all_on_1 = 57,
            CyanBrownToYellow_1 = 67,
            CyanToBlue_1 = 27,
            CyanToPurple_1 = 64,
            CyanToPurple_1_R5D1_z3 = 103,
            CyanToYellow_1 = 12,
            DarkBlue_1 = 61,
            DarkBlueToPurple_1 = 63,
            DarkBlueToPurple_1_R5C2_L1 = 100,
            DarkGreenToOrange_1 = 50,
            DarkGreenToRed_1 = 49,
            DarkGrey_1 = 77,
            DarkGreyToOrange = 76,
            DarktealToYellow = 39,
            GreenToRed_1 = 29,
            HeavyRedToCyan_1 = 80,
            HSU_Prep_R7D1 = 84,
            Mainframe_R7D2 = 85,
            monochrome = 43,
            Monochrome_Blue = 19,
            Monochrome_Blue_Flickering = 74,
            monochrome_copy_R1C2 = 86,
            Monochrome_Cyan = 60,
            Monochrome_Green = 22,
            Monochrome_Green_R4C2 = 98,
            Monochrome_iceblue = 36,
            Monochrome_Orange = 18,
            Monochrome_Orange_Flickering = 78,
            Monochrome_Red = 20,
            Monochrome_Red_R1D1_z9 = 87,
            Monochrome_Red_R7D1 = 83,
            Monochrome_Red_True = 69,
            Monochrome_Yellow = 21,
            Monochrome_YellowToGreen = 59,
            New = 42,
            New_Copy_Copy_1 = 46,
            OrangeToBlue = 41,
            OrangeToBlue_1 = 3,
            OrangeToBluishGrey = 72,
            OrangeToBrown_1 = 65,
            OrangeToYellow_1 = 13,
            PeachToGreen = 37,
            PeachToGreen_R5C1_ALT = 95,
            Pitch_black_1 = 23,
            PurpleToBlue_1 = 15,
            PurpleToBlue_1_R3B2 = 96,
            PurpleToBrown = 40,
            PurpleToOrange_1 = 66,
            PurpleToOrange_1_R2D1 = 90,
            PurpleToOrange_1_R5B2 = 93,
            PurpleToPink_1 = 53,
            PurpleToRed_1 = 51,
            PurpleToRed_1_R5D1_z2 = 102,
            Reactor_blue_to_red_all_on_1 = 54,
            Reactor_blue_to_red_all_on_1_R2D2 = 89,
            Reactor_blue_to_White_all_on_1 = 58,
            Reactor_blue_to_White_R2E1 = 91,
            Reactor_blue_to_White_R5D1 = 101,
            Reactor_green_all_on_1 = 79,
            Reactor_green_all_on_1_R5C1_ALT = 94,
            RedToCyan_1 = 9,
            RedToWhite_1 = 55,
            RedToWhite_1_Flickering = 81,
            RedToWhite_1_R5A2_L1 = 88,
            RedToYellow_1 = 70,
            RustyRedToBrown_1 = 71,
            TGA_1_INTRO_CyanToBlue = 34,
            TGA_2_COCOONS_CyanToBlue = 33,
            TGA_3_ARTIFACT_YellowToCyan = 35,
            TGA_4_IMMORTAL_GreenToRed = 32,
            TGA_5_HSU_BlueToRed = 30,
            TGA_5_HSU_BlueToRed_R3A1 = 92,
            Totally_black = 73,
            Tutorial_StealthSpotlight = 82,
            WashedOutRed_1 = 11,
            White_To_Blue_all_on_1 = 75,
            White_To_Blue_all_on_1_ALL_ON_FOR_REAL = 107,
            YellowToCyan_1 = 26,
            YellowToOrange_1 = 28
        }

        public static List<Light> BasicLights { get; private set; } = new List<Light>
        {
            Light.RedToCyan_1,
            Light.RedToWhite_1,
            Light.RedToYellow_1
        };

        /// <summary>
        /// Picks a random light
        /// </summary>
        /// <returns></returns>
        public static Light GenRandomLight()
        {
            var lights = Enum.GetValues(typeof(Light)).OfType<Light>();

            return lights.ElementAt(Generator.Random.Next(lights.Count()));
        }
    }
}
