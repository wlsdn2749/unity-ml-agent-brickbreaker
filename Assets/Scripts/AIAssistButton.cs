using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AIAssistButton : MonoBehaviour
{
    [SerializeField] private GameObject image1;
    [SerializeField] private GameObject image2;

    private bool _isToggled = false;
    public bool IsToggled => _isToggled;
    public Toggle m_Toggle;

    [SerializeField] private InferenceAgent m_inferenceAgent;
    [SerializeField] private EnvController m_envController;
      
    
    private void Awake()
    {
        m_Toggle = GetComponent<Toggle>();
    }

    private void Start()
    {
        m_Toggle.onValueChanged.AddListener(delegate(bool arg0)
        {
            ToggleValueChanged(m_Toggle);
        });
        
        image1.SetActive(!_isToggled);
        image2.SetActive(_isToggled);
    }
    private void Update()
    {

    }

    public void ToggleValueChanged(Toggle change)
    {
        if (!m_envController.isShootEnabled)
        {
            Debug.Log("Toggle action prevented: isShootEnabled is false.");
            return;
        }
        
        _isToggled = !_isToggled;
        
        // 이미지 상태 전환
        image1.SetActive(!_isToggled);
        image2.SetActive(_isToggled);

        m_inferenceAgent.GetActionInference(_isToggled);

    }
}
