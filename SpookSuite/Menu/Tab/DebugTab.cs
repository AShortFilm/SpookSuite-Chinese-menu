using Photon.Pun;
using SpookSuite.Menu.Core;
using SpookSuite.Util;
using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using Zorro.Core;
using SpookSuite.Manager;
using SpookSuite.Handler;
using SpookSuite.Components;
using SpookSuite.Cheats;
using SpookSuite.Cheats.Core;

namespace SpookSuite.Menu.Tab
{
    internal class DebugTab : MenuTab
    {
        public DebugTab() : base("调试", true) { }
        private ulong steamLobbyId = 0;
        private int steamLobbyIndex = 0;
        private Vector2 scrollPos = Vector2.zero;
        private CallResult<LobbyMatchList_t> matchList;
        public static bool logPlayerPrefs = false;
        public override void Draw()
        {
            GUILayout.BeginVertical();
            MenuContent();
            GUILayout.EndVertical();          
        }
        public float x = 1600, y = 10, w = 300, h = 100;
        public void RectEdit(string title, ref Rect rect)
        {
            UI.HorizontalSpace(title, () =>
            {
                UI.Textbox("X", ref x, false);
                UI.Textbox("Y", ref y, false);
                UI.Textbox("W", ref w, false);
                UI.Textbox("H", ref h, false);
            });
            Notifications.defaultRect = new Rect (x, y, w, h);
        }

        private void MenuContent()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            UI.Header("调试信息");

            if (PhotonNetwork.InRoom)
            {
                UI.Label("是主机", PhotonNetwork.IsMasterClient ? "Yes" : "No");
                UI.Label("主机昵称", PhotonNetwork.MasterClient.NickName);
            }

            UI.Header("大厅工具");
            UI.Button("主机公用", () => MainMenuHandler.Instance.SilentHost());
            UI.Button("主机私用", () => MainMenuHandler.Instance.Host(1));
            UI.Button("设置大厅公用", () => SetPublic(true));
            UI.Button("设置大厅私用", () => SetPublic(false));
            UI.Button("设置大厅可连接", () => SetJoinable(true));
            UI.Button("设置大厅不可连接", () => SetJoinable(false));

            UI.Header("通知内容");
            UI.Checkbox("通知", Cheat.Instance<Notifications>());
            RectEdit("默认矩形", ref Notifications.defaultRect);
            UI.Textbox("间距", ref Notifications.spacing, false);
            UI.Textbox("宽度", ref Notifications.width, false);
            UI.Textbox("高度", ref Notifications.height, false);
            UI.Button("测试信息", () => { Notifications.PushNotifcation(new Notifcation("Title", "Info", NotificationType.Info)); });
            UI.Button("测试警告", () => { Notifications.PushNotifcation(new Notifcation("Title", "Warning", NotificationType.Warning)); });
            UI.Button("测试错误", () => { Notifications.PushNotifcation(new Notifcation("Title", "Error", NotificationType.Error)); });
            UI.Button("测试开发", () => { Notifications.PushNotifcation(new Notifcation("Title", "Dev", NotificationType.Dev)); });
            UI.Button("清除所有", () => { Log.Info($"Cleared {Notifications.notifcations.Count}"); Notifications.notifcations.Clear(); });

            UI.Header("场景工具");
            UI.Button("加载工厂", () => LoadScene("FactoryScene"));
            UI.Button("加载海港", () => LoadScene("HarbourScene"));
            UI.Button("装载矿山", () => LoadScene("MinesScene"));
            UI.Button("加载岛屿", () => LoadScene("SurfaceScene"));
         
            UI.Header("调试作弊");

            UI.Checkbox("记录玩家参数", ref logPlayerPrefs);
            UI.Button("记录RPCS", () => { foreach (string s in PhotonNetwork.PhotonServerSettings.RpcList) Debug.Log(s); });

            UI.Button("加载主菜单", () => LoadScene("NewMainMenu"));
            UI.Button("切换到主菜单", () => SpookPageUI.TransitionToPage<MainMenuMainPage>());
            
            UI.Button("传送所有物品", () => {
                GameObjectManager.pickups.ForEach(x => GameUtil.TeleportItem(x));
            });
            
            UI.Button("使用潜水钟dontcare", () => GameObjectManager.divingBellButton.Interact(Player.localPlayer));
            UI.Button("使用地下潜水钟", () => GameObjectManager.divingBell.GoUnderground());
            UI.Button("使用地上潜水钟", () => GameObjectManager.divingBell.GoToSurface());
            CSteamID id = MainMenuHandler.SteamLobbyHandler.Reflect().GetValue<CSteamID>("m_CurrentLobby");

            UI.Button("获取大厅数据", () => {

                int count = SteamMatchmaking.GetLobbyDataCount(id);
                //GetLobbyDataByIndex(CSteamID steamIDLobby, int iLobbyData, out string pchKey, int cchKeyBufferSize, out string pchValue, int cchValueBufferSize)
                
                Debug.Log($"大厅ID: {id}");

                for (int i = 0; i < count; i++)
                {
                    SteamMatchmaking.GetLobbyDataByIndex(id, i, out string key, 265, out string value, 265);
                    Debug.Log($"Key: {key} Value: {value}");
                }
                
                Debug.Log($"steam://joinlobby/2881650/{id}/{PhotonNetwork.MasterClient.GetSteamID()}");
            });
            if(id.IsValid())
                GUILayout.TextArea($"steam://joinlobby/2881650/{id}/{PhotonNetwork.MasterClient.GetSteamID()}");

