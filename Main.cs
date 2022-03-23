using System;
using System.IO;
using System.Collections.Generic;

namespace GardensPoint
{
    public class Compiler
    {
        public static int errors = 0;

        public static List<string> source;

        // arg[0] określa plik źródłowy
        // pozostałe argumenty są ignorowane
        public static int Main(string[] args)
        {
            string file;
            FileStream source;
            Console.WriteLine("\nSingle-Pass CIL Code Generator for Multiline Calculator - Gardens Point");
            if(args.Length >= 1)
                file = args[0];
            else
            {
                Console.Write("\nsource file:  ");
                file = Console.ReadLine();
            }
            try
            {
                var sr = new StreamReader(file);
                string str = sr.ReadToEnd();
                sr.Close();
                Compiler.source = new System.Collections.Generic.List<string>(str.Split(new string[] { "\r\n" }, System.StringSplitOptions.None));
                source = new FileStream(file, FileMode.Open);
            }
            catch(Exception e)
            {
                Console.WriteLine("\n" + e.Message);
                return 1;
            }
            Scanner scanner = new Scanner(source);
            Parser parser = new Parser(scanner);
            Console.WriteLine();
            sw = new StreamWriter(file + ".il");

            parser.Parse(); // <- to zamienić

            if(parser.head == null)
            {
                Console.WriteLine("  Syntax error\n");
                GenEpilog();
                sw.Close();
                source.Close();
                return 6;
            }

            if(Tree.errors > 0)
            {
                Console.WriteLine("  compilation failed\n");
                Console.WriteLine($"\n  {Tree.errors} errors detected\n");
                GenEpilog();
                sw.Close();
                source.Close();
                return 4;
            }

            parser.head.Check();

            if(Tree.errors > 0)
            {
                Console.WriteLine("  compilation failed\n");
                Console.WriteLine($"\n  {Tree.errors} errors detected\n");
                GenEpilog();
                sw.Close();
                source.Close();
                return 2;
            }
            else
            {
                Console.WriteLine("  compilation successful\n");
                GenProlog();
                parser.head.Generate();
                GenEpilog();
                sw.Close();
                source.Close();
                return 0;
            }
        }

        public static void EmitCode(string instr = null)
        {
            sw.WriteLine(instr);
        }

        public static void EmitCode(string instr, params object[] args)
        {
            sw.WriteLine(instr, args);
        }

        private static StreamWriter sw;

        private static void GenProlog()
        {
            EmitCode(".assembly extern mscorlib { }");
            EmitCode(".assembly kompilator { }");
            EmitCode(".method static void main()");
            EmitCode("{");
            EmitCode(".entrypoint");
            EmitCode(".maxstack 2048");
            EmitCode(".try");
            EmitCode("{");
            EmitCode();

            EmitCode("// prolog");
            EmitCode();
        }

        private static void GenEpilog()
        {
            EmitCode("RET: leave EndMain");
            EmitCode("}");
            EmitCode("catch [mscorlib]System.Exception");
            EmitCode("{");
            EmitCode("callvirt instance string [mscorlib]System.Exception::get_Message()");
            EmitCode("call void [mscorlib]System.Console::WriteLine(string)");
            EmitCode("leave EndMain");
            EmitCode("}");
            EmitCode("EndMain: ret");
            EmitCode("}");
        }
    }

    public abstract class Tree
    {
        public static int index = 0;
        public int ind;
        public List<Tree> children = new List<Tree>();
        public string type;
        public string val;
        public int line;
        public static Dictionary<string, string> vars = new Dictionary<string, string>();
        public static int errors = 0;

        public abstract void Check();
        public abstract void Generate();
    }

    public class If : Tree
    {
        Tree cond;
        Tree block;
        public If(Tree condition, Tree operationList, int line)
        {
            cond = condition;
            block = operationList;
            base.line = line;
            index++;
            ind = index;
        }

        public override void Check()
        {
            cond.Check();
            block.Check();
            if(cond.type != "bool")
            {
                Console.WriteLine($"Wrong type {cond.type} at line {line}");
                errors++;
            }
        }

        public override void Generate()
        {
            cond.Generate();
            Compiler.EmitCode($"brfalse L_IF_END_{ind}");
            block.Generate();
            Compiler.EmitCode($"L_IF_END_{ind}: nop");
        }
    }

    public class IfElse : Tree
    {
        Tree cond;
        Tree block;
        Tree eBlock;
        public IfElse(Tree condition, Tree block, Tree elseBlock, int line)
        {
            cond = condition;
            eBlock = elseBlock;
            this.block = block;
            base.line = line;
            index++;
            ind = index;
        }

