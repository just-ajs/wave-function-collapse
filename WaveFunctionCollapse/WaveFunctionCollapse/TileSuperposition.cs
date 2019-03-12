namespace WaveFunctionCollapse
{
    public class TileSuperposition
    {
        public int[] Superpositions;
        public int Sum;


        public TileSuperposition(int a, int b, int c)
        {
            Superpositions = new int[3];
            Superpositions[0] = a;
            Superpositions[1] = b;
            Superpositions[2] = c;

            Sum = a + b + c;
        }
    }
}