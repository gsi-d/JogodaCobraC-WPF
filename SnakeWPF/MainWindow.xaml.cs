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
using System.Windows.Threading;

namespace SnakeWPF
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        const int SnakeSquareSize = 20;
        const int SnakeStartLenght = 2;
        const int SnakeStartSpeed = 200;
        const int SnakeSpeedThreshold = 100;

        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.Yellow;
        private List<SnakePart> snakeParts = new List<SnakePart>();

        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLength;

        private DispatcherTimer gameTickTimer = new DispatcherTimer();
        private Random rnd = new Random();

        private UIElement snakeApple = null;
        BitmapImage bitSnakeApple = new BitmapImage(new Uri("apple.png", UriKind.Relative));
        Image snakeAppleImg = null;
        private SolidColorBrush appleColor = Brushes.Red;

        private int currentScore = 0;

        public MainWindow()
        {
            InitializeComponent();
            gameTickTimer.Tick += GameTickTimer_Tick;
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;

                case Key.W:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.S:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.A:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.D:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
            }
            if (snakeDirection != originalSnakeDirection)
                MoveSnake();
        }

        private void DrawGameArea()
        {
            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;
            int rowCounter = 0;
            bool nextIsOdd = false;

            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = Brushes.Black
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                nextIsOdd = !nextIsOdd;
                nextX += SnakeSquareSize;

                if (nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }

                doneDrawingBackground = nextY >= GameArea.ActualHeight ? true : false;
            }
        }

        private void DrawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }

        private void MoveSnake()
        {
            // Remove a última parte da serpente 
            while (snakeParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            }
            // Next up, we'll add a new element to the snake, which will be the (new) head  
            // Therefore, we mark all existing parts as non-head (body) elements and then  
            // we make sure that they use the body brush  
            foreach (SnakePart snakePart in snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }

            //Determina em qual direção expandir a snake, baseada na direção atual
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    if (snakeHead.Position.X <= 0)
                    {
                        nextX = SnakeSquareSize * 20;
                    }
                    else
                    {
                        nextX -= SnakeSquareSize;
                    }
                    break;
                case SnakeDirection.Right:
                    if (snakeHead.Position.X >= GameArea.ActualWidth)
                    {
                        nextX = 0;
                    }
                    else
                    {
                        nextX += SnakeSquareSize;
                    }

                    break;
                case SnakeDirection.Up:
                    if (snakeHead.Position.Y <= 0)
                    {
                        nextY = SnakeSquareSize * 20;
                    }
                    else
                    {
                        nextY -= SnakeSquareSize;
                    }

                    break;
                case SnakeDirection.Down:
                    if (snakeHead.Position.Y >= GameArea.ActualHeight)
                    {
                        nextY = 0;
                    }
                    else
                    {
                        nextY += SnakeSquareSize;
                    }

                    break;
            }

            // Agora adicionamos a nova head para a lista snakeParts
            snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });
            //... and then have it drawn!  
            DrawSnake();
            // We'll get to this later...  
            DoCollisionCheck();
        }

        private void StartNewGame()
        {
            //APAGA A SNAKE DA ÁREA DE JOGO E DA LISTA snakeParts
            foreach (SnakePart snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);

                if (snakeAppleImg != null)
                    GameArea.Children.Remove(snakeAppleImg);

            }
            snakeParts.Clear();

            currentScore = 0;
            snakeLength = SnakeStartLenght;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakePart() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);

            DrawSnake();
            DrawSnakeApple();
            UpdateGameStatus();

            // Inicio   
            gameTickTimer.IsEnabled = true;
        }

        private Point GetNextApplePosition()
        {
            int maxX = (int)(GameArea.Width / SnakeSquareSize);
            int maxY = (int)(GameArea.Height / SnakeSquareSize);
            int appleX = rnd.Next(0, maxX) * SnakeSquareSize;
            int appleY = rnd.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakePart snakePart in snakeParts)
            {
                if ((snakePart.Position.X == appleX) && (snakePart.Position.Y == appleY))
                    return GetNextApplePosition();
            }
            return new Point(appleX, appleY);
        }

        private void DrawSnakeApple()
        {
            Point applePosition = GetNextApplePosition();
            snakeAppleImg = new Image()
            {
                Source = bitSnakeApple,
                Width = SnakeSquareSize,
                Height = SnakeSquareSize
            };
            GameArea.Children.Add(snakeAppleImg);

            /*snakeApple = new Ellipse()
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = appleColor
            };
            GameArea.Children.Add(snakeApple);*/
            Canvas.SetTop(snakeAppleImg, applePosition.X);
            Canvas.SetLeft(snakeAppleImg, applePosition.Y);
        }

        private void DoCollisionCheck()
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];

            if (snakeHead.Position.X == Canvas.GetLeft(snakeAppleImg) && (snakeHead.Position.Y == Canvas.GetTop(snakeAppleImg)))
            {
                EatSnakeApple();
                return;
            }

            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                    EndGame();
            }
        }

        private void EatSnakeApple()
        {
            snakeLength++;
            currentScore++;
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(snakeAppleImg);
            DrawSnakeApple();
            UpdateGameStatus();
        }

        private void UpdateGameStatus()
        {
            this.Title = "SnakeWPF - Score: " + currentScore + " - Game speed: " + gameTickTimer.Interval.TotalMilliseconds;
        }

        private void EndGame()
        {
            gameTickTimer.IsEnabled = false;
            MessageBox.Show("Oops, você morreu!\n\nPara começar um novo jogo, basta pressionar a tecla Espaço...", "SnakeWPF");
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            SnakePart snakeHead = new SnakePart();
            foreach (SnakePart snakePartHead in snakeParts)
            {
                if (snakePartHead.IsHead == true)
                {
                    snakeHead = snakePartHead;
                }
            }

            Point clickPosition = e.GetPosition(snakeHead as IInputElement);
            double rightDiference;
            double leftDiference;
            double downDiference;
            double upDiference;


            if (clickPosition.X > snakeHead.Position.X)
            {
                rightDiference = clickPosition.X - snakeHead.Position.X;
                if (clickPosition.Y > snakeHead.Position.Y)
                {
                    downDiference = clickPosition.Y - snakeHead.Position.Y;
                    snakeDirection = rightDiference > downDiference ? SnakeDirection.Right : SnakeDirection.Down;
                }
                else
                {
                    upDiference = snakeHead.Position.Y - clickPosition.Y;
                    snakeDirection = rightDiference > upDiference ? SnakeDirection.Right : SnakeDirection.Up;
                }
            }

            else if (clickPosition.X < snakeHead.Position.X)
            {
                leftDiference = snakeHead.Position.X - clickPosition.X;
                if (clickPosition.Y > snakeHead.Position.Y)
                {
                    downDiference = clickPosition.Y - snakeHead.Position.Y;
                    snakeDirection = leftDiference > downDiference ? SnakeDirection.Left : SnakeDirection.Down;
                }
                else
                {
                    upDiference = snakeHead.Position.Y - clickPosition.Y;
                    snakeDirection = leftDiference > upDiference ? SnakeDirection.Left : SnakeDirection.Up;
                }
            }
            else
            {
                snakeDirection = clickPosition.Y > snakeHead.Position.Y ? SnakeDirection.Down : SnakeDirection.Up;
            }
            if (snakeDirection != originalSnakeDirection)
                MoveSnake();
        }
    }
}