            UI.Button("大厅列表", () => {

                Debug.Log("Fetching Lobby List...");

                //SteamMatchmaking.AddRequestLobbyListStringFilter("ContentWarningVersion", new BuildVersion(Application.version).ToMatchmaking(), ELobbyComparison.k_ELobbyComparisonEqual);
                //SteamMatchmaking.AddRequestLobbyListStringFilter("Plugins", GameHandler.GetPluginHash(), ELobbyComparison.k_ELobbyComparisonEqual);

                //SteamMatchmaking.AddRequestLobbyListStringFilter("PrivateMatch", "true", ELobbyComparison.k_ELobbyComparisonEqual);
                //SteamMatchmaking.AddRequestLobbyListResultCountFilter(50);

                matchList = CallResult<LobbyMatchList_t>.Create(new CallResult<LobbyMatchList_t>.APIDispatchDelegate(MatchListReceived));

                matchList.Set(SteamMatchmaking.RequestLobbyList());
            }, "Get Public");

            

            UI.TextboxAction<ulong>("加入大厅", ref steamLobbyId, 200, new UIButton("OK", () => {
                //this.JoinLobby(SteamMatchmaking.GetLobbyByIndex(array.GetRandom<(CSteamID, int)>().Item2));
                MainMenuHandler.SteamLobbyHandler.Reflect().Invoke("JoinLobby", new CSteamID(steamLobbyId));
            }));

            UI.TextboxAction<int>("加入大厅ILOBBY", ref steamLobbyIndex, 3, new UIButton("OK", () => {
                //this.JoinLobby(SteamMatchmaking.GetLobbyByIndex(array.GetRandom<(CSteamID, int)>().Item2));
                MainMenuHandler.SteamLobbyHandler.Reflect().Invoke("JoinLobby", SteamMatchmaking.GetLobbyByIndex(steamLobbyIndex));
            }));

            GUILayout.EndScrollView();
        }

        internal static bool SetPublic(bool value)
        {
            CSteamID id = MainMenuHandler.SteamLobbyHandler.Reflect().GetValue<CSteamID>("m_CurrentLobby");
            MainMenuHandler.SteamLobbyHandler.Reflect().Invoke("SetLobbyType", id, value ? ELobbyType.k_ELobbyTypePublic : ELobbyType.k_ELobbyTypeFriendsOnly);
            return true;
        }

        internal static bool SetJoinable(bool value)
        {
            CSteamID id = MainMenuHandler.SteamLobbyHandler.Reflect().GetValue<CSteamID>("m_CurrentLobby");
            if (PhotonNetwork.CurrentRoom != null)
            {
                SteamMatchmaking.SetLobbyJoinable(id, value);
                PhotonNetwork.CurrentRoom.IsOpen = value;
                PhotonNetwork.CurrentRoom.IsVisible = value;
            }
            return true;
        }

        internal void MatchListReceived(LobbyMatchList_t param, bool biofailure)
        {
            if (biofailure)
                Debug.LogError((object)"Matchlist Biofail");
            else if (param.m_nLobbiesMatching == 0U)
            {
                Debug.Log((object)"Found No Matches hosting");
                //UnityEngine.Object.FindObjectOfType<MainMenuHandler>().SilentHost();
            }
            else
            {
                List<(CSteamID, int)> array = new List<(CSteamID, int)>();
                for (int iLobby = 0; (long)iLobby < (long)param.m_nLobbiesMatching; ++iLobby)
                {
                    CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(iLobby);
                    string lobbyData1 = SteamMatchmaking.GetLobbyData(lobbyByIndex, "ContentWarningVersion");
                    string lobbyData2 = SteamMatchmaking.GetLobbyData(lobbyByIndex, "PhotonRegion");
                    string isPrivate = SteamMatchmaking.GetLobbyData(lobbyByIndex, "PrivateMatch");
                    int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyByIndex);
                    int lobbyMemberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyByIndex);                

                    CSteamID owner = SteamMatchmaking.GetLobbyOwner(lobbyByIndex);
                    Debug.Log($"LobbyID: {lobbyByIndex.ToString()} ILobby: {iLobby} Players: {numLobbyMembers.ToString()}/{lobbyMemberLimit.ToString()} Private: {isPrivate}");

                    string cloudRegion = PhotonNetwork.CloudRegion;
                    bool flag = !string.IsNullOrEmpty(lobbyData2) && lobbyData2 == cloudRegion;
                    if (lobbyData1 == new BuildVersion(Application.version).ToMatchmaking() & flag && numLobbyMembers < 4)
                        array.Add((lobbyByIndex, iLobby));
                }
                Debug.Log((object)("Received SteamLobby Matchlist: " + param.m_nLobbiesMatching.ToString() + " Matching: " + array.Count.ToString()));
            }

        }

        private void LoadScene(string name) => SurfaceNetworkHandler.Instance.photonView.RPC("RPC_LoadScene", RpcTarget.All, (object)name);
    }
}
