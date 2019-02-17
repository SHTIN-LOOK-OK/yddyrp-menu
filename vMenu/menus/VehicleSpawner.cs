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
    public class VehicleSpawner
    {
        // Variables
        private Menu menu;
        public static Dictionary<string, uint> AddonVehicles;

        public bool SpawnInVehicle { get; private set; } = UserDefaults.VehicleSpawnerSpawnInside;
        public bool ReplaceVehicle { get; private set; } = UserDefaults.VehicleSpawnerReplacePrevious;
        public static List<bool> allowedCategories;

        private void CreateMenu()
        {
            #region initial setup.
            // Create the menu.
            menu = new Menu("YDDY:RP", "Меню спавна автомобилей");

            // Create the buttons and checkboxes.
            MenuItem spawnByName = new MenuItem("Заспавнить по имени", "");
            MenuCheckboxItem spawnInVeh = new MenuCheckboxItem("Спавниться в автомобиле", "", SpawnInVehicle);
            MenuCheckboxItem replacePrev = new MenuCheckboxItem("Заменять предыдущий", "", ReplaceVehicle);

            // Add the items to the menu.
            if (IsAllowed(Permission.VSSpawnByName))
            {
                menu.AddMenuItem(spawnByName);
            }
            menu.AddMenuItem(spawnInVeh);
            menu.AddMenuItem(replacePrev);
            #endregion

            #region addon cars menu
            // Vehicle Addons List
            Menu addonCarsMenu = new Menu("Аддоны", "Заспавнить");
            MenuItem addonCarsBtn = new MenuItem("Аддоны", "") { Label = "→→→" };

            menu.AddMenuItem(addonCarsBtn);

            if (IsAllowed(Permission.VSAddon))
            {
                if (AddonVehicles != null)
                {
                    if (AddonVehicles.Count > 0)
                    {
                        MenuController.BindMenuItem(menu, addonCarsMenu, addonCarsBtn);
                        MenuController.AddSubmenu(menu, addonCarsMenu);
                        Menu unavailableCars = new Menu("Меню спавна аддонов", "Недоступные автомобили");
                        MenuItem unavailableCarsBtn = new MenuItem("Недоступные автомобили", "На данный момент эти автомобили недоступны на YDDY.") { Label = "→→→" };
                        MenuController.AddSubmenu(addonCarsMenu, unavailableCars);

                        for (var cat = 0; cat < 22; cat++)
                        {
                            Menu categoryMenu = new Menu(GetLocalizedName($"VEH_CLASS_{cat}"), "Заспавнить");
                            MenuItem categoryBtn = new MenuItem(GetLocalizedName($"VEH_CLASS_{cat}"), $"Заспавнить автомобиль из класса {GetLocalizedName($"VEH_CLASS_{cat}")}.") { Label = "→→→" };

                            addonCarsMenu.AddMenuItem(categoryBtn);

                            if (!allowedCategories[cat])
                            {
                                categoryBtn.Description = "Этот класс отключен на сервере.";
                                categoryBtn.Enabled = false;
                                categoryBtn.LeftIcon = MenuItem.Icon.LOCK;
                                categoryBtn.Label = "";
                                continue;
                            }

                            // Loop through all addon vehicles in this class.
                            foreach (KeyValuePair<string, uint> veh in AddonVehicles.Where(v => GetVehicleClassFromName(v.Value) == cat))
                            {
                                string localizedName = GetLabelText(GetDisplayNameFromVehicleModel(veh.Value));

                                string name = localizedName != "NULL" ? localizedName : GetDisplayNameFromVehicleModel(veh.Value);
                                name = name != "CARNOTFOUND" ? name : veh.Key;

                                MenuItem carBtn = new MenuItem(name, $"Нажмите, чтобы заспавнить {name}.")
                                {
                                    Label = $"({veh.Key.ToString()})",
                                    ItemData = veh.Key // store the model name in the button data.
                                };

                                // This should be impossible to be false, but we check it anyway.
                                if (IsModelInCdimage(veh.Value))
                                {
                                    categoryMenu.AddMenuItem(carBtn);
                                }
                                else
                                {
                                    carBtn.Enabled = false;
                                    carBtn.Description = "Этот автомобиль недоступен.";
                                    carBtn.LeftIcon = MenuItem.Icon.LOCK;
                                    unavailableCars.AddMenuItem(carBtn);
                                }
                            }

                            //if (AddonVehicles.Count(av => GetVehicleClassFromName(av.Value) == cat && IsModelInCdimage(av.Value)) > 0)
                            if (categoryMenu.Size > 0)
                            {
                                MenuController.AddSubmenu(addonCarsMenu, categoryMenu);
                                MenuController.BindMenuItem(addonCarsMenu, categoryMenu, categoryBtn);

                                categoryMenu.OnItemSelect += (sender, item, index) =>
                                {
                                    SpawnVehicle(item.ItemData.ToString(), SpawnInVehicle, ReplaceVehicle);
                                };
                            }
                            else
                            {
                                categoryBtn.Description = "В этой категории нет автомобилей.";
                                categoryBtn.Enabled = false;
                                categoryBtn.LeftIcon = MenuItem.Icon.LOCK;
                                categoryBtn.Label = "";
                            }
                        }

                        if (unavailableCars.Size > 0)
                        {
                            addonCarsMenu.AddMenuItem(unavailableCarsBtn);
                            MenuController.BindMenuItem(addonCarsMenu, unavailableCars, unavailableCarsBtn);
                        }

                        //addonCarsMenu.OnItemSelect += (sender, item, index) =>
                        //{

                        //    //SpawnVehicle(AddonVehicles.ElementAt(index).Key, SpawnInVehicle, ReplaceVehicle);
                        //};
                    }
                    else
                    {
                        addonCarsBtn.Enabled = false;
                        addonCarsBtn.LeftIcon = MenuItem.Icon.LOCK;
                        addonCarsBtn.Description = "На сервере нет аддонов.";
                    }
                }
                else
                {
                    addonCarsBtn.Enabled = false;
                    addonCarsBtn.LeftIcon = MenuItem.Icon.LOCK;
                    addonCarsBtn.Description = "Ну удалось загрузить список всех аддонов.";
                }
            }
            else
            {
                addonCarsBtn.Enabled = false;
                addonCarsBtn.LeftIcon = MenuItem.Icon.LOCK;
                addonCarsBtn.Description = "Доступ к этому списку был ограничен администратором сервера.";
            }
            #endregion

            #region vehicle classes submenus
            // Loop through all the vehicle classes.
            for (var vehClass = 0; vehClass < 22; vehClass++)
            {
                // Get the class name.
                string className = GetLocalizedName($"VEH_CLASS_{vehClass.ToString()}");

                // Create a button & a menu for it, add the menu to the menu pool and add & bind the button to the menu.
                MenuItem btn = new MenuItem(className, $"Заспавнить автомобиль из класса ~o~{className} ~s~.")
                {
                    Label = "→→→"
                };

                Menu vehicleClassMenu = new Menu(className, "Заспавнить");

                MenuController.AddSubmenu(menu, vehicleClassMenu);
                menu.AddMenuItem(btn);

                if (allowedCategories[vehClass])
                {
                    MenuController.BindMenuItem(menu, vehicleClassMenu, btn);
                }
                else
                {
                    btn.LeftIcon = MenuItem.Icon.LOCK;
                    btn.Description = "Эта категория была выключена админмстратором сервера.";
                    btn.Enabled = false;
                }

                // Create a dictionary for the duplicate vehicle names (in this vehicle class).
                var duplicateVehNames = new Dictionary<string, int>();

                #region Add vehicles per class
                // Loop through all the vehicles in the vehicle class.
                foreach (var veh in VehicleData.Vehicles.VehicleClasses[className])
                {
                    // Convert the model name to start with a Capital letter, converting the other characters to lowercase. 
                    var properCasedModelName = veh[0].ToString().ToUpper() + veh.ToLower().Substring(1);

                    // Get the localized vehicle name, if it's "NULL" (no label found) then use the "properCasedModelName" created above.
                    var vehName = GetVehDisplayNameFromModel(veh) != "NULL" ? GetVehDisplayNameFromModel(veh) : properCasedModelName;

                    // Loop through all the menu items and check each item's title/text and see if it matches the current vehicle (display) name.
                    var duplicate = false;
                    for (var itemIndex = 0; itemIndex < vehicleClassMenu.Size; itemIndex++)
                    {
                        // If it matches...
                        if (vehicleClassMenu.GetMenuItems()[itemIndex].Text.ToString() == vehName)
                        {

                            // Check if the model was marked as duplicate before.
                            if (duplicateVehNames.Keys.Contains(vehName))
                            {
                                // If so, add 1 to the duplicate counter for this model name.
                                duplicateVehNames[vehName]++;
                            }

                            // If this is the first duplicate, then set it to 2.
                            else
                            {
                                duplicateVehNames[vehName] = 2;
                            }

                            // The model name is a duplicate, so get the modelname and add the duplicate amount for this model name to the end of the vehicle name.
                            vehName += $" ({duplicateVehNames[vehName].ToString()})";

                            // Then create and add a new button for this vehicle.

                            if (DoesModelExist(veh))
                            {
                                var vehBtn = new MenuItem(vehName) { Enabled = true };
                                vehicleClassMenu.AddMenuItem(vehBtn);
                            }
                            else
                            {
                                var vehBtn = new MenuItem(vehName, "Этот автомобиль недоступен, так как модель не найдена в файлах Вашей игры.") { Enabled = false };
                                vehicleClassMenu.AddMenuItem(vehBtn);
                                vehBtn.RightIcon = MenuItem.Icon.LOCK;
                            }

                            // Mark duplicate as true and break from the loop because we already found the duplicate.
                            duplicate = true;
                            break;
                        }
                    }

                    // If it's not a duplicate, add the model name.
                    if (!duplicate)
                    {
                        if (DoesModelExist(veh))
                        {
                            var vehBtn = new MenuItem(vehName) { Enabled = true };
                            vehicleClassMenu.AddMenuItem(vehBtn);
                        }
                        else
                        {
                            var vehBtn = new MenuItem(vehName, "Этот автомобиль недоступен, так как модель не найдена в файлах Вашей игры.") { Enabled = false };
                            vehicleClassMenu.AddMenuItem(vehBtn);
                            vehBtn.RightIcon = MenuItem.Icon.LOCK;
                        }
                    }
                }
                #endregion

                // Handle button presses
                vehicleClassMenu.OnItemSelect += (sender2, item2, index2) =>
                {
                    SpawnVehicle(VehicleData.Vehicles.VehicleClasses[className][index2], SpawnInVehicle, ReplaceVehicle);
                };
            }
            #endregion

            #region handle events
            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == spawnByName)
                {
                    // Passing "custom" as the vehicle name, will ask the user for input.
                    SpawnVehicle("custom", SpawnInVehicle, ReplaceVehicle);
                }
            };

            // Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == spawnInVeh)
                {
                    SpawnInVehicle = _checked;
                }
                else if (item == replacePrev)
                {
                    ReplaceVehicle = _checked;
                }
            };
            #endregion
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
    }
}
