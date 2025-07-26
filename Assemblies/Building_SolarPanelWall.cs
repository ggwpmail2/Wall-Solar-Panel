using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WallSolarPanel
{
    public class Building_SolarPanelWall : Building
    {
        private CompPowerTrader powerComp;
        private float maxPowerOutput;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            
            if (powerComp == null)
            {
                Log.Error("CompPowerTrader is missing on " + this);
                return;
            }
            
            maxPowerOutput = CalculateMaxPowerOutput();
        }

        private float CalculateMaxPowerOutput()
        {
            int cellCount = this.def.size.x * this.def.size.z;
            return 300f * cellCount;
        }

        public override void TickRare()
        {
            base.TickRare();
            UpdatePowerOutput();
        }

        private void UpdatePowerOutput()
        {
            if (powerComp == null) return;

            if (this.Spawned && this.Map != null)
            {
                if (IsUnderRoof())
                {
                    powerComp.PowerOutput = 0f;
                    return;
                }

                float skyGlow = this.Map.skyManager.CurSkyGlow;
                float efficiency = Mathf.Clamp01(skyGlow);
                
                if (this.Map.weatherManager.curWeather != null)
                {
                    efficiency *= this.Map.weatherManager.curWeather.accuracyMultiplier;
                }
                
                if (this.Map.weatherManager.curWeather.rainRate > 0.1f || 
                    this.Map.weatherManager.curWeather.snowRate > 0.1f)
                {
                    powerComp.PowerOutput = 0f;
                    return;
                }
                
                powerComp.PowerOutput = maxPowerOutput * efficiency;
            }
        }

        private bool IsUnderRoof()
        {
            foreach (IntVec3 cell in this.OccupiedRect())
            {
                if (!cell.Roofed(this.Map))
                {
                    return false; 
                }
            }
            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            
            if (powerComp != null)
            {
                sb.AppendLine();
                sb.Append(string.Format("WallSolarPanel.PowerOutput".Translate(), Mathf.Abs(powerComp.PowerOutput).ToString("F0")));
                sb.Append(" " + string.Format("WallSolarPanel.MaxPowerOutput".Translate(), maxPowerOutput.ToString("F0")));
                if (this.Map != null)
                {
                    sb.Append(" " + string.Format("WallSolarPanel.SkyGlow".Translate(), this.Map.skyManager.CurSkyGlow.ToString("F2")));
                }
                
                if (IsUnderRoof())
                {
                    sb.Append(" " + "WallSolarPanel.UnderRoof".Translate());
                }
            }
            
            return sb.ToString();
        }

        public override AcceptanceReport ClaimableBy(Faction faction)
        {
            return false; 
        }
        
        public override bool CanStackWith(Thing other)
        {
            return false;
        }
    }
}