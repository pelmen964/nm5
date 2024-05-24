using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private readonly Entity _func;
        

        #endregion

        #region OtherVariables

        private readonly double[][] _frobeniusMatrix;

        #endregion

        public double F(string func,double x, uint der = 0)
        {
            Entity localExpr = func;
            var localDer = der;
            while (localDer > 0)
            {
                localExpr = localExpr.Differentiate("x");
                localDer -= 1;
            }

            return (double)(localExpr.Substitute("x", x).EvalNumerical());
            // return 0;
            // return _func.Substitute("x",x);
            //return PolStr.EvalPolStr(_pstr, x, der);
        }
        
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
            double[,] diffTable = new double[_xGridNodes.Length,_xGridNodes.Length+1];
            for (int i = 0; i < _xGridNodes.Length; i++)
            {
                diffTable[i, 0] = _xGridNodes[i];
                diffTable[i, 1] = _yGridNodes[i];
            }

            for (int i = 1; i < _xGridNodes.Length; i++) // Бегу по столбцам 
            {
                for (int j = 0; j < _xGridNodes.Length-1; j++) // Бегу по строкам
                {
                    diffTable[j, i+1] = (diffTable[j + 1, i] - diffTable[j, i]);
                    
                }
            }
            if (_gridType) // Равномерная сетка
            {
                var sb = new StringBuilder();
                Func<int, string> q = i => $"(x - {i})";
                for (int i = 0; i < _xGridNodes.Length; i++)
                {
                    
                    sb.Append(
                        $"(({diffTable[0,i+1]})/({i}!)");
                    for (int j = 0; j < i; j++)
                    {
                        sb.Append($"*({q(j)})");
                    }

                    sb.Append(")+");
                }
                Console.Out.WriteLine(sb.ToString().TrimEnd('+')); 
                Console.Out.WriteLine(F(sb.ToString().TrimEnd('+'),3,0));
            }
            else // Сетка неравномерная
            {
                
            }

            
            
           
        }
    }
}