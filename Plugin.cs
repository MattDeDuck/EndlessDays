using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using LocalizationSystem;
using ObjectBased.UIElements;
using ObjectBased.UIElements.ConfirmationWindow;
using System.IO;
using System;
using System.Reflection;
using Npc.Parts;

namespace EndlessDays
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }

        public static string pluginLoc = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static GameObject endlessButtonGO;

        public static Texture2D endlessButtonOffTexture;
        public static Texture2D endlessButtonOnTexture;
        public static Sprite endlessButtonOffSprite;
        public static Sprite endlessButtonOnSprite;

        public static bool endlessMode = false;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            // Create textures and sprites
            endlessButtonOffTexture = LoadTextureFromFile(pluginLoc + "/EndlessDaysButtonOff.png");
            endlessButtonOffSprite = Sprite.Create(endlessButtonOffTexture, new Rect(0, 0, endlessButtonOffTexture.width, endlessButtonOffTexture.height), new Vector2(0.5f, 0.5f));
            endlessButtonOnTexture = LoadTextureFromFile(pluginLoc + "/EndlessDaysButtonOn.png");
            endlessButtonOnSprite = Sprite.Create(endlessButtonOnTexture, new Rect(0, 0, endlessButtonOnTexture.width, endlessButtonOnTexture.height), new Vector2(0.5f, 0.5f));

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        public void Update()
        {
            var roomName = Managers.Room.settings.rooms[(int)Managers.Room.currentRoom].name;
            if (roomName == "MeetingRoom")
            {
                if (Managers.Npc.CurrentNpcMonoBehaviour != null || Managers.Npc.spawnedNpcQueue.Count > 0 || Managers.Npc.npcQueue.Count > 0)
                {
                    // If customers are still in queue
                    return;
                }
                else
                {
                    if (endlessMode)
                    {
                        // Get customers in
                        Day getDay = Managers.Day.settings.groundhogDay;
                        Managers.Environment.ResetNpcCounters();
                        Managers.Npc.ClearNpcQueue();
                        foreach (DailyVisitor dailyVisitor in getDay.templatesToSpawn)
                        {
                            Managers.Npc.AddToQueueForSpawn(dailyVisitor, false);
                        }
                    }
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RoomManager), "OrderedStart")]
        public static void OrderedStart_Postfix()
        {
            CreateEndlessDayButton();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ObjectBased.Bedroom.Bed), "OnPrimaryCursorClick")]
        public static void OnPrimaryCursorClick_Prefix()
        {
            if (!endlessMode)
            {
                return;
            }
            else
            {
                ConfirmationWindow.Show(DarkScreen.Layer.Lower, new Key("#bed_start_new_day", null), new Key("#bed_start_new_day_description", null), Managers.Game.settings.confirmationWindowPosition, new Action(Managers.Environment.StartNight), null);
            }
        }

        public static void CreateEndlessDayButton()
        {
            // Give the GameObject a name
            endlessButtonGO = new GameObject("Endless Day Button");

            // Give it a parent
            var parentGO = GameObject.Find("Camera").transform;
            endlessButtonGO.transform.parent = parentGO;

            // Give it the "Off" sprite by default
            var sr = endlessButtonGO.AddComponent<SpriteRenderer>();
            sr.sprite = endlessButtonOffSprite;

            // Fix the polygon collisions
            endlessButtonGO.AddComponent<PolygonCollider2D>();
            Destroy(endlessButtonGO.GetComponent<PolygonCollider2D>());
            endlessButtonGO.AddComponent<PolygonCollider2D>();

            // Give it a sorting sorting group
            var sg = endlessButtonGO.AddComponent<SortingGroup>();
            sg.sortingLayerID = -1758066705;
            sg.sortingLayerName = "GuiBackground";

            // Attach our behaviour
            endlessButtonGO.AddComponent<EndlessButton.EndlessButtonClick>();

            // Give it a layer
            endlessButtonGO.layer = LayerMask.NameToLayer("UI");

            // Give it a position
            endlessButtonGO.transform.localPosition = new Vector3(-9.97f, -6.53f, 0f);

            // Make it active
            endlessButtonGO.SetActive(true);

            Log.LogInfo("Endless Days Button created");
        }

        public static Texture2D LoadTextureFromFile(string filePath)
        {
            var data = File.ReadAllBytes(filePath);

            // Do not create mip levels for this texture, use it as-is.
            var tex = new Texture2D(0, 0, TextureFormat.ARGB32, false, false)
            {
                filterMode = FilterMode.Bilinear,
            };

            if (!tex.LoadImage(data))
            {
                throw new Exception($"Failed to load image from file at \"{filePath}\".");
            }

            return tex;
        }
    }
}
