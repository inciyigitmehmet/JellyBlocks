using System.Collections.Concurrent;
using UnityEngine;



    
    /*  
         LevelGenerator : Bu veriyi partition yöntemiyle algoritmik üretir.
         LevelRepository: Resources/Levels altından bu asset'leri otomatik yükler.
         GridManager    : Bu veriyi okuyup sahneye tile'ları (ızgarayı) spawn eder.
         ShikakuSolver  : Bu verinin gerçekten ÇÖZÜLEBİLİR olduğunu doğrular.
         LevelValidator : Editor'da toplu olarak tüm LevelData'ları test eder.
    */
    [System.Serializable]
    public struct TileHint
    {
        public int x;  //sol taraftan başlayan x koordinatı
        public int y;  //alt taraftan başlayan y koordinatı
        public int number; //Hücrede görünecek sayı (ilgili dikdörtgenin alanıdır.)
    }

    // Bir Shikaku seviyesinin tüm verilerini tutan ScriptableObject sınıfı.
    [CreateAssetMenu(
        fileName = "Level_",
        menuName = "Shikaku/Level Data",
        order = 1
        )]
    public class LevelData : ScriptableObject
    {
        [Min(1)] //Griddeki boyutlar pozitif olmalı yoksa crash yer. Bu sebepten [Min(1)]
        public int width;

        [Min(1)]
        public int height;

        public TileHint[] hints;

        // Yardımcı Hesaplamalar yapar. Solver ve Validatorda kullanacağımız
        public int TotalCells => height * width;
        public int HintCount => (hints != null) ? hints.Length : 0;
    

    }