        public override void Check()
        {
            cond.Check();
            block.Check();
            eBlock.Check();
            if(cond.type != "bool")
            {
                Console.WriteLine($"Wrong type {cond.type} at line {line}");
                errors++;
            }
        }

        public override void Generate()
        {
            cond.Generate();
            Compiler.EmitCode($"brfalse L_ELSE_START_{ind}");
            block.Generate();
            Compiler.EmitCode($"br L_IFELSE_END_{ind}");
            Compiler.EmitCode($"L_ELSE_START_{ind}: nop");
            eBlock.Generate();
            Compiler.EmitCode($"L_IFELSE_END_{ind}: nop");
        }
    }

    public class While : Tree
    {
        Tree cond;
        Tree block;
        public While(Tree condition, Tree block, int line)
        {
            cond = condition;
            this.block = block;
            base.line = line;
            index++;
            ind = index;
        }

        public override void Check()
        {
            cond.Check();
            block.Check();
            if(cond.type != "bool")
            {
                Console.WriteLine($"Wrong type {cond.type} at line {line}");
                errors++;
            }
        }

        public override void Generate()
        {
            Compiler.EmitCode($"L_WHILE_START_{ind}: nop");
            cond.Generate();
            Compiler.EmitCode($"brfalse L_WHILE_END_{ind}");
            block.Generate();
            Compiler.EmitCode($"br L_WHILE_START_{ind}");
            Compiler.EmitCode($"L_WHILE_END_{ind}: nop");
        }
    }

    public class Assign : Tree
    {
        Tree tree;
        public Assign(string ident, Tree val, int line)
        {
            tree = val;
            base.val = ident;
            base.line = line;
        }

        public override void Check()
        {
            tree.Check();
            if(!vars.ContainsKey(val))
            {
                Console.WriteLine($"Undeclared variable at line {line}");
                errors++;
            }
            else
            {
                type = vars[val];
                if(type == tree.type)
                {

                }
                else if(type == "double" && tree.type == "int")
                {

                }
                else
                {
                    Console.WriteLine($"Wrong type {tree.type} at line {line}");
                    errors++;
                }
            }

        }

        public override void Generate()
        {
            tree.Generate();
            if(type == "double" && tree.type == "int")
            {
                Compiler.EmitCode("conv.r8");
            }
            Compiler.EmitCode("dup");
            Compiler.EmitCode($"stloc V_{val}");
        }
    }

    public class BinaryOperation : Tree
    {
        Tree left;
        Tree right;

        public BinaryOperation(Tree l, string val, Tree r, string type, int line)
        {
            left = l;
            right = r;
            base.val = val;
            base.type = type;
            base.line = line;
            index++;
            ind = index;
        }

        public override void Check()
        {
            left.Check();
            right.Check();
            switch(type)
            {
                case "number":
                    {
                        if((left.type != "int" && left.type != "double") || (right.type != "double" && right.type != "int"))
                        {
                            Console.WriteLine($"Wrong types {left.type} and {right.type} of operator {val} at line {line}");
                            errors++;
                        }
                        else if(left.type == "int" && right.type == "int")
                        {
                            type = "int";
                        }
                        else
                        {
                            type = "double";
                        }
                        break;
                    }
                case "bool":
                    {
                        if(val == "&&" || val == "||")
                        {
                            if(left.type == "bool" && right.type == "bool")
                            {
                            }
                            else
                            {
                                Console.WriteLine($"Wrong types {left.type} and {right.type} of operator {val} at line {line}");
                                errors++;
                            }
                        }
                        else
                        {
                            if(left.type == "bool" && right.type == "bool")
                            {
                            }
                            else if((left.type != "int" && left.type != "double") || (right.type != "double" && right.type != "int"))
                            {
                                Console.WriteLine($"Wrong types {left.type} and {right.type} of operator {val} at line {line}");
                                errors++;
                            }
                            else if(left.type == "int" && right.type == "int")
                            {
                            }
                            else
                            {
                            }
                        }
                        break;
                    }
                case "int":
                    {
                        if(left.type != "int" || right.type != "int")
                        {
                            Console.WriteLine($"Wrong types {left.type} and {right.type} of operator {val} at line {line}");
                            errors++;
                        }
                        break;
                    }
            }
        }

