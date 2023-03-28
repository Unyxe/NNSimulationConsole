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
        public static int width = 32;
        public static int height = 32;
        public static List<Cell> cells = new List<Cell>();
        static void Main(string[] args)
        {
            NeuralNetwork test_nn = new NeuralNetwork(5,5, 2, 3, rn.Next(), false);
            for(int i = 0; i < 500; i++)
            {
                double[] output = test_nn.CalculateOutput(new double[] { rn.NextDouble(), rn.NextDouble() });
                Console.WriteLine(output[0] + "\t" + output[1] + "\t" + output[2]);
            }
            /*
            Console.ReadLine();
            Cell test_cell = new Cell(5, 5, rn.Next());
            while (true)
            {
                cells.Add(test_cell);
                cells[0].NextMove();
                DisplayCells();
                Console.ReadLine();
                Console.Clear();
            }
            */
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
    }


    public class Cell
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public Vector2 direction = new Vector2(0, 1);
        public NeuralNetwork nn;
        public Cell(int x, int y, int seed)
        {
            this.x = x;
            this.y = y;
            width = MainSimulation.width;
            height = MainSimulation.height;
            nn = new NeuralNetwork(5, 5, 2, 3, seed, false);
        }

        public void NextMove()
        {
            double[] input = new double[2] {x / (double)width, y/(double)height };
            double[] output = nn.CalculateOutput(input);
            Console.WriteLine(input[0] + "\t" + input[1]);
            Console.WriteLine(output[0] + "\t" + output[1] + "\t" + output[2]);
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
            }
        }

        void Move()
        {
            x += (int)direction.X;
            y += (int)direction.Y;
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
                return output_sum;
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
                    next_input[j] = NN[i][j].calculate_output_from_input(current_input, false);

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
