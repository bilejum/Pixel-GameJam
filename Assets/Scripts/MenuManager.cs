using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    [SerializeField]private AudioSource startAudio;

    public void OnStartButtonClick()
    {
        SceneTransitioner.Instance.TransitionToScene("WorldScene");

        startAudio.Play();
    }
}
