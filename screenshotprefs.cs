using System.Collections;
using System.IO;
using System.Security.Cryptography;
using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotManager : MonoBehaviour
{
    [Header("UI References")] //For UI Panel customisation
    public GameObject saveDialogPanel;
    public TMP_InputField filenameInput;
    public Button saveButton;
    public Button cancelButton;
    public TMP_Dropdown folderDropdown;

    [Header("Screenshot Settings")]
    public GameObject[] gameObjectsUI;
    public AudioSource audioSource;
    public AudioClip shutterSound;
    public Animator anim;
    public Animator saveAnim;

    private bool UIActive = true;
    private string selectedFolderPath;
    

    public GameObject radioController;

    public GameObject settingsPanel;

    public GameObject[] allUI;



    void Start()
    {
       
        saveButton.onClick.AddListener(ConfirmSave); //listeners needed to updates the path the image it to be saved to when the location is changed
        cancelButton.onClick.AddListener(CancelSave);

        // Setup folder dropdown options
        SetupFolderOptions();

        // Hide dialog initially
        saveDialogPanel.SetActive(false);

        var audioScript = GetComponent<audioscript>();
        if (audioScript == null)
        {
            Debug.LogError("audioscript component not found!");
        }
        else
        {
            Debug.Log("audioscript found, playing shutter sound");
            
        }
    }

    public void ToggleUI()
    {
        UIActive = !UIActive;
        
        if(UIActive== false)
        {
            foreach (var obj in allUI)
            {
                obj.SetActive(false);
            }
        }
        else
        {
            foreach (var obj in allUI)
            {
                obj.SetActive(true);
            }
        }
        
    }

    void SetupFolderOptions()
    {
        folderDropdown.options.Clear();
        folderDropdown.options.Add(new TMP_Dropdown.OptionData("Desktop")); //Initiates the folder options when the dropdown is selected
        folderDropdown.options.Add(new TMP_Dropdown.OptionData("Documents"));
        folderDropdown.options.Add(new TMP_Dropdown.OptionData("Pictures"));
        folderDropdown.options.Add(new TMP_Dropdown.OptionData("Downloads")); //More can be added if Needed.
        folderDropdown.RefreshShownValue(); //refreshes th eselected folder in the dropdown

        // Set default folder
        UpdateSelectedFolder(); //Updates the path based on the case selected at each index
        folderDropdown.onValueChanged.AddListener(delegate { UpdateSelectedFolder(); }); //adds updateselectedfolder as a listener, allowing it to change each time a new option is selected from the dropdown
    }

    void UpdateSelectedFolder()
    {
        switch (folderDropdown.value)
        {
            case 0:
                selectedFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                break;
            case 1:
                selectedFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                break;
            case 2:
                selectedFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
                break;
            case 3:
                selectedFolderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
                break;
        } //switched between each folder and its corresponding path as the user selects each option in the dropdown
    }

    public void BeginSave()
    {
        if (radioController.activeSelf) return;
        if (settingsPanel.activeSelf) return;
        saveDialogPanel.SetActive(true); //initiates the save and sets the panel to true making it visible

        
        string defaultName = "outfit_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); //default filename, and assigns it to the actual text in the input field.
        filenameInput.text = defaultName;

        
        filenameInput.Select(); //Allows for text selection with the mouse and active modification of said text and consequent file name 
        filenameInput.ActivateInputField();
    }

    public void ConfirmSave() //mapped to save button
    {
        string filename = filenameInput.text; //stores the filename locally

        
        if (string.IsNullOrWhiteSpace(filename)) //Validation: If the filename is empty, prompt a warning
        {
            filename = "sceenshot";
            return;
        }

        
        filename = SanitizeFilename(filename); //removes restricted characters or symbols

        
        saveDialogPanel.SetActive(false); //closes the panel
        StartCoroutine(Screenshot(filename)); //starts screenshot
    }

    public void CancelSave()
    {
        saveDialogPanel.SetActive(false); //closes the panel, to be mapped to the cancel button.
    }

    string SanitizeFilename(string filename)
    {
        // Remove invalid filename characters
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char c in invalidChars)
        {
            filename = filename.Replace(c, '_'); //relpaces any invalid characters with a dash and returns the fixed filename for use
        }
        return filename;
    }

    public void TakeScreenshot(string customFilename)
    {
        // Checks to make sure the directory actually exists
        if (!Directory.Exists(selectedFolderPath))
        {
            Directory.CreateDirectory(selectedFolderPath);
        }

        //if the filename in lowecase does not end with the suffix .png, we add it to the string for the filename
        if (!customFilename.ToLower().EndsWith(".png"))
        {
            customFilename += ".png";
        }

        string fullPath = Path.Combine(selectedFolderPath, customFilename); //gets the full path by combining the selected folder path with the filename

        // checks if file already exists
        int counter = 1;
        string originalPath = fullPath;
        while (File.Exists(fullPath)) //if a file with that current path exists
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath); //gets the original filename without the path additions
            string directory = Path.GetDirectoryName(originalPath); //gets the directory
            fullPath = Path.Combine(directory, $"{nameWithoutExtension}_{counter}.png"); //combines the directory and filename but with a counter bracket to distingush between them
            counter++; //increments the counter by one for the next save
        }

        ScreenCapture.CaptureScreenshot(fullPath); //captures the screen
        Debug.Log("Screenshot saved to: " + fullPath);
    }





    IEnumerator Screenshot(string filename)
    {
        // hides UI elements
        foreach (GameObject obj in gameObjectsUI)
        {
            obj.SetActive(false);
            UIActive = false;
        }

        // play shutter sound and animation
        audioSource.PlayOneShot(shutterSound);

        yield return new WaitForSeconds(0.4f);



        if (anim)
            anim.SetTrigger("flash");

        yield return new WaitForSeconds(1f);

        
        TakeScreenshot(filename);

        yield return new WaitForSeconds(1f);

        
        foreach (GameObject obj in gameObjectsUI)
        {
            obj.SetActive(true);
            UIActive = true;
        }

        
        if (saveAnim)
        {
            saveAnim.Play("saved in clip");
            yield return new WaitForSeconds(2f);
            saveAnim.Play("saved out clip");
        }
    }

    // Alternative method for editor testing
#if UNITY_EDITOR
    [ContextMenu("Take Screenshot with Editor Dialog")]
    public void TakeScreenshotWithEditorDialog()
    {
        string path = UnityEditor.EditorUtility.SaveFilePanel(
            "Save Screenshot",
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
            "png"
        );

        if (!string.IsNullOrEmpty(path))
        {
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("Screenshot saved to: " + path);
        }
    }
#endif
}