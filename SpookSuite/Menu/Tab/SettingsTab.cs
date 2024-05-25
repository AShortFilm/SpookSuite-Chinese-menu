using SpookSuite.Cheats;
using SpookSuite.Cheats.Core;
using SpookSuite.Menu.Core;
using SpookSuite.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpookSuite.Menu.Tab
{
    internal class SettingsTab : MenuTab
    {
        public SettingsTab() : base("设置") { }

        private Vector2 scrollPos = Vector2.zero;
        private float f_leftWidth;
        private string search = "";
        private int themesselect = 0;
        private string s_Primary = Settings.c_primary.GetHexCode();
        private string s_Text = Settings.c_menuText.GetHexCode();
        private string s_PlayerChams = Settings.c_chamPlayers.GetHexCode();
        private string s_ItemChams = Settings.c_chamItems.GetHexCode();
        private string s_MonsterChams = Settings.c_chamMonsters.GetHexCode();
        private string s_DivingBellCham = Settings.c_chamDivingBell.GetHexCode();
        private string s_PlayerEsp = Settings.c_espPlayers.GetHexCode();
        private string s_ItemEsp = Settings.c_espItems.GetHexCode();
        private string s_MonsterEsp = Settings.c_espMonsters.GetHexCode();
        private string s_DivingBellEsp = Settings.c_espDivingBells.GetHexCode();
        private bool dropdown_makesound = false;
        private bool dropdown_dronespawn = false;
        private bool dropdown_speedmanipulation = false;
        private bool dropdown_kick = false;
        private bool dropdown_shadowrealm = false;
        private bool dropdown_crash = false;
        private bool dropdown_blackscreen = false;

        public override void Draw()
        {
            f_leftWidth = SpookSuiteMenu.Instance.contentWidth * 0.55f - SpookSuiteMenu.Instance.spaceFromLeft;

            GUILayout.BeginVertical(GUILayout.Width(f_leftWidth));         
            MenuContent();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(SpookSuiteMenu.Instance.contentWidth * 0.45f - SpookSuiteMenu.Instance.spaceFromLeft));
            KeybindContent();
            GUILayout.EndVertical();
        }

        private void MenuContent()
        {
            UI.Actions(
                new UIButton("重置设置", () => Settings.Config.RegenerateConfig()),
                new UIButton("保存设置", () => Settings.Config.SaveConfig()),
                new UIButton("加载设置", () => Settings.Config.LoadConfig())
            );
            UI.Checkbox("通知", Cheat.Instance<Notifications>());
            UI.NumSelect("字体大小", ref Settings.i_menuFontSize, 5, 30);
            UI.Slider("菜单透明度", Settings.f_menuAlpha.ToString("0.00"), ref Settings.f_menuAlpha, 0.1f, 1f);
            UI.Button("调整菜单大小", () => MenuUtil.BeginResizeMenu(), "Resize");
            UI.Button("重置菜单", () => SpookSuiteMenu.Instance.ResetMenuSize(), "Reset");

            UI.Select("主题", ref themesselect,
                new UIOption("默认", () => ThemeUtil.ApplyTheme("Default")),
                new UIOption("绿色", () => ThemeUtil.ApplyTheme("Green")),
                new UIOption("蓝色", () => ThemeUtil.ApplyTheme("Blue"))
            );

            UI.Header("菜单颜色");
            UI.TextboxAction("主要", ref s_Primary, 8,
                new UIButton("Set", () => SetColor(ref Settings.c_primary, s_Primary))
            );
            UI.TextboxAction("文本", ref s_Text, 8,
                new UIButton("Set", () => SetColor(ref Settings.c_menuText, s_Text))
            );

            UI.Header("Esp颜色");
            UI.TextboxAction("物品", ref s_ItemEsp, 8,
                new UIButton("Set", () => SetColor(ref Settings.c_espItems, s_ItemEsp))
            );
            UI.TextboxAction("玩家", ref s_PlayerEsp, 8,
                new UIButton("Set", () => SetColor(ref Settings.c_espPlayers, s_PlayerEsp))
            );
            UI.TextboxAction("怪物", ref s_MonsterEsp, 8,
                new UIButton("Set", () => SetColor(ref Settings.c_espMonsters, s_MonsterEsp))
            );
            UI.TextboxAction("潜水钟", ref s_DivingBellEsp, 8,
                new UIButton("Set", () => SetColor(ref Settings.c_espDivingBells, s_DivingBellEsp))
            );

            UI.Header("渲染颜色");
            UI.TextboxAction("物品", ref s_ItemChams, 8,
                new UIButton("Set", () => SetColor(ref Settings.c_chamItems, s_ItemChams))
            );
            UI.TextboxAction("玩家", ref s_PlayerChams, 8,
                new UIButton("Set", () => SetColor(ref Settings.c_chamPlayers, s_PlayerChams))
            );
            UI.TextboxAction("怪物", ref s_MonsterChams, 8,
                new UIButton("Set", () => SetColor(ref Settings.c_chamMonsters, s_MonsterChams))
            );
            UI.TextboxAction("潜水钟", ref s_DivingBellCham, 8,
                new UIButton("Set", () => SetColor(ref Settings.c_chamDivingBell, s_DivingBellCham))
            );

            UI.Header("反应");
            UI.Checkbox("切换", Cheat.Instance<RPCReactions>());
            UI.Checkbox("反应时通知", ref RPCReactions.Value);

            ReactionSetter("声音垃圾", ref Settings.reaction_makesound, ref dropdown_makesound);
            ReactionSetter("无人机生成", ref Settings.reaction_dronespawn, ref dropdown_dronespawn);
            ReactionSetter("速度操纵", ref Settings.reaction_speedmanipulation, ref dropdown_speedmanipulation);
            ReactionSetter("踢出", ref Settings.reaction_kick, ref dropdown_kick);
            ReactionSetter("崩溃", ref Settings.reaction_crash, ref dropdown_crash);
            ReactionSetter("暗影领域", ref Settings.reaction_shadowrealm, ref dropdown_shadowrealm);
            ReactionSetter("黑屏", ref Settings.reaction_blackscreen, ref dropdown_blackscreen);
        }

        private void ReactionSetter(string label, ref RPCReactions.reactionType reaction, ref bool drop)
        {
            RPCReactions.reactionType r = reaction;

            UI.Dropdown(label, ref drop,
                new UIButton("无", () => r = RPCReactions.reactionType.none, r == RPCReactions.reactionType.none ? new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold} : null),
                new UIButton("踢出", () => r = RPCReactions.reactionType.kick, r == RPCReactions.reactionType.kick ? new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold } : null),
                new UIButton("断开连接", () => r = RPCReactions.reactionType.disconnect, r == RPCReactions.reactionType.disconnect ? new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold } : null),
                new UIButton("戏弄", () => r = RPCReactions.reactionType.clownem, r == RPCReactions.reactionType.clownem ? new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold } : null),
                new UIButton("传送领域", () => r = RPCReactions.reactionType.shadowrealm, r == RPCReactions.reactionType.shadowrealm ? new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold } : null)

                );

            reaction = r;
        }

        private void SetColor(ref RGBAColor color, string hexCode)
        {
            while (hexCode.Length < 6) hexCode += "0";
            color = new RGBAColor(hexCode);
            Settings.Config.SaveConfig();
        }

        private void KeybindContent()
        {
            UI.Header("快捷键");
            GUILayout.BeginVertical();

            UI.Textbox("搜索", ref search, big: false);
           
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            List<Cheat> cheats = Cheat.instances.FindAll(c => !c.Hidden);
            foreach (Cheat cheat in cheats)
            {
                //if (!hack.CanHaveKeyBind()) continue;

                if (!cheat.GetType().Name.ToLower().Contains(search.ToLower()))
                    continue;

                GUILayout.BeginHorizontal();

                KeyCode bind = cheat.keybind;

                string kb = cheat.HasKeybind ? bind.ToString() : "无";

                GUILayout.Label(cheat.GetType().Name);
                GUILayout.FlexibleSpace();

                //if (cheat.HasKeybind && hack != Hack.OpenMenu && hack != Hack.UnlockDoorAction && GUILayout.Button("-")) hack.RemoveKeyBind();

                string btnText = cheat.WaitingForKeybind ? "Waiting" : kb;               
                if (GUILayout.Button(btnText, GUILayout.Width(85)))
                {
                    GUI.FocusControl(null);
                    KBUtil.BeginChangeKeybind(cheat);
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
