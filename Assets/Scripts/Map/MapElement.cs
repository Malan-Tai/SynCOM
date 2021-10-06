using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapElement : MonoBehaviour
{
    public bool IsWalkable { get => _isWalkable; }
    [SerializeField] private bool _isWalkable = false;
    public EnumCover CoverValue { get => _coverValue; }
    [SerializeField] private EnumCover _coverValue = EnumCover.None;
}
