using Unity.Mathematics;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    // grid'in alanı
    public GameObject tilePrefab;
    private GameObject[,] gridMatrix; 

    //Awake, Start'tan önce çalışır. tilePrefab'ın diğer scriptler kullanmadan önce hazır olmasını garantiliyoruz.
    void Awake()
    {
        #if UNITY_EDITOR
        if (tilePrefab == null)
        {
            tilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TilePrefab.prefab");
        }
        #endif
    }

    void Update()
    {

    }
    void GenerateGrid()
    {
        // Grid'in tam ortalanması için gerekli offset (kayma) miktarlarını hesaplıyoruz
        float offsetX = (width - 1) / 2f;
        float offsetY = (height - 1) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Hücreleri tam merkeze göre hizalayarak spawn konumunu belirliyoruz
                Vector3 spawnPosition = new Vector3(x - offsetX, y - offsetY, 0);

                // Belirttiğimiz tilePrefab positionu döndürmeden açısını koruyarak newTile'a kopyalar.
                GameObject newTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);

                //GridManager.cs'i newTile'ın parentı yapıyoruz.
                newTile.transform.parent = transform;

                newTile.name = $"Tile_{x}_{y}"; // Bu sayede tile isimlerini matrix şeklinde görebileceğiz.
                gridMatrix[x, y] = newTile;

                // newTile nesnesinin içindeki Tile bileşenine ulaşıp döngüdeki x ve y koordinatlarını atıyoruz.
                Tile tileComponent = newTile.GetComponent<Tile>();
                if (tileComponent != null)
                {
                    tileComponent.coordinates = new Vector2Int(x, y);
                    tileComponent.isFilled = false; // Hücrenin başlangıçta boş olmasını kesinleştiriyoruz.
                }
            }
        }

        CenterCamera(); // Kamerayı grid'e göre ortalıyoruz
    }

    void CenterCamera()
    {
        // Grid'imiz tam (0,0) merkezinde kurulduğu için kamerayı ortalıyoruz.
        // Y ekseninde -1.5f yaparak kamerayı biraz "aşağı" kaydırıyoruz ki, grid yukarı doğru kaysın
        // ve alt tarafta Hint butonu için daha fazla boşluk kalsın!
        Camera.main.transform.position = new Vector3(0, -1.0f, -10);

        // Dinamik Kamera Yakınlaştırma/Uzaklaştırma (Zoom) Sistemi
        // Telefon ekranlarında UI için üstte ve altta pay bırakmamız gerek.
        float marginY = 2.5f; // Üstte ve altta bırakılacak boşluk miktarı
        float marginX = 1.0f; // Sağda ve solda bırakılacak boşluk miktarı

        float screenAspect = (float)Screen.width / Screen.height;
        
        // Grid'in dikeyde veya yatayda ekrana sığması için gereken minimum kamera boyutlarını hesaplıyoruz
        float requiredSizeY = (height / 2f) + marginY;
        float requiredSizeX = ((width / 2f) + marginX) / screenAspect;

        // Hangisi daha büyükse onu alıyoruz ki grid asla ekranın dışına taşmasın!
        Camera.main.orthographicSize = Mathf.Max(requiredSizeY, requiredSizeX);
        
        // Kamera arka plan rengini biraz daha koyu bej/krem yapıyoruz
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.91f, 0.90f, 0.88f, 1f); 
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
    }
    public void LoadLevel(LevelData data)
    {
        if (data == null) return;

        ClearGrid(); // eski tile'ları yok et

        width = data.width;
        height = data.height;
        gridMatrix = new GameObject[width, height];
        GenerateGrid();

        // İpuçlarını yerleştirdiğim kısım.
        foreach (var hint in data.hints)
        {
            GameObject tile = GetTileAt(hint.x, hint.y);
            if (tile != null)
            {
                Tile t = tile.GetComponent<Tile>();
                t.targetNumber = hint.number;
                t.UpdateTextDisplay();

            }
        }
        CenterCamera();
    }
    
    //Eski gridi temizliyoruz performans için
    private void ClearGrid()
    {
        if (gridMatrix == null) return;
        foreach(var obj in gridMatrix)
        {
            if (obj != null) Destroy(obj);
        }
        gridMatrix = null;
    }
    public GameObject GetTileAt(int x, int y)
    {
	if (gridMatrix == null) return null;
        // Basılan x ve y değerleri Gridin içerisindeyse GridMatrixe değerleri dön.
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return gridMatrix[x, y];

        }
        // değilse boş dön.
        return null;
    }
}
