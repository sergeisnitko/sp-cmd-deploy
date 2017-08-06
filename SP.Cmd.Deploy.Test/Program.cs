using System;

namespace SP.Cmd.Deploy.Test {
    class Program {
        static void Main(string[] args) {

            var tasks = new SPFunctions() {
                {
                    "task1", options => {
                        Console.WriteLine("Tasks 1");
                    }
                }, {
                    "task2", options => {
                        Console.WriteLine("Tasks 2");
                    }
                }
            };

            SharePoint.Exec(args, tasks);

        }
    }
}
