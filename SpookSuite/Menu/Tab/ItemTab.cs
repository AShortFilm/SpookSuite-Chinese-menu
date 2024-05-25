using UnityEngine;
using SpookSuite.Menu.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using SpookSuite.Util;

namespace SpookSuite.Menu.Tab
{
    internal class ItemTab : MenuTab
    {
        public ItemTab() : base("物品") { }

        private Vector2 scrollPos = Vector2.zero;
        private string searchText = "";
        private int amount = 1;

        private bool equipOnSpawn = false;
        private bool droneDelivery = false;

        public override void Draw()
        {
            GUILayout.BeginVertical();
            MenuContent();
            GUILayout.EndVertical();
        }

        private void MenuContent()
        {
            GUILayout.BeginHorizontal();

            UI.Textbox("搜索", ref searchText);
            GUILayout.FlexibleSpace();
            UI.Checkbox("面前生成", ref equipOnSpawn);
            GUILayout.FlexibleSpace();
            UI.Checkbox("无人机交付", ref droneDelivery);
            GUILayout.FlexibleSpace();
            UI.Textbox<int>("数量:", ref amount, false);
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            List<Item> items = ItemDatabase.Instance.Objects.ToList().OrderBy(x => String.IsNullOrEmpty(x.displayName) ? x.name : x.displayName).ToList();

            int gridWidth = 4;
            int btnWidth = (int)(SpookSuiteMenu.Instance.contentWidth - (SpookSuiteMenu.Instance.spaceFromLeft * 2)) / gridWidth;
            UI.ButtonGrid<Item>(items, item => item.GetName(), searchText, item => GameUtil.SpawnItem(item.id, equipOnSpawn, droneDelivery, amount), gridWidth, btnWidth);

            GUILayout.EndScrollView();
        }
    }
}