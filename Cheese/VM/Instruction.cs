using System;
using System.Collections.Generic;

using Cheese;


namespace Cheese.Machine
{
	internal static class Tools {
		public static void Add(this List<Instruction> InstList, Instruction.OP code) {
			InstList.Add(new Instruction(code));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a) {
			InstList.Add(new Instruction(code, a));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a, int b) {
			InstList.Add(new Instruction(code, a, b));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a, int b, int c) {
			InstList.Add(new Instruction(code, a, b, c));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a, int b, bool bK, int c) {
			InstList.Add(new Instruction(code, a, b, bK, c));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a, int b, int c, bool cK) {
			InstList.Add(new Instruction(code, a, b, c, cK));	}
		public static void Add(this List<Instruction> InstList, Instruction.OP code, int a, int b, bool bK, int c, bool cK) {
			InstList.Add(new Instruction(code, a, b, bK, c, cK));	}
	}

	class Instruction {
		public enum OP {
			ERROR,
			MOVE,
			LOADK,
			LOADBOOL,
			LOADNIL,
			GETUPVAL,
			GETGLOBAL,
			GETTABLE,
			SETGLOBAL,
			SETUPVAL,
			SETTABLE,
			NEWTABLE,
			SELF,
			ADD,
			SUB,
			MUL,
			DIV,
			MOD,
			POW,
			UNM,
			NOT,
			LEN,
			CONCAT,
			JMP,
			EQ,
			LT,
			LE,
			TEST,
			TESTSET,
			CALL,
			TAILCALL,
			RETURN,
			FORLOOP,
			FORPREP,
			TFORLOOP,
			SETLIST,
			CLOSE,
			CLOSURE,
			VARARG
		}

		internal OP Code;
		internal int A, B, C;
		internal bool isA = false, isB=false, isC=false;
		internal bool rkB = false, rkC = false;

		public Instruction() { ; }
		public Instruction(OP code) { Code = code; }
		public Instruction(OP code, int a) { Code = code; A = a; isA = true; }
		public Instruction(OP code, int a, int b) { Code = code; A = a; B = b; isA = isB = true;}
		public Instruction(OP code, int a, int b, int c) { Code = code; A = a; B = b; C = c; isA = isB = isC = true; }
		public Instruction(OP code, int a, int b, bool bK, int c) { Code = code; A = a; B = b; C = c; isA = isB = isC = true; rkB = bK;}
		public Instruction(OP code, int a, int b, int c, bool cK) { Code = code; A = a; B = b; C = c; isA = isB = isC = true; rkC = cK;}
		public Instruction(OP code, int a, int b, bool bK, int c, bool cK) { Code = code; A = a; B = b; C = c; isA = isB = isC = true; rkB = bK; rkC = cK;}


	}


}

