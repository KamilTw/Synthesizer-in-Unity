public class BPFilter : Filter
{
    public new void UpdateFilterValues(float cutoff, float q)
    {
        base.UpdateFilterValues(cutoff, q);

        a0 = alfa * r;
        a1 = 0;
        a2 = -a0;
        b1 = -2 * c * r;
        b2 = (1 - alfa) * r;
    }
}