using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filter : MonoBehaviour
{
    // Filters values
    //private float[] dataCopy = new float[2048];     // Current frame (array) copy
    //private float[] oldY = new float[4];            // Old frame last samples after filtering
    //private float[] oldX = new float[4];            // Old frame last samples before filtering

    protected float s, c, alfa, r;
    protected float a0, a1, a2, b1, b2;

    public void ExecuteFilter(ref float[] data, int channels, float[] dataCopy, float[] oldY, float[] oldX)
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

    public void UpdateFilterValues(float cutoff, float q)
    {
        s = Mathf.Sin(cutoff * 2.0f * Mathf.PI / 48000);
        c = Mathf.Cos(cutoff * 2.0f * Mathf.PI / 48000);

        alfa = s / (2 * q);
        r = 1 / (1 + alfa);
    }
}