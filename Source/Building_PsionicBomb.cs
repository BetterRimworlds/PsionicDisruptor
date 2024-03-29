/*
 * This file is part of PsionicBomb, a Better Rimworlds Project.
 *
 * Copyright © 2024 Theodore R. Smith
 * Author: Theodore R. Smith <hopeseekr@gmail.com>
 *   GPG Fingerprint: D8EA 6E4D 5952 159D 7759  2BB4 EEB6 CE72 F441 EC41
 *   https://github.com/BetterRimworlds/PsionicBomb
 *
 * This file is licensed under the Creative Commons No-Derivations v4.0 License.
 * Most rights are reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;
using RimWorld;

namespace BetterRimworlds.PsionicBomb
{
    [StaticConstructorOnStartup]
    class Building_PsionicBomb : Building
    {
        const int ADDITION_DISTANCE = 3;

        public int Warned = 0;

        protected static Texture2D UI_DETONATE;
 
        static Graphic graphicActive;
        static Graphic graphicInactive;

        private bool isPowerInited = true;
        CompPowerTrader power;
        // CompProperties_Power powerProps;

        int currentCapacitorCharge = 1000;
        int requiredCapacitorCharge = 20000;
        int chargeSpeed = 1;

        protected Map currentMap;

        static Building_PsionicBomb()
        {
            UI_DETONATE = ContentFinder<Texture2D>.Get("UI/Detonate", true);

        #if RIMWORLD12
            GraphicRequest requestActive = new GraphicRequest(Type.GetType("Graphic_Single"), "Things/Buildings/PsionicBomb-Active",   ShaderDatabase.DefaultShader, new Vector2(3, 3), Color.white, Color.white, new GraphicData(), 0, null);
            GraphicRequest requestInactive = new GraphicRequest(Type.GetType("Graphic_Single"), "Things/Buildings/PsionicBomb", ShaderDatabase.DefaultShader, new Vector2(3, 3), Color.white, Color.white, new GraphicData(), 0, null);
        #else
            GraphicRequest requestActive = new GraphicRequest(Type.GetType("Graphic_Single"), "Things/Buildings/PsionicBomb-Active",   ShaderDatabase.DefaultShader, new Vector2(3, 3), Color.white, Color.white, new GraphicData(), 0, null, null);
            GraphicRequest requestInactive = new GraphicRequest(Type.GetType("Graphic_Single"), "Things/Buildings/PsionicBomb", ShaderDatabase.DefaultShader, new Vector2(3, 3), Color.white, Color.white, new GraphicData(), 0, null, null);
        #endif

            graphicActive = new Graphic_Single();
            graphicActive.Init(requestActive);

            graphicInactive = new Graphic_Single();
            graphicInactive.Init(requestInactive);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            this.currentMap = map;

            this.power = base.GetComp<CompPowerTrader>();

            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override string GetInspectString()
        {
            // float excessPower = this.power.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
            return "Capacitor Charge: " + this.currentCapacitorCharge + " / " + this.requiredCapacitorCharge + "\n"
                 + "Power needed: " + Math.Round(this.power.powerOutputInt * -1.0f) + " W"
                ;
        }

        // Saving game
        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref currentCapacitorCharge, "currentCapacitorCharge");
            // Scribe_Values.Look<int>(ref requiredCapacitorCharge, "requiredCapacitorCharge");
            Scribe_Values.Look<int>(ref chargeSpeed, "chargeSpeed");

            base.ExposeData();
        }

        private bool detectSolarFlare()
        {
            #if RIMWORLD15
            // Solar flares do not exist in Rimworld v1.5.
            var solarFlareDef = DefDatabase<GameConditionDef>.GetNamed("SolarFlare");
            bool isSolarFlare = this.currentMap.gameConditionManager.ConditionIsActive(solarFlareDef);
            #else
            bool isSolarFlare = this.currentMap.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare);
            #endif

            // if (isSolarFlare)
            // {
            //     Log.Error("A solar flare is occuring...");
            // }

            return isSolarFlare;
        }

        #region Commands

        private bool fullyCharged
        {
            get
            {
                return (this.currentCapacitorCharge >= this.requiredCapacitorCharge);
            }
        }

        protected IEnumerable<Gizmo> GetDefaultGizmos()
        {
            return base.GetGizmos();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            // Add the stock Gizmoes
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (this.fullyCharged == true)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = this.DoPsionicBlast;
                act.icon = UI_DETONATE;
                act.defaultLabel = "Detonate";
                act.defaultDesc = "Detonate";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }

            // +57 319-666-8030
        }

        public override void TickRare()
        {
            base.TickRare();

            var isSolarFlare = this.detectSolarFlare();
            if (this.fullyCharged == true)
            {
                this.power.powerOutputInt = 0;
                // chargeSpeed = 0;
                this.updatePowerDrain();
                if (this.Warned == 2)
                {
                    this.Warned += 1;
                    this.DoBlastVisual();
                    return;
                } else if (this.Warned == 3)
                {
                    this.Warned = 0;
                    this.currentCapacitorCharge = 0;

                    PsionicBlast.DoPsionicBlast();
                    return;
                }
            }

            if (this.fullyCharged == false && this.power.PowerOn)
            {
                currentCapacitorCharge += chargeSpeed;

                float excessPower = this.power.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
                if (excessPower + (this.power.PowerNet.CurrentStoredEnergy() * 1000) > 5000)
                {
                    // chargeSpeed += 5 - (this.chargeSpeed % 5);
                    chargeSpeed = (int)Math.Round(this.power.PowerNet.CurrentStoredEnergy() * 0.25 / 10) / 2;
                    this.updatePowerDrain();
                }
                else if (excessPower + (this.power.PowerNet.CurrentStoredEnergy() * 1000) > 1000)
                {
                    chargeSpeed += 1;
                    this.updatePowerDrain();
                }
            }

            if (this.fullyCharged == true)
            {
                bool hasNoPower = this.power.PowerNet == null || !this.power.PowerNet.HasActivePowerSource;
                bool hasInsufficientPower = this.power.PowerOn == false;
                if (hasNoPower || hasInsufficientPower)
                {
                    // Ignore power requirements during a solar flare.
                    if (isSolarFlare)
                    {
                        return;
                    }

                    return;
                }

                if (this.isPowerInited == false)
                {
                    this.isPowerInited = true;
                    this.power.PowerOutput = -1000;
                }
                
            }
        }

        private void DoPsionicBlast()
        {
            if (this.Warned == 0)
            {
                Messages.Message("Alert!! The Psionic Bomb has *harsh* consequences for all higher lifeforms on the map!", MessageTypeDefOf.ThreatBig);
                this.Warned += 1;
            }
            else if (this.Warned == 1)
            {
                this.Warned += 1;
            }
        }

        private void DoBlastVisual()
        {
            var cell = this.Position;
            // Mote mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_PsycastAreaEffect, null);
            Mote mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_Bombardment, null);
            mote.Scale = 180f;
            mote.rotationRate = Rand.Range(-3f, 3f);
            mote.exactPosition = cell.ToVector3Shifted() + new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
            
            mote.instanceColor = new Color(0, 120/255.0f, 1.0f);
            // moteThrown.instanceColor = new Color(0.368f, 0f, 1f);
            
            GenSpawn.Spawn((Thing) mote, cell, this.Map);
            // var cell = this.Position;
        //     Mote newThing = (Mote) ThingMaker.MakeThing(ThingDefOf.Mote_PowerBeam);
        //     newThing.exactPosition = cell.ToVector3Shifted();
        //     newThing.Scale = 90f;
        //     newThing.rotationRate = 1.2f;
        //     GenSpawn.Spawn((Thing) newThing, cell, this.Map);
        }

        private void updatePowerDrain()
        {
            this.power.powerOutputInt = -1000 * this.chargeSpeed;
        }

        #endregion
        
        public override Graphic Graphic
        {
            get
            {
                //if (this.fullyCharged == true)
                if (this.currentCapacitorCharge < 20000f)
                {
                    return Building_PsionicBomb.graphicInactive;
                }
                else
                {
                    return Building_PsionicBomb.graphicActive;
                }
            }
        }

        public bool UpdateRequiredPower(float extraPower)
        {
            this.power.PowerOutput = -1 * extraPower;
            return true;
        }
    }
}
