using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MachineLearning_TicTacToe.ML_Module;
using System.Reflection;
using MachineLearning_TicTacToe.ExtensionMethods;

namespace MachineLearning_TicTacToe
{ 
    public partial class MainWindow : Window
    {
        private Mark _playerRole = Mark.O;
        private Mark _opponentRole = Mark.X;
        private AI _opponentInstance;
        private AI _assistantInstance;

        public MainWindow()
        {
            InitializeComponent();
            _opponentInstance = new AI(_opponentRole);
            _assistantInstance = new AI(_playerRole);

            var T = Type.GetType("System.Windows.Controls.Grid+GridLinesRenderer," +
                                 "PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            var GLR = Activator.CreateInstance(T);
            GLR.GetType().GetField("s_oddDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.Black, 1.0));
            GLR.GetType().GetField("s_evenDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.Black, 1.0));
            RefreshField();
            FirstTurn();
        }

        private void RefreshField()
        {
            foreach (Button button in GameField.Children)
            {
                button.Tag = Mark.Blank;
                (button.Content as Image).Source = null;
            }
        }

        private void FirstTurn()
        {
            Random rand = new Random();
            var decision = new Point(rand.Next(0, 3), rand.Next(0, 3));
            Button area = GameField.Children.Cast<UIElement>().First(a => Grid.GetRow(a) == decision.Y && Grid.GetColumn(a) == decision.X) as Button;
            area.Tag = _opponentRole;
            (area.Content as Image).Source = (_opponentRole == Mark.O) ? new BitmapImage(new Uri("/MachineLearning_TicTacToe;component/Resources/circle.png", UriKind.Relative))
                                                                       : new BitmapImage(new Uri("/MachineLearning_TicTacToe;component/Resources/cross.png", UriKind.Relative));
        }

        private void GameField_Click(object sender, RoutedEventArgs e)
        {
            if ((Mark)(sender as Button).Tag == Mark.Blank)
            {
                (sender as Button).Tag = _playerRole;
                ((sender as Button).Content as Image).Source = (_playerRole == Mark.X) ? new BitmapImage(new Uri("/MachineLearning_TicTacToe;component/Resources/cross.png", UriKind.Relative))
                                                                                       : new BitmapImage(new Uri("/MachineLearning_TicTacToe;component/Resources/circle.png", UriKind.Relative));

                var gameState = CheckWinningState(BuildMatrix());
                if (gameState.winningState)
                    EndGame(gameState.winningMark, gameState.positions);
                else if (BuildMatrix().Cast<Mark>().ToList().Contains(Mark.Blank))
                {
                    var decision = _opponentInstance.TakeTurn(BuildMatrix());
                    Button area = GameField.Children.Cast<UIElement>().First(a => Grid.GetRow(a) == decision.Y && Grid.GetColumn(a) == decision.X) as Button;
                    area.Tag = _opponentRole;
                    (area.Content as Image).Source = (_opponentRole == Mark.O) ? new BitmapImage(new Uri("/MachineLearning_TicTacToe;component/Resources/circle.png", UriKind.Relative))
                                                                               : new BitmapImage(new Uri("/MachineLearning_TicTacToe;component/Resources/cross.png", UriKind.Relative));
                    gameState = CheckWinningState(BuildMatrix());
                    if (gameState.winningState)
                        EndGame(gameState.winningMark, gameState.positions);
                    else if (!BuildMatrix().Cast<Mark>().ToList().Contains(Mark.Blank))
                        EndGame(null, null);
                }
                else EndGame(null, null);
            }
        }

        private (bool winningState, Mark? winningMark, List<Point> positions) CheckWinningState(Mark[,] gameField)
        {
            for (int i = 0; i < gameField.GetLength(0); i++)
            {
                Mark[] row = gameField.GetRow(i);
                if (row.ToList().FindAll(e => e == Mark.O).Count == GameField.ColumnDefinitions.Count ||
                    row.ToList().FindAll(e => e == Mark.X).Count == GameField.ColumnDefinitions.Count)
                    return (true, row[0], new List<Point>() { new Point(0, i), new Point(GameField.ColumnDefinitions.Count - 1, i) });
            }

            for (int i = 0; i < gameField.GetLength(1); i++)
            {
                Mark[] column = gameField.GetColumn(i);
                if (column.ToList().FindAll(e => e == Mark.O).Count == GameField.RowDefinitions.Count ||
                    column.ToList().FindAll(e => e == Mark.X).Count == GameField.RowDefinitions.Count)
                    return (true, column[0], new List<Point>() { new Point(i, 0), new Point(i, GameField.RowDefinitions.Count - 1) });
            }

            var diagonals = new List<Mark[]>() { gameField.GetDiagonal(DiagonalType.Left), gameField.GetDiagonal(DiagonalType.Right) };
            foreach (var diagonal in diagonals)
            {
                if (diagonal.ToList().FindAll(e => e == Mark.O).Count == GameField.RowDefinitions.Count ||
                    diagonal.ToList().FindAll(e => e == Mark.X).Count == GameField.RowDefinitions.Count)
                    return (true, diagonal[0], (diagonals.IndexOf(diagonal) == 0) ?
                         new List<Point>() { new Point(0, 0), new Point(GameField.ColumnDefinitions.Count - 1, GameField.RowDefinitions.Count - 1) }
                       : new List<Point>() { new Point(GameField.ColumnDefinitions.Count - 1, 0), new Point(0, GameField.RowDefinitions.Count - 1) });
            }

            return (false, null, null);
        }

        private void EndGame(Mark? winningMark, List<Point> positions)
        {
            Info.Visibility = Visibility.Visible;
            if (!winningMark.HasValue)
                Info.Content = "Draw";
            else Info.Content = $"{winningMark.Value.ToString("G")} won"; 
        }

        private Mark[,] BuildMatrix()
        {
            Mark[,] matrix = new Mark[GameField.RowDefinitions.Count, GameField.ColumnDefinitions.Count];
            foreach (Control area in GameField.Children)
                matrix[Grid.GetRow(area), Grid.GetColumn(area)] = (Mark)area.Tag;
            return matrix;
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            RefreshField();
            _opponentInstance.Refresh();
            Info.Visibility = Visibility.Hidden;
            FirstTurn();
        }

        private async void Button_Train_Click(object sender, RoutedEventArgs e)
        {
            RefreshField();
            Info.Visibility = Visibility.Visible;
            Info.Content = "Training\nin progress...";
            await Task.Run(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    _opponentInstance.Refresh();
                    _assistantInstance.Refresh();
                    Mark[,] gameField = new Mark[GameField.RowDefinitions.Count, GameField.ColumnDefinitions.Count];
                    var gameState = CheckWinningState(gameField);
                    while (!gameState.winningState && gameField.Cast<Mark>().ToList().Contains(Mark.Blank))
                    {
                        var decision = _assistantInstance.TakeTurn(gameField);
                        gameField[(int)decision.Y, (int)decision.X] = _assistantInstance.InstanceRole;
                        gameState = CheckWinningState(gameField);

                        if (!gameState.winningState && gameField.Cast<Mark>().ToList().Contains(Mark.Blank))
                        {
                            decision = _opponentInstance.TakeTurn(gameField);
                            gameField[(int)decision.Y, (int)decision.X] = _opponentInstance.InstanceRole;
                            gameState = CheckWinningState(gameField);
                        }
                        else break;
                    }

                    if (!gameState.winningState)
                    {
                        _assistantInstance.ProcessResult(GameResult.Draw, 0.1);
                        _opponentInstance.ProcessResult(GameResult.Draw, 0.1);
                    }
                    else
                    {
                        _assistantInstance.ProcessResult(gameState.winningMark == _assistantInstance.InstanceRole ? GameResult.Won : GameResult.Lost, 0.1);
                        _opponentInstance.ProcessResult(gameState.winningMark == _opponentInstance.InstanceRole ? GameResult.Won : GameResult.Lost, 0.1);
                    }
                }
            });
            Info.Visibility = Visibility.Hidden;
            AI.SaveWeights();
        }
    }
}
