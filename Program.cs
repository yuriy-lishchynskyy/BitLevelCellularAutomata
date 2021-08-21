using System;

namespace Assignment1
{
    class Program
    {
        static void Main(string[] args)
        {
            BitCA ca1 = new BitCA(); // initialise instance of BitCA class

            ca1.SetOptions();
            ca1.SetArray();
            ca1.PrintRule();
            ca1.RunCA();

            Console.ReadLine();
        }
    }

    class BitCA
    {
        // data
        private uint current = 1; // current value of array
        private byte rule = 30; // byte representing rule
        private int steps = 0; // no. of steps to run
        private string ini_type = "0"; // initialisation type

        private string[] rule_in_array = { "000", "001", "010", "011", "100", "101", "110", "111" }; // triple lookup array
        private string[] rule_list = { "(0,0,0)", "(0,0,1)", "(0,1,0)", "(0,1,1)", "(1,0,0)", "(1,0,1)", "(1,1,0)", "(1,1,1)" }; // triple combinations to be printed on screen with rule
        private string[] rule_out_array = new string[8]; // rule bits corresponding to triples

        // constructors
        public BitCA()
        {
            // default constructor - do nothing
        }

        // methods
        public void SetArray() // set an array as either a random sequence of bits or single bit switched on
        {
            Console.WriteLine("Please enter the type of initialisation: 0 for random, 1 for single non-zero entry in the middle");
            this.ini_type = Console.ReadLine();

            while (this.ini_type != "0" && this.ini_type != "1") // need to input valid value for program to continue
            {
                Console.WriteLine("Invalid Input for the type of initialisation - please re-enter");
                this.ini_type = Console.ReadLine();
            }

            if (this.ini_type == "0") // random 1's and 0's
            {
                Random a = new Random();
                this.current = (uint)a.Next(0, 2147483647); // results in 32-bit long ranbom sequence of 1's and 0's
            }
            else // single non-zero in middle
            {
                this.current = 1;
                this.current = this.current << 16;
            }
        }

        public void SetOptions()
        {
            Console.WriteLine("Please enter the rule: Any integer between 0 and 255");
            string input_rule = Console.ReadLine();

            bool success = byte.TryParse(input_rule, out this.rule);

            while (!success) // need to input valid value for program to continue
            {
                Console.WriteLine("Invalid Input for the rule - please re-enter");
                input_rule = Console.ReadLine();
                success = byte.TryParse(input_rule, out this.rule);
            }

            CreateRule(); // call method to create rule array

            Console.WriteLine("Please enter the number of steps: Any integer between 0 and 200");
            string input_steps = Console.ReadLine();

            success = Int32.TryParse(input_steps, out this.steps);

            while (!success || this.steps < 0 || this.steps > 200) // need to input valid value for program to continue
            {
                Console.WriteLine("Invalid Input for the number of steps - please re-enter");
                input_steps = Console.ReadLine();
                success = Int32.TryParse(input_steps, out this.steps);
            }
        }

        public void SetArray(uint value) // set an array as specific value
        {
            this.current = value;
        }

        public void RunCA() // Run Cellular Automata process
        {
            Console.WriteLine("The initialisation is:");
            OutputCurrentStep(this.current);

            Console.WriteLine("The Cellular Automata is:");
            OutputNumberOfSteps(this.current, this.steps);
        }

        public void PrintArray() // Print bitwise representation of array
        {
            OutputCurrentStep(this.current);
        }

        public void PrintArray(uint value) // Print bitwise representation of any number
        {
            OutputCurrentStep(value);
        }

        public void PrintRule() // Print Rule
        {
            Console.WriteLine("You have entered rule:");

            for (int i = 0; i < 8; i++)
            {
                Console.WriteLine("{0} -> {1}", this.rule_list[i], this.rule_out_array[i]);
            }
        }

