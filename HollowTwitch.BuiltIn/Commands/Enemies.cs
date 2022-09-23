﻿using System.Collections;
using System.Linq;
using HollowTwitch.Components;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Extensions;
using HollowTwitch.Precondition;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace HollowTwitch.Commands
{
    public class Enemies : CommandBase
    {
        [HKCommand("spawn")]
        [Summary("Spawns an enemy.\nEnemies: [aspid, buzzer, roller]")]
        [Cooldown(60, 3)]
        public IEnumerator SpawnEnemy(string name)
        {
            string[] enemies = { "aspid", "buzzer", "roller" };
            
            Logger.Log($"Trying to spawn enemy {name}");

            if (!ObjectLoader.InstantiableObjects.TryGetValue(name, out GameObject go))
                yield break;
            
            if (!enemies.Contains(name))
                yield break;

            GameObject enemy = Object.Instantiate(go, HeroController.instance.gameObject.transform.position, Quaternion.identity);

            yield return new WaitForSecondsRealtime(1);

            enemy.SetActive(true);
        }

        [HKCommand("jars")]
        [Summary("Summons 5 collector jars from the ceiling.")]
        [Cooldown(60)]
        public IEnumerator Jars()
        {
            const string path = "_GameCameras/CameraParent/tk2dCamera/SceneParticlesController/town_particle_set/Particle System";

            string[] enemies = {"roller", "aspid", "buzzer"};

            AudioClip shatter_clip = Game.Clips.First(x => x.name == "globe_break_larger");

            Vector3 pos = HeroController.instance.transform.position;

            GameObject break_jar = ObjectLoader.InstantiableObjects["prefab_jar"];

            for (int i = -2; i <= 2; i++)
            {
                // Spawn the jar
                GameObject go = Object.Instantiate
                (
                    ObjectLoader.InstantiableObjects["jar"],
                    pos + new Vector3(i * 7, 10, 0),
                    Quaternion.identity
                );

                go.AddComponent<CircleCollider2D>().radius = .3f;
                go.AddComponent<NonThunker>();
                go.AddComponent<Rigidbody2D>();
                go.AddComponent<DamageHero>().damageDealt = 1;
                go.AddComponent<AudioSource>();

                var ctrl = go.AddComponent<BetterSpawnJarControl>();

                var ps = GameObject.Find(path).GetComponent<ParticleSystem>();

                ctrl.Clip = shatter_clip;

                ctrl.ParticleBreak = break_jar.GetChild("Pt Glass L").GetComponent<ParticleSystem>();
                ctrl.ParticleBreakSouth = break_jar.GetChild("Pt Glass S").GetComponent<ParticleSystem>();

                ctrl.ReadyDust = ctrl.Trail = ps;
                
                // TODO: Implement this maybe
                ctrl.StrikeNailReaction = new GameObject();

                ctrl.EnemyPrefab = ObjectLoader.InstantiableObjects[enemies[Random.Range(0, enemies.Length)]];
                ctrl.EnemyHP = 10;

                yield return new WaitForSeconds(0.1f);

                go.SetActive(true);
            }
        }

        [HKCommand("spawnpv")]
        [Summary("Spawns pure vessel with one-fourth the hp.")]
        [Cooldown(360)]
        public IEnumerator SpawnPureVessel()
        {
            // stolen from https://github.com/SalehAce1/PathOfPureVessel

            yield return null;

            var (x, y, _) = HeroController.instance.gameObject.transform.position;

            GameObject pv = Object.Instantiate
            (
                ObjectLoader.InstantiableObjects["pv"],
                HeroController.instance.gameObject.transform.position + new Vector3(0, 2.6f),
                Quaternion.identity
            );

            pv.GetComponent<HealthManager>().hp /= 4;

            pv.SetActive(true);

            RaycastHit2D castLeft = Physics2D.Raycast(new Vector2(x, y), Vector2.left, 1000, 1 << 8);
            RaycastHit2D castRight = Physics2D.Raycast(new Vector2(x, y), Vector2.right, 1000, 1 << 8);

            if (!castLeft)
                castLeft.distance = 30f;
            if (!castRight)
                castRight.distance = 30f;


            PlayMakerFSM control = pv.LocateMyFSM("Control");
            control.FsmVariables.FindFsmFloat("Left X").Value = x - castLeft.distance;
            control.FsmVariables.FindFsmFloat("Right X").Value = x + castRight.distance;
            control.FsmVariables.FindFsmFloat("TeleRange Max").Value = x - castLeft.distance;
            control.FsmVariables.FindFsmFloat("TeleRange Min").Value = x + castRight.distance;
            control.FsmVariables.FindFsmFloat("Plume Y").Value = y - 3.2f;
            control.FsmVariables.FindFsmFloat("Stun Land Y").Value = y + 3f;

            var plume_gen = control.GetState("Plume Gen");

            plume_gen.InsertMethod
            (
                3,
                () =>
                {
                    GameObject go = control.GetAction<SpawnObjectFromGlobalPool>("Plume Gen", 0).storeObject.Value;
                    PlayMakerFSM fsm = go.LocateMyFSM("FSM");
                    fsm.GetAction<FloatCompare>("Outside Arena?", 2).float2.Value = Mathf.Infinity;
                    fsm.GetAction<FloatCompare>("Outside Arena?", 3).float2.Value = -Mathf.Infinity;
                }
            );
            plume_gen.InsertMethod
            (
                5,
                () =>
                {
                    GameObject go = control.GetAction<SpawnObjectFromGlobalPool>("Plume Gen", 4).storeObject.Value;
                    PlayMakerFSM fsm = go.LocateMyFSM("FSM");
                    fsm.GetAction<FloatCompare>("Outside Arena?", 2).float2.Value = Mathf.Infinity;
                    fsm.GetAction<FloatCompare>("Outside Arena?", 3).float2.Value = -Mathf.Infinity;
                }
            );
            control.GetState("HUD Out").RemoveAction(0);

            var cp = pv.GetComponent<ConstrainPosition>();
            cp.xMax = x + castRight.distance;
            cp.xMin = x - castLeft.distance;
        }


        [HKCommand("revek")]
        [Summary("Spawns revek. Goes away after 30s or one parry.")]
        [Cooldown(240)]
        public IEnumerator Revek()
        {
            GameObject revek = Object.Instantiate
            (
                ObjectLoader.InstantiableObjects["Revek"],
                HeroController.instance.gameObject.transform.position,
                Quaternion.identity
            );

            yield return new WaitForSecondsRealtime(1);

            Object.DontDestroyOnLoad(revek);

            revek.SetActive(true);

            PlayMakerFSM ctrl = revek.LocateMyFSM("Control");

            // Make sure init gets to run.
            yield return null;

            // Actually spawn.
            ctrl.SetState("Appear Pause");
            
            // ReSharper disable once ImplicitlyCapturedClosure (ctrl)
            ctrl.GetState("Hit").AddMethod(() => Object.Destroy(revek));

            // ReSharper disable once ImplicitlyCapturedClosure (ctrl)
            void OnUnload()
            {
                if (revek == null)
                    return;
                
                revek.SetActive(false);
            }

            void OnLoad(Scene a, Scene b)
            {
                try
                {
                    if (revek == null)
                        return;
                    
                    revek.SetActive(true);

                    ctrl.SetState("Appear Pause");
                }
                catch
                {
                    Object.Destroy(revek);
                }
            }

            GameManager.instance.UnloadingLevel += OnUnload;
            USceneManager.activeSceneChanged += OnLoad;

            yield return new WaitForSecondsRealtime(30);

            Object.Destroy(revek);

            GameManager.instance.UnloadingLevel -= OnUnload;
            USceneManager.activeSceneChanged -= OnLoad;
        }

        [HKCommand("duplicateboss")]
        [Summary("Duplicates the current boss in the room. Mostly Godhome only.")]
        [Cooldown(160, 2)]
        public IEnumerator DuplicateBoss()
        {
            if (BossSceneController.Instance == null || BossSceneController.Instance.bosses == null)
                yield break;

            foreach (HealthManager boss in BossSceneController.Instance.bosses)
            {
                Object.Instantiate
                (
                    boss.gameObject,
                    boss.gameObject.transform.position,
                    boss.gameObject.transform.rotation
                );

                yield return new WaitForSeconds(0.2f);
            }
        }

        [HKCommand("spawnshade")]
        [Summary("Spawns the shade.")]
        [Cooldown(60)]
        public void SpawnShade()
        {
            Object.Instantiate(GameManager.instance.sm.hollowShadeObject, HeroController.instance.transform.position, Quaternion.identity);
        }

        [HKCommand("communism")]
        [Summary("Makes all enemies the median HP enemy in the scene")]
        [Cooldown(120)]
        public void Communism()
        {
            HealthManager[] hms = Object.FindObjectsOfType<HealthManager>();

            HealthManager[] sorted = hms.OrderByDescending(x => x.hp).ToArray();

            HealthManager median = sorted[sorted.Length / 2];

            foreach (HealthManager hm in hms)
            {
                if (hm == median)
                    continue;

                Vector3 pos = hm.transform.position;
                
                Object.Destroy(hm.gameObject);

                Object.Instantiate(median.gameObject, pos, median.gameObject.transform.rotation);
            }
        }
        
       
        [HKCommand("zap")]
        [Summary("Uumuu's lightning trail attack.")]
        [Cooldown(30)]
        public IEnumerator StartZapping()
        {
            GameObject prefab = ObjectLoader.InstantiableObjects["zap"];
            
            for (int i = 0; i < 12; i++)
            {
                GameObject zap = Object.Instantiate(prefab, HeroController.instance.transform.position, Quaternion.identity);
                
                zap.SetActive(true);
                
                yield return  new WaitForSeconds(0.5f);
            }
        }
        
    }
}