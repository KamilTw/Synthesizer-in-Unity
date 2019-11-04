public class LPFilter : Filter
{
    public new void UpdateFilterValues(float cutoff, float q)
    {
        base.UpdateFilterValues(cutoff, q);

        a0 = 0.5f * (1 - c) * r;
        a1 = (1 - c) * r;
        a2 = a0;
        b1 = -2 * c * r;
        b2 = (1 - alfa) * r;
    }
}