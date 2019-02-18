using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class MiscSettings
    {
        // Variables
        private Menu menu;

        public bool ShowSpeedoKmh { get; private set; } = UserDefaults.MiscSpeedKmh;
        public bool ShowSpeedoMph { get; private set; } = UserDefaults.MiscSpeedMph;
        public bool ShowCoordinates { get; private set; } = false;
        public bool HideHud { get; private set; } = false;
        public bool HideRadar { get; private set; } = false;
        public bool ShowLocation { get; private set; } = UserDefaults.MiscShowLocation;
        public bool DeathNotifications { get; private set; } = UserDefaults.MiscDeathNotifications;
        public bool JoinQuitNotifications { get; private set; } = UserDefaults.MiscJoinQuitNotifications;
        public bool LockCameraX { get; private set; } = false;
        public bool LockCameraY { get; private set; } = false;
        public bool ShowLocationBlips { get; private set; } = UserDefaults.MiscLocationBlips;
        public bool ShowPlayerBlips { get; private set; } = UserDefaults.MiscShowPlayerBlips;
        public bool ShowVehicleModelDimensions { get; private set; } = false;
        public bool MiscRespawnDefaultCharacter { get; private set; } = UserDefaults.MiscRespawnDefaultCharacter;
        public bool RestorePlayerAppearance { get; private set; } = UserDefaults.MiscRestorePlayerAppearance;
        public bool RestorePlayerWeapons { get; private set; } = UserDefaults.MiscRestorePlayerWeapons;
        public bool DrawTimeOnScreen { get; internal set; } = UserDefaults.MiscShowTime;
        public bool MiscRightAlignMenu { get; private set; } = UserDefaults.MiscRightAlignMenu;

        // keybind states
        public bool KbTpToWaypoint { get; private set; } = UserDefaults.KbTpToWaypoint;
        public int KbTpToWaypointKey { get; } = vMenuShared.ConfigManager.GetSettingsInt(vMenuShared.ConfigManager.Setting.vmenu_teleport_to_wp_keybind_key) != -1
            ? vMenuShared.ConfigManager.GetSettingsInt(vMenuShared.ConfigManager.Setting.vmenu_teleport_to_wp_keybind_key)
            : 168; // 168 (F7 by default)
        public bool KbDriftMode { get; private set; } = UserDefaults.KbDriftMode;

        private List<Vector3> tpLocations = new List<Vector3>();
        private List<float> tpLocationsHeading = new List<float>();

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            try
            {
                MenuController.MenuAlignment = MiscRightAlignMenu ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
            }
            catch (AspectRatioException)
            {
                Notify.Error(CommonErrors.RightAlignedNotSupported);
                // (re)set the default to left just in case so they don't get this error again in the future.
                MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                MiscRightAlignMenu = false;
                UserDefaults.MiscRightAlignMenu = false;
            }

            // Create the menu.
            menu = new Menu("YDDY:RP", "Прочие настройки");

            // teleport menu
            Menu teleportMenu = new Menu(Game.Player.Name, "Локации для телепорта");
            MenuItem teleportMenuBtn = new MenuItem("Локации для телепорта", "Телепорт в места добавленные админимтратором сервера.");
            MenuController.AddSubmenu(menu, teleportMenu);
            MenuController.BindMenuItem(menu, teleportMenu, teleportMenuBtn);

            // keybind settings menu
            Menu keybindMenu = new Menu(Game.Player.Name, "Бинды");
            MenuItem keybindMenuBtn = new MenuItem("Настройки биндов", "Включите или выключите бинды на функции трейнера.");
            MenuController.AddSubmenu(menu, keybindMenu);
            MenuController.BindMenuItem(menu, keybindMenu, keybindMenuBtn);

            // keybind settings menu items
            MenuCheckboxItem kbTpToWaypoint = new MenuCheckboxItem("Телепорт на метку", "Телепортирует Вас к метке на карте при нажатии клавиши ~r~F7", KbTpToWaypoint);
            MenuCheckboxItem kbDriftMode = new MenuCheckboxItem("Дрифт мод", "При удерживании ~r~Left Shift~s~ снижает тягу автомобиля до нуля", KbDriftMode);
            MenuItem backBtn = new MenuItem("Назад");

            // Create the menu items.
            MenuItem tptowp = new MenuItem("Телепорт на метку", "Телепорт к метке указанной на вашей карте.");
            MenuCheckboxItem rightAlignMenu = new MenuCheckboxItem("Меню справа", "", MiscRightAlignMenu);
            MenuCheckboxItem speedKmh = new MenuCheckboxItem("Спидометр в км/ч", "", ShowSpeedoKmh);
            MenuCheckboxItem speedMph = new MenuCheckboxItem("Спидометр в м/ч", "", ShowSpeedoMph);
            MenuCheckboxItem coords = new MenuCheckboxItem("Показать координаты", "Показать текущие координаты сверху экрана.", ShowCoordinates);
            MenuCheckboxItem hideRadar = new MenuCheckboxItem("Спрятать радар", "", HideRadar);
            MenuCheckboxItem hideHud = new MenuCheckboxItem("Спрятать худ", "", HideHud);
            MenuCheckboxItem showLocation = new MenuCheckboxItem("Показывать локации", "Показывает ваше текущее местоположение, сторону в которое вы смотрите и ближайщий перекресток.", ShowLocation) { LeftIcon = MenuItem.Icon.WARNING };
            MenuCheckboxItem drawTime = new MenuCheckboxItem("Показывать игровое время", "", DrawTimeOnScreen);
            MenuItem saveSettings = new MenuItem("Сохранить настройки", "")
            {
                RightIcon = MenuItem.Icon.TICK
            };
            MenuCheckboxItem joinQuitNotifs = new MenuCheckboxItem("Уведомления о входе / выходе", "", JoinQuitNotifications);
            MenuCheckboxItem deathNotifs = new MenuCheckboxItem("Уведомления о смерти", "", DeathNotifications);
            MenuCheckboxItem nightVision = new MenuCheckboxItem("Ночное видение", "", false);
            MenuCheckboxItem thermalVision = new MenuCheckboxItem("Включить тепловизор", "Включить или выключить тепловизор.", false);
            MenuCheckboxItem modelDimensions = new MenuCheckboxItem("Показывать размеры т/с", "", ShowVehicleModelDimensions);

            MenuItem clearArea = new MenuItem("Очистить зону", "");
            MenuCheckboxItem lockCamX = new MenuCheckboxItem("Заблокировать камеру горизонтально", "", false);
            MenuCheckboxItem lockCamY = new MenuCheckboxItem("Заблокировать камеру вертикально", "", false);

            Menu connectionSubmenu = new Menu("YDDY:RP", "Опции подключения");
            MenuItem connectionSubmenuBtn = new MenuItem("Подключение", "Опции подключения к серверу");
            MenuItem quitSession = new MenuItem("Выйти из сессии", "");
            MenuItem quitGame = new MenuItem("Выйти из игры", "");
            MenuItem disconnectFromServer = new MenuItem("Отключиться от сервера", "");
            connectionSubmenu.AddMenuItem(quitSession);
            connectionSubmenu.AddMenuItem(quitGame);
            connectionSubmenu.AddMenuItem(disconnectFromServer);

            MenuCheckboxItem locationBlips = new MenuCheckboxItem("Метки локаций", "Показывает метки различных мест на карте.", ShowLocationBlips);
            MenuCheckboxItem playerBlips = new MenuCheckboxItem("Показывать метки игроков", "Показывает метки всех игроков на карте.", ShowPlayerBlips);
            MenuCheckboxItem respawnDefaultCharacter = new MenuCheckboxItem("Спавниться как MP по умолчанию", "", MiscRespawnDefaultCharacter);
            MenuCheckboxItem restorePlayerAppearance = new MenuCheckboxItem("Восстанавливать внешний вид", "", RestorePlayerAppearance);
            MenuCheckboxItem restorePlayerWeapons = new MenuCheckboxItem("Восстанавливать оружие", "", RestorePlayerWeapons);

            MenuController.AddSubmenu(menu, connectionSubmenu);
            MenuController.BindMenuItem(menu, connectionSubmenu, connectionSubmenuBtn);
            //MainMenu.Mp.Add(connectionSubmenu);
            //connectionSubmenu.RefreshIndex();
            //connectionSubmenu.UpdateScaleform();
            //menu.BindMenuToItem(connectionSubmenu, connectionSubmenuBtn);

            keybindMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == kbTpToWaypoint)
                {
                    KbTpToWaypoint = _checked;
                }
                else if (item == kbDriftMode)
                {
                    KbDriftMode = _checked;
                }
            };
            keybindMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == backBtn)
                {
                    keybindMenu.GoBack();
                }
            };

            connectionSubmenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == quitGame)
                {
                    QuitGame();
                }
                else if (item == quitSession)
                {
                    QuitSession();
                }
                else if (item == disconnectFromServer)
                {
                    BaseScript.TriggerServerEvent("vMenu:DisconnectSelf");
                }
            };

            // Add menu items to the menu.
            if (IsAllowed(Permission.MSTeleportToWp))
            {
                menu.AddMenuItem(tptowp);
                keybindMenu.AddMenuItem(kbTpToWaypoint);
            }

            if (IsAllowed(Permission.MSDriftMode))
            {
                keybindMenu.AddMenuItem(kbDriftMode);
            }

            keybindMenu.AddMenuItem(backBtn);

            // Always allowed
            menu.AddMenuItem(rightAlignMenu);
            menu.AddMenuItem(speedKmh);
            menu.AddMenuItem(speedMph);
            menu.AddMenuItem(modelDimensions);
            menu.AddMenuItem(keybindMenuBtn);
            keybindMenuBtn.Label = "→→→";
            if (IsAllowed(Permission.MSConnectionMenu))
            {
                menu.AddMenuItem(connectionSubmenuBtn);
                connectionSubmenuBtn.Label = "→→→";
            }
            if (IsAllowed(Permission.MSShowCoordinates))
            {
                menu.AddMenuItem(coords);
            }
            if (IsAllowed(Permission.MSShowLocation))
            {
                menu.AddMenuItem(showLocation);
            }
            menu.AddMenuItem(drawTime); // always allowed
            if (IsAllowed(Permission.MSJoinQuitNotifs))
            {
                menu.AddMenuItem(deathNotifs);
            }
            if (IsAllowed(Permission.MSDeathNotifs))
            {
                menu.AddMenuItem(joinQuitNotifs);
            }
            if (IsAllowed(Permission.MSNightVision))
            {
                menu.AddMenuItem(nightVision);
            }
            if (IsAllowed(Permission.MSThermalVision))
            {
                menu.AddMenuItem(thermalVision);
            }
            if (IsAllowed(Permission.MSLocationBlips))
            {
                menu.AddMenuItem(locationBlips);
                ToggleBlips(ShowLocationBlips);
            }
            if (IsAllowed(Permission.MSPlayerBlips))
            {
                menu.AddMenuItem(playerBlips);
            }
            if (IsAllowed(Permission.MSTeleportLocations))
            {
                menu.AddMenuItem(teleportMenuBtn);
                teleportMenuBtn.Label = "→→→";

                string json = LoadResourceFile(GetCurrentResourceName(), "config/locations.json");
                if (string.IsNullOrEmpty(json))
                {
                    Notify.Error("An error occurred while loading the locations file.");
                }
                else
                {
                    try
                    {
                        Newtonsoft.Json.Linq.JObject data = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);
                        foreach (Newtonsoft.Json.Linq.JToken teleport in data["teleports"])
                        {
                            string name = teleport["name"].ToString();
                            float heading = (float)teleport["heading"];
                            Vector3 coordinates = new Vector3((float)teleport["coordinates"]["x"], (float)teleport["coordinates"]["y"], (float)teleport["coordinates"]["z"]);
                            MenuItem tpBtn = new MenuItem(name, $"Телепортироваться к X: {(int)coordinates.X} Y: {(int)coordinates.Y} Z: {(int)coordinates.Z} HEADING: {(int)heading}.");
                            teleportMenu.AddMenuItem(tpBtn);
                            tpLocations.Add(coordinates);
                            tpLocationsHeading.Add(heading);
                        }
                        teleportMenu.OnItemSelect += async (sender, item, index) =>
                        {
                            await TeleportToCoords(tpLocations[index], true);
                            SetEntityHeading(Game.PlayerPed.Handle, tpLocationsHeading[index]);
                        };
                    }
                    catch (JsonReaderException ex)
                    {
                        Debug.Write($"\n[vMenu] An error occurred whie loading the teleport locations!\nReport the following error details to the server owner:\n{ex.Message}.\n");
                    }
                }
            }
            if (IsAllowed(Permission.MSClearArea))
            {
                menu.AddMenuItem(clearArea);
            }
            // always allowed, it just won't do anything if the server owner disabled the feature, but players can still toggle it.
            menu.AddMenuItem(respawnDefaultCharacter);
            if (IsAllowed(Permission.MSRestoreAppearance))
            {
                menu.AddMenuItem(restorePlayerAppearance);
            }
            if (IsAllowed(Permission.MSRestoreWeapons))
            {
                menu.AddMenuItem(restorePlayerWeapons);
            }

            // Always allowed
            menu.AddMenuItem(hideRadar);
            menu.AddMenuItem(hideHud);
            menu.AddMenuItem(lockCamX);
            menu.AddMenuItem(lockCamY);
            menu.AddMenuItem(saveSettings);

            // Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == rightAlignMenu)
                {
                    try
                    {
                        MenuController.MenuAlignment = _checked ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
                        MiscRightAlignMenu = _checked;
                        UserDefaults.MiscRightAlignMenu = MiscRightAlignMenu;
                    }
                    catch (AspectRatioException)
                    {
                        Notify.Error(CommonErrors.RightAlignedNotSupported);
                        // (re)set the default to left just in case so they don't get this error again in the future.
                        MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                        MiscRightAlignMenu = false;
                        UserDefaults.MiscRightAlignMenu = false;
                    }
                }
                if (item == speedKmh)
                {
                    ShowSpeedoKmh = _checked;
                }
                else if (item == speedMph)
                {
                    ShowSpeedoMph = _checked;
                }
                else if (item == coords)
                {
                    ShowCoordinates = _checked;
                }
                else if (item == hideHud)
                {
                    HideHud = _checked;
                    DisplayHud(!_checked);
                }
                else if (item == hideRadar)
                {
                    HideRadar = _checked;
                    if (!_checked)
                    {
                        DisplayRadar(true);
                    }
                }
                else if (item == showLocation)
                {
                    ShowLocation = _checked;
                }
                else if (item == drawTime)
                {
                    DrawTimeOnScreen = _checked;
                }
                else if (item == deathNotifs)
                {
                    DeathNotifications = _checked;
                }
                else if (item == joinQuitNotifs)
                {
                    JoinQuitNotifications = _checked;
                }
                else if (item == nightVision)
                {
                    SetNightvision(_checked);
                }
                else if (item == thermalVision)
                {
                    SetSeethrough(_checked);
                }
                else if (item == lockCamX)
                {
                    LockCameraX = _checked;
                }
                else if (item == lockCamY)
                {
                    LockCameraY = _checked;
                }
                else if (item == locationBlips)
                {
                    ToggleBlips(_checked);
                    ShowLocationBlips = _checked;
                }
                else if (item == playerBlips)
                {
                    ShowPlayerBlips = _checked;
                }
                else if (item == respawnDefaultCharacter)
                {
                    MiscRespawnDefaultCharacter = _checked;
                }
                else if (item == restorePlayerAppearance)
                {
                    RestorePlayerAppearance = _checked;
                }
                else if (item == restorePlayerWeapons)
                {
                    RestorePlayerWeapons = _checked;
                }
                else if (item == modelDimensions)
                {
                    ShowVehicleModelDimensions = _checked;
                }
            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // Teleport to waypoint.
                if (item == tptowp)
                {
                    TeleportToWp();
                }
                // save settings
                else if (item == saveSettings)
                {
                    UserDefaults.SaveSettings();
                }
                // clear area
                else if (item == clearArea)
                {
                    var pos = Game.PlayerPed.Position;
                    BaseScript.TriggerServerEvent("vMenu:ClearArea", pos.X, pos.Y, pos.Z);
                }
            };
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }

        private struct Blip
        {
            public readonly Vector3 Location;
            public readonly int Sprite;
            public readonly string Name;
            public readonly int Color;
            public readonly int blipID;

            public Blip(Vector3 Location, int Sprite, string Name, int Color, int blipID)
            {
                this.Location = Location;
                this.Sprite = Sprite;
                this.Name = Name;
                this.Color = Color;
                this.blipID = blipID;
            }
        }

        private List<Blip> blips = new List<Blip>();

        /// <summary>
        /// Toggles blips on/off.
        /// </summary>
        /// <param name="enable"></param>
        private void ToggleBlips(bool enable)
        {
            if (enable)
            {
                string json = LoadResourceFile(GetCurrentResourceName(), "config/locations.json");
                if (string.IsNullOrEmpty(json))
                {
                    Notify.Error("Произошла ошибка при загрузке файла местоположений.");
                }
                else
                {
                    try
                    {
                        Newtonsoft.Json.Linq.JObject data = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);
                        if (data != null && data["blips"] != null && data["blips"].Count() > 0)
                        {
                            foreach (Newtonsoft.Json.Linq.JToken blip in data["blips"])
                            {
                                string name = blip["name"].ToString();
                                int color = (int)blip["color"];
                                int spriteID = (int)blip["spriteID"];
                                Vector3 coords = new Vector3((float)blip["coordinates"]["x"], (float)blip["coordinates"]["y"], (float)blip["coordinates"]["z"]);
                                int blipID = AddBlipForCoord(coords.X, coords.Y, coords.Z);
                                SetBlipSprite(blipID, spriteID);
                                BeginTextCommandSetBlipName("STRING");
                                AddTextComponentSubstringPlayerName(name);
                                EndTextCommandSetBlipName(blipID);
                                SetBlipColour(blipID, color);
                                SetBlipAsShortRange(blipID, true);


                                Blip b = new Blip(coords, spriteID, name, color, blipID);
                                blips.Add(b);
                            }
                        }

                    }
                    catch (JsonReaderException ex)
                    {
                        Debug.Write($"\n\n[vMenu] An error occurred while loading the locations.json file. Please contact the server owner to resolve this.\nWhen contacting the owner, provide the following error details:\n{ex.Message}.\n\n\n");
                    }
                }
            }
            else
            {
                if (blips.Count > 0)
                {
                    foreach (Blip blip in blips)
                    {
                        int id = blip.blipID;
                        if (DoesBlipExist(id))
                        {
                            RemoveBlip(ref id);
                        }
                    }
                }
                blips.Clear();
            }
        }

    }
}
