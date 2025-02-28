using System.Collections.Generic;
using UnityEngine;

public class Button : PlaceableElement, ISource, IReceiver
{
    [Header("Inputs")]
    [SerializeField] private int inputChannel = -10;
    [Header("Outputs")]
    [SerializeField] private int outputChannel = -13;
    private AudioSource button_sound;

    private float _voltage = 0f;

    public float GetOutput(int id) { return _voltage; }

    public void SetInput(float voltage, int id)
    {
        _voltage = voltage;
        UpdateColor(1, _voltage);
        GameManager.GetChannel(outputChannel).UpdateVoltage();
    }

    private void OnTriggerEnter(Collider other)
    {
        button_sound = GetComponent<AudioSource>();
        button_sound.Play();
        GameManager.GetChannel(outputChannel).AddVoltageSource(this);
    }

    private void OnTriggerExit(Collider other)
    {
        GameManager.GetChannel(outputChannel).RemoveVoltageSource(this);
        button_sound.Play();
    }

    // Start is called before the first frame update
    private void Start()
    {
        LightInit();
        GameManager.GetChannel(inputChannel).AddVoltageReceiver(this);
    }

    protected override void UpdateChannels(List<int> inputChannels, List<int> outputChannels)
    {
        ChangeReceiverChannel(ref inputChannel, inputChannels[0]);
        ChangeSourceChannel(ref outputChannel, outputChannels[0]);
    }
}