        private void CreateRule() // Create Rule "lookup" array
        {
            uint rule_mask = 1, res, rule_test = this.rule;

            for (int i = 0; i < 8; i++)
            {
                res = rule_test & rule_mask; // bitwise comparison of current rule integer and mask (00000001)

                if (res == 0)
                {
                    this.rule_out_array[i] = "0";
                }
                else
                {
                    this.rule_out_array[i] = "1";
                }

                rule_mask = (byte)(rule_mask << 1); // shift mask left by 1
            }
        }

        private void OutputCurrentStep(uint value) // print array
        {
            int i;

            uint mask = 1, res = 0; // create mask = 1 = ......00000001
            mask = mask << 31; // shift the 1 left 31 times = 10000000......

            for (i = 0; i < 32; i++)
            {
                res = value & mask; // bitwise comparison of current array and mask
                if (res == 0) // if result = 0, then i-th element of "current" is a 0
                {
                    Console.Write('0');
                }
                else // if result = a number, then i-th element of "current" is a 1
                {
                    Console.Write('1');
                }

                mask = mask >> 1;
            }

            Console.WriteLine();
        }

        private void OutputNumberOfSteps(uint value, int steps) // Output x number of steps of CA
        {
            int j;

            OutputCurrentStep(value);

            for (j = 0; j < steps; j++)
            {
                value = GetNextArray(value);
                OutputCurrentStep(value);
            }
        }

        private uint CircularLeftShift(uint value) // helper function - takes in array, shift all bits left by 1, shift far-left bit to far right
        {
            uint mask = 1;
            mask = mask << 31;
            uint output;

            bool firstbit;

            output = mask & value;

            if (output == 0)
            {
                firstbit = false; // left-most bit = 0
            }
            else
            {
                firstbit = true; // left-most bit = 1
            }

            output = value << 1; // carry out shift by shifting all bits in "value" left 1, first (far right) bit = 0 by default from circular shift

            // need to use XOR mask, 10000000 = set bit on, 000000001 = set bit on
            mask = 1;

            if (firstbit == true) // only need to run if bit circling around is not = false = 0 (default)
            {
                output = output ^ mask; // perform XOR on current array with XOR (same values = 0, opposite values = 1)
            }

            return output;
        }

        private uint GetNextArray(uint value) // determines bit arrangement in next step of CA
        {
            int i, j, k;
            uint mask = 1;
            uint next = 0;

            for (i = 0; i < 31; i++) // shift entire array right by 1 (to get first "circular" triple) using 31 shifts left
            {
                value = CircularLeftShift(value);
            }

            for (i = 0; i < 32; i++) // for each of 32 bits in new array, look at 3 adjacent bits in current array
            {
                mask = 1;
                mask = mask << 31; // triple checking mask acts on 0th, 1st and 2nd elements of "circular-shifted" array
                uint output;
                string triple = ""; // store value of adjacent triple

                for (j = 0; j < 3; j++) // look at 3 adjacent bits
                {
                    output = mask & value;

                    if (output == 0) // if result = 0, bit = 0
                    {
                        triple = triple + "0";
                    }
                    else // if result = 1, bit = 1
                    {
                        triple = triple + "1";
                    }

                    mask = mask >> 1;
                }

                mask = 1;
                mask = mask << 31 - i; // assignment mask acts on (32 - i)'th element

                for (k = 0; k < 8; k++) // check all 8 rules
                {
                    if (triple == this.rule_in_array[k] && this.rule_out_array[k] == "0") // if triple order matches a rule, and that rule = 0, set current bit to 0
                    {
                        mask = ~(mask);
                        next = next & mask;

                    }
                    else if (triple == this.rule_in_array[k] && this.rule_out_array[k] == "1") // if triple order matches a rule, and that rule = 1, set current bit to 1
                    {
                        next = next | mask;
                    }
                }

                value = CircularLeftShift(value); // shift all bits left by 1 for new triplet
            }

            return next;
        }
    }
}
