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
    public class WeaponOptions
    {
        // Variables
        private Menu menu;

        public bool UnlimitedAmmo { get; private set; } = UserDefaults.WeaponsUnlimitedAmmo;
        public bool NoReload { get; private set; } = UserDefaults.WeaponsNoReload;
        public bool AutoEquipChute { get; private set; } = UserDefaults.AutoEquipChute;
        public bool UnlimitedParachutes { get; private set; } = UserDefaults.WeaponsUnlimitedParachutes;

        public static Dictionary<string, uint> AddonWeapons = new Dictionary<string, uint>();

        private Dictionary<Menu, ValidWeapon> weaponInfo = new Dictionary<Menu, ValidWeapon>();
        private Dictionary<MenuItem, string> weaponComponents = new Dictionary<MenuItem, string>();

        #region Create Menu
        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            #region create main weapon options menu and add items
            // Create the menu.
            menu = new Menu("YDDY:RP", "Оружие");

            MenuItem getAllWeapons = new MenuItem("Получить все оружие", "Оружие получено");
            MenuItem removeAllWeapons = new MenuItem("Убрать все оружие", "");
            MenuCheckboxItem unlimitedAmmo = new MenuCheckboxItem("Бесконечные патроны", "Бесконечные патроны ", UnlimitedAmmo);
            MenuCheckboxItem noReload = new MenuCheckboxItem("Отключить перезарядку", "Перезарядка отключена", NoReload);
            MenuItem setAmmo = new MenuItem("Установить количество патронов для всего оружия", "Установлено указанное количество патронов");
            MenuItem refillMaxAmmo = new MenuItem("Пополнить весь боезапас", "Максимальное количество патронов на всем оружие");
            MenuItem spawnByName = new MenuItem("Получить оружие по названию", "Введите название оружия");

            // Add items based on permissions
            if (IsAllowed(Permission.WPGetAll))
            {
                menu.AddMenuItem(getAllWeapons);
            }
            if (IsAllowed(Permission.WPRemoveAll))
            {
                menu.AddMenuItem(removeAllWeapons);
            }
            if (IsAllowed(Permission.WPUnlimitedAmmo))
            {
                menu.AddMenuItem(unlimitedAmmo);
            }
            if (IsAllowed(Permission.WPNoReload))
            {
                menu.AddMenuItem(noReload);
            }
            if (IsAllowed(Permission.WPSetAllAmmo))
            {
                menu.AddMenuItem(setAmmo);
                menu.AddMenuItem(refillMaxAmmo);
            }
            if (IsAllowed(Permission.WPSpawnByName))
            {
                menu.AddMenuItem(spawnByName);
            }
            #endregion

            #region addonweapons submenu
            MenuItem addonWeaponsBtn = new MenuItem("Аддоны", "");
            Menu addonWeaponsMenu = new Menu("Аддоны", "Меню аддонов");
            menu.AddMenuItem(addonWeaponsBtn);

            #region manage creating and accessing addon weapons menu
            if (IsAllowed(Permission.WPSpawn) && AddonWeapons != null && AddonWeapons.Count > 0)
            {
                MenuController.BindMenuItem(menu, addonWeaponsMenu, addonWeaponsBtn);
                foreach (KeyValuePair<string, uint> weapon in AddonWeapons)
                {
                    string name = weapon.Key.ToString();
                    uint model = weapon.Value;
                    var item = new MenuItem(name, $"Нажмите что бы добавить/удалить это оружие ({name}) в/из вашего инвентаря");
                    addonWeaponsMenu.AddMenuItem(item);
                    if (!IsWeaponValid(model))
                    {
                        item.Enabled = false;
                        item.LeftIcon = MenuItem.Icon.LOCK;
                        item.Description = "Данная модель недоступна.";
                    }
                }
                addonWeaponsMenu.OnItemSelect += (sender, item, index) =>
                {
                    var weapon = AddonWeapons.ElementAt(index);
                    if (HasPedGotWeapon(Game.PlayerPed.Handle, weapon.Value, false))
                    {
                        RemoveWeaponFromPed(Game.PlayerPed.Handle, weapon.Value);
                    }
                    else
                    {
                        var maxAmmo = 200;
                        GetMaxAmmo(Game.PlayerPed.Handle, weapon.Value, ref maxAmmo);
                        GiveWeaponToPed(Game.PlayerPed.Handle, weapon.Value, maxAmmo, false, true);
                    }
                };
                addonWeaponsBtn.Label = "→→→";
            }
            else
            {
                addonWeaponsBtn.LeftIcon = MenuItem.Icon.LOCK;
                addonWeaponsBtn.Enabled = false;
                addonWeaponsBtn.Description = "Данная опция недоступна для использования, у вас недостаточно прав или была загружена неправильно.";
            }
            #endregion
            addonWeaponsMenu.RefreshIndex();
            #endregion

            #region parachute options menu

            if (IsAllowed(Permission.WPParachute))
            {
                // main parachute options menu setup
                Menu parachuteMenu = new Menu("Парашюты", "Опции парашютов");
                MenuItem parachuteBtn = new MenuItem("Парашюты", "") { Label = "→→→" };

                MenuController.AddSubmenu(menu, parachuteMenu);
                menu.AddMenuItem(parachuteBtn);
                MenuController.BindMenuItem(menu, parachuteMenu, parachuteBtn);

                List<string> chutes = new List<string>()
                {
                    GetLabelText("PM_TINT0"),
                    GetLabelText("PM_TINT1"),
                    GetLabelText("PM_TINT2"),
                    GetLabelText("PM_TINT3"),
                    GetLabelText("PM_TINT4"),
                    GetLabelText("PM_TINT5"),
                    GetLabelText("PM_TINT6"),
                    GetLabelText("PM_TINT7"),

                    // broken in FiveM for some weird reason:
                    GetLabelText("PS_CAN_0"),
                    GetLabelText("PS_CAN_1"),
                    GetLabelText("PS_CAN_2"),
                    GetLabelText("PS_CAN_3"),
                    GetLabelText("PS_CAN_4"),
                    GetLabelText("PS_CAN_5")
                };
                List<string> chuteDescriptions = new List<string>()
                {
                    GetLabelText("PD_TINT0"),
                    GetLabelText("PD_TINT1"),
                    GetLabelText("PD_TINT2"),
                    GetLabelText("PD_TINT3"),
                    GetLabelText("PD_TINT4"),
                    GetLabelText("PD_TINT5"),
                    GetLabelText("PD_TINT6"),
                    GetLabelText("PD_TINT7"),

                    // broken in FiveM for some weird reason:
                    GetLabelText("PSD_CAN_0") + " ~r~For some reason this one doesn't seem to work in FiveM.",
                    GetLabelText("PSD_CAN_1") + " ~r~For some reason this one doesn't seem to work in FiveM.",
                    GetLabelText("PSD_CAN_2") + " ~r~For some reason this one doesn't seem to work in FiveM.",
                    GetLabelText("PSD_CAN_3") + " ~r~For some reason this one doesn't seem to work in FiveM.",
                    GetLabelText("PSD_CAN_4") + " ~r~For some reason this one doesn't seem to work in FiveM.",
                    GetLabelText("PSD_CAN_5") + " ~r~For some reason this one doesn't seem to work in FiveM."
                };

                MenuItem togglePrimary = new MenuItem("Основной парашют", "");
                MenuItem toggleReserve = new MenuItem("Резервный парашют", "");
                MenuListItem primaryChutes = new MenuListItem("Цвет основного", chutes, 0, $"Основной парашют: {chuteDescriptions[0]}");
                MenuListItem secondaryChutes = new MenuListItem("Цвет резервного", chutes, 0, $"Резервный парашют: {chuteDescriptions[0]}");
                MenuCheckboxItem unlimitedParachutes = new MenuCheckboxItem("Бесконечные парашюты", "", UnlimitedParachutes);
                MenuCheckboxItem autoEquipParachutes = new MenuCheckboxItem("Автоматические парашюты", "Получите парашют в самолете.", AutoEquipChute);

                // smoke color list
                List<string> smokeColorsList = new List<string>()
                {
                    GetLabelText("PM_TINT8"), // no smoke
                    GetLabelText("PM_TINT9"), // red
                    GetLabelText("PM_TINT10"), // orange
                    GetLabelText("PM_TINT11"), // yellow
                    GetLabelText("PM_TINT12"), // blue
                    GetLabelText("PM_TINT13"), // black
                };
                List<int[]> colors = new List<int[]>()
                {
                    new int[3] { 255, 255, 255 },
                    new int[3] { 255, 0, 0 },
                    new int[3] { 255, 165, 0 },
                    new int[3] { 255, 255, 0 },
                    new int[3] { 0, 0, 255 },
                    new int[3] { 20, 20, 20 },
                };

                MenuListItem smokeColors = new MenuListItem("Трейл", smokeColorsList, 0, "");

                parachuteMenu.AddMenuItem(togglePrimary);
                parachuteMenu.AddMenuItem(toggleReserve);
                parachuteMenu.AddMenuItem(autoEquipParachutes);
                parachuteMenu.AddMenuItem(unlimitedParachutes);
                parachuteMenu.AddMenuItem(smokeColors);
                parachuteMenu.AddMenuItem(primaryChutes);
                parachuteMenu.AddMenuItem(secondaryChutes);

                parachuteMenu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == togglePrimary)
                    {
                        if (HasPedGotWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("gadget_parachute"), false))
                        {
                            Subtitle.Custom("Основной парашют удален.");
                            RemoveWeaponFromPed(Game.PlayerPed.Handle, (uint)GetHashKey("gadget_parachute"));
                        }
                        else
                        {
                            Subtitle.Custom("Основной парашют добавлен.");
                            GiveWeaponToPed(Game.PlayerPed.Handle, (uint)GetHashKey("gadget_parachute"), 0, false, false);
                        }
                    }
                    else if (item == toggleReserve)
                    {
                        SetPlayerHasReserveParachute(Game.Player.Handle);
                        Subtitle.Custom("Резервный парашют добавлен.");

                    }
                };

                parachuteMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    if (item == unlimitedParachutes)
                    {
                        UnlimitedParachutes = _checked;
                    }
                    else if (item == autoEquipParachutes)
                    {
                        AutoEquipChute = _checked;
                    }
                };

                bool switching = false;
                async void IndexChangedEventHandler(Menu sender, MenuListItem item, int oldIndex, int newIndex, int itemIndex)
                {
                    if (item == smokeColors && oldIndex == -1)
                    {
                        if (!switching)
                        {
                            switching = true;
                            SetPlayerCanLeaveParachuteSmokeTrail(Game.Player.Handle, false);
                            await Delay(4000);
                            int[] color = colors[newIndex];
                            SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, color[0], color[1], color[2]);
                            SetPlayerCanLeaveParachuteSmokeTrail(Game.Player.Handle, newIndex != 0);
                            switching = false;
                        }
                    }
                    else if (item == primaryChutes)
                    {
                        item.Description = $"Основной парашют: {chuteDescriptions[newIndex]}";
                        SetPlayerParachuteTintIndex(Game.Player.Handle, newIndex);
                    }
                    else if (item == secondaryChutes)
                    {
                        item.Description = $"Резервный парашют: {chuteDescriptions[newIndex]}";
                        SetPlayerReserveParachuteTintIndex(Game.Player.Handle, newIndex);
                    }
                }

                parachuteMenu.OnListItemSelect += (sender, item, index, itemIndex) => IndexChangedEventHandler(sender, item, -1, index, itemIndex);
                parachuteMenu.OnListIndexChange += IndexChangedEventHandler;
            }
            #endregion

            #region Create Weapon Category Submenus
            MenuItem spacer = GetSpacerMenuItem("↓ Категории ↓");
            menu.AddMenuItem(spacer);

            Menu handGuns = new Menu("Оружие", "Пистолеты");
            MenuItem handGunsBtn = new MenuItem("Пистолеты");

            Menu rifles = new Menu("Оружие", "Винтовки");
            MenuItem riflesBtn = new MenuItem("Винтовки");

            Menu shotguns = new Menu("Оружие", "Дробовики");
            MenuItem shotgunsBtn = new MenuItem("Дробовики");

            Menu smgs = new Menu("Оружие", "Пистолеты-пулеметы");
            MenuItem smgsBtn = new MenuItem("Пистолеты-пулеметы");

            Menu throwables = new Menu("Оружие", "Кидаемое");
            MenuItem throwablesBtn = new MenuItem("Кидаемое");

            Menu melee = new Menu("Оружие", "Ближнего боя");
            MenuItem meleeBtn = new MenuItem("Ближнего боя");

            Menu heavy = new Menu("Оружие", "Тяжелое");
            MenuItem heavyBtn = new MenuItem("Тяжелое");

            Menu snipers = new Menu("Оружие", "Снайперские винтовки");
            MenuItem snipersBtn = new MenuItem("Снайперские винтовки");

            MenuController.AddSubmenu(menu, handGuns);
            MenuController.AddSubmenu(menu, rifles);
            MenuController.AddSubmenu(menu, shotguns);
            MenuController.AddSubmenu(menu, smgs);
            MenuController.AddSubmenu(menu, throwables);
            MenuController.AddSubmenu(menu, melee);
            MenuController.AddSubmenu(menu, heavy);
            MenuController.AddSubmenu(menu, snipers);
            #endregion

            #region Setup weapon category buttons and submenus.
            handGunsBtn.Label = "→→→";
            menu.AddMenuItem(handGunsBtn);
            MenuController.BindMenuItem(menu, handGuns, handGunsBtn);

            riflesBtn.Label = "→→→";
            menu.AddMenuItem(riflesBtn);
            MenuController.BindMenuItem(menu, rifles, riflesBtn);

            shotgunsBtn.Label = "→→→";
            menu.AddMenuItem(shotgunsBtn);
            MenuController.BindMenuItem(menu, shotguns, shotgunsBtn);

            smgsBtn.Label = "→→→";
            menu.AddMenuItem(smgsBtn);
            MenuController.BindMenuItem(menu, smgs, smgsBtn);

            throwablesBtn.Label = "→→→";
            menu.AddMenuItem(throwablesBtn);
            MenuController.BindMenuItem(menu, throwables, throwablesBtn);

            meleeBtn.Label = "→→→";
            menu.AddMenuItem(meleeBtn);
            MenuController.BindMenuItem(menu, melee, meleeBtn);

            heavyBtn.Label = "→→→";
            menu.AddMenuItem(heavyBtn);
            MenuController.BindMenuItem(menu, heavy, heavyBtn);

            snipersBtn.Label = "→→→";
            menu.AddMenuItem(snipersBtn);
            MenuController.BindMenuItem(menu, snipers, snipersBtn);
            #endregion

            #region Loop through all weapons, create menus for them and add all menu items and handle events.
            foreach (ValidWeapon weapon in ValidWeapons.WeaponList)
            {
                uint cat = (uint)GetWeapontypeGroup(weapon.Hash);
                if (!string.IsNullOrEmpty(weapon.Name) && (IsAllowed(weapon.Perm) || IsAllowed(Permission.WPGetAll)))
                {
                    //Log($"[DEBUG LOG] [WEAPON-BUG] {weapon.Name} - {weapon.Perm} = {IsAllowed(weapon.Perm)} & All = {IsAllowed(Permission.WPGetAll)}");
                    #region Create menu for this weapon and add buttons
                    Menu weaponMenu = new Menu("Опции", weapon.Name);
                    MenuItem weaponItem = new MenuItem(weapon.Name, $"Открыты настройки для ~y~{weapon.Name.ToString()}~s~.")
                    {
                        Label = "→→→",
                        LeftIcon = MenuItem.Icon.GUN
                    };

                    weaponInfo.Add(weaponMenu, weapon);

                    MenuItem getOrRemoveWeapon = new MenuItem("Взять/Убрать", "")
                    {
                        LeftIcon = MenuItem.Icon.GUN
                    };
                    weaponMenu.AddMenuItem(getOrRemoveWeapon);
                    if (!IsAllowed(Permission.WPSpawn))
                    {
                        getOrRemoveWeapon.Enabled = false;
                        getOrRemoveWeapon.Description = "У вас недостаточно прав для использования этой опции.";
                        getOrRemoveWeapon.LeftIcon = MenuItem.Icon.LOCK;
                    }

                    MenuItem fillAmmo = new MenuItem("Пополнить боезапас", "")
                    {
                        LeftIcon = MenuItem.Icon.AMMO
                    };
                    weaponMenu.AddMenuItem(fillAmmo);

                    List<string> tints = new List<string>();
                    if (weapon.Name.Contains(" Mk II"))
                    {
                        foreach (var tint in ValidWeapons.WeaponTintsMkII)
                        {
                            tints.Add(tint.Key);
                        }
                    }
                    else
                    {
                        foreach (var tint in ValidWeapons.WeaponTints)
                        {
                            tints.Add(tint.Key);
                        }
                    }

                    MenuListItem weaponTints = new MenuListItem("Раскраски", tints, 0, "");
                    weaponMenu.AddMenuItem(weaponTints);
                    #endregion

                    #region Handle weapon specific list changes
                    weaponMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
                    {
                        if (item == weaponTints)
                        {
                            if (HasPedGotWeapon(Game.PlayerPed.Handle, weaponInfo[sender].Hash, false))
                            {
                                SetPedWeaponTintIndex(Game.PlayerPed.Handle, weaponInfo[sender].Hash, newIndex);
                            }
                            else
                            {
                                Notify.Error("Вам необходимо сначала получить оружие!");
                            }
                        }
                    };
                    #endregion

                    #region Handle weapon specific button presses
                    weaponMenu.OnItemSelect += (sender, item, index) =>
                    {
                        var info = weaponInfo[sender];
                        uint hash = info.Hash;

                        SetCurrentPedWeapon(Game.PlayerPed.Handle, hash, true);

                        if (item == getOrRemoveWeapon)
                        {
                            if (HasPedGotWeapon(Game.PlayerPed.Handle, hash, false))
                            {
                                RemoveWeaponFromPed(Game.PlayerPed.Handle, hash);
                                Subtitle.Custom("Оружие удалено.");
                            }
                            else
                            {
                                var ammo = 255;
                                GetMaxAmmo(Game.PlayerPed.Handle, hash, ref ammo);
                                GiveWeaponToPed(Game.PlayerPed.Handle, hash, ammo, false, true);
                                Subtitle.Custom("Оружие добавлено.");
                            }
                        }
                        else if (item == fillAmmo)
                        {
                            if (HasPedGotWeapon(Game.PlayerPed.Handle, hash, false))
                            {
                                var ammo = 900;
                                GetMaxAmmo(Game.PlayerPed.Handle, hash, ref ammo);
                                SetPedAmmo(Game.PlayerPed.Handle, hash, ammo);
                            }
                            else
                            {
                                Notify.Error("Вам нужно сначала получить оружие!");
                            }
                        }
                    };
                    #endregion

                    #region load components
                    if (weapon.Components != null)
                    {
                        if (weapon.Components.Count > 0)
                        {
                            foreach (var comp in weapon.Components)
                            {
                                //Log($"{weapon.Name} : {comp.Key}");
                                MenuItem compItem = new MenuItem(comp.Key, "Нажмите что бы добавить этот компонент.");
                                weaponComponents.Add(compItem, comp.Key);
                                weaponMenu.AddMenuItem(compItem);

                                #region Handle component button presses
                                weaponMenu.OnItemSelect += (sender, item, index) =>
                                {
                                    if (item == compItem)
                                    {
                                        var Weapon = weaponInfo[sender];
                                        var componentHash = Weapon.Components[weaponComponents[item]];
                                        if (HasPedGotWeapon(Game.PlayerPed.Handle, Weapon.Hash, false))
                                        {
                                            SetCurrentPedWeapon(Game.PlayerPed.Handle, Weapon.Hash, true);
                                            if (HasPedGotWeaponComponent(Game.PlayerPed.Handle, Weapon.Hash, componentHash))
                                            {
                                                RemoveWeaponComponentFromPed(Game.PlayerPed.Handle, Weapon.Hash, componentHash);

                                                Subtitle.Custom("Компонент добавлен.");
                                            }
                                            else
                                            {
                                                int ammo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, Weapon.Hash);

                                                int clipAmmo = GetMaxAmmoInClip(Game.PlayerPed.Handle, Weapon.Hash, false);
                                                GetAmmoInClip(Game.PlayerPed.Handle, Weapon.Hash, ref clipAmmo);

                                                GiveWeaponComponentToPed(Game.PlayerPed.Handle, Weapon.Hash, componentHash);

                                                SetAmmoInClip(Game.PlayerPed.Handle, Weapon.Hash, clipAmmo);

                                                SetPedAmmo(Game.PlayerPed.Handle, Weapon.Hash, ammo);
                                                Subtitle.Custom("Компонент экипирован.");
                                            }
                                        }
                                        else
                                        {
                                            Notify.Error("Вам необходимо сначала получить оружие!");
                                        }
                                    }
                                };
                                #endregion
                            }
                        }
                    }
                    #endregion

                    // refresh and add to menu.
                    weaponMenu.RefreshIndex();

                    if (cat == 970310034) // 970310034 rifles
                    {
                        MenuController.AddSubmenu(rifles, weaponMenu);
                        MenuController.BindMenuItem(rifles, weaponMenu, weaponItem);
                        rifles.AddMenuItem(weaponItem);
                    }
                    else if (cat == 416676503 || cat == 690389602) // 416676503 hand guns // 690389602 stun gun
                    {
                        MenuController.AddSubmenu(handGuns, weaponMenu);
                        MenuController.BindMenuItem(handGuns, weaponMenu, weaponItem);
                        handGuns.AddMenuItem(weaponItem);
                    }
                    else if (cat == 860033945) // 860033945 shotguns
                    {
                        MenuController.AddSubmenu(shotguns, weaponMenu);
                        MenuController.BindMenuItem(shotguns, weaponMenu, weaponItem);
                        shotguns.AddMenuItem(weaponItem);
                    }
                    else if (cat == 3337201093 || cat == 1159398588) // 3337201093 sub machine guns // 1159398588 light machine guns
                    {
                        MenuController.AddSubmenu(smgs, weaponMenu);
                        MenuController.BindMenuItem(smgs, weaponMenu, weaponItem);
                        smgs.AddMenuItem(weaponItem);
                    }
                    else if (cat == 1548507267 || cat == 4257178988 || cat == 1595662460) // 1548507267 throwables // 4257178988 fire extinghuiser // jerry can
                    {
                        MenuController.AddSubmenu(throwables, weaponMenu);
                        MenuController.BindMenuItem(throwables, weaponMenu, weaponItem);
                        throwables.AddMenuItem(weaponItem);
                    }
                    else if (cat == 3566412244 || cat == 2685387236) // 3566412244 melee weapons // 2685387236 knuckle duster
                    {
                        MenuController.AddSubmenu(melee, weaponMenu);
                        MenuController.BindMenuItem(melee, weaponMenu, weaponItem);
                        melee.AddMenuItem(weaponItem);
                    }
                    else if (cat == 2725924767) // 2725924767 heavy weapons
                    {
                        MenuController.AddSubmenu(heavy, weaponMenu);
                        MenuController.BindMenuItem(heavy, weaponMenu, weaponItem);
                        heavy.AddMenuItem(weaponItem);
                    }
                    else if (cat == 3082541095) // 3082541095 sniper rifles
                    {
                        MenuController.AddSubmenu(snipers, weaponMenu);
                        MenuController.BindMenuItem(snipers, weaponMenu, weaponItem);
                        snipers.AddMenuItem(weaponItem);
                    }
                }
            }
            #endregion

            #region Disable submenus if no weapons in that category are allowed.
            if (handGuns.Size == 0)
            {
                handGunsBtn.LeftIcon = MenuItem.Icon.LOCK;
                handGunsBtn.Description = "У вас недостаточно полномочий для использования оружия из этой категории.";
                handGunsBtn.Enabled = false;
            }
            if (rifles.Size == 0)
            {
                riflesBtn.LeftIcon = MenuItem.Icon.LOCK;
                riflesBtn.Description = "У вас недостаточно полномочий для использования оружия из этой категории.";
                riflesBtn.Enabled = false;
            }
            if (shotguns.Size == 0)
            {
                shotgunsBtn.LeftIcon = MenuItem.Icon.LOCK;
                shotgunsBtn.Description = "У вас недостаточно полномочий для использования оружия из этой категории.";
                shotgunsBtn.Enabled = false;
            }
            if (smgs.Size == 0)
            {
                smgsBtn.LeftIcon = MenuItem.Icon.LOCK;
                smgsBtn.Description = "У вас недостаточно полномочий для использования оружия из этой категории..";
                smgsBtn.Enabled = false;
            }
            if (throwables.Size == 0)
            {
                throwablesBtn.LeftIcon = MenuItem.Icon.LOCK;
                throwablesBtn.Description = "У вас недостаточно полномочий для использования оружия из этой категории.";
                throwablesBtn.Enabled = false;
            }
            if (melee.Size == 0)
            {
                meleeBtn.LeftIcon = MenuItem.Icon.LOCK;
                meleeBtn.Description = "У вас недостаточно полномочий для использования оружия из этой категории.";
                meleeBtn.Enabled = false;
            }
            if (heavy.Size == 0)
            {
                heavyBtn.LeftIcon = MenuItem.Icon.LOCK;
                heavyBtn.Description = "У вас недостаточно полномочий для использования оружия из этой категории.";
                heavyBtn.Enabled = false;
            }
            if (snipers.Size == 0)
            {
                snipersBtn.LeftIcon = MenuItem.Icon.LOCK;
                snipersBtn.Description = "У вас недостаточно полномочий для использования оружия из этой категории.";
                snipersBtn.Enabled = false;
            }
            #endregion

            #region Handle button presses
            menu.OnItemSelect += (sender, item, index) =>
            {
                Ped ped = new Ped(Game.PlayerPed.Handle);
                if (item == getAllWeapons)
                {
                    foreach (ValidWeapon vw in ValidWeapons.WeaponList)
                    {
                        if (IsAllowed(vw.Perm))
                        {
                            GiveWeaponToPed(Game.PlayerPed.Handle, vw.Hash, vw.GetMaxAmmo, false, true);

                            int ammoInClip = GetMaxAmmoInClip(Game.PlayerPed.Handle, vw.Hash, false);
                            SetAmmoInClip(Game.PlayerPed.Handle, vw.Hash, ammoInClip);
                            int ammo = 0;
                            GetMaxAmmo(Game.PlayerPed.Handle, vw.Hash, ref ammo);
                            SetPedAmmo(Game.PlayerPed.Handle, vw.Hash, ammo);
                        }
                    }

                    SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("weapon_unarmed"), true);
                }
                else if (item == removeAllWeapons)
                {
                    ped.Weapons.RemoveAll();
                }
                else if (item == setAmmo)
                {
                    SetAllWeaponsAmmo();
                }
                else if (item == refillMaxAmmo)
                {
                    foreach (ValidWeapon vw in ValidWeapons.WeaponList)
                    {
                        if (HasPedGotWeapon(Game.PlayerPed.Handle, vw.Hash, false))
                        {
                            int ammoInClip = GetMaxAmmoInClip(Game.PlayerPed.Handle, vw.Hash, false);
                            SetAmmoInClip(Game.PlayerPed.Handle, vw.Hash, ammoInClip);
                            int ammo = 0;
                            GetMaxAmmo(Game.PlayerPed.Handle, vw.Hash, ref ammo);
                            SetPedAmmo(Game.PlayerPed.Handle, vw.Hash, ammo);
                        }
                    }
                }
                else if (item == spawnByName)
                {
                    SpawnCustomWeapon();
                }
            };
            #endregion

            #region Handle checkbox changes
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == noReload)
                {
                    NoReload = _checked;
                    Subtitle.Custom($"Перезарядка {(_checked ? "включена" : "отключена")}.");
                }
                else if (item == unlimitedAmmo)
                {
                    UnlimitedAmmo = _checked;
                    Subtitle.Custom($"Бесконечные патроны {(_checked ? "включены" : "выключены")}.");
                }
            };
            #endregion
        }


        #endregion

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
    }
}
