using UnityEngine;
using TMPro;
// Tile'ı bir bileşen yapıyoruz Monobehaviourın içine alarak
public class Tile : MonoBehaviour

{
    public Vector2Int coordinates;

    public bool isFilled = false;

    public int targetNumber = 0;

    public TextMeshPro numberText;

    void Start()
    {
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
            if(targetNumber > 0)
            {
                numberText.text = targetNumber.ToString();
            }
            else
            {
                numberText.text = "";
            }
        }
    }

}
