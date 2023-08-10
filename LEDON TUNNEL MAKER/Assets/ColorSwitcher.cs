using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSwitcher : MonoBehaviour

{
    public RawImage rawImage;
    public Color colorON;
    public Color colorOFF;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SwitchColorON() {
        rawImage.color = colorON;
    }
    public void SwitchColorOFF() { 
        rawImage.color = colorOFF;
    }
}
