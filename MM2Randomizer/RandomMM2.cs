﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

using MM2Randomizer.Patcher;
using MM2Randomizer.Enums;
using MM2Randomizer.Randomizers;
using MM2Randomizer.Randomizers.Enemies;
using MM2Randomizer.Randomizers.Colors;
using MM2Randomizer.Randomizers.Stages;
using MM2Randomizer.Utilities;

namespace MM2Randomizer
{
    public static class RandomMM2
    {
        public static int Seed = -1;
        public static Random Random;
        public static Patch Patch;
        public static MainWindowViewModel Settings;
        public static readonly string TempFileName = "temp.nes";
        public static string RecentlyCreatedFileName = "";

        public static RStages randomStages = new RStages();
        public static RWeaponGet randomWeaponGet = new RWeaponGet();
        public static RWeaponBehavior randomWeaponBehavior = new RWeaponBehavior();
        public static RWeaknesses randomWeaknesses = new RWeaknesses(true);
        public static RBossAI randomBossAI = new RBossAI();
        public static RItemGet randomItemGet = new RItemGet();
        public static RTeleporters randomTeleporters = new RTeleporters();
        public static REnemies randomEnemies = new REnemies();
        public static RTilemap randomTilemap = new RTilemap();
        public static RColors randomColors = new RColors();
        public static RMusic randomMusic = new RMusic();
        public static RWeaponNames rWeaponNames = new RWeaponNames();
        public static List<IRandomizer> Randomizers;

        /// <summary>
        /// Perform the randomization based on the seed and user-provided settings, and then
        /// generate the new ROM.
        /// </summary>
        public static void RandomizerCreate()
        {
            try
            {
                Randomizers = new List<IRandomizer>();

                // Add randomizers according to each flag
                if (Settings.Is8StagesRandom)
                {
                    Randomizers.Add(randomStages);
                }
                if (Settings.IsWeaponsRandom)
                {
                    Randomizers.Add(randomWeaponGet);
                }
                if (Settings.IsWeaponBehaviorRandom)
                {
                    Randomizers.Add(randomWeaponBehavior);
                }
                if (Settings.IsWeaknessRandom)
                {
                    Randomizers.Add(randomWeaknesses);
                }
                if (Settings.IsBossAIRandom)
                {
                    Randomizers.Add(randomBossAI);
                }
                if (Settings.IsItemsRandom)
                {
                    Randomizers.Add(randomItemGet);
                }
                if (Settings.IsTeleportersRandom)
                {
                    Randomizers.Add(randomTeleporters);
                }
                if (Settings.IsEnemiesRandom)
                {
                    Randomizers.Add(randomEnemies);
                }
                if (Settings.IsTilemapChangesEnabled)
                {
                    Randomizers.Add(randomTilemap);
                }
                if (Settings.IsColorsRandom)
                {
                    Randomizers.Add(randomColors);
                }
                if (Settings.IsBGMRandom)
                {
                    Randomizers.Add(randomMusic);
                }
                if (Settings.IsWeaponNamesRandom)
                {
                    Randomizers.Add(rWeaponNames);
                }
                
                // Instantiate RNG object r based on RandomMM2.Seed
                InitializeSeed();

                // Create randomization patch
                Patch = new Patch();
                foreach (IRandomizer randomizer in Randomizers)
                {
                    randomizer.Randomize(Patch, Random);
                    Debug.WriteLine(randomizer);
                }

                // Create patch with additional modifications
                if (Settings.FastText)
                {
                    MiscHacks.SetFastText(Patch, Settings.IsJapanese);
                }
                if (Settings.BurstChaserMode)
                {
                    MiscHacks.SetBurstChaser(Patch, Settings.IsJapanese);
                }
                if (Settings.Is8StagesRandom || Settings.IsWeaponsRandom)
                {
                    MiscHacks.FixPortraits(Patch, Settings.Is8StagesRandom, randomStages, Settings.IsWeaponsRandom, randomWeaponGet);
                }
                if (!Settings.IsJapanese)
                {
                    MiscHacks.DrawTitleScreenChanges(Patch, Seed);
                }
                MiscHacks.EnablePressDamage(Patch);

                // Prepare a copy of the source rom for modification
                File.Copy(Settings.SourcePath, TempFileName, true);

                // Apply pre-patch changes via IPS patch (manual title screen, stage select, and stage changes)
                Patch.ApplyIPSPatch(TempFileName, "mm2rng_prepatch.dat");

                // Apply patch with randomized content
                Patch.ApplyRandoPatch(TempFileName);

                // Create file name based on seed and game region
                string newfilename = (Settings.IsJapanese) ? "RM2" : "MM2";
                string seedAlpha = SeedConvert.ConvertBase10To26(Seed);
                newfilename = String.Format("{0}-RNG-{1}.nes", newfilename, seedAlpha);

                // If a file of the same seed already exists, delete it
                if (File.Exists(newfilename))
                {
                    File.Delete(newfilename);
                }

                // Finish the copy/rename and open Explorer at that location
                File.Move(TempFileName, newfilename);
                RecentlyCreatedFileName = newfilename;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Create a random seed or use the user-provided seed.
        /// </summary>
        private static void InitializeSeed()
        {
            if (Seed < 0)
            {
                Random rndSeed = new Random();
                Seed = rndSeed.Next(int.MaxValue);
            }
            Random = new Random(Seed);
        }

        /// <summary>
        /// Shuffle the elements of the provided list.
        /// </summary>
        /// <typeparam name="T">The Type of the elements in the list.</typeparam>
        /// <param name="list">The object to be shuffled.</param>
        /// <param name="rng">The seed used to perform the shuffling.</param>
        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
