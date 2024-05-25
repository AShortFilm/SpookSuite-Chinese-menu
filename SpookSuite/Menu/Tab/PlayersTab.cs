using Photon.Pun;
using Photon.Realtime;
using SpookSuite.Cheats;
using SpookSuite.Cheats.Core;
using SpookSuite.Handler;
using SpookSuite.Manager;
using SpookSuite.Menu.Core;
using SpookSuite.Util;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;
using Zorro.Core;
using Steamworks;
using System.Drawing;
using ExitGames.Client.Photon;
using Photon.Voice.Unity;

namespace SpookSuite.Menu.Tab
{
    internal class PlayersTab : MenuTab
    {
        public PlayersTab() : base("玩家") { }

        private Vector2 scrollPos = Vector2.zero;
        private Vector2 scrollPos2 = Vector2.zero;
        public static Player selectedPlayer = null;
        public static string faceText = "SS";
        public static string faceColor = "000000";
        public int num;

        public override void Draw()
        {
            GUILayout.BeginVertical(GUILayout.Width(SpookSuiteMenu.Instance.contentWidth * 0.3f - SpookSuiteMenu.Instance.spaceFromLeft));
            PlayersList();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(SpookSuiteMenu.Instance.contentWidth * 0.7f - SpookSuiteMenu.Instance.spaceFromLeft));
            scrollPos2 = GUILayout.BeginScrollView(scrollPos2);
            GeneralActions();
            PlayerActions();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void GeneralActions()
        {
            if (!PhotonNetwork.InRoom) return;

            UI.Header("所有玩家");
            UI.Button("全部踢出", () => Cheat.Instance<KickAll>().Execute());
            UI.Button("全部杀死", () => Cheat.Instance<KillAll>().Execute());
            UI.Button("全部复活", () => Cheat.Instance<ReviveAll>().Execute());
            UI.Button("将其他人加入领域", () => Limbo.AddPlayers(GameObjectManager.players.Where(p => !p.IsLocal).ToList()));
            UI.HorizontalSpace(null, () =>
            {
                UI.Textbox("炸弹", ref BombAll.Value, false, 3); //max 999 otherwise too laggy
                UI.Button("全部轰炸", () => Cheat.Instance<BombAll>().Execute(), null);
            });
            UI.CheatToggleSlider(Cheat.Instance<SuperSpeedOthers>(), "超高速", SuperSpeedOthers.Value.ToString(), ref SuperSpeedOthers.Value, 1, 6);
            UI.Checkbox("反转其他人移动按键", Cheat.Instance<ReverseOthers>());

            if (Player.localPlayer.Handle().IsDev())
            {
                UI.Header("仅限操控非SpookSuite玩家");
                UI.Checkbox("冻结其他人", Cheat.Instance<FreezeAll>());
            }

            UI.CheatToggleSlider(Cheat.Instance<OthersFly>(), "给予飞行", OthersFly.Value.ToString(), ref OthersFly.Value, 1, 30);
        }

