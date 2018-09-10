using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using Verse.Sound;

namespace MetallicBatteries
{
    [StaticConstructorOnStartup]
    public class Building_MetallicBattery : Building
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref ticksToExplode, "ticksToExplode", 0, false);
        }

        public override void Draw()
        {
            base.Draw();
            CompPowerBattery comp = GetComp<CompPowerBattery>();
            GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
            r.center = DrawPos + Vector3.up * 0.1f;
            r.size = BarSize;
            r.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
            r.filledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(GenMath.LerpDouble(0.5f, 1f, 1f, 0f, r.fillPercent), GenMath.LerpDouble(0f, 0.5f, 0f, 1f, r.fillPercent), 0.2f), false);
            r.unfilledMat = BatteryBarUnfilledMat;
            r.margin = 0.15f;
            Rot4 rotation = Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
            if (ticksToExplode > 0 && Spawned)
            {
                Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (ticksToExplode > 0)
            {
                if (wickSustainer == null)
                {
                    StartWickSustainer();
                }
                else
                {
                    wickSustainer.Maintain();
                }
                ticksToExplode--;
                if (ticksToExplode == 0)
                {
                    IntVec3 randomCell = this.OccupiedRect().RandomCell;
                    float radius = Rand.Range(0.5f, 1f) * 3f;
                    GenExplosion.DoExplosion(randomCell, Map, radius, DamageDefOf.Flame, null, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);
                    GetComp<CompPowerBattery>().DrawPower(400f);
                }
            }
        }
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (!Destroyed && ticksToExplode == 0 && dinfo.Def == DamageDefOf.Flame && Rand.Value < 0.05f && GetComp<CompPowerBattery>().StoredEnergy > 500f)
            {
                ticksToExplode = Rand.Range(70, 150);
                StartWickSustainer();
            }
        }
        private void StartWickSustainer()
        {
            SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
            wickSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
        }

        private int ticksToExplode;
        private Sustainer wickSustainer;
        private static readonly Vector2 BarSize = new Vector2(1.3f, 0.4f);
        private const float MinEnergyToExplode = 500f;
        private const float EnergyToLoseWhenExplode = 400f;
        private const float ExplodeChancePerDamage = 0.05f;
        private static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);
    }
}
