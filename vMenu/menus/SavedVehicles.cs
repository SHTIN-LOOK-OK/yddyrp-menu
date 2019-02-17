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
    public class SavedVehicles
    {
        // Variables
        private Menu menu;
        private Menu selectedVehicleMenu = new Menu("Управление", "Сохраненные автомобили");
        private Menu unavailableVehiclesMenu = new Menu("Недоступные", "Недоступные сохраненные автомобили");
        private Dictionary<string, VehicleInfo> savedVehicles = new Dictionary<string, VehicleInfo>();
        private List<Menu> subMenus = new List<Menu>();
        private Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>> svMenuItems = new Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>>();
        private KeyValuePair<string, VehicleInfo> currentlySelectedVehicle = new KeyValuePair<string, VehicleInfo>();
        private int deleteButtonPressedCount = 0;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            string menuTitle = "YDDY:RP";
            #region Create menus and submenus
            // Create the menu.
            menu = new Menu(menuTitle, "Меню сохраненных автомобилей");

            MenuItem saveVehicle = new MenuItem("Сохранить текущий автомобиль", "");
            menu.AddMenuItem(saveVehicle);
            saveVehicle.LeftIcon = MenuItem.Icon.CAR;

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == saveVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        SaveVehicle();
                    }
                    else
                    {
                        Notify.Error("На данный момент Вы не находитесь в транспортном средстве. Зайдите в него, прежде чем пытаться его сохранить.");
                    }
                }
            };


            for (int i = 0; i < 22; i++)
            {
                Menu categoryMenu = new Menu("Сохраненные", GetLabelText($"VEH_CLASS_{i}"));

                MenuItem categoryButton = new MenuItem(GetLabelText($"VEH_CLASS_{i}"), $"Все сохраненные автомобили категории {(GetLabelText($"VEH_CLASS_{i}"))}.");
                subMenus.Add(categoryMenu);
                MenuController.AddSubmenu(menu, categoryMenu);
                menu.AddMenuItem(categoryButton);
                categoryButton.Label = "→→→";
                MenuController.BindMenuItem(menu, categoryMenu, categoryButton);

                categoryMenu.OnMenuClose += (sender) =>
                {
                    UpdateMenuAvailableCategories();
                };

                categoryMenu.OnItemSelect += (sender, item, index) =>
                {
                    UpdateSelectedVehicleMenu(item, sender);
                };
            }

            MenuItem unavailableModels = new MenuItem("Недоступные сохраненные автомобили", "")
            {
                Label = "→→→"
            };

            menu.AddMenuItem(unavailableModels);
            MenuController.BindMenuItem(menu, unavailableVehiclesMenu, unavailableModels);
            MenuController.AddSubmenu(menu, unavailableVehiclesMenu);


            MenuController.AddMenu(selectedVehicleMenu);
            MenuItem spawnVehicle = new MenuItem("Заспавнить", "");
            MenuItem renameVehicle = new MenuItem("Переименовать", "");
            MenuItem replaceVehicle = new MenuItem("~r~Заменить текущим", "");
            MenuItem deleteVehicle = new MenuItem("~r~Удалить", "");
            selectedVehicleMenu.AddMenuItem(spawnVehicle);
            selectedVehicleMenu.AddMenuItem(renameVehicle);
            selectedVehicleMenu.AddMenuItem(replaceVehicle);
            selectedVehicleMenu.AddMenuItem(deleteVehicle);

            selectedVehicleMenu.OnMenuClose += (sender) =>
            {
                selectedVehicleMenu.RefreshIndex();
                deleteButtonPressedCount = 0;
                deleteVehicle.Label = "";
            };

            selectedVehicleMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == spawnVehicle)
                {
                    if (MainMenu.VehicleSpawnerMenu != null)
                    {
                        SpawnVehicle(currentlySelectedVehicle.Value.model, MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                    else
                    {
                        SpawnVehicle(currentlySelectedVehicle.Value.model, true, true, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                }
                else if (item == renameVehicle)
                {
                    string newName = await GetUserInput(windowTitle: "Введите имя для этого автомобиля.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName))
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                    }
                    else
                    {
                        if (StorageManager.SaveVehicleInfo("veh_" + newName, currentlySelectedVehicle.Value, false))
                        {
                            DeleteResourceKvp(currentlySelectedVehicle.Key);
                            while (!selectedVehicleMenu.Visible)
                            {
                                await BaseScript.Delay(0);
                            }
                            Notify.Success("Ваш автомобиль был переименован.");
                            UpdateMenuAvailableCategories();
                            selectedVehicleMenu.GoBack();
                            currentlySelectedVehicle = new KeyValuePair<string, VehicleInfo>(); // clear the old info
                        }
                        else
                        {
                            Notify.Error("Это имя автомобиля уже используется.");
                        }
                    }
                }
                else if (item == replaceVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        SaveVehicle(currentlySelectedVehicle.Key.Substring(4));
                        selectedVehicleMenu.GoBack();
                        Notify.Success("Вы успешно заменили сохраненный автомобиль.");
                    }
                    else
                    {
                        Notify.Error("Вы должны находится в автомобиле, прежде чем Вы сможете его заменить.");
                    }
                }
                else if (item == deleteVehicle)
                {
                    if (deleteButtonPressedCount == 0)
                    {
                        deleteButtonPressedCount = 1;
                        item.Label = "Нажмите повторно для подтверждения.";
                        Notify.Alert("Вы уверены, что хотите удалить этот автомобиль? Нажмите кнопку снова для подтверждения");
                    }
                    else
                    {
                        deleteButtonPressedCount = 0;
                        item.Label = "";
                        DeleteResourceKvp(currentlySelectedVehicle.Key);
                        UpdateMenuAvailableCategories();
                        selectedVehicleMenu.GoBack();
                        Notify.Success("Ваш сохранненый автомобиль был удален.");
                    }
                }
                if (item != deleteVehicle) // if any other button is pressed, restore the delete vehicle button pressed count.
                {
                    deleteButtonPressedCount = 0;
                    deleteVehicle.Label = "";
                }
            };

            #endregion
        }


        /// <summary>
        /// Updates the selected vehicle.
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns>A bool, true if successfull, false if unsuccessfull</returns>
        private bool UpdateSelectedVehicleMenu(MenuItem selectedItem, Menu parentMenu = null)
        {
            if (!svMenuItems.ContainsKey(selectedItem))
            {
                Notify.Error("In some very strange way, you've managed to select a button, that does not exist according to this list. So your vehicle could not be loaded. :( Maybe your save files are broken?");
                return false;
            }
            var vehInfo = svMenuItems[selectedItem];
            selectedVehicleMenu.MenuSubtitle = $"{vehInfo.Key.Substring(4)} ({vehInfo.Value.name})";
            currentlySelectedVehicle = vehInfo;
            MenuController.CloseAllMenus();
            selectedVehicleMenu.OpenMenu();
            if (parentMenu != null)
            {
                MenuController.AddSubmenu(parentMenu, selectedVehicleMenu);
            }
            return true;
        }


        /// <summary>
        /// Updates the available vehicle category list.
        /// </summary>
        public void UpdateMenuAvailableCategories()
        {
            savedVehicles = GetSavedVehicles();
            svMenuItems = new Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>>();

            for (int i = 1; i < GetMenu().Size - 1; i++)
            {
                if (savedVehicles.Any(a => GetVehicleClassFromName(a.Value.model) == i - 1 && IsModelInCdimage(a.Value.model)))
                {
                    GetMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.NONE;
                    GetMenu().GetMenuItems()[i].Label = "→→→";
                    GetMenu().GetMenuItems()[i].Enabled = true;
                    GetMenu().GetMenuItems()[i].Description = $"Все сохраненные автомобилей из категории {GetMenu().GetMenuItems()[i].Text} .";
                }
                else
                {
                    GetMenu().GetMenuItems()[i].Label = "";
                    GetMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.LOCK;
                    GetMenu().GetMenuItems()[i].Enabled = false;
                    GetMenu().GetMenuItems()[i].Description = $"У вас нет сохраненных автомобилей из категории {GetMenu().GetMenuItems()[i].Text} .";
                }
            }

            // Check if the items count will be changed. If there are less cars than there were before, one probably got deleted
            // so in that case we need to refresh the index of that menu just to be safe. If not, keep the index where it is for improved
            // usability of the menu.
            foreach (Menu m in subMenus)
            {
                int size = m.Size;
                int vclass = subMenus.IndexOf(m);

                int count = savedVehicles.Count(a => GetVehicleClassFromName(a.Value.model) == vclass);
                if (count < size)
                {
                    m.RefreshIndex();
                }
            }

            foreach (Menu m in subMenus)
            {
                // Clear items but don't reset the index because we can guarantee that the index won't be out of bounds.
                // this is the case because of the loop above where we reset the index if the items count changes.
                m.ClearMenuItems(true);
            }

            // Always clear this index because it's useless anyway and it's safer.
            unavailableVehiclesMenu.ClearMenuItems();

            foreach (var sv in savedVehicles)
            {
                if (IsModelInCdimage(sv.Value.model))
                {
                    int vclass = GetVehicleClassFromName(sv.Value.model);
                    Menu menu = subMenus[vclass];

                    MenuItem savedVehicleBtn = new MenuItem(sv.Key.Substring(4), $"Управление над сохраненными автомобилями.")
                    {
                        Label = $"({sv.Value.name}) →→→"
                    };
                    menu.AddMenuItem(savedVehicleBtn);

                    svMenuItems.Add(savedVehicleBtn, sv);
                }
                else
                {
                    MenuItem missingVehItem = new MenuItem(sv.Key.Substring(4), "Данный автомобиль не удалось найти в файлах игры.")
                    {
                        Label = "(" + sv.Value.name + ")",
                        Enabled = false,
                        LeftIcon = MenuItem.Icon.LOCK
                    };
                    unavailableVehiclesMenu.AddMenuItem(missingVehItem);
                }
            }
            foreach (Menu m in subMenus)
            {
                m.SortMenuItems((MenuItem A, MenuItem B) =>
                {
                    return A.Text.ToLower().CompareTo(B.Text.ToLower());
                });
            }
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
