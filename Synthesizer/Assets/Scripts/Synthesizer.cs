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
    public Slider redNoiseSlider;
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

    [Header("LPFilter")]
    public Slider lpCutoff;
    public Slider lpQ;

    [Header("HPFilter")]
    public Slider hpCutoff;
    public Slider hpQ;

    [Header("BPFilter")]
    public Slider bpCutoff;
    public Slider bpQ;

    // Filters values
    private float[] dataCopy = new float[2048];     // Current frame (array) copy
    private float[] oldY = new float[4];            // Old frame last samples after filtering
    private float[] oldX = new float[4];            // Old frame last samples before filtering

    private float s;
    private float c;
    private float alfa;
    private float r;

    private float a0;
    private float a1;
    private float a2;
    private float b1;
    private float b2;

    void Start()
    {
        // Oscillators
        sinSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });
        squareSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });
        triangleSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });
        sawtoothSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });
        whiteNoiseSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });
        redNoiseSlider.onValueChanged.AddListener(delegate { OscillatorChange(); });

        // Envelope
        EnvelopeChange();
        attackSlider.onValueChanged.AddListener(delegate { EnvelopeChange(); });
        decaySlider.onValueChanged.AddListener(delegate { EnvelopeChange(); });
        sustainSlider.onValueChanged.AddListener(delegate { EnvelopeChange(); });
        releaseSlider.onValueChanged.AddListener(delegate { EnvelopeChange(); });

        // Filters
        lpCutoff.onValueChanged.AddListener(delegate { LPFilterChange(); });
        lpQ.onValueChanged.AddListener(delegate { LPFilterChange(); });

        hpCutoff.onValueChanged.AddListener(delegate { HPFilterChange(); });
        hpQ.onValueChanged.AddListener(delegate { HPFilterChange(); });

        bpCutoff.onValueChanged.AddListener(delegate { BPFilterChange(); });
        bpQ.onValueChanged.AddListener(delegate { BPFilterChange(); });
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
            oscillator.redNoiseGain = redNoiseSlider.value;
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

    void LPFilterChange()
    {
        s = Mathf.Sin(lpCutoff.value * 2.0f * Mathf.PI / 48000);
        c = Mathf.Cos(lpCutoff.value * 2.0f * Mathf.PI / 48000);
        alfa = s / (2 * lpQ.value);
        r = 1 / (1 + alfa);

        a0 = 0.5f * (1 - c) * r;
        a1 = (1 - c) * r;
        a2 = a0;
        b1 = -2 * c * r;
        b2 = (1 - alfa) * r;
    }

    void HPFilterChange()
    {
        s = Mathf.Sin(hpCutoff.value * 2.0f * Mathf.PI / 48000);
        c = Mathf.Cos(hpCutoff.value * 2.0f * Mathf.PI / 48000);
        alfa = s / (2 * hpQ.value);
        r = 1 / (1 + alfa);

        a0 = 0.5f * (1 + c) * r;
        a1 = -(1 + c) * r;
        a2 = a0;
        b1 = -2 * c * r;
        b2 = (1 - alfa) * r;
    }

    void BPFilterChange()
    {
        s = Mathf.Sin(bpCutoff.value * 2.0f * Mathf.PI / 48000);
        c = Mathf.Cos(bpCutoff.value * 2.0f * Mathf.PI / 48000);
        alfa = s / (2 * bpQ.value);
        r = 1 / (1 + alfa);

        a0 = alfa * r;
        a1 = 0;
        a2 = -a0;
        b1 = -2 * c * r;
        b2 = (1 - alfa) * r;
    }

    // Signals mixing
    void OnAudioFilterRead(float[] data, int channels)
    {
        // Oscillators
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


        // Filters
        for (int i = 0; i < data.Length; i++)
        {
            dataCopy[i] = data[i];
        }

        if (lpCutoff.value != lpCutoff.minValue)
        {
            Filter(ref data, channels);
        }
        if (hpCutoff.value != hpCutoff.minValue)
        {
            Filter(ref data, channels);
        }
        if (bpCutoff.value != bpCutoff.minValue)
        {
            Filter(ref data, channels);
        }

        oldY[0] = data[2044];
        oldY[1] = data[2045];
        oldY[2] = data[2046];
        oldY[3] = data[2047];

        oldX[0] = dataCopy[2044];
        oldX[1] = dataCopy[2045];
        oldX[2] = dataCopy[2046];
        oldX[3] = dataCopy[2047];
    }

    void Filter(ref float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {
            if (i >= 4)
            {
                data[i] = a0 * dataCopy[i] + a1 * dataCopy[i - 1 * channels] + a2 * dataCopy[i - 2 * channels] - b1 * data[i - 1 * channels] - b2 * data[i - 2 * channels];
            }
            else if (i == 0)
            {                
                data[0] = a0 * dataCopy[0] + a1 * oldX[2] + a2 * oldX[0] - b1 * oldY[2] - b2 * oldY[0];
            }
            else if (i == 2)
            {
                data[2] = a0 * dataCopy[2] + a1 * dataCopy[0] + a2 * oldX[2] - b1 * data[0] - b2 * oldY[2];
            }

            if (channels == 2)
            {
                data[i + 1] = data[i];
            }
        }
    }
}