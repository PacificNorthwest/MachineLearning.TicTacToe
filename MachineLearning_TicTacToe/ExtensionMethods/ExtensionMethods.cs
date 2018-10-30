using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearning_TicTacToe.ExtensionMethods
{
    static class ExtensionMethods
    {
        public static T[] GetRow<T>(this T[,] matrix, int index)
               => matrix.Cast<T>().Skip(matrix.GetLength(1) * index).Take(matrix.GetLength(1)).ToArray();

        public static T[] GetColumn<T>(this T[,] matrix, int index)
        {
            T[] array = new T[matrix.GetLength(0)];
            for (int i = 0; i < matrix.GetLength(0); i++)
                array[i] = matrix[i, index];
            return array;
        }

        public static T[] GetDiagonal<T>(this T[,] matrix, DiagonalType dType)
        {
            T[] array = new T[Math.Min(matrix.GetLength(0), matrix.GetLength(1))];
            int rowIterator = 0;
            int colIterator = (dType == DiagonalType.Left) ? 0 : (Math.Min(matrix.GetLength(0), matrix.GetLength(1)) - 1);
            while ((dType == DiagonalType.Left) ? colIterator < Math.Min(matrix.GetLength(0), matrix.GetLength(1)) : colIterator >=  0)
            {
                array[colIterator] = matrix[rowIterator, colIterator];
                rowIterator++;
                if (dType == DiagonalType.Left) colIterator++;
                else colIterator--;
            }
            return array;
        }
    }
}
