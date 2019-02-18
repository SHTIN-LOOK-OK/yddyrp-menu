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
    public class PersonalVehicle
    {
        // Variables
        private Menu menu;
        public bool EnableVehicleBlip { get; private set; } = UserDefaults.PVEnableVehicleBlip;

        // Empty constructor
        public PersonalVehicle() { }

        public Vehicle CurrentPersonalVehicle { get; internal set; } = null;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Menu
            menu = new Menu(GetSafePlayerName(Game.Player.Name), "Параметры автомобиля");

            // menu items
            MenuItem setVehice = new MenuItem("Сохранить автомобиль", "Сохранить текущий автомобиль, как персональный. Если у вас уже сохранен персональный автомобиль, он будет перезаписан на текущий.") { Label = "Current Vehicle: None" };
            MenuItem toggleEngine = new MenuItem("Включить двигатель", "Включить или выключить двигатель автомобиля, даже если вы не находитесь в нем. Данная функция не работает, если кто то использует этот автомобиль.");
            MenuListItem toggleLights = new MenuListItem("Включить фары", new List<string>() { "Включить", "Выключить", "Сбросить" }, 0, "Это включит или выключит фары на вашем автомобиле, если двигатель запущен.");
            MenuItem kickAllPassengers = new MenuItem("Выкинуть пассажира", "Это выбросит пассажира из вашего автомобиля.");
            //MenuItem
            MenuItem lockDoors = new MenuItem("Закрыть автомобиль", "Это закроет ваш автомобиль для всех игроков. Все внутри смогут выйти из закрытого автомобиля.");
            MenuItem unlockDoors = new MenuItem("Открыть автомобиль", "Это откроет ваш автомобиль для всех игроков.");
            MenuItem soundHorn = new MenuItem("Сигнал", "Активирует сигнал на автомобиле.");
            MenuItem toggleAlarm = new MenuItem("Включить сигнализацию", "Включает или выключает сигнализацию на вашем автомобиле.");
            MenuCheckboxItem enableBlip = new MenuCheckboxItem("Добавить метку для Вашего авто", "Добавляет или удаляет метку для Вашего автомобиля на карте.", EnableVehicleBlip) { Style = MenuCheckboxItem.CheckboxStyle.Cross };
            MenuCheckboxItem exclusiveDriver = new MenuCheckboxItem("Персональный автомобиль", "Если вы активируете эту функцию, никто кроме вас не сможет сесть в автомобиль в качестве водителя.", false) { Style = MenuCheckboxItem.CheckboxStyle.Cross };

            // This is always allowed if this submenu is created/allowed.
            menu.AddMenuItem(setVehice);

            // Add conditional features.

            // Toggle engine.
            if (IsAllowed(Permission.PVToggleEngine))
            {
                menu.AddMenuItem(toggleEngine);
            }

            // Toggle lights
            if (IsAllowed(Permission.PVToggleLights))
            {
                menu.AddMenuItem(toggleLights);
            }

            // Kick vehicle passengers
            if (IsAllowed(Permission.PVKickPassengers))
            {
                menu.AddMenuItem(kickAllPassengers);
            }

            // Lock and unlock vehicle doors
            if (IsAllowed(Permission.PVLockDoors))
            {
                menu.AddMenuItem(lockDoors);
                menu.AddMenuItem(unlockDoors);
            }

            // Sound horn
            if (IsAllowed(Permission.PVSoundHorn))
            {
                menu.AddMenuItem(soundHorn);
            }

            // Toggle alarm sound
            if (IsAllowed(Permission.PVToggleAlarm))
            {
                menu.AddMenuItem(toggleAlarm);
            }

            // Enable blip for personal vehicle
            if (IsAllowed(Permission.PVAddBlip))
            {
                menu.AddMenuItem(enableBlip);
            }

            if (IsAllowed(Permission.PVExclusiveDriver))
            {
                menu.AddMenuItem(exclusiveDriver);
            }


            // Handle list presses
            menu.OnListItemSelect += (sender, item, itemIndex, index) =>
            {
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists())
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("Вы не можете сейчас управлять своим автомобилем. Убедитесь что в вашем автомобиле никого нету.");
                            return;
                        }
                    }

                    if (item == toggleLights)
                    {
                        if (itemIndex == 0)
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 3);
                        }
                        else if (itemIndex == 1)
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 1);
                        }
                        else
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 0);
                        }
                    }
                }
                else
                {
                    Notify.Error("У вас не выбран персональный автомобиль или он удален. Сохраните персональный автомобиль перед использованием.");
                }
            };

            // Handle checkbox changes
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == enableBlip)
                {
                    EnableVehicleBlip = _checked;
                    if (EnableVehicleBlip)
                    {
                        if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                        {
                            if (CurrentPersonalVehicle.AttachedBlip == null || !CurrentPersonalVehicle.AttachedBlip.Exists())
                            {
                                CurrentPersonalVehicle.AttachBlip();
                            }
                            CurrentPersonalVehicle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                            CurrentPersonalVehicle.AttachedBlip.Name = "Персональный автомобиль";
                        }
                        else
                        {
                            Notify.Error("У вас не выбран персональный автомобиль или он удален. Сохраните персональный автомобиль перед использованием.");
                        }

                    }
                    else
                    {
                        if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists() && CurrentPersonalVehicle.AttachedBlip != null && CurrentPersonalVehicle.AttachedBlip.Exists())
                        {
                            CurrentPersonalVehicle.AttachedBlip.Delete();
                        }
                    }
                }
                else if (item == exclusiveDriver)
                {
                    if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                    {
                        if (NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            if (_checked)
                            {
                                SetVehicleExclusiveDriver(CurrentPersonalVehicle.Handle, Game.PlayerPed.Handle);
                                SetVehicleExclusiveDriver_2(CurrentPersonalVehicle.Handle, Game.PlayerPed.Handle, 1);
                            }
                            else
                            {
                                SetVehicleExclusiveDriver(CurrentPersonalVehicle.Handle, 0);
                                SetVehicleExclusiveDriver_2(CurrentPersonalVehicle.Handle, 0, 1);
                            }
                        }
                        else
                        {
                            item.Checked = !_checked;
                            Notify.Error("Вы не можете сейчас управлять своим автомобилем. Убедитесь что в вашем автомобиле никого нету.");
                        }
                    }
                }
            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == setVehice)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        if (veh != null && veh.Exists())
                        {
                            if (Game.PlayerPed == veh.Driver)
                            {
                                CurrentPersonalVehicle = veh;
                                veh.PreviouslyOwnedByPlayer = true;
                                veh.IsPersistent = true;
                                if (EnableVehicleBlip && IsAllowed(Permission.PVAddBlip))
                                {
                                    if (veh.AttachedBlip == null || !veh.AttachedBlip.Exists())
                                    {
                                        veh.AttachBlip();
                                    }
                                    veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    veh.AttachedBlip.Name = "Персональный автомобиль";
                                }
                                var name = GetLabelText(veh.DisplayName);
                                if (string.IsNullOrEmpty(name) || name.ToLower() == "null")
                                {
                                    name = veh.DisplayName;
                                }
                                item.Label = $"Текущий автомобиль: {name}";
                            }
                            else
                            {
                                Notify.Error(CommonErrors.NeedToBeTheDriver);
                            }
                        }
                        else
                        {
                            Notify.Error(CommonErrors.NoVehicle);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                else if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                {
                    if (item == kickAllPassengers)
                    {
                        if (CurrentPersonalVehicle.Occupants.Count() > 0 && CurrentPersonalVehicle.Occupants.Any(p => p != Game.PlayerPed))
                        {
                            var netId = VehToNet(CurrentPersonalVehicle.Handle);
                            TriggerServerEvent("vMenu:GetOutOfCar", netId, Game.Player.ServerId);
                        }
                        else
                        {
                            Notify.Info("Никого нет в вашем автомобиле, кого можно выкинуть.");
                        }
                    }
                    else
                    {
                        if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                            {
                                Notify.Error("Вы не можете сейчас управлять своим автомобилем. Убедитесь что в вашем автомобиле никого нету.");
                                return;
                            }
                        }

                        if (item == toggleEngine)
                        {
                            SetVehicleEngineOn(CurrentPersonalVehicle.Handle, !CurrentPersonalVehicle.IsEngineRunning, true, true);
                        }

                        else if (item == lockDoors || item == unlockDoors)
                        {
                            bool _lock = item == lockDoors;
                            LockOrUnlockDoors(CurrentPersonalVehicle, _lock);
                        }

                        else if (item == soundHorn)
                        {
                            SoundHorn(CurrentPersonalVehicle);
                        }

                        else if (item == toggleAlarm)
                        {
                            ToggleVehicleAlarm(CurrentPersonalVehicle);
                        }
                    }
                }
                else
                {
                    Notify.Error("У вас не выбран персональный автомобиль или он удален. Сохраните персональный автомобиль перед использованием.");
                }
            };
        }

        private async void SoundHorn(Vehicle veh)
        {
            if (veh != null && veh.Exists())
            {
                int timer = GetGameTimer();
                while (GetGameTimer() - timer < 1000)
                {
                    SoundVehicleHornThisFrame(veh.Handle);
                    await Delay(0);
                }
            }
        }

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
