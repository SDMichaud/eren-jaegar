using BepInEx;
using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using UnityEngine;

// TODO: Add AoT music for boss music
namespace ErenJaegar
{
    // This attribute is required, and lists metadata for your plugin.
    [BepInDependency(NetworkingAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class ErenJaegar : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SDMichaud";
        public const string PluginName = "ErenJaegar";
        public const string PluginVersion = "0.9.1";

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            R2API.Networking.NetworkingAPI.RegisterMessageType<SyncAudio>();
            //
            // Uncomment this an the mod only works for when Titan spawns as a teleporter boss
            //
            // On.RoR2.TeleporterInteraction.OnBossDirectorSpawnedMonsterServer += (orig, self, bossBody) =>
            // {
            //     orig(self, bossBody);
            //     Log.Debug($"Body is: {bossBody.gameObject.name}");
            //     if ("TitanMaster(Clone)" == bossBody.gameObject.name)
            //     {
            //         RoR2.Util.PlaySound("Play_eren_jaeger", bossBody);
            //         NetworkIdentity identity = bossBody.GetComponent<NetworkIdentity>();
            //         if (!identity)
            //         {
            //             Log.Error("SyncAudio: body does not have NetworkIdentity component");
            //             return;
            //         }
            //         new SyncAudio(identity.netId).Send(NetworkDestination.Clients);
            //     }
            // };
            On.EntityStates.TitanMonster.SpawnState.OnEnter += (orig, self) =>
            {
                orig(self);
                Log.Info("Eren Jaegar!");
                GameObject titanGameObject = self.characterBody.master.GetBodyObject();
                RoR2.Util.PlaySound("Play_eren_jaeger", titanGameObject);
                NetworkIdentity identity = titanGameObject.GetComponent<NetworkIdentity>();
                    if (!identity)
                    {
                        Log.Error("SyncAudio: body does not have NetworkIdentity component");
                        return;
                    }
                    new SyncAudio(identity.netId).Send(NetworkDestination.Clients);
            };
        }
    }
    public class SyncAudio : INetMessage
    {
        NetworkInstanceId netId;

        public SyncAudio()
        {
        }

        public SyncAudio(NetworkInstanceId netId)
        {
            this.netId = netId;
        }
        public void Deserialize(NetworkReader reader)
        {
            netId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            if (NetworkServer.active)
            {
                Log.Debug("SyncAudio: Activated by host. Ending...");
                return;
            }
            Log.Debug("SyncAudio: Activated by client. Getting netId for boss");
            GameObject bossGameObject = RoR2.Util.FindNetworkObject(netId);
            if (!bossGameObject)
            {
                Log.Error("SyncAudio: bossGameObject is null :(");
                return;
            }
            RoR2.Util.PlaySound("Play_eren_jaeger", bossGameObject);
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(netId);
        }
    }
}
