using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Globalization;
using System.Collections;

namespace targem_test
{
    class Program
    {
        static char[] splitChars = new char[] { '%', '/', '*', '+', '-' };
        static char[] floatChars = new char[] { '.', ',' };

        /// <summary>
        /// </summary>
        /// <remarks> 
        /// Форматы для целого: -2*(-2)+2*-2+(-2/2%  2) <br/>
        /// Форматы для флота: -2.00*(-2)+2*-2+(-2/2%  2)
        /// </remarks>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            string input = "2/(3+4+(3+1))  -4*6f%20";
            input = input.Replace(',', '.');
            input = "2/(3+4+(3+1))  -4*6f%20 * (8+9)"; // F|f not supported
            input = "2*2*2+7+2*2+6/2*3";
            input = "(7+3+11+25*(7-9)+(8-1)*4/2*(1+3+(1+2)))";
            input = "(-2.010*(7+12)+ 2,00*2.000/2.02)";
            input = "2+1";

            while (true)
            {
                string read = Console.ReadLine();

                if (InputValidation(read))
                {
                    StringBuilder stringBuilder = new StringBuilder(read, read.Length);

                    InputStringFormat(stringBuilder);

                    bool isFloat = false;

                    foreach (var charr in read)
                    {
                        if (floatChars.Contains(charr))
                        {
                            isFloat = true;

                            break;
                        }
                    }
                    try
                    {
                        if (isFloat)
                        {
                            Console.WriteLine("Введен флоат");
                            Console.WriteLine(StringMatchFloat(stringBuilder.ToString()) + "   Результат");
                        }
                        else
                        {
                            Console.WriteLine("Введен инт");
                            Console.WriteLine(StringMatchInt(stringBuilder.ToString()) + "   Результат");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            {
                StringBuilder stringBuilder = new StringBuilder(input, input.Length);

                InputStringFormat(stringBuilder);

                if (true)
                {
                    //TestInt(stringBuilder.ToString());
                    TestFloat(stringBuilder.ToString());
                }

                Console.ReadKey();
            }
        }
        /// <summary>
        /// Форматирование входной строки, замена некоторых символов для корректной работы
        /// </summary>
        /// <param name="stringBuilder"></param>
        public static void InputStringFormat(StringBuilder stringBuilder)
        {
            Regex regex = new Regex(@"(^[-]|[^\d\s)][-][\d]*)");

            string s = stringBuilder.ToString().Replace(',', '.');

            stringBuilder.Clear();

            stringBuilder.Append(s);

            var matches = regex.Matches(s);

            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];

                int index = m.Value.IndexOf('-');

                stringBuilder[index + m.Index] = '[';
            }
        }
        public static bool InputValidation(string input)
        {
            Regex regex = new Regex(@"(^[\d\s*+\/\-.,()%]+$)");

            return regex.IsMatch(input);
        }
        /// <summary>
        /// Разбор выражения на многочлены по маске. Поочередное решение каждого члена выражения
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StringMatchInt(string input)
        {
            int count = input.Count();

            StringBuilder stringBuilder = new StringBuilder(input, count);
            StringBuilder temp = new StringBuilder(count);

            string pattern = @"([(][+\-\/*%\w\s\d\[\]]*[)])";

            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            bool isMatch = reg.IsMatch(stringBuilder.ToString());

            int depthLimit = 100;
            int depthLimitCur = depthLimit;

            while (isMatch)
            {
                depthLimitCur--;

                if (depthLimitCur < 0) throw new FormatException($"Невозможно проанализировать строку за {depthLimit} ходов.{Environment.NewLine}{input}");

                Console.WriteLine($"{input} Input");

                var matches = reg.Matches(input);

                int countGp = matches.Count;

                for (int i = 0; i < countGp; i++)
                {
                    Group group = matches[i];

                    string val = group.Value;

                    int index = group.Index;

                    int replace = ExpressionCalculateInt(val.Trim('(', ')'));

                    string s = replace > 0 ? replace.ToString() : $"[{(replace * -1).ToString()}";

                    int diff = val.Length - s.Length;

                    Console.WriteLine($"{val}=>{replace} {i} diff {diff} index {index} repl length:{s.Length} val Lengh{val.Length}");

                    for (int j = 0; j < s.Length; j++)
                    {
                        stringBuilder[group.Index + j] = s[j];
                    }
                    for (int j = 0; j < diff; j++)
                    {
                        stringBuilder[group.Index + s.Length + j] = ' ';
                    }
                }
                Console.WriteLine($"{stringBuilder} Replaced{Environment.NewLine}");

                isMatch = reg.IsMatch(input = stringBuilder.ToString());
            }

            input = ExpressionCalculateInt(input).ToString().Replace('[', '-');

            return input.Replace('[', '-');
        }
        public static string StringMatchFloat(string input)
        {
            int count = input.Count();

            StringBuilder stringBuilder = new StringBuilder(input, count);
            StringBuilder temp = new StringBuilder(count);

            string pattern = @"([(][,.+\-\/*f\w\s\d\[\]]*[)])";

            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            bool isMatch = reg.IsMatch(stringBuilder.ToString());

            int depthLimit = 100;
            int depthLimitCur = depthLimit;

            while (isMatch)
            {
                depthLimitCur--;

                if (depthLimitCur < 0) throw new FormatException($"Невозможно проанализировать строку за {depthLimit} ходов.{Environment.NewLine}{input}");

                var matches = reg.Matches(input);

                int countGp = matches.Count;

                for (int i = 0; i < countGp; i++)
                {
                    Group group = matches[i];

                    string val = group.Value;

                    int index = group.Index;

                    float replace = ExpressionCalculateFloat(val.Trim('(', ')'));

                    string s = replace > 0 ? replace.ToString("F2", CultureInfo.InvariantCulture) : $"[{(replace * -1).ToString("F2", CultureInfo.InvariantCulture)}";

                    //string s = replace.ToString();

                    int diff = val.Length - s.Length;

                    Console.WriteLine($"{val}=>{replace} {i} diff {diff} index {index} repl length:{s.Length} val Lengh{val.Length}");

                    //stringBuilder.Replace(val, s, group.Index, s.Length);

                    for (int j = 0; j < s.Length; j++)
                    {
                        stringBuilder[group.Index + j] = s[j];
                    }
                    for (int j = 0; j < diff; j++)
                    {
                        stringBuilder[group.Index + s.Length + j] = ' ';
                    }
                }
                Console.WriteLine($"{stringBuilder} Replaced{Environment.NewLine}");

                isMatch = reg.IsMatch(input = stringBuilder.ToString());
            }

            input = ExpressionCalculateFloat(input).ToString("F2", CultureInfo.InvariantCulture).Replace('[', '-');

            return input.Replace('[', '-');
        }
        public static int ExpressionCalculateInt(string val)
        {
            string[] strSplits = val.Split(splitChars);

            RestoreSign(val, strSplits);

            checked
            {
                Span<int> operands = stackalloc int[strSplits.Length];

                for (int i = 0; i < strSplits.Length; i++)
                {
                    if (!int.TryParse(strSplits[i], out operands[i]))
                    //if (!int.TryParse((ReadOnlySpan<char>)strSplits[i], NumberStyles.Integer | NumberStyles.AllowParentheses | NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out operands[i]))
                    {
                        throw new ArgumentException($"Не удалось преобразовать строку в целое. фрагмент конвертации:{strSplits[i]}{Environment.NewLine} Arg: {val}");
                    }
                }

                Span<int> completedOperandIndexes = stackalloc int[operands.Length];

                int completeOpeIndCount = default;

                int sum = default;
                int mul = default;
                // последовталеьное умножение?
                bool isBreakMul = true;
                int operatorIndex = default;
                // сначала умножение и деление
                for (int i = 0; i < val.Length; i++)
                {
                    if (splitChars.Contains(val[i]))
                    {
                        switch (val[i])
                        {
                            case '*':
                                {
                                    if (isBreakMul)
                                    {
                                        mul = operands[operatorIndex] * operands[operatorIndex + 1];
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex;
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex + 1;
                                        isBreakMul = false;
                                    }
                                    else
                                    {
                                        mul *= operands[operatorIndex + 1];
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex + 1;
                                    }
                                    break;
                                }
                            case '/':
                                {
                                    if (isBreakMul)
                                    {
                                        if (operands[operatorIndex + 1] == 0)
                                            throw new DivideByZeroException($"CharIndex: {i}. Деление на ноль.{Environment.NewLine}Arg: {val}");
                                        mul = operands[operatorIndex] / operands[operatorIndex + 1];
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex;
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex + 1;
                                        isBreakMul = false;
                                    }
                                    else
                                    {
                                        mul /= operands[operatorIndex + 1];
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex + 1;
                                    }
                                    break;
                                }
                            case '%':
                                {
                                    if (isBreakMul)
                                    {
                                        if (operands[operatorIndex + 1] == 0)
                                            throw new DivideByZeroException($"CharIndex: {i}. Деление на ноль.{Environment.NewLine}Arg: {val}");
                                        mul = operands[operatorIndex] % operands[operatorIndex + 1];
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex;
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex + 1;
                                        isBreakMul = false;
                                    }
                                    else
                                    {
                                        mul %= operands[operatorIndex + 1];
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex + 1;
                                    }
                                    break;
                                }
                            default:
                                isBreakMul = true;
                                sum += mul;
                                mul = default;
                                break;
                        }
                        operatorIndex++;
                    }
                }

                sum += mul;
                //  затем сложение вычитание
                for (int i = 0; i < operands.Length; i++)
                {
                    bool isMatch = false;

                    for (int j = 0; j < completeOpeIndCount; j++)
                    {
                        if (i == completedOperandIndexes[j])
                        {
                            isMatch = true;
                            break;
                        }
                    }
                    if (!isMatch) sum += operands[i];
                }
                return sum;
            }
        }
        public static float ExpressionCalculateFloat(string val)
        {
            string[] strSplits = val.Split(splitChars);

            RestoreSign(val, strSplits);

            checked
            {
                Span<float> operands = stackalloc float[strSplits.Length];

                for (int i = 0; i < strSplits.Length; i++)
                {
                    if (!float.TryParse((ReadOnlySpan<char>)strSplits[i], NumberStyles.Float | NumberStyles.AllowParentheses | NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out operands[i]))
                    {
                        throw new ArgumentException($"Не удалось преобразовать строку в д. фрагмент конвертации:{strSplits[i]}{Environment.NewLine} Arg: {val}");
                    }
                }

                Span<int> completedOperandIndexes = stackalloc int[operands.Length];

                int completeOpeIndCount = default;

                float sum = default;
                float mul = default;
                // последовталеьное умножение?
                bool isBreakMul = true;
                int operatorIndex = default;
                // сначала умножение и деление
                for (int i = 0; i < val.Length; i++)
                {
                    if (splitChars.Contains(val[i]))
                    {
                        switch (val[i])
                        {
                            case '*':
                                {
                                    if (isBreakMul)
                                    {
                                        mul = MulFloat(operands[operatorIndex], operands[operatorIndex + 1]);
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex;
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex + 1;
                                        isBreakMul = false;
                                    }
                                    else
                                    {
                                        mul = MulFloat(mul, operands[operatorIndex + 1]);
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex + 1;
                                    }
                                    break;
                                }
                            case '/':
                                {
                                    if (isBreakMul)
                                    {
                                        mul = DivFloat(operands[operatorIndex], operands[operatorIndex + 1]);
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex;
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex + 1;
                                        isBreakMul = false;
                                    }
                                    else
                                    {
                                        mul = DivFloat(mul, operands[operatorIndex + 1]);
                                        completedOperandIndexes[completeOpeIndCount++] = operatorIndex + 1;
                                    }
                                    break;
                                }
                            default:
                                isBreakMul = true;
                                sum += mul;
                                mul = default;
                                break;
                        }
                        operatorIndex++;
                    }
                }

                sum += mul;
                //  затем сложение вычитание
                for (int i = 0; i < operands.Length; i++)
                {
                    bool isMatch = false;

                    for (int j = 0; j < completeOpeIndCount; j++)
                    {
                        if (i == completedOperandIndexes[j])
                        {
                            isMatch = true;
                            break;
                        }
                    }
                    if (!isMatch) sum = SumFloat(sum, operands[i]);
                }
                return sum;
            }
        }
        public static void RestoreSign(string origin, string[] split)
        {
            int operationCount = origin.Count(x => splitChars.Contains(x));

            int operationCurent = default;

            for (int i = 0; i < origin.Length; i++)
            {
                if (splitChars.Contains(origin[i]))
                {
                    if (origin[i] == '-')
                    {
                        if (split.Length > 1)
                        {
                            split[operationCurent + 1] = $"-{split[operationCurent + 1]}";
                        }
                        else
                        {
                            split[0] = $"-{split[0]}";
                        }
                    }
                    operationCurent++;
                }
            }

            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Contains('['))
                {
                    split[i] = $"-{split[i].Trim('[')}";
                }
            }
        }
        public static bool InputCheckInt(string input)
        {
            bool result = true;

            int countBracLeft = input.Count(x => x == '(');
            int countBracRigt = input.Count(x => x == ')');

            result |= countBracLeft == countBracRigt;
            result |= Regex.IsMatch(input, @"(^[\d*\\\-%+\(\)]+$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return result;
        }
        public static bool TestInt(string expr)
        {
            DataTable dt = new DataTable();

            string arg = expr.Trim(' ').Replace('[', '-');

            var v = dt.Compute(arg, "");

            //string res = ExpressionCalculateInt(expr).ToString();
            string res = StringMatchInt(expr);

            Console.WriteLine($"{v.ToString()} ARG {arg} Ethalon {Environment.NewLine}{res} Result");

            return v.ToString() == res;
        }
        public static bool TestFloat(string expr)
        {
            DataTable dt = new DataTable();

            string arg = expr.Trim(' ').Replace('[', '-').Replace(',', '.');

            var v = dt.Compute(arg, "");

            //string res = ExpressionCalculateInt(expr).ToString();
            string res = StringMatchFloat(expr);

            Console.WriteLine($"{Convert.ToSingle(v).ToString("F2", CultureInfo.InvariantCulture)} ARG {arg} Ethalon {Environment.NewLine}{res} Result");

            return Convert.ToSingle(v).ToString("F2") == res;
        }
        private static float MulFloat(float left, float right)
        {
            float result = left * right;

            if (float.IsNaN(result))
                throw new InvalidOperationException($"Результат флота - не число left {left} right {right}");
            if (float.IsInfinity(result))
                throw new InvalidOperationException($"Не хвтило точности. left {left} right {right}");

            return result;
        }
        private static float DivFloat(float left, float right)
        {
            float result = left * right;

            if (float.IsNaN(result))
                throw new InvalidOperationException($"Результат флота - не число left {left} right {right}");
            if (float.IsInfinity(result))
                throw new InvalidOperationException($"Не хвтило точности. left {left} right {right}");

            return left / right;
        }
        private static float SumFloat(float left, float right)
        {
            float result = left * right;

            if (float.IsNaN(result))
                throw new InvalidOperationException($"Результат флота - не число left {left} right {right}");
            if (float.IsInfinity(result))
                throw new InvalidOperationException($"Не хвтило точности. left {left} right {right}");

            return left + right;
        }
    }