        public override void Generate()
        {
            switch(val)
            {
                case "&&":
                    {
                        left.Generate();
                        Compiler.EmitCode("dup");
                        Compiler.EmitCode($"brfalse END_AND_{ind}");
                        Compiler.EmitCode("pop");
                        right.Generate();
                        Compiler.EmitCode($"END_AND_{ind}:");
                        break;
                    }
                case "||":
                    {
                        left.Generate();
                        Compiler.EmitCode("dup");
                        Compiler.EmitCode($"brtrue END_AND_{ind}");
                        Compiler.EmitCode("pop");
                        right.Generate();
                        Compiler.EmitCode($"END_AND_{ind}:");
                        break;
                    }
                case "|":
                    {
                        left.Generate();
                        right.Generate();
                        Compiler.EmitCode("or");
                        break;
                    }
                case "&":
                    {
                        left.Generate();
                        right.Generate();
                        Compiler.EmitCode("and");
                        break;
                    }
                case "+":
                    {
                        if(type == "double")
                        {
                            if(left.type == "int")
                            {
                                left.Generate();
                                Compiler.EmitCode("conv.r8");
                                right.Generate();
                            }
                            else if(right.type == "int")
                            {
                                left.Generate();
                                right.Generate();
                                Compiler.EmitCode("conv.r8");
                            }
                            else
                            {
                                left.Generate();
                                right.Generate();
                            }
                        }
                        else
                        {
                            left.Generate();
                            right.Generate();
                        }
                        Compiler.EmitCode("add");
                        break;
                    }
                case "-":
                    {
                        if(type == "double")
                        {
                            if(left.type == "int")
                            {
                                left.Generate();
                                Compiler.EmitCode("conv.r8");
                                right.Generate();
                            }
                            else if(right.type == "int")
                            {
                                left.Generate();
                                right.Generate();
                                Compiler.EmitCode("conv.r8");
                            }
                            else
                            {
                                left.Generate();
                                right.Generate();
                            }
                        }
                        else
                        {
                            left.Generate();
                            right.Generate();
                        }
                        Compiler.EmitCode("sub");
                        break;
                    }
                case "*":
                    {
                        if(type == "double")
                        {
                            if(left.type == "int")
                            {
                                left.Generate();
                                Compiler.EmitCode("conv.r8");
                                right.Generate();
                            }
                            else if(right.type == "int")
                            {
                                left.Generate();
                                right.Generate();
                                Compiler.EmitCode("conv.r8");
                            }
                            else
                            {
                                left.Generate();
                                right.Generate();
                            }
                        }
                        else
                        {
                            left.Generate();
                            right.Generate();
                        }
                        Compiler.EmitCode("mul");
                        break;
                    }
                case "/":
                    {
                        if(type == "double")
                        {
                            if(left.type == "int")
                            {
                                left.Generate();
                                Compiler.EmitCode("conv.r8");
                                right.Generate();
                            }
                            else if(right.type == "int")
                            {
                                left.Generate();
                                right.Generate();
                                Compiler.EmitCode("conv.r8");
                            }
                            else
                            {
                                left.Generate();
                                right.Generate();
                            }
                        }
                        else
                        {
                            left.Generate();
                            right.Generate();
                        }
                        Compiler.EmitCode("div");
                        break;
                    }
                case "<":
                    {
                        if(left.type == "int" && right.type == "double")
                        {
                            left.Generate();
                            Compiler.EmitCode("conv.r8");
                            right.Generate();
                        }
                        else if(right.type == "int" && left.type == "double")
                        {
                            left.Generate();
                            right.Generate();
                            Compiler.EmitCode("conv.r8");
                        }
                        else
                        {
                            left.Generate();
                            right.Generate();
                        }
                        Compiler.EmitCode("clt");
                        break;
                    }
                case ">":
                    {
                        if(left.type == "int" && right.type == "double")
                        {
                            left.Generate();
                            Compiler.EmitCode("conv.r8");
                            right.Generate();
                        }
                        else if(right.type == "int" && left.type == "double")
                        {
                            left.Generate();
                            right.Generate();
                            Compiler.EmitCode("conv.r8");
                        }
                        else
                        {
                            left.Generate();
                            right.Generate();
                        }
                        Compiler.EmitCode("cgt");
                        break;
                    }
                case "==":
                    {
                        if(left.type == "int" && right.type == "double")
                        {
                            left.Generate();
                            Compiler.EmitCode("conv.r8");
                            right.Generate();
                        }
                        else if(right.type == "int" && left.type == "double")
                        {
                            left.Generate();
                            right.Generate();
                            Compiler.EmitCode("conv.r8");
                        }
                        else
                        {
                            left.Generate();
                            right.Generate();
                        }
                        Compiler.EmitCode("ceq");
                        break;
                    }
                case "!=":
                    {
                        if(left.type == "int" && right.type == "double")
                        {
                            left.Generate();
                            Compiler.EmitCode("conv.r8");
                            right.Generate();
                        }
                        else if(right.type == "int" && left.type == "double")
                        {
                            left.Generate();
                            right.Generate();
                            Compiler.EmitCode("conv.r8");
                        }
                        else
                        {
                            left.Generate();
                            right.Generate();
                        }
                        Compiler.EmitCode("ceq");
                        Compiler.EmitCode("ldc.i4.0");
                        Compiler.EmitCode("ceq");
                        break;
                    }
                case "<=":
                    {
                        if(left.type == "int" && right.type == "double")
                        {
                            left.Generate();
                            Compiler.EmitCode("conv.r8");
                            right.Generate();
                        }
                        else if(right.type == "int" && left.type == "double")
                        {
                            left.Generate();
                            right.Generate();
                            Compiler.EmitCode("conv.r8");
                        }
                        else
                        {
                            left.Generate();
                            right.Generate();
                        }
                        Compiler.EmitCode("cgt");
                        Compiler.EmitCode("ldc.i4.0");
                        Compiler.EmitCode("ceq");
                        break;
                    }
                case ">=":
                    {
                        if(left.type == "int" && right.type == "double")
                        {
                            left.Generate();
                            Compiler.EmitCode("conv.r8");
                            right.Generate();
                        }
                        else if(right.type == "int" && left.type == "double")
                        {
                            left.Generate();
                            right.Generate();
                            Compiler.EmitCode("conv.r8");
                        }
                        else
                        {
                            left.Generate();
                            right.Generate();
                        }
                        Compiler.EmitCode("clt");
                        Compiler.EmitCode("ldc.i4.0");
                        Compiler.EmitCode("ceq");
                        break;
                    }
            }
        }
    }

