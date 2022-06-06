using UnityEngine;
using System.Collections.Generic;
using ProtoBuf;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("DecalTool", "bmgjet", "1.0.0")]
    [Description("Scans Map For Placed Spray Decal Prefabs And Loads There Settings")]

    public class DecalTool : RustPlugin
    {
        [PluginReference]
        Plugin EntityScaleManager;
        public List<BaseEntity> Applied = new List<BaseEntity>();

        void OnServerInitialized() { Startup(); }

        public void Startup()
        {
            //Find All Spray Decals in the map file
            int Sprays = 0;
            for (int i = World.Serialization.world.prefabs.Count - 1; i >= 0; i--)
            {
                PrefabData prefabdata = World.Serialization.world.prefabs[i];
                if (prefabdata.id == 3884356627)
                {
                    ulong SkinID = 0;
                    float Scale = 1f;
                    //Read settings from prefab category aka custom prefabbed group in rustedit
                    if (prefabdata.category.Contains("SkinID="))
                    {
                        try
                        {
                            string[] settings = prefabdata.category.Split(',');
                            string _skinid = settings[0].Split('=')[1];
                            string _scale = settings[1].Split('=')[1].Split(':')[0];
                            SkinID = ulong.Parse(_skinid);
                            Scale = float.Parse(_scale);
                        }
                        catch
                        {
                            Puts("Error Parsing Prefab Category! @ " + prefabdata.position.ToString());
                            continue;
                        }
                        if (ApplySkin(FindDecal(prefabdata.position, 0.2f), Scale, SkinID)) { Sprays++; }
                    }
                }
            }
            Puts("Applied " + Sprays.ToString() + " Spray Decals");
        }

        public bool ApplySkin(BaseEntity be, float radius, ulong SkinID)
        {
            if (be == null || SkinID == 0) { return false; }
            radius = Mathf.Clamp(radius, 0.3f, 100);
            //Skin
            be.skinID = SkinID;
            be.SendNetworkUpdate();
            //Scale
            if (EntityScaleManager != null)
            {
                //Sends Command to EntityScaleManager
                EntityScaleManager.Call("API_ScaleEntity", be, radius);
                return true;
            }
            return false;
        }

        public BaseEntity FindDecal(Vector3 pos, float radius)
        {
            //Casts a sphere at given position and find all signs there
            var x = new List<BaseEntity>();
            Vis.Entities<BaseEntity>(pos, radius, x);
            foreach (BaseEntity entity in x)
            {
                if (entity && entity.prefabID == 3884356627)
                {
                    if (Applied.Contains(entity)) { continue; }
                    Applied.Add(entity);
                    return entity;
                }
            }
            return null;
        }
    }
}