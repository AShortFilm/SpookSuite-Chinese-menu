using SpookSuite.Cheats;
using SpookSuite.Cheats.Core;
using SpookSuite.Handler;
using SpookSuite.Manager;
using SpookSuite.Menu.Core;
using SpookSuite.Util;
using System.Linq;
using UnityEngine;

namespace SpookSuite.Menu.Tab
{
    internal class VisualTab : MenuTab
    {
        public VisualTab() : base("视觉") { }
        private Vector2 scrollPos = Vector2.zero;

        public override void Draw()
        {
            GUILayout.BeginVertical(GUILayout.Width(SpookSuiteMenu.Instance.contentWidth * 0.5f - SpookSuiteMenu.Instance.spaceFromLeft));
            VisualContent();
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(SpookSuiteMenu.Instance.contentWidth * 0.5f - SpookSuiteMenu.Instance.spaceFromLeft));
            ESPContent();
            GUILayout.EndVertical();
        }

        private void VisualContent()
        {
            UI.CheatToggleSlider(Cheat.Instance<FOV>(), "FOV", Cheats.FOV.Value.ToString(), ref Cheats.FOV.Value, 1, 170);
            UI.CheatToggleSlider(Cheat.Instance<ThirdPerson>(), "第三人称", Cheats.ThirdPerson.Value.ToString(), ref Cheats.ThirdPerson.Value, 0, 20);
            UI.Checkbox("禁用Visor", Cheat.Instance<PlayerVisorToggle>());
            UI.Checkbox("显示死亡", Cheat.Instance<DisplayDead>());
            UI.Checkbox("显示名称", Cheat.Instance<Nameplates>());
        }
        public static bool rainbow;
        private void ESPContent()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            
            UI.Checkbox("启用ESP", Cheat.Instance<ESP>());
            UI.Button("启用所有ESP", () => ESP.ToggleAll());
            UI.Checkbox("显示玩家", ref ESP.displayPlayers);
            UI.Checkbox("显示怪物", ref ESP.displayEnemies);
            UI.Checkbox("显示物品", ref ESP.displayItems);
            UI.Checkbox("显示激光", ref ESP.displayLasers);
            UI.Checkbox("显示潜水钟", ref ESP.displayDivingBell);

            UI.SubHeader("渲染");
            UI.CheatToggleSlider(Cheat.Instance<ChamESP>(), "启用渲染", $"最小距离: {ChamESP.Value.ToString("#")}", ref ChamESP.Value, 0, 170);
            UI.ToggleSlider("彩虹模式", ChamESP.Speed.ToString(), ref ChamESP.rainbowMode, ref ChamESP.Speed, 0.1f, 30f);
            UI.Slider("透明度", ChamESP.opacity.ToString(), ref ChamESP.opacity, 0, 1);
            UI.Button("启用所有渲染", () => ChamESP.ToggleAll());
            UI.Checkbox("显示玩家", ref ChamESP.displayPlayers);
            UI.Checkbox("显示怪物", ref ChamESP.displayEnemies);
            UI.Checkbox("显示物品", ref ChamESP.displayItems);
            UI.Checkbox("显示激光", ref ChamESP.displayLasers);
            UI.Checkbox("显示潜水钟", ref ChamESP.displayDivingBell);

            GUILayout.EndScrollView();
        }
    }
}
