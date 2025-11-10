using System;
using System.Collections;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;
public class RADIOController : MonoBehaviour
{
    [System.Serializable]
    public struct TrackData
    {
        public string TrackName;
        public AudioClip Track;
        public Sprite CassetteCover;
    }

    public TrackData[] allTracks;
    public Button NextButton;
    public Button PrevButton;
    public Button PauseButton;
    public Button PauseButtonRed;

    public Image PauseButtonImage;

    public Sprite pauseON;
    public Sprite pauseOFF;

    public Slider volumeSlider;
    public Slider staticSlider;
    public settingsController settingsCont;


    public AudioMixer mixer;



    public AudioSource musicSource;
    public AudioSource SFXSource;
    public AudioSource StaticSource;

    public AudioClip CassetteClip;
    public AudioClip ButtonClick;
    public AudioClip Static;

    public Image CassetteViewer;

    public int currentTrackIndex = 0;
    public bool isPaused = false;


    public dialogueManager manager;


    public GameObject controller;
    public GameObject settingsController;
    public GameObject SavePanel;

    public bool isChangedManual = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPaused = false;

        // Start the track monitoring coroutine
        StartCoroutine(MonitorTrackProgress());



        //applies the new value of typingSpeed based on the index. The first applysetting sets in the index, while the one in the function refers to the value in the statement directly.
    }

    void Update()
    {
        
    }

    // Coroutine to monitor track progress even when UI is inactive
    private IEnumerator MonitorTrackProgress()
    {
        while (true)
        {
            // Check if we have a clip loaded and the music source has finished playing and isn't paused
            if (musicSource.clip != null && !musicSource.isPlaying && !isPaused && musicSource.time == 0f && !isChangedManual)
            {
                // Only auto-advance if we're not manually paused and the track actually finished
                // Add a slight delay before advancing to next track
                yield return new WaitForSeconds(1f);

                if (!isChangedManual)
                {
                    AutoAdvanceToNext(); //Automatically play the next track when current finishes
                }

            }

            // Wait a short time before checking again (reduces performance impact)
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void RadioToggle()
    {
        Debug.Log("radiotoggle called");
        Debug.Log("settingsController.activeSelf: " + settingsController.activeSelf);
        Debug.Log("SavePanel.activeSelf: " + SavePanel.activeSelf);

        if (settingsController.activeSelf)
        {
            Debug.Log("Returning because settingsController is active");
            return;
        }
        if (SavePanel.activeSelf)
        {
            Debug.Log("Returning because SavePanel is active");
            return;
        }
        if (controller != null)
        {
            Debug.Log("attempting to close/open");
            bool isControllerActive = controller.activeSelf;
            Debug.Log("Controller was active: " + isControllerActive + ", setting to: " + !isControllerActive);
            controller.SetActive(!isControllerActive);
        }
        if (controller == null)
        {
            Debug.LogWarning("CONTROLLER NOT ASSIGNED");
        }
    }

    public IEnumerator NextTrack()
    {
        if (isPaused)
        {
            UnPauseMusic();
            yield return new WaitForEndOfFrame();
        }

        isChangedManual = true;
        musicSource.Stop();

        PlaySound(ButtonClick);

        yield return new WaitForSeconds(0.5f);

        StaticSource.Stop();
        PlaySound(CassetteClip);

        yield return new WaitForSeconds(4f);

        isPaused = false;

        currentTrackIndex = (currentTrackIndex + 1) % allTracks.Length; //cycles through the enun on each click by using the currentSpeed as the index representative
        PlayTrack(currentTrackIndex);                                                                               //The "%" divides the index by a value of 3, which allows it to wrap around when the total constants reach the end of the enum
        Debug.Log("Playing Next Track...");//applies changes based on currentspeed value which corresponds to each case, based on its indexed position in the enum

        isChangedManual = false;
    }

    // New method for automatic track advancement without sound effects
    public void AutoAdvanceToNext()
    {
        currentTrackIndex = (currentTrackIndex + 1) % allTracks.Length; //increments the track index and wraps it around

        StartCoroutine(StartFade(mixer, "music", 1, 0));

 

        float currentvolume = volumeSlider.value;
        musicSource.volume = currentvolume;


        PlayTrack(currentTrackIndex); //plays the next track immediately
        StartCoroutine(StartFade(mixer, "music", 1, currentvolume));
        Debug.Log("Auto-advancing to next track: " + allTracks[currentTrackIndex].TrackName);
    }

    public IEnumerator PrevTrack()
    {
        if (isPaused)
        {
            UnPauseMusic();
            yield return new WaitForEndOfFrame();
        }

        isChangedManual = true;
        musicSource.Stop();
        PlaySound(ButtonClick);

        yield return new WaitForSeconds(0.5f);

        StaticSource.Stop();
        PlaySound(CassetteClip);

        yield return new WaitForSeconds(4f);

        isPaused = false;

        currentTrackIndex = (currentTrackIndex - 1 + allTracks.Length) % allTracks.Length; //cycles through the enun on each click by using the currentSpeed as the index representative
        PlayTrack(currentTrackIndex);

        Debug.Log("Playing Previous Track...");//The "%" divides the index by a value of 3, which allows it to wrap around when the total constants reach the end of the enum
                                               //applies changes based on currentspeed value which corresponds to each case, based on its indexed position in the enum
        isChangedManual = false;
    }

    public IEnumerator PauseTrack()
    {



        if (!isPaused)
        {

            PauseButtonImage.sprite = pauseOFF;
            PlaySound(ButtonClick);

            yield return new WaitForSeconds(0.5f);

            
            yield return StartCoroutine(StartFade(mixer, "music", 1, 0));

            yield return new WaitForSeconds(1f);

            musicSource.Pause();

            musicSource.volume = 0;

            float staticVolume = staticSlider.value;
            StaticSource.volume = staticVolume; //HERE YOU ARE ONLY ALTERING THE AUDIOSOURCE VOLUME AND NOT THE ONE OF THE CHANNEL. BECAUSE THE FADE COROUTINE USES THE MIXER VOLUME, ITS REMAINING AT 0 AFTER IT RUNS FOR THE FIRST TIME
            //EVEN THOUGH WE ARE RESETTING THE AUDIOSOURCE VOLUME. The mixer volume also has to be reset with the line below.

            yield return StartCoroutine(StartFade(mixer, "static", 1, staticVolume)); //IMP: WITHOUT THIS LINE THE VOLUME FOR THE CHANNEL REMAINS AT 0 BECAUSE OF JOW THE COMPLIMENTARY FUNCTION WORKS

            PlayStatic();
            isPaused = true;

            PauseButton.gameObject.SetActive(false); //the gameobject of the buton has to be specified to allow SetActive to function
            PauseButtonRed.gameObject.SetActive(true);
            Debug.Log("Pausing...");
            yield break;

        }
        if (isPaused)
        {

        }


    }
    public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume) //These are float PARAMETERS and cannot take a return value
    {
        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        audioMixer.SetFloat(exposedParam, Mathf.Log10(targetValue) * 20); //makes sure that by the end of the fade, the exposed parameter hits the float specified
    }

    public void PlaySound(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);


    }

    public float OnVolumeChanged()
    {
        float targetMusicVolume = volumeSlider.value;
        Debug.Log(targetMusicVolume);

        return targetMusicVolume;
    }
    public IEnumerator UnPause()
    {
        

        PauseButtonImage.sprite = pauseON;
        PlaySound(ButtonClick);


        yield return new WaitForSeconds(0.5f);

        float currentvolume = volumeSlider.value; //computer cant remember shit so this is here (again)
        yield return StartCoroutine(StartFade(mixer, "music", 1, currentvolume)); //yied return here ensures that the coroutine reaches completion before proceeding

        musicSource.volume = currentvolume;

        yield return StartCoroutine(StartFade(mixer, "static", 1, 0));


        StaticSource.Stop();
        musicSource.UnPause(); //dont use .Play() here because it will restart the track from the fucking beginning.
        isPaused = false;


        PauseButton.gameObject.SetActive(true);
        PauseButtonRed.gameObject.SetActive(false);

        Debug.Log("Unpausing...");
        yield break;
    }
    public void PlayTrack(int index)
    {
        musicSource.clip = allTracks[currentTrackIndex].Track;
        musicSource.Play();

        CassetteViewer.sprite = allTracks[index].CassetteCover;
    }

    public void PlayStatic()
    {
        StaticSource.clip = Static;
        StaticSource.Play();
    }

    public void PauseMusic()
    {
        StartCoroutine(PauseTrack());
    }

    public void PlayNext()
    {
        StartCoroutine(NextTrack());
    }
    public void PlayPrev()
    {
        StartCoroutine(PrevTrack());
    }

    public void UnPauseMusic()
    {
        StartCoroutine(UnPause());
    }
}