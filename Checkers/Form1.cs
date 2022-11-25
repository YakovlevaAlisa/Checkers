using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Checkers
{
    public partial class Form1 : Form
    {
        const int mapSize = 8;
        const int cellSize = 70;
        int countEatSteps = 0;
        int currentPlayer;

        List<Button> simpleSteps = new List<Button>();

        Button prevButton;
        Button pressedButton;

        bool isMoving;
        bool isContinue = false;

        int[,] map = new int[mapSize, mapSize];
        Button[,] buttons = new Button[mapSize, mapSize];

        Image whiteFigure;
        Image blackFigure;

        public Form1()
        {
            InitializeComponent();

            whiteFigure = new Bitmap(new Bitmap(@"D:\VS\Checkers\Checkers\Checkers\WhiteChecker.png"), new Size(cellSize - 10, cellSize - 10));
            blackFigure = new Bitmap(new Bitmap(@"D:\VS\Checkers\Checkers\Checkers\BlackChecker.png"), new Size(cellSize - 10, cellSize - 10));

            this.Text = "Checkers";

            Init();
        }

        public void Init()
        {
            currentPlayer = 1;
            isMoving = false;
            prevButton = null;

            map = new int[mapSize, mapSize];

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if ((i + j) % 2 == 1 && i < 3) map[i, j] = 1;
                    else if ((i + j) % 2 == 1 && i > 4) map[i, j] = 2;
                    else map[i, j] = 0;
                }
            }

            CreateMap();
        }

        public void ResetGame()
        {
            bool player1 = false;
            bool player2 = false;

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (map[i, j] == 1)
                        player1 = true;
                    if (map[i, j] == 2)
                        player2 = true;
                }
            }
            if (!player1 || !player2)
            {
                this.Controls.Clear();
                Init();
            }
        }

        public void CreateMap()
        {
            this.Width = (mapSize + 1) * cellSize;
            this.Height = (mapSize + 1) * cellSize;

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    Button button = new Button();
                    button.Location = new Point(j * cellSize, i * cellSize);
                    button.Size = new Size(cellSize, cellSize);
                    button.Click += new EventHandler(OnFigurePress);

                    if (map[i, j] == 1) button.Image = whiteFigure;
                    else if (map[i, j] == 2) button.Image = blackFigure;

                    button.BackColor = GetPrevButtonColor(button);
                    button.ForeColor = Color.Red;

                    buttons[i, j] = button;

                    this.Controls.Add(button);
                }
            }
        }

        public void SwitchPlayer()
        {
            currentPlayer = (currentPlayer == 1) ? 2 : 1;
            ResetGame();
        }

        public Color GetPrevButtonColor(Button prevButton)
        {
            return ((prevButton.Location.Y / cellSize + prevButton.Location.X / cellSize) % 2 == 0 ? Color.Tan : Color.SaddleBrown);
        }

        public void OnFigurePress(object sender, EventArgs e)
        {
            if (prevButton != null) prevButton.BackColor = GetPrevButtonColor(prevButton);

            pressedButton = sender as Button;

            if (map[pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize] != 0 && map[pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize] == currentPlayer)
            {
                CloseSteps();
                pressedButton.BackColor = Color.Red;
                DeactivateAllButtons();
                pressedButton.Enabled = true;
                countEatSteps = 0;
                if (pressedButton.Text == "King")
                    ShowSteps(pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize, false);
                else 
                    ShowSteps(pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize);
                if (isMoving)
                {
                    CloseSteps();
                    pressedButton.BackColor = GetPrevButtonColor(pressedButton);
                    ShowPossibleSteps();
                    isMoving = false;
                }
                else
                    isMoving = true;
            }
            else
            {
                if (isMoving)
                {
                    isContinue = false;
                    if (Math.Abs(pressedButton.Location.X / cellSize - prevButton.Location.X / cellSize) > 1)
                    {
                        isContinue = true;
                        DeleteEaten(pressedButton, prevButton);
                    }
                    int temp = map[pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize];
                    map[pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize] = map[prevButton.Location.Y / cellSize, prevButton.Location.X / cellSize];
                    map[prevButton.Location.Y / cellSize, prevButton.Location.X / cellSize] = temp;
                    pressedButton.Image = prevButton.Image;
                    prevButton.Image = null;
                    pressedButton.Text = prevButton.Text;
                    prevButton.Text = "";
                    SwitchButtonToCheat(pressedButton);
                    countEatSteps = 0;
                    isMoving = false;
                    CloseSteps();
                    DeactivateAllButtons();
                    if (pressedButton.Text == "King")
                        ShowSteps(pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize, false);
                    else
                        ShowSteps(pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize);
                    if (countEatSteps == 0 || !isContinue)
                    {
                        CloseSteps();
                        SwitchPlayer();
                        ShowPossibleSteps();
                        isContinue = false;
                    }
                    else if (isContinue)
                    {
                        pressedButton.BackColor = Color.Red;
                        pressedButton.Enabled = true;
                        isMoving = true;
                    }
                }
            }
            prevButton = pressedButton;
        }

        public void ShowPossibleSteps()
        {
            bool isOneStep = true;
            bool isEatStep = false;
            DeactivateAllButtons();
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (map[i, j] == currentPlayer)
                    {
                        if (buttons[i, j].Text == "King")
                            isOneStep = false;
                        else
                            isOneStep = true;
                        if (IsButtonHasEatStep(i, j, isOneStep, new int[2] { 0, 0 }))
                        {
                            isEatStep = true;
                            buttons[i, j].Enabled = true;
                        }
                    }
                }
            }
            if (!isEatStep)
                ActivateAllButtons();
        }

        public void SwitchButtonToCheat(Button button)
        {
            if (map[button.Location.Y / cellSize, button.Location.X / cellSize] == 1 && button.Location.Y / cellSize == mapSize - 1 ||
                map[button.Location.Y / cellSize, button.Location.X / cellSize] == 2 && button.Location.Y / cellSize == 0)
                button.Text = "King";
        }

        public void DeleteEaten(Button endButton, Button startButton)
        {
            int count = Math.Abs(endButton.Location.Y / cellSize - startButton.Location.Y / cellSize);
            int startIndexX = endButton.Location.Y / cellSize - startButton.Location.Y / cellSize;
            int startIndexY = endButton.Location.X / cellSize - startButton.Location.X / cellSize;
            startIndexX = startIndexX < 0 ? -1 : 1;
            startIndexY = startIndexY < 0 ? -1 : 1;
            int currCount = 0;
            int i = startButton.Location.Y / cellSize + startIndexX;
            int j = startButton.Location.X / cellSize + startIndexY;
            while (currCount < count - 1)
            {
                map[i, j] = 0;
                buttons[i, j].Image = null;
                buttons[i, j].Text = "";
                i += startIndexX;
                j += startIndexY;
                currCount++;
            }
        }

        public void ShowSteps(int iCurrFigure, int jCurrFigure, bool isOnestep = true)
        {
            simpleSteps.Clear();
            ShowDiagonal(iCurrFigure, jCurrFigure, isOnestep);
            if (countEatSteps > 0)
                CloseSimpleSteps(simpleSteps);
        }

        public void ShowDiagonal(int IcurrFigure, int JcurrFigure, bool isOneStep = false)
        {
            int j = JcurrFigure + 1;
            for (int i = IcurrFigure - 1; i >= 0; i--)
            {
                if (currentPlayer == 1 && isOneStep && !isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j < mapSize - 1)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure - 1;
            for (int i = IcurrFigure - 1; i >= 0; i--)
            {
                if (currentPlayer == 1 && isOneStep && !isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure - 1;
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (currentPlayer == 2 && isOneStep && !isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure + 1;
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (currentPlayer == 2 && isOneStep && !isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j < mapSize - 1)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }
        }

        public bool DeterminePath(int indexI, int indexJ)
        {

            if (map[indexI, indexJ] == 0 && !isContinue)
            {
                buttons[indexI, indexJ].BackColor = Color.Yellow;
                buttons[indexI, indexJ].Enabled = true;
                simpleSteps.Add(buttons[indexI, indexJ]);
            }
            else
            {
                if (map[indexI, indexJ] != currentPlayer)
                {
                    if (pressedButton.Text == "King")
                        ShowProceduralEat(indexI, indexJ, false);
                    else
                        ShowProceduralEat(indexI, indexJ);
                }
                return false;
            }
            return true;
        }

        public void CloseSimpleSteps(List<Button> simpleSteps)
        {
            if (simpleSteps.Count > 0)
            {
                for (int i = 0; i < simpleSteps.Count; i++)
                {
                    simpleSteps[i].BackColor = GetPrevButtonColor(simpleSteps[i]);
                    simpleSteps[i].Enabled = false;
                }
            }
        }

        public void ShowProceduralEat(int i, int j, bool isOneStep = true)
        {
            int directionX = i - pressedButton.Location.Y / cellSize;
            int directionY = j - pressedButton.Location.X / cellSize;
            directionX = directionX < 0 ? -1 : 1;
            directionY = directionY < 0 ? -1 : 1;
            int il = i;
            int jl = j;
            bool isEmpty = true;
            while (IsInsideBorders(il, jl))
            {
                if (map[il, jl] != 0 && map[il, jl] != currentPlayer)
                {
                    isEmpty = false;
                    break;
                }
                il += directionX;
                jl += directionY;

                if (isOneStep)
                    break;
            }
            if (isEmpty)
                return;
            List<Button> toClose = new List<Button>();
            bool closeSimple = false;
            int ik = il + directionX;
            int jk = jl + directionY;
            while (IsInsideBorders(ik, jk))
            {
                if (map[ik, jk] == 0)
                {
                    if (IsButtonHasEatStep(ik, jk, isOneStep, new int[2] { directionX, directionY }))
                    {
                        closeSimple = true;
                    }
                    else
                    {
                        toClose.Add(buttons[ik, jk]);
                    }
                    buttons[ik, jk].BackColor = Color.Yellow;
                    buttons[ik, jk].Enabled = true;
                    countEatSteps++;
                }
                else break;
                if (isOneStep)
                    break;
                jk += directionY;
                ik += directionX;
            }
            if (closeSimple && toClose.Count > 0)
                CloseSimpleSteps(toClose);
        }

        public bool IsButtonHasEatStep(int IcurrFigure, int JcurrFigure, bool isOneStep, int[] direction)
        {
            bool eatStep = false;
            int j = JcurrFigure + 1;
            for (int i = IcurrFigure - 1; i >= 0; i--)
            {
                if (currentPlayer == 1 && isOneStep && !isContinue) break;
                if (direction[0] == 1 && direction[1] == -1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i - 1, j + 1))
                            eatStep = false;
                        else if (map[i - 1, j + 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure - 1;
            for (int i = IcurrFigure - 1; i >= 0; i--)
            {
                if (currentPlayer == 1 && isOneStep && !isContinue) break;
                if (direction[0] == 1 && direction[1] == 1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i - 1, j - 1))
                            eatStep = false;
                        else if (map[i - 1, j - 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure - 1;
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (currentPlayer == 2 && isOneStep && !isContinue) break;
                if (direction[0] == -1 && direction[1] == 1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i + 1, j - 1))
                            eatStep = false;
                        else if (map[i + 1, j - 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure + 1;
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (currentPlayer == 2 && isOneStep && !isContinue) break;
                if (direction[0] == -1 && direction[1] == -1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i + 1, j + 1))
                            eatStep = false;
                        else if (map[i + 1, j + 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }
            return eatStep;
        }

        public void CloseSteps()
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                    buttons[i, j].BackColor = GetPrevButtonColor(buttons[i, j]);
            }
        }

        public bool IsInsideBorders(int indexI, int indexJ)
        {
            if (indexI >= mapSize || indexJ >= mapSize || indexI < 0 || indexJ < 0)
                return false;
            return true;
        }

        public void ActivateAllButtons()
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                    buttons[i, j].Enabled = true;
            }
        }

        public void DeactivateAllButtons()
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                    buttons[i, j].Enabled = false;
            }
        }
    }
}
