﻿using System;
using Controller.MainWindow;
using Generator.Cookbook;
using Generator.Generator;
using Model.Root;
using Model.Settings;
using System.Linq;

namespace GeneratorUT
{
    public abstract class ParentUT
    {
        public void SelectProfile(string profileName)
        {
            ProfilesManager profileManager = ProfilesManager.GetInstance();
            profileManager.ActiveProfile = profileManager.Profiles
                .First(prof => prof.Name == profileName);
        }
        public RootModel LoadModel(string fileName)
        {         
            ModelLoader loader = new ModelLoader();
            return loader.LoadModel(fileName);
        }

        public DataOutputGenerator CreateOutputGenerator(string modelName, string profileName)
        {
            SelectProfile(profileName);
            RootModel model = LoadModel(modelName);
            GeneratorRecipe recipe = new GeneratorRecipe(new SequenceGroupGeneratorRecipe());
            return (DataOutputGenerator)recipe.Cook(model);
        }


        public static bool DoubleEquals(double value1, double value2)
        {
            const double TOLERATED_DIFFERENCE = 1.0E-15;
            return Math.Abs(value1 - value2) <= TOLERATED_DIFFERENCE;
        }
    }
}
