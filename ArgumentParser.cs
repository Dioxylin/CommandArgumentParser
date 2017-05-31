using System.Collections.Generic;
using System.Text;

namespace CommandArgumentParser {
    /// Option class.
    ///
    /// Takes a short name and/or a long name.  Coupled with ArgumentParser.
    class Option {
        public char? ShortName { get; }
        public string LongName { get; }
        public BoolRef Flag { get; set; }
        public StringBuilder Value { get; set; }
        public bool Set { get; set; }

        public Option(char? shortName, string longName) {
            ShortName = shortName;
            LongName = longName;
            Flag = null;
            Value = null;
            Set = true;
        }
    }

    /// A wrapper around the bool value type.
    ///
    /// This allows implicit conversion to bool, but only explicit conversion from bool so that bool doesn't accidentally get
    /// passed with subtle bugs happening.
    public class BoolRef {
        public bool Value { get; set; }

        public BoolRef(bool value) {
            Value = value;
        }

        public static implicit operator bool(BoolRef boolref) {
            return boolref.Value;
        }

        // Explicitly require conversion from bool to prevent programmer gotcha.
        public static explicit operator BoolRef(bool value) {
            return new BoolRef(value);
        }
    }

    /// Command argument parser.
    ///
    /// For code example, see CommandArgumentParserTest.
    ///
    /// @throws Nothing - No exceptions are thrown.  Error handling is done by checking the Errors property.
    public class ArgumentParser {
        /// Errors.  This is the only way errors are communicated.
        public List<string> Errors { get; }

        private List<Option> Options;
        private IList<string> Arguments;

        public ArgumentParser() {
            Options = new List<Option>();
            Errors = new List<string>();
            Arguments = null;
        }

        /// Add an option that will set variable to true.
        //
        /// If variable is null, add an error into Errors and continue on.
        public void AddOnOption(char? shortName, string longName, BoolRef variable) {
            if (variable == null) {
                Errors.Add("Variable is null for boolref " + shortName + "/" + longName + ".");
                return;
            }

            var option = new Option(shortName, longName);
            option.Flag = variable;
            Options.Add(option);
        }

        /// Add an option that will set variable to false.
        ///
        /// If variable is null, add an error into Errors and continue on.
        public void AddOffOption(char? shortName, string longName, BoolRef variable) {
            if (variable == null) {
                Errors.Add("Variable is null for " + shortName + "/" + longName);
                return;
            }

            var option = new Option(shortName, longName);
            option.Flag = variable;
            option.Set = false;
            Options.Add(option);
        }
        
        /// Add an option that takes a value.
        ///
        /// We use StringBuilder because it is mutable.  Unfortunately normal string cannot have a pointer to it.
        ///
        /// If variable is null, add an error into Errors and continue on.
        public void AddValueOption(char? shortName, string longName, StringBuilder variable) {
            if (variable == null) {
                Errors.Add("Variable is null for " + shortName + "/" + longName);
                return;
            }

            var option = new Option(shortName, longName);
            option.Value = variable;
            Options.Add(option);
        }

        /// Give an arguments list to add arguments to.
        /// 
        /// If this is not specified, any arguments added will be errors.
        public void AddArgumentList(IList<string> arguments) {
            Arguments = arguments;
        }

        /// Add an argument, adding an error if argument list is not given.
        private void AddArgument(string argument) {
            if (Arguments == null) {
                Errors.Add("Adding argument " + argument + ": No argument list to add to.");
                return;
            }
            Arguments.Add(argument);
        }

        /// Expand short options groups, like -acdtrux to -a -c -d -t -r -u -x.
        private string[] ExpandShortGroups(string[] args) {
            var retval = new List<string>();
            for (int i = 0; i < args.Length; ++i) {
                if (args[i].Length > 2 && args[i][0] == '-' && args[i][1] != '-') {
                    for (int j = 1; j < args[i].Length; ++j) {
                        retval.Add("-" + args[i][j]);
                    }
                    continue;
                }
                retval.Add(args[i]);
            }
            return retval.ToArray();
        }

        // Parse an array of args according to options added.
        //
        // Just give it an args array and then you can use your variables that were pointed from it.
        public void Parse(string[] args) {
            args = ExpandShortGroups(args);
            bool parse = true;
            for (int i = 0; i < args.Length; ++i) {
                if (parse) {
                    // Turn off parsing after hitting --.  Just add arguments.
                    if (args[i] == "--") {
                        parse = false;
                        continue;
                    }
                    // Usually - for stdin/stdout.
                    else if (args[i] == "-") {
                        AddArgument(args[i]);
                        continue;
                    }
                    // Add argument; not an option argument.
                    else if (!args[i].StartsWith("-")) {
                        AddArgument(args[i]);
                        continue;
                    }

                    bool recognized = false;
                    for (int j = 0; j < Options.Count; ++j) {
                        Option opt = Options[j];
                        if (args[i] == "-" + opt.ShortName.ToString() || args[i] == "--" + opt.LongName) {
                            recognized = true;
                            // We're a flag option.
                            if (opt.Flag != null) {
                                opt.Flag.Value = opt.Set;
                            }
                            // We're a value option.
                            else {
                                if (i+1 >= args.Length) {
                                    Errors.Add(args[i] + " requires an argument, but there are no more arguments to parse.");
                                    opt.Value.Clear();
                                    return;
                                }
                                ++i;
                                opt.Value.Clear();
                                opt.Value.Append(args[i]);
                            }
                        }
                    }
                    if (!recognized) {
                        Errors.Add("Unknown argument: " + args[i]);
                    }
                }
                else {
                    AddArgument(args[i]);
                }
            }
        }
    }
}
