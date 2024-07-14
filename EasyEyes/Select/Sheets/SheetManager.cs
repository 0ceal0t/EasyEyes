using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using VFXSelect.Data.Sheets;

namespace VFXSelect {
    public class SheetManager {
        public static string NpcCsv { get; private set; }
        public static string MonsterJson { get; private set; }

        public static IDataManager DataManager { get; private set; }
        public static IDalamudPluginInterface PluginInterface { get; private set; }

        public static ItemSheetLoader Items { get; private set; }
        public static ActionSheetLoader Actions { get; private set; }
        public static CutsceneSheetLoader Cutscenes { get; private set; }
        public static EmoteSheetLoader Emotes { get; private set; }
        public static GimmickSheetLoader Gimmicks { get; private set; }
        public static NonPlayerActionSheetLoader NonPlayerActions { get; private set; }
        public static NpcSheetLoader Npcs { get; private set; }
        public static StatusSheetLoader Statuses { get; private set; }
        public static ZoneSheetLoader Zones { get; private set; }
        public static MountSheeetLoader Mounts { get; private set; }
        public static HousingSheetLoader Housing { get; private set; }
        public static CommonLoader Misc { get; private set; }

        public static void Initialize( string npcCsv, string monsterJson, IDataManager dataManager, IDalamudPluginInterface pluginInterface ) {
            NpcCsv = npcCsv;
            MonsterJson = monsterJson;
            DataManager = dataManager;
            PluginInterface = pluginInterface;

            Items = new ItemSheetLoader();
            Actions = new ActionSheetLoader();
            Cutscenes = new CutsceneSheetLoader();
            Emotes = new EmoteSheetLoader();
            Gimmicks = new GimmickSheetLoader();
            NonPlayerActions = new NonPlayerActionSheetLoader();
            Npcs = new NpcSheetLoader();
            Statuses = new StatusSheetLoader();
            Zones = new ZoneSheetLoader();
            Mounts = new MountSheeetLoader();
            Housing = new HousingSheetLoader();
            Misc = new CommonLoader();
        }
    }
}
