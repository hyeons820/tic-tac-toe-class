using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(AudioSource))]
public class SwitchController : MonoBehaviour
{
    [SerializeField] private Image handleImage;
    [SerializeField] private AudioClip clickSound;
    
    public delegate void OnSwitchChangedDelegate(bool isOn);
    public OnSwitchChangedDelegate OnSwitchChanged;
    
    private static Color32 _onColor = new Color32(229, 167, 156, 255);
    private static Color32 _offColor = new Color32(80, 80, 80, 255);
    
    private RectTransform _handleRectTransform;
    private Image _backgroundImage;
    private AudioSource _audioSource;
    
    
    private bool _isOn;
    
    private void Awake()
    {
        _handleRectTransform = handleImage.GetComponent<RectTransform>();
        _backgroundImage = GetComponent<Image>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Start()
    {
        _handleRectTransform.anchoredPosition = new Vector2(-14, 0);
        _backgroundImage.color = _offColor;
        _isOn = false;
    }

    private void SetOn(bool isOn)
    {
        if (isOn)
        {
            _handleRectTransform.DOAnchorPosX(14, 0.2f);
            _backgroundImage.DOBlendableColor(_onColor, 0.2f);
        }
        else
        {
            _handleRectTransform.DOAnchorPosX(-14, 0.2f);
            _backgroundImage.DOBlendableColor(_offColor, 0.2f);
        }
        
        // 효과음 재생
        if (OnSwitchChanged != null)
            _audioSource.PlayOneShot(clickSound);
        
        OnSwitchChanged?.Invoke(isOn);
        _isOn = isOn;
    }

    public void OnClickSwitch()
    {
        SetOn(!_isOn);
    }
}
