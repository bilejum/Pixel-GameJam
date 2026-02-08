using UnityEngine;

[System.Serializable] // 必须加这个，否则面板不显示
public class Sound
{
    public string name;      // 你给这段音频起的名字（如 "Explosion"）
    public AudioClip clip;   // 拖入你的 .mp3 或 .wav 文件

    [Range(0f, 1f)]
    public float volume = 1f; // 甚至可以为每个音效单独设置默认音量

    [Range(0.1f, 3f)]
    public float pitch = 1f;  // 设置音调（高音或低音）
}