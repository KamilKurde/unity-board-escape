using System.Collections.Generic;
using UnityEngine;

public class Button : PlaceableElement, ISource, IListener
{
    [Header("Inputs")]
    [SerializeField] private int inputChannel;
    [Header("Outputs")]
    [SerializeField] private int outputChannel;
    
    private float _voltage = 0f;

    public float GetOutput() { return _voltage; }

    public void SetInput(float voltage)
    {
        _voltage = voltage;
        UpdateColor(new []{1}, _voltage);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameManager.GetChannel(outputChannel).AddVoltageSource(this);
    }

    private void OnTriggerExit(Collider other)
    {
        GameManager.GetChannel(outputChannel).RemoveVoltageSource(this);
    }

    // Start is called before the first frame update
    private void Start()
    {
        LightInit();
        GameManager.GetChannel(inputChannel).AddVoltageListener(this);
    }

    protected override void UpdateChannels(List<int> inputChannels, List<int> outputChannels)
    {
        ChangeLinstenerChannel(ref inputChannel, inputChannels[0]);
        ChangeSourceChannel(ref outputChannel, outputChannels[0]);
    }
}
