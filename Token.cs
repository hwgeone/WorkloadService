using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkloadService.CalculateEngine
{
    /// <summary>
	/// Represents a node in the expression tree.
    /// </summary>
    internal class Token
    {
        // ** fields
        public TKID ID;
        public TKTYPE Type;
        public object Value;

        // ** ctor
        public Token(object value, TKID id, TKTYPE type)
        {
            Value = value;
            ID = id;
            Type = type;
        }
    }
    /// <summary>
    /// Token types (used when building expressions, sequence defines operator priority)
    /// </summary>
    internal enum TKTYPE
    {
        COMPARE,	// < > = <= >=                                  比较
        ADDSUB,		// + -                                          加减
        MULDIV,		// * /                                          乘除
        POWER,		// ^                                            幂
        GROUP,		// ( ) , .                                      分组
        LITERAL,	// 123.32, "Hello", etc.                        文字
        IDENTIFIER  // functions, external objects, bindings        方法、内部对象、属性
    }
    /// <summary>
    /// Token ID (used when evaluating expressions)
    /// </summary>
    internal enum TKID
    {
        GT, LT, GE, LE, EQ, NE, // COMPARE
        ADD, SUB, // ADDSUB
        MUL, DIV, DIVINT, MOD, // MULDIV
        POWER, // POWER
        OPEN, CLOSE, END, COMMA, PERIOD, // GROUP
        ATOM, // LITERAL, IDENTIFIER
    }
}
