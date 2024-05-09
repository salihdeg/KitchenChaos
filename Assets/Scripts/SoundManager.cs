using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance {  get; private set; }

    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

    [SerializeField] private AudioClipRefsSO _audioClipRefsSO;

    private float _volume = 1f;

    private void Awake()
    {
        Instance = this;

        _volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
    }

    private void Start()
    {
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
        BaseCounter.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
        Player.OnAnyPickedSomething += Player_OnPickedSomething;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySound(_audioClipRefsSO.trash, trashCounter.transform.position);
    }

    private void BaseCounter_OnAnyObjectPlacedHere(object sender, System.EventArgs e)
    {
        BaseCounter baseCounter = sender as BaseCounter;
        PlaySound(_audioClipRefsSO.objectDrop, baseCounter.transform.position);
    }

    private void Player_OnPickedSomething(object sender, System.EventArgs e)
    {
        Player player = sender as Player;
        PlaySound(_audioClipRefsSO.objectPickup, player.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySound(_audioClipRefsSO.chop, cuttingCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(_audioClipRefsSO.deliverySuccess, deliveryCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(_audioClipRefsSO.deliveryFail, deliveryCounter.transform.position);
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volume);
    }

    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volumeMultipler = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volumeMultipler * _volume);
    }

    public void PlayFootstepSound(Vector3 position, float volume)
    {
        PlaySound(_audioClipRefsSO.footstep, position, volume);
    }

    public void PlayCountDownSound()
    {
        PlaySound(_audioClipRefsSO.warning, Vector3.zero);
    }

    public void PlayWarningSound(Vector3 position)
    {
        PlaySound(_audioClipRefsSO.warning, position);
    }

    public void ChangeVolume()
    {
        _volume += 0.1f;
        if (_volume > 1f)
        {
            _volume = 0f;
        }
        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, _volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return _volume;
    }
}
