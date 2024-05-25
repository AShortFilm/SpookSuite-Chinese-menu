using Photon.Pun;
using SpookSuite.Cheats;
using SpookSuite.Cheats.Core;
using SpookSuite.Menu.Core;
using System;
using UnityEngine;

namespace SpookSuite.Menu.Tab
{
    internal class SelfTab : MenuTab
    {
        public SelfTab() : base("自己") { }

        private Vector2 scrollPos = Vector2.zero;
        private float faceSize = .035f;
        private string faceText = "SS";
        private float faceRotation = 0;

        public override void Draw()
        {   
            GUILayout.BeginVertical(GUILayout.Width(SpookSuiteMenu.Instance.contentWidth * 0.5f - SpookSuiteMenu.Instance.spaceFromLeft));
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            MenuContent();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(SpookSuiteMenu.Instance.contentWidth * 0.5f - SpookSuiteMenu.Instance.spaceFromLeft));
            Toggles();
            GUILayout.EndVertical();
        }
        
        private void MenuContent()
        {
            GUILayout.BeginHorizontal();
            UI.Button("自杀", () => Player.localPlayer.refs.view.RPC("RPCA_PlayerDie", RpcTarget.All, Array.Empty<object>()), null);
            UI.Button("复活", () => Player.localPlayer.refs.view.RPC("RPCA_PlayerRevive", RpcTarget.All, Array.Empty<object>()), null);
            GUILayout.EndHorizontal();

            UI.Header("和某人交谈");
            GUILayout.BeginHorizontal();
            UI.Button("每个人", NetworkVoiceHandler.ListenAndSendToAll, null);
            UI.Button("活着的", NetworkVoiceHandler.TalkToAlive, null);
            UI.Button("死去的", NetworkVoiceHandler.TalkToDead, null);
            GUILayout.EndHorizontal();

            UI.Textbox("假名称", ref NameSpoof.Value, length: 100, onChanged: NameSpoof.OnValueChanged);
            UI.Checkbox("使用假名称", Cheat.Instance<NameSpoof>());

            UI.HorizontalSpace("面部", () =>
            {
                UI.ExecuteSlider("旋转", faceRotation.ToString(), () =>
                {
                    if (!Player.localPlayer.refs.visor)
                        return;

                    PlayerVisor v = Player.localPlayer.refs.visor;
                    v.SetAllFaceSettings(v.hue.Value, v.visorColorIndex, v.visorFaceText.text, faceRotation, v.FaceSize); //doesnt check limit
                }, ref faceRotation, 0, 360);
            });

            UI.HorizontalSpace(null, () =>
            {
                UI.Textbox("文本", ref faceText, false, 3);
                UI.Button("Set", () => Player.localPlayer.refs.view.RPC("RPCA_SetVisorText", RpcTarget.All, faceText));
            });

            UI.HorizontalSpace(null, () =>
            {
                UI.ExecuteSlider("大小", faceSize.ToString(), () =>
                {
                    if (!Player.localPlayer.refs.visor)
                        return;

                    PlayerVisor v = Player.localPlayer.refs.visor;
                    v.SetAllFaceSettings(v.hue.Value, v.visorColorIndex, v.visorFaceText.text, v.FaceRotation, faceSize); //doesnt check limit
                }, ref faceSize, .001f, 1f);
                UI.Button("重置", () =>
                {
                    faceSize = .035f;
                    if (!Player.localPlayer.refs.visor)
                        return;

                    PlayerVisor v = Player.localPlayer.refs.visor;
                    v.SetAllFaceSettings(v.hue.Value, v.visorColorIndex, v.visorFaceText.text, v.FaceRotation, 0.035f);
                });
            });
        }

        private void Toggles()
        {
            UI.CheatToggleSlider(Cheat.Instance<SuperSpeed>(), "超高速", SuperSpeed.Value.ToString("#"), ref SuperSpeed.Value, 10f, 100f);
            UI.CheatToggleSlider(Cheat.Instance<SuperJump>(), "超级跳跃", SuperJump.Value.ToString("#.#"), ref SuperJump.Value, 0.6f, 20f);
            UI.CheatToggleSlider(Cheat.Instance<NoClip>(), "穿墙", NoClip.Value.ToString(), ref NoClip.Value, 1f, 20f);
            UI.CheatToggleSlider(Cheat.Instance<RainbowFace>(), "彩虹脸", RainbowFace.Value.ToString(), ref RainbowFace.Value, 0.1f, 30f);
            UI.CheatToggleSlider(Cheat.Instance<RollingFace>(), "面部旋转", RollingFace.Value.ToString(), ref RollingFace.Value, 0.1f, 30f);
            UI.CheatToggleSlider(Cheat.Instance<Spinbot>(), "陀螺", Spinbot.Value.ToString(), ref Spinbot.Value, 1f, 23f);

            UI.Checkbox("无敌模式", Cheat.Instance<Godmode>());
            UI.Checkbox("隐形", Cheat.Instance<Invisibility>());
            UI.Checkbox("无限跳跃", Cheat.Instance<InfiniteJump>());
            UI.Checkbox("不摔倒", Cheat.Instance<NoRagdoll>());
            UI.Checkbox("无限制的氧气", Cheat.Instance<UnlimitedOxygen>());
            UI.Checkbox("无限耐力", Cheat.Instance<UnlimitedStamina>());
            UI.Checkbox("无限制电池", Cheat.Instance<UnlimitedBattery>());
            UI.Checkbox("无限录制", Cheat.Instance<UnlimitedFilm>());
        }
    }
}