        private void PlayerActions()
        {
            if (selectedPlayer is null) return;

            if (selectedPlayer.Handle().IsSpookUser() && Player.localPlayer.Handle().IsDev())
            {
                UI.Header("SpookSuite Specialty");
                //add things that we could do to our users for fun, maybe disabling something in their menu?
                UI.Button("WASSUP", () => { });           
            }     

            UI.Header("选择的玩家操作");

            GUILayout.TextArea("SteamID: " + (selectedPlayer.Handle().IsDev() ? 0 : selectedPlayer.GetSteamID().m_SteamID));
            UI.Button("打开主页", () => System.Diagnostics.Process.Start("https://steamcommunity.com/profiles/" + selectedPlayer.GetSteamID().m_SteamID));
            UI.Label("SpookSuite用户", selectedPlayer.Handle().IsSpookUser().ToString());

            if (!Player.localPlayer.Handle().IsDev() && selectedPlayer.Handle().IsDev())
            {
                UI.Label("用户是开发者，所以你什么都做不了 :(");
                return;
            }

            if (!selectedPlayer.IsLocal)
                UI.Button("阻止RPC", () => selectedPlayer.Handle().ToggleRPCBlock(), selectedPlayer.Handle().IsRPCBlocked() ? "UnBlock" : "Block");

            UI.Button("传送", () => { Player.localPlayer.Reflect().Invoke("Teleport", selectedPlayer.refs.cameraPos.position, new Vector3(0, 0, 0)); }, "Teleport");
            UI.TextboxAction("面部文字", ref faceText, 3, new UIButton("Set", () => selectedPlayer.Handle().RPC("RPCA_SetVisorText", RpcTarget.All, faceText)));
            UI.TextboxAction("面部颜色", ref faceColor, 8,
                new UIButton("Set", () =>
                {
                    while (faceColor.Length < 6) faceColor += "0";
                    selectedPlayer.refs.visor.ApplyVisorColor(new RGBAColor(faceColor).GetColor());
                }
            ));
            UI.Button("测试", () => PhotonNetwork.RaiseEvent(20, selectedPlayer.photonView.Owner.ActorNumber, RaiseEventOptions.Default, SendOptions.SendReliable));
            UI.Button("生成炸弹", () => GameUtil.SpawnItem(GameUtil.GetItemByName("bomb").id, selectedPlayer.refs.cameraPos.position), "Bomb");
            UI.Button("冻结", () => selectedPlayer.Reflect().Invoke("CallSlowFor", 0f, 4f), "Freeze");
            
            UI.Button("杀死", () => selectedPlayer.Reflect().Invoke("CallDie"), "Kill");
            UI.Button("复活", () => selectedPlayer.CallRevive(), "Revive");

            UI.Button("摔倒", () => selectedPlayer.Reflect().Invoke("CallTakeDamageAndAddForceAndFall", 0f, Vector3.zero, 2f), "Ragdoll");
            UI.Button("肘飞", () => selectedPlayer.Reflect().Invoke("CallTakeDamageAndAddForceAndFall", 0f, selectedPlayer.refs.cameraPos.up * 100, 0f), "Launch");
            UI.Button("电击", () => selectedPlayer.Reflect().Invoke("CallTakeDamageAndTase", 1f, 5f));

            UI.Button("强制坐下", () => { Sittable s = GameObjectManager.sittables.GetRandom(); selectedPlayer.refs.view.RPC("RPCA_Sit", RpcTarget.All, s.Reflect().GetValue<PhotonView>("view").ViewID, s.Reflect().GetValue<int>("seatID")); });
            UI.Button("治愈", () => selectedPlayer.Handle().RPC("RPCA_Heal", RpcTarget.All, 100f));

            UI.Button("踢出", () => { SurfaceNetworkHandler.Instance.photonView.RPC("RPC_LoadScene", selectedPlayer.PhotonPlayer(), "NewMainMenu"); });
            UI.Button("传送暗影领域", () => ShadowRealmHandler.instance.TeleportPlayerToRandomRealm(selectedPlayer));
            UI.HorizontalSpace("领域", () => 
            {
                UI.Button("添加到领域", () => Limbo.limboList.Add(selectedPlayer)); 
                UI.Button("移出领域", () => Limbo.limboList.Remove(selectedPlayer));
            });


            UI.Header("帽子", true);
            UI.Button("卸下帽子", () => selectedPlayer.refs.view.RPC("RPCA_EquipHat", RpcTarget.All, -1));
            UI.ButtonGrid<Hat>(HatDatabase.instance.hats.ToList(), h => h.GetName(), "", h => selectedPlayer.refs.view.RPC("RPCA_EquipHat", RpcTarget.All, HatDatabase.instance.GetIndexOfHat(h)), 3);
        }

        private void PlayersList()
        {
            float width = SpookSuiteMenu.Instance.contentWidth * 0.3f - SpookSuiteMenu.Instance.spaceFromLeft * 2;
            float height = SpookSuiteMenu.Instance.contentHeight - 20;

            Rect rect = new Rect(0, 0, width, height);
            GUI.Box(rect, "玩家列表");

            GUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));

            GUILayout.Space(25);
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            foreach (Player player in GameObjectManager.players)
            {
                if (!player.IsValid()) continue;
                if (selectedPlayer is null) selectedPlayer = player;
                if (player.Handle().IsSpookUser()) GUI.contentColor = Settings.c_primary.GetColor();
                if (selectedPlayer.GetInstanceID() == player.GetInstanceID()) GUI.contentColor = Settings.c_espPlayers.GetColor();
                if (GUILayout.Button(player.refs.view.Owner.NickName, GUI.skin.label)) selectedPlayer = player;

                GUI.contentColor = Settings.c_menuText.GetColor();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
