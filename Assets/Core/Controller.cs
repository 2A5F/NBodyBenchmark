using System;
using Core.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Core
{

    public class Controller : MonoBehaviour
    {
        public UIDocument document;

        private VisualElement root;
        private Blur menu_btn;
        private Blur menu;
        private Blur time;
        private VisualElement play;
        private VisualElement speed1x;
        private VisualElement speed2x;
        private VisualElement speed5x;
        private VisualElement speed10x;
        private VisualElement speed100x;
        private (VisualElement, float)[] speedItems;
        private FloatField speedInput;
        private Label versionLabel;
        private Label projRepoLabel;
        private VisualElement exitBtn;

        private void Start()
        {
            root = document.rootVisualElement;
            menu_btn = root.Q<Blur>("menu-btn");
            menu = root.Q<Blur>("menu");
            time = root.Q<Blur>("time");
            play = root.Q("play");
            speed1x = root.Q("speed-1x");
            speed2x = root.Q("speed-2x");
            speed5x = root.Q("speed-5x");
            speed10x = root.Q("speed-10x");
            speed100x = root.Q("speed-100x");
            speedItems = new[] { (speed1x, 1f), (speed2x, 2), (speed5x, 5), (speed10x, 10), (speed100x, 100) };
            speedInput = root.Q<FloatField>("speed-input");
            versionLabel = root.Q<Label>("version-label");
            projRepoLabel = root.Q<Label>("proj-repo-label");
            exitBtn = root.Q("exit-btn");

            menu_btn.RegMouseOn();
            menu.RegMouseOn();
            time.RegMouseOn();

            menu_btn.RegisterCallback<ClickEvent>(OnClickMenuBtn);
            UpdateMenu();

            play.RegisterCallback<ClickEvent>(OnClickPlayBtn);
            UpdatePlay();

            speed1x.RegisterCallback<ClickEvent>(OnClickSpeed1x);
            speed2x.RegisterCallback<ClickEvent>(OnClickSpeed2x);
            speed5x.RegisterCallback<ClickEvent>(OnClickSpeed5x);
            speed10x.RegisterCallback<ClickEvent>(OnClickSpeed10x);
            speed100x.RegisterCallback<ClickEvent>(OnClickSpeed100x);
            speedInput.RegisterCallback<ChangeEvent<float>>(OnSpeedInputChange);
            UpdateSpeed();

            versionLabel.text = $"v{Application.version}";
            
            projRepoLabel.RegisterCallback<ClickEvent>(OnClickProjRepoLabel);

            exitBtn.RegisterCallback<ClickEvent>(OnClickExitBtn);
        }

        public bool MouseOnCover => menu_btn.MouseOn || time.MouseOn || (menu.MouseOn && openMenu);

        private bool openMenu;
        private bool playing = true;
        private float speed = 1;

        #region menu-btn

        private void OnClickMenuBtn(ClickEvent e)
        {
            ToggleMenu();
        }

        public void OnOpenMenu(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) return;
            ToggleMenu();
        }

        private void ToggleMenu()
        {
            openMenu = !openMenu;
            UpdateMenu();
        }

        private void UpdateMenu()
        {
            if (openMenu)
            {
                menu_btn.AddToClassList("open");
                menu.AddToClassList("open");
            }
            else
            {
                menu_btn.RemoveFromClassList("open");
                menu.RemoveFromClassList("open");
            }
        }

        #endregion

        #region play-btn

        private void OnClickPlayBtn(ClickEvent e)
        {
            TogglePlay();
        }

        public void OnTogglePlay(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) return;
            TogglePlay();
        }

        private void TogglePlay()
        {
            playing = !playing;
            UpdatePlay();
        }

        private void UpdatePlay()
        {
            if (playing)
            {
                play.RemoveFromClassList("pause");
                Time.timeScale = speed;
            }
            else
            {
                play.AddToClassList("pause");
                Time.timeScale = 0;
            }
        }

        #endregion

        #region speed-btn

        #region OnClick

        private void OnClickSpeed1x(ClickEvent e) => SetSpeed(1);
        private void OnClickSpeed2x(ClickEvent e) => SetSpeed(2);
        private void OnClickSpeed5x(ClickEvent e) => SetSpeed(5);
        private void OnClickSpeed10x(ClickEvent e) => SetSpeed(10);
        private void OnClickSpeed100x(ClickEvent e) => SetSpeed(100);

        #endregion

        #region OnSelect

        public void OnSelectSpeed1(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) return;
            SetSpeed(1);
        }

        public void OnSelectSpeed2(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) return;
            SetSpeed(2);
        }

        public void OnSelectSpeed5(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) return;
            SetSpeed(5);
        }

        public void OnSelectSpeed10(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) return;
            SetSpeed(10);
        }

        public void OnSelectSpeed100(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) return;
            SetSpeed(100);
        }

        #endregion

        private void OnSpeedInputChange(ChangeEvent<float> e)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (e.newValue == speed) return;
            SetSpeed(e.newValue);
        }

        public void SetSpeed(float v)
        {
            speed = math.max(0, v);
            UpdateSpeed();
            if (speed <= 0)
            {
                playing = false;
                UpdatePlay();
            }
        }

        private void UpdateSpeed()
        {
            speedInput.value = speed;
            Time.timeScale = speed;
            foreach (var (item, val) in speedItems)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Math.Abs(val - speed) < 0.001f || val == speed)
                {
                    item.AddToClassList("select");
                }
                else
                {
                    item.RemoveFromClassList("select");
                }
            }
        }

        #endregion

        #region proj-repo-link

        private void OnClickProjRepoLabel(ClickEvent e)
        {
            Application.OpenURL("https://github.com/2A5F/NBodyBenchmark");
        }

        #endregion
        
        #region exit-btn

        private void OnClickExitBtn(ClickEvent e)
        {
            Exit();
        }

        private void Exit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        #endregion
    }

}
