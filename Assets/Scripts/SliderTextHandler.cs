using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SliderTextHandler : MonoBehaviour
{
    public void OnSliderChange(float value)
    {
        transform.GetComponent<TMPro.TextMeshProUGUI>().text = value.ToString();
    }
}
