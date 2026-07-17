using UnityEngine;
using TMPro;
// Tile'ı bir bileşen yapıyoruz Monobehaviourın içine alarak
public class Tile : MonoBehaviour

{
    public Vector2Int coordinates;

    public bool isFilled = false;

    public int targetNumber = 0;

    public TextMeshPro numberText;

    private SpriteRenderer spriteRenderer;

    //Awake, Start'tan önce çalışır. Prefab'daki varsayılan değer ne olursa olsun hücreyi boş başlatıyoruz.
    void Awake()
    {
        isFilled = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateTextDisplay();
    }

    void Update()
    {
        //Hücre dolduğunda arkadaki gri ızgara çizgilerini görmemek için görselini kapatıyoruz.
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = !isFilled;
        }

        // Doluluk durumuna göre sayıyı da her karede dinamik olarak güncelliyoruz (jelly yerleşince gizler).
        UpdateTextDisplay();
    }

    void OnValidate()
    {
        UpdateTextDisplay();
    }

    public void UpdateTextDisplay()
    {
        if(numberText != null)
        {
            // Eğer hücre zaten jöle ile doldurulduysa üzerindeki sayıyı fiziksel olarak kapatıyoruz (gizliyoruz).
            if(targetNumber > 0 && !isFilled)
            {
                numberText.text = targetNumber.ToString();
                numberText.gameObject.SetActive(true);
            }
            else
            {
                numberText.gameObject.SetActive(false);
            }
        }
    }

}
