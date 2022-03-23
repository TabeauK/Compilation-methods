.assembly extern mscorlib { }
.assembly kompilator { }
.method static void main()
{
.entrypoint
.try
{

// prolog

leave EndMain
}
catch [mscorlib]System.Exception
{
callvirt instance string [mscorlib]System.Exception::get_Message()
call void [mscorlib]System.Console::WriteLine(string)
leave EndMain
}
EndMain: ret
}
