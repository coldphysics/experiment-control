using Controller.MainWindow;
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
                .Where(prof => prof.Name == profileName)
                .First();
        }
        public RootModel LoadModel(string fileName)
        {         
            ModelLoader loader = new ModelLoader();
            return loader.LoadModel(fileName);
        }
    }
}
