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
    /// Plays the opening end to end with a virtual keyboard: intro cutscene, the mage's talk, the leap,
    /// woodcutting and lighting a fire.
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
            yield return Hold(Key.S, 0.5f);
            Assert.Less(player.transform.position.y, start.y - 0.8f, "Holding S should walk the player south");
            Assert.AreNotEqual(Vector2.zero, player.Controller.Facing);
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
            yield return WaitFor(() => guide.Current == TutorialGuide.Step.TakeFeather, 3f, "the feather to be conjured");

            var feather = FindGroundItem("Gale feather");
            yield return WalkTo(feather.transform.position, 0.5f);
            yield return Tap(Key.E);
            yield return WaitFor(() => guide.Current == TutorialGuide.Step.LeapAcross, 2f, "taking the feather");
            Assert.IsTrue(HasItem(player, "Gale feather"));

            var leap = Object.FindObjectsByType<LeapPoint>().First(l => l.name == "Leap East");
            yield return WalkTo(leap.InteractPoint, 0.3f);
            yield return Tap(Key.E);
            yield return WaitFor(() => guide.Current == TutorialGuide.Step.TakeAxe, 4f, "leaping across");
            Assert.Greater(player.transform.position.x, 16f, "The player should be on the second peak");

            var axe = FindGroundItem("Bronze axe");
            yield return WalkTo(axe.InteractPoint, 0.5f);
            yield return Tap(Key.E);
            yield return WaitFor(() => guide.Current == TutorialGuide.Step.ChopTree, 2f, "taking the axe");

            var tree = Object.FindAnyObjectByType<ChoppableTree>();
            yield return WalkTo(tree.InteractPoint, 0.9f);
            yield return Tap(Key.E);
            yield return WaitFor(() => guide.Current == TutorialGuide.Step.LightFire, 6f, "chopping the tree");
            Assert.AreEqual(25, player.Skills.GetXp(Skill.Woodcutting));
            Assert.IsTrue(HasItem(player, "Logs"));
            yield return WaitFor(() => tree.IsFelled, 1f, "the tree to fall");

            int logs = SlotOf(player, "Logs");
            player.UseItem(logs);
            yield return WaitFor(() => guide.Current == TutorialGuide.Step.Done, 4f, "lighting a fire");
            Assert.IsNotNull(Object.FindAnyObjectByType<Campfire>());
            Assert.IsFalse(HasItem(player, "Logs"));
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
            for (int i = 0; i < 80 && box.IsOpen; i++)
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

        IEnumerator Hold(Key key, float seconds)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
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

        static GroundItem FindGroundItem(string name) =>
            Object.FindObjectsByType<GroundItem>().First(g => g.Item != null && g.Item.DisplayName == name);

        static int SlotOf(Player player, string itemName)
        {
            for (int i = 0; i < Inventory.Capacity; i++)
                if (!player.Inventory[i].IsEmpty && player.Inventory[i].item.DisplayName == itemName)
                    return i;
            return -1;
        }

        static bool HasItem(Player player, string itemName) => SlotOf(player, itemName) >= 0;
    }
}
