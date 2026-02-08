using UnityEngine;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // 单例模式，方便全局调用
    public static AudioManager Instance;

    public List<Sound> musicSounds;
    public Sound[] sfxSounds;

    //public List<AudioSource> BackgroundList = new List<AudioSource>();

    public AudioSource musicSource;
    public AudioSource sfxSource;

    private int playListIndex = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景时不销毁
        }
        else
        {
            Destroy(gameObject);
        }


        ShuffleMusic();
    }


    private void Update()
    {
        // 核心逻辑：检查音乐是否播放结束
        // 如果没在播，说明上一首完了，该切歌了
        if (!musicSource.isPlaying)
        {
            // 先增加索引，并使用取模运算 (%) 确保它永远在 0 到 Count-1 之间
            playListIndex = (playListIndex + 1) % musicSounds.Count;

            PlayMusic(playListIndex);
        }
    }

    public void PlayMusic(int index)
    {
        Sound s = musicSounds[index];


        musicSource.pitch = s.pitch;
        musicSource.volume = s.volume;
        musicSource.clip = s.clip;
        musicSource.Play();
    }

    // 播放音效
    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        float randomPitch = UnityEngine.Random.Range(0.7f, 1.5f);
        //s.pitch = randomPitch;
        sfxSource.pitch = randomPitch;


        sfxSource.volume =s.volume;

        if (s == null) { Debug.LogWarning("SFX Not Found: " + name); return; }

        // 使用 PlayOneShot 可以让多个音效重叠播放（比如连射子弹）
        sfxSource.PlayOneShot(s.clip);
    }

    public void ShuffleMusic()
    {
        if (musicSounds == null || musicSounds.Count <= 1) return;

        // 从列表最后一个元素向前遍历
        for (int i = musicSounds.Count - 1; i > 0; i--)
        {
            // 随机选择一个 0 到 i 之间的索引
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            // 交换两个元素
            Sound temp = musicSounds[i];
            musicSounds[i] = musicSounds[randomIndex];
            musicSounds[randomIndex] = temp;
        }

        Debug.Log("Music playlist has been shuffled.");
    }
}