using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    [SerializeField] private Selection_Manager selection_Manager;

    [SerializeField] private GameManager gameManager;

    private LevelData[] levels;
    private int currentIndex = 0;

    //Mevcut seviyenin çözücü tarafından bulunan tekil çözümü.
    private System.Collections.Generic.List<Shikaku_Solver.SolutionRect> currentSolution;

    //Tekil çözümü dışarıya açan mülk.
    public System.Collections.Generic.List<Shikaku_Solver.SolutionRect> CurrentSolution => currentSolution;
    
    private void Awake()
    {
        levels = LevelRepository.LoadAllLevels();
        if (levels.Length == 0) Debug.LogError("Hiç seviye yok!");

    }

    private void Start()
    {
        //Bileşenleri sahnedeki aktif nesnelerden otomatik olarak bulup bağlıyoruz.
        gridManager = FindAnyObjectByType<GridManager>();
        selection_Manager = FindAnyObjectByType<Selection_Manager>();
        gameManager = FindAnyObjectByType<GameManager>();

        LoadLevelByIndex(0); //İlk leveli yükleyen yer.
                             
    }

    private void Update()
    {
        //N tuşuna basıldığında bir sonraki seviyeye geçeriz.
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }

        //B tuşuna basıldığında bir önceki seviyeye geçeriz.
        if (Input.GetKeyDown(KeyCode.B))
        {
            LoadPreviousLevel();
        }
    }

    public void LoadLevelByIndex(int index)
    {
        if (index < 0 || index >= levels.Length) return;
        currentIndex = index;
        
        //Seviye yüklenirken eski çizimleri sıfırlayıp temizliyoruz.
        Debug.Log($"[DEBUG] LoadLevelByIndex - Seviye: {index}, selection_Manager var mı: {selection_Manager != null}");
        if (selection_Manager != null)
        {
            selection_Manager.ClearAllSelections();
        }

        //Yeni seviyeye geçtiğimiz için kazanma kilidini kaldırıyoruz.
        if (gameManager != null)
        {
            gameManager.ResetWinState();
        }

        gridManager.LoadLevel(levels[currentIndex]);

        //Seviyenin tekil çözümünü bulup hafızaya alıyoruz.
        currentSolution = Shikaku_Solver.Solve(levels[currentIndex]);
    }

    public void LoadNextLevel()
    {
        if (currentIndex + 1 < levels.Length)
            LoadLevelByIndex(currentIndex + 1);
        else
            Debug.Log("Tüm seviyeler bitti!");
    }

    public void LoadPreviousLevel()
    {
        if (currentIndex - 1 >= 0)
            LoadLevelByIndex(currentIndex - 1);
        else
            Debug.Log("İlk seviyedesiniz!");
    }
}