    public class MyList<T> : IList<T>
    {
        private T[] chunk;
        public int maxCapacity;

        public MyList(int size = default)
        {
            chunk = new T[size];

            maxCapacity = size;

            Count = default;
        }
        public T this[int index]
        {
            get
            {
                if (-1 < index && index < chunk.Length)
                {
                    return chunk[index];
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                /// <see cref="IsReadOnly"/>
                if (IsReadOnly) throw new AccessViolationException("Is ReadOnly = true");

                if (-1 < index && index < chunk.Length)
                {
                    chunk[index] = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
        public int Count { get; set; }
        public bool IsReadOnly { get; set; } = false;

        public void Add(T item)
        {
            ResizeCheck();

            chunk[Count] = item;

            Count++;
        }

        public void ResizeCheck()
        {
            if (Count + 1 > maxCapacity)
            {
                maxCapacity *= 2;

                T[] newChunk = new T[maxCapacity];

                chunk.CopyTo(newChunk, 0);

                chunk = newChunk;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++)
            {
                chunk[i] = default;
            }
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (chunk[i].Equals(item)) return true;
            }

            return false;
        }
        /// <summary>
        /// Shallow copy
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = chunk[i];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new MyEnum<T>(chunk, Count);
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (chunk[i].Equals(item)) return i;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            ResizeCheck();

            Count++;

            T prev = default;

            prev = chunk[index];

            chunk[index] = item;

            for (int i = index + 1; i < Count; i++)
            {
                T temp = chunk[i];

                chunk[i] = prev;

                prev = temp;
            }
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);

            RemoveAt(index);
            
            return index > -1;
        }

        public void RemoveAt(int index)
        {
            if (index > -1)
            {
                int moveCount = Count - index - 1;
                // shift right to left
                for (int i = 0; i < moveCount; i++)
                {
                    chunk[i + index] = chunk[i + index + 1];
                }
                Count--;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class MyEnum<T> : IEnumerator<T>
    {
        public T Current { get; set; }

        private int index;
        private int indexMax;

        private T[] chunk;

        object IEnumerator.Current { get; }

        public MyEnum(T[] chunkIn, int max)
        {
            chunk = chunkIn;
            index = default;
            indexMax = max;
        }
        public void Dispose()
        {
            this.Dispose();
        }

        public bool MoveNext()
        {
            index++;

            bool isContinue = index < indexMax;

            if (isContinue)
            {
                Current = chunk[index];
            }
            else
            {
                Reset();
            }

            return isContinue;
        }

        public void Reset()
        {
            index = default;
            Current = chunk[index];
        }
    }
}
