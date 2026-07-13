using Unity.Mathematics;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    // grid'in alanı
    public GameObject tilePrefab;
    private GameObject[,] gridMatrix; 



    void Start()
    {
        gridMatrix = new GameObject[width, height];
        GenerateGrid();
    }


    void Update()

    {

    }
    void GenerateGrid()

    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Gridimizi 3 boyutlu sistemdeki yeri belli 2 boyutlu olduğu için z değeri 0.
                Vector3 spawnPosition = new Vector3(x, y, 0);
               
                // Belirttiğimiz tilePrefab positionu döndürmeden açısını koruyarak newTile'a kopyalar.
                GameObject newTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
             
                //GridManager.cs'i newTile'ın parentı yapıyoruz.
                newTile.transform.parent = transform;

                newTile.name = $"Tile_{x}_{y}"; // Bu sayede tile isimlerini matrix şeklinde görebileceğiz.
                gridMatrix[x, y] = newTile;

                // newTile nesnesinin içindeki Tile bileşenine ulaşıp döngüdeki x ve y koordinatlarını atıyoruz.
                newTile.GetComponent<Tile>().coordinates = new Vector2Int(x, y);
            }


        }

        CenterCamera();
    }
    void CenterCamera()
    {
        // float şeklinde merkez x ve y noktalarını gridimizde belirliyoruz.
        float centerX = (width - 1) / 2f;
        float centerY = (height - 1) / 2f;
         
        // Cameranın posizyonu ve görüş açısı burada
        Camera.main.transform.position = new Vector3(centerX, centerY, -10);
        Camera.main.orthographicSize = Mathf.Max(width, height) / 1.5f + 1f;

    }
    public GameObject GetTileAt(int x, int y)
    {
        // Basılan x ve y değerleri Gridin içerisindeyse GridMatrixe değerleri dön.
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return gridMatrix[x, y];

        }
        // değilse boş dön.
        return null;
    }
}
