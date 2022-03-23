%namespace GardensPoint

%union
{
public string val;
public int line;
public Tree node;
}

%token '+' '-' '*' '/'
%token '|' '&'
%token '!' '~'
%token Or And
%token '<' '>' Equals NotEquals GreaterEqual LessEqual
%token '(' ')' '{' '}'
%token Int Double Bool
%token Program If Else While Read Write Return '=' ';'
%token Ident IntNumber RealNumber BoolValue StringValue
%%

start
	: Program '{' block '}' EOF { head = $3.node; }
	| error { Console.WriteLine("Syntax error at line {0}",$1.line); Tree.errors++; YYACCEPT; }
	;
block
	: declarations operations { $$.node = new Block($2.node, $1.node); }
	| error ';' { Console.WriteLine("Syntax error at line {0}",$2.line); Tree.errors++; }
	| error EOF { Console.WriteLine("Syntax error at line {0}",$2.line); Tree.errors++; }
	| error '}' { Console.WriteLine("Syntax error at line {0}",$2.line); Tree.errors++; }
	;
declarations
	: declaration declarations { $$ = $2; $$.node.children.Insert(0,$1.node); }
	| /* EMPTY */ { $$.node = new Block(); } 
	;
operations
	: operation operations { $$ = $2; $$.node.children.Insert(0,$1.node); }
	| /* EMPTY */ { $$.node = new Block(); }
	;
declaration
	: Double Ident ';' { $$.node = new Declaration($2.val, "double", $1.line); }
	| Int Ident ';' { $$.node = new Declaration($2.val, "int", $1.line); }
	| Bool Ident ';' { $$.node = new Declaration($2.val, "bool", $1.line); }
	;
operation
	: If '(' assignable ')'  operation  { $$.node = new If($3.node, $5.node, $1.line); }
	| If '(' assignable ')'  operation  Else  operation  { $$.node = new IfElse($3.node, $5.node, $7.node, $1.line); }
	| While '(' assignable ')' operation { $$.node = new While($3.node, $5.node, $1.line); }
	| Return ';' { $$.node = new Return(); } 
	| Read Ident ';' { $$.node = new Read($2.val, $1.line); }
	| Write assignable ';' { $$.node = new Write($2.node, $1.line); }
	| Write StringValue ';' { $$.node = new Write($2.val, $1.line); }
	| assignable ';' { $$.node = new Pop($1.node, $2.line); }
	| '{' operations '}' {$$ = $2; }
	;
assignable
	: Ident '=' assignable { $$.node = new Assign($1.val, $3.node, $2.line); }
	| logical
	;
logical
	: logical And relative { $$.node = new BinaryOperation($1.node, "&&", $3.node, "bool", $2.line); }
	| logical Or relative { $$.node = new BinaryOperation($1.node, "||", $3.node, "bool", $2.line); }
	| relative
	;
relative
	: relative '>' additive { $$.node = new BinaryOperation($1.node, ">", $3.node, "bool", $2.line); }
	| relative GreaterEqual additive { $$.node = new BinaryOperation($1.node, ">=", $3.node, "bool", $2.line); }
	| relative '<' additive { $$.node = new BinaryOperation($1.node, "<", $3.node, "bool", $2.line); }
	| relative LessEqual additive { $$.node = new BinaryOperation($1.node, "<=", $3.node, "bool", $2.line); }
	| relative Equals additive { $$.node = new BinaryOperation($1.node, "==", $3.node, "bool", $2.line); }
	| relative NotEquals additive { $$.node = new BinaryOperation($1.node, "!=", $3.node, "bool", $2.line); }
	| additive
	;
additive
	: additive '+' multiplicative { $$.node = new BinaryOperation($1.node, "+", $3.node, "number", $2.line); }
	| additive '-' multiplicative { $$.node = new BinaryOperation($1.node, "-", $3.node, "number", $2.line); }
	| multiplicative
	;
multiplicative
	: multiplicative '*' bitwise { $$.node = new BinaryOperation($1.node, "*", $3.node, "number", $2.line); }
	| multiplicative '/' bitwise { $$.node = new BinaryOperation($1.node, "/", $3.node, "number", $2.line); }
	| bitwise
	;
bitwise
	: bitwise '|' unary { $$.node = new BinaryOperation($1.node, "|", $3.node, "int", $2.line); }
	| bitwise '&' unary { $$.node = new BinaryOperation($1.node, "&", $3.node, "int", $2.line); }
	| unary
	;
unary
	: '!' unary { $$.node = new UnaryOperation($2.node, "!", "bool", $1.line); }
	| '~' unary { $$.node = new UnaryOperation($2.node, "~", "int", $1.line); }
	| '-' unary { $$.node = new UnaryOperation($2.node, "-", "number", $1.line); }
	| '(' Double ')' unary { $$.node = new UnaryOperation($4.node, "toDouble", "double", $2.line); }
	| '(' Int ')' unary { $$.node = new UnaryOperation($4.node, "toInt", "int", $2.line); }
	| value
	;
value
	: IntNumber { $$.node = new Value($1.val, "int", $1.line); }
	| RealNumber { $$.node = new Value($1.val, "double", $1.line); }
	| BoolValue { $$.node = new Value($1.val, "bool", $1.line); }
	| Ident	{ $$.node = new Value($1.val, "ident", $1.line); }
	| '(' assignable ')' { $$.node = new Pointer($2.node); } 
	;

%%
public Parser(Scanner scanner) : base(scanner) { }
public Tree head;