using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace KenKen
{
    enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Constant
    }

    class Point
    {
        public Point()
        {
        }
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X;
        public int Y;
    }

    class KenKenBoard
    {
        int[,] mState = new int[9, 9];
        // These two variables track which numbers exist in a row/column.
        bool[,] mRowHash = new bool[9, 9];
        bool[,] mColHash = new bool[9, 9];

        public bool CanPlace(Point p, int val)
        {
            int valMinusOne = val - 1;
            return !mRowHash[p.Y, valMinusOne] && !mColHash[p.X, valMinusOne];
        }



        public void Print()
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    Console.Write("{0} ", mState[x, y]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public void UnsetPlace(Point p)
        {
            int valMinusOne = mState[p.X, p.Y] - 1;
            mRowHash[p.Y, valMinusOne] = false;
            mColHash[p.X, valMinusOne] = false;
            mState[p.X, p.Y] = 0;
        }

        public bool TryPlace(Point p, int val)
        {
            int valMinusOne = val - 1;
            if (!CanPlace(p, val))
                return false;
            mRowHash[p.Y, valMinusOne] = true;
            mColHash[p.X, valMinusOne] = true;
            mState[p.X, p.Y] = val;
            return true;
        }
    }

    class SubProblem
    {
        public override string ToString()
        {
            return string.Format("[SubProblem: Operator={0}, Value={1}, Squares={2}]", Operator, Value, Squares);
        }

        public SubProblem Prune(KenKenBoard prune)
        {
            // Prune this solution set given a board state.
            SubProblem ret = new SubProblem(Operator, Value);
            ret.mSolutions = new List<List<int>>();
            ret.mSquares = mSquares;
            // Iterate through every solution.
            foreach (var sol in Solutions)
            {
                bool success = true;
                // See if we can place this solution on the board.
                for (int i = 0; i < mSquares.Count; i++)
                {
                    if (!prune.CanPlace(mSquares[i], sol[i]))
                    {
                        success = false;
                        break;
                    }
                }
                if (success)
                    ret.mSolutions.Add(sol);
            }
            // No solutions = fail!
            if (mSolutions.Count == 0)
                return null;
            return ret;
        }

        public static bool IsValid(int val)
        {
            return val > 0 && val < 10;
        }

        bool AddExplode(List<int> current, int currentVal)
        {
            if (currentVal > Value)
                return true;
            if (current == null)
                current = new List<int>();
            if (current.Count == mSquares.Count - 1)
            {
                int val = Value - currentVal;
                if (IsValid(val))
                {
                    List<int> copy = new List<int>(current);
                    copy.Add(val);
                    mSolutions.Add(copy);
                    return true;
                }
                return val <= 9;
            }

            for (int i = 9; i >= 1; i--)
            {
                current.Add(i);
                bool ret = AddExplode(current, currentVal + i);
                current.RemoveAt(current.Count - 1);
                if (!ret)
                    break;
            }
            return true;
        }

        bool MultExplode(List<int> current, int currentVal)
        {
            if (currentVal > Value)
                return true;
            if (current == null)
                current = new List<int>();
            if (current.Count == mSquares.Count - 1)
            {
                int val = Value / currentVal;
                if (IsValid(val))
                {
                    if (Value % currentVal != 0)
                        return true;
                    List<int> copy = new List<int>(current);
                    copy.Add(val);
                    mSolutions.Add(copy);
                    return true;
                }
                return val <= 9;
            }

            for (int i = 9; i >= 1; i--)
            {
                current.Add(i);
                bool ret = MultExplode(current, currentVal * i);
                current.RemoveAt(current.Count - 1);
                if (!ret)
                    break;
            }
            return true;
        }

        public SubProblem(Operator op, int val)
        {
            Operator = op;
            Value = val;
        }

        public Operator Operator
        {
            get;
            set;
        }

        public int Value
        {
            get;
            set;
        }

        List<Point> mSquares = new List<Point>();
        public List<Point> Squares
        {
            get
            {
                return mSquares;
            }
        }

        List<List<int>> mSolutions;

        public void GenerateSolutions()
        {
            // Generate the solutions, given a bunch of squares, operator, and a final value.
            if (mSolutions != null)
                return;
            mSolutions = new List<List<int>>();

            switch (Operator)
            {
                case Operator.Add:
                    AddExplode(null, 0);
                    break;
                case Operator.Subtract:
                    for (int a = 1; a <= 9; a++)
                    {
                        int b = Math.Abs(Value - a);
                        if (IsValid(b))
                        {
                            List<int> sol = new List<int>();
                            sol.Add(a);
                            sol.Add(b);
                            mSolutions.Add(sol);
                            sol = new List<int>();
                            sol.Add(b);
                            sol.Add(a);
                            mSolutions.Add(sol);
                        }
                    }
                    break;
                case Operator.Divide:
                    if (Value == 4)
                        Console.WriteLine();
                    for (int a = 1; a <= 9; a++)
                    {
                        int b = a / Value;
                        if (IsValid(b) && a % Value == 0)
                        {
                            List<int> sol = new List<int>();
                            sol.Add(a);
                            sol.Add(b);
                            mSolutions.Add(sol);

                            sol = new List<int>();
                            sol.Add(b);
                            sol.Add(a);
                            mSolutions.Add(sol);
                        }
                    }
                    break;
                case Operator.Multiply:
                    MultExplode(null, 1);
                    break;
                case Operator.Constant:
                    {
                        List<int> sol = new List<int>();
                        sol.Add(Value);
                        mSolutions.Add(sol);
                        break;
                    }
            }
        }

        public List<List<int>> Solutions
        {
            get
            {
                return mSolutions;
            }
        }
    }

    class Problem
    {
        SubProblem[,] mSubProblems = new SubProblem[9, 9];
        public SubProblem[,] SubProblems
        {
            get
            {
                return mSubProblems;
            }
        }

        public Problem Prune(KenKenBoard board)
        {
            // Prune out solutions that are no longer valid
            // given the current board.
            Problem ret = new Problem();
            ret.mAll = new List<SubProblem>();
            for (int i = 1; i < mAll.Count; i++)
            {
                SubProblem sub = mAll[i];
                SubProblem s = sub.Prune(board);
                // If there are no solutions for this pruned SubProblem, this Problem failed.
                if (s == null)
                    return null;
                ret.mAll.Add(s);
            }
            return ret;
        }

        public KenKenBoard Solve(KenKenBoard board)
        {
            if (0 == mAll.Count)
                return board;

            // Get the next SubProblem that we want to try.
            var subproblem = mAll[0];
            var squares = subproblem.Squares;
            // Iterate through all the solutions.
            foreach (var solution in subproblem.Solutions)
            {
                for (int i = 0; i < solution.Count; i++)
                {
                    // No ned to check the result, since the solution
                    // set is pruned to only valid solutions.
                    board.TryPlace(squares[i], solution[i]);
                }
                // Prune the board with this solution.
                var newProb = Prune(board);
                if (newProb != null)
                {
                    // Resort based on the new pruned solutions.
                    newProb.Sort();
                    var ret = newProb.Solve(board);
                    if (ret != null)
                        return ret;
                }
                // Undo board state change.
                foreach (Point p in squares)
                    board.UnsetPlace(p);
            }
            // No solution in this branch!
            return null;
        }

        List<SubProblem> mAll = new List<SubProblem>();

        public void Solve()
        {
            // Just do some sanity checking to make sure every cell is part of a subproblem.
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (mSubProblems[x, y] == null)
                        Console.WriteLine("{0} {1}", x, y);
                    Debug.Assert(mSubProblems[x, y] != null);
                    mSubProblems[x, y].Squares.Add(new Point(x, y));
                }
            }
            // Generate the solution set for each SubProblem.
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    mSubProblems[x, y].GenerateSolutions();
                }
            }

            // Grab all the unique subproblems from the 9 by 9 board.
            Dictionary<SubProblem, int> subProblems = new Dictionary<SubProblem, int>();
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (!subProblems.ContainsKey(mSubProblems[x, y]))
                    {
                        var sub = mSubProblems[x, y];
                        subProblems.Add(sub, sub.Squares.Count);
                    }
                }
            }

            // Add the unique subproblems to a list.
            mAll = new List<SubProblem>();
            foreach (var val in subProblems.Keys)
            {
                var squares = val.Squares;
                // Filter out invalid solutions, ie, 6+ = 2 2 2 would never be valid.
                foreach (var solution in new List<List<int>>(val.Solutions))
                {
                    KenKenBoard test = new KenKenBoard();
                    for (int i = 0; i < solution.Count; i++)
                    {
                        if (!test.TryPlace(squares[i], solution[i]))
                        {
                            val.Solutions.Remove(solution);
                            break;
                        }
                    }
                }
                mAll.Add(val);
            }
            // Sort the list in order of increasing number of possible solutions.
            Sort();

            // Attempt to solve with a empty KenKenBoard.
            var ret = Solve(new KenKenBoard());
            if (ret != null)
                ret.Print();
            else
                Console.WriteLine("fail");
        }

        void Sort()
        {
            // Sort the subproblems by lowest number of valid solutions, to highest.
            // Prioritizing subproblems with less solutions prunes the tree faster.
            mAll.Sort(new Comparison<SubProblem>((x, y) =>
            {
                return x.Solutions.Count.CompareTo(y.Solutions.Count);
            }
            ));
        }
    }

    class MainClass
    {
        public static void Main(string[] args)
        {
            // Build up the board.
            // A board is a set of "subproblems" that piece together to form the entire "problem"
            // A subproblem would be, for example: s[0] = s[1] = ... = s[n] = SubProblem(Operator.Multiply, 18).
            // Meaning the squares 0 through n multiply to equal 18.
            Problem problem = new Problem();
            problem.SubProblems[0, 0] = problem.SubProblems[0, 1] = problem.SubProblems[1, 1] = new SubProblem(Operator.Multiply, 18);
            problem.SubProblems[1, 0] = problem.SubProblems[2, 0] = problem.SubProblems[3, 0] = problem.SubProblems[4, 0] = problem.SubProblems[4, 1] = new SubProblem(Operator.Multiply, 5040);
            problem.SubProblems[5, 0] = new SubProblem(Operator.Constant, 7);
            problem.SubProblems[6, 0] = problem.SubProblems[7, 0] = new SubProblem(Operator.Divide, 2);
            problem.SubProblems[8, 0] = problem.SubProblems[8, 1] = problem.SubProblems[7, 1] = new SubProblem(Operator.Multiply, 180);
            problem.SubProblems[2, 1] = problem.SubProblems[2, 2] = problem.SubProblems[1, 2] = new SubProblem(Operator.Add, 17);
            problem.SubProblems[3, 1] = problem.SubProblems[3, 2] = new SubProblem(Operator.Add, 7);
            problem.SubProblems[5, 1] = problem.SubProblems[5, 2] = new SubProblem(Operator.Add, 17);
            problem.SubProblems[6, 1] = problem.SubProblems[6, 2] = new SubProblem(Operator.Divide, 4);
            problem.SubProblems[0, 2] = problem.SubProblems[0, 3] = problem.SubProblems[0, 4] = new SubProblem(Operator.Add, 15);
            problem.SubProblems[4, 2] = problem.SubProblems[4, 3] = problem.SubProblems[5, 3] = new SubProblem(Operator.Add, 13);
            problem.SubProblems[7, 2] = problem.SubProblems[8, 2] = problem.SubProblems[8, 3] = problem.SubProblems[8, 4] = new SubProblem(Operator.Multiply, 56);
            problem.SubProblems[1, 3] = problem.SubProblems[2, 3] = problem.SubProblems[3, 3] = new SubProblem(Operator.Multiply, 224);
            problem.SubProblems[6, 3] = problem.SubProblems[7, 3] = new SubProblem(Operator.Subtract, 8);
            problem.SubProblems[1, 4] = problem.SubProblems[1, 5] = problem.SubProblems[0, 5] = new SubProblem(Operator.Multiply, 126);
            problem.SubProblems[2, 4] = problem.SubProblems[2, 5] = new SubProblem(Operator.Subtract, 3);
            problem.SubProblems[3, 4] = problem.SubProblems[4, 4] = problem.SubProblems[5, 4] = new SubProblem(Operator.Multiply, 35);
            problem.SubProblems[6, 4] = problem.SubProblems[6, 5] = problem.SubProblems[5, 5] = new SubProblem(Operator.Add, 8);
            problem.SubProblems[7, 4] = problem.SubProblems[7, 5] = problem.SubProblems[8, 5] = new SubProblem(Operator.Add, 15);
            problem.SubProblems[3, 5] = problem.SubProblems[4, 5] = new SubProblem(Operator.Add, 17);
            problem.SubProblems[0, 6] = problem.SubProblems[1, 6] = problem.SubProblems[2, 6] = problem.SubProblems[3, 6] = new SubProblem(Operator.Multiply, 216);
            problem.SubProblems[4, 6] = new SubProblem(Operator.Constant, 1);
            problem.SubProblems[5, 6] = problem.SubProblems[6, 6] = problem.SubProblems[7, 6] = problem.SubProblems[8, 6] = new SubProblem(Operator.Add, 26);
            problem.SubProblems[0, 7] = problem.SubProblems[1, 7] = new SubProblem(Operator.Subtract, 1);
            problem.SubProblems[2, 7] = problem.SubProblems[3, 7] = new SubProblem(Operator.Add, 3);
            problem.SubProblems[4, 7] = problem.SubProblems[4, 8] = problem.SubProblems[3, 8] = problem.SubProblems[5, 8] = new SubProblem(Operator.Add, 23);
            problem.SubProblems[5, 7] = problem.SubProblems[6, 7] = new SubProblem(Operator.Divide, 2);
            problem.SubProblems[7, 7] = problem.SubProblems[8, 7] = new SubProblem(Operator.Subtract, 1);
            problem.SubProblems[0, 8] = problem.SubProblems[1, 8] = problem.SubProblems[2, 8] = new SubProblem(Operator.Add, 14);
            problem.SubProblems[6, 8] = problem.SubProblems[7, 8] = problem.SubProblems[8, 8] = new SubProblem(Operator.Multiply, 135);

            problem.Solve();
        }
    }
}