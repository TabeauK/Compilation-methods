%using QUT.Gppg;
%namespace GardensPoint

IntNumber			0|([1-9][0-9]*)
RealNumber			(0|([1-9][0-9]*))\.[0-9]+
BoolValue			"true"|"false"
Ident				[A-Za-z]([A-Za-z0-9]*)
StringValue			\"([^\n\\\"]|\\.)*\"
Comment				\/\/[^\n]*\n
WhiteSpace			[ \n\t\f\r]

%%

"program"	{ yylval.line=yyline; return (int)Tokens.Program; }
"if"		{ yylval.line=yyline; return (int)Tokens.If; }
"else"		{ yylval.line=yyline; return (int)Tokens.Else; }
"while"		{ yylval.line=yyline; return (int)Tokens.While; }
"read"		{ yylval.line=yyline; return (int)Tokens.Read; }
"write"		{ yylval.line=yyline; return (int)Tokens.Write; }
"return"	{ yylval.line=yyline; return (int)Tokens.Return; }
"int"		{ yylval.line=yyline; return (int)Tokens.Int; }
"double"	{ yylval.line=yyline; return (int)Tokens.Double; }
"bool"		{ yylval.line=yyline; return (int)Tokens.Bool; }
"="			{ yylval.line=yyline; return '='; }
"||"		{ yylval.line=yyline; return (int)Tokens.Or; }
"&&"		{ yylval.line=yyline; return (int)Tokens.And; }
"|"			{ yylval.line=yyline; return '|'; }
"&"			{ yylval.line=yyline; return '&'; }
"=="		{ yylval.line=yyline; return (int)Tokens.Equals; }
"!="		{ yylval.line=yyline; return (int)Tokens.NotEquals; }
">"			{ yylval.line=yyline; return '>'; }
">="		{ yylval.line=yyline; return (int)Tokens.GreaterEqual; }
"<"			{ yylval.line=yyline; return '<'; }
"<="		{ yylval.line=yyline; return (int)Tokens.LessEqual; }
"+"			{ yylval.line=yyline; return '+'; }
"-"			{ yylval.line=yyline; return '-'; }
"*"			{ yylval.line=yyline; return '*'; }
"/"			{ yylval.line=yyline; return '/'; }
"!"			{ yylval.line=yyline; return '!'; }
"~"			{ yylval.line=yyline; return '~'; }
"("			{ yylval.line=yyline; return '('; }
")"			{ yylval.line=yyline; return ')'; }
"{"			{ yylval.line=yyline; return '{'; }
"}"			{ yylval.line=yyline; return '}'; }
";"			{ yylval.line=yyline; return ';'; }
{WhiteSpace}	{ }
{IntNumber}		{ yylval.val=yytext; yylval.line=yyline; return (int)Tokens.IntNumber; }
{RealNumber}	{ yylval.val=yytext; yylval.line=yyline; return (int)Tokens.RealNumber; }
{BoolValue}     { yylval.val=yytext; yylval.line=yyline; return (int)Tokens.BoolValue; }
{StringValue}	{ yylval.val=yytext; yylval.line=yyline; return (int)Tokens.StringValue; }
{Ident}			{ yylval.val=yytext; yylval.line=yyline; return (int)Tokens.Ident; }
{Comment}		{ }