﻿using UnityEngine;

namespace LayerLab
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private GameObject[] otherPanels;

        public void OnEnable()
        {
            for (var i = 0; i < otherPanels.Length; i++) otherPanels[i].SetActive(true);
        }

        public void OnDisable()
        {
            for (var i = 0; i < otherPanels.Length; i++) otherPanels[i].SetActive(false);
        }
    }
}