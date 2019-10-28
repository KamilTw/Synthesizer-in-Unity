public struct Envelope
{
    public double attackTime;
    public double decayTime;
    public double releaseTime;

    public double sustainAmplitude;
    public double maxAmplitude;

    public double triggerOnTime;
    public double triggerOffTime;

    public bool noteOn;

    public double GetAmplitude(double time)
    {
        double amplitude = 0.0;
        double lifeTime = time - triggerOnTime;

        if (noteOn)
        {
            //ads

            // Attack
            if (lifeTime <= attackTime)
            {
                amplitude = (lifeTime / attackTime) * maxAmplitude;
            }

            // Decay
            if (lifeTime > attackTime && lifeTime <= (attackTime + decayTime))
            {
                amplitude = ((lifeTime - attackTime) / decayTime) * (sustainAmplitude - maxAmplitude) + maxAmplitude;
            }

            // Sustain
            if (lifeTime > (attackTime + decayTime))
            {
                amplitude = sustainAmplitude;
            }
        }
        else
        {
            // Release
            amplitude = ((time - triggerOffTime) / releaseTime) * (0.0 - sustainAmplitude) + sustainAmplitude;
        }

        if (amplitude <= 0.0001)
        {
            amplitude = 0;
        }

        return amplitude;
    }

    public void NoteOn(double time)
    {
        triggerOnTime = time;
        noteOn = true;
    }

    public void NoteOff(double time)
    {
        triggerOffTime = time;
        noteOn = false;
    }
}