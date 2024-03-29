﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
    public double frequency;
    public float sinGain;
    public float squareGain;
    public float triangleGain;
    public float sawtoothGain;
    public float whiteNoiseGain;
    public float redNoiseGain;

    private double samplingFrequency = 48000.0;
    private double phase;
    private float time = 0;
    private Envelope envelope;
    private System.Random rand = new System.Random();
    private int redNoiseSampleCounter = 50;
    private float redNoiseFirstSample;
    private float redNoiseLastSample;

    private int skippedSamples = 15;

    public void NoteOn()
    {
        envelope.NoteOn(time / samplingFrequency);
    }

    public void NoteOff()
    {
        envelope.NoteOff(time / samplingFrequency);
    }

    public void SetEnvelope(Envelope envelope)
    {
        this.envelope = envelope;
    }

    public float GenerateSignal(float lfoFreq, float lfoAmpSlider)
    {
        float data;
        double increment;
        time++;

        increment = frequency * 2.0 * Mathf.PI / samplingFrequency;
        phase += increment;
        //phase = increment * time;
        
        data = GenerateWave(phase, lfoFreq, lfoAmpSlider);

        return data;
    }

    float GenerateWave(double phase, float lfoFreq, float lfoAmpSlider)
    {
        float data = 0;
        int activeOscCounter = 0;

        // Calculated sin (with LFO modulation if lfoFreq and lfoAmpSlider are not zeros)
        float calculatedSin = Mathf.Sin((float)phase + lfoAmpSlider * (float)frequency * Mathf.Sin(lfoFreq * time));

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
                d += (Mathf.Sin(n * (float)phase + lfoAmpSlider * (float)frequency * Mathf.Sin(lfoFreq * time))) / n;
            }
            data += d * sawtoothGain * (2.0f / Mathf.PI);
            activeOscCounter++;
        }
        if (whiteNoiseGain > 0.0f)
        {
            if (lfoAmpSlider == 0)
            {
                data += whiteNoiseGain * (float)(rand.NextDouble() * 2.0 - 1.0);
            }
            else
            {
                data += whiteNoiseGain * Mathf.Sin((float)(rand.NextDouble() * 2.0 - 1.0) + lfoAmpSlider * (float)frequency * Mathf.Sin(lfoFreq * time));
            }
            
            activeOscCounter++;
        }
        if (redNoiseGain > 0.0f)
        {
            float d = 0;
            if (redNoiseSampleCounter < skippedSamples)
            {
                // Interpolation
                d = redNoiseFirstSample * (1.0f - ((redNoiseSampleCounter - 1.0f) / (skippedSamples - 1.0f))) + redNoiseLastSample * ((redNoiseSampleCounter - 1.0f) / (skippedSamples - 1.0f));
                redNoiseSampleCounter++;
            }
            else
            {
                redNoiseFirstSample = redNoiseLastSample;              
                if (lfoAmpSlider == 0)
                {
                    redNoiseLastSample = (float)(rand.NextDouble() * 2.0 - 1.0);
                }
                else
                {
                    redNoiseLastSample = Mathf.Sin((float)(rand.NextDouble() * 2.0 - 1.0) + lfoAmpSlider * (float)frequency * Mathf.Sin(lfoFreq * time));
                }
                
                redNoiseSampleCounter = 2;

                d = redNoiseFirstSample;
            }

            data += redNoiseGain * d;
            activeOscCounter++;
        }

        return (float)envelope.GetAmplitude(time / samplingFrequency) * data / activeOscCounter;
    }
}