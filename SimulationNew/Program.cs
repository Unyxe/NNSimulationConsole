using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimulationNew
{
    internal class MainSimulation
    {
        static Random rn = new Random();
        public static int width = 100;
        public static int height = 60;
        public static List<Cell> cells = new List<Cell>();
        public static List<Cell> cells_to_remove = new List<Cell>();
        public static List<Cell> cells_to_add = new List<Cell>();
        static void Main(string[] args)
        {
            /*
            NeuralNetwork test_nn = new NeuralNetwork(1,5, 4, 3, rn.Next(), false);
            for(int i = 0; i < 500; i++)
            {
                double[] output = test_nn.CalculateOutput(new double[] { rn.NextDouble(), rn.NextDouble(), rn.NextDouble(), rn.NextDouble() });
                Console.WriteLine(output[0] + "    " + output[1] + "    " + output[2]);
            }
            */
            
            Console.ReadLine();
            for(int i = 0; i < width*height/3; i++)
            {
                int x_s = rn.Next() % width;
                int y_s = rn.Next() % height;
                if(GetCellFromCoord(x_s, y_s) != null)
                {
                    continue;
                }
                cells.Add(new Cell(x_s, y_s, rn.Next()));
            }
            while (true)
            {
                cells_to_remove.Clear();
                cells_to_add.Clear();
                foreach(Cell c in cells)
                {
                    c.NextMove();
                }
                foreach(Cell c in cells_to_remove)
                {
                    cells.Remove(c);
                }
                foreach (Cell c in cells_to_add)
                {
                    cells.Add(c);
                }
                DisplayCells();
                Console.ReadLine();
                Console.Clear();
            }
            
            Console.ReadLine();
        }

        public static void DisplayCells()
        {
            
            for(int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Console.Write(" |");
                }
                Console.WriteLine();
            }
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            foreach(Cell c in cells)
            {
                Console.SetCursorPosition(2 * c.x, c.y);
                if(c.direction.X == 0)
                {
                    if(c.direction.Y == 1)
                    {
                        Console.Write("U");
                    } else
                    {
                        Console.Write("D");
                    }
                } else
                {
                    if (c.direction.X == 1)
                    {
                        Console.Write("R");
                    }
                    else
                    {
                        Console.Write("L");
                    }
                }
            }
            Console.SetCursorPosition(x, y + 1);
        }


        public static Cell GetCellFromCoord(int x_w, int y_w)
        {
            foreach (Cell cell in cells)
            {
                if (cell.x == x_w && cell.y == y_w)
                {
                    return cell;
                }
            }
            return null;
        }
    }


    public class Cell
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public Vector2 direction = new Vector2(0, 1);
        public NeuralNetwork nn;
        public int energy = 30;
        Random rn;
        public Cell(int x, int y, int seed)
        {
            this.x = x;
            this.y = y;
            width = MainSimulation.width;
            height = MainSimulation.height;
            rn = new Random(seed);
            nn = new NeuralNetwork(5, 5, 4, 5, seed, false);
        }
        public Cell(int x, int y, int seed, NeuralNetwork nn_)
        {
            this.x = x;
            this.y = y;
            width = MainSimulation.width;
            height = MainSimulation.height;
            rn = new Random(seed);
            nn = new NeuralNetwork(5, 5, 4, 5, seed, false);
            nn.SetBiases(nn_.ExportBiases());
            nn.SetWeights(nn_.ExportWeights());
            if(rn.Next()%1000 < 250)
            {
                nn.Mutate(1);
            }
        }

        public void NextMove()
        {
            energy--;
            double[] input = new double[4] {direction.X, direction.Y, (y*1.0)/height, energy/100.0};
            double[] output = nn.CalculateOutput(input);
            //Console.WriteLine(input[0] + "\t" + input[1] + "\t" + input[2] + "\t" + input[3]);
            //Console.WriteLine(output[0] + "\t" + output[1] + "\t" + output[2]);
            int choice = 0;
            double max = -1;
            for(int i = 0; i < output.Length; i++)
            {
                if (output[i] > max)
                {
                    max = output[i];
                    choice = i;
                }
            }

            switch (choice)
            {
                case 0:
                    Move();
                    break;
                case 1:
                    if(direction.Y == 0)
                    {
                        direction.Y -= direction.X;
                        direction.X = 0;
                    } else
                    {
                        direction.X += direction.Y;
                        direction.Y = 0;
                    }
                    Move();
                    break;
                case 2:
                    if (direction.Y == 0)
                    {
                        direction.Y += direction.X;
                        direction.X = 0;
                    }
                    else
                    {
                        direction.X -= direction.Y;
                        direction.Y = 0;
                    }
                    Move();
                    break;
                case 3:
                    Photosynthesis();
                    break;
                case 4:
                    Breed();
                    break;
            }

            if(energy < 0)
            {
                MainSimulation.cells_to_remove.Add(this);
            }
        }

        void Photosynthesis()
        {
            int add_energy = y / (height / 10);
            if(add_energy <= 0)
            {
                return;
            }
            energy += add_energy;
            if(energy > 100)
            {
                energy = 100;
            }
        }
        void Breed()
        {
            int x_s = x + (int)direction.X;
            int y_s = y + (int)direction.Y;

            if (CheckPosition(x_s, y_s, false) && energy >= 30)
            {
                energy -= 30;
                MainSimulation.cells_to_add.Add(new Cell(x_s, y_s, rn.Next(), nn));
            }
        }
        void Move()
        {
            int x_w = x + (int)direction.X;
            int y_w = y + (int)direction.Y;
            if (CheckPosition(x_w, y_w, false))
            {
                x += (int)direction.X;
                y += (int)direction.Y;
            }
        }
        bool CheckPosition(int w_x, int w_y, bool check_occup)
        {
            if ((w_x < width && w_y < height && w_x >= 0 && w_y >= 0))
            {
                bool is_occupied = false;
                Cell cell = GetCellFromCoord(w_x, w_y);
                if(cell != null)
                {
                    is_occupied = true;
                }
                if (check_occup)
                {
                    return is_occupied;
                }
                return !is_occupied;
            }
            return false;
        }
        public Cell GetCellFromCoord(int x_w, int y_w)
        {
            foreach (Cell cell in MainSimulation.cells)
            {
                if (cell.x == x_w && cell.y == y_w)
                {
                    return cell;
                }
            }
            return null;
        }
    }
    public class Neuron
    {
        public string id = "Neuron";
        Random rand;
        public double[] weights;
        public double bias;
        public Neuron(int num_of_weights, int seed)
        {
            rand = new Random(seed);
            weights = new double[num_of_weights];
            for (int i = 0; i < num_of_weights; i++)
            {
                weights[i] = rand.NextDouble();
            }

            bias = rand.NextDouble();
        }
        public Neuron(double[] weights_, double bias_)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = weights_[i];
            }

            bias = bias_;
        }
        public void print_weights()
        {
            foreach (double w in weights)
            {
                Console.WriteLine(w);
            }
        }
        public double calculate_output_from_input(double[] input, bool is_output)
        {
            double output_sum = 0;
            for (int i = 0; i < input.Length; i++)
            { 
                output_sum += input[i] * weights[i];
            }

            output_sum += bias;
            //Console.WriteLine(output_sum);
            if (is_output)
            {
                //Console.WriteLine("hi");
                return output_sum / input.Length;
            }
            if(output_sum / input.Length>= 0.5)
            {
                return 1;
            }
            return 0;
            
        }
        public void Mutate()
        {
            
            if (rand.NextDouble() < 0.1)
            {
                bias = rand.NextDouble();
            } else
            {
                weights[rand.Next() % weights.Length] = rand.NextDouble();
            }
        }
        public double[] CloneWeights()
        {
            double[] m_weights = new double[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                m_weights[i] = weights[i];
            }
            return m_weights;
        }

        public double CloneBias()
        {
            return bias;
        }
        public void SetWeights(double[] weig)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = weig[i];
            }
        }

        public void SetBias(double bias_)
        {
            bias = bias_;
        }

        public void SetSeed(int seed_)
        {
            rand = new Random(seed_);
        }
    }
    public class NeuralNetwork
    {
        Random rand;
        public List<List<Neuron>> NN = new List<List<Neuron>>();
        public int num_outputs;
        bool debug_on = false;
        public string pattern;

        public NeuralNetwork(int hidden_layers, int neurons_per_hidden_layer, int num_of_inputs, int num_of_outputs, int seed, bool debug_info)
        {
            rand = new Random(seed);
            num_outputs = num_of_outputs;
            debug_on = debug_info;
            if (debug_on)
            {
                Console.WriteLine("Initializating weights");
            }
            for (int i = 0; i < hidden_layers; i++)
            {
                List<Neuron> layer = new List<Neuron>();
                for (int j = 0; j < neurons_per_hidden_layer; j++)
                {

                    if (i == 0)
                    {
                        layer.Add(new Neuron(num_of_inputs, rand.Next()));
                        if (debug_on)
                        {
                            Console.WriteLine(i + " " + j + " " + layer[j].weights.Length + layer[j].id);
                            layer[j].print_weights();
                            Console.WriteLine();
                        }

                        continue;
                    }
                    layer.Add(new Neuron(neurons_per_hidden_layer, rand.Next()));
                    if (debug_on)
                    {
                        Console.WriteLine(i + " " + j + " " + layer[j].weights.Length + layer[j].id);
                        layer[j].print_weights();
                        Console.WriteLine();
                    }

                }
                NN.Add(layer);
            }
            List<Neuron> output_layer = new List<Neuron>();
            for (int i = 0; i < num_of_outputs; i++)
            {
                output_layer.Add(new Neuron(neurons_per_hidden_layer, rand.Next()));
                if (debug_on)
                {
                    Console.WriteLine("9" + " " + i + " " + output_layer[i].weights.Length + output_layer[i].id);
                    output_layer[i].print_weights();
                    Console.WriteLine();
                }
            }
            NN.Add(output_layer);
        }

        public void SetSeed(int seed_)
        {
            rand = new Random(seed_);
            foreach (List<Neuron> layer in NN)
            {
                foreach (Neuron n in layer)
                {
                    n.SetSeed(seed_);
                }
            }
        }

        public double[] CalculateOutput(double[] input)
        {
            if (debug_on)
            {
                Console.WriteLine("Calculating output");
            }
            double[] next_input;
            double[] current_input = new double[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                current_input[i] = input[i];
            }
            for (int i = 0; i < NN.Count; i++)
            {
                next_input = new double[NN[i].Count];
                for (int j = 0; j < NN[i].Count; j++)
                {
                    if (debug_on)
                    {
                        Console.WriteLine(i + " " + j);
                    }
                    if (i == NN.Count-1)
                    {
                        next_input[j] = NN[i][j].calculate_output_from_input(current_input, true);
                        continue;
                    }
                    next_input[j] = NN[i][j].calculate_output_from_input(current_input, true);

                }
                current_input = new double[next_input.Length];
                for (int j = 0; j < next_input.Length; j++)
                {
                    current_input[j] = next_input[j];
                }
            }
            return current_input;
        }

        public void Mutate(int lvl)
        {
            for (int i = 0; i < lvl; i++)
            {
                int l = rand.Next() % NN.Count;
                int j = rand.Next() % NN[l].Count;
                NN[l][j].Mutate();
            }
        }

        public List<double[]> ExportWeights()
        {
            List<double[]> exp_weights = new List<double[]>();
            foreach (List<Neuron> layer in NN)
            {
                foreach (Neuron n in layer)
                {
                    exp_weights.Add(n.CloneWeights());
                }
            }
            return exp_weights;
        }

        public double ExportSumWeightsAndBiases()
        {
            double exp_weights = 0;
            foreach (List<Neuron> layer in NN)
            {
                foreach (Neuron n in layer)
                {
                    foreach (double weight in n.CloneWeights())
                    {
                        exp_weights += weight;
                    }

                    exp_weights += n.CloneBias();
                }
            }
            return exp_weights;
        }
        public List<double> ExportBiases()
        {
            List<double> exp_biases = new List<double>();
            foreach (List<Neuron> layer in NN)
            {
                foreach (Neuron n in layer)
                {
                    exp_biases.Add(n.CloneBias());
                }
            }
            return exp_biases;
        }

        public void SetWeights(List<double[]> weights_e)
        {
            int ind = 0;
            foreach (List<Neuron> layer in NN)
            {
                foreach (Neuron n in layer)
                {
                    n.SetWeights(weights_e[ind]);
                    ind++;
                }
            }
        }

        public void SetBiases(List<double> biases_s)
        {
            int ind = 0;
            foreach (List<Neuron> layer in NN)
            {
                foreach (Neuron n in layer)
                {
                    n.SetBias(biases_s[ind]);
                    ind++;
                }
            }
        }
        public List<double[]> MiosisWeights(List<double[]> weights_m, List<double[]> weights_n)
        {
            List<double[]> exp_weights = this.ExportWeights();

            for (int i = 0; i < exp_weights.Count; i++)
            {
                for (int j = 0; j < exp_weights[i].Length; j++)
                {
                    if (rand.Next() % 100 > 50)
                    {
                        exp_weights[i][j] = weights_m[i][j];

                    }
                    else
                    {
                        exp_weights[i][j] = weights_n[i][j];
                        pattern += "0";
                    }
                }
            }
            return exp_weights;
        }
        public List<double> MiosisBiases(List<double> biases_m, List<double> biases_n)
        {
            List<double> exp_weights = this.ExportBiases();
            pattern = "";
            for (int i = 0; i < exp_weights.Count; i++)
            {
                if (rand.Next() % 100 > 50)
                {
                    exp_weights[i] = biases_m[i];
                    pattern += "1";
                }
                else
                {
                    exp_weights[i] = biases_n[i];
                    pattern += "0";
                }
            }
            return exp_weights;
        }
    }
}
