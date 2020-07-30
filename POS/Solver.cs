using System.Collections.Generic;

namespace POSProgram
{
    class Solver
    {
        readonly int p = MDIParent.p;
        readonly int z = MDIParent.z;

        public void matrix(float[][] day, ref float[][] mas, ref List<float[]> cp)
        {
            mas[0] = new float[z + 1];
            for (int j = 0; j < z; j++)
                mas[0][j + 1] = mas[0][j] + day[0][j];
            for (int i = 1; i < p; i++)
            {
                mas[i] = new float[z + 1];
                for (int j = 0; j <= z; j++)
                {
                    if (day[i - 1][j + 1] <= day[i][j])
                    {
                        mas[i][j] = mas[i - 1][j + 1];
                        for (int g = j; g < z; g++)
                            mas[i][g + 1] = mas[i][g] + day[i][g];
                        for (int g = j; g >= 0; g--)
                            mas[i][g] = mas[i][g + 1] - day[i][g];
                        for (int g = 0; g < z; g++)
                            if (mas[i - 1][g + 1] > mas[i][g] && !(mas[i][g] == 0)) goto one;
                        for (int g = 0; g < z; g++)
                            if (mas[i][g] == mas[i - 1][g + 1]) cp.Add(new float[] { mas[i][g], g }); // j -> g
                        break;
                    }
                    one:
                    continue;
                }
            }
        }

        public float matrix(float[][] day)
        {
            float[][] mas = new float[p][];
            mas[0] = new float[z + 1];
            for (int j = 0; j < z; j++)
                mas[0][j + 1] = mas[0][j] + day[0][j];
            for (int i = 1; i < p; i++)
            {
                mas[i] = new float[z + 1];
                for (int j = 0; j <= z; j++)
                {
                    if (day[i - 1][j + 1] <= day[i][j])
                    {
                        mas[i][j] = mas[i - 1][j + 1];
                        for (int g = j; g < z; g++)
                            mas[i][g + 1] = mas[i][g] + day[i][g];
                        for (int g = j; g >= 0; g--)
                            mas[i][g] = mas[i][g + 1] - day[i][g];
                        for (int g = 0; g < z; g++)
                            if (mas[i - 1][g + 1] > mas[i][g] && !(mas[i][g] == 0)) goto one;
                        break;
                    }
                    one:
                    continue;
                }
            }
            return mas[p - 1][z];
        }

        public float workers(ref float[][] wm, float[][] mas) //wm[1] - рабочие; wm[2] - стоимость.
        {
            float[][] rs = new float[p * 2][]; //3
            float[] Ji = new float[p];
            float[] rab = new float[p];
            float temp;
            float w;

            for (int i = 0; i < p * 2; i++)
            {//заполнение массива с указанием процесса, его начала и окончания
                rs[i] = new float[3];
                if (i < p)
                {
                    rs[i][0] = mas[i][0];
                    rs[i][1] = i;
                    rs[i][2] = 1;
                }
                else
                {
                    rs[i][0] = mas[i - p][z];
                    rs[i][1] = i - p;
                    rs[i][2] = 0;
                }
            }
            for (int i = 0; i < p * 2; i++) //сортировка по первому столбцу (не более 198 элементов при 99)
                for (int j = 0; j < p * 2 - 1; j++)
                    if (rs[j][0] > rs[j + 1][0])
                        for (int g = 0; g < 3; g++)
                        {
                            temp = rs[j][g];
                            rs[j][g] = rs[j + 1][g];
                            rs[j + 1][g] = temp;
                        }
            for (int i = 0; i < p * 2; i++) wm[0][i] = rs[i][0];
            for (int i = 0; i < p; i++)
            {
                Ji[i] = wm[2][i] / (mas[i][z] - mas[i][0]);
                rab[i] = wm[1][i];
            }
            w = 0;
            temp = 0;
            for (int i = 0; i < (p * 2 - 1); i++)
            {
                if (rs[i][2] == 1)
                    temp += rab[(int)rs[i][1]];
                else
                    temp -= rab[(int)rs[i][1]];
                wm[1][i] = temp;
                w += (wm[0][i + 1] - wm[0][i]) * temp;
            }
            wm[2][0] = 0;
            temp = 0;
            for (int i = 0; i < p * 2 - 1; i++)
            {
                if (rs[i][2] == 1)
                    temp += Ji[(int)rs[i][1]];
                else
                    temp -= Ji[(int)rs[i][1]];
                wm[2][i + 1] = wm[2][i] + (wm[0][i + 1] - wm[0][i]) * temp;
            }
            return (w / mas[p - 1][z]);
        }

        public void faster(ref float[][] day, ref int[] tarr)
        {
            int[] arr = new int[z];
            float a;
            float b = matrix(day);
            float[][] tday = new float[p][];

            for (int i = 0; i < z; i++) arr[i] = i;
            arr.CopyTo(tarr, 0);
            do
            {
                int k, j, l;
                for (j = arr.Length - 2; (j >= 0) && (arr[j] >= arr[j + 1]); j--) ;
                if (j == -1) break;
                for (l = arr.Length - 1; (arr[j] >= arr[l]) && (l >= 0); l--) ;
                var tmp = arr[j];
                arr[j] = arr[l];
                arr[l] = tmp;
                for (k = j + 1, l = arr.Length - 1; k < l; k++, l--)
                {
                    tmp = arr[k];
                    arr[k] = arr[l];
                    arr[l] = tmp;
                }

                for (int i = 0; i < p; i++)
                {
                    tday[i] = new float[z + 1];
                    for (int g = 0; g < z; g++)
                        tday[i][g] = day[i][arr[g]];
                }
                a = matrix(tday);
                if (a < b)
                {
                    b = a;
                    arr.CopyTo(tarr, 0);
                }
            } while (true);

            for (int i = 0; i < p; i++)
                for (int g = 0; g < z; g++)
                    tday[i][g] = day[i][tarr[g]];
            for (int i = 0; i < p; i++)
                for (int g = 0; g < z; g++)
                    day[i][g] = tday[i][g];
        }
    }
}
