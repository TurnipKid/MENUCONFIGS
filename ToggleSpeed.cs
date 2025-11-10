using UnityEngine;
using UnityEngine.UI;
public class ToggleSpeed : MonoBehaviour
{
    public enum SpeedSetting { Slow, Medium, Fast }; //Enums are basically hoods which contain a set of constants.
    public SpeedSetting currentSpeed = SpeedSetting.Medium; //This will allow us to fetch our currentSpeed based on the constant contained in the enum

    public Image buttonImage;
    public Button speedButton;
    
    public Sprite buttonSlow;
    public Sprite buttonMedium;
    public Sprite buttonFast;

    public Sprite slowHighlighted;
    public Sprite mediumHighlighted;
    public Sprite fastHighlighted;

    public dialogueManager manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speedButton.onClick.AddListener(ChangeSpeed); //On every click, we will cycle through the enum by using the currentSpeed, which will represent the index so we can access the values at each value
        
        ApplySpeedSetting(); //applies the new value of typingSpeed based on the index. The first applysetting sets in the index, while the one in the function refers to the value in the statement directly.
    }

    // Update is called once per frame
    void Update()
    {

    }

    


    public void ChangeSpeed()
    {
        currentSpeed = (SpeedSetting)(((int)currentSpeed + 1) % 3); //cycles through the enun on each click by using the currentSpeed as the index representative
                                                                    //The "%" divides the index by a value of 3, which allows it to wrap around when the total constants reach the end of the enum
        ApplySpeedSetting(); //applies changes based on currentspeed value which corresponds to each case, based on its indexed position in the enum
    }

    void ApplySpeedSetting()
    {
        SpriteState spritestate = speedButton.spriteState; //locally declares the sprite state of the button allowing us to modify the sprites occupied on each case

        switch (currentSpeed)
        {
            case SpeedSetting.Slow:
                manager.typingSpeed = 0.08f;
                speedButton.image.sprite = buttonSlow; //swaps the default image sprite
                spritestate.highlightedSprite = slowHighlighted; //swaps the highlighted sprite but we need to refernce the state
                break;

            case SpeedSetting.Medium:
                manager.typingSpeed = 0.05f;
                speedButton.image.sprite = buttonMedium;
                spritestate.highlightedSprite = mediumHighlighted;
                break;

            case SpeedSetting.Fast:
                manager.typingSpeed = 0.02f;
                speedButton.image.sprite = buttonFast;
                spritestate.highlightedSprite = fastHighlighted;
                break;
        }
        speedButton.spriteState = spritestate; //applies changes
    }
}

    
