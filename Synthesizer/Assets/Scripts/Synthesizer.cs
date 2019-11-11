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

    [Header("Sound buttons")]
    public Button seaSound;

    [Header("LP cutoff lfo")]
    public Toggle lpCutoffLfoSwitch;
    public Slider lpCutoffLfoFreq;
    public Slider lpCutoffLfoAmp;
    public Slider lpCutoffLfoStartPoint;

    // Filters
    private LPFilter lpFilter = new LPFilter();
    private HPFilter hpFilter = new HPFilter();
    private BPFilter bpFilter = new BPFilter();

    // Filters values
    private float[] dataCopy = new float[2048];     // Current frame (array) copy
    private float[] oldY = new float[4];            // Old frame last samples after filtering
    private float[] oldX = new float[4];            // Old frame last samples before filtering

    private int timer = 0;

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

        // Buttons
        seaSound.onClick.AddListener(delegate { EnableSeaSound(); });
    }

    void Update()
    {
        LPCutoffLFOChange();
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
        lpFilter.UpdateFilterValues(lpCutoff.value, lpQ.value);
    }

    void HPFilterChange()
    {
        hpFilter.UpdateFilterValues(hpCutoff.value, hpQ.value);
    }

    void BPFilterChange()
    {
        bpFilter.UpdateFilterValues(bpCutoff.value, bpQ.value);
    }

    void LPCutoffLFOChange()
    {
        if (lpCutoffLfoSwitch.isOn)
        {
            lpCutoff.value = (Mathf.Sin(lpCutoffLfoFreq.value * timer) * lpCutoffLfoAmp.value) + lpCutoffLfoStartPoint.value;
        }
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
            lpFilter.ExecuteFilter(ref data, channels, dataCopy, oldY, oldX);          
        }
        if (hpCutoff.value != hpCutoff.minValue)
        {
            hpFilter.ExecuteFilter(ref data, channels, dataCopy, oldY, oldX);
        }
        if (bpCutoff.value != bpCutoff.minValue)
        {
            bpFilter.ExecuteFilter(ref data, channels, dataCopy, oldY, oldX);
        }

        oldY[0] = data[2044];
        oldY[1] = data[2045];
        oldY[2] = data[2046];
        oldY[3] = data[2047];

        oldX[0] = dataCopy[2044];
        oldX[1] = dataCopy[2045];
        oldX[2] = dataCopy[2046];
        oldX[3] = dataCopy[2047];

        // Needed to cutoff lfo
        timer++;
    }

    void EnableSeaSound()
    {
        sinSlider.value = 0;
        squareSlider.value = 0;
        triangleSlider.value = 0;
        sawtoothSlider.value = 0;
        whiteNoiseSlider.value = whiteNoiseSlider.maxValue;
        redNoiseSlider.value = 0.05f;


        lfoFreqSlider.value = 0.0000001f;
        lfoAmpSlider.value = lfoAmpSlider.maxValue;


        lpCutoff.value = 500;
        lpQ.value = 0.55f;

        hpCutoff.value = 0;
        hpQ.value = 0;

        bpCutoff.value = 0;
        bpQ.value = 0;

        lpCutoffLfoSwitch.isOn = true;
        lpCutoffLfoFreq.value = 0.02f;
        lpCutoffLfoAmp.value = 500;
        lpCutoffLfoStartPoint.value = 1200;       
    }
}