using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Synthesizer : MonoBehaviour
{
    [Header("Oscillators")]
    public Slider sinSlider;
    public Slider squareSlider;
    public Slider triangleSlider;
    public Slider sawtoothSlider;
    public Slider whiteNoiseSlider;
    public List<Oscillator> oscillators = new List<Oscillator>();

    [Header("Envelope")]
    public Slider attackSlider;
    public Slider decaySlider;
    public Slider sustainSlider;
    public Slider releaseSlider;
    private Envelope envelope;

    [Header("General")]
    public Slider volumeSlider;

    [Header("LFO")]
    public Slider lfoFreqSlider;
    public Slider lfoAmpSlider;

    void Start()
    {
        // Oscillators
        sinSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });
        squareSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });
        triangleSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });
        sawtoothSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });
        whiteNoiseSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });

        // Envelope
        EnvelopeChange();
        attackSlider.onValueChanged.AddListener(delegate { EnvelopeChange(); });
        decaySlider.onValueChanged.AddListener(delegate { EnvelopeChange(); });
        sustainSlider.onValueChanged.AddListener(delegate { EnvelopeChange(); });
        releaseSlider.onValueChanged.AddListener(delegate { EnvelopeChange(); });
    }

    void Update()
    {
    }

    void OscillatorChange()
    {
        foreach (Oscillator oscillator in oscillators)
        {
            oscillator.sinGain = sinSlider.value;
            oscillator.squareGain = squareSlider.value;
            oscillator.triangleGain = triangleSlider.value;
            oscillator.sawtoothGain = sawtoothSlider.value;
            oscillator.whiteNoiseGain = whiteNoiseSlider.value;
        }
    }

    void EnvelopeChange()
    {
        envelope.attackTime = attackSlider.value;
        envelope.decayTime = decaySlider.value;
        envelope.maxAmplitude = 1.0;
        envelope.sustainAmplitude = sustainSlider.value;
        envelope.releaseTime = releaseSlider.value;
        envelope.triggerOnTime = 0.0;
        envelope.triggerOffTime = 0.0;
        envelope.noteOn = false;

        foreach (Oscillator oscillator in oscillators)
        {
            oscillator.SetEnvelope(envelope);
        }
    }

    // Signals mixing
    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {
            foreach (Oscillator oscillator in oscillators)
            {
                data[i] += oscillator.GenerateSignal(lfoFreqSlider.value, lfoAmpSlider.value) * volumeSlider.value;
            }
            if (channels == 2)
            {
                data[i + 1] += data[i];
            }
        }
    }
}