using System;
using System.Diagnostics;
using System.Numerics;
using System.Linq;
using System.Globalization;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;
using System.Collections.Generic;

namespace DelvUI.Interface {
    public class DancerHudWindow : HudWindow {

        #region static IDs
        public override uint JobId => 38;
        private static int FlCascade = 1814;
        private static int FlFountain = 1815;
        private static int FlWindmill = 1816;
        private static int FlShower = 1817;
        private static int StandardStepOngoing = 1818;
        private static int TechnicalStepOngoing = 1819;
        private static int StandardStepBuff = 1821;
        private static int TechnicalStepBuff = 1822;
        public static int Fd3Buff = 1820;
        private const ulong EmboiteId = 15999;
        private const ulong EntrechatId = 16000;
        private const ulong JeteId = 16001;
        private const ulong PirouetteId = 16002;
        #endregion
        #region configuration
        private Dictionary<string, uint> CascadeColour => PluginConfiguration.JobColorMap[Jobs.DNC * 1000];
        private Dictionary<string, uint> FountainColour => PluginConfiguration.JobColorMap[Jobs.DNC * 1000+1];
        private Dictionary<string, uint> WindmillColour => PluginConfiguration.JobColorMap[Jobs.DNC * 1000+2];
        private Dictionary<string, uint> ShowerColour => PluginConfiguration.JobColorMap[Jobs.DNC * 1000+3];
        private Dictionary<string, uint> EmboiteColour => PluginConfiguration.JobColorMap[Jobs.DNC * 1000+4];
        private Dictionary<string, uint> EntrechatColour => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 5];
        private Dictionary<string, uint> JeteColour => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 6];
        private Dictionary<string, uint> PirouetteColour => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 7];
        private Dictionary<string, uint> StandardBuffColour => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 +8];
        private Dictionary<string, uint> TechnicalBuffColour => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 9];


        private int ProcBarHeight => PluginConfiguration.DNCVerticalBarsHeight;
        private int ProcBarWidth => PluginConfiguration.DNCVerticalBarsWidth;
        private int ProcBarOffsetLower => PluginConfiguration.DNCLowerProcOffset;
        private int ProcBarOffsetRight => PluginConfiguration.DNCRightProcOffset;
        private int BarHeight => PluginConfiguration.DNCHorizontalBarsHeight;
        private int BarWidth => PluginConfiguration.DNCHorizontalBarsWidth;
        private int ProcBarXPos => PluginConfiguration.DNCVerticalBarsXPos;
        private int ProcBarYPos => PluginConfiguration.DNCVerticalBarsYPos;

        private int EspritPadding => PluginConfiguration.DNCEspritBarPadding;
        private int FeatherPadding => PluginConfiguration.DNCFeatherBarPadding;
        private int StepPadding => PluginConfiguration.DNCStepBarPadding;
        private int EspritBarXPOS => PluginConfiguration.DNCEspritBarXPos;
        private int EspritBarYPOS => PluginConfiguration.DNCEspritBarYPos;
        private int FeatherBarXPos => PluginConfiguration.DNCFeatherBarXPos;
        private int FeatherBarYPos => PluginConfiguration.DNCFeatherBarYPos;
        private int StepBarXPos => PluginConfiguration.DNCStepBarXPos;
        private int StepBarYPos => PluginConfiguration.DNCStepBarYPos;
        private bool enableEspritBar => PluginConfiguration.DNCEnableEspritBar;
        private bool enableFeatherBar => PluginConfiguration.DNCEnableFeatherBar;
        private bool enableStepBar => PluginConfiguration.DNCEnableStepBar;
        private bool enableProcBars => PluginConfiguration.DNCEnableProcBars;
        private bool enableProcTimerText => PluginConfiguration.DNCEnableProcTimerText;
        

        private new static int XOffset => 127;
        private new static int YOffset => 476;
        #endregion
        private ulong[] steps = new ulong[4];

        public DancerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            if(enableEspritBar) DrawPrimaryResourceBar();
            if(enableFeatherBar) DrawSecondaryResourceBar();
            if(enableProcBars) DrawProcBars();
            if(enableStepBar) DrawStepBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        protected override void DrawPrimaryResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();

            int xPadding = EspritPadding;
            var barWidth = (BarWidth - xPadding) / 2;
            var xPos = CenterX + EspritBarXPOS - BarWidth/2;
            var yPos = CenterY + EspritBarYPOS;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 50;
            var barSize = new Vector2(barWidth, BarHeight);

            // Chunk 1
            var esprit = Math.Min((int)gauge.Esprit, chunkSize);
            var scale = (float)esprit / chunkSize;
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (scale >= 1.0f) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                    0xFF3DD8FE, 0xFF3BF3FF, 0xFF3BF3FF, 0xFF3DD8FE
                );
            }
            else {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                    0xFF90827C, 0xFF8E8D8F, 0xFF8E8D8F, 0xFF90827C
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            // Chunk 2
            esprit = Math.Max(Math.Min((int)gauge.Esprit, chunkSize * 2) - chunkSize, 0);
            scale = (float)esprit / chunkSize;
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (scale >= 1.0f) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                    0xFF3DD8FE, 0xFF3BF3FF, 0xFF3BF3FF, 0xFF3DD8FE
                );
            }
            else {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                    0xFF90827C, 0xFF8E8D8F, 0xFF8E8D8F, 0xFF90827C
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private void DrawSecondaryResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();

            int xPadding = FeatherPadding;
            var barWidth = (BarWidth - xPadding * 3) / 4;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX + FeatherBarXPos - BarWidth/2;
            var yPos = CenterY + FeatherBarYPos;
            var cursorPos = new Vector2(xPos, yPos);
            var fd3Buff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == Fd3Buff);


            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i <= 4 - 1; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (gauge.NumFeathers > i)
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y), 0xFF4FD29B);
                }

                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            }
            cursorPos = new Vector2(xPos, yPos);
            if(fd3Buff.Any())
            {
                var duration = Math.Abs(fd3Buff.First().Duration);
                drawList.AddRectFilled(new Vector2(cursorPos.X, cursorPos.Y + (BarHeight/4)*3),
                    new Vector2(cursorPos.X + (BarWidth/20) * duration, cursorPos.Y + BarHeight),
                    0xFF4FD29B);
            }
            for (var i = 0; i <= 4 - 1; i++)
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            }
        }

        private void DrawProcBars()
        {

            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var barSize = new Vector2(ProcBarWidth, ProcBarHeight);
            var cursorPos = new Vector2(CenterX + ProcBarXPos, CenterY + ProcBarYPos - 40);
            var drawList = ImGui.GetWindowDrawList();
            var offsetLower = ProcBarOffsetLower;
            var flCascadeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == FlCascade);
            var flFountainBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == FlFountain);
            var flWindmillBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == FlWindmill);
            var flShowerBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == FlShower);

            //flourishing windmill
            drawList.AddRectFilled(new Vector2(cursorPos.X, cursorPos.Y + barSize.Y),
                new Vector2(cursorPos.X + barSize.X, cursorPos.Y + barSize.Y * 2), 0x88000000);
            if (flWindmillBuff.Count() == 1)
            {
                var duration = Math.Abs(flWindmillBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    new Vector2(cursorPos.X, cursorPos.Y + barSize.Y + ((barSize.Y/20)*(20 - duration))),
                    new Vector2(cursorPos.X+ barSize.X, cursorPos.Y + barSize.Y*2),
                    WindmillColour["gradientLeft"], WindmillColour["gradientRight"], WindmillColour["gradientRight"], WindmillColour["gradientLeft"]
                );
                if (enableProcTimerText)
                {
                    var durationText = duration != 0 ? Math.Round(duration).ToString(CultureInfo.InvariantCulture) : "";
                    var textSize = ImGui.CalcTextSize(durationText);
                    DrawOutlinedText(durationText, new Vector2(cursorPos.X + barSize.X / 2f - textSize.X / 2f, cursorPos.Y + barSize.Y - 22));
                }
            }
            drawList.AddRect(new Vector2(cursorPos.X, cursorPos.Y + barSize.Y),
                             new Vector2(cursorPos.X+ barSize.X, cursorPos.Y + barSize.Y * 2), 0xFF000000);

            //flourishing shower
            drawList.AddRectFilled(new Vector2(cursorPos.X, cursorPos.Y + barSize.Y + offsetLower),
                new Vector2(cursorPos.X + barSize.X, cursorPos.Y + barSize.Y * 2 + offsetLower), 0x88000000);
            if (flShowerBuff.Count() == 1)
            {
                var duration = Math.Abs(flShowerBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    new Vector2(cursorPos.X, cursorPos.Y + barSize.Y + offsetLower),
                    new Vector2(cursorPos.X + barSize.X, cursorPos.Y + barSize.Y + ((barSize.Y) / 20) * duration + offsetLower),
                    ShowerColour["gradientLeft"], ShowerColour["gradientRight"], ShowerColour["gradientRight"], ShowerColour["gradientLeft"]
                );
                if (enableProcTimerText)
                {
                    var durationText = duration != 0 ? Math.Round(duration).ToString(CultureInfo.InvariantCulture) : "";
                    var textSize = ImGui.CalcTextSize(durationText);
                    DrawOutlinedText(durationText, new Vector2(cursorPos.X + barSize.X / 2f - textSize.X / 2f, cursorPos.Y + barSize.Y + offsetLower + 48));
                }
            }
            drawList.AddRect(new Vector2(cursorPos.X, cursorPos.Y + barSize.Y + offsetLower),
                             new Vector2(cursorPos.X + barSize.X, cursorPos.Y + barSize.Y * 2 + offsetLower), 0xFF000000);

            cursorPos = new Vector2(CenterX - ProcBarXPos + ProcBarOffsetRight, CenterY + ProcBarYPos - 40);

            //flourishing cascade
            drawList.AddRectFilled(new Vector2(cursorPos.X, cursorPos.Y + barSize.Y),
                new Vector2(cursorPos.X + barSize.X, cursorPos.Y + barSize.Y * 2 ), 0x88000000);
            if (flCascadeBuff.Count() == 1)
            {
                var duration = Math.Abs(flCascadeBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    new Vector2(cursorPos.X, cursorPos.Y + barSize.Y + ((barSize.Y / 20) * (20 - duration))),
                    new Vector2(cursorPos.X + barSize.X, cursorPos.Y + barSize.Y*2),
                    CascadeColour["gradientLeft"], CascadeColour["gradientRight"], CascadeColour["gradientRight"], CascadeColour["gradientLeft"]
                );
                if (enableProcTimerText)
                {
                    var durationText = duration != 0 ? Math.Round(duration).ToString(CultureInfo.InvariantCulture) : "";
                    var textSize = ImGui.CalcTextSize(durationText);
                    DrawOutlinedText(durationText, new Vector2(cursorPos.X + barSize.X / 2f - textSize.X / 2f, cursorPos.Y + barSize.Y - 22));
                }
            }
            drawList.AddRect(new Vector2(cursorPos.X, cursorPos.Y + barSize.Y),
                             new Vector2(cursorPos.X + barSize.X, cursorPos.Y + barSize.Y * 2), 0xFF000000);

            //flourishing fountain
            drawList.AddRectFilled(new Vector2(cursorPos.X, cursorPos.Y + barSize.Y + offsetLower),
                new Vector2(cursorPos.X + barSize.X, cursorPos.Y + barSize.Y * 2 + offsetLower), 0x88000000);
            if (flFountainBuff.Count() == 1)
            {
                var duration = Math.Abs(flFountainBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    new Vector2(cursorPos.X, cursorPos.Y + barSize.Y + offsetLower),
                    new Vector2(cursorPos.X + barSize.X, cursorPos.Y + barSize.Y + ((barSize.Y) / 20) * duration + offsetLower),
                    FountainColour["gradientLeft"], FountainColour["gradientRight"], FountainColour["gradientRight"], FountainColour["gradientLeft"]
                );
                if (enableProcTimerText)
                {
                    var durationText = duration != 0 ? Math.Round(duration).ToString(CultureInfo.InvariantCulture) : "";
                    var textSize = ImGui.CalcTextSize(durationText);
                    DrawOutlinedText(durationText, new Vector2(cursorPos.X + barSize.X / 2f - textSize.X / 2f, cursorPos.Y + barSize.Y + offsetLower + 48));
                }
            }
            drawList.AddRect(new Vector2(cursorPos.X, cursorPos.Y + barSize.Y + offsetLower),
                             new Vector2(cursorPos.X + barSize.X, cursorPos.Y + barSize.Y * 2 + offsetLower), 0xFF000000);

        }

        private void DrawStepBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();

            int xPadding = StepPadding;
            var barWidthTech = (BarWidth - xPadding * 3) / 4;
            var barWidthStandard = (BarWidth - xPadding) / 2;
            var barWidthNormal = BarWidth;
            var barSize = new Vector2(barWidthNormal, BarHeight);
            var xPos = CenterX + StepBarXPos - BarWidth/2;
            var yPos = CenterY + StepBarYPos;
            var cursorPos = new Vector2(xPos, yPos);

            var standardStepOngoing = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == StandardStepOngoing);
            var technicalStepOngoing = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == TechnicalStepOngoing);
            var standardStepBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == StandardStepBuff);
            var technicalStepBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == TechnicalStepBuff);

            var drawList = ImGui.GetWindowDrawList();
            if (!gauge.IsDancing())
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if(standardStepBuff.Any())
                {
                    var duration = Math.Abs(standardStepBuff.First().Duration);
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2((barSize.X / 60) * duration, barSize.Y),
                        StandardBuffColour["base"], StandardBuffColour["base"], StandardBuffColour["base"], StandardBuffColour["base"]
                    );
                    var durationText = duration != 0 ? Math.Round(duration).ToString(CultureInfo.InvariantCulture) : "";
                    var textSize = ImGui.CalcTextSize(durationText);
                    DrawOutlinedText(durationText, new Vector2(cursorPos.X + barSize.X / 2f - textSize.X / 2f, cursorPos.Y - 2));
                }
            }
            //currently performing a standard step
            if(standardStepOngoing.Any())
            {
                barSize = new Vector2(barWidthStandard, BarHeight);
                //drawList.AddRect(cursorPos +new Vector2(0,16), cursorPos+ new Vector2(0, 16) +barSize, 0xFF000000);
                for (var i = 0; i <= 2 - 1; i++)
                {
                    if(gauge.NumCompleteSteps == i)
                    {
                        steps[i] = gauge.NextStep();
                        drawList.AddRect(cursorPos, cursorPos + barSize, 0xFFFFFFFF);
                    }
                    if (gauge.NumCompleteSteps >= i)
                    {
                        uint barColor;
                        switch (steps[i])
                        {
                            case EmboiteId:
                                barColor = EmboiteColour["base"];
                                break;
                            case EntrechatId:
                                barColor = EntrechatColour["base"];
                                break;
                            case JeteId:
                                barColor = JeteColour["base"];
                                break;
                            case PirouetteId:
                                barColor = PirouetteColour["base"];
                                break;
                            default:
                                barColor = EmboiteColour["base"];
                                break;
                        }
                        if(gauge.NumCompleteSteps == i)
                        {
                            //barColor -= 0x55000000;
                        }
                        drawList.AddRectFilled(cursorPos, cursorPos + barSize, barColor);
                        drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    }
                    cursorPos = new Vector2(cursorPos.X + barSize.X + xPadding, cursorPos.Y);
                }
            } else if (technicalStepOngoing.Any())
            {
                barSize = new Vector2(barWidthTech, BarHeight);
                //drawList.AddRect(cursorPos +new Vector2(0,16), cursorPos+ new Vector2(0, 16) +barSize, 0xFF000000);
                for (var i = 0; i <= 4 - 1; i++)
                {
                    if (gauge.NumCompleteSteps == i)
                    {
                        steps[i] = gauge.NextStep();
                    }
                    if (gauge.NumCompleteSteps >= i)
                    {
                        uint barColor;
                        switch (steps[i])
                        {
                            case EmboiteId:
                                barColor = EmboiteColour["base"];
                                break;
                            case EntrechatId:
                                barColor = EntrechatColour["base"];
                                break;
                            case JeteId:
                                barColor = JeteColour["base"];
                                break;
                            case PirouetteId:
                                barColor = PirouetteColour["base"];
                                break;
                            default:
                                barColor = EmboiteColour["base"];
                                break;
                        }
                        if (gauge.NumCompleteSteps == i)
                        {
                            //barColor -= 0x55000000;
                        }
                        drawList.AddRectFilled(cursorPos, cursorPos + barSize, barColor);
                    }
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barSize.X + xPadding, cursorPos.Y);
                }
            } else
            {
                //no steps going on, clear array and create an outline over the whole bar
                Array.Clear(steps, 0, steps.Length);
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
            cursorPos = new Vector2(xPos, yPos);
            if (technicalStepBuff.Any())
            {
                var duration = Math.Abs(technicalStepBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    new Vector2(cursorPos.X,cursorPos.Y + barSize.Y/4*3), cursorPos + new Vector2((barSize.X / 20) * duration, barSize.Y),
                    TechnicalBuffColour["base"], TechnicalBuffColour["base"], TechnicalBuffColour["base"], TechnicalBuffColour["base"]
                );
            }
        }
    }
}

