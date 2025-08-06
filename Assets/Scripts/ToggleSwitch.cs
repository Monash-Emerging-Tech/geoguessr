using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/***
 * 
 * ToggleSwitch, Responsible for toggling ON/OFF different game settings 
 * 
 * Written by aleu0007
 * Last Modified: 6/08/2025
 * 
 */
public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [Header("Slider Setup")]
    [SerializeField, Range(0, 1f)] protected float sliderValue;
    public bool CurrentValue { get; private set; }

    private Slider _slider;

    [Header("Events")]
    [SerializeField] private UnityEvent onToggleOn;
    [SerializeField] private UnityEvent onToggleOff;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    protected void OnValidate()
    {
        SetupToggleComponents();
        _slider.value = sliderValue;
    }

    private void SetupToggleComponents()
    {
        if (_slider != null)
            return;

        SetupSliderComponent();
    }

    private void SetupSliderComponent()
    {
        _slider = GetComponent<Slider>();

        if (_slider == null)
        {
            Debug.Log("No slider found!", this);
            return;
        }

        _slider.interactable = false;
        var sliderColors = _slider.colors;
        sliderColors.disabledColor = Color.white;
        _slider.colors = sliderColors;
        _slider.transition = Selectable.Transition.None;
    }

    protected virtual void Awake()
    {
        SetupToggleComponents();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    private void Toggle()
    {
        SetState(!CurrentValue);
    }

    private void SetState(bool state)
    {
        CurrentValue = state;
        _slider.value = sliderValue = CurrentValue ? 1 : 0;

        if (CurrentValue)
            onToggleOn?.Invoke();
        else
            onToggleOff?.Invoke();
    }
}
