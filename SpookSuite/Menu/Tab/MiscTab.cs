using Photon.Pun;
using SpookSuite.Cheats.Core;
using SpookSuite.Cheats;
using SpookSuite.Menu.Core;
using SpookSuite.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SpookSuite.Manager;
using Steamworks;

namespace SpookSuite.Menu.Tab
{
    internal class MiscTab : MenuTab
    {
        public MiscTab() : base("杂项") { }

        private Vector2 scrollPos = Vector2.zero;
        private Vector2 scrollPos2 = Vector2.zero;
        private string searchText = "";
        private string moneyToSet = "0";
        private string metaToSet = "0";

        public override void Draw()
        {
            GUILayout.BeginVertical(GUILayout.Width(SpookSuiteMenu.Instance.contentWidth * 0.5f - SpookSuiteMenu.Instance.spaceFromLeft));
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            MenuContent();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(SpookSuiteMenu.Instance.contentWidth * 0.5f - SpookSuiteMenu.Instance.spaceFromLeft));
            HelmetTextContent();
            GUILayout.EndVertical();
        }

        public static float x, y, w, h;

        private void MenuContent()
        {
            UI.Button("下一天", GameUtil.AdvanceDay);
            UI.TextboxAction("钱", ref moneyToSet, 10,
                new UIButton("添加", () => { int.TryParse(moneyToSet, out int o); GameUtil.SendHospitalBill(-o); }),
                new UIButton("去除", () => { int.TryParse(moneyToSet, out int o); GameUtil.SendHospitalBill(o); })
            );
            UI.TextboxAction("MC (倍数50)", ref metaToSet, 10,
                new UIButton("添加", () =>
                {
                    if (int.TryParse(metaToSet, out int o))
                        SurfaceNetworkHandler.Instance.photonView.RPC("RPCA_OnNewWeek", RpcTarget.All, o);
                })
            );

            UI.Button("刷新帽子商店", () => HatShop.instance.Reflect().GetValue<PhotonView>("view").RPC("RPCA_StockShop", RpcTarget.All, Guid.NewGuid().GetHashCode()));
            UI.Button("获取播放量/钱", () =>
            {
                UnityEngine.Object.FindObjectOfType<ExtractVideoMachine>().Reflect()
                .GetValue<PhotonView>("m_photonView").RPC("RPC_ExtractDone", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, false);
            });
            UI.Button("打开/关闭潜水钟", () => GameObjectManager.divingBell.SetDoorStateInstant(!GameObjectManager.divingBell.opened));
            UI.Button("激活潜水钟", Cheat.Instance<UseDivingBell>().Execute);
            UI.Button("解锁岛屿设施", () => { GameObjectManager.unlocks.ForEach(u => { if (u.locked) UnityEngine.Object.FindObjectOfType<IslandUnlocks>().Reflect().GetValue<PhotonView>("view").RPC("RPCA_Activate", RpcTarget.All, new int[] { u.Reflect().Invoke<int>("GetID") }); }); });
            UI.Checkbox("防拾取", Cheat.Instance<AntiPickup>());
            UI.Checkbox("使用Mod加入大厅", Cheat.Instance<JoinWithPlugins>());
            UI.Checkbox("反生成（自动从其他玩家身上移除生成的物品）", Cheat.Instance<AntiSpawner>());
            //UI.Checkbox("Hear Push To Talk Players Always", Cheat.Instance<NoPushToTalk>()); is broken since update?
        }

        private void HelmetTextContent()
        {
            UI.Header("发送信息");
            UI.Textbox("搜索", ref searchText);

            List<LocalizationKeys.Keys> keys = Enum.GetValues(typeof(LocalizationKeys.Keys)).Cast<LocalizationKeys.Keys>().ToList().Where(key => key.ToString().ToLower().Contains(searchText.ToLower())).ToList();

            int gridWidth = 3;
            int btnWidth = (int)(SpookSuiteMenu.Instance.contentWidth * 0.5f - SpookSuiteMenu.Instance.spaceFromLeft) / gridWidth;
            scrollPos2 = GUILayout.BeginScrollView(scrollPos2);
            UI.ButtonGrid(keys, key => key.ToString(), "", key => sendHelmetText(key), gridWidth, btnWidth);
            GUILayout.EndScrollView();
        }

        private void sendHelmetText(LocalizationKeys.Keys key)
        {
            SurfaceNetworkHandler.Instance.Reflect().GetValue<PhotonView>("m_View").RPC("RPCA_HelmetText", RpcTarget.All, (int)key, (object)3);
        }
    }
}
