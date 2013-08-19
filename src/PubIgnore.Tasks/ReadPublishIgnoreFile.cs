namespace PubIgnore.Tasks {
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This task will read the .publishIgnore file and return the results
    /// </summary>
    public class ReadPublishIgnoreFile : Task {
        [Required]
        public string FilePath { get; set; }

        /// <summary>
        /// Contains the files which were read from the file, excluding
        /// any comments in the file
        /// </summary>
        [Output]
        public ITaskItem[] LinesFromFile { get; set; }        

        public override bool Execute() {
            // read the file line by line and exclude any lines which start with # or 
            //  just contain whitespace
            Log.LogMessage("Starting to read .publishignore file at [{0}]", this.FilePath);

            if (!File.Exists(FilePath)) {
                string msg = string.Format("Unable to find the .publishIgnore file at [{0}]",this.FilePath);
                Log.LogError(msg);
                return false;
            }

            // TODO: do this better
            string[] allLinesRaw = File.ReadAllLines(this.FilePath);

            List<ITaskItem> linesNotComments = new List<ITaskItem>();
            foreach(string line in allLinesRaw){
                if (string.IsNullOrEmpty(line))
                    continue;

                // trim the line and see if it starts with #
                string lineTrimmed = line.TrimStart();
                if (lineTrimmed.StartsWith("#"))
                    continue;

                // add it to the list to be returned
                linesNotComments.Add(new TaskItem(lineTrimmed));
            }

            this.LinesFromFile = linesNotComments.ToArray();

            Log.LogMessage("Finished reading .publishIgnore file at [{0}]. Found [{0}] lines which are not comments or blank.", this.FilePath, this.LinesFromFile.Length);

            return !Log.HasLoggedErrors;
        }
    }
}
