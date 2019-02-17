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
    public class OnlinePlayers
    {
        public List<int> PlayersWaypointList = new List<int>();

        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        Menu playerMenu = new Menu("Игроки онлайн", "Онлайн:");
        Player currentPlayer = new Player(Game.Player.Handle);


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Игроки онлайн") { };
            menu.CounterPreText = "Игроки: ";

            MenuController.AddSubmenu(menu, playerMenu);

            MenuItem teleport = new MenuItem("Телепортировать игрока", "Телепортироваться к этому игроку.");
            MenuItem teleportVeh = new MenuItem("Телепортироватся в автомобиль игрока", "Телепортация игрока в автомобиль.");
            MenuItem summon = new MenuItem("Вызвать игрока", "Телепортировать игрока к вам.");
            MenuItem toggleGPS = new MenuItem("Переключение GPS", "Включить или отключить маршрут GPS на радаре данному игроку.");
            MenuItem spectate = new MenuItem("Наблюдать за игроком", "Наблюдать за этим игроком. Нажмите эту кнопку еще раз, чтобы остановить просмотр.");
            MenuItem printIdentifiers = new MenuItem("Печать идентификаторов", "При этом идентификаторы плеера будут выведены на клиентскую консоль (F8). А также сохранит его в CitizenFX.log file.");
            MenuItem kill = new MenuItem("~r~Убить Игрока", "Убейте этого игрока, обратите внимание, что они получат уведомление о том, что вы убили их. Он также будет зарегистрирован в Actions log.");
            MenuItem kick = new MenuItem("~r~Кик игрока", "Кик игрока с сервера.");
            MenuItem ban = new MenuItem("~r~Забанить игрока навсегда", "Навсегда заблокировать этого игрока на сервере.Вы уверен, что хотите это сделать? Вы можете указать причину после нажатия этой кнопки.");
            MenuItem tempban = new MenuItem("~r~Временно забанить игрока", "Дать этому игроку бан до 30 дней (максимум). Вы можете указать продолжительность и причину бана после нажатия этой кнопки.");

            if (IsAllowed(Permission.OPTeleport))
            {
                playerMenu.AddMenuItem(teleport);
                playerMenu.AddMenuItem(teleportVeh);
            }
            if (IsAllowed(Permission.OPSummon))
            {
                playerMenu.AddMenuItem(summon);
            }
            if (IsAllowed(Permission.OPSpectate))
            {
                playerMenu.AddMenuItem(spectate);
            }
            if (IsAllowed(Permission.OPWaypoint))
            {
                playerMenu.AddMenuItem(toggleGPS);
            }
            if (IsAllowed(Permission.OPIdentifiers))
            {
                playerMenu.AddMenuItem(printIdentifiers);
            }
            if (IsAllowed(Permission.OPKill))
            {
                playerMenu.AddMenuItem(kill);
            }
            if (IsAllowed(Permission.OPKick))
            {
                playerMenu.AddMenuItem(kick);
            }
            if (IsAllowed(Permission.OPTempBan))
            {
                playerMenu.AddMenuItem(tempban);
            }
            if (IsAllowed(Permission.OPPermBan))
            {
                playerMenu.AddMenuItem(ban);
                ban.LeftIcon = MenuItem.Icon.WARNING;
            }

            playerMenu.OnMenuClose += (sender) =>
            {
                playerMenu.RefreshIndex();
                //playerMenu.UpdateScaleform();
                ban.Label = "";
            };

            playerMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                ban.Label = "";
            };

            // handle button presses for the specific player's menu.
            playerMenu.OnItemSelect += (sender, item, index) =>
            {
                // teleport (in vehicle) button
                if (item == teleport || item == teleportVeh)
                {
                    if (Game.Player.Handle != currentPlayer.Handle)
                        TeleportToPlayer(currentPlayer.Handle, item == teleportVeh); // teleport to the player. optionally in the player's vehicle if that button was pressed.
                    else
                        Notify.Error("Вы не можете телепортироваться к себе!");
                }
                // summon button
                else if (item == summon)
                {
                    if (Game.Player.Handle != currentPlayer.Handle)
                        SummonPlayer(currentPlayer);
                    else
                        Notify.Error("Вы не можете призвать себя.");
                }
                // spectating
                else if (item == spectate)
                {
                    SpectatePlayer(currentPlayer);
                }
                // kill button
                else if (item == kill)
                {
                    KillPlayer(currentPlayer);
                }
                // manage the gps route being clicked.
                else if (item == toggleGPS)
                {
                    bool selectedPedRouteAlreadyActive = false;
                    if (PlayersWaypointList.Count > 0)
                    {
                        if (PlayersWaypointList.Contains(currentPlayer.Handle))
                        {
                            selectedPedRouteAlreadyActive = true;
                        }
                        foreach (int playerId in PlayersWaypointList)
                        {
                            int playerPed = GetPlayerPed(playerId);
                            if (DoesEntityExist(playerPed) && DoesBlipExist(GetBlipFromEntity(playerPed)))
                            {
                                int oldBlip = GetBlipFromEntity(playerPed);
                                SetBlipRoute(oldBlip, false);
                                RemoveBlip(ref oldBlip);
                                Notify.Custom($"~g~GPS route to ~s~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~ is now disabled.");
                            }
                        }
                        PlayersWaypointList.Clear();
                    }

                    if (!selectedPedRouteAlreadyActive)
                    {
                        if (currentPlayer.Handle != Game.Player.Handle)
                        {
                            int ped = GetPlayerPed(currentPlayer.Handle);
                            int blip = GetBlipFromEntity(ped);
                            if (DoesBlipExist(blip))
                            {
                                SetBlipColour(blip, 58);
                                SetBlipRouteColour(blip, 58);
                                SetBlipRoute(blip, true);
                            }
                            else
                            {
                                blip = AddBlipForEntity(ped);
                                SetBlipColour(blip, 58);
                                SetBlipRouteColour(blip, 58);
                                SetBlipRoute(blip, true);
                            }
                            PlayersWaypointList.Add(currentPlayer.Handle);
                            Notify.Custom($"~g~GPS route to ~s~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~ is now active, press the ~s~Toggle GPS Route~g~ button again to disable the route.");
                        }
                        else
                        {
                            Notify.Error("Вы не можете установить путевую точку для себя.");
                        }
                    }
                }
                else if (item == printIdentifiers)
                {
                    Func<string, string> CallbackFunction = (data) =>
                    {
                        Debug.WriteLine(data);
                        string ids = "~s~";
                        foreach (string s in JsonConvert.DeserializeObject<string[]>(data))
                        {
                            ids += "~n~" + s;
                        }
                        Notify.Custom($"~y~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~'s Identifiers: {ids}", false);
                        return data;
                    };
                    BaseScript.TriggerServerEvent("vMenu:GetPlayerIdentifiers", currentPlayer.ServerId, CallbackFunction);
                }
                // kick button
                else if (item == kick)
                {
                    if (currentPlayer.Handle != Game.Player.Handle)
                        KickPlayer(currentPlayer, true);
                    else
                        Notify.Error("Вы не можете кикнуть самого себя!");
                }
                // temp ban
                else if (item == tempban)
                {
                    BanPlayer(currentPlayer, false);
                }
                // perm ban
                else if (item == ban)
                {
                    if (ban.Label == "Вы уверены?")
                    {
                        ban.Label = "";
                        UpdatePlayerlist();
                        playerMenu.GoBack();
                        BanPlayer(currentPlayer, true);
                    }
                    else
                    {
                        ban.Label = "Вы уверены?";
                    }
                }
            };

            // handle button presses in the player list.
            menu.OnItemSelect += (sender, item, index) =>
                {
                    if (MainMenu.PlayersList.ToList().Any(p => p.ServerId.ToString() == item.Label.Replace(" →→→", "").Replace("Server #", "")))
                    {
                        currentPlayer = MainMenu.PlayersList.ToList().Find(p => p.ServerId.ToString() == item.Label.Replace(" →→→", "").Replace("Server #", ""));
                        playerMenu.MenuSubtitle = $"~s~Player: ~y~{GetSafePlayerName(currentPlayer.Name)}";
                        playerMenu.CounterPreText = $"[Server ID: ~y~{currentPlayer.ServerId}~s~] ";
                    }
                    else
                    {
                        playerMenu.GoBack();
                    }
                };
        }

        /// <summary>
        /// Updates the player items.
        /// </summary>
        public void UpdatePlayerlist()
        {
            menu.ClearMenuItems();

            foreach (Player p in MainMenu.PlayersList)
            {
                MenuItem pItem = new MenuItem($"{GetSafePlayerName(p.Name)}", $"Click to view the options for this player. Server ID: {p.ServerId}. Local ID: {p.Handle}.")
                {
                    Label = $"Server #{p.ServerId} →→→"
                };
                menu.AddMenuItem(pItem);
                MenuController.BindMenuItem(menu, playerMenu, pItem);
            }

            menu.RefreshIndex();
            //menu.UpdateScaleform();
            playerMenu.RefreshIndex();
            //playerMenu.UpdateScaleform();
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Online Players Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
                return menu;
            }
            else
            {
                UpdatePlayerlist();
                return menu;
            }
        }
    }
}
