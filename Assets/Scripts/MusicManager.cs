using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private const string PLAYER_PREFS_MUSIC_VOLUME = "MusicVolume";

    private AudioSource _musicSource;
    private float _volume = 0.2f;

    private void Awake()
    {
        Instance = this;
        _musicSource = GetComponent<AudioSource>();

        _volume = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUME, 0.2f);
        _musicSource.volume = _volume;
    }

    public void ChangeVolume()
    {
        _volume += 0.1f;
        if (_volume > 1f)
        {
            _volume = 0f;
        }

        _musicSource.volume = _volume;

        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME, _volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return _volume;
    }
}
