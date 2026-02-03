using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Ship _ship;

    [SerializeField] private TextMeshProUGUI _ammoText;

    [SerializeField] private List<Tile> tilesDic;

    private void Awake()
    {
        Instance = this;
    }
    public void UpdateAmmoText(int ammoAmount, int AmmoCapacity)
    {
        if (_ammoText == null) return;
        _ammoText.text = $"Ammo: {ammoAmount}/{AmmoCapacity}";
    }


    public void SwitchColor(string tileColor)
    {
        switch (tileColor)
        {
            case "White":
                _ship.SelectedTile(tilesDic[0]);
                break;
            case "Red":
                _ship.SelectedTile(tilesDic[1]);
                break;
            case "Blue":
                _ship.SelectedTile(tilesDic[2]);
                break;
            default:
                break;
        }
    }

    public void ButtonClick()
    {

    }

}
