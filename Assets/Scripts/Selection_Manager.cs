using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Selection_Manager : MonoBehaviour

{
    // GridManager'a erişim sağlamak için referans tutuyoruz.
    private GridManager gridManager;

    // Dokunuyor mu şu an?
    private bool isSelecting = false;

    //Matrisde nereye dokunuyor?
    private Vector2Int startCoords;

    //Matriste elini kaldırdığında nerede?
    private Vector2Int currentCoords;

    // Oyun ilk açıldığında Gizmos'un (0,0) noktasını sahte bir şekilde yeşile boyamasını engellemek için kontrol ediyoruz.
    private bool hasSelectedOnce = false;




void Start()
     {
        gridManager = GetComponent<GridManager>();


     }
    void Update()
    {
        //ekrana dokunuyor mu dokunuyorsa true yap.
       if (Input.GetMouseButtonDown(0))
        {
            Vector2Int clickedCoords = GetCurrentTileCoords();

            //Oyuncu Grid dışında bir yere dokunulmadığı zaman tıklandı sayar ve ona göre yerleri belirler.
            if(gridManager != null && clickedCoords.x >= 0 && clickedCoords.x < gridManager.width && clickedCoords.y >= 0 && clickedCoords.y < gridManager.height)
                {
                isSelecting = true;
                hasSelectedOnce = true;
                startCoords = clickedCoords;
                currentCoords = startCoords;
                }

            else
            {
                //Bu kısım grid dışına tıklanınca renk değişimini engeller.
                isSelecting = false;

            }

        }

        // Elini basılı tutup sürüklediğin sürece (Sadece geçerli bir seçim başladıysa çalışır)
        if (Input.GetMouseButton(0) && isSelecting)
        {
            
            // Sürüklerken de grid dışına taşmayı engellemek için sadece grid içindeyse güncelliyoruz.
            Vector2Int hooverCoords = GetCurrentTileCoords();
            if (gridManager != null && hooverCoords.x >=0 && hooverCoords.x < gridManager.width && hooverCoords.y >= 0 && hooverCoords.y < gridManager.height)
            {
                currentCoords = hooverCoords;
            }

        }
        
       //ekrana dokunuyor mu dokunmuyorsa false yap.
        if (Input.GetMouseButtonUp(0))
        {
            // Elini kaldırdığında sürükleme bitsin ama son koordinatlar hafızada kalsın.
            isSelecting = false;
        }
    }
    Vector2Int GetCurrentTileCoords()
    {
        
        //oyuncunun mouseunu oyun ekranında 2 boyutlu alanda nerede olduğunu anlamamızı sağlar.
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Ondalıklı olan pozisyonları integer değerlere yuvarlar.
        int x = Mathf.RoundToInt(mouseWorldPos.x);
        int y = Mathf.RoundToInt(mouseWorldPos.y);


        // elde ettiğimiz yeni x ve y yi Vector2Int olarak geri döndürür.
        return new Vector2Int(x, y);

    }

    void OnDrawGizmos()
    {
        //Halihazırda oyun başlamadan karelere renk veriyorsa bunu durdurmak için kontrol yapmalıyız.
        if(Application.isPlaying == false)
        {
            return;
        }
        if(hasSelectedOnce == false)
        {
            return;
        }
        // startCoords veya currentCoords negatifse (yani geçersizse) ya da isSelecting ile ilk defa başlanmadıysa koru
        if (gridManager == null) 
            return;
        // Eğer koordinatlar grid dışındaysa çizimi iptal et
        if (startCoords.x < 0 || startCoords.x >= gridManager.width || startCoords.y < 0 || startCoords.y >= gridManager.height)
            return;

        Gizmos.color = Color.green;

        //Başlangıç ve bitiş noktalarımızın tam orta noktasını hesaplıyoruz bu kısımda.
        float centerX = (startCoords.x + currentCoords.x) / 2f;
        float centerY = (startCoords.y + currentCoords.y) / 2f;

        // Burda da yeni centerımızı atıyoruz.
        Vector3 center = new Vector3(centerX, centerY, 0);


        // Seçilen alanın kaç kare yüksekliğinde ve genişliğinde olduğunu buluyoruz bu sayede.
        float width = Mathf.Abs(startCoords.x - currentCoords.x) + 1;
        float height = Mathf.Abs(startCoords.y - currentCoords.y) + 1;

        Vector3 size = new Vector3(width, height, 0.1f);

        //Burda da işte merkezi belli olan alanı belli olan rectangularımız çizdiriyoruz.
        Gizmos.DrawCube(center, size);

    }


    
}
