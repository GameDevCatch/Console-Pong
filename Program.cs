using System;
using System.Threading;

namespace Pong
{
    //START GAME
    class Program
    {
        static void Main(string[] args)
        {
            Game newGame = new();
            newGame.Start();
        }
    }

    class Game
    {
        //CLASSES
        private struct Vector2
        {
            public int x { get; private set; }
            public int y { get; private set; }

            public Vector2(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
        private class Paddle
        {
            //ALL POSITIONS THAT MAKE UP PADDLE
            public Vector2[] Positions { get; private set; }

            public int Width { get; private set; }
            public int Height { get; private set; }

            public void MovePosition(int x, int y)
            {
                for (int i = 0; i < this.Positions.Length; i++)
                {
                    this.Positions[i] = new Vector2(this.Positions[i].x + x,
                                                    this.Positions[i].y + y);
                }
            }

            public Paddle(Vector2 origin, int width, int height)
            {
                this.Width = width;
                this.Height = height;

                this.Positions = new Vector2[height * width];

                int i = 0;

                //SET POSITIONS TO MAKE UP PADDLES HEIGHT AND WIDTH
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        if (i == 0)
                            this.Positions[i] = new Vector2(origin.x, origin.y);
                        else
                            this.Positions[i] = new Vector2(origin.x + k, origin.y + j);

                        i++;
                    }
                }
            }
        }
        private class Ball
        {
            public Vector2 pos;
            public Vector2 vel;

            public Ball(Vector2 origin)
            {
                pos = origin;
            }

            public void Reset(Vector2 originPos)
            {
                this.vel = new Vector2(0, 0);
                this.pos = originPos;
                Serve();
            }

            public void Serve()
            {
                Random rand = new Random();
                float randomFloat = (float)rand.NextDouble();

                //ONE OF FOUR SERVE ANGLES
                if (randomFloat < 0.25)
                    this.vel = new Vector2(1, -1);
                else if (randomFloat < 0.50 && randomFloat > .25f)
                    this.vel = new Vector2(-1, -1);
                else if (randomFloat < 0.75 && randomFloat > .50f)
                    this.vel = new Vector2(1, 1);
                else if (randomFloat < 1 && randomFloat > .75f)
                    this.vel = new Vector2(-1, 1);
            }
        }

        //ENUMS
        private enum TIME { RESUMED, PAUSED }
        private TIME _timeMode = TIME.RESUMED;

        //SPECIAL
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int key);

        //PRIVATE VARS
        private int _screenWidth = 70, _screenHeight = 15;
        private int _player1Score, _player2Score;
        private int _maxScore = 5;
        private string _winText;

        //PRIVATE OBJECTS
        private Paddle _player1Paddle;
        private Paddle _player2Paddle;
        private Ball _ball;

        public void Start()
        {
            //CREATE OBJECTS
            _player1Paddle = new Paddle(new Vector2(3, _screenHeight / 2 - 1), 2, 3);
            _player2Paddle = new Paddle(new Vector2(_screenWidth - 5, _screenHeight / 2 - 1), 2, 3);
            _ball = new Ball(new Vector2(_screenWidth / 2, _screenHeight / 2));

            _ball.Serve();

            //BEGIN GAME LOOP
            Update();
        }

        public void Update()
        {
            while (true)
            {
                Console.Clear();
                Draw();
                
                if (_timeMode == TIME.RESUMED)
                {
                    Input();
                    Logic();
                } else
                {
                    //PAUSE GAME
                    Thread.Sleep(800);
                    //RESUME GAME
                    _timeMode = TIME.RESUMED;
                    _player1Score = 0;
                    _player2Score = 0;
                }

                //FPS
                Thread.Sleep(30);
            }
        }

