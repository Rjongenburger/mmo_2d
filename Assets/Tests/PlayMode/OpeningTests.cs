using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace TopDown.Tests
{
    /// <summary>
    /// Plays the opening end to end with a virtual keyboard: intro cutscene, the mage's talk, flying to the
    /// next peak, falling off the edge, worn equipment, and defeating the wind crows with Fire Bolt.
    /// </summary>
    public class OpeningTests
    {
        Keyboard keyboard;
        InputSettings originalSettings;
        InputSettings testSettings;

        [SetUp]
        public void SetUp()
        {
            // Batch mode has no focused Game view; let device input through regardless (restored in TearDown).
            originalSettings = InputSystem.settings;
            testSettings = ScriptableObject.CreateInstance<InputSettings>();
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings = testSettings;
            keyboard = InputSystem.AddDevice<Keyboard>();
            MountainArrival.SkipForTests = true;
        }

        [TearDown]
        public void TearDown()
        {
            InputSystem.RemoveDevice(keyboard);
            if (originalSettings != null)
                InputSystem.settings = originalSettings;
            Object.Destroy(testSettings);
            MountainArrival.SkipForTests = false;
        }

        [UnityTest]
        public IEnumerator KeyboardMovesThePlayer()
        {
            yield return LoadMountains();
            yield return FinishDialogue();

            var player = Player.Instance;
            Vector2 start = player.transform.position;
            yield return Hold(0.5f, Key.S);
            Assert.Less(player.transform.position.y, start.y - 0.8f, "Holding S should walk the player south");
            Assert.IsFalse(player.Airborne.Falling, "Walking on the rock should not make the player fall");
        }

        [UnityTest]
        public IEnumerator WalkingOffTheEdgeMakesYouFall()
        {
            yield return LoadMountains();
            yield return FinishDialogue();

            var player = Player.Instance;
            int healthBefore = player.Health.Current;
            bool fell = false;
            player.Airborne.Fell += () => fell = true;

            // Straight south from the stone circle runs off the cliff (west is blocked by a cairn).
            yield return Hold(1.6f, Key.S);
            yield return WaitFor(() => fell, 2f, "the player to fall off the southern cliff");
            yield return WaitFor(() => !player.Airborne.Falling, 4f, "the player to wake up on the ledge");
            yield return new WaitForSeconds(0.5f);

            Assert.Less(player.Health.Current, healthBefore, "Falling should cost health");
            Assert.IsTrue(WalkableGround.IsGround(player.transform.position), "The player should be back on solid rock");
        }

        [UnityTest]
        public IEnumerator JumpThenFlyAcrossTheGap()
        {
            yield return LoadMountains();
            yield return FinishDialogue();
            var player = Player.Instance;
            player.Inventory.Add(FindGroundItem("Gale feather", includeInactive: true).Item);

            yield return WalkTo(new Vector2(9.9f, 4.5f), 0.1f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.Space));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            yield return new WaitForSeconds(0.2f);
            Assert.IsTrue(player.Airborne.Airborne, "Space should jump");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.Space));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            yield return null;
            Assert.IsTrue(player.Airborne.Flying, "Space in mid-air should start flying with the gale feather");

            yield return WaitFor(() => player.transform.position.x > 18f, 4f, "flying over to the second peak");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return WaitFor(() => !player.Airborne.Airborne, 4f, "landing");
            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(player.Airborne.Falling, "Landing on the second peak should not be a fall");
            Assert.Greater(player.transform.position.x, 16.5f);
        }

        [UnityTest]
        public IEnumerator StartsInSleepwearThatCanBeRemovedAndWorn()
        {
            yield return LoadMountains();
            yield return FinishDialogue();
            var player = Player.Instance;
            var shirt = player.Equipment[EquipSlot.Body];
            Assert.IsNotNull(shirt, "The hero should start out wearing a nightshirt");
            Assert.AreEqual("Nightshirt", shirt.DisplayName);
            Assert.IsNotNull(player.Equipment[EquipSlot.Legs]);
            Assert.IsNotNull(player.Equipment[EquipSlot.Feet]);

            Assert.IsTrue(player.Equipment.Remove(EquipSlot.Body));
            Assert.IsNull(player.Equipment[EquipSlot.Body]);
            int slot = SlotOf(player, "Nightshirt");
            Assert.GreaterOrEqual(slot, 0, "A removed item goes back to the inventory");

            player.UseItem(slot);
            Assert.AreEqual(shirt, player.Equipment[EquipSlot.Body], "Clicking the shirt in the inventory wears it again");
            Assert.AreEqual(-1, SlotOf(player, "Nightshirt"));
        }

        [UnityTest]
        public IEnumerator TutorialCanBeCompleted()
        {
            yield return LoadMountains();
            var guide = Object.FindAnyObjectByType<TutorialGuide>();
            var player = Player.Instance;

            Assert.AreEqual(TutorialGuide.Step.TalkToMage, guide.Current);
            Assert.IsTrue(GameUI.Instance.Dialogue.IsOpen, "The mage should start talking after the portal closes");
            yield return FinishDialogue();
            yield return WaitFor(() => guide.Current == TutorialGuide.Step.TakeFeather, 4f, "the mage's gifts");
            Assert.IsNotNull(player.Caster.Active, "Aldric should teach Fire Bolt");
            Assert.AreEqual("Fire Bolt", player.Caster.Active.DisplayName);

            var feather = FindGroundItem("Gale feather");
            yield return WalkTo(feather.transform.position, 0.5f);
            yield return Tap(Key.E);
            yield return WaitFor(() => guide.Current == TutorialGuide.Step.FlyAcross, 2f, "taking the feather");

            yield return WalkTo(new Vector2(9.9f, 4.5f), 0.1f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.Space));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            yield return new WaitForSeconds(0.2f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.Space));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            yield return WaitFor(() => player.transform.position.x > 18.5f, 4f, "flying across");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return WaitFor(() => guide.Current == TutorialGuide.Step.DefeatCrows, 4f, "landing on the second peak");

            // Fight: walk towards the nearest crow and press F until all four have fallen.
            for (float t = 0f; guide.Current != TutorialGuide.Step.Done; t += Time.deltaTime)
            {
                if (t > 40f)
                    Assert.Fail($"Timed out fighting crows ({guide.CrowsDefeated} defeated)");
                player.Health.HealFull(); // keep the test about casting, not about dodging
                var target = Enemy.Nearest(player.transform.position, 50f);
                if (target != null && Vector2.Distance(player.transform.position, target.transform.position) > 5f && !player.Controller.IsWalkingToTarget)
                    player.Controller.WalkTo(target.transform.position, 4f);
                if (!player.Caster.IsCasting && target != null && Vector2.Distance(player.transform.position, target.transform.position) <= 6f)
                    yield return Tap(Key.F);
                yield return null;
            }

            Assert.GreaterOrEqual(guide.CrowsDefeated, 4);
            Assert.GreaterOrEqual(player.Skills.GetXp(Skill.Magic), 100);
            Assert.AreEqual(2, player.Skills.GetLevel(Skill.Magic), "Four Fire Bolt hits should reach Magic level 2");
        }

        [UnityTest]
        public IEnumerator IntroCutsceneLeadsToTheMountains()
        {
            yield return SceneManager.LoadSceneAsync("Intro");
            Assert.AreEqual("Intro", SceneManager.GetActiveScene().name);
            Assert.IsNotNull(Object.FindAnyObjectByType<IntroCutscene>());
            yield return WaitFor(() => SceneManager.GetActiveScene().name == "Mountains", 30f, "the intro to finish");
        }

        // ------------------------------------------------------------ helpers

        static IEnumerator LoadMountains()
        {
            yield return SceneManager.LoadSceneAsync("Mountains");
            yield return WaitFor(() =>
            {
                var arrival = Object.FindAnyObjectByType<MountainArrival>();
                return arrival != null && arrival.Finished;
            }, 5f, "the mountain scene");
        }

        IEnumerator FinishDialogue()
        {
            var box = GameUI.Instance.Dialogue;
            for (int i = 0; i < 120 && box.IsOpen; i++)
                yield return Tap(Key.Space);
            Assert.IsFalse(box.IsOpen, "The dialogue should close after advancing through it");
        }

        IEnumerator Tap(Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
        }

        IEnumerator Hold(float seconds, params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            yield return new WaitForSeconds(seconds);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
        }

        static IEnumerator WalkTo(Vector2 point, float stopDistance)
        {
            bool arrived = false;
            Player.Instance.Controller.WalkTo(point, stopDistance, () => arrived = true);
            yield return WaitFor(() => arrived, 10f, $"walking to {point}");
        }

        static IEnumerator WaitFor(Func<bool> condition, float timeout, string what)
        {
            for (float t = 0f; !condition(); t += Time.unscaledDeltaTime)
            {
                if (t > timeout)
                    Assert.Fail($"Timed out waiting for {what}");
                yield return null;
            }
        }

        static GroundItem FindGroundItem(string name, bool includeInactive = false) =>
            Object.FindObjectsByType<GroundItem>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude)
                .First(g => g.Item != null && g.Item.DisplayName == name);

        static int SlotOf(Player player, string itemName)
        {
            for (int i = 0; i < Inventory.Capacity; i++)
                if (!player.Inventory[i].IsEmpty && player.Inventory[i].item.DisplayName == itemName)
                    return i;
            return -1;
        }
    }
}
