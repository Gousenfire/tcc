using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text algorithmName;
    public Text percentageText;
    public Slider percentageSlider;

    private CaveStatiscsGenerator caveStatiscs;

    private void Start()
    {
        caveStatiscs = FindObjectOfType<CaveStatiscsGenerator>();
    }

    private void Update()
    {
        algorithmName.text = caveStatiscs.GetAlgorithmName();
        float cavePercent = caveStatiscs.GetPercent() * 100;
        float cavePercentF = ((int)((cavePercent - (int)cavePercent) * 100)) / 100f;
        cavePercent = (int)cavePercent + cavePercentF;
        percentageText.text = cavePercent + "%";
        percentageSlider.value = caveStatiscs.GetPercent();
        if (percentageSlider.value == 1)
        {
            percentageText.text = "Done!";
        }
    }
}
