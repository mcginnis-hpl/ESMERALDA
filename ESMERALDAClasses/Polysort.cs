namespace ESMERALDAClasses
{
    using System;

    internal class Polysort
    {
        private static void qsort<T>(Ordered<T>[] arr, int a, int b)
        {
            if (a < b)
            {
                int i = a;
                int j = b;
                Ordered<T> x = arr[(i + j) / 2];
                do
                {
                    while (arr[i].Less(x))
                    {
                        i++;
                    }
                    while (x.Less(arr[j]))
                    {
                        j--;
                    }
                    if (i <= j)
                    {
                        swap<Ordered<T>>(arr, i, j);
                        i++;
                        j--;
                    }
                }
                while (i <= j);
                qsort<T>(arr, a, j);
                qsort<T>(arr, i, b);
            }
        }

        public static void Quicksort<T>(Ordered<T>[] arr)
        {
            qsort<T>(arr, 0, arr.Length - 1);
        }

        private static void swap<T>(T[] arr, int s, int t)
        {
            T tmp = arr[s];
            arr[s] = arr[t];
            arr[t] = tmp;
        }
    }
}

