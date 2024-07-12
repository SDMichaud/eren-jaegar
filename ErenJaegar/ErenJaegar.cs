using BepInEx;

// TODO: Add AoT music for boss music
namespace ErenJaegar
{
    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class ErenJaegar : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SDMichaud";
        public const string PluginName = "ErenJaegar";
        public const string PluginVersion = "0.9.0";

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            On.EntityStates.TitanMonster.SpawnState.OnEnter += (orig, self) =>
            {
                orig(self);
                Log.Info("Eren Jaegar!");
                AkSoundEngine.PostEvent(3206641567, self.characterBody.master.GetBodyObject());
            };
        }
    }
}
