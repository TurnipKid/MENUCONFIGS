using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;



public class settingsController : MonoBehaviour
{
    public static settingsController Instance;
    public GameObject settingsPanel;
    [SerializeField] public UnityEngine.UI.Slider volumeSliderSFX;
    [SerializeField] public UnityEngine.UI.Slider volumeSliderMUSIC;

    [SerializeField] public AudioMixer audioMixer;

    [SerializeField] public dialogueManager dialogueManager;

  
    public ButtonToggle button;

    public GameObject radioController;

    public GameObject SavePanel;

    public Button DialogueON;
    public Button DialogueOFF;


    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetVolume("sfx", volumeSliderSFX);
        SetVolume("music", volumeSliderMUSIC);

        dialogueManager.dialogueEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DialogueToggleON()
    {
        Debug.Log($"Dialogue enabled: {dialogueManager.dialogueEnabled}");
        dialogueManager.dialogueEnabled = !dialogueManager.dialogueEnabled;

        if (button == null)
        {
            Debug.Log("button not assigned");
        }

        Debug.Log($"Calling SetToggleState with: {dialogueManager.dialogueEnabled}");
        
        Debug.Log($"Dialogue enabled: {dialogueManager.dialogueEnabled}");

        DialogueON.gameObject.SetActive(false);
        DialogueOFF.gameObject.SetActive(true);

    }

    public void DialogueToggleOFF()
    {
        Debug.Log($"Dialogue enabled: {dialogueManager.dialogueEnabled}");
        dialogueManager.dialogueEnabled = !dialogueManager.dialogueEnabled;

        if (button == null)
        {
            Debug.Log("button not assigned");
        }

        Debug.Log($"Calling SetToggleState with: {dialogueManager.dialogueEnabled}");
       
        Debug.Log($"Dialogue enabled: {dialogueManager.dialogueEnabled}");

        DialogueON.gameObject.SetActive(true);
        DialogueOFF.gameObject.SetActive(false);

    }


    public void SettingsToggle()
    {
        if (radioController.activeSelf) return;
        if (SavePanel.activeSelf) return;
        if (settingsPanel != null) { //if settingsPanel is assigned...

            bool isActive = settingsPanel.activeSelf; //stores the boolean tracking the panels activity locally
            settingsPanel.SetActive(!isActive); //sets the activity of the panel to opposite its current state
            
            
        }
    }

    public void BackButton()
    {
        SceneManager.LoadScene(1);
    }

    public void SetVolume(string source, UnityEngine.UI.Slider Slider)
    {
        float volume = Slider.value;  //assigns the value of the slider based on its position to a local float called volume
        audioMixer.SetFloat(source, Mathf.Log10(volume) * 20f); //it then sets this float to the volume of the corresponding mixer and child
        //IMPORTANT NOTE: THE SLIDER SCALES LINEARLY BETWEEN 0 AND 1 WHILE THE VOLUME MIXER VARIES LOGORITHMICALLY. IN ORDER FOR THE TWO TO MATCH, WE MUST MULTIPLY IT BY TEND TO THE LOG 10 TO GET THE FULL AUDIO RANGE
    }

    public void SetVolumeSFX()
    {
        SetVolume("sfx", volumeSliderSFX);
    }
    public void SetVolumeMUSIC()
    {
        SetVolume("music", volumeSliderMUSIC);
    }
}
