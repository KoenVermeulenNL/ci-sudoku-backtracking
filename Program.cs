
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Xml;

class Program
{
    static void Main()
    {
        int evaluation_value;
        int[,] sudoku = new int[9, 9];
        int[,] static_sudoku = new int[9, 9];
        int[,] block_bool = new int[3, 3];
        int block_bool_full;
        int switches;

        import();

        //Import() is de enige functie die aangeroepen wordt in Main(), hierin wordt de inpu-file gelezen en wordt het algoritme uitgevoerd (dit kunnen we nog scheiden)
        void import()
        {
            int index;
            int counter;
            string[] woorden;
            char[] separators = { ' ' };
            string regel;

            StreamReader sr = new StreamReader("../../../sudokus.txt");

            while ((regel = sr.ReadLine()) != null)
            {
                //1 regel representeerd 1 sudoku
                //split de regel en zet het om naar een sudoku bord
                woorden = regel.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (woorden.Length == 81)
                {
                    //valid sudoku

                    index = 0;
                    for (int y = 0; y < 9; y++)
                        for (int x = 0; x < 9; x++)
                        {
                            sudoku[x, y] = int.Parse(woorden[index]);
                            static_sudoku[x, y] = int.Parse(woorden[index]);
                            index++;
                        }

                    //print de originele input
                    printSudoku(static_sudoku);
                    Console.WriteLine();

                    //setup timer
                    double setupTime = 0;
                    double totalTime = 0;
                    var setupTimer = System.Diagnostics.Stopwatch.StartNew();

                    //De sudoko wordt at random zo ingevuld dat in alle 9 blokken de cijfers 1 t/m 9 staan, maar dus nog niet in de rijen/kolommen
                    random_start();

                    //Evaluatie van het aanvankelijk at random gegenereerde sudokubord
                    initial_evaluation();

                    //We blijven de sudoku opnieuw at random invullen totdat we een initiële evaluatiewaarde hebben lager dan 30
                    while (evaluation_value > 30)
                    {
                        random_start();
                        initial_evaluation();
                    }

                    setupTimer.Stop();
                    setupTime += Math.Round(setupTimer.Elapsed.TotalMilliseconds);
                    totalTime += setupTime;

                    //Visualisatie van de at random ingevulde sudoku en de statische sudoku waarin alleen de beginwaarden staan, samen met de nullen
                    printSudoku(sudoku);
                    Console.WriteLine();

                    Console.WriteLine($"Evalutation: {evaluation_value}, Time to setup: {setupTime}ms, Total time: {totalTime}ms");


                    //setup timer
                    double switchTime = 0;
                    var switchTimer = System.Diagnostics.Stopwatch.StartNew();

                    //start switching
                    switches = 0;

                    //Zolang de sudoku nog niet opgelost is worden er cijfers binnen vakken geswitcht en wordt er bij een plateau of lokaal maximum een random walk geïnitieerd
                    while (evaluation_value != 0)
                    {
                        //Deze variabelen houden respectievelijk bij in welke blokken er nog switches zijn die de evaluatiewaarde verkleinen en hoeveel blokken dit zijn
                        block_bool = new int[3, 3];
                        block_bool_full = 0;

                        //Zolang er nog blokken met switches zijn die de evaluatiewaarde verkleinen worden deze switches uitgevoerd
                        while (block_bool_full != 9)
                        {

                            //De volgende code zorgt ervoor dat er een random blok wordt gekozen om in te switchen uit de blokken waarvan we niet al gezien hebben
                            // dat er geen nuttige switches meer over zijn
                            List<(int, int)> random_number = new List<(int, int)> { (0, 0), (0, 3), (0, 6), (3, 0), (3, 3), (3, 6), (6, 0), (6, 3), (6, 6) };
                            counter = 8;
                            for (int x = 2; x > -1; x--)
                                for (int y = 2; y > -1; y--)
                                {
                                    if (block_bool[x, y] == 1)
                                    {
                                        random_number.RemoveAt(counter);
                                    }
                                    counter--;
                                }

                            Random random = new Random();
                            int random_block = random.Next(0, random_number.Count);
                            int random_x_block = random_number[random_block].Item1;
                            int random_y_block = random_number[random_block].Item2;

                            //De switch functie wordt telkens at random aangeroepen om in een van deze blokken de beste switch uit te voeren
                            switch_function(random_x_block, random_y_block);
                        }

                        //Als er geen nuttige switches meer zijn worden er random switches uitgevoerd (op het moment 3)
                        if (evaluation_value != 0)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                random_walk();
                            }
                        }
                    }

                    switchTimer.Stop();
                    switchTime += Math.Round(switchTimer.Elapsed.TotalMilliseconds);
                    totalTime += switchTime;

                    //Visualisatie
                    printSudoku(sudoku);
                    Console.WriteLine();
                    Console.WriteLine($"evaluation number: {evaluation_value}, Time to switch: {switchTime}ms, Total time: {totalTime}ms");
                    Console.WriteLine(switches);
                }
            }
        }

        void random_start()
        {
            List<int> availableNumbers;

            //initialize the sudoku board

            //fill every x and y which are the spaces (0) with a random value
            //and evaluate if the blocks are valid

            //we do this by looping through all blocks in the sudoku
            //3 horizonal and 3 vertical blocks, each containing 3 x and 3 y values
            for (int block_y = 0; block_y < 3; block_y++)
            {
                for (int block_x = 0; block_x < 3; block_x++)
                {
                    fillBlock(block_x, block_y);
                }
            }


            void fillBlock(int block_x, int block_y)
            {
                //set the contains list to track which numbers are already present in the block
                availableNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                //within a block go through the xs and ys of that block
                //for this we need another 2 loops of each 3 iterations to account for the 3 xs and ys of the block

                //first we will loop through the loop to note the already present values:
                for (int relative_y = 0; relative_y < 3; relative_y++)
                {
                    for (int relative_x = 0; relative_x < 3; relative_x++)
                    {
                        int absolute_x = block_x * 3 + relative_x;
                        int absolute_y = block_y * 3 + relative_y;
                        int cordValue = static_sudoku[absolute_x, absolute_y];

                        if (cordValue != 0)
                        {
                            //there is a number predefined in the input
                            //remove this value from the available numbers
                            availableNumbers.Remove(cordValue);
                        }
                    }
                }

                for (int relative_y = 0; relative_y < 3; relative_y++)
                {
                    for (int relative_x = 0; relative_x < 3; relative_x++)
                    {
                        int absolute_x = block_x * 3 + relative_x;
                        int absolute_y = block_y * 3 + relative_y;

                        //first remove the already filled in values from the availableNumbers list
                        int cordValue = static_sudoku[absolute_x, absolute_y];

                        if (cordValue == 0 && availableNumbers.Count > 0)
                        {
                            //the number is not predefined and there are still numbers available

                            //generate a new random value that is still available
                            Random random = new Random();
                            int newValueIndex = random.Next(0, availableNumbers.Count);
                            int newValue = availableNumbers[newValueIndex];

                            //set this new value to the new sudoku board
                            sudoku[absolute_x, absolute_y] = newValue;

                            //update the available nubmers
                            availableNumbers.Remove(newValue);
                        }
                    }
                }
            }
        }

        //De initiële evaluatiewaarde wordt berekend door voor elke rij en elke kolom (9-c) op te tellen, waarbij c het aantal getallen is wat tenminste 
        // één keer voorkomt
        void initial_evaluation()
        {
            HashSet<int> contains = new HashSet<int>();
            int current_number = 0;

            evaluation_value = 0;
            for (int y = 0; y < 9; y++)
            {
                contains.Clear();
                for (int x = 0; x < 9; x++)
                {
                    current_number = sudoku[x, y];
                    contains.Add(current_number);
                }
                evaluation_value += 9 - contains.Count;
            }
            for (int x = 0; x < 9; x++)
            {
                contains.Clear();
                for (int y = 0; y < 9; y++)
                {
                    current_number = sudoku[x, y];
                    contains.Add(current_number);
                }
                evaluation_value += 9 - contains.Count;
            }
        }

        void switch_function(int a, int b)
        {
            //Parameters a en b worden meegegeven en zijn de random blokken waarin geswitcht gaat worden    
            int random_x_block = a;
            int random_y_block = b;
            int best_value, best_number1_x, best_number1_y, best_number2_x, best_number2_y;
            best_value = 0;
            best_number1_x = -1; best_number1_y = -1; best_number2_x = -1; best_number2_y = -1;
            //Deze variabelen moeten een waarde krijgen ondanks dat ze niet gebruikt worden als er geen nuttige switch in het blok gevonden wordt

            //Elke switch wordt geëvalueerd door de switch uit te voeren en te kijken naar het effect op de evaluatiewaarde
            for (int y1 = 0; y1 < 3; y1++)
            {
                for (int x1 = 0; x1 < 3; x1++)
                {
                    for (int y2 = y1; y2 < 3; y2++)
                    {
                        for (int x2 = 0; x2 < 3; x2++)
                        {
                            //Deze constraints zorgen ervoor dat alleen switches die mogen en nog niet in deze loop zijn getest worden getest
                            if (static_sudoku[random_x_block + x1, random_y_block + y1] == 0 && static_sudoku[random_x_block + x2, random_y_block + y2] == 0 &&
                                ((y1 == y2 && x1 < x2) || (y1 < y2)))
                            {
                                int evaluation_before_switch = evaluation(random_x_block + x1, random_y_block + y1, random_x_block + x2, random_y_block + y2);

                                int switching_num1 = sudoku[random_x_block + x1, random_y_block + y1];
                                int switching_num2 = sudoku[random_x_block + x2, random_y_block + y2];
                                sudoku[random_x_block + x1, random_y_block + y1] = switching_num2;
                                sudoku[random_x_block + x2, random_y_block + y2] = switching_num1;

                                int evaluation_after_switch = evaluation(random_x_block + x1, random_y_block + y1, random_x_block + x2, random_y_block + y2);
                                int test_value = evaluation_before_switch - evaluation_after_switch;

                                //Als een switch positief en beter dan de tot dan toe beste blijkt te zijn wordt dit geupdate
                                if (test_value > best_value)
                                {
                                    best_value = test_value;
                                    best_number1_x = x1;
                                    best_number1_y = y1;
                                    best_number2_x = x2;
                                    best_number2_y = y2;
                                }

                                //Hierna wordt er weer teruggeswitcht om door te gaan met testen
                                sudoku[random_x_block + x1, random_y_block + y1] = switching_num1;
                                sudoku[random_x_block + x2, random_y_block + y2] = switching_num2;
                            }
                        }
                    }
                }
            }
            //Als er minstens een positieve switch is wordt de beste uitgevoerd
            if (best_number1_x != -1)
            {
                evaluation_value -= evaluation(random_x_block + best_number1_x, random_y_block + best_number1_y, random_x_block + best_number2_x, random_y_block + best_number2_y);
                int switching_num1 = sudoku[best_number1_x + random_x_block, best_number1_y + random_y_block];
                int switching_num2 = sudoku[best_number2_x + random_x_block, best_number2_y + random_y_block];
                sudoku[best_number1_x + random_x_block, best_number1_y + random_y_block] = switching_num2;
                sudoku[best_number2_x + random_x_block, best_number2_y + random_y_block] = switching_num1;
                block_bool = new int[3, 3];     //Deze wordt gereset omdat er na een switch weer nieuwe mogelijke positieve switches kunnen ontstaan
                block_bool_full = 0;

                //De evaluatiewaarde wordt geupdate
                evaluation_value += evaluation(random_x_block + best_number1_x, random_y_block + best_number1_y, random_x_block + best_number2_x, random_y_block + best_number2_y);
                switches += 1;
            }

            //Als er geen positieve switch gevonden is wordt dit genoteerd
            if (best_number1_x == -1)
            {
                block_bool[a / 3, b / 3] = 1;
                block_bool_full++;
            }
        }

        //De random walk kiest twee random cijfers binnen een random blok, switcht deze als ze geswitcht mogen worden en update de evaluatiewaarde
        void random_walk()
        {
            List<int> random_number = new List<int> { 0, 3, 6 };
            Random random = new Random();
            int random_x_block = random_number[random.Next(0, 3)];
            int random_y_block = random_number[random.Next(0, 3)];
            int random_x1 = random.Next(0, 3);
            int random_x2 = random.Next(0, 3);
            int random_y1 = random.Next(0, 3);
            int random_y2 = random.Next(0, 3);

            if (static_sudoku[random_x_block + random_x1, random_y_block + random_y1] == 0 && static_sudoku[random_x_block + random_x2, random_y_block + random_y2] == 0)
            {
                int evaluation_before_switch = evaluation(random_x_block + random_x1, random_y_block + random_y1, random_x_block + random_x2, random_y_block + random_y2);

                int switching_num1 = sudoku[random_x_block + random_x1, random_y_block + random_y1];
                int switching_num2 = sudoku[random_x_block + random_x2, random_y_block + random_y2];
                sudoku[random_x_block + random_x1, random_y_block + random_y1] = switching_num2;
                sudoku[random_x_block + random_x2, random_y_block + random_y2] = switching_num1;

                int evaluation_after_switch = evaluation(random_x_block + random_x1, random_y_block + random_y1, random_x_block + random_x2, random_y_block + random_y2);

                evaluation_value -= evaluation_before_switch;
                evaluation_value += evaluation_after_switch;
                switches += 1;
            }
        }

        //Deze functie berekent de het aandeel van de twee rijen en kolommen van twee cijfers in de evaluatiewaarde. Dit wordt gebruikt om de switch te evalueren
        int evaluation(int x1, int y1, int x2, int y2)
        {
            int local_evaluation = 0;
            HashSet<int> contains1 = new HashSet<int>();
            HashSet<int> contains2 = new HashSet<int>();

            int current_number1 = 0;
            int current_number2 = 0;

            for (int x = 0; x < 9; x++)
            {
                current_number1 = sudoku[x, y1];
                current_number2 = sudoku[x, y2];
                contains1.Add(current_number1);
                contains2.Add(current_number2);
            }

            local_evaluation += 9 + 9 - contains1.Count - contains2.Count;       //Voor de switch moet de waarde van de voorgaande rijen worden afgetrokken van de evaluatiewaarde, en hebben we het globale evaluatienummer nodig

            contains1 = new HashSet<int>();
            contains2 = new HashSet<int>();

            for (int y = 0; y < 9; y++)
            {
                current_number1 = sudoku[x1, y];
                current_number2 = sudoku[x2, y];
                contains1.Add(current_number1);
                contains2.Add(current_number2);
            }

            local_evaluation += 9 + 9 - contains1.Count - contains2.Count;       //Voor de switch moet de waarde van de voorgaande kolommen worden afgetrokken van de evaluatiewaarde, en hebben we het globale evaluatienummer nodig
            return local_evaluation;
        }

        //Print de sudoku in een simpele vorm
        void printSudoku(int[,] sudoku)
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    Console.Write(sudoku[x, y] + " ");
                }
                Console.WriteLine();
            }
        }

    }
}

