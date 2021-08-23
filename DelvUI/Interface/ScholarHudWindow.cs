using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class ScholarHudWindow : HudWindow
    {
        public override uint JobId => 28;

        protected int FairyBarHeight => PluginConfiguration.FairyBarHeight;
        protected int FairyBarWidth => PluginConfiguration.FairyBarWidth;
        protected int FairyBarX => PluginConfiguration.FairyBarX;
        protected int FairyBarY => PluginConfiguration.FairyBarY;
        protected int SchAetherBarHeight => PluginConfiguration.SchAetherBarHeight;
        protected int SchAetherBarWidth => PluginConfiguration.SchAetherBarWidth;
        protected int SchAetherBarX => PluginConfiguration.SchAetherBarX;
        protected int SchAetherBarY => PluginConfiguration.SchAetherBarY;
        protected int SchAetherBarPad => PluginConfiguration.SchAetherBarPad;

        protected Dictionary<string, uint> SchAetherColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000];
        protected Dictionary<string, uint> SchFairyColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000 + 1];
        protected Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000 + 2];

        protected new Vector2 BarSize { get; private set; }

        protected Vector2 BarCoords { get; private set; }

        public ScholarHudWindow(
            ClientState clientState,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable, 
            PluginConfiguration pluginConfiguration,
            TargetManager targetManager
        ) : base(
            clientState,
            pluginInterface,
            dataManager,
            gameGui,
            jobGauges,
            objectTable,
            pluginConfiguration,
            targetManager
        ) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawFairyBar();
            DrawAetherBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        private void DrawFairyBar()
        {
            var gauge = (float)JobGauges.Get<SCHGauge>().FairyGauge;
            BarSize = new Vector2(FairyBarWidth, FairyBarHeight);
            BarCoords = new Vector2(FairyBarX, FairyBarY);
            var cursorPos = new Vector2(CenterX - BarCoords.X, CenterY + BarCoords.Y - 49);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(BarSize.X * gauge / 100, BarSize.Y),
                SchFairyColor["gradientLeft"], SchFairyColor["gradientRight"], SchFairyColor["gradientRight"], SchFairyColor["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            DrawOutlinedText(gauge.ToString(CultureInfo.InvariantCulture), new Vector2(cursorPos.X + BarSize.X * gauge / 100 - (gauge == 100 ? 30 : gauge > 3 ? 20 : 0), cursorPos.Y + (BarSize.Y / 2) - 12));
        }

        private void DrawAetherBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var aetherFlowBuff = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 304);
            var barWidth = (SchAetherBarWidth / 3);
            BarSize = new Vector2(barWidth, SchAetherBarHeight);
            BarCoords = new Vector2(SchAetherBarX, SchAetherBarY);
            var cursorPos = new Vector2(CenterX + BarCoords.X, CenterY + BarCoords.Y - 71);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X - barWidth*2 - SchAetherBarPad * 2, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

            var stackCount = aetherFlowBuff == null ? 0 : aetherFlowBuff.StackCount;
            switch (stackCount)
            {
                case 1:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

                    break;
                case 2:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    break;
                case 3:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    break;
            }

        }
    }
}