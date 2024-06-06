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
        private readonly string _func;
        

        #endregion

        // #region OtherVariables
        // #endregion

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
                _gridType = fstream.ReadLine() == "равн";

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
                _funcType = fstream.ReadLine() == "+";
                _func = fstream.ReadLine();

            }

            string outStr = Interpolate();
            Console.Out.WriteLine(outStr);
            string cringeOut = "C:/Users/ivan/RiderProjects/nm5/";
            using (StreamWriter sw = new StreamWriter(cringeOut+_outFileName))
            {
                sw.WriteLine(outStr);
            }

        }

        public string Interpolate()
        {   
            
            // https://ru.wikipedia.org/wiki/Интерполяционные_формулы_Ньютона
            // https://ru.wikipedia.org/wiki/Разделённая_разность 
            var funcSb = new StringBuilder();
            var outSb = new StringBuilder();
            if (_gridType) // Равномерная сетка
            {
                // Таблица конечных разностей
                double[,] diffTable = new double[_xGridNodes.Length,_xGridNodes.Length+1];
                for (int i = 0; i < _xGridNodes.Length; i++)
                {
                    diffTable[i, 0] = _xGridNodes[i];
                    diffTable[i, 1] = _yGridNodes[i];
                }

                for (int i = 1; i < _xGridNodes.Length; i++) // Бегу по столбцам 
                {
                    for (int j = 0; j < _xGridNodes.Length-i; j++) // Бегу по строкам
                    {
                        diffTable[j, i+1] = (diffTable[j + 1, i] - diffTable[j, i]);
                    
                    }
                }
                Func<int, string> q = i => $"x - {i}";
                double res;
                double accuracy;
                switch (_derivate) // Производная
                {
                    case 0: 
                        
                        for (int i = 0; i < _xGridNodes.Length; i++)
                        {
                    
                            funcSb.Append(
                                $"(({diffTable[0,i+1]})/({i}!)");
                            for (int j = 0; j < i; j++)
                            {
                                funcSb.Append($"*({q(j)})");
                            }

                            funcSb.Append(")+");
                        }
                        if (funcSb[funcSb.Length-1] == '+')
                        {
                            funcSb.Remove(funcSb.Length - 1, 1);
                        }

                        string func = funcSb.ToString().Replace(',', '.');
                        accuracy = 0;
                        outSb.AppendLine("X\tY");
                        for (int i = 0; i < _xResultGridNodes.Length; i++)
                        {
                            res = F(func, _xResultGridNodes[i]);
                            accuracy += Math.Pow(res,2) - (Math.Pow( F(_func, _xResultGridNodes[i]),2));
                            outSb.AppendLine($"{_xResultGridNodes[i]}\t{res}");
                        }

                        if (_funcType)
                        {
                            outSb.AppendLine($"{Math.Pow(accuracy,0.5)}");
                        }
                        break;
                    case 1:
                        funcSb.Append($"1/{_xGridNodes.Length}*(");
                        for (int i = 1; i < _xGridNodes.Length; i++)
                        {
                    
                            funcSb.Append(
                                $"(({diffTable[0,i+1]})/({i}!)");
                            if (i>1)
                            {
                                funcSb.Append("*(");
                            }
                            for (int j = 0; j < i; j++)
                            {
                                
                                for (int k = 0; k < i; k++)
                                {
                                    if (k == j)
                                        continue;
                                    funcSb.Append($"({q(k)})*");
                                }
                                if (i>2)
                                {
                                    if (funcSb[funcSb.Length-1] == '*')
                                    {
                                        funcSb.Remove(funcSb.Length - 1, 1);
                                    }
                                    funcSb.Append("+");
                                }
                            }
                            if (funcSb[funcSb.Length-1] == '+')
                            {
                                funcSb.Remove(funcSb.Length - 1, 1);
                            }
                            if (funcSb[funcSb.Length-1] == '*')
                            {
                                funcSb.Remove(funcSb.Length - 1, 1);
                            }
                            if (i>1)
                            {
                                funcSb.Append(")");
                            }
                            funcSb.Append(")+");
                        }
                        funcSb.Append(")");
                        func = funcSb.ToString().Replace(',', '.').Replace("+)", ")");
                        accuracy = 0;
                        for (int i = 0; i < _xResultGridNodes.Length; i++)
                        {
                            res = F(func, _xResultGridNodes[i]);
                            accuracy += Math.Pow(res,2) - (Math.Pow( F(_func, _xResultGridNodes[i]),2));
                            outSb.AppendLine($"{_xResultGridNodes[i]} {res}");
                        }

                        if (_funcType)
                        {
                            outSb.AppendLine($"{Math.Pow(accuracy,0.5)}");
                        }
                        break;
                    case 2:
                        funcSb.Append($"1/{Math.Pow(_xGridNodes.Length,2)}*(");
                        for (int i = 2; i < _xGridNodes.Length; i++)
                        {
                    
                            funcSb.Append(
                                $"(({diffTable[0,i+1]})/({i}!)");
                            if (i>1)
                            {
                                funcSb.Append("*(");
                            }
                            for (int j = 0; j < i; j++)
                            {

                                for (int k = 0; k < i; k++)
                                {
                                    if (k == j)
                                        continue;
                                    for (int l = 0; l < i; l++)
                                    {
                                        if (l == j)
                                            continue;
                                        if (l == k)
                                            continue;
                                        funcSb.Append($"({q(l)})*");
                                    }
                                    if (i>2)
                                    {
                                        if (funcSb[funcSb.Length-1] == '*')
                                        {
                                            funcSb.Remove(funcSb.Length - 1, 1);
                                        }
                                        funcSb.Append("+");
                                    }
                                }
                                if (i>2)
                                {
                                    if (funcSb[funcSb.Length-1] == '*')
                                    {
                                        funcSb.Remove(funcSb.Length - 1, 1);
                                    }
                                    funcSb.Append("+");
                                }
                                
                            }
                            if (funcSb[funcSb.Length-1] == '+')
                            {
                                funcSb.Remove(funcSb.Length - 1, 1);
                            }
                            if (funcSb[funcSb.Length-1] == '*')
                            {
                                funcSb.Remove(funcSb.Length - 1, 1);
                            }
                            if (i>1)
                            {
                                funcSb.Append(")");
                            }
                            funcSb.Append(")+");
                        }
                        funcSb.Append(")");
                        func = funcSb.ToString().Replace(',', '.').Replace("+)", ")");
                        func = func.Replace("()", "").Replace("*)",")").Replace("++","+").Replace("+)",")");
                       
                        accuracy = 0;
                        for (int i = 0; i < _xResultGridNodes.Length; i++)
                        {
                            res = F(func, _xResultGridNodes[i]);
                            accuracy += Math.Pow(res,2) - (Math.Pow( F(_func, _xResultGridNodes[i]),2));
                            outSb.AppendLine($"{_xResultGridNodes[i]} {res}");
                        }

                        if (_funcType)
                        {
                            outSb.AppendLine($"{Math.Pow(accuracy,0.5)}");
                        }
                        break;
                }
                
            }
            else // Сетка неравномерная
            {
                // Таблица разделённых разностей
                // Таблица конечных разностей
                double[,] diffTable = new double[_xGridNodes.Length,_xGridNodes.Length+1];
                for (int i = 0; i < _xGridNodes.Length; i++)
                {
                    diffTable[i, 0] = _xGridNodes[i];
                    diffTable[i, 1] = _yGridNodes[i];
                }

                for (int i = 1; i < _xGridNodes.Length; i++) // Бегу по столбцам 
                {
                    for (int j = 0; j < _xGridNodes.Length-i; j++) // Бегу по строкам
                    {
                        diffTable[j, i+1] = (diffTable[j + 1, i] - diffTable[j, i])/(diffTable[j+i, 0] - diffTable[j, 0]);
                    
                    }
                    
                }
                Func<int, string> q = i => $"x - {diffTable[i,0]}";
                double accuracy;
                double res;
                string func;
                switch (_derivate)
                {
                    case 0:
                        for (int i = 0; i < _xGridNodes.Length; i++)
                        {
                    
                            funcSb.Append(
                                $"(({diffTable[0,i+1]})");
                            for (int j = 0; j < i; j++)
                            {
                                funcSb.Append($"*({q(j)})");
                            }

                            funcSb.Append(")+");
                        }

                        func = funcSb.ToString().Replace(',', '.').TrimEnd('+');
                       
                        accuracy = 0;
                        for (int i = 0; i < _xResultGridNodes.Length; i++)
                        {
                            res = F(func, _xResultGridNodes[i]);
                            accuracy += Math.Pow(res,2) - (Math.Pow( F(_func, _xResultGridNodes[i]),2));
                            outSb.AppendLine($"{_xResultGridNodes[i]} {res}");
                        }

                        if (_funcType)
                        {
                            outSb.AppendLine($"{Math.Pow(accuracy,0.5)}");
                        }
                        Console.Out.WriteLine(outSb.ToString());
                        break;
                    case 1:
                        funcSb.Append('(');
                        for (int i = 1; i < _xGridNodes.Length; i++)
                        {
                    
                            funcSb.Append(
                                $"(({diffTable[0,i+1]})");
                            if (i>1)
                            {
                                funcSb.Append("*(");
                            }
                            for (int j = 0; j < i; j++)
                            {
                                for (int k = 0; k < i; k++)
                                {
                                    if (k == j)
                                        continue;
                                    funcSb.Append($"({q(k)})*");
                                }
                                if (i>2)
                                {
                                    if (funcSb[funcSb.Length-1] == '*')
                                    {
                                        funcSb.Remove(funcSb.Length - 1, 1);
                                    }
                                    funcSb.Append("+");
                                }
                            }
                            if (funcSb[funcSb.Length-1] == '+')
                            {
                                funcSb.Remove(funcSb.Length - 1, 1);
                            }
                            if (funcSb[funcSb.Length-1] == '*')
                            {
                                funcSb.Remove(funcSb.Length - 1, 1);
                            }
                            if (i>1)
                            {
                                funcSb.Append(")");
                            }
                            funcSb.Append(")+");
                        }
                        funcSb.Append(")");
                        func = funcSb.ToString().Replace(',', '.').Replace("+)", ")");
                       
                        accuracy = 0;
                        for (int i = 0; i < _xResultGridNodes.Length; i++)
                        {
                            res = F(func, _xResultGridNodes[i]);
                            accuracy += Math.Pow(res,2) - (Math.Pow( F(_func, _xResultGridNodes[i]),2));
                            outSb.AppendLine($"{_xResultGridNodes[i]} {res}");
                        }

                        if (_funcType)
                        {
                            outSb.AppendLine($"{Math.Pow(accuracy,0.5)}");
                        }
                        break;
                    case 2:
                        funcSb.Append('(');
                        for (int i = 2; i < _xGridNodes.Length; i++)
                        {
                    
                            funcSb.Append(
                                $"(({diffTable[0,i+1]})");
                            if (i>1)
                            {
                                funcSb.Append("*(");
                            }
                            for (int j = 0; j < i; j++)
                            {

                                for (int k = 0; k < i; k++)
                                {
                                    if (k == j)
                                        continue;
                                    for (int l = 0; l < i; l++)
                                    {
                                        if (l == j)
                                            continue;
                                        if (l == k)
                                            continue;
                                        funcSb.Append($"({q(l)})*");
                                    }
                                    if (i>2)
                                    {
                                        if (funcSb[funcSb.Length-1] == '*')
                                        {
                                            funcSb.Remove(funcSb.Length - 1, 1);
                                        }
                                        funcSb.Append("+");
                                    }
                                }
                                if (i>2)
                                {
                                    if (funcSb[funcSb.Length-1] == '*')
                                    {
                                        funcSb.Remove(funcSb.Length - 1, 1);
                                    }
                                    funcSb.Append("+");
                                }
                                
                            }
                            if (funcSb[funcSb.Length-1] == '+')
                            {
                                funcSb.Remove(funcSb.Length - 1, 1);
                            }
                            if (funcSb[funcSb.Length-1] == '*')
                            {
                                funcSb.Remove(funcSb.Length - 1, 1);
                            }
                            if (i>1)
                            {
                                funcSb.Append(")");
                            }
                            funcSb.Append(")+");
                        }
                        funcSb.Append(")");
                        func = funcSb.ToString().Replace(',', '.').Replace("+)", ")");
                        func = func.Replace("()", "").Replace("*)",")").Replace("++","+").Replace("+)",")");
                       
                        accuracy = 0;
                        for (int i = 0; i < _xResultGridNodes.Length; i++)
                        {
                            res = F(func, _xResultGridNodes[i]);
                            accuracy += Math.Pow(res,2) - (Math.Pow( F(_func, _xResultGridNodes[i]),2));
                            outSb.AppendLine($"{_xResultGridNodes[i]} {res}");
                        }

                        if (_funcType)
                        {
                            outSb.AppendLine($"{Math.Pow(accuracy,0.5)}");
                        }
                        break;
                    
                }
                
                
            }
            return outSb.ToString();
        }
    }
}