    public class UnaryOperation : Tree
    {
        Tree tree;
        public UnaryOperation(Tree t, string val, string type, int line)
        {
            tree = t;
            base.val = val;
            base.type = type;
            base.line = line;
        }

        public override void Check()
        {
            tree.Check();
            switch(val)
            {
                case "!":
                    {
                        if(tree.type != "bool")
                        {
                            Console.WriteLine($"Wrong types {tree.type} of operator {val} at line {line}");
                            errors++;
                        }
                        break;
                    }
                case "~":
                    {
                        if(tree.type != "int")
                        {
                            Console.WriteLine($"Wrong types {tree.type} of operator {val} at line {line}");
                            errors++;
                        }
                        break;
                    }
                case "-":
                    {
                        if(tree.type != "int" && tree.type != "double")
                        {
                            Console.WriteLine($"Wrong types {tree.type} of operator {val} at line {line}");
                            errors++;
                        }
                        type = tree.type;
                        break;
                    }
                case "toDouble":
                    {
                        break;
                    }
                case "toInt":
                    {
                        break;
                    }
            }
        }

        public override void Generate()
        {
            tree.Generate();
            switch(val)
            {
                case "!":
                    {
                        Compiler.EmitCode("ldc.i4.0");
                        Compiler.EmitCode("ceq");
                        break;
                    }
                case "~":
                    {
                        Compiler.EmitCode("not");
                        break;
                    }
                case "-":
                    {
                        Compiler.EmitCode("neg");
                        break;
                    }
                case "toDouble":
                    {
                        Compiler.EmitCode("conv.r8");
                        break;
                    }
                case "toInt":
                    {
                        Compiler.EmitCode("conv.i4");
                        break;
                    }
            }

        }
    }

    public class Value : Tree
    {
        readonly bool isIdent;
        public Value(string val, string type, int line)
        {
            base.val = val;
            base.type = type;
            base.line = line;
            isIdent = type == "ident";
        }

        public override void Check()
        {
            if(type == "ident")
            {
                if(!vars.ContainsKey(val))
                {
                    Console.WriteLine($"Undeclared variable at {line}");
                    errors++;
                }
                else
                {
                    type = vars[val];
                }
            }
        }

