using BepInEx;
using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

// FIX: Only one titan when multiple spawn? networking issue?

// TODO: Add AoT music for boss music
namespace ErenJaegar
{
    [BepInDependency(NetworkingAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class ErenJaegar : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SDMichaud";
        public const string PluginName = "ErenJaegar";
        public const string PluginVersion = "0.9.5";

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            R2API.Networking.NetworkingAPI.RegisterMessageType<SyncAudio>();
            On.RoR2.TeleporterInteraction.OnBossDirectorSpawnedMonsterServer += (orig, self, bossBody) =>
            {
                orig(self, bossBody);
                //Log.Debug($"Body is: {bossBody.gameObject.name}");
                if ("TitanMaster(Clone)" == bossBody.gameObject.name)
                {
                    RoR2.Util.PlaySound("Play_eren_jaeger", bossBody);
                    NetworkIdentity identity = bossBody.GetComponent<NetworkIdentity>();
                    if (!identity)
                    {
                        Log.Error("SyncAudio: body does not have NetworkIdentity component");
                        return;
                    }
                    new SyncAudio(identity.netId).Send(NetworkDestination.Clients);
                }
            };
            //This hook interferes with multiple boss spawns when playing multiplayer
            //The host could see 5 Stone Titans spawn in but the clients might only see 1 or 2.
            //Removing the call to SyncAudio fixes the bug but prevents the sound from playing for anyone but the host
            //Removing all the code inside the SyncAudio function doesn't fix the issue, meaning the call itself is the problem.
            // On.EntityStates.TitanMonster.SpawnState.OnEnter += (orig, self) =>
            // {
            //     orig(self);
            //     Log.Info("Eren Jaegar!");
            //     GameObject titanGameObject = self.characterBody.master.GetBodyObject();
            //     RoR2.Util.PlaySound("Play_eren_jaeger", titanGameObject);
            //     NetworkIdentity identity = titanGameObject.GetComponent<NetworkIdentity>();
            //     if (!identity)
            //     {
            //         Log.Error("SyncAudio: body does not have NetworkIdentity component");
            //         return;
            //     }
            //     new SyncAudio(identity.netId).Send(NetworkDestination.Clients);
            // };
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
            else
            {
                Log.Debug("SyncAudio: Activated by client. Getting netId for boss");
                GameObject bossGameObject = RoR2.Util.FindNetworkObject(netId);
                if (!bossGameObject)
                {
                    Log.Error("SyncAudio: bossGameObject is null :(");
                    return;
                }
                RoR2.Util.PlaySound("Play_eren_jaeger", bossGameObject);
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(netId);
        }
    }
}
