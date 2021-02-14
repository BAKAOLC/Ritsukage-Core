using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands {
    [CommandGroup]
    class CalcOperator {

        public enum Operator {
            ADD, SUB, MUL, DIV,
            LB, RB, MOD
        }

        public static int GetOperatorPriority(Operator op) {
            switch (op) {
                case Operator.ADD:
                case Operator.SUB:
                    return 0;
                case Operator.MUL:
                case Operator.DIV:
                    return 1;
                case Operator.LB:
                case Operator.RB:
                    return 9961;
                case Operator.MOD:
                    return 1;
                default:
                    return 0;
            }
        }

        public static double OpNum(Operator op, double a, double b) {
            switch (op) {
                case Operator.ADD:
                    return a + b;
                case Operator.SUB:
                    return a - b;
                case Operator.MUL:
                    return a * b;
                case Operator.DIV:
                    return a / b;
                case Operator.MOD:
                    return a % b;
                default:
                    throw new ArgumentOutOfRangeException($"operator {op} is not suppported calc");
            }
        }

        public static Operator? GetOperator(char c) {
            switch (c) {
                case '+':
                    return Operator.ADD;
                case '-':
                    return Operator.SUB;
                case 'x':
                case 'X':
                case '*':
                    return Operator.MUL;
                case '/':
                    return Operator.DIV;
                case '(':
                    return Operator.LB;
                case ')':
                    return Operator.RB;
                case '%':
                    return Operator.MOD;
                default:
                    return null;

            }
        }

        public static double ParseNum(string n) {
            switch (n) {
                case "PI":
                case "pi":
                case "Π":
                    return Math.PI;
                case "e": {
                    return Math.E;
                }
                default: {
                    return double.Parse(n);
                }
            }
        }

        /// <summary>
        /// 从开始地点解析到第一个()结束的值
        /// </summary>
        /// <param name="expr">字符串表达式</param>
        /// <param name="idx">字符串表达式解析开始点</param>
        /// <returns>(表达式的值, 结束时的索引位置(包含))</returns>
        public static (double, int) ParseUnitValue(string expr, int start) {
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

        public static double GetExprValue(string expr) {

            var nums = new Stack<double>();
            var ops = new Stack<Operator>();
            int idx = 0;
            int start = 0;

            while (idx < expr.Length) {
                char c = expr[idx];
                var op = GetOperator(c);
                //what is c# nullable mean??????
                //op != null mean the nullable is not null or has value?
                if (op != null && op.HasValue) {
                    //maybe sqrt(.... or sth else
                    if (op.Value == Operator.LB) {
                        if (start != idx) {
                            var v = expr[start..idx];
                            switch (v) {
                                case "sqrt": {
                                    //point '(' at begin
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(Math.Sqrt(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                case "sin": {
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(Math.Sin(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                case "cos": {
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(Math.Cos(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                case "tan": {
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(Math.Tan(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                case "abs": {
                                    var (numV, endIdx) = ParseUnitValue(expr, idx);
                                    nums.Push(Math.Abs(numV));
                                    idx = endIdx;
                                    start = idx + 1;
                                    break;
                                }
                                default: {
                                    nums.Push(ParseNum(expr[start..idx]));
                                    ops.Push(op.Value);
                                    start = idx += 1;
                                    break;
                                }
                            }
                        } else {
                            start = idx + 1;
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
                            start = idx + 1;
                        } else {
                            if (!(op == Operator.SUB && start == idx)) {
                                if (ops.Count > 0) {
                                    var lastOp = ops.Peek();
                                    while (lastOp != Operator.LB && GetOperatorPriority(op.Value) < GetOperatorPriority(lastOp)) {
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

            if (idx != start) {
                nums.Push(ParseNum(expr[start..]));
            }
            while (ops.TryPop(out var op)) {
                double b = nums.Pop();
                double a = nums.Pop();
                nums.Push(OpNum(op, a, b));
            }

            if (ops.Count == 1 && nums.Count == 1 && ops.Pop() == Operator.SUB) {
                nums.Push(-nums.Pop());
            }
            return nums.Pop();
        }
        [Command("calc")]
        public static async void CalcMath(SoraMessage e, string expr) {
            try {
                double result = GetExprValue(expr.Replace(" ", ""));
                await e.AutoAtReply($"{expr} = {result}");
            } catch {
                await e.AutoAtReply("操作失败");
            }
        }
    }
}
