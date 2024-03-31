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
        private int? Countdown = null;
        private int quarterLongTick = 0;

        protected static Texture2D UI_DETONATE;

        static Graphic graphicActive;
        static Graphic graphicInactive;

        CompPowerTrader power;
        // CompProperties_Power powerProps;

        int currentCapacitorCharge = 1000;
        readonly int requiredCapacitorCharge = 20000;
        int chargeSpeed = 1;
        private Graphic currentGraphic;

        protected Map currentMap;

        static Building_PsionicBomb()
        {
            UI_DETONATE = ContentFinder<Texture2D>.Get("UI/Detonate", true);

        #if RIMWORLD12
            GraphicRequest requestActive = new GraphicRequest(Type.GetType("Graphic_Single"), "Things/Buildings/PsionicBomb-Active", ShaderDatabase.DefaultShader, new Vector2(3, 3), Color.white, Color.white, new GraphicData(), 0, null);
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
            this.currentGraphic = this.fullyCharged ?  Building_PsionicBomb.graphicActive : Building_PsionicBomb.graphicInactive;

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
            Scribe_Values.Look<int>(ref currentCapacitorCharge, "currentCapacitorCharge", 0);
            Scribe_Values.Look<int?>(ref Countdown, "countdown", null);
            Scribe_Values.Look<int>(ref chargeSpeed, "chargeSpeed", 1);

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

        private bool fullyCharged => (this.currentCapacitorCharge >= this.requiredCapacitorCharge);

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
                act.action = this.InitiatePsionicBlast;
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

        public override void Tick()
        {
            base.Tick();
            // this.currentGraphic = this.fullyCharged ?  Building_PsionicBomb.graphicActive : Building_PsionicBomb.graphicInactive;

            if (Find.TickManager.TicksGame % 250 == 0)
            {
                Messages.Message("Is fully charged? " + this.fullyCharged, MessageTypeDefOf.NeutralEvent);
                this.TickRare();
            }
        }

        public override void TickRare()
        {
            Log.Warning("===== PSIONIC COUNTDOWN: " + this.Countdown);
            base.TickRare();
            var isSolarFlare = this.detectSolarFlare();

            // Initialize charge speed if not charging and not fully charged
            if (this.chargeSpeed == 0 && !this.fullyCharged) this.chargeSpeed = 1;

            if (this.fullyCharged)
            {
                // Reset current charge if needed and update power drain
                if (this.chargeSpeed != 0)
                {
                    this.currentCapacitorCharge = this.requiredCapacitorCharge;
                    this.power.powerOutputInt = 0;
                    this.chargeSpeed = 0;
                    this.updatePowerDrain();
                    UpdateMapMeshDirty();
                }

                if (this.Countdown == null)
                {
                    return;
                }

                // Handle quarterly ticks
                Log.Warning("1");
                if (++quarterLongTick == 4)
                {
                    quarterLongTick = 0;
                    if (this.Countdown > 0)
                    {
                        Log.Warning("2");
                        Messages.Message("Alert!! The Psionic Disruptor Countdown: " + this.Countdown, MessageTypeDefOf.ThreatBig);
                        this.Countdown--;
                        this.DoBlastVisual();
                    }
                    else
                    {
                        Log.Warning("3");
                        this.currentCapacitorCharge = 0;
                        this.chargeSpeed = 1;
                        this.Countdown = null;
                        PsionicBlast.DoPsionicBlast();
                        UpdateMapMeshDirty();
                    }
                }
            }
            else if (this.power.PowerOn) // Charging logic when not fully charged
            {
                // Increment charge
                currentCapacitorCharge += chargeSpeed;
                AdjustChargeSpeedBasedOnPower();
            }

            // // Check for power outage conditions when fully charged
            // if (this.fullyCharged)
            // {
            //     bool hasNoPower = this.power.PowerNet == null || !this.power.PowerNet.HasActivePowerSource;
            //     if (!this.HasSufficientPower() && !isSolarFlare)
            //     {
            //         return;
            //     }
            //
            //     if (!this.isPowerInited)
            //     {
            //         this.isPowerInited = true;
            //         this.power.PowerOutput = -1000;
            //     }
            // }
        }

        private void UpdateMapMeshDirty()
        {
        #if !RIMWORLD15
            Find.CurrentMap.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things, true, false);
        #else
            Find.CurrentMap.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things, true, false);
        #endif
        }

        private void AdjustChargeSpeedBasedOnPower()
        {
            float excessPower = this.power.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
            float storedEnergy = this.power.PowerNet.CurrentStoredEnergy() * 1000;
            if (excessPower + storedEnergy > 5000)
            {
                chargeSpeed += 5 - (this.chargeSpeed % 5) + (int)Math.Round(this.power.PowerNet.CurrentStoredEnergy() * 0.25 / 10) / 2;
                this.updatePowerDrain();
            }
            else if (excessPower + storedEnergy > 1000)
            {
                chargeSpeed += 1;
                this.updatePowerDrain();
            }
            else
            {
                this.chargeSpeed = 1;
            }
        }

        private bool HasSufficientPower()
        {
            return this.power.PowerNet != null && this.power.PowerNet.HasActivePowerSource && this.power.PowerOn;
        }

        private void InitiatePsionicBlast()
        {
            if (this.Countdown == null)
            {
                Messages.Message("Alert!! The Psionic Bomb has *harsh* consequences for all higher lifeforms on the map!", MessageTypeDefOf.ThreatBig);
                this.Countdown = 10;
            }
        }

        private void DoBlastVisual()
        {
            var cell = this.Position;
            // Mote mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_PsycastAreaEffect, null);
            Mote mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_Bombardment, null);
            //mote.Scale = 180f + (this.Countdown );
            // mote.Scale = (-80f * this.Countdown ?? 0) + 800f + 180f;
            mote.Scale = (-100f * this.Countdown ?? 0) + 1000f + 180f;
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

        #region Graphics-text
        public override Graphic Graphic
        {
            get
            {
                // return this.currentGraphic;
                //if (this.fullyCharged == true)
                // Log.Error("Current charge: " + this.currentCapacitorCharge);
                // return this.currentGraphic;
                if (this.currentCapacitorCharge < 20000f)
                {
                    return Building_PsionicBomb.graphicInactive;
                }

                return Building_PsionicBomb.graphicActive;
            }
        }
        #endregion

        public bool UpdateRequiredPower(float extraPower)
        {
            this.power.PowerOutput = -1 * extraPower;
            return true;
        }
    }
}
