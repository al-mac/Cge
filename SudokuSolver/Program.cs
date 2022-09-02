using System;

namespace SudokuSolver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var board = new Board("530070000600195000098000060800060003400803001700020006060000280000419005000080079");
            //var board = new Board("209000600040870012800019040030700801065008030100030007000650709604000020080301450");
            var board = new Board("078009500005200030000000004000080000620000070000003600700016000819500006003408000");

            while (true)
            {
                board.Print();
                board.Step();

                Console.ReadKey();
            }
        }
    }
}