using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
    public double frequency;
    public float sinGain;
    public float squareGain;
    public float triangleGain;
    public float sawtoothGain;

    private double samplingFrequency = 48000.0;
    private double phase;
    public float time;
    private int t = 0;
    private Envelope envelope;

    void Update()
    {
        time = Time.time;
    }

    public void NoteOn()
    {
        envelope.NoteOn(time);
    }

    public void NoteOff()
    {
        envelope.NoteOff(time);
    }

    public void SetEnvelope(Envelope envelope)
    {
        this.envelope = envelope;
    }

    public float GenerateSignal(float lfoFreq, float lfoAmpSlider)
    {
        float data;
        double increment;
        t++;

        increment = frequency * 2.0 * Mathf.PI / samplingFrequency;
        phase += increment;
        //phase = increment * t;
        
        data = GenerateWave(phase, lfoFreq, lfoAmpSlider);

        return data;
    }

    float GenerateWave(double phase, float lfoFreq, float lfoAmpSlider)
    {
        float data = 0;
        int activeOscCounter = 0;

        // Calculated sin (with LFO modulation if lfoFreq and lfoAmpSlider are not zeros)
        float calculatedSin = Mathf.Sin((float)phase + lfoAmpSlider * (float)frequency * Mathf.Sin(lfoFreq * t));

        if (sinGain > 0.0f)
        {
            data += sinGain * calculatedSin;
            activeOscCounter++;
        }
        if (squareGain > 0.0f)
        {
            if(calculatedSin > 0)
            {
                 data += squareGain;
            }
            else
            {
                 data += -squareGain;
            }
            activeOscCounter++;
        }
        if (triangleGain > 0.0f)
        {
            data += triangleGain * Mathf.Asin(calculatedSin) * 2.0f / Mathf.PI;
            activeOscCounter++;
        }
        if (sawtoothGain > 0.0f)
        {
            //data += gain * (2.0f / Mathf.PI) * ((float)phase * Mathf.PI * ((float)deltaTime * 7.5f % (1.0f / (float)phase)) - (Mathf.PI / 2.0f));
            //data += gain * ((float)phase - Mathf.Floor((float)phase));
            //data += gain * ((float)phase % 1.0f);
            float d = 0;
            for (int n = 1; n < 20; n++)
            {
                d += (Mathf.Sin(n * (float)phase + lfoAmpSlider * (float)frequency * Mathf.Sin(lfoFreq * t))) / n;
            }
            data += d * sawtoothGain * (2.0f / Mathf.PI);
            activeOscCounter++;
        }

        return (float)envelope.GetAmplitude(time) * data / activeOscCounter;
    }
}