        public override void Generate()
        {
            if(isIdent)
            {
                Compiler.EmitCode($"ldloc V_{val}");
            }
            else
            {
                switch(type)
                {
                    case "int":
                        {
                            Compiler.EmitCode("ldc.i4 {0}", int.Parse(val));
                            break;
                        }
                    case "double":
                        {
                            double d = double.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                            Compiler.EmitCode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "ldc.r8 {0}", d));
                            break;
                        }
                    case "bool":
                        {
                            if(val == "true")
                            {
                                Compiler.EmitCode("ldc.i4 {0}", 1);
                            }
                            else
                            {
                                Compiler.EmitCode("ldc.i4 {0}", 0);
                            }
                            break;
                        }
                }
            }
        }
    }

    public class Declaration : Tree
    {

        public Declaration(string ident, string type, int line)
        {
            val = ident;
            base.type = type;
            base.line = line;
            if(vars.ContainsKey(val))
            {
                Console.WriteLine($"Variable already decleared {val}. Error at line {line}");
                errors++;
            }
            else
            {
                vars.Add(ident, type);
            }
        }

        public override void Check()
        {
        }

        public override void Generate()
        {
            switch(type)
            {
                case "bool":
                    {
                        Compiler.EmitCode($".locals  init ( int32 V_{val})");
                        break;
                    }
                case "int":
                    {
                        Compiler.EmitCode($".locals  init ( int32 V_{val})");
                        break;
                    }
                case "double":
                    {
                        Compiler.EmitCode($".locals  init ( float64 V_{val})");
                        break;
                    }
            }
        }
    }

    public class Read : Tree
    {

        public Read(string ident, int line)
        {
            val = ident;
            this.line = line;
        }

        public override void Check()
        {
            if(!vars.ContainsKey(val))
            {
                Console.WriteLine($"Undeclared variable at line {line}");
                errors++;
            }
        }

        public override void Generate()
        {
            Compiler.EmitCode("call string[mscorlib]System.Console::ReadLine()");
            switch(vars[val])
            {
                case "bool":
                    {
                        Compiler.EmitCode("call bool[mscorlib] System.Boolean::Parse(string)");
                        break;
                    }
                case "int":
                    {
                        Compiler.EmitCode("call int32[mscorlib] System.Int32::Parse(string)");
                        break;
                    }
                case "double":
                    {
                        Compiler.EmitCode("call float64[mscorlib] System.Double::Parse(string)");
                        break;
                    }

            }
            Compiler.EmitCode($"stloc V_{val}");
        }
    }

    public class Write : Tree
    {
        Tree pValue;
        public Write(string str, int line)
        {
            val = str;
            this.type = "string";
            this.line = line;
        }

        public Write(Tree a, int line)
        {
            pValue = a;
            this.type = "value";
            this.line = line;
        }

        public override void Check()
        {
            if(type == "value")
            {
                pValue.Check();
            }
            else if(type != "string")
            {
                Console.WriteLine($"Wrong type {type} at line {line}");
                errors++;
            }
        }

        public override void Generate()
        {
            switch(type)
            {
                case "value":
                    {
                        if(pValue.type == "int")
                        {
                            pValue.Generate();
                            Compiler.EmitCode($"call void [System.Console]System.Console::Write(int32)");
                        }
                        else if(pValue.type == "double")
                        {
                            Compiler.EmitCode("call class [mscorlib] System.Globalization.CultureInfo[mscorlib] System.Globalization.CultureInfo::get_InvariantCulture()");
                            Compiler.EmitCode("ldstr \"{0:0.000000}\"");
                            pValue.Generate();
                            Compiler.EmitCode("box[mscorlib]System.Double");
                            Compiler.EmitCode("call string[mscorlib] System.String::Format(class [mscorlib] System.IFormatProvider, string, object)");
                            Compiler.EmitCode($"call void [System.Console]System.Console::Write(string)");
                        }
                        else
                        {
                            pValue.Generate();
                            Compiler.EmitCode($"call void [System.Console]System.Console::Write(bool)");
                        }
                        break;
                    }
                case "string":
                    {
                        Compiler.EmitCode($"ldstr {val}");
                        Compiler.EmitCode("call void[mscorlib] System.Console::Write(string)");
                        break;
                    }
            }
        }
    }

    public class Return : Tree
    {

        public Return()
        {
        }

        public override void Check()
        {
        }

        public override void Generate()
        {
            Compiler.EmitCode($"br RET");
        }
    }

    public class Block : Tree
    {
        public Block()
        {
        }

        public Block(Tree a, Tree b)
        {
            children.Insert(0, a);
            children.Insert(0, b);
        }

        public override void Check()
        {
            foreach(var elem in children)
            {
                elem.Check();
            }
        }

        public override void Generate()
        {
            foreach(var elem in children)
            {
                elem.Generate();
            }
        }
    }

    public class Pointer : Tree
    {
        Tree point;

        public Pointer(Tree p)
        {
            point = p;
        }

        public override void Check()
        {
            point.Check();
            type = point.type;
        }

        public override void Generate()
        {
            point.Generate();
        }
    }

    public class Pop : Tree
    {
        Tree assignTree;

        public Pop(Tree a, int l)
        {
            line = l;
            assignTree = a;
        }

        public override void Check()
        {
            assignTree.Check();
        }

        public override void Generate()
        {
            assignTree.Generate();
            Compiler.EmitCode("pop");
        }
    }
}