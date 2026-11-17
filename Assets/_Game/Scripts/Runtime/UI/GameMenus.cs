using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LastMemory
{
    /// <summary>In-level pause, death and ending ("The Last Memory") screens.</summary>
    public class GameMenus : MonoBehaviour
    {
        static GameMenus instance;

        public GameObject pausePanel, deathPanel, endingPanel;
        public Button resumeButton, restartButton, pauseMenuButton;
        public Button retryButton, deathMenuButton;
        public Button endingMenuButton;
        [Tooltip("Ending panel: keep playing the new cycle from the forest.")]
        public Button beginAgainButton;
        public TMP_Text endingStats;

        public static bool IsPaused => instance != null && instance.pausePanel != null && instance.pausePanel.activeSelf;

        void Awake()
        {
            instance = this;
            HideAllInternal();
            resumeButton.onClick.AddListener(Resume);
            restartButton.onClick.AddListener(() => { Resume(); LevelManager.Instance.RestartLevel(); });
            pauseMenuButton.onClick.AddListener(() => { Resume(); LevelManager.Instance.QuitToMenu(); });
            retryButton.onClick.AddListener(() => LevelManager.Instance.Retry());
            deathMenuButton.onClick.AddListener(() => LevelManager.Instance.QuitToMenu());
            endingMenuButton.onClick.AddListener(() => LevelManager.Instance.QuitToMenu());
            if (beginAgainButton != null) beginAgainButton.onClick.AddListener(() => LevelManager.Instance.BeginAgain());
        }

        void Update()
        {
            if (!GameInput.PausePressed || SceneLoader.IsLoading) return;
            if (deathPanel.activeSelf || endingPanel.activeSelf) return;
            // Dialogue and cutscenes can't be paused: they own the time scale and the player.
            if (DialogueBox.IsBlocking || DialogueBox.IsCinematic || StoryDirector.InCutscene) return;
            if (pausePanel.activeSelf) Resume(); else Pause();
        }

        void Pause()
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            Select(resumeButton);
            AudioManager.Play(Sfx.UISelect);
        }

        void Resume()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        static void Select(Button b)
        {
            if (EventSystem.current != null && b != null) EventSystem.current.SetSelectedGameObject(b.gameObject);
        }

        void HideAllInternal()
        {
            pausePanel.SetActive(false);
            deathPanel.SetActive(false);
            endingPanel.SetActive(false);
        }

        public static void HideAll()
        {
            if (instance == null) return;
            instance.HideAllInternal();
            Time.timeScale = 1f;
        }

        public static void ShowDeath()
        {
            if (instance == null) return;
            instance.deathPanel.SetActive(true);
            Select(instance.retryButton);
        }

        public static bool EndingVisible => instance != null && instance.endingPanel != null && instance.endingPanel.activeSelf;

        /// <summary>The ending panel, shown after the loop back to the forest.</summary>
        public static void ShowEnding(Story.CycleSummary s)
        {
            if (instance == null) return;
            instance.endingStats.text =
                $"Gems gathered: {s.gems}\n" +
                $"Memories glimpsed: {s.memories} of {Story.TotalMemories}\n" +
                $"Time: {GameSession.FormatTime(s.time)}    Deaths: {s.deaths}\n" +
                $"Notches on the tree: {s.notches}";
            instance.endingPanel.SetActive(true);
            Select(instance.beginAgainButton != null ? instance.beginAgainButton : instance.endingMenuButton);
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
