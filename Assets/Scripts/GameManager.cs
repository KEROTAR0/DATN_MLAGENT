using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    #region Menu Panels

    [Header("Main Menu Panels")]
    public GameObject mainMenuPanel;       // Panel của Main Menu
    public GameObject optionPanel;         // Panel Option của Main Menu
    public GameObject helpPanel;           // Panel Help của Main Menu

    [Header("In-Game (Pause) Menu Panels")]
    // Các panel này sẽ được tìm lại khi chuyển scene
    public GameObject pauseMenuPanel;      // Panel Pause Menu trong game
    public GameObject inGameOptionPanel;   // Panel Option trong game
    public GameObject confirmBackPanel;    // Panel xác nhận Back to Menu (Yes/No)

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text highScoreText;
    public Button newGameButton;
    public Button exitButton;


    #endregion

    #region Audio Settings

    [Header("Audio Settings")]
    public AudioSource backgroundAudio;    // Audio background
    public bool isSoundOn = true;

    #endregion

    #region Persistent Player Data (tạm thời giữa các scene)

    [Header("Persistent Player Data (Non-permanent)")]
    public int persistentScore = 0;
    public float persistentHealth = 100f;

    #endregion

    private void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại GameManager qua các scene
        }
        else
        {
            Debug.LogWarning("Multiple GameManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Lắng nghe sự kiện sceneLoaded để cập nhật Sub Menu và tải dữ liệu người chơi
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Khi scene mới được load, cập nhật lại các panel In-Game (nếu là scene gameplay)
    /// và tải lại dữ liệu score/health tạm thời.
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nếu là scene gameplay (ví dụ tên scene bắt đầu bằng "Stage")
        if (scene.name.StartsWith("Stage"))
        {
            pauseMenuPanel = GameObject.Find("Canvas/Menu/Menu Panel");
            inGameOptionPanel = GameObject.Find("Canvas/Menu/Option Panel");
            confirmBackPanel = GameObject.Find("Canvas/Menu/Confirm Back Panel");

            // Đặt lại TimeScale
            Time.timeScale = 1f;
            // Game Over Panel
            Transform canvas = GameObject.Find("Canvas").transform;
            gameOverPanel = canvas.Find("Game Over Panel")?.gameObject;

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false); // Ẩn ban đầu

                highScoreText = gameOverPanel.transform.Find("High Score")?.GetComponent<Text>();
                newGameButton = gameOverPanel.transform.Find("New Game")?.GetComponent<Button>();
                exitButton = gameOverPanel.transform.Find("Exit")?.GetComponent<Button>();

                if (newGameButton != null) newGameButton.onClick.AddListener(() => LoadFirstStage());
                if (exitButton != null) exitButton.onClick.AddListener(() => LoadMainMenu());
            }
            else
            {
                Debug.LogWarning("Game Over Panel not found in the scene.");
            }

            // Tải dữ liệu tạm thời (score, health) vào các component của Player
            LoadPlayerDataForScene();

            AssignSubMenuButtonCallbacks();
        }
        else if (scene.name == "MainMenu")
        {
            pauseMenuPanel = null;
            inGameOptionPanel = null;
            confirmBackPanel = null;
            // Ở Main Menu có thể có panel riêng, gán lại nếu cần
            mainMenuPanel = GameObject.Find("Canvas/Main Menu Panel");
            optionPanel = GameObject.Find("Canvas/Option Panel");
            helpPanel = GameObject.Find("Canvas/Help Panel");
        }
    }
    void AssignSubMenuButtonCallbacks()
    {
        if (pauseMenuPanel != null)
        {
            // Tìm nút Resume
            Button resumeBtn = pauseMenuPanel.transform.Find("Resume")?.GetComponent<Button>();
            if (resumeBtn != null)
            {
                resumeBtn.onClick.RemoveAllListeners();
                resumeBtn.onClick.AddListener(ResumeGame);
            }

            // Tìm nút Reset Stage
            Button resetBtn = pauseMenuPanel.transform.Find("New Game")?.GetComponent<Button>();
            if (resetBtn != null)
            {
                resetBtn.onClick.RemoveAllListeners();
                resetBtn.onClick.AddListener(ResetStage);
            }

            // Tìm nút Option trong Pause Menu
            Button optionBtn = pauseMenuPanel.transform.Find("Setting")?.GetComponent<Button>();
            if (optionBtn != null)
            {
                optionBtn.onClick.RemoveAllListeners();
                optionBtn.onClick.AddListener(OpenInGameOption);
            }

            // Tìm nút Back to Menu
            Button backBtn = pauseMenuPanel.transform.Find("Exit")?.GetComponent<Button>();
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(OpenConfirmBack);
            }
        }

        if (inGameOptionPanel != null)
        {
            // Tìm nút Close Option trong In-Game Option Panel
            Button closeOptionBtn = inGameOptionPanel.transform.Find("Close Option")?.GetComponent<Button>();
            if (closeOptionBtn != null)
            {
                closeOptionBtn.onClick.RemoveAllListeners();
                closeOptionBtn.onClick.AddListener(CloseInGameOption);
            }
        }

        if (confirmBackPanel != null)
        {
            // Tìm nút Confirm Yes
            Button yesBtn = confirmBackPanel.transform.Find("ConfirmYesButton")?.GetComponent<Button>();
            if (yesBtn != null)
            {
                yesBtn.onClick.RemoveAllListeners();
                yesBtn.onClick.AddListener(ConfirmBackYes);
            }

            // Tìm nút Confirm No
            Button noBtn = confirmBackPanel.transform.Find("ConfirmNoButton")?.GetComponent<Button>();
            if (noBtn != null)
            {
                noBtn.onClick.RemoveAllListeners();
                noBtn.onClick.AddListener(ConfirmBackNo);
            }
        }
    }
    #region Main Menu Functions

    public void StartGame()
    {
        // Khi bắt đầu game, reset dữ liệu người chơi tạm thời và load stage 1
        persistentScore = 0;
        persistentHealth = 100f;
        LoadFirstStage();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
    }

    public void OpenOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(true);
    }

    public void CloseOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    public void OpenHelp()
    {
        if (helpPanel != null)
            helpPanel.SetActive(true);
    }

    public void CloseHelp()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);
    }

    public void ExitGame()
    {
        // Lưu điểm cuối cùng nếu game đã hoàn thành (nếu cần)
        SaveFinalScore();
        Application.Quit();
    }

    #endregion

    #region In-Game (Pause) Menu Functions

    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ResetStage()
    {
        Time.timeScale = 1f;
        // Lưu dữ liệu người chơi tạm thời nếu cần (không reset điểm hay máu)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenInGameOption()
    {
        if (inGameOptionPanel != null)
            inGameOptionPanel.SetActive(true);
    }

    public void CloseInGameOption()
    {
        if (inGameOptionPanel != null)
            inGameOptionPanel.SetActive(false);
    }

    public void OpenConfirmBack()
    {
        if (confirmBackPanel != null)
            confirmBackPanel.SetActive(true);
    }

    public void ConfirmBackYes()
    {
        Time.timeScale = 1f;
        LoadMainMenu();
    }

    public void ConfirmBackNo()
    {
        if (confirmBackPanel != null)
            confirmBackPanel.SetActive(false);
    }

    private void TogglePauseMenu()
    {
        if (pauseMenuPanel == null)
            return;

        if (pauseMenuPanel.activeSelf)
        {
            ResumeGame();
        }
        else
        {
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    #endregion

    public void ShowGameOver()
    {
        StartCoroutine(DelayPauseTime());
    }

    private IEnumerator DelayPauseTime()
    {
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 0f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            PlayerScore ps = FindFirstObjectByType<PlayerScore>();
            int finalScore = ps.currentScore;
            int highScore = PlayerPrefs.GetInt("FinalScore", 0);

            if (finalScore > highScore)
            {
                PlayerPrefs.SetInt("HighScore", finalScore);
                PlayerPrefs.Save();
                highScore = finalScore;
            }

            if (highScoreText != null)
                highScoreText.text = $"Score: {highScore}";
        }
    }


    #region Audio Functions

    public void SoundOn()
    {
        isSoundOn = true;
        if (backgroundAudio != null)
        {
            backgroundAudio.Play();
        }
    }
    public void SoundOff()
    {
        isSoundOn = false;
        if (backgroundAudio != null)
        {
            backgroundAudio.Pause();
        }
    }

    #endregion

    #region Scene Loading Functions

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadFirstStage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Stage1");
        LoadPlayerDataForScene();
    }

    public void LoadSecondStage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Stage2");
        LoadPlayerDataForScene();
    }

    public void LoadThirdStage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Stage3");
        LoadPlayerDataForScene();
    }

    #endregion

    #region Persistent Data Functions (Giữa các scene)

    /// <summary>
    /// Lưu dữ liệu người chơi (score và health) tạm thời vào biến persistent.
    /// Gọi hàm này trước khi chuyển scene hoặc reset stage.
    /// </summary>
    public void SavePlayerDataForScene()
    {
        PlayerScore ps = FindFirstObjectByType<PlayerScore>();
        PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();

        if (ps != null)
            persistentScore = ps.currentScore;
        if (ph != null)
            persistentHealth = ph.currentHealth;
    }

    /// <summary>
    /// Tải dữ liệu người chơi tạm thời từ biến persistent.
    /// Gọi sau khi chuyển scene để gán lại giá trị cho Player.
    /// </summary>
    public void LoadPlayerDataForScene()
    {
        PlayerScore ps = FindFirstObjectByType<PlayerScore>();
        PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();

        if (ps != null)
            ps.currentScore = persistentScore;
        if (ph != null)
        {
            ph.currentHealth = persistentHealth;
            ph.UpdateHealthBar(); // Phải đảm bảo UpdateHealthBar() là public trong PlayerHealth
        }
    }

    #endregion

    #region Final Score (Lưu vĩnh viễn khi hoàn thành game)

    /// <summary>
    /// Sau khi hoàn thành game (Stage3 xong), lưu điểm cuối cùng vĩnh viễn.
    /// </summary>
    public void SaveFinalScore()
    {
        PlayerScore ps = FindFirstObjectByType<PlayerScore>();
        if (ps != null)
        {
            PlayerPrefs.SetInt("FinalScore", ps.currentScore);
            PlayerPrefs.Save();
            Debug.Log("Final Score saved: " + ps.currentScore);
        }
    }

    /// <summary>
    /// Đọc điểm cuối cùng từ PlayerPrefs.
    /// </summary>
    public int LoadFinalScore()
    {
        return PlayerPrefs.GetInt("FinalScore", 0);
    }

    #endregion

    private void Update()
    {
        // Mở/đóng In-Game Menu bằng phím Esc (chỉ áp dụng khi đang trong scene gameplay)
        if (SceneManager.GetActiveScene().name.StartsWith("Stage") && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }
}
