using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Gridmanagere soruyoruz tahtanın durumu ne diye.
    public GridManager gridManager;

    //Kazanıp kazanmadığımızı belli eden fonksiyon.
    public void CheckWinCondition()
    {
        //Eğer hatalıysa yani grid boşsa sıkıntılıysa geri döndürüyorum ki donup kalmasın ekran.
        if (gridManager == null) return;

        //Bütün tilellara bu şekilde bakabiliriz.
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                //Bu sayede tileObj kısmına şu anki kordinatları eklemiş oluyoruz.
                GameObject tileObj = gridManager.GetTileAt(x, y);

                //Hücrenin varlığından emin olmak istiyoruz.
                if(tileObj != null)
                {
                    //isFilled mı o tile diye bakıyoruz bir de eğer öyleyse return atıyoruz.
                    if(tileObj.GetComponent<Tile>().isFilled == false)
                    {
                        return;
                    }
                }

            }
        }

        //Her şeyin bittiği an kazanılan an. Tahta tamamen doldu.
        Debug.Log("Deliciousss!!!");
    }
}
