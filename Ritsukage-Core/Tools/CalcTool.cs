using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Tools
{
    public static class CalcTool
    {
        enum Operator {
            ADD, SUB, MUL, DIV,
            LB, RB, MOD,
            POW
        }

        static int GetOperatorPriority(Operator op)
            => op switch {
                Operator.ADD or Operator.SUB => 9,
                Operator.MUL or Operator.DIV or Operator.MOD => 16,
                Operator.POW => 100,
                Operator.LB => 0,
                Operator.RB => 9961,
                _ => -1,
            };

        static double OpNum(Operator op, double a, double b)
            => op switch {
                Operator.ADD => a + b,
                Operator.SUB => a - b,
                Operator.MUL => a * b,
                Operator.DIV => a / b,
                Operator.MOD => a % b,
                Operator.POW => Math.Pow(a, b),
                _ => throw new ArgumentOutOfRangeException($"operator {op} is not suppported calc"),
            };

        static Operator? GetOperator(char c)
            => c switch {
                '+' => Operator.ADD,
                '-' => Operator.SUB,
                'x' or 'X' or '*' => Operator.MUL,
                '/' => Operator.DIV,
                '(' => Operator.LB,
                ')' => Operator.RB,
                '%' => Operator.MOD,
                '^' => Operator.POW,
                _ => null,
            };

        static double ParseNum(string n)
            => n.ToLower() switch {
                "pi" or "π" => Math.PI,
                "e" => Math.E,
                _ => double.Parse(n),
            };

        /// <summary>
        /// 从开始地点解析到第一个()结束的值
        /// </summary>
        /// <param name="expr">字符串表达式</param>
        /// <param name="idx">字符串表达式解析开始点</param>
        /// <returns>(表达式的值, 结束时的索引位置(包含))</returns>
        static (double, int) ParseUnitValue(string expr, int start) {
            int idx = start;
            int brackets = 0;
            bool internalParse = true;
            while (idx < expr.Length && internalParse) {
                var possibleOp = GetOperator(expr[idx]);
                if (possibleOp != null && possibleOp.HasValue) {
                    switch (possibleOp.Value) {
                        case Operator.LB:
                            brackets += 1;
                            break;
                        case Operator.RB:
                            brackets -= 1;
                            if (brackets == 0) {
                                internalParse = false;
                            }
                            break;
                    }
                }
                idx += 1;
            }
            double value = GetExprValue(expr[start..idx]);
            return (value, idx - 1);
        }
        /// <summary>	
        /// 解析简单的数学表达式，不保证结果，表达式有问题可能会丢未知异常	
        /// </summary>	
        /// <param name="expr">表达式 需要保证无空格</param>	
        /// <returns>表达式计算结果 不保证正确性</returns>
        public static double GetExprValue(string expr) {
            var nums = new Stack<double>();
            var ops = new Stack<Operator>();
            int brackets = 0;
            int idx = 0;
            int start = 0;
            while (idx < expr.Length) {
                char c = expr[idx];
                var op = GetOperator(c);
                //what is c# nullable mean??????
                //op != null mean the nullable is not null or has value?
                if (op != null && op.HasValue && !(idx > 1 && expr[idx - 1] == 'e' && char.IsDigit(expr[idx - 2]))) {
                    //maybe sqrt(.... or sth else
                    if (op.Value == Operator.LB) {
                        if (start != idx) {
                            var v = expr[start..idx];
                            var maybeOp = GetOperator(v[0]);
                            bool maybeSubFirst = false;
                            if (maybeOp != null && maybeOp.HasValue) {
                                v = v[1..];
                                if (maybeOp.Value == Operator.SUB) {
                                    maybeSubFirst = true;
                                }
                            }
                            switch (v) {
                                case "sqrt": {
                                    //point '(' at begin
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(maybeSubFirst ? -Math.Sqrt(numV) : Math.Sqrt(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                case "sin": {
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(maybeSubFirst ? -Math.Sin(numV) : Math.Sin(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                case "cos": {
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(maybeSubFirst ? -Math.Cos(numV) : Math.Cos(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                case "tan": {
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(maybeSubFirst ? -Math.Tan(numV) : Math.Tan(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                case "abs": {
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(maybeSubFirst ? -Math.Abs(numV) : Math.Abs(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                case "ln": {
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(maybeSubFirst ? -Math.Log(numV) : Math.Log(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                case "lg": {
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(maybeSubFirst ? -Math.Log10(numV) : Math.Log10(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                default: {
                                    if (v.Length > 0) {
                                        var numV = ParseNum(v);
                                        nums.Push(maybeSubFirst ? -numV : numV);
                                    } else if (maybeSubFirst) {
                                        nums.Push(0);
                                        ops.Push(Operator.SUB);
                                    }
                                    ops.Push(op.Value);
                                    brackets += 1;
                                    start = idx += 1;
                                    break;
                                }
                            }
                        } else {
                            start = idx + 1;
                            brackets += 1;
                            ops.Push(op.Value);
                        }
                    } else {
                        if (start != idx) {
                            var v = expr[start..idx];
                            nums.Push(ParseNum(v));
                        }
                        if (op.Value == Operator.RB) {
                            var lastOp = ops.Pop();
                            while (lastOp != Operator.LB) {
                                double b = nums.Pop();
                                double a = nums.Pop();
                                nums.Push(OpNum(lastOp, a, b));
                                lastOp = ops.Pop();
                            }
                            brackets -= 1;
                            start = idx + 1;
                        } else {
                            if (!(start == idx && nums.Count == ops.Count - brackets)) {
                                if (ops.Count > 0) {
                                    var lastOp = ops.Peek();
                                    while (lastOp != Operator.LB && GetOperatorPriority(op.Value) <= GetOperatorPriority(lastOp)) {
                                        double b = nums.Pop();
                                        double a = nums.Pop();
                                        nums.Push(OpNum(lastOp, a, b));
                                        ops.Pop();
                                        if (ops.Count == 0) {
                                            break;
                                        }
                                        lastOp = ops.Peek();
                                    }
                                }
                                ops.Push(op.Value);
                                start = idx + 1;
                            }
                        }
                    }
                }
                idx += 1;
            }
            if (idx != start)
                nums.Push(ParseNum(expr[start..]));
            while (ops.TryPop(out var op)) {
                double b = nums.Pop();
                double a = nums.Pop();
                nums.Push(OpNum(op, a, b));
            }

            if (nums.Count != 1) {
                throw new ArgumentException($"the expr is wrong with nums left: {nums}");
            }
            return nums.Pop();
        }
    }
}
