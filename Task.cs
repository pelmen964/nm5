using System;
using System.IO;
using System.Text;
using AngouriMath;
using AngouriMath.Extensions;

namespace nm5
{
    public class Task
    {
        /*
         *  
         *
         */

        #region TaskVariables

        private readonly string _outFileName = "TaskOutput.txt";
        private readonly int _derivate;
        private readonly int _polyDegree;
        private readonly bool _gridType;
        private readonly double[] _xGridNodes;
        private readonly double[] _yGridNodes;
        private readonly int _intervalsCount;
        private readonly double[] _xResultGridNodes;
        private readonly bool _funcType;
        private readonly string _func;
        

        #endregion

        #region OtherVariables

        private readonly double[][] _frobeniusMatrix;

        #endregion

        public Task(string inputFileName)
        {
            int i;
            using (StreamReader fstream = new StreamReader(inputFileName))
            {
                _derivate = int.Parse(fstream.ReadLine() ?? "0");
                _polyDegree = int.Parse(fstream.ReadLine() ?? "0");
                _gridType = fstream.ReadLine() == "равн" ? true : false;

                Converter<string, double> parseStringToDouble = str => double.Parse(str);
                if (_gridType)
                {
                    _xGridNodes = Array.ConvertAll(fstream.ReadLine().Split(), parseStringToDouble);
                    _yGridNodes = Array.ConvertAll(fstream.ReadLine().Split(), parseStringToDouble);
                    fstream.ReadLine();
                    fstream.ReadLine();
                }
                else
                {
                    fstream.ReadLine();
                    fstream.ReadLine();
                    _xGridNodes = Array.ConvertAll(fstream.ReadLine().Split(), parseStringToDouble);
                    _yGridNodes = Array.ConvertAll(fstream.ReadLine().Split(), parseStringToDouble);
                }

                _intervalsCount = int.Parse(fstream.ReadLine() ?? "0");
                _xResultGridNodes = Array.ConvertAll(fstream.ReadLine().Split(), parseStringToDouble);
                _funcType = fstream.ReadLine() == "+" ? true : false;
                _func = fstream.ReadLine();

            }
        }

        public void Interpolate()
        {
            // https://ru.wikipedia.org/wiki/Интерполяционные_формулы_Ньютона
            // https://ru.wikipedia.org/wiki/Разделённая_разность 
            // Таблица конечных разностей
            double[,] finiteDifferenceTable = new double[_xGridNodes.Length,_xGridNodes.Length+1];
            for (int i = 0; i < _xGridNodes.Length; i++)
            {
                finiteDifferenceTable[i, 0] = _xGridNodes[i];
                finiteDifferenceTable[i, 1] = _yGridNodes[i];
            }
            for (int i = 1; i < _xGridNodes.Length; i++) // Бегу по столбцам 
            {
                for (int j = 0; j < _xGridNodes.Length-1; j++) // Бегу по строкам
                {
                    finiteDifferenceTable[j, i+1] = (finiteDifferenceTable[j + 1, i] - finiteDifferenceTable[j, i]);
                    
                }
            }
            // TODO: Написать полином
            var sb = new StringBuilder();
            for (int i = 0; i < _xGridNodes.Length; i++)
            {
                double elem = (finiteDifferenceTable[0, i + 1] /
                               (finiteDifferenceTable[i, 0] - finiteDifferenceTable[0, 0]));
                sb.Append(
                    $"x^{i}*{(double.IsInfinity(elem) ? 0.0 : elem)}+");
            }
            
            Console.Out.WriteLine(sb.ToString());
        }
    }
}