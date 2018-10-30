using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using MachineLearning_TicTacToe.ExtensionMethods;
using System.Windows;

namespace MachineLearning_TicTacToe.ML_Module
{
    class AI
    {
        private static List<double> _weights;
        private List<KeyValuePair<double, List<int>>> _history = new List<KeyValuePair<double, List<int>>>();
        public Mark InstanceRole { get; private set; }
        private Mark _opponentRole;

        static AI()
        {
            if (_weights == null)
            {
                Random rand = new Random();
                if (File.Exists("weights.dat"))
                    _weights = JsonConvert.DeserializeObject<List<double>>(File.ReadAllText("weights.dat"));
                else
                    _weights = Enumerable.Range(0, 7).Select(i => (double)rand.Next(-100, 101)).ToList();
            }
        }

        public AI(Mark instanceRole)
        {
            InstanceRole = instanceRole;
            _opponentRole = (InstanceRole == Mark.O) ? Mark.X : Mark.O;
        }

        public static void SaveWeights() { File.WriteAllText("weights.dat", JsonConvert.SerializeObject(_weights)); }

        public Point TakeTurn(Mark[,] gameField)
        {
            Dictionary<double, Point> options = new Dictionary<double, Point>();
            Dictionary<double, List<int>> summarys = new Dictionary<double, List<int>>();
            for (int i = 0; i < gameField.GetLength(0); i++)
            {
                for (int j = 0; j < gameField.GetLength(1); j++)
                {
                    if (gameField[i, j] == Mark.Blank)
                    {
                        Mark[,] option = (Mark[,])gameField.Clone();
                        option[i, j] = InstanceRole;
                        (List<int> features, double evaluation) = Summarize(option);
                        if (!options.Keys.Contains(evaluation))
                        {
                            options.Add(evaluation, new Point(j, i));
                            summarys.Add(evaluation, features);
                        }
                    }
                }
            }

            KeyValuePair<double, Point> pair = options.Where(p => p.Key == options.Keys.Max()).ToList()[0];
                _history.Add(new KeyValuePair<double, List<int>>(pair.Key, summarys[pair.Key]));
            return pair.Value;
        }

        public void ProcessResult(GameResult result, double learningRate)
        {
            for (int i = _history.Count - 1; i >= 0; i--)
            {
                _weights[0] = _weights[0] + learningRate * 
                              (((i == _history.Count - 1) ? (double)result 
                                                          : _history[i + 1].Key) - _history[i].Key);
                for (int j = 1; j < _weights.Count; j++)
                {
                    _weights[j] = _weights[j] + learningRate *
                        (((i == _history.Count - 1) ? (double)result
                                                    : _history[i + 1].Key) - _history[i].Key)
                                                    * _history[i].Value[j - 1];
                }
            }
        }

        private (List<int> features, double summary) Summarize(Mark[,] gameField)
        {
            int completedSequences_Self = 0;
            int completedSequences_Opponent = 0;
            int sequencesToBeCompleted_Self = 0;
            int sequencesToBeComplited_Opponent = 0;
            int singleMarks_Self = 0;
            int singleMarks_Opponent = 0;

            List<Mark[]> rows = new List<Mark[]>();
            List<Mark[]> columns = new List<Mark[]>();
            List<Mark[]> diagonals = new List<Mark[]>();

            for (int i = 0; i < gameField.GetLength(0); i++)
                rows.Add(gameField.GetRow(i));
            for (int i = 0; i < gameField.GetLength(1); i++)
                columns.Add(gameField.GetColumn(i));
            diagonals.Add(gameField.GetDiagonal(DiagonalType.Left));
            diagonals.Add(gameField.GetDiagonal(DiagonalType.Right));

            foreach (var line in rows.Concat(columns).Concat(diagonals))
            {
                if (!line.ToList().Exists(m => m != InstanceRole))
                    completedSequences_Self++;
                if (!line.ToList().Exists(m => m != _opponentRole))
                    completedSequences_Opponent++;

                if (line.Where(m => m == InstanceRole).Count() > 1 && !line.Contains(_opponentRole))
                    sequencesToBeCompleted_Self++;
                if (line.Where(m => m == _opponentRole).Count() > 1 && !line.Contains(InstanceRole))
                    sequencesToBeComplited_Opponent++;

                if (line.Where(m => m == InstanceRole).Count() == 1 && !line.Contains(_opponentRole))
                    singleMarks_Self++;
                if (line.Where(m => m == _opponentRole).Count() == 1 && !line.Contains(InstanceRole))
                    singleMarks_Opponent++;
            }

            return (new List<int>
                   { completedSequences_Self,
                     completedSequences_Opponent,
                     sequencesToBeCompleted_Self,
                     sequencesToBeComplited_Opponent,
                     singleMarks_Self,
                     singleMarks_Opponent },

                    _weights[0] + _weights[1] * completedSequences_Self
                                + _weights[2] * completedSequences_Opponent
                                + _weights[3] * sequencesToBeCompleted_Self
                                + _weights[4] * sequencesToBeComplited_Opponent
                                + _weights[5] * singleMarks_Self
                                + _weights[6] * singleMarks_Opponent);
        }

        public void Refresh() { _history.Clear(); }
    }
}
