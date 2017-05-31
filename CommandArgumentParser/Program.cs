using System;
using System.Collections.Generic;
using System.Text;

using CommandArgumentParser;

namespace CommandArgumentParserTest {
    class CommandArgumentParserTest {
        static int Main(string[] args) {
            // We must explictly convert from bool to BoolRef.
            BoolRef aOption = (BoolRef)false;
            BoolRef bOption = (BoolRef)true;
            BoolRef dOption = (BoolRef)false;

            // We use StringBuilder to have a string that has reference semantics.
            StringBuilder cValue = new StringBuilder();
            // Arguments that the application can use itself.
            List<string> arguments = new List<string>();
            var parser = new ArgumentParser();
            parser.AddArgumentList(arguments);
            parser.AddOnOption('a', "a-option", aOption);
            parser.AddOffOption('b', "b-option", bOption);
            parser.AddValueOption('c', "c-option", cValue);
            parser.Parse(args);

            // Error handling.
            if (parser.Errors.Count > 0) {
                Console.WriteLine("ERRORS: ");
                foreach (var error in parser.Errors) {
                    Console.WriteLine("    " + error);
                }
                Console.WriteLine();
                // You would normally exit failure here, but let's keep going.
            }

            // Implicit conversion from BoolRef to bool.
            if (aOption) {
                Console.WriteLine("A switched on.");
            }
            else {
                Console.WriteLine("A off by default.");
            }

            if (bOption) {
                Console.WriteLine("B on by default.");
            }
            else {
                Console.WriteLine("B switched off.");
            }

            // Value arguments always have an argument, even if it's blank.
            if (cValue.ToString() != "") {
                Console.WriteLine("C has value \"" + cValue.ToString() + "\".");
            }
            else {
                Console.WriteLine("C has no value as it was not specified.");
            }

            Console.Write("Arguments:");
            for (int i = 0; i < arguments.Count; ++i) {
                Console.Write(" " + arguments[i]);
            }
            Console.WriteLine();

            return 0;
        }
    }
}
