using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandArgumentParser {
    // Option class.
    //
    // Takes a short name and/or a long name.  Most of the logic of it is used in ArgumentParser.  It's pretty much a value class.
    //
    // To note, we do take a pointer to bool as the flag so that ArgumentParser can set it.
    class Option {
        public char? ShortName { get; }
        public string LongName { get; }
        unsafe public bool* Flag { get; set; }
        public StringBuilder Value { get; set; }
        public bool Set { get; set; }

        unsafe public Option(char? shortName, string longName) {
            ShortName = shortName;
            LongName = longName;
            Flag = null;
            Value = null;
            Set = true;
        }
    }

    // Command argument parser.
    //
    // This class is considered unsafe by C#, but because C# doesn't allow pointers to value types without unsafe, it was convenient
    // to justify it.
    public class ArgumentParser {
        private List<Option> Options;
        public List<string> Errors { get; }
        public List<string> Arguments { get; }

        public ArgumentParser() {
            Options = new List<Option>();
            Errors = new List<string>();
            Arguments = new List<string>();
        }

        // Add an option that will set variable to true.
        //
        // If variable is null, add an error into Errors and continue on.
        unsafe public void AddOnOption(char? shortName, string longName, bool* variable) {
            if (variable == null) {
                Errors.Add("Variable is null for " + shortName + "/" + longName);
                return;
            }

            var option = new Option(shortName, longName);
            option.Flag = variable;
            Options.Add(option);
        }

        // Add an option that will set variable to false.
        //
        // If variable is null, add an error into Errors and continue on.
        unsafe public void AddOffOption(char? shortName, string longName, bool* variable) {
            if (variable == null) {
                Errors.Add("Variable is null for " + shortName + "/" + longName);
                return;
            }

            var option = new Option(shortName, longName);
            option.Flag = variable;
            option.Set = false;
            Options.Add(option);
        }
        
        // Add an option that takes a value.
        //
        // We use StringBuilder because it is mutable.  Unfortunately normal string cannot have a pointer to it.
        //
        // If variable is null, add an error into Errors and continue on.
        unsafe public void AddValueOption(char? shortName, string longName, StringBuilder variable) {
            if (variable == null) {
                Errors.Add("Variable is null for " + shortName + "/" + longName);
                return;
            }

            var option = new Option(shortName, longName);
            option.Value = variable;
            Options.Add(option);
        }

        // Parse an array of args according to options added.
        //
        // Just give it an args array and then you can use your variables that were pointed from it.
        unsafe public void Parse(string[] args) {
            bool parse = true;
            for (int i = 0; i < args.Length; ++i) {
                string arg = args[i];
                if (parse) {
                    if (arg == "--") {
                        parse = false;
                        continue;
                    }
                    else if (!arg.StartsWith("-")) {
                        Arguments.Add(arg);
                        continue;
                    }
                    bool recognized = false;
                    for (int j = 0; j < Options.Count; ++j) {
                        Option opt = Options[j];
                        if (arg == "-" + opt.ShortName.ToString() || arg == "--" + opt.LongName) {
                            recognized = true;
                            if (opt.Flag != null) {
                                *opt.Flag = opt.Set;
                            }
                            else {
                                string prev = args[i];
                                if (i+1 >= args.Length) {
                                    Errors.Add(prev + " requires an argument, but there are no more arguments to parse.");
                                    return;
                                }
                                ++i;
                                opt.Value.Clear();
                                opt.Value.Append(args[i]);
                            }
                        }
                    }
                    if (!recognized) {
                        Errors.Add("Unknown argument: " + arg);
                    }
                }
                else {
                    Arguments.Add(args[i]);
                }
            }
        }
    }
}