        public void Draw()
        {
            //LOOP THROUGH SCREEN LIKE A CRT SCREEN
            for (int y = 0; y < _screenHeight; y++)
            {
                for (int x = 0; x < _screenWidth; x++)
                {
                    //DRAW BOUNDRY
                    if (y == 0 || y == _screenHeight - 1)
                        Console.Write("#");

                    //DRAW PLAYER 1 SCORE
                    else if (y == 2 && x == _screenWidth / 2 - 4)
                        Console.Write(_player1Score.ToString("0"));

                    //DRAW PLAYER 2 SCORE
                    else if (y == 2 && x == _screenWidth / 2 + 4)
                        Console.Write(_player2Score.ToString("0"));

                    //DRAW WIN TEXT
                    else if (y == 4 && x == _screenWidth / 2 - 6 && _timeMode == TIME.PAUSED)
                        Console.Write(_winText);

                    //DRAW ENDS OF BOUNDRY
                    else if (x == 0 || x == _screenWidth - 1)
                        Console.Write("#");

                    //DRAW OBJECTS
                    else
                    {
                        bool empty = true;

                        //DRAW PLAYER 1
                        for (int i = 0; i < _player1Paddle.Positions.Length; i++)
                        {
                            if (x == _player1Paddle.Positions[i].x && y == _player1Paddle.Positions[i].y)
                            {
                                Console.Write("#");
                                empty = false;
                            }
                        }

                        //DRAW PLAYER 2
                        for (int i = 0; i < _player2Paddle.Positions.Length; i++)
                        {
                            if (x == _player2Paddle.Positions[i].x && y == _player2Paddle.Positions[i].y)
                            {
                                Console.Write("#");
                                empty = false;
                            }
                        }

                        //DRAW BALL
                        if (x == _ball.pos.x && y == _ball.pos.y)
                        {
                            Console.Write("@");
                            empty = false;
                        }

                        //ELSE DRAW NOTHING
                        if (empty)
                            Console.Write(" ");
                    }
                }

                Console.WriteLine("");
            }
        }

        private void Input()
        {
            bool[] keys = new bool[4];

            //FETCH CONTROLS
            for (int i = 0; i < 4; i++)
                keys[i] = (0x8000 & GetAsyncKeyState((char)"\x57\x53\x26\x28"[i])) != 0;

            //PLAYER 1 INPUT
            if (keys[0])
            {
                if (_player1Paddle.Positions[0].y > 1)
                    _player1Paddle.MovePosition(0, -1);
            } else if (keys[1]) 
            {
                if (_player1Paddle.Positions[_player1Paddle.Positions.Length - 1].y < _screenHeight - 2)
                    _player1Paddle.MovePosition(0, 1);
            }

            //PLAYER 2 INPUT
            if (keys[2])
            {
                if (_player2Paddle.Positions[0].y > 1)
                    _player2Paddle.MovePosition(0, -1);
            }
            else if (keys[3])
            {
                if (_player2Paddle.Positions[_player2Paddle.Positions.Length - 1].y < _screenHeight - 2)
                    _player2Paddle.MovePosition(0, 1);
            }
        }

        private void Logic()
        {
            //MOVE BALL
            _ball.pos = new Vector2(_ball.pos.x + _ball.vel.x, _ball.pos.y + _ball.vel.y);

            //CLAMP BALL
            if (_ball.pos.y < 2 || _ball.pos.y > _screenHeight - 3)
                _ball.vel = new Vector2(_ball.vel.x, -_ball.vel.y);

            //PLAYER 1 BALL COLLISION
            for (int i = 0; i < _player1Paddle.Positions.Length; i++)
            {
                if (_ball.pos.y == _player1Paddle.Positions[i].y && _ball.pos.x <= _player1Paddle.Positions[0].x + 2)
                    _ball.vel = new Vector2(1, _ball.vel.y);
            }

            //PLAYER 2 BALL COLLISION
            for (int i = 0; i < _player2Paddle.Positions.Length; i++)
            {
                if (_ball.pos.y == _player2Paddle.Positions[i].y && _ball.pos.x >= _player2Paddle.Positions[0].x - 1)
                    _ball.vel = new Vector2(-1, _ball.vel.y);
            }

            //BALL GOAL COLLISION
            if (_ball.pos.x > _screenWidth - 2)
            {
                _ball.Reset(new Vector2(_screenWidth / 2, _screenHeight / 2));
                _player1Score++;
            }
            else if (_ball.pos.x < 2)
            {
                _ball.Reset(new Vector2(_screenWidth / 2, _screenHeight / 2));
                _player2Score++;
            }

            if (_player1Score >= _maxScore)
                Won(1);

            if (_player2Score >= _maxScore)
                Won(2);
        }

        private void Won(int player)
        {
            _timeMode = TIME.PAUSED;

            switch (player)
            {
                case 1:
                    _winText = "Player 1 Wins!";
                    break;

                case 2:
                    _winText = "Player 2 Wins!";
                    break;
            }
        }
    }
}
