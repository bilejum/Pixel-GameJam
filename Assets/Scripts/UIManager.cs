using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField]private TextMeshProUGUI _ammoText;

    private void Awake()
    {
        Instance = this;
    }
    public void UpdateAmmoText(int ammoAmount,int AmmoCapacity)
    {
        if (_ammoText == null) return;
        _ammoText.text = $"Ammo: {ammoAmount}/{AmmoCapacity}";
    }
}
